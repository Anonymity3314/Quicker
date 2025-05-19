using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.IO;
using Quicker;

namespace Quicker.Windows
{
    public partial class CustomMenu : Window
    {
        private readonly App app = (App.Current as App); // App实例
        private readonly SettingDatabase db1 = new(); // 数据库实例

        public CustomMenu()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            WindowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 弹出面板窗口
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                app.PreLoadMainWindow(); // 调用App类中的PreLoadMainWindow方法
                app.CloseOrShowMainWindow(); // 调用App类中的CloseOrShowMainWindow方法
            });
            this.Visibility = Visibility.Hidden;
        }

        // 弹出设置窗口
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenTargetWindow("SettingWindow"); // 打开设置窗口
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
            WindowManager.OpenTargetWindow("ActionPageManageWindow"); // 打开动作管理窗口
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

        // 未实现的功能
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        // 重启应用
        public void Restart(object sender, RoutedEventArgs e)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location; // 获取当前应用程序的完整路径
            string directory = Path.GetDirectoryName(exePath); // 获取可执行文件的目录
            string exeName = Path.GetFileNameWithoutExtension(exePath) + ".exe"; // 获取可执行文件的名称

            // 启动新进程运行当前应用程序
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = Path.Combine(directory, exeName),
                UseShellExecute = true
            });
            Application.Current.Shutdown();
        }

        // 退出应用
        private void Exit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        // 失去焦点时关闭窗口
        private void CustomMenu_Deactivated(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Hidden;
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