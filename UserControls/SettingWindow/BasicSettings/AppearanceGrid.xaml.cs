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
                ClipHelper.SetEnableCustomClip(btn, false); // 解绑自定义裁剪事件，防止内存泄漏
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
                //LoadGlobalButtonsForPreview();
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

    public class PreviewBorderHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]: 按钮高度, values[1]: 按钮间隙
            if (values.Length >= 2 && double.TryParse(values[0]?.ToString(), out double btnHeight) && double.TryParse(values[1]?.ToString(), out double gap))
            {
                return 27 + 25 + btnHeight * 7 + gap * 5;
            }
            return 0;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// 附加属性帮助类，用于为按钮动态设置自定义裁剪（圆角）
    /// </summary>
    public static class ClipHelper
    {
        // 定义附加属性 EnableCustomClip，控制是否启用自定义裁剪
        public static readonly DependencyProperty EnableCustomClipProperty =
            DependencyProperty.RegisterAttached(
                "EnableCustomClip",
                typeof(bool),
                typeof(ClipHelper),
                new PropertyMetadata(false, OnEnableCustomClipChanged));

        // 设置附加属性方法
        public static void SetEnableCustomClip(UIElement element, bool value)
            => element.SetValue(EnableCustomClipProperty, value);

        // 获取附加属性方法
        public static bool GetEnableCustomClip(UIElement element)
            => (bool)element.GetValue(EnableCustomClipProperty);

        // 附加属性值变化时的回调
        private static void OnEnableCustomClipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button btn)
            {
                if ((bool)e.NewValue)
                {
                    btn.SizeChanged += Btn_SizeChanged; // 启用时，注册 SizeChanged 事件，并立即设置一次裁剪
                    UpdateButtonClip(btn);
                }
                else
                {
                    btn.SizeChanged -= Btn_SizeChanged; // 关闭时，移除事件并清除裁剪
                    btn.Clip = null;
                }
            }
        }

        // 按钮尺寸变化时，更新裁剪路径
        private static void Btn_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Button btn)
            {
                UpdateButtonClip(btn);
            }
        }

        /// <summary>
        /// 根据按钮名称，动态设置左下角或右下角圆角裁剪，圆角半径固定为5
        /// </summary>
        /// <param name="btn">需要裁剪的按钮</param>
        private static void UpdateButtonClip(Button btn)
        {
            double height = btn.ActualHeight; // 按钮实际高度
            double width = btn.ActualWidth;   // 按钮实际宽度
            double radius = 5;                // 固定圆角半径

            // 判断是左下角还是右下角圆角（通过按钮 Name 区分）
            if (btn.Name.Contains("LeftBottom")) // 左下角圆角裁剪
            {
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(0, 0), IsClosed = true };
                figure.Segments.Add(new LineSegment(new Point(width, 0), true));                // 上边
                figure.Segments.Add(new LineSegment(new Point(width, height), true));           // 右边
                figure.Segments.Add(new LineSegment(new Point(radius, height), true));          // 下边（右下到左下圆角起点）
                figure.Segments.Add(new ArcSegment(new Point(0, height - radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true)); // 左下角圆弧
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));                    // 左边
                geometry.Figures.Add(figure);
                btn.Clip = geometry;
            }
            else if (btn.Name.Contains("RightBottom")) // 右下角圆角裁剪
            {
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(0, 0), IsClosed = true };
                figure.Segments.Add(new LineSegment(new Point(width, 0), true));                // 上边
                figure.Segments.Add(new LineSegment(new Point(width, height - radius), true));  // 右边（右上到右下圆角起点）
                figure.Segments.Add(new ArcSegment(new Point(width - radius, height), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true)); // 右下角圆弧
                figure.Segments.Add(new LineSegment(new Point(0, height), true));               // 下边
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));                    // 左边
                geometry.Figures.Add(figure);
                btn.Clip = geometry;
            }
        }
    }
}