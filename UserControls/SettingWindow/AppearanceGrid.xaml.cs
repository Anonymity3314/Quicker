using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Media;
using Quicker.Managers;
using System.Windows;

namespace Quicker.UserControls.SettingWindow
{
    public partial class AppearanceGrid : UserControl
    {
        private WeakReference<Quicker.Windows.MainWindows.SettingWindow> weakSettingWindow; // 弱引用设置窗口
        private SolidColorBrush _currentBrush; // 当前选中的颜色画刷
        SettingManager settingManager; // 设置管理器

        public AppearanceGrid(Quicker.Windows.MainWindows.SettingWindow settingWindow)
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

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button == null) return; // 如果按钮为null，则返回

            // 获取按钮中的SolidColorBrush
            _currentBrush = null; // 初始化brush为null
            switch (button.Name)
            {
                case "BackgroundColorButton":
                    _currentBrush = BackgroundColorBrush;
                    break; // 背景颜色
                case "ToolbarColorButton":
                    _currentBrush = ToolbarColorBrush;
                    break; // 工具栏颜色
                case "ToolbarIconColorButton":
                    _currentBrush = ToolbarIconColorBrush;
                    break; // 工具栏图标颜色
                case "ActionButtonColorButton":
                    _currentBrush = ActionButtonColorBrush;
                    break; // 动作按钮颜色
                case "ActionButtonMouseOverColorButton":
                    _currentBrush = ActionButtonMouseOverColorBrush;
                    break; // 动作按钮鼠标悬停颜色
                case "BlankButtonColorButton":
                    _currentBrush = BlankButtonColorBrush;
                    break; // 空白按钮颜色
                case "BlankButtonMouseOverColorButton":
                    _currentBrush = BlankButtonMouseOverColorBrush;
                    break; // 空白按钮鼠标悬停颜色
                case "ButtonTextColorButton":
                    _currentBrush = ButtonTextColorBrush;
                    break; // 按钮文字颜色
                case "ActionIconColorButton":
                    _currentBrush = ActionIconColorBrush;
                    break; // 动作图标颜色
                case "TriggerKeyTextColorButton":
                    _currentBrush = TriggerKeyTextColorBrush;
                    break; // 触发键文字颜色
                case "OtherIconColorButton":
                    _currentBrush = OtherIconColorBrush;
                    break; // 其他位置图标颜色
            }
            if (_currentBrush == null) return; // 如果brush为null，则返回
            
            // 使用新方法强制刷新色彩选择器
            PopupColorPicker.ResetColorControls(_currentBrush.Color);
            
            // 设置Popup的位置并打开
            ColorPickerPopup.PlacementTarget = button;
            ColorPickerPopup.IsOpen = true;
        }

        // 处理颜色选择器颜色变化事件
        private void PopupColorPicker_SelectedColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (_currentBrush != null)
            {
                _currentBrush.Color = e.NewColor; // 实时更新按钮颜色
            }
        }

        // 释放资源
        private void AppearanceGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            // 释放资源
            settingManager = null; // 释放设置管理器
            weakSettingWindow = null; // 释放弱引用设置窗口
            _currentBrush = null; // 释放当前选中的颜色画刷
        }
    }
}