using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Database;
using Microsoft.Win32;
using System.Windows;

namespace Quicker.Windows
{
    public partial class SettingWindow : Window
    {
        private const string DefaultButtonColor = "#FFE0E0E0"; // 默认按钮颜色
        private const string SelectedButtonColor = "#FFF4F4F4"; // 选中按钮颜色

        private List<string> ShortcutKeys = new List<string>(); // 保存快捷键
        private readonly SettingDatabase db1; // 设置数据库
        private SettingsCache settingsCache; // 缓存对象
        private double currentSessionTime; // 总使用时长
        private double totalUsageTime; // 总使用时长
        private DispatcherTimer timer; // 定时器

        public SettingWindow()
        {
            db1 = new SettingDatabase(); // 创建设置数据库
            InitializeComponent(); // 初始化窗口
            InitializeWindow(); // 初始化窗口
        }

        // 设置StackPanel可见性
        private static void SetStackPanelVisibility(StackPanel childrenstackpanel, Grid fathergrid)
        {
            foreach (var stackpanel in fathergrid.Children.OfType<StackPanel>())
            {
                stackpanel.Visibility = Visibility.Hidden;
            } // 重置所有StackPanel的可见性
            childrenstackpanel.Visibility = Visibility.Visible; // 设置指定StackPanel的可见性
        }

        // 设置Grid可见性
        private static void SetGridVisible(Grid childrengrid, Grid fathergrid)
        {
            foreach (var grid in fathergrid.Children.OfType<Grid>())
            {
                grid.Visibility = Visibility.Collapsed;
            } // 重置所有Grid的可见性
            childrengrid.Visibility = Visibility.Visible; // 设置指定Grid的可见性
        }

        // 初始化窗口
        private async void InitializeWindow()
        {
            SetStackPanelVisibility(BasicSettingStackPanel, MenuGrid); // 设置默认显示的StackPanel
            UpdateButtonStyle2(Convention, BasicSettingStackPanel);
            SetGridVisible(ConventionGrid, ResultGrid); // 设置默认显示的Grid
            
            await LoadUsageTimeAsync(); // 异步加载使用时长           
            await LoadSettingsAsync(); // 异步加载常规设置信息
        }

        // 异步加载使用时长
        private async Task LoadUsageTimeAsync()
        {
            DateTime currentTime = DateTime.Now;
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息            
            totalUsageTime = Conventions.TotalUsageTime + (currentTime - App.RecordedTime).TotalSeconds; // 加载总使用时长
            currentSessionTime = (currentTime - App.StartTime).TotalSeconds; // 更新当次应用使用时长
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // 设置定时器间隔为1秒
            }; // 创建定时器
            timer.Tick += Timer_Tick; // 定时器每秒更新使用时长
            timer.Start(); // 启动定时器
            Application.Current.Dispatcher.Invoke(() => // 更新界面显示
            {
                CurrentUsingTimeTextBlock.Text = TimeSpan.FromSeconds(currentSessionTime).ToString(@"hh\:mm\:ss"); // 当次应用使用时长
                TotalUsageTimeTextBlock.Text = TimeSpan.FromSeconds(totalUsageTime).ToString(@"hh\:mm\:ss"); // 总使用时长
            });
        }

        // 异步加载常规设置信息
        private async Task LoadSettingsAsync()
        {
            // 模拟异步操作
            await Task.Run(() =>
            {
                var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
                var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取弹出面板设置信息
                settingsCache = new SettingsCache
                {
                    AutoStart = Conventions.AutoStart,
                    ShowNotification = Conventions.ShowNotification,
                    ShowAddImage = Conventions.ShowAddImage,
                    HideTooltip = Conventions.HideTooltip,
                    LongPressThreshold = Conventions.LongPressThreshold,
                    MouseMovePixels = Conventions.MouseMovePixels,
                    LoopPageFlipping = Conventions.LoopPageFlipping,
                    OpenMainWindowByMiddleMouseClick = OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick,
                    OpenMainWindowByX1MouseClick = OpenMainWindowConditions.OpenMainWindowByX1MouseClick,
                    OpenMainWindowByX2MouseClick = OpenMainWindowConditions.OpenMainWindowByX2MouseClick,
                    OpenMainWindowByCtrl_MiddleMouseClick = OpenMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick,
                    OpenMainWindowByCtrl_RightMouseClick = OpenMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick,
                    OpenMainWindowByMiddleMouseClickLonger = OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger,
                    OpenMainWindowByRightMouseClickLonger = OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger,
                    OpenMainWindowByRightMouseClick_Move = OpenMainWindowConditions.OpenMainWindowByRightMouseClick_Move,
                    OpenMainWindowByCtrl = OpenMainWindowConditions.OpenMainWindowByCtrl,
                    WindowStartupLocation = OpenMainWindowConditions.WindowStartupLocation
                }; // 加载设置数据到缓存
            });
        }

