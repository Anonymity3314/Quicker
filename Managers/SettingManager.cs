using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System;

namespace Quicker.Managers
{
    internal class SettingManager
    {
        public T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // 循环查找父元素
            while ((child = VisualTreeHelper.GetParent(child)) != null)
            {
                if (child is T)
                    return (T)child;
            }
            return null;
        } // 查找父元素
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮颜色

        private readonly SettingDatabase db1; // 设置数据库
        public SettingsCache settingsCache; // 缓存对象

        public SettingManager()
        {
            db1 = new SettingDatabase(); // 实例化设置数据库
            InitializeCache(); // 初始化缓存对象
        }

        // 初始化缓存
        private async void InitializeCache()
        {
            await LoadSettingsAsync(); // 异步加载常规设置信息
        }

        // 异步加载常规设置信息
        public async Task LoadSettingsAsync()
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetStackPanel"> 目标StackPanel </param>
        /// <param name="targetButton"> 目标Button </param>
        /// <param name="fatherStackPanel"> 父级StackPanel </param>
        public void ButtonStyle1_Click(StackPanel targetStackPanel, Button targetButton, StackPanel fatherStackPanel, Grid fathergrid, StackPanel fatherStackPanel1)
        {
            if (targetStackPanel.Visibility == Visibility.Visible) return; // 如果目标面板已经打开，则不执行任何操作
            foreach (var stackpanel in fathergrid.Children.OfType<StackPanel>())
            {
                stackpanel.Visibility = stackpanel == targetStackPanel ? Visibility.Visible : Visibility.Hidden; // 设置StackPanel可见性
            } // 设置StackPanel可见性

            foreach (var button in fatherStackPanel1.Children.OfType<Button>())
            {
                button.Background = button == targetButton ?
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1)) :
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button类型1颜色
            } // 设置Button类型1颜色
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="stackPanel"> 目标StackPanel </param>
        public void ButtonStyle1_MouseLeave(object sender, StackPanel stackPanel)
        {
            Button button = sender as Button; // 获取Button
            button.Background = stackPanel.Visibility == Visibility.Visible ?
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1)) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button颜色
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetButton"> 目标Button </param>
        /// <param name="stackPanel"> 目标StackPanel </param>
        /// <param name="targetGrid"> 目标Grid </param>
        /// <param name="fatherGrid"> 父级Grid </param>
        public void ButtonStyle2_Click(Button targetButton, StackPanel stackPanel, UserControl targetGrid, Grid fatherGrid)
        {
            if (fatherGrid.Children.Contains(targetGrid)) return; // 如果目标控件已经存在，则不执行任何操作
            var existingGrid = fatherGrid.Children.OfType<UserControl>().FirstOrDefault(); // 获取第一个 UserControl 子元素
            fatherGrid.Children.Remove(existingGrid); // 移除现有的 Grid
            targetGrid.SetValue(Grid.ColumnSpanProperty, 2); // 设置列跨度
            fatherGrid.Children.Add(targetGrid); // 添加目标控件

            foreach (var button in stackPanel.Children.OfType<Button>()) // 设置按钮样式
            {
                button.Background = button == targetButton ?
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor2)) :
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor2)); // 设置Button颜色
                button.FontWeight = button == targetButton ? FontWeights.Bold : FontWeights.Normal; // 设置字体粗细
            } // 设置Button颜色&&字体粗细
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"> 目标Button </param>
        /// <param name="targetGrid"> 目标Grid </param>
        public void ButtonStyle2_MouseLeave(object sender, UserControl targetGrid, Grid fatherGrid)
        {
            Button button = sender as Button; // 获取Button
            button.Background = fatherGrid.Children.Contains(targetGrid) ?
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor2)) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor2)); // 设置Button颜色
        }

        /// <summary>
        /// 设置Button类型3边框
        /// </summary>
        /// <param name="clickedButton"> 被点击的Button </param>
        /// <param name="buttonPanelGrid"> Button面板Grid </param>
        public static void UpdateButtonStyle3(Button clickedButton, Grid buttonPanelGrid)
        {
            foreach (var button in buttonPanelGrid.Children.OfType<Button>())
            {
                button.BorderThickness = button == clickedButton ? new Thickness(0, 0, 0, 1.3) : new Thickness(0); // 设置Button边框
            }
        }

        // 下拉框选择改变事件
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        // 勾选框点击事件
        public void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
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

        // 文本框内容改变事件
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                                textBox.Text = "30"; // 设置最小值
                                settingsCache.LongPressThreshold = 30; // 设置最小值
                            }
                            else if (shortPressThreshold > 3000) // 长按阈值不能大于3000
                            {
                                textBox.Text = "3000"; // 设置最大值
                                settingsCache.LongPressThreshold = 3000; // 设置最大值
                            }
                            else settingsCache.LongPressThreshold = shortPressThreshold; // 设置长按阈值
                        }
                        else // 返回原来的数值
                        {
                            textBox.Text = settingsCache.LongPressThreshold.ToString(); // 设置原来的数值
                        } // 设置长按阈值
                        break; // 长按阈值
                    case "MouseMovePixelsTextBox":
                        if (int.TryParse(textBoxValue, out int mouseMovePixels))
                        {
                            if ((int)mouseMovePixels < 1) // 鼠标移动像素不能小于 1
                            {
                                textBox.Text = "1"; // 设置最小值
                                settingsCache.MouseMovePixels = 1; // 设置最小值
                            }
                            else if ((int)mouseMovePixels > 200) // 鼠标移动像素不能大于 200
                            {
                                textBox.Text = "200"; // 设置最大值
                                settingsCache.MouseMovePixels = 200; // 设置最大值
                            }
                            else settingsCache.MouseMovePixels = mouseMovePixels; // 设置鼠标移动像素
                        }
                        else // 返回原来的数值
                        {
                            textBox.Text = settingsCache.MouseMovePixels.ToString(); // 设置原来的数值
                        } // 设置鼠标移动像素
                        break; // 鼠标移动像素
                }
            }
        }

        /// <summary>
        /// 打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        public void OpenWebsite(string website)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = website, // 打开指定网站
                UseShellExecute = true // 使用外壳程序启动
            });
        }

        // 清理缓存
        public void ClearCache()
        {
            settingsCache = null; // 清理缓存
        }

        // 缓存对象类
        public class SettingsCache
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