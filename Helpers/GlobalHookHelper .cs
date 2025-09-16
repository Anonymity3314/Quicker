using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Text;

namespace Quicker.Helpers
{
    /// <summary>
    /// 全局键鼠钩子帮助类
    /// </summary>
    public class GlobalHookHelper : IDisposable
    {
        #region 常量定义

        // 钩子类型
        private const int WH_KEYBOARD_LL = 13; // 键盘
        private const int WH_MOUSE_LL = 14; // 鼠标

        // 鼠标消息
        private const int WM_MOUSEMOVE = 0x0200; // 鼠标移动
        private const int WM_LBUTTONDOWN = 0x0201; // 鼠标左键按下
        private const int WM_LBUTTONUP = 0x0202; // 鼠标左键弹起
        private const int WM_RBUTTONDOWN = 0x0204; // 鼠标右键按下
        private const int WM_RBUTTONUP = 0x0205; // 鼠标右键弹起
        private const int WM_MBUTTONDOWN = 0x0207; // 鼠标中键按下
        private const int WM_MBUTTONUP = 0x0208; // 鼠标中键弹起
        private const int WM_XBUTTONDOWN = 0x020B; // 鼠标 X1 键按下
        private const int WM_XBUTTONUP = 0x020C; // 鼠标 X1 键弹起
        private const int WM_MOUSEWHEEL = 0x020A; // 鼠标滚轮

        // 键盘消息
        private const int WM_KEYDOWN = 0x0100; // 键盘按下
        private const int WM_KEYUP = 0x0101; // 键盘弹起
        private const int WM_SYSKEYDOWN = 0x0104; // 系统键盘按下
        private const int WM_SYSKEYUP = 0x0105; // 系统键盘弹起

        // XButton 常量
        private const int XBUTTON1 = 0x0001; // X1 键
        private const int XBUTTON2 = 0x0002; // X2 键

        #endregion

        #region API 声明

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        #endregion

        #region 结构体和委托

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region 事件定义

        public event EventHandler<GlobalKeyboardEventArgs> KeyboardEvent;
        public event EventHandler<GlobalMouseEventArgs> MouseEvent;

        #endregion

        #region 字段

        private IntPtr _keyboardHookHandle = IntPtr.Zero;
        private IntPtr _mouseHookHandle = IntPtr.Zero;
        private LowLevelKeyboardProc _keyboardProc;
        private LowLevelMouseProc _mouseProc;
        private bool _disposed = false;

        #endregion

        #region 构造函数和析构函数

        public GlobalHookHelper()
        {
            _keyboardProc = KeyboardHookProc;
            _mouseProc = MouseHookProc;
        }