        // 定时器每秒更新使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            currentSessionTime++; // 更新当次应用使用时长
            totalUsageTime++; // 更新总使用时长
            Application.Current.Dispatcher.Invoke(() => // 更新界面显示
            {
                CurrentUsingTimeTextBlock.Text = TimeSpan.FromSeconds(currentSessionTime).ToString(@"hh\:mm\:ss"); // 当次应用使用时长
                TotalUsageTimeTextBlock.Text = TimeSpan.FromSeconds(totalUsageTime).ToString(@"hh\:mm\:ss"); // 总使用时长
            });
        }

        // 加载常规设置信息
        private void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
            Application.Current.Dispatcher.Invoke(() =>
            {
                AutoStartCheckBox.IsChecked = Conventions.AutoStart; // 加载开机自启动设置
                ShowNotificationCheckBox.IsChecked = Conventions.ShowNotification; // 加载显示启动完成提示设置
                ShowAddImageCheckBox.IsChecked = Conventions.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
                HideTooltipCheckBox.IsChecked = Conventions.HideTooltip; // 加载隐藏提示框设置
                LongPressThresholdTextBox.Text = Conventions.LongPressThreshold.ToString(); // 加载长按阈值设置
                MouseMovePixelsTextBox.Text = Conventions.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
                LoopPageFlippingCheckBox.IsChecked = Conventions.LoopPageFlipping; // 加载循环翻页设置
            });
        }

        /// <summary>
        /// 更新按钮类型1颜色
        /// </summary>
        /// <param name="clickedButton">点击的按钮</param>
        /// <param name="buttonPanel">按钮面板</param>
        private static void UpdateButtonStyle1(Button clickedButton, Panel buttonPanel)
        {
            foreach (var button in buttonPanel.Children.OfType<Button>())
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor)); // 设置默认颜色
            }
            clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor)); // 设置选中颜色
        }

        // 设置Button类型2颜色&&字体粗细
        private static void UpdateButtonStyle2(Button clickedButton, Panel buttonPanel)
        {
            foreach (var button in buttonPanel.Children.OfType<Button>())
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                button.FontWeight = FontWeights.Normal;
            }// 重置所有按钮的颜色&&字体粗细
            if (clickedButton != null)
            {
                clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
                clickedButton.FontWeight = FontWeights.Bold;
            }// 设置当前点击的按钮颜色&&加粗字体
        }

        // 设置Button类型3边框
        private static void UpdateButtonStyle3(Button clickedButton, Grid buttonPanelGrid)
        {
            foreach (var button in buttonPanelGrid.Children.OfType<Button>())
            {
                button.BorderThickness = new Thickness(0);
            }// 重置所有按钮的边框
            if (clickedButton != null)
            {
                clickedButton.BorderThickness = new Thickness(0, 0, 0, 1.3);
            }// 设置当前点击的边框
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

        // 下拉框选择改变事件
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string comboBoxName = comboBox.Name; // 获取ComboBox名称
                int selectedIndex = comboBox.SelectedIndex; // 获取选中项索引

                switch (comboBoxName)
                {
                    case "WindowStartupLocationComboBox":
                        settingsCache.WindowStartupLocation = selectedIndex; // 设置窗口启动位置
                        break; // 功能面板打开位置
                }
            }
        }

        // 文本框内容改变事件
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string textBoxName = textBox.Name; // 获取文本框名称
                string textBoxValue = textBox.Text; // 获取文本框内容
                switch (textBoxName)
                {
                    case "LongPressThresholdTextBox":
                        if (int.TryParse(textBoxValue, out int shortPressThreshold))
                        {
                            if (shortPressThreshold < 30) // 长按阈值不能小于30
                            {
                                LongPressThresholdTextBox.Text = "30"; // 设置最小值
                                settingsCache.LongPressThreshold = 30; // 设置最小值
                            }
                            else if (shortPressThreshold > 3000) // 长按阈值不能大于3000
                            {
                                LongPressThresholdTextBox.Text = "3000"; // 设置最大值
                                settingsCache.LongPressThreshold = 3000; // 设置最大值
                            }
                            else settingsCache.LongPressThreshold = shortPressThreshold; // 设置长按阈值
                        }
                        else // 返回原来的数值
                        {
                            LongPressThresholdTextBox.Text = settingsCache.LongPressThreshold.ToString(); // 设置原来的数值
                        } // 设置长按阈值
                        break; // 长按阈值
                    case "MouseMovePixelsTextBox":
                        if (int.TryParse(textBoxValue, out int mouseMovePixels))
                        {
                            if((int)mouseMovePixels < 1) // 鼠标移动像素不能小于 1
                            {
                                MouseMovePixelsTextBox.Text = "1"; // 设置最小值
                                settingsCache.MouseMovePixels = 1; // 设置最小值
                            }
                            else if ((int)mouseMovePixels > 200) // 鼠标移动像素不能大于 200
                            {
                                MouseMovePixelsTextBox.Text = "200"; // 设置最大值
                                settingsCache.MouseMovePixels = 200; // 设置最大值
                            }
                            else settingsCache.MouseMovePixels = mouseMovePixels; // 设置鼠标移动像素
                        }
                        else // 返回原来的数值
                        {
                            MouseMovePixelsTextBox.Text = settingsCache.MouseMovePixels.ToString(); // 设置原来的数值
                        } // 设置鼠标移动像素
                        break; // 鼠标移动像素
                }
            }
        }

        // 勾选框点击事件
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                string checkBoxName = checkBox.Name; // 获取勾选框名称
                bool? isChecked = checkBox.IsChecked; // 获取勾选框状态
                switch (checkBoxName)
                {
                    case "AutoStartCheckBox":
                        settingsCache.AutoStart = isChecked == true;
                        break; // 开机自启动
                    case "ShowNotificationCheckBox":
                        settingsCache.ShowNotification = isChecked == true;
                        break;  // 显示启动完成提示
                    case "ShowAddImageCheckBox":
                        settingsCache.ShowAddImage = isChecked == true;
                        break; // 左键点击空白按钮时显示创建动作菜单
                    case "HideTooltipCheckBox":
                        settingsCache.HideTooltip = isChecked == true;
                        break; // 隐藏提示框
                    case "LoopPageFlippingCheckBox":
                        settingsCache.LoopPageFlipping = isChecked == true;
                        break; // 循环翻页
                    case "OpenMainWindowByMiddleMouseClickCheckBox":
                        settingsCache.OpenMainWindowByMiddleMouseClick = isChecked == true;
                        break; // 按下中键
                    case "OpenMainWindowByX1MouseClickCheckBox":
                        settingsCache.OpenMainWindowByX1MouseClick = isChecked == true; // 按下X1键
                        break; // 按下X1键
                    case "OpenMainWindowByX2MouseClickCheckBox":
                        settingsCache.OpenMainWindowByX2MouseClick = isChecked == true; // 按下X2键
                        break; // 按下X2键
                    case "OpenMainWindowByCtrl_MiddleMouseClickCheckBox":
                        settingsCache.OpenMainWindowByCtrl_MiddleMouseClick = isChecked == true; // Ctrl+中键单击
                        break; // Ctrl+中键单击
                    case "OpenMainWindowByCtrl_RightMouseClickCheckBox":
                        settingsCache.OpenMainWindowByCtrl_RightMouseClick = isChecked == true; // Ctrl+右键单击
                        break; // Ctrl+右键单击
                    case "OpenMainWindowByMiddleMouseClickLongerCheckBox":
                        settingsCache.OpenMainWindowByMiddleMouseClickLonger = isChecked == true; // 长按中键
                        break; // 长按中键
                    case "OpenMainWindowByRightMouseClickLongerCheckBox":
                        settingsCache.OpenMainWindowByRightMouseClickLonger = isChecked == true; // 长按右键
                        break; // 长按右键
                    case "OpenMainWindowByRightMouseClick_MoveCheckBox":
                        settingsCache.OpenMainWindowByRightMouseClick_Move = isChecked == true; // 按右键移动
                        break; // 按右键移动
                    case "OpenMainWindowByCtrlCheckBox":
                        settingsCache.OpenMainWindowByCtrl = isChecked == true; // 单击Ctrl键
                        break; // 单击Ctrl键
                }
            }
        }

        // 应用设置
        private async void ApplySettings(object sender, RoutedEventArgs e)
        {
            // 在后台线程中执行保存操作
            await Task.Run(() =>
            {
                bool succeed = true; // 保存成功标志
                var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
                bool originalAutoStart = Convention.AutoStart; // 保存原始的开机自启动设置
                bool newAutoStart = settingsCache.AutoStart; // 新的开机自启动设置

                // 更新开机自启动设置
                if (originalAutoStart != newAutoStart)
                {
                    succeed = UpdateAutostart(newAutoStart);
                    if (!succeed)
                    {
                        // 更新失败，回退到原来的设置
                        settingsCache.AutoStart = originalAutoStart;
                    }
                }

                // 更新数据库中的设置
                db1.ApplySettings(
                    settingsCache.AutoStart,
                    settingsCache.ShowNotification,
                    settingsCache.ShowAddImage,
                    settingsCache.HideTooltip,
                    settingsCache.LongPressThreshold,
                    settingsCache.MouseMovePixels,
                    settingsCache.LoopPageFlipping,
                    settingsCache.OpenMainWindowByMiddleMouseClick,
                    settingsCache.OpenMainWindowByX1MouseClick,
                    settingsCache.OpenMainWindowByX2MouseClick,
                    settingsCache.OpenMainWindowByCtrl_MiddleMouseClick,
                    settingsCache.OpenMainWindowByCtrl_RightMouseClick,
                    settingsCache.OpenMainWindowByMiddleMouseClickLonger,
                    settingsCache.OpenMainWindowByRightMouseClickLonger,
                    settingsCache.OpenMainWindowByRightMouseClick_Move,
                    settingsCache.OpenMainWindowByCtrl,
                    settingsCache.WindowStartupLocation
                );

                // 显示设置成功通知
                string message = succeed ? "设置应用成功！" : "设置开机自启动失败！";
                new ToastContentBuilder().AddText(message).Show();
            });
        }

        // 更新开机自启动设置
        private bool UpdateAutostart(bool autostart)
        {
            try
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location; // 获取应用程序路径
                string keyName = "Quicker"; // 注册表中的键名            
                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; // 获取注册表路径
                using (RegistryKey localMachine = Registry.LocalMachine.OpenSubKey(registryPath, true)) // 打开注册表
                {
                    if (localMachine != null)
                    {
                        if (autostart)
                        {
                            localMachine.SetValue(keyName, appPath); // 设置开机自启动
                        }
                        else
                        {
                            localMachine.DeleteValue(keyName, false); // 移除开机自启动
                        }
                    }
                    else
                    {
                        return false; // 如果无法打开注册表，返回失败
                    }
                }
                return true; // 返回设置成功
            }
            catch
            {
                return false; // 返回设置失败
            }
        }

        // 基础设置
        private void BasicSetting_Click(object sender, RoutedEventArgs e)
        {
            SetStackPanelVisibility(BasicSettingStackPanel, MenuGrid); // 设置StackPanel可见性
            UpdateButtonStyle1(BasicSetting, MainStackPanel); // 设置Button类型1颜色
        }
        // 鼠标移出Button恢复Background
        private void BasicSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (BasicSettingStackPanel.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF4F4F4"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0E0E0"));
                }
            }
        }


        // 基础设置-常规
        private void Convention_Click(object sender, RoutedEventArgs e)
        {
            if(ConventionGrid.Visibility == Visibility.Visible)
            {
                return; // 如果常规设置面板已经打开，则不执行任何操作
            } // 防止重复加载
            // 更新Button外观
            UpdateButtonStyle2(Convention, BasicSettingStackPanel);

            // 加载常规设置信息
            AutoStartCheckBox.IsChecked = settingsCache.AutoStart; // 加载开机自启动设置
            ShowNotificationCheckBox.IsChecked = settingsCache.ShowNotification; // 加载显示启动完成提示设置
            ShowAddImageCheckBox.IsChecked = settingsCache.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
            HideTooltipCheckBox.IsChecked = settingsCache.HideTooltip; // 加载隐藏提示框设置
            LongPressThresholdTextBox.Text = settingsCache.LongPressThreshold.ToString(); // 加载长按阈值设置
            MouseMovePixelsTextBox.Text = settingsCache.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
            LoopPageFlippingCheckBox.IsChecked = settingsCache.LoopPageFlipping; // 加载循环翻页设置

            // 设置ApplySettingsButton可见性
            Button ApplySettingsButton = FindName("ApplySettingsButton") as Button; // 获取ApplySettingsButton
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性

            // 设置Grid可见性
            SetGridVisible(ConventionGrid, ResultGrid);
        }
        // 鼠标移出Button恢复Background
        private void Convention_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (ConventionGrid.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                }
            }
        }

        // 基础设置-弹出面板
        private void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            // 更新Button外观
            UpdateButtonStyle2(OpenMainWindow, BasicSettingStackPanel);

            // 设置ApplySettingsButton可见性
            Button ApplySettingsButton = FindName("ApplySettingsButton") as Button; // 获取ApplySettingsButton
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性

            // 设置Grid可见性
            SetGridVisible(OpenMainWindowGrid, ResultGrid);

            // 重置测试Button
            TestButton.Content = "按键测试区";

            // 加载勾选框
            OpenMainWindowByMiddleMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByMiddleMouseClick; // 按下中键
            OpenMainWindowByX1MouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByX1MouseClick; // 按下X1键
            OpenMainWindowByX2MouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByX2MouseClick; // 按下X2键
            OpenMainWindowByCtrl_MiddleMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl_MiddleMouseClick; // Ctrl+中键单击
            OpenMainWindowByCtrl_RightMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl_RightMouseClick; // Ctrl+右键单击
            OpenMainWindowByMiddleMouseClickLongerCheckBox.IsChecked = settingsCache.OpenMainWindowByMiddleMouseClickLonger; // 长按中键
            OpenMainWindowByRightMouseClickLongerCheckBox.IsChecked = settingsCache.OpenMainWindowByRightMouseClickLonger; // 长按右键
            OpenMainWindowByRightMouseClick_MoveCheckBox.IsChecked = settingsCache.OpenMainWindowByRightMouseClick_Move; // 按右键移动
            OpenMainWindowByCtrlCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl; // 单击Ctrl键
            WindowStartupLocationComboBox.SelectedIndex = settingsCache.WindowStartupLocation; // 功能面板打开位置
        }
        // 鼠标移出Button恢复Background
        private void OpenMainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (OpenMainWindowGrid.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                }
            }
        }

        // 基础设置-弹出面板-弹出面板
        private void OpenMainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SetGridVisible(OpenMainWindowButtonGrid, OpenMainWindowGrid); // 设置Grid可见性
            UpdateButtonStyle3(OpenMainWindowButton, OpenMainWindowGrid); // 设置Button类型3边框
        }

        // 测试鼠标按键名称
        private void TestMouseKey(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    TestButton.Content = "左键";
                    break; // 左键
                case MouseButton.Right:
                    TestButton.Content = "右键";
                    break; // 右键
                case MouseButton.Middle:
                    TestButton.Content = "中键";
                    break; // 中键
                case MouseButton.XButton1:
                    TestButton.Content = "X1键";
                    break; // X1键
                case MouseButton.XButton2:
                    TestButton.Content = "X2键";
                    break; // X2键
                default:
                    TestButton.Content = "未知按键";
                    break; // 未知按键
            }
        }

        /*
        // 阻止文本输入
        private void DIY_ShortcutKeys_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true; // 阻止文本输入
        }

        // 自定义快捷键
        private void DIY_ShortcutKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (DIY_ShortcutKeys.IsFocused)
            {
                //e.Handled = true; // 阻止输入
                InputMethod.SetIsInputMethodEnabled(DIY_ShortcutKeys, false); // 禁用输入法
                ModifierKeys modifiers = Keyboard.Modifiers; // 获取修饰键
                Key key = e.Key; // 获取按键

                List<string> keys = new List<string>(); // 保存所有键

                // 清除之前的普通键（非 Ctrl、Shift、Alt）
                List<string> keysToRemove = new List<string>();
                foreach (string keyStr in ShortcutKeys)
                {
                    if (!keyStr.Equals("LeftCtrl") && !keyStr.Equals("RightCtrl") &&
                        !keyStr.Equals("LeftShift") && !keyStr.Equals("RightShift") &&
                        !keyStr.Equals("LeftAlt") && !keyStr.Equals("RightAlt"))
                    {
                        keysToRemove.Add(keyStr);
                    }
                } // 获取所有普通键
                foreach (string keyStr in keysToRemove)
                {
                    ShortcutKeys.Remove(keyStr);
                } // 移除所有普通键

                if (ShortcutKeys == null || ShortcutKeys.Count == 0)
                {
                    switch (key)
                    {
                        case Key.LeftCtrl:
                            if (!ShortcutKeys.Contains("LeftCtrl"))
                            {
                                ShortcutKeys.Add("LeftCtrl");
                            }
                            break; // 添加左Ctrl键
                        case Key.RightCtrl:
                            if (!ShortcutKeys.Contains("RightCtrl"))
                            {
                                ShortcutKeys.Add("RightCtrl");
                            }
                            break; // 添加右Ctrl键
                        case Key.LeftShift:
                            if (!ShortcutKeys.Contains("LeftShift"))
                            {
                                ShortcutKeys.Add("LeftShift");
                            }
                            break; // 添加左Shift键
                        case Key.RightShift:
                            if (!ShortcutKeys.Contains("RightShift"))
                            {
                                ShortcutKeys.Add("RightShift");
                            }
                            break; // 添加右Shift键
                        case Key.LeftAlt:
                            if (!ShortcutKeys.Contains("LeftAlt"))
                            {
                                ShortcutKeys.Add("LeftAlt");
                            }
                            break; // 添加左Alt键
                        case Key.RightAlt:
                            if (!ShortcutKeys.Contains("RightAlt"))
                            {
                                ShortcutKeys.Add("RightAlt");
                            }
                            break; // 添加右Alt键
                        case Key.Back:
                            if (!ShortcutKeys.Contains("Back"))
                            {
                                ShortcutKeys.Add("Back");
                            }
                            break; // 添加Back键
                        default:
                            string keyName = key.ToString();
                            if (ShortcutKeys.Contains(keyName))
                            {
                                ShortcutKeys.Remove(keyName);
                            }
                            ShortcutKeys.Add(keyName);
                            break; // 添加普通键
                    }
                } // 处理单独按键
                else
                {
                    if (modifiers.HasFlag(ModifierKeys.Control))
                    {                       
                        if (ShortcutKeys.Contains("LeftCtrl") || ShortcutKeys.Contains("RightCtrl"))
                        {
                           
                            ShortcutKeys.RemoveAll(key => key == "LeftCtrl" || key == "RightCtrl"); // 移除所有的Ctrl相关键
                           
                            if (!ShortcutKeys.Contains("Ctrl")) // 添加Ctrl
                            {
                                ShortcutKeys.Add("Ctrl");
                            }
                        } // 检查是否存在LeftCtrl或RightCtrl，并替换为Ctrl
                        else
                        {                           
                            if (!ShortcutKeys.Contains("Ctrl"))
                            {
                                ShortcutKeys.Add("Ctrl");
                            }
                        } // 如果没有LeftCtrl或RightCtrl，但有Ctrl modifier，直接添加Ctrl
                    } // 处理Ctrl键                     
                    if (modifiers.HasFlag(ModifierKeys.Shift))
                    {                       
                        if (ShortcutKeys.Contains("LeftShift") || ShortcutKeys.Contains("RightShift"))
                        {                           
                            ShortcutKeys.RemoveAll(key => key == "LeftShift" || key == "RightShift"); // 移除所有的Shift相关键                           
                            if (!ShortcutKeys.Contains("Shift")) // 添加Shift
                            {
                                ShortcutKeys.Add("Shift");
                            }
                        } // 检查是否存在LeftShift或RightShift，并替换为Shift
                        else
                        {
                            // 如果没有LeftShift或RightShift，但有Shift modifier，直接添加Shift
                            if (!ShortcutKeys.Contains("Shift"))
                            {
                                ShortcutKeys.Add("Shift");
                            }
                        }
                    } // 处理Shift键                   
                    if (modifiers.HasFlag(ModifierKeys.Alt))
                    {                       
                        if (ShortcutKeys.Contains("LeftAlt") || ShortcutKeys.Contains("RightAlt"))
                        {                           
                            ShortcutKeys.RemoveAll(key => key == "LeftAlt" || key == "RightAlt"); // 移除所有的Alt相关键                           
                            if (!ShortcutKeys.Contains("Alt")) // 添加Alt
                            {
                                ShortcutKeys.Add("Alt");
                            }
                        } // 检查是否存在LeftAlt或RightAlt，并替换为Alt
                        else
                        {                           
                            if (!ShortcutKeys.Contains("Alt"))
                            {
                                ShortcutKeys.Add("Alt");
                            }
                        } // 如果没有LeftAlt或RightAlt，但有Alt modifier，直接添加Alt
                    } // 处理Alt键
                    if(modifiers.HasFlag(ModifierKeys.Windows))
                    {
                        if (ShortcutKeys.Contains("LeftWindows") || ShortcutKeys.Contains("RightWindows"))
                        {
                            ShortcutKeys.RemoveAll(key => key == "LeftWindows" || key == "RightWindows"); // 移除所有的Windows相关键
                            if (!ShortcutKeys.Contains("Windows")) // 添加Windows
                            {
                                ShortcutKeys.Add("Windows");
                            }
                        } // 检查是否存在LeftWindows或RightWindows，并替换为Windows
                        else
                        {
                            // 如果没有LeftWindows或RightWindows，但有Windows modifier，直接添加Windows
                            if (!ShortcutKeys.Contains("Windows"))
                            {
                                ShortcutKeys.Add("Windows");
                            }
                        }
                    } // 处理Windows键

                    string keyName = key.ToString();
                    if (!ShortcutKeys.Contains(keyName))
                    {
                        ShortcutKeys.Add(keyName);
                    } // 添加普通键
                } // 处理组合键

                // 移除重复的键
                ShortcutKeys = ShortcutKeys.Distinct().ToList();

                // 将键组合成字符串
                string shortcut = string.Join("+", ShortcutKeys);
                DIY_ShortcutKeys.Text = shortcut;
            }
        }

        // 清除之前的输入
        private void DIY_ShortcutKeys_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ShortcutKeys.Clear();
        }

        // 清除文本框内容
        private void ClearDIY_ShortcutKeys(object sender, RoutedEventArgs e)
        {
            DIY_ShortcutKeys.Text = null;
            ShortcutKeys.Clear();
        }
        */
        // 基础设置-弹出面板-动作触发按键
        /*
        private void DoActionKeyboard_Click(object sender, RoutedEventArgs e)
        {
            SetGridVisible(DoActionKeyboardButtonGrid, OpenMainWindowGrid);

            UpdateButtonStyle3(DoActionKeyboardButton, OpenMainWindowGrid);
        }*/

        // 基础设置-关于Quicker
        private void AboutQuicker_Click(object sender, RoutedEventArgs e)
        {
            // 更新Button外观
            UpdateButtonStyle2(AboutQuicker, BasicSettingStackPanel); // 设置Button类型2颜色&&字体粗细
            AboutQuickerGrid.BringIntoView(); // 滚动到AboutQuickerGrid

            // 设置ApplySettingsButton可见性
            Button ApplySettingsButton = FindName("ApplySettingsButton") as Button; // 获取ApplySettingsButton
            ApplySettingsButton.Visibility = Visibility.Hidden; // 设置ApplySettingsButton可见性

            // 设置Grid可见性
            SetGridVisible(AboutQuickerGrid, ResultGrid);
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (AboutQuickerGrid.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                }
            }
        }

        // 基础设置-关于Quicker-关于Quicker
        private void AboutQuickerButton_Click(object sender, RoutedEventArgs e)
        {
            SetGridVisible(AboutQuickerButtonGrid, AboutQuickerGrid); // 设置Grid可见性
            UpdateButtonStyle3(AboutQuickerButton, AboutQuickerGrid); // 设置Button类型3边框
        }

        // 打开更新历史文件
        private void OpenUpdateLog(object sender, MouseButtonEventArgs e)
        {
            Process.Start("notepad.exe", "UpdateLog.txt"); // 打开更新历史文件
        }

        // 前往图标网站www.iconfont.cn
        private void www_iconfont_cn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.iconfont.cn", // 打开图标网站www.iconfont.cn
                UseShellExecute = true // 使用外壳程序启动
            });
        }

        // 前往图标网站icons8.com
        private void icons8_com_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://icons8.com/", // 打开图标网站icons8.com
                UseShellExecute = true // 使用外壳程序启动
            });
        }

        // 前往图标网站fontawesome.com
        private void fontawesome_com_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://fontawesome.com/", // 打开图标网站fontawesome.com
                UseShellExecute = true // 使用外壳程序启动
            });
        }


        // 基础设置-关于Quicker-隐私声明
        private void Privacy_StatementButton_Click(object sender, RoutedEventArgs e)
        {
            SetGridVisible(Privacy_StatementButtonGrid, AboutQuickerGrid); // 设置Grid可见性
            UpdateButtonStyle3(Privacy_StatementButton, AboutQuickerGrid); // 设置Button类型3边框
        }

        // 辅助功能
        private void Auxiliary_Functions_Click(object sender, RoutedEventArgs e)
        {
            SetStackPanelVisibility(Auxiliary_FunctionsStackPanel, MenuGrid); // 设置StackPanel可见性
            UpdateButtonStyle1(Auxiliary_Functions, MainStackPanel); // 设置Button类型1颜色
        }
        // 鼠标移出Button恢复Background
        private void Auxiliary_Functions_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (Auxiliary_FunctionsStackPanel.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF4F4F4"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0E0E0"));
                }
            }
        }

        // 工具
        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            SetStackPanelVisibility(ToolsStackPanel, MenuGrid); // 设置StackPanel可见性
            UpdateButtonStyle1(Tools, MainStackPanel); // 设置Button类型1颜色
        }
        // 鼠标移出Button恢复Background
        private void Tools_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (ToolsStackPanel.Visibility == Visibility.Visible)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF4F4F4"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0E0E0"));
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e); // 调用基类的OnClosing方法
            timer.Stop(); // 停止定时器
        }

        // 缓存对象类
        private class SettingsCache
        {
            public bool AutoStart { get; set; } // 开机自启动
            public bool ShowNotification { get; set; } // 显示启动完成提示
            public bool ShowAddImage { get; set; } // 左键点击空白按钮时显示创建动作菜单
            public bool HideTooltip { get; set; } // 隐藏提示框
            public int LongPressThreshold { get; set; } // 长按阈值
            public int MouseMovePixels { get; set; } // 鼠标移动像素
            public bool LoopPageFlipping { get; set; } // 循环翻页
            public bool OpenMainWindowByMiddleMouseClick { get; set; } // 按下中键
            public bool OpenMainWindowByX1MouseClick { get; set; } // 按下X1键
            public bool OpenMainWindowByX2MouseClick { get; set; } // 按下X2键
            public bool OpenMainWindowByCtrl_MiddleMouseClick { get; set; } // Ctrl+中键单击
            public bool OpenMainWindowByCtrl_RightMouseClick { get; set; } // Ctrl+右键单击
            public bool OpenMainWindowByMiddleMouseClickLonger { get; set; } // 长按中键
            public bool OpenMainWindowByRightMouseClickLonger { get; set; } // 长按右键
            public bool OpenMainWindowByRightMouseClick_Move { get; set; } // 按右键移动
            public bool OpenMainWindowByCtrl { get; set; } // 单击Ctrl键
            public int WindowStartupLocation { get; set; } // 功能面板打开位置
        }
    }
}