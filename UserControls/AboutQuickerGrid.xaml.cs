using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;

namespace Quicker.UserControls
{
    public partial class AboutQuickerGrid : UserControl
    {
        SettingManager settingManager = new SettingManager();

        public AboutQuickerGrid()
        {
            InitializeComponent();
        }

        private static void SetGridVisible(Grid childrengrid, Grid fathergrid)
        {
            foreach (var grid in fathergrid.Children.OfType<Grid>())
            {
                grid.Visibility = grid == childrengrid ? Visibility.Visible : Visibility.Collapsed; // 设置Grid可见性
            }
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
            SetGridVisible(AboutQuickerButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(AboutQuickerButton, MainGrid); // 保存Button类型3边框设置
        }

        // 打开更新历史文件
        private void OpenUpdateLog(object sender, MouseButtonEventArgs e)
        {
            Process.Start("notepad.exe", "UpdateLog.txt"); // 打开更新历史文件
        }

        // 前往图标网站www.iconfont.cn
        private void www_iconfont_cn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://www.iconfont.cn"); // 打开图标网站www.iconfont.cn
        }

        // 前往图标网站icons8.com
        private void icons8_com_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://icons8.com/"); // 打开图标网站icons8.com
        }

        // 前往图标网站fontawesome.com
        private void fontawesome_com_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://fontawesome.com/"); // 打开图标网站fontawesome.com
        }

        /// <summary>
        /// 打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void OpenWebsite(string website)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = website, // 打开指定网站
                UseShellExecute = true // 使用外壳程序启动
            });
        }

        // 基础设置-关于Quicker-隐私声明
        private void Privacy_StatementButton_Click(object sender, RoutedEventArgs e)
        {
            SetGridVisible(Privacy_StatementButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(Privacy_StatementButton, MainGrid); // 保存Button类型3边框设置
        }
    }
}