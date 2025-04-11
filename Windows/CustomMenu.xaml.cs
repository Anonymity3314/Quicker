using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Quicker.Database;
using System.Windows;
using System.IO;

namespace Quicker.Windows
{
    public partial class CustomMenu : Window
    {
        // 设置窗口位置
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)] // 返回值为布尔类型
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags); // 设置窗口位置

        // 查找窗口和设置前台窗口
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] // 指定字符集
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName); // 查找窗口句柄
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd); // 将窗口置于前台
        private readonly SettingDatabase db1; // 数据库实例
        private readonly App app; // App实例

        // 窗口置顶相关常量
        private const int HWND_TOPMOST = -1; // 置顶
        private const int SWP_NOSIZE = 0x0001; // 大小不变
        private const int SWP_NOMOVE = 0x0002; // 位置不变

        public CustomMenu()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden; // 隐藏窗口

            app = (App.Current as App); // 获取App实例

            db1 = new SettingDatabase();
            db1.InitializeDatabase();
        }

        // 后台加载数据库
        private void CustomMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                SettingDatabase db1 = new();
                db1.InitializeDatabase();
            });

            // 设置窗口置顶
            IntPtr hWnd = new WindowInteropHelper(this).Handle; // 获取窗口句柄
            SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE); // 设置窗口置顶
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
        public void ShowSettingWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => // 确保UI操作在主线程中执行
            {
                SettingWindow settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (settingWindow != null) // 如果找到现有的设置窗口
                {
                    if (settingWindow.WindowState == WindowState.Minimized) // 如果设置窗口被最小化
                    {
                        settingWindow.WindowState = WindowState.Normal; // 恢复窗口
                    }
                    SetForegroundWindow(new WindowInteropHelper(settingWindow).Handle); // 将窗口置于前台
                }
                else // 如果没有找到现有的设置窗口，则创建并显示新窗口
                {
                    settingWindow = new(); // 创建设置窗口实例
                    settingWindow.Show(); // 显示设置窗口
                }
                settingWindow.Activate(); // 激活设置窗口
                this.Visibility = Visibility.Hidden; // 隐藏当前的CustomMenu窗口
            });
        }

        // 打开动作管理窗口
        public void OpenActionManageWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => // 确保UI操作在主线程中执行
            {
                ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 查找现有的动作管理窗口
                if (actionPageManageWindow != null) // 如果找到现有的设置窗口
                {
                    if (actionPageManageWindow.WindowState == WindowState.Minimized) // 如果设置窗口被最小化
                    {
                        actionPageManageWindow.WindowState = WindowState.Normal; // 恢复窗口
                    }
                    SetForegroundWindow(new WindowInteropHelper(actionPageManageWindow).Handle); // 将窗口置于前台
                }
                else // 如果没有找到现有的设置窗口，则创建并显示新窗口
                {
                    actionPageManageWindow = new(); // 创建设置窗口实例
                    actionPageManageWindow.Show(); // 显示设置窗口
                }
                actionPageManageWindow.Activate(); // 激活设置窗口
                this.Visibility = Visibility.Hidden; // 隐藏当前的CustomMenu窗口
            });
            this.Visibility = Visibility.Hidden; // 隐藏当前窗口
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
        public void Exit(object sender, RoutedEventArgs e)
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