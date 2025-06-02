using System.Runtime.InteropServices;
using Quicker.Windows.MainWindows;
using System.Windows.Interop;
using System.Diagnostics;
using Quicker.Windows;
using System.Windows;
using System.Text;

namespace Quicker.Managers
{
    public class WindowManager : IDisposable
    {
        // 设置窗口位置和大小
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint); // 获取鼠标位置

        // 设置窗口位置和大小
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); // 设置窗口位置和大小

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X; // 鼠标X坐标
            public int Y; // 鼠标Y坐标
        }

        // 查找窗口句柄
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern nint FindWindow(string lpClassName, string lpWindowName); // 查找窗口句柄

        // 设置前台窗口
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd); // 设置前台窗口

        // 窗口置顶相关常量
        private const int HWND_TOPMOST = -1; // 置顶
        private const int SWP_NOSIZE = 0x0001; // 不调整大小
        private const int SWP_NOMOVE = 0x0002; // 不调整位置
        private const int SWP_NOZORDER = 0x0004; // 不调整Z轴顺序

        // 获取当前活动窗口句柄
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow(); // 当前活动窗口句柄

        // 获取窗口标题
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(nint hWnd, StringBuilder text, int count); // 窗口标题

        // 获取窗口进程ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId); // 窗口进程ID

        // 判断窗口是否最大化
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsZoomed(nint hWnd); // 窗口最大化

        // 获取窗口样式
        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex); // 窗口样式

        // 获取系统参数
        [DllImport("user32.dll")]
        public static extern uint GetSystemMetrics(int nIndex); // 屏幕宽度

        // 判断是否全屏
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT rect); // 窗口客户区矩形

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left; // 窗口左边缘
            public int top;  // 窗口上边缘
            public int right; // 窗口右边缘
            public int bottom; // 窗口下边缘
        }

        // 窗口样式常量
        private const uint WS_CAPTION = 0x00C00000; // 标题栏
        private const uint WS_POPUP = 0x80000000; // 弹出窗口
        private const int GWL_STYLE = -20; // 窗口样式
        private const int SM_CXSCREEN = 0; // 屏幕宽度
        private const int SM_CYSCREEN = 1; // 屏幕高度

        /// <summary>
        /// 将窗口置顶
        /// </summary>
        /// <param name="window"> 窗口对象 </param>
        public void SetWindowTopmost(Window window)
        {
            nint hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, new nint(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }

        /// <summary>
        /// 尝试打开已经存在的窗口
        /// </summary>
        /// <param name="windowTitle"> 窗口标题 </param>
        /// <param name="windowHandle"> 窗口句柄 </param>
        public void TryToOpenExitingWindow(string windowTitle, nint windowHandle = default)
        {
            if (windowHandle != nint.Zero) // 窗口句柄不为空
                SetForegroundWindow(windowHandle); // 尝试打开已存在的窗口
            else
            {
                nint hWnd = FindWindow(null, windowTitle); // 查找窗口句柄
                if (hWnd != nint.Zero) SetForegroundWindow(hWnd); // 尝试打开已存在的窗口
            }
        }

        /// <summary>
        /// 获取当前活动窗口句柄
        /// </summary>
        /// <returns> 当前活动窗口句柄 </returns>
        public nint GetCurrentForegroundWindow()
        {
            return GetForegroundWindow(); // 获取当前活动窗口句柄
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="hWnd"> 窗口句柄 </param>
        /// <returns> 窗口标题 </returns>
        public string GetWindowText(nint hWnd)
        {
            StringBuilder text = new StringBuilder(256); // 窗口标题缓冲区
            GetWindowText(hWnd, text, text.Capacity); // 获取窗口标题
            return text.ToString(); // 返回窗口标题
        }

        /// <summary>
        /// 获取窗口进程ID
        /// </summary>
        /// <param name="hWnd"> 窗口句柄 </param>
        /// <returns> 窗口进程ID </returns>
        public uint GetWindowProcessId(nint hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId); // 获取窗口进程ID
            return processId; // 返回窗口进程ID
        }

        /// <summary>
        /// 打开目标窗口
        /// </summary>
        /// <param name="targetWindow"> 窗口对象 </param>
        public void OpenTargetWindow(string targetWindow)
        {
            Window window = null; // 窗口对象
            switch (targetWindow)
            {
                case "SettingWindow":
                    window = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault();
                    break;
                case "ActionPageManageWindow":
                    window = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault();
                    break;
            }

            if (window != null) // 窗口对象不为空
            {
                if (window.WindowState == WindowState.Minimized) // 窗口最小化
                    window.WindowState = WindowState.Normal; // 窗口恢复
                nint windowHandle = new WindowInteropHelper(window).Handle; // 获取窗口句柄
                TryToOpenExitingWindow(null, windowHandle); // 尝试打开已存在的窗口
            }
            else
            {
                window = CreateWindow(targetWindow); // 创建窗口对象
                window.Show(); // 显示窗口
            }
            window?.Activate(); // 激活窗口
        }

        /// <summary>
        /// 创建窗口对象
        /// </summary>
        /// <param name="windowType"> 窗口类型 </param>
        /// <returns> 窗口对象 </returns>
        private Window CreateWindow(string windowType)
        {
            switch (windowType)
            {
                case "SettingWindow":
                    return new SettingWindow(); // 创建设置窗口
                case "ActionPageManageWindow":
                    return new ActionPageManageWindow(); // 创建动作页面管理窗口
                default:
                    return null; // 窗口类型不存在
            }
        }

        /// <summary>
        /// 判断是否全屏
        /// </summary>
        /// <returns> 是否全屏 </returns>
        public bool IsFullScreen()
        {
            IntPtr hWnd = GetForegroundWindow(); // 获取当前活动窗口句柄
            uint style = GetWindowLong(hWnd, GWL_STYLE); // 获取窗口样式
            int screenWidth = (int)GetSystemMetrics(SM_CXSCREEN); // 获取屏幕宽度
            int screenHeight = (int)GetSystemMetrics(SM_CYSCREEN); // 获取屏幕高度

            if (!GetClientRect(hWnd, out RECT rect))
                return false; // 窗口客户区矩形获取失败
            int windowWidth = rect.right - rect.left; // 窗口宽度
            int windowHeight = rect.bottom - rect.top; // 窗口高度

            bool isFullScreenCondition1 = (style & WS_POPUP) == WS_POPUP && (style & WS_CAPTION) != WS_CAPTION; // 窗口为弹出窗口且无标题栏
            bool isFullScreenCondition2 = windowWidth == screenWidth && windowHeight == screenHeight; // 窗口大小等于屏幕大小
            bool isFullScreenCondition3 = IsZoomed(hWnd); // 窗口最大化
            return isFullScreenCondition1 || isFullScreenCondition2 || isFullScreenCondition3; // 返回是否全屏
        }

        /// <summary>
        /// 获取焦点窗口的进程名称
        /// </summary>
        /// <returns> 进程名称 </returns>
        public string GetProcessName()
        {
            nint foregroundWindow = GetCurrentForegroundWindow(); // 获取当前活动窗口句柄
            uint processId = GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
            Process process = Process.GetProcessById((int)processId); // 获取进程对象
            return process.ProcessName; // 返回进程名称
        }

        // 设置主窗口焦点
        public void SetMainWindowFocused()
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 获取主窗口对象
            if (mainWindow != null) // 主窗口对象不为空
                mainWindow.Activate(); // 激活主窗口
        }

        // 延时关闭窗口
        public async Task CloseMenuAsync(Window window)
        {
            await Task.Delay(1000); // 延迟100毫秒
            window.Close(); // 关闭窗口
        }

        /// <summary>
        /// 获取鼠标位置并设置窗口位置
        /// </summary>
        /// <param name="window"> 要定位的窗口 </param>
        /// <param name="left"> 窗口左边界 </param>
        /// <param name="top"> 窗口上边界 </param>
        private void SetWindowPosition(Window window, int left, int top)
        {
            nint hWnd = new WindowInteropHelper(window).Handle; // 获取窗口句柄
            SetWindowPos(hWnd, IntPtr.Zero, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER); // 设置窗口位置
        }

        /// <summary>
        /// 将窗口定位到鼠标位置附近
        /// </summary>
        /// <param name="window"> 要定位的窗口 </param>
        public void SetWindowPositionNearMouse(Window window)
        {
            GetCursorPos(out POINT cursorPos); // 获取鼠标位置
            SetWindowPosition(window, cursorPos.X, cursorPos.Y); // 设置窗口位置
        }

        /// <summary>
        /// 将窗口左下角定位到鼠标位置附近
        /// </summary>
        /// <param name="window"> 要定位的窗口 </param>
        public void SetWindowBottomLeftNearMouse(Window window)
        {
            GetCursorPos(out POINT cursorPos); // 获取鼠标位置

            // 计算窗口左下角的位置
            int left = cursorPos.X - 20; // 窗口左边界
            int top = cursorPos.Y - 290; // 窗口上边界（鼠标Y坐标减去窗口高度）

            SetWindowPosition(window, left, top); // 设置窗口位置
        }

        // 释放资源
        public void Dispose()
        {
            // 释放资源
        }
    }
}