using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Input;
using System.Diagnostics;
using System.Reflection;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System.IO;

namespace Quicker.UserControls
{
    public partial class AboutQuickerGrid : UserControl
    {
        ActionManager actionManager = new(); // 创建动作管理器
        SettingManager settingManager; // 读取设置的管理器

        public AboutQuickerGrid(SettingWindow settingWindow)
        {
            InitializeComponent();
            settingManager = settingWindow.settingManager; // 创建设置管理器
            settingManager.LoadConventionsAsync(); // 初始化缓存数据
            VersionLabel.Content = $"版本：{settingManager.conventions.Version}"; // 加载版本信息
        }

        // 当鼠标移入事件文本框时，改变鼠标样式为手型
        private void Event_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand; // 改变鼠标样式为手型
        }

        // 当鼠标移出事件文本框时，恢复默认鼠标样式
        private void Event_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow; // 恢复默认鼠标样式
        }

        // 基础设置-关于Quicker-关于Quicker
        private void AboutQuickerButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(AboutQuickerButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(AboutQuickerButton, MainGrid); // 保存Button类型3边框设置
        }

        // 打开更新历史文件
        private void OpenUpdateHistory(object sender, MouseButtonEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集
            string resourceName = "Quicker.UpdateHistory.txt"; // 获取更新历史文件名
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) // 打开资源流
            {
                if (stream == null) return; // 资源不存在则返回
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd(); // 读取资源内容
                    string tempPath = Path.GetTempPath(); // 获取系统临时文件夹路径
                    string tempFilePath = Path.Combine(tempPath, "更新历史.txt"); // 更改临时文件名
                    File.WriteAllText(tempFilePath, content); // 将内容写入临时文件
                    System.Diagnostics.Process.Start("notepad.exe", tempFilePath); // 使用系统默认文本编辑器打开临时文件
                }
            }
        }

        // 前往图标网站www.iconfont.cn
        private void www_iconfont_cn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            actionManager.LaunchDefaultBrowser("https://www.iconfont.cn"); // 打开图标网站www.iconfont.cn
        }

        // 前往图标网站icons8.com
        private void icons8_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            actionManager.LaunchDefaultBrowser("https://icons8.com/"); // 打开图标网站icons8.com
        }

        // 前往图标网站fontawesome.com
        private void fontawesome_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            actionManager.LaunchDefaultBrowser("https://fontawesome.com/"); // 打开图标网站fontawesome.com
        }

        // 前往icon11社区图标库
        private void icon11_community_github_io_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            actionManager.LaunchDefaultBrowser("https://icon11-community.github.io/icons/"); // 前往icon11社区图标库
        }

        // BUG反馈、需求
        private void FeedBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            actionManager.LaunchDefaultBrowser("https://github.com/Anonymity3314/Quicker/issues"); // 前往Github反馈
        }

        // 基础设置-关于Quicker-隐私声明
        private void Privacy_StatementButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(Privacy_StatementButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(Privacy_StatementButton, MainGrid); // 保存Button类型3边框设置
        }

        // 控件关闭释放资源
        private void AboutQuickerGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear(); // 清理UI元素

            // 清理事件处理程序
            AboutQuickerButton.Click -= AboutQuickerButton_Click;
            Privacy_StatementButton.Click -= Privacy_StatementButton_Click;
            UpdateHistory.MouseLeftButtonDown -= OpenUpdateHistory;
            www_iconfont_cn.MouseLeftButtonDown -= www_iconfont_cn_MouseLeftButtonDown;
            icons8_com.MouseLeftButtonDown -= icons8_com_MouseLeftButtonDown;
            fontawesome_com.MouseLeftButtonDown -= fontawesome_com_MouseLeftButtonDown;
            FeedBack.MouseLeftButtonDown -= FeedBack_MouseLeftButtonDown;
            VersionLabel.MouseEnter -= Event_MouseEnter;
            VersionLabel.MouseLeave -= Event_MouseLeave;

            // 清理外部资源
            actionManager?.Dispose();
            actionManager = null;
            settingManager = null;

            VersionLabel.Content = string.Empty; // 清理文本内容
            MainGrid.Background = null;
        }
    }
}