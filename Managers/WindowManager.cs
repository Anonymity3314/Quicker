using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using Quicker.Windows;
using System.Windows;
using System.Text;

namespace Quicker.Managers
{
    public static class WindowManager
    {
        // 设置窗口位置和大小
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); // 窗口位置和大小

        // 查找窗口句柄
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern nint FindWindow(string lpClassName, string lpWindowName); // 查找窗口

        // 设置前台窗口
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd); // 设置前台窗口

        // 窗口置顶相关常量
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;

        // 获取当前活动窗口句柄
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow(); // 获取当前活动窗口句柄

        // 获取窗口标题
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(nint hWnd, StringBuilder text, int count); // 获取窗口标题

        // 获取窗口进程ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId); // 获取窗口进程ID

        // 判断窗口是否最大化
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsZoomed(nint hWnd); // 判断窗口是否最大化

        // 获取窗口样式
        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex); // 获取窗口样式

        // 获取系统参数
        [DllImport("user32.dll")]
        public static extern uint GetSystemMetrics(int nIndex); // 获取系统参数

        // 判断是否全屏
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT rect); // 判断是否全屏

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left; // 左边
            public int top; // 上边
            public int right; // 右边
            public int bottom; // 下边
        }

        private const uint WS_CAPTION = 0x00C00000; // 标题栏
        private const uint WS_POPUP = 0x80000000; // 弹出窗口
        private const int GWL_STYLE = -20; // 窗口样式
        private const int SM_CXSCREEN = 0; // 屏幕宽度
        private const int SM_CYSCREEN = 1; // 屏幕高度

        /// <summary>
        /// 设置窗口置顶
        /// </summary>
        /// <param name="window"></param>
        public static void SetWindowTopmost(Window window)
        {
            nint hWnd = new WindowInteropHelper(window).Handle; // 获取窗口句柄
            SetWindowPos(hWnd, new nint(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE); // 设置窗口置顶
        }

        /// <summary>
        /// 尝试打开已存在的窗口
        /// </summary>
        /// <param name="windowTitle">窗口标题</param>
        /// <param name="windowHandle">窗口句柄（可选）</param>
        public static void TryToOpenExitingWindow(string windowTitle, nint windowHandle = default)
        {
            if (windowHandle != nint.Zero) // 如果窗口存在，则尝试打开
                SetForegroundWindow(windowHandle); // 尝试打开已存在的窗口
            else
            {
                nint hWnd = FindWindow(null, windowTitle); // 查找窗口句柄
                if (hWnd != nint.Zero) SetForegroundWindow(hWnd); // 尝试打开已存在的窗口
            }
        }

        /// <summary>
        /// 获取当前前台窗口句柄
        /// </summary>
        /// <returns> 当前前台窗口句柄 </returns>
        public static nint GetCurrentForegroundWindow()
        {
            return GetForegroundWindow(); // 获取当前前台窗口句柄
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="hWnd"> 窗口句柄 </param>
        /// <returns> 窗口标题 </returns>
        public static string GetWindowText(nint hWnd)
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
        public static uint GetWindowProcessId(nint hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId); // 获取窗口进程ID
            return processId; // 返回窗口进程ID
        }

        /// <summary>
        /// 打开目标窗口
        /// </summary>
        /// <param name="targetWindow"> 目标窗口的类型名称 </param>
        public static void OpenTargetWindow(string targetWindow)
        {
            Window window = null; // 窗口实例
            switch (targetWindow)
            {
                case "SettingWindow":
                    window = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault();
                    break;
                case "ActionPageManageWindow":
                    window = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault();
                    break;
            }

            if (window != null) // 如果窗口存在
            {
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal; // 窗口最小化
                nint windowHandle = new WindowInteropHelper(window).Handle; // 获取窗口句柄
                TryToOpenExitingWindow(null, windowHandle); // 尝试打开已存在的窗口
            }
            else
            {
                window = CreateWindow(targetWindow); // 创建窗口实例
                window.Show(); // 显示窗口
            }
            window?.Activate(); // 激活窗口
        }

        /// <summary>
        /// 创建窗口实例
        /// </summary>
        /// <param name="windowType">窗口类型名称</param>
        /// <returns> 创建的窗口实例 </returns>
        private static Window CreateWindow(string windowType)
        {
            switch (windowType)
            {
                case "SettingWindow":
                    return new SettingWindow(); // 打开设置窗口
                case "ActionPageManageWindow":
                    return new ActionPageManageWindow(); // 打开动作页面管理窗口
                default:
                    return null;
            }
        }

        /// <summary>
        /// 判断是否全屏
        /// </summary>
        /// <returns> 是否全屏 </returns>
        public static bool IsFullScreen()
        {
            IntPtr hWnd = GetForegroundWindow(); // 获取当前前台窗口句柄
            uint style = GetWindowLong(hWnd, GWL_STYLE); // 获取窗口样式
            int screenWidth = (int)GetSystemMetrics(SM_CXSCREEN); // 获取屏幕宽度
            int screenHeight = (int)GetSystemMetrics(SM_CYSCREEN); // 获取屏幕高度

            if (!GetClientRect(hWnd, out RECT rect)) // 如果获取窗口大小失败
                return false; // 窗口不可见
            int windowWidth = rect.right - rect.left; // 获取窗口宽度
            int windowHeight = rect.bottom - rect.top; // 获取窗口高度

            bool isFullScreenCondition1 = (style & WS_POPUP) == WS_POPUP && (style & WS_CAPTION) != WS_CAPTION; // 窗口为弹出窗口且无标题栏
            bool isFullScreenCondition2 = windowWidth == screenWidth && windowHeight == screenHeight; // 窗口大小等于屏幕大小
            bool isFullScreenCondition3 = IsZoomed(hWnd); // 窗口最大化
            return isFullScreenCondition1 || isFullScreenCondition2 || isFullScreenCondition3; // 返回是否全屏
        }

        /// <summary>
        /// 获取进程名称
        /// </summary>
        /// <returns> 进程名称 </returns>
        public static string GetProcessName()
        {
            nint foregroundWindow = GetCurrentForegroundWindow(); // 获取当前前台窗口句柄
            uint processId = GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
            Process process = Process.GetProcessById((int)processId); // 获取进程实例
            return process.ProcessName; // 返回进程名称
        }
    }
}