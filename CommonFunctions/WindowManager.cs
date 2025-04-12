using System.Runtime.InteropServices;
using System.Windows.Interop;
using Quicker.Windows;
using System.Windows;
using System.Text;
using System;

namespace Quicker.CommonFunctions
{
    internal class WindowManager
    {
        // 设置窗口位置和大小
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); // 设置窗口位置和大小

        // 查找窗口句柄
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName); // 查找窗口句柄

        // 设置前台窗口
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd); // 设置前台窗口

        // 获取当前活动窗口句柄
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow(); // 获取当前活动窗口句柄

        // 获取窗口标题
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count); // 获取窗口标题

        // 获取窗口进程ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId); // 获取窗口进程ID

        // 窗口置顶相关常量
        private const int HWND_TOPMOST = -1; // 置顶
        private const int SWP_NOSIZE = 0x0001; // 不改变大小
        private const int SWP_NOMOVE = 0x0002; // 不改变位置

        /// <summary>
        /// 设置窗口置顶
        /// </summary>
        /// <param name="windows"></param>
        public void SetWindowTopmost(Window windows)
        {
            IntPtr hWnd = new WindowInteropHelper(windows).Handle; // 获取窗口句柄
            SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE); // 设置窗口置顶
        }

        /// <summary>
        /// 尝试打开已存在的窗口
        /// </summary>
        /// <param name="windowTitle">窗口标题</param>
        /// <param name="windowHandle">窗口句柄（可选）</param>
        public void TryToOpenExitingWindow(string windowTitle, IntPtr windowHandle = default)
        {           
            if (windowHandle != IntPtr.Zero) SetForegroundWindow(windowHandle); // 如果提供了窗口句柄，则直接使用该句柄
            else // 如果没有提供窗口句柄，则通过标题查找
            {
                IntPtr hWnd = FindWindow(null, windowTitle);
                if (hWnd != IntPtr.Zero) SetForegroundWindow(hWnd);
            }
        }

        // 获取当前前台窗口句柄
        public IntPtr GetCurrentForegroundWindow()
        {
            return GetForegroundWindow(); // 调用静态外部方法
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public string GetWindowText(IntPtr hWnd)
        {
            StringBuilder text = new StringBuilder(256);
            GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }

        /// <summary>
        /// 获取窗口进程ID
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public uint GetWindowProcessId(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            return processId;
        }

        /// <summary>
        /// 打开目标窗口
        /// </summary>
        /// <param name="targetWindow">目标窗口的类型名称</param>
        public void OpenTargetWindow(string targetWindow)
        {
            Window window = null;
            switch (targetWindow) // 尝试查找已存在的窗口
            {
                case "SettingWindow":
                    window = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault();
                    break;
                case "ActionPageManageWindow":
                    window = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault();
                    break;
            }

            if (window != null)
            {               
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal; // 如果窗口被最小化，则恢复窗口
                IntPtr windowHandle = new WindowInteropHelper(window).Handle; // 获取窗口句柄
                TryToOpenExitingWindow(null, windowHandle); // 将窗口置于前台
            }
            else // 如果未找到窗口，则创建并显示新窗口
            {
                window = CreateWindow(targetWindow);
                window.Show();
            }
            window?.Activate(); // 激活窗口
        }

        /// <summary>
        /// 创建窗口实例
        /// </summary>
        /// <param name="windowType">窗口类型名称</param>
        /// <returns>创建的窗口实例</returns>
        private Window CreateWindow(string windowType)
        {
            switch (windowType)
            {
                case "SettingWindow":
                    return new SettingWindow();
                case "ActionPageManageWindow":
                    return new ActionPageManageWindow();
                default:
                    throw new NotSupportedException($"不支持的窗口类型: {windowType}");
            }
        }
    }
}