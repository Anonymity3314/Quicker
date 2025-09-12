using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;

namespace Quicker.Managers
{
    /// <summary>
    /// 基于 UI Automation 的全局钩子管理器，用于替代 SharpHook
    /// </summary>
    public class UIAutomationHookManager : IDisposable
    {
        #region Windows API 声明
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int HC_ACTION = 0;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

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
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region 事件定义
        public event EventHandler<KeyboardHookEventArgs>? KeyPressed;
        public event EventHandler<KeyboardHookEventArgs>? KeyReleased;
        public event EventHandler<MouseHookEventArgs>? MousePressed;
        public event EventHandler<MouseHookEventArgs>? MouseReleased;
        #endregion

        #region 私有字段
        private readonly LowLevelKeyboardProc _keyboardProc;
        private readonly LowLevelMouseProc _mouseProc;
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private bool _disposed = false;
        private readonly Dispatcher _dispatcher;
        #endregion

        #region 构造函数
        public UIAutomationHookManager()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 启动钩子
        /// </summary>
        public async Task RunAsync()
        {
            await Task.Run(() =>
            {
                _keyboardHookId = SetHook(_keyboardProc, WH_KEYBOARD_LL);
                _mouseHookId = SetHook(_mouseProc, WH_MOUSE_LL);
                
                if (_keyboardHookId == IntPtr.Zero)
                {
                    throw new InvalidOperationException("无法设置键盘钩子");
                }
                
                if (_mouseHookId == IntPtr.Zero)
                {
                    throw new InvalidOperationException("无法设置鼠标钩子");
                }
                
                System.Diagnostics.Debug.WriteLine("钩子设置成功");
            });
        }

        /// <summary>
        /// 停止钩子
        /// </summary>
        public void Stop()
        {
            if (_keyboardHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookId);
                _keyboardHookId = IntPtr.Zero;
            }

