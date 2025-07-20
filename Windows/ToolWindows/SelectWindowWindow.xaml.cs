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
        public delegate void WindowSelectedEventHandler(object sender, WindowSelectedEventArgs e); // 窗口选择事件委托
        public event WindowSelectedEventHandler WindowSelected; // 窗口选择事件

        private readonly WindowManager windowManager = new(); // 窗口管理器
        private readonly IconManager iconManager = new(); // 图标管理器
        private bool isSelecting = false; // 是否正在选择
        private Window ownerWindow; // 所有者窗口

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

        /// <summary>
        /// 开始窗口选择
        /// </summary>
        /// <param name="owner"> 所有者窗口 </param>
        public void StartSelecting(Window owner)
        {
            ownerWindow = owner; // 保存所有者窗口
            isSelecting = true; // 开始选择
            // 最小化所有者窗口
            if (ownerWindow != null)
            {
                ownerWindow.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// 鼠标按下事件处理
        /// </summary>
        private async void MouseHook_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSelecting) return; // 未开始选择
            if (e.LeftButton == MouseButtonState.Released) // 鼠标左键释放
            {
                isSelecting = false; // 停止选择

                var point = GetMousePosition(); // 获取鼠标位置
                nint windowHandle = GetWindowHandleFromPoint(point);
                if (IsOwnerWindow(windowHandle)) return;

                string windowTitle = GetWindowTitle(windowHandle); // 获取窗口标题
                string processPath = GetProcessPath(windowHandle); // 获取进程路径
                BitmapSource processIcon = GetProcessIcon(processPath); // 获取进程图标

                await RestoreOwnerWindowAsync();

                TriggerWindowSelectedEvent(windowHandle, windowTitle, processPath, processIcon); // 触发窗口选择事件
                MouseHook.MouseDown -= MouseHook_MouseDown; // 取消注册鼠标事件
            }
        }

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        /// <returns> 鼠标位置 </returns>
        private POINT GetMousePosition()
        {
            GetCursorPos(out POINT point); // 获取鼠标位置
            return point; // 返回鼠标位置
        }

        /// <summary>
        /// 根据坐标点获取窗口句柄
        /// </summary>
        /// <param name="point"> 坐标点 </param>
        /// <returns> 窗口句柄 </returns>
        private nint GetWindowHandleFromPoint(POINT point)
        {
            return WindowFromPoint(point.X, point.Y); // 获取窗口句柄
        }

        /// <summary>
        /// 判断指定窗口句柄是否为所有者窗口
        /// </summary>
        /// <param name="windowHandle"> 窗口句柄 </param>
        /// <returns> 是否为所有者窗口 </returns>
        private bool IsOwnerWindow(nint windowHandle)
        {
            return ownerWindow != null && windowHandle == new System.Windows.Interop.WindowInteropHelper(ownerWindow).Handle;
        }

        /// <summary>
        /// 获取指定进程路径的图标
        /// </summary>
        /// <param name="processPath"> 进程路径 </param>
        /// <returns> 图标 </returns>
        private BitmapSource GetProcessIcon(string processPath)
        {
            if (!string.IsNullOrEmpty(processPath))
            {
                try
                {
                    return iconManager.GetIcon(processPath) as BitmapSource; // 获取图标
                }
                catch { }
            }
            return null; // 图标获取失败
        }

        /// <summary>
        /// 恢复所有者窗口到正常状态并激活
        /// </summary>
        /// <returns> 异步任务 </returns>
        private async Task RestoreOwnerWindowAsync()
        {
            await Task.Delay(10); // 等待10毫秒，确保所有者窗口完全最小化
            if (ownerWindow != null)
            {
                ownerWindow.WindowState = WindowState.Normal; // 恢复所有者窗口
                ownerWindow.Activate(); // 所有者窗口激活
            }
        }

        /// <summary>
        /// 触发窗口选择事件
        /// </summary>
        /// <param name="windowHandle"> 窗口句柄 </param>
        /// <param name="windowTitle"> 窗口标题 </param>
        /// <param name="processPath"> 进程路径 </param>
        /// <param name="processIcon"> 进程图标 </param>
        private void TriggerWindowSelectedEvent(nint windowHandle, string windowTitle, string processPath, BitmapSource processIcon)
        {
            if (!string.IsNullOrEmpty(processPath))
            {
                WindowSelected?.Invoke(this, new WindowSelectedEventArgs
                {
                    WindowHandle = windowHandle,
                    WindowTitle = windowTitle,
                    ProcessPath = processPath,
                    ProcessIcon = processIcon
                }); // 触发窗口选择事件
            }
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="hWnd"> 窗口句柄 </param>
        /// <returns> 窗口标题 </returns>
        private string GetWindowTitle(nint hWnd)
        {
            StringBuilder title = new StringBuilder(256); // 窗口标题缓冲区
            GetWindowText(hWnd, title, title.Capacity); // 获取窗口标题
            return title.ToString(); // 返回窗口标题
        }

        /// <summary>
        /// 获取指定窗口句柄的进程路径
        /// </summary>
        /// <param name="hWnd"> 窗口句柄 </param>
        /// <returns> 进程路径 </returns>
        private string GetProcessPath(nint hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId); // 获取进程ID
                Process process = Process.GetProcessById((int)processId); // 获取进程对象
                return process.MainModule?.FileName ?? string.Empty; // 返回进程路径
            }
            catch
            {
                return string.Empty; // 进程获取失败
            }
        }

        public void Close()
        {
            MouseHook.MouseDown -= MouseHook_MouseDown; // 取消注册鼠标事件

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