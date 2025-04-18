using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media.Imaging;
using Quicker.CommonFunctions;
using System.Windows.Interop;
using Quicker.Database;
using System.Windows;
using System.IO;

namespace Quicker.Windows
{
    public partial class CustomMenu : Window
    {
        private readonly WindowManager windowManager; // 窗口管理器
        private readonly SettingDatabase db1; // 数据库实例
        private readonly App app; // App实例

        public CustomMenu()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden; // 隐藏窗口

            app = (App.Current as App); // 获取App实例

            db1 = new SettingDatabase();
            db1.Initialize();

            windowManager = new WindowManager();
        }

        // 后台加载数据库
        private void CustomMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                SettingDatabase db1 = new();
                db1.Initialize();
            });
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 弹出面板窗口
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                app.CloseOrShowMainWindow(); // 调用App类中的CloseOrShowMainWindow方法
            });
            this.Visibility = Visibility.Hidden;
        }

        // 弹出设置窗口
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            windowManager.OpenTargetWindow("SettingWindow");
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
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

        // 未实现的功能
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        // 重启应用
        private void Restart(object sender, RoutedEventArgs e)
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
    }
}