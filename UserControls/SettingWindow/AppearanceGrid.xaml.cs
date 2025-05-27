using System.Windows.Controls;
using Quicker.Windows.Forms;
using Quicker.UserControls;
using Quicker.Managers;
using System.Windows;

namespace Quicker.UserControls
{
    public partial class AppearanceGrid : UserControl
    {
        private WeakReference<SettingWindow> weakSettingWindow; // 弱引用设置窗口
        SettingManager settingManager; // 设置管理器

        public AppearanceGrid(SettingWindow settingWindow)
        {
            InitializeComponent(); // 初始化xaml界面
            settingManager = settingWindow._settingManager; // 初始化设置管理器
            weakSettingWindow = new(settingWindow); // 保存设置窗口
            InitializeAsync(); // 异步初始化
        }

        // 异步初始化方法
        private async void InitializeAsync()
        {
            await LoadSettingsAsync(); // 异步加载设置
        }

        // 异步加载设置
        private async Task LoadSettingsAsync()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //AutoHideTitleBarCheckBox.IsChecked = settingManager.settingsCache.AutoHideTitleBar; // 设置自动隐藏标题栏复选框
                //ShowActionButtonMouseOverCheckBox.IsChecked = settingManager.settingsCache.ShowActionButtonMouseOver; // 设置显示动作按钮鼠标悬停复选框
                //HideActionNameAfterIconCheckBox.IsChecked = settingManager.settingsCache.HideActionNameAfterIcon; // 设置隐藏动作名称后面的图标复选框
                //ShowActionIconShadowCheckBox.IsChecked = settingManager.settingsCache.ShowActionIconShadow; // 设置显示动作图标阴影复选框
            });
        }

        // 同步滚动条数据
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ScrollViewer == null) return; // 防止空引用
            AppearanceScrollBar.Maximum = ScrollViewer.ScrollableHeight; // 设置最大值
            AppearanceScrollBar.ViewportSize = ScrollViewer.ViewportHeight; // 设置视口大小
            AppearanceScrollBar.Value = ScrollViewer.VerticalOffset; // 设置当前值
        }
        private void AppearanceScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToVerticalOffset(AppearanceScrollBar.Value); // 设置滚动条值
        }

        // 鼠标移入界面显示滚动条
        private void SettingGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceScrollBar.Visibility = Visibility.Visible; // 显示滚动条
        }

        // 鼠标移出界面隐藏滚动条
        private void SettingGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceScrollBar.Visibility = Visibility.Collapsed; // 隐藏滚动条
        }

        // 复选框点击事件
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender); // 调用设置管理器的复选框点击事件
        }
    }
}