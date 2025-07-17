using SixLabors.ImageSharp.Formats.Png.Chunks;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Quicker.Windows.ToolWindows;
using System.Windows.Threading;
using Quicker.Models.Settings;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Globalization;
using SixLabors.ImageSharp;
using System.Windows.Data;
using Quicker.Database;
using Quicker.Managers;
using System.Text.Json;
using Quicker.Helpers;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.UserControls.SettingWindow.BasicSettings
{
    public partial class AppearanceGrid : UserControl, INotifyPropertyChanged // 实现INotifyPropertyChanged接口，支持属性变更通知
    {
        private WeakReference<Quicker.Windows.MainWindows.SettingWindow> weakSettingWindow; // 弱引用设置窗口
        private readonly ButtonManager buttonManager = new(); // 添加按钮管理器
        private DispatcherTimer _settingsChangeTimer; // 设置变化检测计时器
        private readonly ButtonDatabase db2 = new(); // 添加按钮数据库
        private SolidColorBrush _currentBrush; // 当前选中的颜色画刷
        private Button _currentColorButton; // 记录当前按钮
        SettingManager settingManager; // 设置管理器

        // INotifyPropertyChanged接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // 预览区圆角属性，绑定到XAML的CornerRadius
        private CornerRadius _previewCornerRadius = new CornerRadius(5); // 默认圆角为5
        public CornerRadius PreviewCornerRadius
        {
            get => _previewCornerRadius;
            set
            {
                if (_previewCornerRadius != value)
                {
                    _previewCornerRadius = value;
                    OnPropertyChanged(nameof(PreviewCornerRadius)); // 通知界面属性变更
                }
            }
        }

        // 标题栏圆角属性，绑定到XAML
        private CornerRadius _titleBarCornerRadius = new CornerRadius(5, 5, 0, 0); // 默认5,5,0,0
        public CornerRadius TitleBarCornerRadius
        {
            get => _titleBarCornerRadius;
            set
            {
                if (_titleBarCornerRadius != value)
                {
                    _titleBarCornerRadius = value;
                    OnPropertyChanged(nameof(TitleBarCornerRadius));
                }
            }
        }

        // 按钮文字颜色Brush属性，供XAML和代码绑定
        private SolidColorBrush _buttonTextColorBrush = new SolidColorBrush(Colors.Black);
        public SolidColorBrush ButtonTextColorBrush
        {
            get => _buttonTextColorBrush;
            set
            {
                if (_buttonTextColorBrush != value)
                {
                    _buttonTextColorBrush = value;
                    OnPropertyChanged(nameof(ButtonTextColorBrush));
                }
            }
        }

        public AppearanceGrid(Quicker.Windows.MainWindows.SettingWindow settingWindow)
        {
            InitializeComponent(); // 初始化xaml界面
            settingManager = settingWindow._settingManager; // 初始化设置管理器
            weakSettingWindow = new(settingWindow); // 保存设置窗口
            this.DataContext = this; // 设置自身为DataContext，便于属性绑定
            InitializeAsync(); // 异步初始化
            InitializeSettingsChangeTimer(); // 初始化设置变化检测计时器
            InitializeFontComboBoxes(); // 初始化字体下拉框
        }

        // 初始化设置变化检测计时器
        private void InitializeSettingsChangeTimer()
        {
            _settingsChangeTimer = new DispatcherTimer();
            _settingsChangeTimer.Interval = TimeSpan.FromMilliseconds(500); // 每0.5秒检测一次
            _settingsChangeTimer.Tick += SettingsChangeTimer_Tick;
            _settingsChangeTimer.Start();
        }

        // 计时器事件处理
        private void SettingsChangeTimer_Tick(object sender, EventArgs e)
        {
            if (settingManager != null)
            {
                bool hasChanges = settingManager.IsMainAppearanceSettingsChanged();
                ResetAppearanceButton.Visibility = hasChanges ? Visibility.Visible : Visibility.Collapsed;
            }
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
                ApplyButtonSettings(); // 应用按钮相关设置
                ApplyColorSettings(); // 应用颜色相关设置
                ApplyFontSettings(); // 应用字体相关设置
                ApplyBackgroundImageSettings(); // 应用背景图片设置
                ApplyBlurAndCornerSettings(); // 应用模糊与圆角设置
                ApplyOptionSettings(); // 应用选项设置

                // 初始化重置按钮为隐藏状态
                ResetAppearanceButton.Visibility = Visibility.Collapsed;
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
            BackgroundColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BackgroundColor));
            BorderColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BorderColor));
            ToolbarColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarColor));
            ToolbarIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarIconColor));
            ActionButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonColor));
            ActionButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonMouseOverColor));
            BlankButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonColor));
            BlankButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonMouseOverColor));
            TextColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ButtonTextColor));
            ButtonTextColorBrush = TextColorButton.Background as SolidColorBrush;
            ActionIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionIconColor));
            TriggerKeyTextColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.TriggerKeyTextColor));
            OtherIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.OtherIconColor));
        }

        // 字体相关设置
        private void ApplyFontSettings()
        {
            FontSizeComboBox1.SelectedIndex = settingManager.appearanceConditions.Font1; // 设置字体1
            FontSizeComboBox2.SelectedIndex = settingManager.appearanceConditions.Font2; // 设置字体2
            FontSizeSlider.Value = settingManager.appearanceConditions.FontSize; // 设置字体大小
            FontWeightComboBox.SelectedIndex = settingManager.appearanceConditions.FontWeight; // 设置字体粗细
            ApplyGlobalFontFamily(); // 应用全局字体
        }

        // 背景图片相关设置
        private void ApplyBackgroundImageSettings()
        {
            BackgroundImagePath = settingManager.appearanceConditions.BackgroundImagePath; // 设置背景图片路径
            BackgroundImageOpacitySlider.Value = settingManager.appearanceConditions.BackgroundImageOpacity; // 设置背景图片不透明度
        }

        // 模糊与圆角相关设置
        private void ApplyBlurAndCornerSettings()
        {
            BlurComboBox.SelectedIndex = settingManager.appearanceConditions.Blur; // 设置模糊模式
            Win11CornerRadiusComboBox.SelectedIndex = settingManager.appearanceConditions.Win11CornerRadius; // 设置Win11圆角模式
            UpdatePreviewCornerRadiusByComboBox(); // 同步预览区圆角显示
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
                    settingManager.appearanceConditions.AutoHideTitleBar = value; // 自动缩小动作名称文字
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
                    break;
                default:
                    return; // 其它CheckBox不处理
            }
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
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
                UpdateAppearanceColorProperty(_currentBrush, e.NewColor);
                if (_currentColorButton != null && _currentColorButton.Name == "TextColorButton") // 如果当前是文字颜色按钮，顺便同步ViewModel属性
                {
                    ButtonTextColorBrush.Color = e.NewColor;
                    settingManager.appearanceConditions.ButtonTextColor = e.NewColor.ToString(); // 同步更新数据库字段
                }
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions);
            }
        }

        // 按钮名到属性名的映射字典
        private static readonly Dictionary<string, string> ColorButtonToProperty = new()
        {
            { "BackgroundColorButton", "BackgroundColor" },
            { "BorderColorButton", "BorderColor" },
            { "ToolbarColorButton", "ToolbarColor" },
            { "ToolbarIconColorButton", "ToolbarIconColor" },
            { "ActionButtonColorButton", "ActionButtonColor" },
            { "ActionButtonMouseOverColorButton", "ActionButtonMouseOverColor" },
            { "BlankButtonColorButton", "BlankButtonColor" },
            { "BlankButtonMouseOverColorButton", "BlankButtonMouseOverColor" },
            { "TextColorButton", "ButtonTextColor" },
            { "ActionIconColorButton", "ActionIconColor" },
            { "TriggerKeyTextColorButton", "TriggerKeyTextColor" },
            { "OtherIconColorButton", "OtherIconColor" }
        };

        /// <summary>
        /// 根据当前颜色按钮的Name，自动设置appearanceConditions的对应属性
        /// </summary>
        /// <param name="brush">当前选中的颜色画刷</param>
        /// <param name="color">新颜色</param>
        private void UpdateAppearanceColorProperty(SolidColorBrush brush, Color color)
        {
            if (_currentColorButton == null) return;
            if (ColorButtonToProperty.TryGetValue(_currentColorButton.Name, out string propertyName))
            {
                var prop = settingManager.appearanceConditions.GetType().GetProperty(propertyName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(settingManager.appearanceConditions, color.ToString());
                }
            }
        }

        // 释放资源
        private void AppearanceGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            // 停止并清理计时器
            if (_settingsChangeTimer != null)
            {
                _settingsChangeTimer.Stop();
                _settingsChangeTimer.Tick -= SettingsChangeTimer_Tick;
                _settingsChangeTimer = null;
            }

            GlobalGrid.Children.Clear(); // 清空按钮

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
            var buttons = GlobalGrid.Children.OfType<Button>().ToList(); // 获取Grid中的所有Button
            for (int i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];
                string buttonName = btn.Name;
                int buttonIndex = int.Parse(btn.Name.Replace("Global","")); // 获取按钮索引

                // 查找对应的按钮数据
                var buttonData = globalButtons.FirstOrDefault(b => b.ButtonID == buttonIndex);
                if (buttonData != null)
                {
                    btn.Tag = buttonData; // 绑定按钮数据到Tag
                    btn.Background = ActionButtonColorButton.Background; // 设置为动作按钮颜色
                    buttonManager.RefreshButtonDisplay(btn, buttonData, 0); // 刷新按钮显示内容
                }
                else
                {
                    btn.Tag = null; // 没有数据则Tag置空
                    btn.Background = BlankButtonColorButton.Background; // 设置为空白按钮颜色
                    btn.Content = null; // 清空内容或设置为空白样式
                }
            }
        }

        private void PreviewButton_MouseEnter(object sender, EventArgs e)
        {
            var btn = sender as Button; // 获取按钮
            btn.Background = btn.Tag == null
                ? BlankButtonMouseOverColorButton.Background
                : ActionButtonMouseOverColorButton.Background; // 绑定颜色选择

            // 判断是否需要放大按钮
            if (ShowActionButtonMouseOverCheckBox != null &&
                ShowActionButtonMouseOverCheckBox.IsChecked == true &&
                btn.Tag != null)
            {
                btn.RenderTransformOrigin = new Point(0.5, 0.5); // 设置缩放中心为按钮中心
                btn.RenderTransform = new ScaleTransform(1.05, 1.05); // 放大1.05倍
            }
        }

        // 鼠标移出 Button 还原 Background，并还原按钮大小
        private void PreviewButton_MouseLeave(object sender, EventArgs e)
        {
            var btn = sender as Button; // 获取按钮
            btn.Background = btn.Tag == null
                ? BlankButtonColorButton.Background
                : ActionButtonColorButton.Background; // 绑定颜色选择

            // 判断是否需要还原按钮大小
            if (ShowActionButtonMouseOverCheckBox != null && ShowActionButtonMouseOverCheckBox.IsChecked == true)
            {
                btn.RenderTransform = null; // 还原为默认大小
            }
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
                    break;
                case "ButtonGapSlider":
                    settingManager.appearanceConditions.ButtonGap = slider.Value; // 设置按钮间距
                    break;
                case "BorderWidthSlider":
                    settingManager.appearanceConditions.BorderWidth = slider.Value; // 设置边框宽度
                    break;
                case "ButtonCornerRadiusSlider":
                    settingManager.appearanceConditions.ButtonCornerRadius = slider.Value; // 设置按钮圆角
                    break;
                case "FontSizeSlider":
                    settingManager.appearanceConditions.FontSize = slider.Value; // 设置字体大小
                    LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
                    break;
                case "BackgroundImageOpacitySlider":
                    settingManager.appearanceConditions.BackgroundImageOpacity = slider.Value; // 设置背景图片不透明度
                    break;
                default:
                    return;
            }
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
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
                        UpdatePreviewCornerRadiusByComboBox(); // 变更预览区圆角
                        break;
                    case "FontSizeComboBox1":
                        settingManager.appearanceConditions.Font1 = comboBox.SelectedIndex; // 设置字体1
                        break;
                    case "FontSizeComboBox2":
                        settingManager.appearanceConditions.Font2 = comboBox.SelectedIndex; // 设置字体2
                        break;
                    case "FontWeightComboBox":
                        settingManager.appearanceConditions.FontWeight = comboBox.SelectedIndex; // 设置字体粗细
                        break;
                    default:
                        return; // 其它ComboBox不处理
                }
                ApplyGlobalFontFamily(); // 每次字体选择后应用全局字体
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            }
        }

        /// <summary>
        /// 根据Win11圆角模式ComboBox的选项，动态设置预览区圆角
        /// </summary>
        private void UpdatePreviewCornerRadiusByComboBox()
        {
            double previewRadius;
            switch (Win11CornerRadiusComboBox.SelectedIndex) // 0: 默认 1: 无 2: 圆角 3: 小圆角
            {
                case 1: // 无
                    previewRadius = 0;
                    break;
                case 3: // 小圆角
                    previewRadius = 3;
                    break;
                default: // 默认、圆角
                    previewRadius = 5;
                    break;
            }
            PreviewCornerRadius = new CornerRadius(previewRadius); // 预览区圆角
            TitleBarCornerRadius = new CornerRadius(previewRadius, previewRadius, 0, 0); // 标题栏只上面有圆角

            // 延迟到UI刷新后再裁剪
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ClipHelper.UpdateBorderClip(ViewPreviewBorder);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // "开启预览"复选框点击事件
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

        // 重置外观设置按钮点击事件
        private void ResetAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAppearanceButton.Visibility = Visibility.Collapsed; // 隐藏按钮
            if (settingManager != null)
            {
                // 保存当前预览设置
                bool currentPreviewSetting = settingManager.appearanceConditions.EnablePreview;

                // 恢复原始外观设置
                settingManager.RestoreOriginalAppearanceSettings();

                // 恢复预览设置为当前值（不重置预览设置）
                settingManager.appearanceConditions.EnablePreview = currentPreviewSetting;

                // 重新应用设置到界面
                ApplyButtonSettings(); // 应用按钮相关设置
                ApplyColorSettings(); // 应用颜色相关设置
                ApplyFontSettings(); // 应用字体相关设置
                ApplyBackgroundImageSettings(); // 应用背景图片设置
                ApplyBlurAndCornerSettings(); // 应用模糊与圆角设置
                ApplyOptionSettings(); // 应用选项设置

                // 刷新预览区
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新数据库
                LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
            }
        }

        // 初始化字体下拉框
        private void InitializeFontComboBoxes()
        {
            var fontFamilies = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();
            fontFamilies.Add("(系统默认)"); // 在最后插入一个空项
            FontSizeComboBox1.ItemsSource = fontFamilies;
            FontSizeComboBox2.ItemsSource = fontFamilies;
        }

        // 应用全局字体方法
        private void ApplyGlobalFontFamily()
        {
            // 获取当前选择的字体名
            string font1 = FontSizeComboBox1.SelectedItem as string;
            string font2 = FontSizeComboBox2.SelectedItem as string;

            // 判断是否为“系统默认”
            bool isDefault1 = font1 == "(系统默认)";
            bool isDefault2 = font2 == "(系统默认)";
            FontFamily fontFamily;
            if (!isDefault1 && !isDefault2)
            {
                fontFamily = new FontFamily($"{font1}, {font2}");
            }
            else if (!isDefault1)
            {
                fontFamily = new FontFamily(font1);
            }
            else if (!isDefault2)
            {
                fontFamily = new FontFamily(font2);
            }
            else
            {
                fontFamily = new FontFamily("微软雅黑"); // 系统默认改为微软雅黑
            }

            Application.Current.Resources["GlobalFontFamily"] = fontFamily;
        }

        // 点击“选择...”按钮
        private void BackgroundImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "图片文件|*.jpg;*.png;*.bmp",
                Title = "选择背景图片"
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                double aspectRatio = ViewPreviewBorder.ActualWidth / ViewPreviewBorder.ActualHeight; // 计算宽高比
                var imageCropWindow = new ImageCropWindow(dialog.FileName, aspectRatio, ViewPreviewBorder.CornerRadius);
                // 定义事件处理方法，便于后续解绑，防止内存泄漏
                Action<object, string> cropCompletedHandler = null; // 裁剪完成事件处理器
                EventHandler closedHandler = null; // 窗口关闭事件处理器

                // 裁剪完成事件：设置图片路径，启用按钮，并解绑事件
                cropCompletedHandler = (s, croppedPath) =>
                {
                    if (!string.IsNullOrEmpty(croppedPath))
                    {
                        BackgroundImagePathTextBox.Text = croppedPath; // 设置新图片路径
                    }
                    BackgroundImagePathButton.IsEnabled = true; // 启用选择按钮
                    // 解绑事件，防止内存泄漏
                    imageCropWindow.CropCompleted -= cropCompletedHandler;
                    imageCropWindow.Closed -= closedHandler;
                };

                // 窗口关闭事件：无论是否裁剪，均启用按钮并解绑事件
                closedHandler = (s, args) =>
                {
                    BackgroundImagePathButton.IsEnabled = true; // 启用选择按钮
                    // 解绑事件，防止内存泄漏
                    imageCropWindow.CropCompleted -= cropCompletedHandler;
                    imageCropWindow.Closed -= closedHandler;
                };

                // 绑定事件，确保窗口关闭或裁剪完成后都能正确处理
                imageCropWindow.CropCompleted += cropCompletedHandler;
                imageCropWindow.Closed += closedHandler;
                BackgroundImagePathButton.IsEnabled = false;
                imageCropWindow.Show();
            }
        }

        // 设置背景
        private void UpdateBackgroundImage()
        {
            var path = BackgroundImagePathTextBox.Text;
            var opacity = BackgroundImageOpacitySlider.Value;
        }

        private string _backgroundImagePath; // 背景图片路径
        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set
            {
                if (_backgroundImagePath != value)
                {
                    _backgroundImagePath = value;
                    OnPropertyChanged(nameof(BackgroundImagePath));
                }
            }
        }

        private double _backgroundImageOpacity = 1.0; // 背景图片不透明度
        public double BackgroundImageOpacity
        {
            get => _backgroundImageOpacity;
            set
            {
                if (_backgroundImageOpacity != value)
                {
                    _backgroundImageOpacity = value;
                    OnPropertyChanged(nameof(BackgroundImageOpacity));
                }
            }
        }

        // 文本框失去焦点时，保存背景图片路径
        private void BackgroundImagePathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新外观设置到数据库
            settingManager.appearanceConditions.BackgroundImagePath = BackgroundImagePathTextBox.Text; // 保存路径到缓存
        }

        // 点击按钮分享外观
        private void ShareSaveAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. 获取当前外观设置对象
            var appearance = settingManager.appearanceConditions;
            // 2. 序列化为 JSON 字符串
            string json = JsonSerializer.Serialize(appearance);
            // 3. 获取作为分享载体的 PNG 图片路径（优先用用户自定义背景，否则用内置图片）
            string inputPngPath = GetAppearanceCarrierImagePath();
            inputPngPath = EnsureTrueColorPng(inputPngPath); // 保证是32位真彩色
            // 4. 获取输出路径（自动创建保存文件夹，文件名带时间戳）
            string outputPngPath = GetShareAppearanceOutputPath();
            // 5. 写入 PNG 文件并嵌入 JSON 数据
            WriteAppearanceToPng(inputPngPath, outputPngPath, json);
            // 6. 显示保存成功的 Toast 提示
            ShowToast("外观保存成功！", "Success");
            // 7. 打开资源管理器并选中刚保存的 PNG 文件
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputPngPath}\"");
        }

        /// <summary>
        /// 获取外观分享载体图片路径（优先用用户自定义背景，否则用内置图片）
        /// </summary>
        /// <returns>载体图片路径</returns>
        private string GetAppearanceCarrierImagePath()
        {
            // 如果用户设置了背景图片且文件存在，则直接用该图片
            if (!string.IsNullOrEmpty(BackgroundImagePathTextBox.Text) && File.Exists(BackgroundImagePathTextBox.Text))
            {
                return BackgroundImagePathTextBox.Text;
            }
            else // 否则用内置资源图片，先保存到临时文件
            {
                var resourceUri = new Uri("pack://application:,,,/Resources/Images/Quicker1.png"); // 内置图片资源路径
                var streamInfo = Application.GetResourceStream(resourceUri); // 获取资源流
                string tempPath = Path.GetTempFileName() + ".png"; // 临时文件路径
                using (var fileStream = File.Create(tempPath)) // 创建临时文件
                {
                    streamInfo.Stream.CopyTo(fileStream); // 复制资源流到临时文件
                }
                return tempPath;
            }
        }

        /// <summary>
        /// 获取分享 PNG 的输出路径（自动创建文件夹，文件名带时间戳）
        /// </summary>
        /// <returns>输出路径</returns>
        private string GetShareAppearanceOutputPath()
        {
            string baseDir = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\SharedAppearance"; // 目标文件夹
            if (!Directory.Exists(baseDir)) // 如果文件夹不存在则自动创建
            {
                Directory.CreateDirectory(baseDir);
            }
            string fileName = $"Quicker外观_{DateTime.Now:yyyyMMdd_HHmmss}.png"; // 文件名格式：Quicker外观_年月日_时分秒.png
            return Path.Combine(baseDir, fileName);
        }

        /// <summary>
        /// 将 JSON 数据写入 PNG 图片的 tEXt 块，实现外观参数嵌入（ImageSharp 版本）
        /// </summary>
        /// <param name="inputPngPath">输入PNG路径（载体图片）</param>
        /// <param name="outputPngPath">输出PNG路径（保存路径）</param>
        /// <param name="json">要嵌入的 JSON 数据</param>
        private void WriteAppearanceToPng(string inputPngPath, string outputPngPath, string json)
        {
            // 用 ImageSharp 读取图片，写入 tEXt 块
            using (var image = SixLabors.ImageSharp.Image.Load(inputPngPath))
            {
                var pngMeta = image.Metadata.GetPngMetadata();
                string base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                // 先移除同名块，避免重复
                var toRemove = pngMeta.TextData.Where(t => t.Keyword == "QuickerAppearance").ToList();
                foreach (var item in toRemove)
                {
                    pngMeta.TextData.Remove(item);
                }
                pngMeta.TextData.Add(new PngTextData("QuickerAppearance", base64, null, null));
                image.Save(outputPngPath);
            }
        }

        /// <summary>
        /// 显示 Toast 提示
        /// </summary>
        /// <param name="message">提示内容</param>
        /// <param name="type">提示类型 (Success, Error, Info)</param>
        private void ShowToast(string message, string type)
        {
            using var toast = new ToastManager();
            toast.Show(message, type);
        }

        /// <summary>
        /// 确保输入的 PNG 图片为 32 位真彩色格式（BGRA32），如果不是则自动转换并返回新文件路径。
        /// </summary>
        /// <param name="inputPath">原始 PNG 图片路径（可以是索引色或其他格式）</param>
        /// <returns>转换为 32 位真彩色的 PNG 文件路径（临时文件）</returns>
        private string EnsureTrueColorPng(string inputPath)
        {
            // 用 WPF BitmapImage 读取
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(inputPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            // 转换为 32 位 BGRA
            var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);

            // 保存为临时 PNG
            string tempPath = Path.GetTempFileName() + ".png";
            using (var fileStream = File.Create(tempPath))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(converted));
                encoder.Save(fileStream);
            }
            return tempPath;
        }

        /// <summary>
        /// 从 PNG 文件导入外观数据，返回 Appearance 对象，失败返回 null
        /// </summary>
        /// <param name="pngPath">PNG 文件路径</param>
        /// <returns>Appearance 对象或 null</returns>
        private Appearance ImportAppearanceFromPng(string pngPath)
        {
            string json = null;
            using (var image = SixLabors.ImageSharp.Image.Load(pngPath))
            {
                var pngMeta = image.Metadata.GetPngMetadata();
                var textData = pngMeta.TextData.FirstOrDefault(t => t.Keyword == "QuickerAppearance");
                string text = textData.Equals(default(PngTextData)) ? null : textData.Value;
                if (!string.IsNullOrEmpty(text))
                {
                    json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(text));
                }
            }
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonSerializer.Deserialize<Appearance>(json);
        }

        // 导入外观设置按钮点击事件
        private void ImportAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedPngPath = ShowSelectAppearancePngDialog(); // 选择图片
                if (string.IsNullOrEmpty(selectedPngPath))
                    return;

                var appearance = ImportAppearanceFromPng(selectedPngPath); // 导入外观数据
                if (appearance == null)
                {
                    ShowToast("未检测到外观数据！", "Error");
                    return;
                }

                HandleImportedAppearanceBackgroundImage(appearance, selectedPngPath); // 处理背景图片
                ApplyImportedAppearance(appearance); // 应用外观参数并刷新界面
                ShowToast("导入成功！", "Success");
            }
            catch
            {
                ShowToast("导入失败！", "Error");
            }
        }

        /// <summary>
        /// 弹出文件选择对话框，返回用户选择的PNG文件路径，取消则返回null
        /// </summary>
        /// <returns>PNG文件路径</returns>
        private string ShowSelectAppearancePngDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PNG图片|*.png",
                Title = "选择外观分享图片"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// 处理导入Appearance对象的背景图片，将其去除文本块后复制到本地并更新路径
        /// </summary>
        /// <param name="appearance">要处理的Appearance对象</param>
        /// <param name="srcPngPath">源PNG文件路径（载体图片）</param>
        private void HandleImportedAppearanceBackgroundImage(Appearance appearance, string srcPngPath)
        {
            if (!string.IsNullOrEmpty(appearance.BackgroundImagePath))
            {
                string bgDir = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\BackgroundImages"; // 背景图片文件夹路径
                if (!Directory.Exists(bgDir)) // 如果文件夹不存在则自动创建
                    Directory.CreateDirectory(bgDir);
                string hash = DateTime.Now.Ticks.ToString("x"); // 随机哈希值作为文件名
                string newBgPath = Path.Combine(bgDir, $"bg_{hash}.png"); // 新背景图片路径
                RemoveTextChunkAndCopy(srcPngPath, newBgPath); // 去除文本块并复制图片
                appearance.BackgroundImagePath = newBgPath; // 更新路径
            }
        }

        /// <summary>
        /// 应用导入的Appearance对象到设置，并刷新界面和预览
        /// </summary>
        /// <param name="appearance">要应用的Appearance对象</param>
        private void ApplyImportedAppearance(Appearance appearance)
        {
            SettingDatabase.UpdateAppearance(appearance);
            settingManager.appearanceConditions = appearance;
            ApplyButtonSettings();
            ApplyColorSettings();
            ApplyFontSettings();
            ApplyBackgroundImageSettings();
            ApplyBlurAndCornerSettings();
            ApplyOptionSettings();
            LoadGlobalButtonsForPreview();
            // 保证导入后预览区可见
            EnablePreviewCheckBox.IsChecked = true;
            settingManager.appearanceConditions.EnablePreview = true;
            EnablePreviewCheckBox_Click(null, null);
        }

        /// <summary>
        /// 去除 PNG 图片的 tEXt/iTXt/zTXt 元数据并复制到指定路径
        /// 用于处理导入Appearance对象的背景图片，将其去除文本块后复制到本地
        /// </summary>
        /// <param name="srcPath"> 源 PNG 文件路径 </param>
        /// <param name="destPath"> 目标 PNG 文件路径 </param>
        private void RemoveTextChunkAndCopy(string srcPath, string destPath)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(srcPath)) // 用 ImageSharp 读取图片像素并去除所有 tEXt/iTXt/zTXt 元数据
            {
                var pngMeta = image.Metadata.GetPngMetadata();
                pngMeta.TextData.Clear(); // 移除所有文本块
                image.Save(destPath);
            }
        }

        // 点击“已分享或保存的外观”按钮打开外观分享文件夹
        private void SharedSavedAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\SharedAppearance"; // 外观分享文件夹路径
            if (!Directory.Exists(folderPath)) // 如果文件夹不存在，则自动创建
            {
                Directory.CreateDirectory(folderPath);
            }
            System.Diagnostics.Process.Start("explorer.exe", folderPath); // 使用资源管理器打开该文件夹
        }
    }
}