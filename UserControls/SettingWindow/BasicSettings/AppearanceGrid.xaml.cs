using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.UserControls;
using System.Globalization;
using System.Windows.Data;
using Quicker.Database;
using Quicker.Managers;
using System.Xml.Linq;
using System.Windows;

namespace Quicker.UserControls.SettingWindow.BasicSettings
{
    public partial class AppearanceGrid : UserControl
    {
        private WeakReference<Quicker.Windows.MainWindows.SettingWindow> weakSettingWindow; // 弱引用设置窗口
        private SolidColorBrush _currentBrush; // 当前选中的颜色画刷
        SettingManager settingManager; // 设置管理器
        private readonly ButtonManager buttonManager = new(); // 添加按钮管理器
        private readonly ButtonDatabase db2 = new(); // 添加按钮数据库

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
            settingManager.LoadAppearanceAsync(); // 初始化缓存数据
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 按钮
                ButtonSizeSlider.Value = settingManager.appearanceConditions.ButtonSize; // 设置按钮大小
                ButtonGapSlider.Value = settingManager.appearanceConditions.ButtonGap; // 设置按钮间距
                BorderWidthSlider.Value = settingManager.appearanceConditions.BorderWidth; // 设置边框宽度
                ButtonCornerRadiusSlider.Value = settingManager.appearanceConditions.ButtonCornerRadius; // 设置按钮圆角半径