        ~GlobalHookHelper()
        {
            Dispose(false);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 启动钩子
        /// </summary>
        public void Start()
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                InstallKeyboardHook();
                InstallMouseHook();
            }, DispatcherPriority.Render);
        }

        /// <summary>
        /// 停止钩子
        /// </summary>
        public void Stop()
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                UninstallKeyboardHook();
                UninstallMouseHook();
            }, DispatcherPriority.Render);
        }

        #endregion

        #region 钩子安装和卸载

        // 安装鼠标钩子
        private void InstallMouseHook()
        {
            if (_mouseHookHandle != IntPtr.Zero) return;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule!;
                _mouseHookHandle = SetWindowsHookEx(
                    WH_MOUSE_LL, _mouseProc, GetModuleHandle(curModule.ModuleName), 0);
            }, DispatcherPriority.Render);
        }

        // 安装键盘钩子
        private void InstallKeyboardHook()
        {
            if (_keyboardHookHandle != IntPtr.Zero) return;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule!;
                _keyboardHookHandle = SetWindowsHookEx(
                    WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
            }, DispatcherPriority.Render);
        }

        // 卸载鼠标钩子
        private void UninstallMouseHook()
        {
            if (_mouseHookHandle == IntPtr.Zero) return;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                if (_mouseHookHandle != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_mouseHookHandle);
                    _mouseHookHandle = IntPtr.Zero;
                }
            }, DispatcherPriority.Render);
        }

        // 卸载键盘钩子
        private void UninstallKeyboardHook()
        {
            if (_keyboardHookHandle == IntPtr.Zero) return;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                if (_keyboardHookHandle != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHookHandle);
                    _keyboardHookHandle = IntPtr.Zero;
                }
            }, DispatcherPriority.Render);
        }

        #endregion

        #region 钩子过程

        /// <summary>
        /// 鼠标钩子过程
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseEvent != null)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int message = wParam.ToInt32();

                MouseEventType eventType = MouseEventType.None;
                MouseButton button = MouseButton.Left;
                switch (message)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButton.Left;
                        eventType = MouseEventType.ButtonDown;
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButton.Left;
                        eventType = MouseEventType.ButtonUp;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButton.Right;
                        eventType = MouseEventType.ButtonDown;
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButton.Right;
                        eventType = MouseEventType.ButtonUp;
                        break;
                    case WM_MBUTTONDOWN:
                        button = MouseButton.Middle;
                        eventType = MouseEventType.ButtonDown;
                        break;
                    case WM_MBUTTONUP:
                        button = MouseButton.Middle;
                        eventType = MouseEventType.ButtonUp;
                        break;
                    case WM_XBUTTONDOWN:
                        // 获取X按钮信息
                        uint xButton = (hookStruct.mouseData >> 16) & 0xFFFF;
                        button = xButton == XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
                        eventType = MouseEventType.ButtonDown;
                        break;
                    case WM_XBUTTONUP:
                        // 获取X按钮信息
                        xButton = (hookStruct.mouseData >> 16) & 0xFFFF;
                        button = xButton == XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
                        eventType = MouseEventType.ButtonUp;
                        break;
                    case WM_MOUSEMOVE:
                        eventType = MouseEventType.Move;
                        break;
                    case WM_MOUSEWHEEL:
                        eventType = MouseEventType.Wheel;
                        break;
                }

                if (eventType != MouseEventType.None)
                {
                    var args = new GlobalMouseEventArgs
                    {
                        Button = button,
                        EventType = eventType,
                        X = hookStruct.pt.x,
                        Y = hookStruct.pt.y,
                        Timestamp = hookStruct.time,
                        MouseData = hookStruct.mouseData
                    };

                    MouseEvent?.Invoke(this, args);
                    if (args.Handled)
                    {
                        return (IntPtr)1; // 阻止事件传递
                    }
                }
            }
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        /// <summary>
        /// 键盘钩子过程
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && KeyboardEvent != null)
            {
                KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int message = wParam.ToInt32();

                KeyboardEventType eventType = KeyboardEventType.None;

                switch (message)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        eventType = KeyboardEventType.KeyDown;
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        eventType = KeyboardEventType.KeyUp;
                        break;
                }

                if (eventType != KeyboardEventType.None)
                {
                    // 检查修饰键状态
                    bool ctrl = (GetKeyState(0x11) & 0x8000) != 0; // VK_CONTROL
                    bool shift = (GetKeyState(0x10) & 0x8000) != 0; // VK_SHIFT
                    bool alt = (GetKeyState(0x12) & 0x8000) != 0;   // VK_MENU (Alt)

                    var args = new GlobalKeyboardEventArgs
                    {
                        KeyCode = (int)hookStruct.vkCode,
                        ScanCode = (int)hookStruct.scanCode,
                        EventType = eventType,
                        IsCtrlPressed = ctrl,
                        IsShiftPressed = shift,
                        IsAltPressed = alt,
                        Timestamp = hookStruct.time
                    };

                    KeyboardEvent?.Invoke(this, args);
                    if (args.Handled)
                    {
                        return (IntPtr)1; // 阻止事件传递
                    }
                }
            }
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Stop();
            }, DispatcherPriority.Send);

            _disposed = true;
        }

        #endregion
    }

    #region 事件参数类

    /// <summary>
    /// 鼠标事件类型
    /// </summary>
    public enum MouseEventType
    {
        None,
        ButtonDown,
        ButtonUp,
        Move,
        Wheel
    }

    /// <summary>
    /// 键盘事件类型
    /// </summary>
    public enum KeyboardEventType
    {
        None,
        KeyDown,
        KeyUp
    }

    /// <summary>
    /// 全局鼠标事件参数
    /// </summary>
    public class GlobalMouseEventArgs : EventArgs
    {
        public MouseButton Button { get; set; }
        public MouseEventType EventType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public uint Timestamp { get; set; }
        public uint MouseData { get; set; }
        public bool Handled { get; set; }
    }

    /// <summary>
    /// 全局键盘事件参数
    /// </summary>
    public class GlobalKeyboardEventArgs : EventArgs
    {
        public int KeyCode { get; set; }
        public int ScanCode { get; set; }
        public KeyboardEventType EventType { get; set; }
        public bool IsCtrlPressed { get; set; }
        public bool IsShiftPressed { get; set; }
        public bool IsAltPressed { get; set; }
        public uint Timestamp { get; set; }
        public bool Handled { get; set; }
    }

    #endregion
}