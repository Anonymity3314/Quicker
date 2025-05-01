using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System;

namespace Quicker.Managers
{
    public class SettingManager
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

        public OpenMainWindowConditionsSettingsCache openMainWindowConditions; // 弹出面板设置缓存对象
        public AppearanceConditionsSettingsCache appearanceConditions; // 外观设置缓存对象
        private readonly SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        public BlacklistSettingsSettingsCache blacklistSettings; // 黑名单设置缓存对象
        public ConventionsSettingsCache conventions; // 常规设置缓存对象

        // 异步加载常规设置信息
        public async Task LoadConventionsAsync()
        {
            if (conventions != null) return; // 如果已经初始化了数据，直接返回
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
            conventions = new ConventionsSettingsCache // 常规设置
            {
                Version = Conventions.Version, // 版本号
                AutoStart = Conventions.AutoStart, // 开机自启
                ShowNotification = Conventions.ShowNotification, // 显示通知
                ShowAddImage = Conventions.ShowAddImage, // 显示添加图片按钮
                HideTooltip = Conventions.HideTooltip, // 隐藏工具提示
                LongPressThreshold = Conventions.LongPressThreshold, // 长按阈值
                MouseMovePixels = Conventions.MouseMovePixels, // 鼠标移动像素
                LoopPageFlipping = Conventions.LoopPageFlipping // 循环翻页
            }; // 加载设置数据到缓存
        }

        // 异步加载弹出面板设置信息
        public async Task LoadOpenMainWindowConditionsAsync()
        {
            if (openMainWindowConditions != null) return; // 如果已经初始化了数据，直接返回
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取弹出面板设置信息
            openMainWindowConditions = new OpenMainWindowConditionsSettingsCache // 弹出面板设置
            {
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

        // 异步加载黑名单设置信息
        public async Task LoadBlacklistSettingsAsync()
        {
            if (blacklistSettings != null) return; // 如果已经初始化了数据，直接返回
            var BlacklistSettings = db1.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置信息
            blacklistSettings = new BlacklistSettingsSettingsCache // 黑名单设置
            {
                FullScreenDisable = BlacklistSettings.IsFullScreenDisabled, // 启用黑名单
                ApplyBlacklistToExpandHotkeys = BlacklistSettings.IsBlacklistEnabledForExtendedHotkey // 是否将黑名单与全屏禁用设置应用于扩展热键功能
            }; // 加载设置数据到缓存
        }

        // 异步加载外观设置信息
        public async Task LoadAppearanceAsync()
        {
            if (appearanceConditions != null) return; // 如果已经初始化了数据，直接返回
            //var Appearance = db1.GetAllAppearance().FirstOrDefault(); // 获取外观设置信息
            //settingsCache = new SettingsCache // 外观设置
            {
                //AutoHideTitleBar = Appearance.AutoHideTitleBar,
                //ShowActionButtonMouseOver = Appearance.ShowActionButtonMouseOver,
                //HideActionNameAfterIcon = Appearance.HideActionNameAfterIcon,
                //ShowActionIconShadow = Appearance.ShowActionIconShadow
            }; // 加载设置数据到缓存
        }

        // 设置Grid可见性
        public void SetGridVisible(Grid childrengrid, Grid fathergrid)
        {
            foreach (var grid in fathergrid.Children.OfType<Grid>())
            {
                grid.Visibility = grid == childrengrid ? Visibility.Visible : Visibility.Collapsed; // 设置Grid可见性
            }
        }

        /// <summary>
        /// 改变Button类型1样式
        /// </summary>
        /// <param name="targetStackPanel"> 目标StackPanel </param>
        /// <param name="targetButton"> 目标Button </param>
        /// <param name="fatherStackPanel"> 父级StackPanel </param>
        public void ButtonStyle1_Click(StackPanel targetStackPanel, Button targetButton, StackPanel fatherStackPanel, Grid fathergrid)
        {
            if (targetStackPanel.Visibility == Visibility.Visible) return; // 如果目标面板已经打开，则不执行任何操作
            foreach (var stackpanel in fathergrid.Children.OfType<StackPanel>())
            {
                stackpanel.Visibility = stackpanel == targetStackPanel ? Visibility.Visible : Visibility.Collapsed; // 设置StackPanel可见性
            } // 设置StackPanel可见性

            foreach (var button in fatherStackPanel.Children.OfType<Button>())
            {
                button.Background = button == targetButton
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button类型1颜色
            } // 设置Button类型1颜色
        }

        /// <summary>
        /// 设置Button类型1背景色
        /// </summary>
        /// <param name="sender"> 目标Button </param>
        /// <param name="stackPanel"> 目标StackPanel </param>
        public void ButtonStyle1_MouseLeave(object sender, StackPanel stackPanel)
        {
            Button button = sender as Button; // 获取Button
            button.Background = stackPanel.Visibility == Visibility.Visible ?
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1)) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button颜色
        }

