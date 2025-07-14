using Quicker.Models.Settings;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Diagnostics;
using Quicker.Database;
using System.Windows;

namespace Quicker.Managers
{
    public class SettingManager : IDisposable
    {
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮颜色

        public OpenMainWindow openMainWindowConditions; // 弹出面板设置缓存对象
        public Appearance appearanceConditions; // 外观设置缓存对象
        public Blacklist blacklistSettings; // 黑名单设置缓存对象
        public Convention conventions; // 常规设置缓存对象

        // 原始设置缓存
        private Convention _originalConventions;
        private OpenMainWindow _originalOpenMainWindowConditions;
        private Blacklist _originalBlacklistSettings;
        private Appearance _originalAppearanceConditions;

        /// <summary>
        /// 缓存窗口加载时的原始设置
        /// </summary>
        public async Task CacheOriginalSettingsAsync()
        {
            // 确保设置已加载
            await LoadConventionsAsync(); // 加载常规设置
            await LoadOpenMainWindowConditionsAsync(); // 加载弹出面板设置
            await LoadBlacklistSettingsAsync(); // 加载黑名单设置
            await LoadAppearanceAsync(); // 加载外观设置

            // 复制当前设置作为原始设置
            _originalConventions = CloneSettingsCache(conventions); // 复制常规设置
            _originalOpenMainWindowConditions = CloneSettingsCache(openMainWindowConditions); // 复制弹出面板设置
            _originalBlacklistSettings = CloneSettingsCache(blacklistSettings); // 复制黑名单设置
            _originalAppearanceConditions = CloneSettingsCache(appearanceConditions); // 复制外观设置
        }

        /// <summary>
        /// 恢复到原始设置
        /// </summary>
        public void RestoreOriginalSettings()
        {
            // 恢复原始设置
            if (_originalConventions != null)
                conventions = CloneSettingsCache(_originalConventions);
            
            if (_originalOpenMainWindowConditions != null)
                openMainWindowConditions = CloneSettingsCache(_originalOpenMainWindowConditions);
            
            if (_originalBlacklistSettings != null)
                blacklistSettings = CloneSettingsCache(_originalBlacklistSettings);
            
            if (_originalAppearanceConditions != null)
                appearanceConditions = CloneSettingsCache(_originalAppearanceConditions);
        }