            if (_mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookId);
                _mouseHookId = IntPtr.Zero;
            }
        }
        #endregion

        #region 私有方法
        private IntPtr SetHook(LowLevelKeyboardProc proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetHook(LowLevelMouseProc proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 添加基本调试信息
            System.Diagnostics.Debug.WriteLine($"KeyboardHookCallback called: nCode={nCode}");
            
            if (nCode >= HC_ACTION)
            {
                var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                var vkCode = hookStruct.vkCode;
                var isPressed = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;

                // 添加调试信息
                System.Diagnostics.Debug.WriteLine($"Keyboard Hook: VKCode={vkCode}, IsPressed={isPressed}");

                // 只处理我们关心的按键
                if (IsKeyCodeSupported(vkCode))
                {
                    var keyCode = (KeyCode)vkCode;
                    
                    // 直接触发事件，不使用 Dispatcher
                    try
                    {
                        var eventArgs = new KeyboardHookEventArgs(keyCode);
                        if (isPressed)
                        {
                            System.Diagnostics.Debug.WriteLine($"Triggering KeyPressed event for {keyCode}");
                            KeyPressed?.Invoke(this, eventArgs);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Triggering KeyReleased event for {keyCode}");
                            KeyReleased?.Invoke(this, eventArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in keyboard callback: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"KeyCode {vkCode} not supported");
                }
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= HC_ACTION)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var isPressed = IsMouseButtonDown(wParam);
                var button = GetMouseButton(wParam, hookStruct.mouseData);

                if (button != MouseButton.Button1) // 只处理非左键事件，避免与系统冲突
                {
                    // 直接触发事件，不使用 Dispatcher
                    try
                    {
                        var eventArgs = new MouseHookEventArgs(button, hookStruct.pt.x, hookStruct.pt.y);
                        if (isPressed)
                        {
                            System.Diagnostics.Debug.WriteLine($"Triggering MousePressed event for {button}");
                            MousePressed?.Invoke(this, eventArgs);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Triggering MouseReleased event for {button}");
                            MouseReleased?.Invoke(this, eventArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in mouse callback: {ex.Message}");
                    }
                }
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private bool IsMouseButtonDown(IntPtr wParam)
        {
            return wParam == (IntPtr)WM_RBUTTONDOWN || 
                   wParam == (IntPtr)WM_MBUTTONDOWN || 
                   wParam == (IntPtr)WM_XBUTTONDOWN;
        }

        private MouseButton GetMouseButton(IntPtr wParam, uint mouseData)
        {
            if (wParam == (IntPtr)WM_RBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONUP)
                return MouseButton.Button2;
            else if (wParam == (IntPtr)WM_MBUTTONDOWN || wParam == (IntPtr)WM_MBUTTONUP)
                return MouseButton.Button3;
            else if (wParam == (IntPtr)WM_XBUTTONDOWN || wParam == (IntPtr)WM_XBUTTONUP)
            {
                // X1 和 X2 按钮通过 mouseData 的高位字区分
                return (mouseData & 0xFFFF0000) == 0x010000 ? MouseButton.Button4 : MouseButton.Button5;
            }
            
            return MouseButton.Button1;
        }

        /// <summary>
        /// 检查虚拟键码是否被支持
        /// </summary>
        /// <param name="vkCode">虚拟键码</param>
        /// <returns>是否支持</returns>
        private bool IsKeyCodeSupported(uint vkCode)
        {
            // 特别处理 Ctrl 键，因为这是最重要的
            if (vkCode == 162 || vkCode == 163) // VK_LCONTROL, VK_RCONTROL
            {
                System.Diagnostics.Debug.WriteLine($"Ctrl key detected: VKCode={vkCode}");
                return true;
            }
            
            // 检查是否在我们的 KeyCode 枚举中
            return Enum.IsDefined(typeof(KeyCode), (int)vkCode);
        }
        #endregion

        #region IDisposable 实现
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }
                _disposed = true;
            }
        }

        ~UIAutomationHookManager()
        {
            Dispose(false);
        }
        #endregion
    }

    #region 事件参数类
    /// <summary>
    /// 键盘钩子事件参数
    /// </summary>
    public class KeyboardHookEventArgs : EventArgs
    {
        public KeyCode KeyCode { get; }
        public KeyboardData Data { get; }

        public KeyboardHookEventArgs(KeyCode keyCode)
        {
            KeyCode = keyCode;
            Data = new KeyboardData { KeyCode = keyCode };
        }
    }

    /// <summary>
    /// 鼠标钩子事件参数
    /// </summary>
    public class MouseHookEventArgs : EventArgs
    {
        public MouseButton Button { get; }
        public int X { get; }
        public int Y { get; }
        public MouseData Data { get; }

        public MouseHookEventArgs(MouseButton button, int x, int y)
        {
            Button = button;
            X = x;
            Y = y;
            Data = new MouseData { Button = button, X = x, Y = y };
        }
    }

    /// <summary>
    /// 键盘数据
    /// </summary>
    public class KeyboardData
    {
        public KeyCode KeyCode { get; set; }
    }

    /// <summary>
    /// 鼠标数据
    /// </summary>
    public class MouseData
    {
        public MouseButton Button { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        Button1 = 1, // 左键
        Button2 = 2, // 右键
        Button3 = 3, // 中键
        Button4 = 4, // X1
        Button5 = 5  // X2
    }

    /// <summary>
    /// 键盘按键代码枚举（使用正确的 Windows 虚拟键码）
    /// </summary>
    public enum KeyCode
    {
        VcLeftControl = 162,  // VK_LCONTROL
        VcRightControl = 163, // VK_RCONTROL
        VcLeftAlt = 164,      // VK_LMENU
        VcRightAlt = 165,     // VK_RMENU
        VcLeftShift = 160,    // VK_LSHIFT
        VcRightShift = 161,   // VK_RSHIFT
        VcSpace = 32,
        VcEnter = 13,
        VcEscape = 27,
        VcTab = 9,
        VcBack = 8,
        VcDelete = 46,
        VcInsert = 45,
        VcHome = 36,
        VcEnd = 35,
        VcPageUp = 33,
        VcPageDown = 34,
        VcUp = 38,
        VcDown = 40,
        VcLeft = 37,
        VcRight = 39,
        VcF1 = 112,
        VcF2 = 113,
        VcF3 = 114,
        VcF4 = 115,
        VcF5 = 116,
        VcF6 = 117,
        VcF7 = 118,
        VcF8 = 119,
        VcF9 = 120,
        VcF10 = 121,
        VcF11 = 122,
        VcF12 = 123,
        VcA = 65,
        VcB = 66,
        VcC = 67,
        VcD = 68,
        VcE = 69,
        VcF = 70,
        VcG = 71,
        VcH = 72,
        VcI = 73,
        VcJ = 74,
        VcK = 75,
        VcL = 76,
        VcM = 77,
        VcN = 78,
        VcO = 79,
        VcP = 80,
        VcQ = 81,
        VcR = 82,
        VcS = 83,
        VcT = 84,
        VcU = 85,
        VcV = 86,
        VcW = 87,
        VcX = 88,
        VcY = 89,
        VcZ = 90,
        VcD0 = 48,
        VcD1 = 49,
        VcD2 = 50,
        VcD3 = 51,
        VcD4 = 52,
        VcD5 = 53,
        VcD6 = 54,
        VcD7 = 55,
        VcD8 = 56,
        VcD9 = 57
    }
    #endregion
}