        /// <summary>
        /// 改变Button类型2样式
        /// </summary>
        /// <param name="targetButton"> 目标Button </param>
        /// <param name="stackPanel"> 目标StackPanel </param>
        /// <param name="targetGrid"> 目标Grid </param>
        /// <param name="fatherGrid"> 父级Grid </param>
        public async Task ButtonStyle2_Click(Button targetButton, StackPanel stackPanel, UserControl targetGrid, Grid fatherGrid)
        {
            var existingGrid = fatherGrid.Children.OfType<UserControl>().FirstOrDefault(); // 获取第一个 UserControl 子元素
            if (existingGrid == null || existingGrid.Name != targetGrid.Name)
            {
                if (existingGrid != null) fatherGrid.Children.Remove(existingGrid); // 移除旧的 UserControl 子元素
                targetGrid.SetValue(Grid.ColumnSpanProperty, 2); // 设置目标Grid列跨度为2
                fatherGrid.Children.Add(targetGrid); // 添加目标Grid子元素
            }
            else return; // 如果目标面板已经打开，则不执行任何操作

            foreach (var button in stackPanel.Children.OfType<Button>())
            {
                button.Background = button == targetButton
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor2))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor2)); // 设置Button类型2颜色
                button.FontWeight = button == targetButton ? FontWeights.Bold : FontWeights.Normal; // 设置Button类型2粗体
            } // 设置Button类型2颜色&&粗体
        }

        /// <summary>
        /// 设置Button类型2背景色
        /// </summary>
        /// <param name="sender"> 目标Button </param>
        /// <param name="targetGrid"> 目标Grid </param>
        public void ButtonStyle2_MouseLeave(object sender, UserControl targetGrid, Grid fatherGrid)
        {
            Button button = sender as Button; // 获取Button
            var existingGrid = fatherGrid.Children.OfType<UserControl>().FirstOrDefault(); // 获取第一个 UserControl 子元素
            if (existingGrid.Name == targetGrid.Name)
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor2)); // 设置Button颜色
        }

        /// <summary>
        /// 设置Button类型3边框
        /// </summary>
        /// <param name="clickedButton"> 被点击的Button </param>
        /// <param name="buttonPanelGrid"> Button面板Grid </param>
        public void ButtonStyle3_Click(Button clickedButton, Grid buttonPanelGrid)
        {
            foreach (var button in buttonPanelGrid.Children.OfType<Button>())
            {
                button.BorderThickness = button == clickedButton ? new Thickness(0, 0, 0, 2) : new Thickness(0); // 设置Button边框
            }
        }

        // 下拉框选择改变事件
        public void ComboBox_SelectionChanged(object sender)
        {
            if (sender is ComboBox comboBox)
            {
                string comboBoxName = comboBox.Name; // 获取ComboBox名称
                int selectedIndex = comboBox.SelectedIndex; // 获取选中项索引
                switch (comboBoxName)
                {
                    case "WindowStartupLocationComboBox":
                        openMainWindowConditions.WindowStartupLocation = selectedIndex; // 设置窗口启动位置
                        break; // 功能面板打开位置
                }
            }
        }

        /// <summary>
        /// 勾选框点击事件
        /// </summary>
        /// <param name="sender"> 勾选框 </param>
        public void CheckBox_Click(object sender)
        {
            CheckBox checkBox = (CheckBox)sender; // 获取勾选框
            string checkBoxName = checkBox.Name; // 获取勾选框名称
            bool? isChecked = checkBox.IsChecked; // 获取勾选框状态
            switch (checkBoxName)
            {
                case "AutoStartCheckBox":
                    conventions.AutoStart = isChecked == true;
                    break; // 开机自启动
                case "ShowNotificationCheckBox":
                    conventions.ShowNotification = isChecked == true;
                    break;  // 显示启动完成提示
                case "ShowAddImageCheckBox":
                    conventions.ShowAddImage = isChecked == true;
                    break; // 左键点击空白按钮时显示创建动作菜单
                case "HideTooltipCheckBox":
                    conventions.HideTooltip = isChecked == true;
                    break; // 隐藏提示框
                case "LoopPageFlippingCheckBox":
                    conventions.LoopPageFlipping = isChecked == true;
                    break; // 循环翻页
                case "OpenMainWindowByMiddleMouseClickCheckBox":
                    openMainWindowConditions.OpenMainWindowByMiddleMouseClick = isChecked == true;
                    break; // 按下中键
                case "OpenMainWindowByX1MouseClickCheckBox":
                    openMainWindowConditions.OpenMainWindowByX1MouseClick = isChecked == true; // 按下X1键
                    break; // 按下X1键
                case "OpenMainWindowByX2MouseClickCheckBox":
                    openMainWindowConditions.OpenMainWindowByX2MouseClick = isChecked == true; // 按下X2键
                    break; // 按下X2键
                case "OpenMainWindowByCtrl_MiddleMouseClickCheckBox":
                    openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick = isChecked == true; // Ctrl+中键单击
                    break; // Ctrl+中键单击
                case "OpenMainWindowByCtrl_RightMouseClickCheckBox":
                    openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick = isChecked == true; // Ctrl+右键单击
                    break; // Ctrl+右键单击
                case "OpenMainWindowByMiddleMouseClickLongerCheckBox":
                    openMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger = isChecked == true; // 长按中键
                    break; // 长按中键
                case "OpenMainWindowByRightMouseClickLongerCheckBox":
                    openMainWindowConditions.OpenMainWindowByRightMouseClickLonger = isChecked == true; // 长按右键
                    break; // 长按右键
                case "OpenMainWindowByRightMouseClick_MoveCheckBox":
                    openMainWindowConditions.OpenMainWindowByRightMouseClick_Move = isChecked == true; // 按右键移动
                    break; // 按右键移动
                case "OpenMainWindowByCtrlCheckBox":
                    openMainWindowConditions.OpenMainWindowByCtrl = isChecked == true; // 单击Ctrl键
                    break; // 单击Ctrl键
                case "FullScreenDisableCheckBox":
                    blacklistSettings.FullScreenDisable = isChecked == true; // 全屏禁用Quicker
                    break; // 全屏禁用Quicker
                case "ApplyBlacklistToExpandHotkeysCheckBox":
                    blacklistSettings.ApplyBlacklistToExpandHotkeys = isChecked == true; // 黑名单应用到热键扩展
                    break; // 黑名单应用到热键扩展
                //case "AutoHideTitleBarCheckBox":
                //    settingsCache.AutoHideTitleBar = isChecked == true; // 自动缩小动作名称文字
                //    break; // 自动缩小动作名称文字
                //case "ShowActionButtonMouseOverCheckBox":
                //    settingsCache.ShowActionButtonMouseOver = isChecked == true; // 鼠标悬浮在动作按钮上时放大显示按钮
                //    break; // 鼠标悬浮在动作按钮上时放大显示按钮
                //case "HideActionNameAfterIconCheckBox":
                //    settingsCache.HideActionNameAfterIcon = isChecked == true; // 设置动作图标后隐藏动作名称
                //    break; // 设置动作图标后隐藏动作名称
                //case "ShowActionIconShadowCheckBox":
                //    settingsCache.ShowActionIconShadow = isChecked == true; // 动作图标显示阴影
                //    break; // 动作图标显示阴影
            }
        }

        // 文本框内容改变事件
        public void TextBox_TextChanged(object sender)
        {
            TextBox textBox = sender as TextBox;
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
                            conventions.LongPressThreshold = 30; // 设置最小值
                        }
                        else if (shortPressThreshold > 3000) // 长按阈值不能大于3000
                        {
                            textBox.Text = "3000"; // 设置最大值
                            conventions.LongPressThreshold = 3000; // 设置最大值
                        }
                        else conventions.LongPressThreshold = shortPressThreshold; // 设置长按阈值
                    }
                    else // 返回原来的数值
                    {
                        textBox.Text = conventions.LongPressThreshold.ToString(); // 设置原来的数值
                    } // 设置长按阈值
                    break; // 长按阈值
                case "MouseMovePixelsTextBox":
                    if (int.TryParse(textBoxValue, out int mouseMovePixels))
                    {
                        if ((int)mouseMovePixels < 1) // 鼠标移动像素不能小于 1
                        {
                            textBox.Text = "1"; // 设置最小值
                            conventions.MouseMovePixels = 1; // 设置最小值
                        }
                        else if ((int)mouseMovePixels > 200) // 鼠标移动像素不能大于 200
                        {
                            textBox.Text = "200"; // 设置最大值
                            conventions.MouseMovePixels = 200; // 设置最大值
                        }
                        else conventions.MouseMovePixels = mouseMovePixels; // 设置鼠标移动像素
                    }
                    else // 返回原来的数值
                    {
                        textBox.Text = conventions.MouseMovePixels.ToString(); // 设置原来的数值
                    } // 设置鼠标移动像素
                    break; // 鼠标移动像素
                case "ButtonSizeTextBox":
                    break;
                case "ButtonGapTextBox":
                    break;
                case "BorderWidthTextBox":
                    break;
                case "ButtonCornerRadiusTextBox":
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e, TextBox targetTextBox)
        {
            Slider slider = sender as Slider;
            string sliderName = slider.Name;
            switch(sliderName)
            {
                case "ButtonSizeSlider":
                    break;
                case "ButtonGapSlider":
                    break;
                case "BorderWidthSlider":
                    break;
                case "ButtonCornerRadiusSlider":
                    break;
            }
        }

        // 手动释放资源
        public void Dispose()
        {
            // 清理缓存
            conventions = null;
            openMainWindowConditions = null;
            blacklistSettings = null;
            appearanceConditions = null;

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // 常规设置
        public class ConventionsSettingsCache
        {
            public string Version { get; set; } // 版本号
            public bool AutoStart { get; set; } // 开机自启动
            public bool ShowNotification { get; set; } // 显示启动完成提示
            public bool ShowAddImage { get; set; } // 左键点击空白按钮时显示创建动作菜单
            public bool HideTooltip { get; set; } // 隐藏提示框
            public int LongPressThreshold { get; set; } // 长按阈值
            public int MouseMovePixels { get; set; } // 鼠标移动像素
            public bool LoopPageFlipping { get; set; } // 循环翻页
        }

        // 弹出面板设置
        public class OpenMainWindowConditionsSettingsCache
        {
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
  
        // 黑名单设置
        public class BlacklistSettingsSettingsCache
        {
            public bool FullScreenDisable { get; set; } // 全屏禁用Quicker
            public bool ApplyBlacklistToExpandHotkeys { get; set; } // 黑名单应用到热键扩展
        }

        // 外观设置
        public class AppearanceConditionsSettingsCache
        {
            public bool AutoHideTitleBar { get; set; } // 自动缩小动作名称文字
            public bool ShowActionButtonMouseOver { get; set; } // 鼠标悬浮在动作按钮上时放大显示按钮
            public bool HideActionNameAfterIcon { get; set; } // 设置动作图标后隐藏动作名称
            public bool ShowActionIconShadow { get; set; } // 动作图标显示阴影
        }
    }
}