        /// <summary>
        /// 克隆设置缓存对象
        /// </summary>
        /// <typeparam name="T">设置缓存类型</typeparam>
        /// <param name="source">源设置对象</param>
        /// <returns>克隆的设置对象</returns>
        private T CloneSettingsCache<T>(T source) where T : class
        {
            if (source == null) return null; // 如果源设置对象为空，返回空
            var clone = Activator.CreateInstance<T>(); // 创建新的设置对象
            var properties = typeof(T).GetProperties(); // 获取设置对象的属性
            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(source); // 获取源设置对象的属性值
                    property.SetValue(clone, value); // 设置新设置对象的属性值
                }
            }
            return clone; // 返回克隆的设置对象
        }

        // 异步加载常规设置信息
        public async Task LoadConventionsAsync()
        {
            if (conventions != null) return; // 如果已经初始化了数据，直接返回
            var Conventions = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            conventions = new Convention // 常规设置
            {
                Version = Conventions.Version, // 版本号
                AutoStart = Conventions.AutoStart, // 开机自启
                ShowNotification = Conventions.ShowNotification, // 显示通知
                ShowAddImage = Conventions.ShowAddImage, // 显示添加图片按钮
                HideTooltip = Conventions.HideTooltip, // 隐藏工具提示
                LongPressThreshold = Conventions.LongPressThreshold, // 长按阈值
                MouseMovePixels = Conventions.MouseMovePixels, // 鼠标移动像素
                LoopPageFlipping = Conventions.LoopPageFlipping, // 循环翻页
                RememberLastPage = Conventions.RememberLastPage, // 是否记住设置窗口中最后打开的页面
                LastPage = Conventions.LastPage, // 最后打开的页面
                EnableMemoryOptimization = Conventions.EnableMemoryOptimization, // 是否启用内存优化
            }; // 加载设置数据到缓存
        }

        // 异步加载弹出面板设置信息
        public async Task LoadOpenMainWindowConditionsAsync()
        {
            if (openMainWindowConditions != null) return; // 如果已经初始化了数据，直接返回
            var OpenMainWindowConditions = SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取弹出面板设置信息
            openMainWindowConditions = new OpenMainWindow // 弹出面板设置
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
            var BlacklistSettings = SettingDatabase.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置信息
            blacklistSettings = new Blacklist // 黑名单设置
            {
                IsFullScreenDisabled = BlacklistSettings.IsFullScreenDisabled, // 启用黑名单
                IsBlacklistEnabledForExtendedHotkey = BlacklistSettings.IsBlacklistEnabledForExtendedHotkey // 是否将黑名单与全屏禁用设置应用于扩展热键功能
            }; // 加载设置数据到缓存
        }

        // 异步加载外观设置信息
        public async Task LoadAppearanceAsync()
        {
            if (appearanceConditions != null) return; // 如果已经初始化了数据，直接返回
            var Appearance = SettingDatabase.GetAllAppearanceSettings().FirstOrDefault(); // 获取外观设置信息
            appearanceConditions = new Appearance // 外观设置
            {
                // 按钮
                ButtonSize = Appearance.ButtonSize, // 按钮大小
                ButtonGap = Appearance.ButtonGap, // 按钮间距
                BorderWidth = Appearance.BorderWidth, // 边框宽度
                ButtonCornerRadius = Appearance.ButtonCornerRadius, // 按钮圆角半径

                // 颜色
                BackgroundColor = Appearance.BackgroundColor, // 背景颜色
                BorderColor = Appearance.BorderColor, // 边框颜色
                ToolbarColor = Appearance.ToolbarColor, // 工具栏颜色
                ToolbarIconColor = Appearance.ToolbarIconColor, // 工具栏图标颜色
                ActionButtonColor = Appearance.ActionButtonColor, // 动作按钮颜色
                ActionButtonMouseOverColor = Appearance.ActionButtonMouseOverColor, // 动作按钮鼠标悬停颜色
                BlankButtonColor = Appearance.BlankButtonColor, // 空白按钮颜色
                BlankButtonMouseOverColor = Appearance.BlankButtonMouseOverColor, // 空白按钮鼠标悬停颜色
                ButtonTextColor = Appearance.ButtonTextColor, // 按钮文字颜色
                ActionIconColor = Appearance.ActionIconColor, // 动作图标颜色
                TriggerKeyTextColor = Appearance.TriggerKeyTextColor, // 触发键文字颜色
                OtherIconColor = Appearance.OtherIconColor, // 其他位置图标颜色

                // 字体
                Font1 = Appearance.Font1, // 字体1
                Font2 = Appearance.Font2, // 字体2
                FontSize = Appearance.FontSize, // 字体大小
                FontWeight = Appearance.FontWeight, // 字体粗细

                // 背景图片
                BackgroundImagePath = Appearance.BackgroundImagePath, // 背景图片路径
                BackgroundImageOpacity = Appearance.BackgroundImageOpacity, // 背景图片不透明度

                // 模糊与圆角
                Blur = Appearance.Blur, // 模糊模式
                Win11CornerRadius = Appearance.Win11CornerRadius, // Win11圆角模式

                // 选项
                AutoHideTitleBar = Appearance.AutoHideTitleBar, // 自动隐藏标题栏
                ShowActionButtonMouseOver = Appearance.ShowActionButtonMouseOver, // 鼠标悬停显示动作按钮
                HideActionNameAfterIcon = Appearance.HideActionNameAfterIcon, // 隐藏动作名称
                ShowActionIconShadow = Appearance.ShowActionIconShadow, // 显示动作图标阴影

                EnablePreview = Appearance.EnablePreview // 显示设置应用效果
            }; // 加载设置数据到缓存
        }

        // 设置Grid可见性
        public void SetGridVisible(Grid childrengrid, Grid fathergrid)
        {
            foreach (var grid in fathergrid.Children.OfType<Grid>())
                grid.Visibility = grid == childrengrid ? Visibility.Visible : Visibility.Collapsed; // 设置Grid可见性
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
                stackpanel.Visibility = stackpanel == targetStackPanel ? Visibility.Visible : Visibility.Collapsed; // 设置StackPanel可见性

            foreach (var button in fatherStackPanel.Children.OfType<Button>())
                button.Background = button == targetButton
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button类型1颜色
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
            foreach (var button in buttonPanelGrid.Children.OfType<Button>()) // 遍历Button容器中所有Button，设置边框
                button.BorderThickness = button == clickedButton ? new Thickness(0, 0, 0, 2) : new Thickness(0);
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
                case "RememberLastPageCheckBox":
                    conventions.RememberLastPage = isChecked == true;
                    break; // 记住设置窗口中最后打开的页面
                case "EnableMemoryOptimizationCheckBox":
                    conventions.EnableMemoryOptimization = isChecked == true;
                    break; // 启用内存优化
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
                    blacklistSettings.IsFullScreenDisabled = isChecked == true; // 全屏禁用Quicker
                    break; // 全屏禁用Quicker
                case "ApplyBlacklistToExpandHotkeysCheckBox":
                    blacklistSettings.IsBlacklistEnabledForExtendedHotkey = isChecked == true; // 黑名单应用到热键扩展
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

        /// <summary>
        /// 判断当前设置是否与原始设置相同
        /// </summary>
        /// <returns>是否相同</returns>
        public bool IsSettingsChanged()
        {
            // 判断常规设置是否变化
            if (_originalConventions != null && conventions != null && !AreSettingsEqual(_originalConventions, conventions))
                return true;
                
            // 判断弹出面板设置是否变化
            if (_originalOpenMainWindowConditions != null && openMainWindowConditions != null && 
                !AreSettingsEqual(_originalOpenMainWindowConditions, openMainWindowConditions))
                return true;
                
            // 判断黑名单设置是否变化
            if (_originalBlacklistSettings != null && blacklistSettings != null &&
                !AreSettingsEqual(_originalBlacklistSettings, blacklistSettings))
                return true;
                
            // 判断外观设置是否变化
            if (_originalAppearanceConditions != null && appearanceConditions != null &&
                !AreSettingsEqual(_originalAppearanceConditions, appearanceConditions))
                return true;
                
            return false;
        }
        
        /// <summary>
        /// 比较两个设置对象是否相等
        /// </summary>
        private bool AreSettingsEqual<T>(T obj1, T obj2) where T : class
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            
            var properties = typeof(T).GetProperties();
            
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value1 = property.GetValue(obj1);
                    var value2 = property.GetValue(obj2);
                    
                    if (value1 == null && value2 == null) continue;
                    if (value1 == null || value2 == null) return false;
                    
                    if (!value1.Equals(value2))
                        return false;
                }
            }
            
            return true;
        }

        #region IDisposable实现

        // 是否已释放资源标志
        private bool _disposed = false;

        // 手动释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // 告知垃圾回收器不需要调用终结器
        }

        // 释放资源
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; // 如果已经释放，直接返回
            if (disposing)
            {
                // 释放托管资源
                // 清除原始设置缓存
                _originalConventions = null;
                _originalOpenMainWindowConditions = null;
                _originalBlacklistSettings = null;
                _originalAppearanceConditions = null;
                
                // 清除当前设置缓存
                conventions = null;
                openMainWindowConditions = null;
                blacklistSettings = null;
                appearanceConditions = null;
            }

            // 释放非托管资源
            _disposed = true; // 标记为已释放
        }

        // 析构函数
        ~SettingManager()
        {
            Dispose(false);
        }

        #endregion
    }
}