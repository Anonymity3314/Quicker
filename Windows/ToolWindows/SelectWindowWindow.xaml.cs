using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;
using System.Text;

namespace Quicker.Windows.ToolWindows
{
    public partial class SelectWindowWindow
    {
        // 定义窗口选择事件委托和事件
        public delegate void WindowSelectedEventHandler(object sender, WindowSelectedEventArgs e);
        public event WindowSelectedEventHandler WindowSelected;

        private readonly WindowManager windowManager = new();
        private readonly IconManager iconManager = new();
        private bool isSelecting = false;
        private Window ownerWindow;

        public class WindowSelectedEventArgs : EventArgs
        {
            public nint WindowHandle { get; set; }
            public string WindowTitle { get; set; }
            public string ProcessPath { get; set; }
            public BitmapSource ProcessIcon { get; set; }
        }

        public SelectWindowWindow()
        {
            // 注册全局鼠标事件
            MouseHook.MouseDown += MouseHook_MouseDown;
        }

        public void StartSelecting(Window owner)
        {
            ownerWindow = owner;
            isSelecting = true;

            // 最小化所有者窗口
            if (ownerWindow != null)
            {
                ownerWindow.WindowState = WindowState.Minimized;
            }
        }

        private async void MouseHook_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSelecting) return;
            // 只处理鼠标左键点击事件
            if (e.LeftButton == MouseButtonState.Released)
            {
                isSelecting = false;

                // 获取鼠标位置
                GetCursorPos(out POINT point);

                // 获取鼠标位置下的窗口句柄
                nint windowHandle = WindowFromPoint(point.X, point.Y);

                // 如果是所有者窗口，则忽略
                if (ownerWindow != null && windowHandle == new System.Windows.Interop.WindowInteropHelper(ownerWindow).Handle)
                    return;

                // 获取窗口标题
                string windowTitle = GetWindowTitle(windowHandle);

                // 获取进程路径
                string processPath = GetProcessPath(windowHandle);

                // 获取进程图标
                BitmapSource processIcon = null;
                if (!string.IsNullOrEmpty(processPath))
                {
                    try
                    {
                        processIcon = iconManager.GetIcon(processPath) as BitmapSource;
                    }
                    catch
                    {
                        // 图标加载失败，忽略
                    }
                }

                // 延时10毫秒后恢复所有者窗口
                await Task.Delay(10);
                if (ownerWindow != null)
                {
                    ownerWindow.WindowState = WindowState.Normal;
                    ownerWindow.Activate();
                }

                // 触发窗口选择事件
                if (!string.IsNullOrEmpty(processPath))
                {
                    WindowSelected?.Invoke(this, new WindowSelectedEventArgs
                    {
                        WindowHandle = windowHandle,
                        WindowTitle = windowTitle,
                        ProcessPath = processPath,
                        ProcessIcon = processIcon
                    });
                }

                // 取消注册鼠标事件
                MouseHook.MouseDown -= MouseHook_MouseDown;
            }
        }

        private string GetWindowTitle(nint hWnd)
        {
            StringBuilder title = new StringBuilder(256);
            GetWindowText(hWnd, title, title.Capacity);
            return title.ToString();
        }

        private string GetProcessPath(nint hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                Process process = Process.GetProcessById((int)processId);
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void Close()
        {
            // 取消注册鼠标事件
            MouseHook.MouseDown -= MouseHook_MouseDown;

            // 释放资源
            windowManager.Dispose();
            iconManager.Dispose();
        }

        #region Win32 API
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern nint WindowFromPoint(int x, int y);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        #endregion
    }

    // 鼠标钩子类，用于捕获全局鼠标事件
    public static class MouseHook
    {
        public static event MouseButtonEventHandler MouseDown;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        static MouseHook()
        {
            _hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    // 创建鼠标按钮事件参数
                    MouseButtonEventArgs args = new MouseButtonEventArgs(
                        Mouse.PrimaryDevice,
                        Environment.TickCount,
                        MouseButton.Left
                    );

                    MouseDown?.Invoke(null, args);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void Unhook()
        {
            UnhookWindowsHookEx(_hookID); // 释放钩子
        }
    }
}