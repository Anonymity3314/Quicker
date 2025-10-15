using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace Quicker.Helpers
{
    // 由应用层更新，钩子仅读取，避免跨线程访问复杂对象
    public static class HookConfig
    {
        public static volatile bool SuppressCtrlRightClick;
        public static volatile bool SuppressCtrlMiddleClick;
    }

    // 事件数据：键盘
    public sealed class KeyboardHookEventArgs : EventArgs
    {
        public KeyboardHookEventArgs(KeyCode keyCode)
        {
            Data = new KeyboardData { KeyCode = keyCode };
        }

        public KeyboardData Data { get; }

        public sealed class KeyboardData
        {
            public KeyCode KeyCode { get; init; }
        }
    }

    // 事件数据：鼠标
    public sealed class MouseHookEventArgs : EventArgs
    {
        public MouseHookEventArgs(MouseButton button, int x, int y)
        {
            Data = new MouseData { Button = button, X = x, Y = y };
        }

        public MouseData Data { get; }

        public sealed class MouseData
        {
            public MouseButton Button { get; init; }
            public int X { get; init; }
            public int Y { get; init; }
        }
    }

    // 与 SharpHook 对齐的按键与鼠标按钮命名（仅实现当前用到的部分）
    public enum KeyCode
    {
        VcLeftControl,
        VcRightControl,
    }

    public enum MouseButton
    {
        // 与现有代码使用保持一致
        Button1 = 1, // Left（未使用，仅占位）
        Button2 = 2, // Right
        Button3 = 3, // Middle
        Button4 = 4, // X1
        Button5 = 5, // X2
    }

    // 低层全局钩子实现（原 TaskPoolGlobalHook），便于后续开发使用
    public sealed class LowLevelGlobalHookHelper : IDisposable
    {
        // Win32 常量
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;

        // XBUTTON 识别
        private const int XBUTTON1 = 0x0001;
        private const int XBUTTON2 = 0x0002;

        // VK 常量（仅用到 Ctrl）
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;

        // 委托保持引用，避免被 GC
        private LowLevelKeyboardProc? _keyboardProc;
        private LowLevelMouseProc? _mouseProc;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private IntPtr _mouseHook = IntPtr.Zero;

        // 事件
        public event EventHandler<KeyboardHookEventArgs>? KeyPressed;
        public event EventHandler<KeyboardHookEventArgs>? KeyReleased;
        public event EventHandler<MouseHookEventArgs>? MousePressed;
        public event EventHandler<MouseHookEventArgs>? MouseReleased;

        // Ctrl 状态由键盘钩子维护，鼠标回调零成本读取
        private volatile bool _ctrlDown;

        // 事件泵：单消费者，避免 ThreadPool 风暴与 UI 线程争用
        private enum EventKind
        {
            KeyDownLeftCtrl,
            KeyDownRightCtrl,
            KeyUpLeftCtrl,
            KeyUpRightCtrl,
            MouseDownRight,
            MouseUpRight,
            MouseDownMiddle,
            MouseUpMiddle,
            MouseDownX1,
            MouseUpX1,
            MouseDownX2,
            MouseUpX2,
        }

        private readonly struct EventItem
        {
            public EventItem(EventKind kind, int x, int y)
            {
                Kind = kind;
                X = x;
                Y = y;
            }
            public EventItem(EventKind kind)
            {
                Kind = kind;
                X = 0;
                Y = 0;
            }
            public EventKind Kind { get; }
            public int X { get; }
            public int Y { get; }
        }

        // 有界无锁队列 + 信号，防止事件积压导致卡顿；超出容量时丢弃最新事件
        private const int QueueCapacity = 2048;
        private readonly ConcurrentQueue<EventItem> _queue = new();
        private readonly ManualResetEventSlim _signal = new(false, 64);
        private int _queued = 0;
        private Thread? _eventThread;

        // 缓存桌面窗口根句柄，避免每次查询类名
        private readonly HashSet<IntPtr> _desktopRoots = new();

        // 钩子线程入队（极轻量），满载时自动丢弃
        private void Enqueue(EventItem item)
        {
            int newCount = Interlocked.Increment(ref _queued);
            if (newCount > QueueCapacity)
            {
                Interlocked.Decrement(ref _queued);
                return;
            }
            _queue.Enqueue(item);
            _signal.Set();
        }

        // 对外 API
        public Task RunAsync()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(null), 0);
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(null), 0);
            // 启动单消费者事件线程（前置于窗口打开等耗时操作）
            _eventThread = new Thread(EventLoop) { IsBackground = true, Name = "LowLevelHook-EventLoop", Priority = ThreadPriority.Highest };
            _eventThread.Start();
            // 构建桌面窗口缓存
            TryBuildDesktopRoots();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                if (_keyboardHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHook);
                    _keyboardHook = IntPtr.Zero;
                }
            }
            catch { }
            try
            {
                if (_mouseHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_mouseHook);
                    _mouseHook = IntPtr.Zero;
                }
            }
            catch { }

            try
            {
                if (_eventThread != null && _eventThread.IsAlive)
                {
                    _signal.Set();
                    // 最多等待 200ms，避免阻塞退出
                    if (!_eventThread.Join(200))
                    {
                        try { _eventThread.Interrupt(); } catch { }
                    }
                }
            }
            catch { }
        }

        private void EventLoop()
        {
            try
            {
                const int MaxBatch = 128;
                while (true)
                {
                    _signal.Wait();
                    _signal.Reset();
                    for (int i = 0; i < MaxBatch; i++)
                    {
                        if (!_queue.TryDequeue(out var item)) break;
                        Interlocked.Decrement(ref _queued);
                        try
                        {
                            switch (item.Kind)
                            {
                                case EventKind.KeyDownLeftCtrl:
                                    KeyPressed?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcLeftControl));
                                    break;
                                case EventKind.KeyDownRightCtrl:
                                    KeyPressed?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcRightControl));
                                    break;
                                case EventKind.KeyUpLeftCtrl:
                                    KeyReleased?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcLeftControl));
                                    break;
                                case EventKind.KeyUpRightCtrl:
                                    KeyReleased?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcRightControl));
                                    break;
                                case EventKind.MouseDownRight:
                                    MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button2, item.X, item.Y));
                                    break;
                                case EventKind.MouseUpRight:
                                    MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button2, item.X, item.Y));
                                    break;
                                case EventKind.MouseDownMiddle:
                                    MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button3, item.X, item.Y));
                                    break;
                                case EventKind.MouseUpMiddle:
                                    MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button3, item.X, item.Y));
                                    break;
                                case EventKind.MouseDownX1:
                                    MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button4, item.X, item.Y));
                                    break;
                                case EventKind.MouseUpX1:
                                    MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button4, item.X, item.Y));
                                    break;
                                case EventKind.MouseDownX2:
                                    MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button5, item.X, item.Y));
                                    break;
                                case EventKind.MouseUpX2:
                                    MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button5, item.X, item.Y));
                                    break;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // 键盘回调
        private unsafe IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                KBDLLHOOKSTRUCT* p = (KBDLLHOOKSTRUCT*)lParam;
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    if (p->vkCode == VK_LCONTROL)
                    {
                        _ctrlDown = true;
                        Enqueue(new EventItem(EventKind.KeyDownLeftCtrl));
                    }
                    else if (p->vkCode == VK_RCONTROL)
                    {
                        _ctrlDown = true;
                        Enqueue(new EventItem(EventKind.KeyDownRightCtrl));
                    }
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    if (p->vkCode == VK_LCONTROL)
                    {
                        _ctrlDown = false;
                        Enqueue(new EventItem(EventKind.KeyUpLeftCtrl));
                    }
                    else if (p->vkCode == VK_RCONTROL)
                    {
                        _ctrlDown = false;
                        Enqueue(new EventItem(EventKind.KeyUpRightCtrl));
                    }
                }
            }
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        // 鼠标回调（包含必要的系统菜单抑制）
        private unsafe IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                MSLLHOOKSTRUCT* p = (MSLLHOOKSTRUCT*)lParam;

                // 坐标
                int x = p->pt.x;
                int y = p->pt.y;

                // 先记录是否需要抑制系统菜单，但不要立刻返回，先派发事件供应用处理
                bool suppressContextMenu = ShouldSuppressContextMenu(msg, x, y);

                switch (msg)
                {
                    case WM_RBUTTONDOWN:
                        Enqueue(new EventItem(EventKind.MouseDownRight, x, y));
                        break;
                    case WM_RBUTTONUP:
                        Enqueue(new EventItem(EventKind.MouseUpRight, x, y));
                        break;
                    case WM_MBUTTONDOWN:
                        Enqueue(new EventItem(EventKind.MouseDownMiddle, x, y));
                        break;
                    case WM_MBUTTONUP:
                        Enqueue(new EventItem(EventKind.MouseUpMiddle, x, y));
                        break;
                    case WM_XBUTTONDOWN:
                        {
                            var btn = GetXButton(p->mouseData);
                            Enqueue(new EventItem(btn == MouseButton.Button4 ? EventKind.MouseDownX1 : EventKind.MouseDownX2, x, y));
                        }
                        break;
                    case WM_XBUTTONUP:
                        {
                            var btn = GetXButton(p->mouseData);
                            Enqueue(new EventItem(btn == MouseButton.Button4 ? EventKind.MouseUpX1 : EventKind.MouseUpX2, x, y));
                        }
                        break;
                }

                // 事件派发后再抑制系统默认行为，避免阻断应用触发
                if (suppressContextMenu)
                {
                    return (IntPtr)1; // 非零表示拦截
                }
            }
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private static MouseButton GetXButton(int mouseData)
        {
            int hiWord = (mouseData >> 16) & 0xFFFF;
            return hiWord == XBUTTON1 ? MouseButton.Button4 : MouseButton.Button5;
        }

        // 仅在 Ctrl+右键 / Ctrl+中键 配置开启且目标为桌面时，抑制系统菜单
        private bool ShouldSuppressContextMenu(int msg, int x, int y)
        {
            if (!_ctrlDown) return false;
            if ((msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP) && HookConfig.SuppressCtrlRightClick)
                return IsDesktopWindowAt(x, y);
            if ((msg == WM_MBUTTONDOWN || msg == WM_MBUTTONUP) && HookConfig.SuppressCtrlMiddleClick)
                return IsDesktopWindowAt(x, y);
            return false;
        }

        // 判断坐标所在窗口是否为桌面（使用缓存句柄命中，失败时回退类名判断）
        private bool IsDesktopWindowAt(int x, int y)
        {
            try
            {
                POINT pt; pt.x = x; pt.y = y;
                IntPtr hwnd = WindowFromPoint(pt);
                if (hwnd == IntPtr.Zero) return false;
                IntPtr root = GetAncestor(hwnd, GA_ROOT);
                if (root == IntPtr.Zero) root = hwnd;
                if (_desktopRoots.Count > 0)
                    return _desktopRoots.Contains(root);
                // 缓存尚未建立或为空时的回退
                var cls = GetClassName(root);
                if (string.IsNullOrEmpty(cls)) return false;
                return cls == "Progman" || cls == "WorkerW";
            }
            catch { return false; }
        }

        // 尝试构建桌面窗口根句柄缓存
        private void TryBuildDesktopRoots()
        {
            try
            {
                _desktopRoots.Clear();
                // Progman
                IntPtr progman = FindWindow("Progman", null);
                if (progman != IntPtr.Zero)
                {
                    IntPtr root = GetAncestor(progman, GA_ROOT);
                    if (root == IntPtr.Zero) root = progman;
                    _desktopRoots.Add(root);
                    // Progman -> SHELLDLL_DefView
                    IntPtr defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (defView != IntPtr.Zero)
                    {
                        IntPtr top = GetAncestor(defView, GA_ROOT);
                        if (top == IntPtr.Zero) top = defView;
                        _desktopRoots.Add(top);
                    }
                }
                // 枚举顶层窗口添加 WorkerW 及其 DefView
                EnumWindows((h, l) =>
                {
                    var cls = GetClassName(h);
                    if (cls == "WorkerW")
                    {
                        IntPtr root = GetAncestor(h, GA_ROOT);
                        if (root == IntPtr.Zero) root = h;
                        _desktopRoots.Add(root);
                        IntPtr defView = FindWindowEx(h, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (defView != IntPtr.Zero)
                        {
                            IntPtr top = GetAncestor(defView, GA_ROOT);
                            if (top == IntPtr.Zero) top = defView;
                            _desktopRoots.Add(top);
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch { }
        }

        private static string GetClassName(IntPtr hwnd)
        {
            try
            {
                var sb = new StringBuilder(256);
                int len = GetClassNameW(hwnd, sb, sb.Capacity);
                if (len <= 0) return string.Empty;
                return sb.ToString();
            }
            catch { return string.Empty; }
        }

        // Win32 结构与导入
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        private const uint GA_ROOT = 2;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetClassNameW(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    }
}