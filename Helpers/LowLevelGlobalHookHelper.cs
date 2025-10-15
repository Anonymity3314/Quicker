using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Diagnostics;

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

        // 将事件分发异步排队，避免在钩子回调线程内执行用户代码
        private static void FireAsync(Action action)
        {
            try
            {
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    try { action(); } catch { }
                }, null);
            }
            catch { }
        }

        // 对外 API
        public Task RunAsync()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(null), 0);
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(null), 0);
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
        }

        // 键盘回调
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                KBDLLHOOKSTRUCT info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    if (info.vkCode == VK_LCONTROL)
                        FireAsync(() => KeyPressed?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcLeftControl)));
                    else if (info.vkCode == VK_RCONTROL)
                        FireAsync(() => KeyPressed?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcRightControl)));
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    if (info.vkCode == VK_LCONTROL)
                        FireAsync(() => KeyReleased?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcLeftControl)));
                    else if (info.vkCode == VK_RCONTROL)
                        FireAsync(() => KeyReleased?.Invoke(this, new KeyboardHookEventArgs(KeyCode.VcRightControl)));
                }
            }
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        // 鼠标回调（包含必要的系统菜单抑制）
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                MSLLHOOKSTRUCT info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                // 坐标
                int x = info.pt.x;
                int y = info.pt.y;

                // 先记录是否需要抑制系统菜单，但不要立刻返回，先派发事件供应用处理
                bool suppressContextMenu = ShouldSuppressContextMenu(msg, info);

                switch (msg)
                {
                    case WM_RBUTTONDOWN:
                        FireAsync(() => MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button2, x, y)));
                        break;
                    case WM_RBUTTONUP:
                        FireAsync(() => MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button2, x, y)));
                        break;
                    case WM_MBUTTONDOWN:
                        FireAsync(() => MousePressed?.Invoke(this, new MouseHookEventArgs(MouseButton.Button3, x, y)));
                        break;
                    case WM_MBUTTONUP:
                        FireAsync(() => MouseReleased?.Invoke(this, new MouseHookEventArgs(MouseButton.Button3, x, y)));
                        break;
                    case WM_XBUTTONDOWN:
                        {
                            var btn = GetXButton(info.mouseData);
                            FireAsync(() => MousePressed?.Invoke(this, new MouseHookEventArgs(btn, x, y)));
                        }
                        break;
                    case WM_XBUTTONUP:
                        {
                            var btn = GetXButton(info.mouseData);
                            FireAsync(() => MouseReleased?.Invoke(this, new MouseHookEventArgs(btn, x, y)));
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

        private static bool IsCtrlDown()
        {
            short l = GetAsyncKeyState(VK_LCONTROL);
            short r = GetAsyncKeyState(VK_RCONTROL);
            return (l & 0x8000) != 0 || (r & 0x8000) != 0;
        }

        // 仅在 Ctrl+右键 / Ctrl+中键 配置开启时，抑制系统菜单
        private static bool ShouldSuppressContextMenu(int msg, MSLLHOOKSTRUCT info)
        {
            if (!IsCtrlDown()) return false;
            if ((msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP) && HookConfig.SuppressCtrlRightClick)
                return true;
            if ((msg == WM_MBUTTONDOWN || msg == WM_MBUTTONUP) && HookConfig.SuppressCtrlMiddleClick)
                return true;
            return false;
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
