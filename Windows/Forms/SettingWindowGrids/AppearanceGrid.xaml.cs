using System.Windows.Controls;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Forms.SettingWindowGrids
{
    public partial class AppearanceGrid : UserControl
    {
        SettingManager settingManager;

        public AppearanceGrid()
        {
            settingManager = new SettingManager();
            InitializeComponent();
            InitializeScrollBar();
        }

        // 初始化滚动条
        private void InitializeScrollBar()
        {
            AppearanceScrollBar.Maximum = ScrollViewer.ScrollableHeight; // 设置最大值
            AppearanceScrollBar.ViewportSize = ScrollViewer.ViewportHeight; // 设置视口大小
            AppearanceScrollBar.Value = ScrollViewer.VerticalOffset; // 设置当前值
        }

        // 同步滚动条数据
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ScrollViewer == null) return;
            AppearanceScrollBar.Maximum = ScrollViewer.ScrollableHeight; // 设置最大值
            AppearanceScrollBar.ViewportSize = ScrollViewer.ViewportHeight; // 设置视口大小
            AppearanceScrollBar.Value = ScrollViewer.VerticalOffset; // 设置当前值
        }
        private void AppearanceScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToVerticalOffset(AppearanceScrollBar.Value);
        }

        // 鼠标移入界面显示滚动条
        private void SettingGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceScrollBar.Visibility = Visibility.Visible;
        }

        // 鼠标移出界面隐藏滚动条
        private void SettingGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceScrollBar.Visibility = Visibility.Collapsed;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender);
        }
    }
}