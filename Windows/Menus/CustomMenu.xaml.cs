using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;
using System.IO;

namespace Quicker.Windows.Menus
{
    public partial class CustomMenu : Window
    {
        private readonly App app = (App.Current as App); // App实例

        public CustomMenu()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 弹出面板窗口
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                app.PreLoadMainWindow(); // 调用App类中的PreLoadMainWindow方法
                app.CloseOrShowMainWindow(); // 调用App类中的CloseOrShowMainWindow方法
            }); // 调用Dispatcher.Invoke方法确保在主线程中执行
            this.Visibility = Visibility.Hidden; // 隐藏当前窗口
        }

        // 弹出设置窗口
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("SettingWindow"); // 打开设置窗口
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("ActionPageManageWindow"); // 打开动作管理窗口
        }

        // 暂停Quicker
        private void PauseQuicker(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                app.PauseQuicker(sender, e); // 调用App类中的PauseQuicker方法 
            });
            this.Visibility = Visibility.Hidden; // 隐藏当前窗口
        }

        // 重启应用
        public void Restart(object sender, RoutedEventArgs e)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location; // 获取当前应用程序的完整路径
            string directory = Path.GetDirectoryName(exePath); // 获取可执行文件的目录
            string exeName = Path.GetFileNameWithoutExtension(exePath) + ".exe"; // 获取可执行文件的名称
            SingleInstanceManager.ReleaseMutex(); // 释放互斥锁
            Application.Current.Shutdown(); // 关闭当前应用程序
            Process.Start(new ProcessStartInfo // 启动新进程
            {
                FileName = Path.Combine(directory, exeName), // 指定可执行文件的路径
                UseShellExecute = true // 使用ShellExecute来启动程序
            });
        }

        // 退出应用
        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // 关闭当前应用程序
        }

        // 失去焦点时关闭窗口
        private void CustomMenu_Deactivated(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            GC.Collect(); // 强制回收非托管资源
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制回收非托管资源
        }
    }
}