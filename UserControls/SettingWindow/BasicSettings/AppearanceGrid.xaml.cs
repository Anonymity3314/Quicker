using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using Quicker.Windows.MainWindows;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using Quicker.UserControls;
using System.Globalization;
using System.Windows.Data;
using Quicker.Database;
using Quicker.Managers;
using System.Xml.Linq;
using System.Windows;
using Quicker.Models;
using System.Linq;

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
            BackgroundColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BackgroundColor)); // 设置背景颜色
            ToolbarColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarColor)); // 设置工具栏颜色
            ToolbarIconColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ToolbarIconColor)); // 设置工具栏图标颜色
            ActionButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonColor)); // 设置动作按钮颜色
            ActionButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ActionButtonMouseOverColor)); // 设置动作按钮鼠标悬停颜色
            BlankButtonColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonColor)); // 设置空白按钮颜色
            BlankButtonMouseOverColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.BlankButtonMouseOverColor)); // 设置空白按钮鼠标悬停颜色
            TextColorButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settingManager.appearanceConditions.ButtonTextColor)); // 设置按钮文字颜色
            ButtonTextColorBrush = TextColorButton.Background as SolidColorBrush; // 同步ViewModel属性
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
            FontWeightComboBox.SelectedIndex = settingManager.appearanceConditions.FontWeight; // 设置字体粗细
            ApplyGlobalFontFamily(); // 应用全局字体
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
            LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
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
            var buttons = GlobalGrid.Children.OfType<Button>().ToList(); // 获取UniformGrid中的所有Button
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
                    RefreshButtonDisplay(btn, buttonData); // 刷新按钮显示内容
                }
                else
                {
                    btn.Tag = null; // 没有数据则Tag置空
                    btn.Background = BlankButtonColorButton.Background; // 设置为空白按钮颜色
                    btn.Content = null; // 清空内容或设置为空白样式
                }
            }
        }

        /// <summary>
        /// 刷新按钮显示内容
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        public void RefreshButtonDisplay(Button button, ButtonData buttonInformation)
        {
            if (buttonInformation == null || buttonInformation.Location == null)
            {
                ResetButtonDisplay(button); // 重置按钮显示
                return; // 直接返回
            }

            button.Tag = buttonInformation; // 设置按钮标签
            var grid = CreateButtonGrid(buttonInformation); // 创建按钮网格
            button.Content = grid; // 设置按钮内容
        }

        /// <summary>
        /// 重置按钮显示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        private void ResetButtonDisplay(Button button)
        {
            button.Content = null; // 清空按钮内容
            button.Tag = null; // 清空标签
            button.Background = BlankButtonColorButton.Background; // 设置背景色
        }

        /// <summary>
        /// 创建按钮网格
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="maxWidth"> 最大宽度 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private Grid CreateButtonGrid(ButtonData buttonInformation)
        {
            Grid grid = new(); // 创建网格
            if (!string.IsNullOrEmpty(buttonInformation.ImagePath)) // 如果图像路径不为空
            {
                AddImageToGrid(grid, buttonInformation); // 添加图像到网格
            }

            if (!string.IsNullOrEmpty(buttonInformation.Title)) // 如果标题不为空
            {
                AddTitleToGrid(grid, buttonInformation); // 添加标题到网格
            }

            return grid; // 返回网格
        }

        /// <summary>
        /// 添加图像到网格
        /// </summary>
        /// <param name="grid"> 目标网格 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        private void AddImageToGrid(Grid grid, ButtonData buttonInformation)
        {
            try
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                Image image = LoadActionIcon(buttonInformation); // 加载动作图标
                grid.Children.Add(image); // 添加图像到网格
                Grid.SetRow(image, 0); // 设置图像行
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show($"图标加载失败：动作{buttonInformation.Title}的图标被移动或删除", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 向指定Grid中添加标题TextBlock，并根据设置自动调整字号或省略号，控制可见性和颜色
        /// </summary>
        /// <param name="grid"> 目标网格 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        private void AddTitleToGrid(Grid grid, ButtonData buttonInformation)
        {
            double buttonSize = double.Parse(ButtonSizeTextBox.Text); // 获取按钮大小
            double borderWidth = BorderWidthSlider.Value; // 获取边框宽度
            double maxWidth = buttonSize - borderWidth * 2; // 计算最大可用宽度

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
            TextBlock textBlock = LoadActionTitle(buttonInformation); // 创建标题TextBlock
            textBlock.Visibility = HideActionNameAfterIconCheckBox != null && HideActionNameAfterIconCheckBox.IsChecked == true
                ? Visibility.Collapsed : Visibility.Visible; // 根据设置控制可见性

            if (AutoHideTitleBarCheckBox != null && AutoHideTitleBarCheckBox.IsChecked == true)
            {
                ShrinkTextBlockFontToFit(textBlock, buttonSize, borderWidth); // 自动缩小字号以适应宽度
            }
            else
            {
                AutoEllipsisTextBlock(textBlock, (int)maxWidth); // 超出宽度时自动省略号
            }
            grid.Children.Add(textBlock); // 添加TextBlock到网格
            Grid.SetRow(textBlock, 1); // 设置TextBlock所在行
        }

        /// <summary>
        /// 加载动作图标
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <returns> 图像对象 </returns>
        private Image LoadActionIcon(ButtonData buttonInformation)
        {
            double buttonSize = double.Parse(ButtonSizeTextBox.Text); // 获取按钮大小
            double imageSize = buttonSize / 2.0; // 图片大小
            Image image = new()
            {
                Width = imageSize,
                Height = imageSize,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Source = new BitmapImage(new Uri(buttonInformation.ImagePath))
            };

            image.Effect = ShowActionIconShadowCheckBox.IsChecked == true
                ? new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.5
                }
                : null; // 判断是否需要阴影

            return image;
        }

        /// <summary>
        /// 加载动作名称TextBlock，并绑定文字颜色
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <returns> 文本块对象 </returns>
        private TextBlock LoadActionTitle(ButtonData buttonInformation)
        {
          
            var binding = new System.Windows.Data.Binding
            {
                Path = new PropertyPath("ButtonTextColorBrush"), // 绑定到ViewModel属性
                Source = this
            };  // 创建绑定，绑定到按钮文字颜色Brush
           
            var textBlock = new TextBlock
            {
                Text = buttonInformation.Title, // 设置文本内容
                TextAlignment = TextAlignment.Center, // 文本居中
                HorizontalAlignment = HorizontalAlignment.Stretch // 水平拉伸填满
            }; // 创建TextBlock用于显示按钮标题

           
            textBlock.SetBinding(TextBlock.ForegroundProperty, binding); // 绑定前景色为按钮文字颜色
            if (double.TryParse(FontSizeTextBox.Text, out double fontSize)) // 用滑块的值设置初始字体大小
                textBlock.FontSize = fontSize; // 滑块值
            else
                textBlock.FontSize = 12; // 默认字号

            return textBlock; // 返回TextBlock对象，由AddTitleToGrid后续决定是否自动缩小字号
        }

        /// <summary>
        /// 动态调整TextBlock的字体大小以适应最大宽度
        /// </summary>
        /// <param name="textBlock">指定的TextBlock</param>
        /// <param name="maxWidth">最大宽度</param>
        public void AutoEllipsisTextBlock(TextBlock textBlock, int maxWidth)
        {
            if (string.IsNullOrEmpty(textBlock.Text)) return; // 如果文本为空，直接返回
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
            double textWidth = textBlock.DesiredSize.Width; // 获取文本宽度           
            if (textWidth <= maxWidth) return; // 如果文本宽度小于等于最大宽度，直接返回

            string originalText = textBlock.Text; // 获取原始文本
            string ellipsis = "..."; // 设置省略号
            string truncatedText = originalText; // 初始化截断文本
            while (true) // 循环直到文本宽度小于等于最大宽度
            {
                truncatedText = truncatedText.Substring(0, truncatedText.Length - 1); // 截断文本
                string newText = truncatedText + ellipsis; // 添加省略号
                textBlock.Text = newText; // 更新 TextBlock 的文本
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
                double newWidth = textBlock.DesiredSize.Width; // 获取新文本宽度
                if (newWidth <= maxWidth) break; // 如果新文本宽度小于等于最大宽度，退出循环
            }
            textBlock.Text = truncatedText + ellipsis; // 更新 TextBlock 的文本
        }

        /// <summary>
        /// 缩小TextBlock字号直到文本宽度小于等于按钮大小 - 按钮边框 * 2
        /// </summary>
        /// <param name="textBlock"> 要调整的TextBlock </param>
        /// <param name="buttonSize"> 按钮大小 </param>
        /// <param name="borderWidth"> 按钮边框宽度 </param>
        public void ShrinkTextBlockFontToFit(TextBlock textBlock, double buttonSize, double borderWidth)
        {
            if (textBlock == null || string.IsNullOrEmpty(textBlock.Text)) return; // 防止空引用或空文本
            double maxWidth = buttonSize - borderWidth * 2; // 计算最大允许宽度
            double fontSize = double.Parse(FontSizeTextBox.Text); // 获取初始字号
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量当前文本块宽度
            while (textBlock.DesiredSize.Width >= maxWidth)
            {
                fontSize -= 0.1; // 递减字号
                textBlock.FontSize = fontSize; // 应用新字号
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 重新测量宽度
            }
        }

        // 鼠标移入 Button 切换 Background，并根据设置放大按钮
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
            LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
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

            // 检测预览设置变化
            bool previewChanged = settingManager.IsPreviewSettingChanged();
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
                LoadGlobalButtonsForPreview(); // 刷新预览区按钮内容和样式
                SettingDatabase.UpdateAppearance(settingManager.appearanceConditions); // 更新数据库
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
            bool isDefault1 = string.IsNullOrEmpty(font1) || font1 == "(系统默认)";
            bool isDefault2 = string.IsNullOrEmpty(font2) || font2 == "(系统默认)";
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

        // 定义附加属性 ClipToBounds，控制是否裁剪超出边界的子元素
        public static readonly DependencyProperty ClipToBoundsProperty =
            DependencyProperty.RegisterAttached(
                "ClipToBounds",
                typeof(bool),
                typeof(ClipHelper),
                new PropertyMetadata(false, OnClipToBoundsChanged));

        // 设置附加属性方法
        public static void SetEnableCustomClip(UIElement element, bool value)
            => element.SetValue(EnableCustomClipProperty, value);

        // 获取附加属性方法
        public static bool GetEnableCustomClip(UIElement element)
            => (bool)element.GetValue(EnableCustomClipProperty);

        // 设置裁剪边界属性
        public static void SetClipToBounds(UIElement element, bool value)
            => element.SetValue(ClipToBoundsProperty, value);

        // 获取裁剪边界属性
        public static bool GetClipToBounds(UIElement element)
            => (bool)element.GetValue(ClipToBoundsProperty);

        // 附加属性值变化时的回调
        private static void OnEnableCustomClipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button btn)
            {
                if ((bool)e.NewValue)
                {
                    UpdateButtonClip(btn);
                }
                else
                {
                    btn.Clip = null;
                }
            }
        }

        // 裁剪边界属性变化时的回调
        private static void OnClipToBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                if ((bool)e.NewValue)
                {
                    border.SizeChanged += Border_SizeChanged; // 启用时，注册 SizeChanged 事件
                    UpdateBorderClip(border);
                }
                else
                {
                    border.SizeChanged -= Border_SizeChanged; // 关闭时，移除事件并清除裁剪
                    border.Clip = null;
                }
            }
        }

        // Border 尺寸变化时，更新裁剪路径
        private static void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Border border)
            {
                UpdateBorderClip(border);
            }
        }

        /// <summary>
        /// 为 Border 设置裁剪路径，使其子元素超出边界时被裁剪
        /// </summary>
        /// <param name="border">需要裁剪的 Border</param>
        public static void UpdateBorderClip(Border border)
        {
            double width = border.ActualWidth;
            double height = border.ActualHeight;
            double cornerRadius = border.CornerRadius.TopLeft; // 直接用Border的CornerRadius
            if (cornerRadius > 0)
            {
                // 创建圆角矩形裁剪路径
                var geometry = new RectangleGeometry(new Rect(0, 0, width, height), cornerRadius, cornerRadius);
                border.Clip = geometry;
            }
            else
            {
                // 创建矩形裁剪路径
                var geometry = new RectangleGeometry(new Rect(0, 0, width, height));
                border.Clip = geometry;
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

    /// <summary>
    /// 毛玻璃效果转换器：根据模糊下拉框选项返回BlurEffect或null
    /// </summary>
    public class BlurEffectConverter : IMultiValueConverter
    {
        /// <summary>
        /// 将模糊下拉框选项转换为BlurEffect对象
        /// </summary>
        /// <param name="values">[0]为SelectedIndex</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域</param>
        /// <returns>BlurEffect或null</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = (int)values[0];
            if (selectedIndex == 1) // 毛玻璃
            {
                return new BlurEffect { Radius = 15 };
            }
            return null;
        }
        /// <summary>
        /// 不支持反向转换
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="targetTypes">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域</param>
        /// <returns>null</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 字体粗细转换器：根据SelectedIndex返回对应FontWeight
    /// </summary>
    public class FontWeightConverter : IValueConverter
    {
        // WPF标准16种字体粗细枚举，顺序需与ComboBox一致
        private static readonly FontWeight[] FontWeightList = new FontWeight[]
        {
            FontWeights.Thin,        // 0
            FontWeights.ExtraLight,  // 1
            FontWeights.UltraLight,  // 2
            FontWeights.Light,       // 3
            FontWeights.Normal,      // 4
            FontWeights.Regular,     // 5
            FontWeights.Medium,      // 6
            FontWeights.DemiBold,    // 7
            FontWeights.SemiBold,    // 8
            FontWeights.Bold,        // 9
            FontWeights.ExtraBold,   // 10
            FontWeights.UltraBold,   // 11
            FontWeights.Black,       // 12
            FontWeights.Heavy,       // 13
            FontWeights.ExtraBlack,  // 14
            FontWeights.UltraBlack   // 15
        };

        /// <summary>
        /// 将SelectedIndex或字符串转换为FontWeight
        /// </summary>
        /// <param name="value">SelectedIndex(int)或ComboBoxItem或string</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域</param>
        /// <returns>对应FontWeight</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value 可能是 ComboBoxItem、string、int
            int index = -1;
            if (value is int i)
                index = i;
            else if (value is ComboBoxItem item && item.Parent is ComboBox combo)
                index = combo.Items.IndexOf(item);
            else if (value is string s)
            {
                // 尝试用字符串查找索引
                for (int j = 0; j < FontWeightList.Length; j++)
                {
                    if (FontWeightList[j].ToString().Equals(s, StringComparison.OrdinalIgnoreCase))
                    {
                        index = j;
                        break;
                    }
                }
            }
            if (index >= 0 && index < FontWeightList.Length)
                return FontWeightList[index];
            return FontWeights.Normal;
        }

        /// <summary>
        /// 将FontWeight反向转换为索引
        /// </summary>
        /// <param name="value">FontWeight</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域</param>
        /// <returns>索引（int）</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 反向转换为索引
            if (value is FontWeight fw)
            {
                for (int i = 0; i < FontWeightList.Length; i++)
                {
                    if (FontWeightList[i] == fw)
                        return i;
                }
            }
            return 4; // Normal
        }
    }
}