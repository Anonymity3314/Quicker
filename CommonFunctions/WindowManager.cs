using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System;

namespace Quicker.CommonFunctions
{
    // 窗口管理器接口
    public interface IWindowManager
    {
        void SetWindowTopmost(Window window); // 设置窗口置顶
        void TryToOpenExitingWindow(string windowTitle); // 尝试打开已存在的窗口
    }

    // 窗口管理器实现
    internal class WindowManager : IWindowManager
    {
        // 查找窗口和设置前台窗口
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); // 设置窗口位置
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName); // 查找窗口句柄
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd); // 将窗口置于前台

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
        /// <param name="windowTitle"></param>
        public void TryToOpenExitingWindow(string windowTitle)
        {
            IntPtr windowHandle = FindWindow(null, windowTitle); // 查找窗口句柄
            if (windowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(windowHandle); // 将窗口置于前台
                return; // 如果找到窗口，则将窗口置于前台
            } // 如果找到窗口并且允许打开已存在的窗口，则将窗口置于前台
        }
    }
}