                // 颜色
                var backgroundColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BackgroundColor));
                var toolbarColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarColor));
                var toolbarIconColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarIconColor));
                var actionButtonColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonColor));
                var actionButtonMouseOverColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonMouseOverColor));
                var blankButtonColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonColor));
                var blankButtonMouseOverColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonMouseOverColor));
                var buttonTextColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ButtonTextColor));
                var actionIconColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionIconColor));
                var triggerKeyTextColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.TriggerKeyTextColor));
                var otherIconColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.OtherIconColor));

                BackgroundColorButton.Background = backgroundColorBrush; // 设置背景颜色
                ToolbarColorButton.Background = toolbarColorBrush; // 设置工具栏颜色
                ToolbarIconColorButton.Background = toolbarIconColorBrush; // 设置工具栏图标颜色
                ActionButtonColorButton.Background = actionButtonColorBrush; // 设置动作按钮颜色
                ActionButtonMouseOverColorButton.Background = actionButtonMouseOverColorBrush; // 设置动作按钮鼠标悬停颜色
                BlankButtonColorButton.Background = blankButtonColorBrush; // 设置空白按钮颜色
                BlankButtonMouseOverColorButton.Background = blankButtonMouseOverColorBrush; // 设置空白按钮鼠标悬停颜色
                ButtonTextColorButton.Background = buttonTextColorBrush; // 设置按钮文字颜色
                ActionIconColorButton.Background = actionIconColorBrush; // 设置动作图标颜色
                TriggerKeyTextColorButton.Background = triggerKeyTextColorBrush; // 设置触发键文字颜色
                OtherIconColorButton.Background = otherIconColorBrush; // 设置其他位置图标颜色

                // 字体
                FontSizeComboBox1.SelectedIndex = settingManager.appearanceConditions.Font1; // 设置字体1
                FontSizeComboBox2.SelectedIndex = settingManager.appearanceConditions.Font2; // 设置字体2
                FontSizeSlider.Value = settingManager.appearanceConditions.FontSize; // 设置字体大小
                FontWeightTextBox.Text = settingManager.appearanceConditions.FontWeight.ToString(); // 设置字体粗细

                // 背景图片
                BackgroundImagePathTextBox.Text = settingManager.appearanceConditions.BackgroundImagePath; // 设置背景图片路径
                BackgroundImageOpacitySlider.Value = settingManager.appearanceConditions.BackgroundImageOpacity; // 设置背景图片不透明度

                // 模糊与圆角
                BlurComboBox.SelectedIndex = settingManager.appearanceConditions.Blur; // 设置模糊模式
                Win11CornerRadiusComboBox.SelectedIndex = settingManager.appearanceConditions.Win11CornerRadius; // 设置Win11圆角模式

                // 选项
                AutoHideTitleBarCheckBox.IsChecked = settingManager.appearanceConditions.AutoHideTitleBar; // 设置自动隐藏标题栏
                ShowActionButtonMouseOverCheckBox.IsChecked = settingManager.appearanceConditions.ShowActionButtonMouseOver; // 设置鼠标悬停显示动作按钮
                HideActionNameAfterIconCheckBox.IsChecked = settingManager.appearanceConditions.HideActionNameAfterIcon; // 设置隐藏动作名称
                ShowActionIconShadowCheckBox.IsChecked = settingManager.appearanceConditions.ShowActionIconShadow; // 设置显示动作图标阴影

                EnablePreviewCheckBox.IsChecked = settingManager.appearanceConditions.EnablePreview; // 设置显示设置应用效果
                EnablePreviewCheckBox_Click(null, null); // 切换预览可见性
            }); // 异步加载设置
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

            _currentBrush = button.Background as SolidColorBrush; // 获取按钮的Background作为当前选中的颜色画刷
            if (_currentBrush == null) return; // 如果brush为null，则返回

            PopupColorPicker.ResetColorControls(_currentBrush.Color); // 重置色彩选择器
            PopupColorPicker.SelectedColor = _currentBrush.Color; // 同步SelectedColor依赖属性

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
            foreach (Button btn in ViewGlobalUniformGrid.Children.OfType<Button>())
            {
                btn.MouseEnter -= PreviewButton_MouseEnter;
                btn.MouseLeave -= PreviewButton_MouseLeave;
            }
            ViewGlobalUniformGrid.Children.Clear(); // 解绑全局预览按钮的事件

            settingManager = null; // 释放设置管理器
            weakSettingWindow = null; // 释放弱引用设置窗口
            _currentBrush = null; // 释放当前选中的颜色画刷
        }

        // 点击按钮打开预设菜单
        private void PresetStyleButton_Click(object sender, RoutedEventArgs e)
        {
            PresetStylePopup.IsOpen = true; // 打开预设样式弹出窗口
        }

        // 为预览加载全局按钮
        private void LoadGlobalButtonsForPreview()
        {
            ViewGlobalUniformGrid.Children.Clear(); // 清空现有的预览按钮
            var globalButtons = db2.GetPagesOfButtons("Global", 0); // 获取全局按钮数据（第一页，即pageIndex=0）
            double buttonSize = ButtonSizeSlider.Value; // 获取当前按钮大小
            for (int row = 0; row < 3; row++) // 为每个按钮位置创建预览按钮
            {
                for (int col = 0; col < 4; col++)
                {
                    int buttonIndex = 0 * 100 + (row + 1) * 10 + (col + 1); // 按钮索引
                    string buttonName = $"Global{buttonIndex}"; // 按钮名称
                    Button previewButton = new Button
                    {
                        Style = FindResource("PreviewButtonStyle") as Style,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        MinHeight = buttonSize,
                        MaxHeight = buttonSize,
                        MinWidth = buttonSize,
                        MaxWidth = buttonSize,
                        Height = buttonSize,
                        Width = buttonSize,
                        Name = buttonName
                    };

                    var buttonData = globalButtons.FirstOrDefault(b => b.ButtonID == buttonIndex);
                    if (buttonData != null)
                    {
                        previewButton.Tag = buttonData;
                        previewButton.Background = ActionButtonColorButton.Background;
                        buttonManager.RefreshButtonDisplay(previewButton, buttonData, (int)buttonSize, false);
                    }
                    else
                    {
                        previewButton.Tag = null;
                        previewButton.Background = BlankButtonColorButton.Background;
                    }

                    // 悬浮事件
                    previewButton.MouseEnter += PreviewButton_MouseEnter;
                    previewButton.MouseLeave += PreviewButton_MouseLeave;

                    ViewGlobalUniformGrid.Children.Add(previewButton);
                }
            }
        }

        // 点击预设样式按钮
        private void EnablePreviewCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ViewPreviewBorder.Visibility = (Visibility)(EnablePreviewCheckBox.IsChecked == true ? 0 : 2);
            if (EnablePreviewCheckBox.IsChecked == true) // 当开启预览时，加载全局按钮
            {
                LoadGlobalButtonsForPreview();
            }
        }

        // 鼠标移入 Button 切换 Background
        private void PreviewButton_MouseEnter(object sender, EventArgs e)
        {
            var btn = sender as Button; // 获取按钮
            btn.Background = btn.Tag == null
                ? BlankButtonMouseOverColorButton.Background
                : ActionButtonMouseOverColorButton.Background; // 绑定颜色选择
        }

        // 鼠标移入 Button 还原 Background
        private void PreviewButton_MouseLeave(object sender, EventArgs e)
        {
            var btn = sender as Button; // 获取按钮
            btn.Background = btn.Tag == null
                ? BlankButtonColorButton.Background
                : ActionButtonColorButton.Background; // 绑定颜色选择
        }
    }

    // 内联定义 ThicknessConverter
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return new Thickness(d);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return thickness.Left;
            }
            return 0.0;
        }
    }

    public class GridHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]: 按钮大小, values[1]: 按钮间隙, parameter: 行数
            if (values.Length >= 2 && double.TryParse(values[0]?.ToString(), out double btnSize) && double.TryParse(values[1]?.ToString(), out double gap))
            {
                int rows = 3; // 默认3行
                if (parameter != null && int.TryParse(parameter.ToString(), out int pRows))
                    rows = pRows;
                return btnSize * rows + gap * (rows - 1);
            }
            return 0;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GridWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]: 按钮大小, values[1]: 按钮间隙, parameter: 列数
            if (values.Length >= 2 && double.TryParse(values[0]?.ToString(), out double btnSize) && double.TryParse(values[1]?.ToString(), out double gap))
            {
                int cols = 4; // 默认4列
                if (parameter != null && int.TryParse(parameter.ToString(), out int pCols))
                    cols = pCols;
                return btnSize * cols + gap * (cols - 1);
            }
            return 0;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}