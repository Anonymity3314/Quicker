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
        private readonly ButtonManager buttonManager = new(); // 添加按钮管理器
        private readonly ButtonDatabase db2 = new(); // 添加按钮数据库
        private SolidColorBrush _currentBrush; // 当前选中的颜色画刷
        private Button _currentColorButton; // 记录当前按钮
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
            settingManager.LoadAppearanceAsync(); // 初始化缓存数据
            Application.Current.Dispatcher.Invoke(() =>
            {
                ApplyButtonSettings();
                ApplyColorSettings();
                ApplyFontSettings();
                ApplyBackgroundImageSettings();
                ApplyBlurAndCornerSettings();
                ApplyOptionSettings();
            }); // 异步加载设置
        }

        // 按钮相关设置
        private void ApplyButtonSettings()
        {
            ButtonSizeSlider.Value = settingManager.appearanceConditions.ButtonSize; // 设置按钮大小
            ButtonGapSlider.Value = settingManager.appearanceConditions.ButtonGap; // 设置按钮间距
            BorderWidthSlider.Value = settingManager.appearanceConditions.BorderWidth; // 设置边框宽度
            ButtonCornerRadiusSlider.Value = settingManager.appearanceConditions.ButtonCornerRadius; // 设置按钮圆角半径
        }

        // 颜色相关设置
        private void ApplyColorSettings()
        {
            BackgroundColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BackgroundColor)); // 设置背景颜色
            ToolbarColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarColor)); // 设置工具栏颜色
            ToolbarIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarIconColor)); // 设置工具栏图标颜色
            ActionButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonColor)); // 设置动作按钮颜色
            ActionButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonMouseOverColor)); // 设置动作按钮鼠标悬停颜色
            BlankButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonColor)); // 设置空白按钮颜色
            BlankButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonMouseOverColor)); // 设置空白按钮鼠标悬停颜色
            ButtonTextColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ButtonTextColor)); // 设置按钮文字颜色
            ActionIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionIconColor)); // 设置动作图标颜色
            TriggerKeyTextColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.TriggerKeyTextColor)); // 设置触发键文字颜色
            OtherIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.OtherIconColor)); // 设置其他位置图标颜色
        }

        // 字体相关设置
        private void ApplyFontSettings()
        {
            FontSizeComboBox1.SelectedIndex = settingManager.appearanceConditions.Font1; // 设置字体1
            FontSizeComboBox2.SelectedIndex = settingManager.appearanceConditions.Font2; // 设置字体2
            FontSizeSlider.Value = settingManager.appearanceConditions.FontSize; // 设置字体大小
            FontWeightTextBox.Text = settingManager.appearanceConditions.FontWeight.ToString(); // 设置字体粗细
        }

        // 背景图片相关设置
        private void ApplyBackgroundImageSettings()
        {
            BackgroundImagePathTextBox.Text = settingManager.appearanceConditions.BackgroundImagePath; // 设置背景图片路径
            BackgroundImageOpacitySlider.Value = settingManager.appearanceConditions.BackgroundImageOpacity; // 设置背景图片不透明度
        }

        // 模糊与圆角相关设置
        private void ApplyBlurAndCornerSettings()
        {
            BlurComboBox.SelectedIndex = settingManager.appearanceConditions.Blur; // 设置模糊模式
            Win11CornerRadiusComboBox.SelectedIndex = settingManager.appearanceConditions.Win11CornerRadius; // 设置Win11圆角模式
        }

        // 选项相关设置
        private void ApplyOptionSettings()
        {
            AutoHideTitleBarCheckBox.IsChecked = settingManager.appearanceConditions.AutoHideTitleBar; // 设置自动隐藏标题栏
            ShowActionButtonMouseOverCheckBox.IsChecked = settingManager.appearanceConditions.ShowActionButtonMouseOver; // 设置鼠标悬停显示动作按钮
            HideActionNameAfterIconCheckBox.IsChecked = settingManager.appearanceConditions.HideActionNameAfterIcon; // 设置隐藏动作名称
            ShowActionIconShadowCheckBox.IsChecked = settingManager.appearanceConditions.ShowActionIconShadow; // 设置显示动作图标阴影

            EnablePreviewCheckBox.IsChecked = settingManager.appearanceConditions.EnablePreview; // 设置显示设置应用效果
            EnablePreviewCheckBox_Click(null, null); // 切换预览可见性
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
            var checkBox = sender as CheckBox; // 获取CheckBox控件
            if (checkBox == null) return;
            bool value = checkBox.IsChecked == true; // 获取当前勾选状态
            switch (checkBox.Name) // 根据控件名称区分处理
            {
                case "AutoHideTitleBarCheckBox":
                    settingManager.appearanceConditions.AutoHideTitleBar = value; // 自动隐藏标题栏
                    break;
                case "ShowActionButtonMouseOverCheckBox":
                    settingManager.appearanceConditions.ShowActionButtonMouseOver = value; // 鼠标悬浮放大动作按钮
                    break;
                case "HideActionNameAfterIconCheckBox":
                    settingManager.appearanceConditions.HideActionNameAfterIcon = value; // 设置动作图标后隐藏动作名称
                    break;
                case "ShowActionIconShadowCheckBox":
                    settingManager.appearanceConditions.ShowActionIconShadow = value; // 动作图标显示阴影
                    break;
                case "EnablePreviewCheckBox":
                    settingManager.appearanceConditions.EnablePreview = value; // 开启/关闭预览
                    // 预览区显示/隐藏逻辑
                    ViewPreviewBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    if (value)
                        LoadGlobalButtonsForPreview(); // 加载全局按钮到预览区
                    break;
                default:
                    return; // 其它CheckBox不处理
            }
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
        }

        // 颜色按钮点击事件，弹出颜色选择器并初始化为当前颜色
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button == null) return; // 如果按钮为null，则返回
            _currentColorButton = button; // 记录当前按钮
            _currentBrush = button.Background as SolidColorBrush; // 获取按钮的Background作为当前选中的颜色画刷
            if (_currentBrush == null) return; // 如果brush为null，则返回

            PopupColorPicker.ResetColorControls(_currentBrush.Color); // 重置色彩选择器为当前颜色
            PopupColorPicker.SelectedColor = _currentBrush.Color; // 同步SelectedColor依赖属性

            // 设置Popup的位置并打开
            ColorPickerPopup.PlacementTarget = button; // 设置弹窗目标为当前按钮
            ColorPickerPopup.IsOpen = true; // 打开颜色选择器弹窗
        }

        // 处理颜色选择器颜色变化事件
        private void PopupColorPicker_SelectedColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (_currentBrush != null)
            {
                _currentBrush.Color = e.NewColor;
                // 假设你有个方法可以根据_currentBrush找到对应的属性
                UpdateAppearanceColorProperty(_currentBrush, e.NewColor);
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions);
            }
        }

        /// <summary>
        /// 根据当前颜色按钮的Name，自动设置appearanceConditions的对应属性
        /// </summary>
        /// <param name="brush">当前选中的颜色画刷</param>
        /// <param name="color">新颜色</param>
        private void UpdateAppearanceColorProperty(SolidColorBrush brush, Color color)
        {
            if (_currentColorButton == null) return; // 防止空引用
            string propertyName = _currentColorButton.Name.Replace("Button", ""); // 通过按钮名去掉"Button"后缀，得到属性名
            var prop = settingManager.appearanceConditions.GetType().GetProperty(propertyName); // 反射获取属性
            if (prop != null && prop.CanWrite) // 如果属性存在且可写，则赋值为新颜色
            {
                prop.SetValue(settingManager.appearanceConditions, color.ToString()); // 设置对应属性
            }
        }

        // 释放资源
        private void AppearanceGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in ViewGlobalUniformGrid.Children.OfType<Button>())
            {
                ClipHelper.SetEnableCustomClip(btn, false); // 解绑自定义裁剪事件，防止内存泄漏
            }
            ViewGlobalUniformGrid.Children.Clear(); // 清空按钮

            settingManager = null; // 释放设置管理器
            weakSettingWindow = null; // 释放弱引用设置窗口
            _currentBrush = null; // 释放当前选中的颜色画刷
            _currentColorButton = null; // 释放当前颜色按钮引用
        }

        // 点击按钮打开预设菜单
        private void PresetStyleButton_Click(object sender, RoutedEventArgs e)
        {
            PresetStylePopup.IsOpen = true; // 打开预设样式弹出窗口
        }

        // 为预览加载全局按钮
        private void LoadGlobalButtonsForPreview()
        {
            var globalButtons = db2.GetPagesOfButtons("Global", 0); // 获取全局按钮数据
            var buttons = ViewGlobalUniformGrid.Children.OfType<Button>().ToList(); // 获取UniformGrid中的所有Button
            for (int i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];
                int buttonIndex = 0 * 100 + (i / 4 + 1) * 10 + (i % 4 + 1); // 计算按钮索引（按原有规则）
                string buttonName = $"Global{buttonIndex}";

                // 查找对应的按钮数据
                var buttonData = globalButtons.FirstOrDefault(b => b.ButtonID == buttonIndex);
                if (buttonData != null)
                {
                    btn.Tag = buttonData; // 绑定按钮数据到Tag
                    btn.Background = ActionButtonColorButton.Background; // 设置为动作按钮颜色
                    buttonManager.RefreshButtonDisplay(btn, buttonData, (int)ButtonSizeSlider.Value, false); // 刷新按钮显示内容
                }
                else
                {
                    btn.Tag = null; // 没有数据则Tag置空
                    btn.Background = BlankButtonColorButton.Background; // 设置为空白按钮颜色
                    btn.Content = null; // 清空内容或设置为空白样式
                }
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

        // 滑块值改变事件，统一处理所有相关Slider
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider; // 获取Slider控件
            if (slider == null || settingManager == null) return; // 防止空引用
            switch (slider.Name)
            {
                case "ButtonSizeSlider":
                    settingManager.appearanceConditions.ButtonSize = slider.Value; // 设置按钮大小
                    LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
                    break;
                case "ButtonGapSlider":
                    settingManager.appearanceConditions.ButtonGap = slider.Value; // 设置按钮间距
                    LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
                    break;
                case "BorderWidthSlider":
                    settingManager.appearanceConditions.BorderWidth = slider.Value; // 设置边框宽度
                    break;
                case "ButtonCornerRadiusSlider":
                    settingManager.appearanceConditions.ButtonCornerRadius = slider.Value; // 设置按钮圆角
                    break;
                case "FontSizeSlider":
                    settingManager.appearanceConditions.FontSize = slider.Value; // 设置字体大小
                    break;
                case "BackgroundImageOpacitySlider":
                    settingManager.appearanceConditions.BackgroundImageOpacity = slider.Value; // 设置背景图片不透明度
                    break;
                default:
                    return;
            }
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
        }

        // 字体粗细文本框内容改变事件
        private void FontWeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(FontWeightTextBox.Text, out double fw))
                settingManager.appearanceConditions.FontWeight = fw; // 设置字体粗细
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
        }

        // 下拉框选择改变事件
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox) // 判断事件源是否为ComboBox
            {
                switch (comboBox.Name) // 根据ComboBox的Name区分处理
                {
                    case "BlurComboBox":
                        settingManager.appearanceConditions.Blur = comboBox.SelectedIndex; // 设置模糊模式
                        break;
                    case "Win11CornerRadiusComboBox":
                        settingManager.appearanceConditions.Win11CornerRadius = comboBox.SelectedIndex; // 设置Win11圆角模式
                        break;
                    case "FontSizeComboBox1":
                        settingManager.appearanceConditions.Font1 = comboBox.SelectedIndex; // 设置字体1
                        break;
                    case "FontSizeComboBox2":
                        settingManager.appearanceConditions.Font2 = comboBox.SelectedIndex; // 设置字体2
                        break;
                    default:
                        return; // 其它ComboBox不处理
                }
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            }
        }

        // “开启预览”复选框点击事件
        private void EnablePreviewCheckBox_Click(object sender, RoutedEventArgs e)
        {
            settingManager.appearanceConditions.EnablePreview = EnablePreviewCheckBox.IsChecked == true; // 同步复选框状态到缓存
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            ViewPreviewBorder.Visibility = (Visibility)(EnablePreviewCheckBox.IsChecked == true ? 0 : 2); // 切换预览区可见性
            if (EnablePreviewCheckBox.IsChecked == true) // 如果开启预览
            {
                LoadGlobalButtonsForPreview(); // 加载全局按钮到预览区
            }
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