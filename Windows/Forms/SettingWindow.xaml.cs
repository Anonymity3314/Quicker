using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Database;
using Quicker.Managers;
using Microsoft.Win32;
using System.Windows;

namespace Quicker.Windows
{
    public partial class SettingWindow : Window
    {
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮颜色

        private List<string> ShortcutKeys = new List<string>(); // 保存快捷键
        private readonly SettingDatabase db1; // 设置数据库
        SettingManager settingManager; // 设置管理器

        public SettingWindow()
        {
            db1 = new SettingDatabase(); // 创建设置数据库
            InitializeComponent(); // 初始化窗口
            settingManager = new SettingManager(); // 创建设置管理器
            InitializeWindow(); // 初始化窗口
        }

        // 设置StackPanel可见性
        private void SetStackPanelVisibility(StackPanel childrenstackpanel)
        {
            foreach (var stackpanel in MenuGrid.Children.OfType<StackPanel>())
            {
                stackpanel.Visibility = stackpanel == childrenstackpanel? Visibility.Visible: Visibility.Hidden; // 设置StackPanel可见性
            }
        }

        // 初始化窗口
        private async void InitializeWindow()
        {
            SetStackPanelVisibility(BasicSettingStackPanel); // 设置默认显示的StackPanel
            settingManager.ButtonStyle2_Click(Convention, BasicSettingStackPanel, ConventionGrid, ResultGrid); // 设置默认显示的Grid
        }

        // 加载常规设置信息
        private void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    AutoStartCheckBox.IsChecked = Conventions.AutoStart; // 加载开机自启动设置
            //    ShowNotificationCheckBox.IsChecked = Conventions.ShowNotification; // 加载显示启动完成提示设置
            //    ShowAddImageCheckBox.IsChecked = Conventions.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
            //    HideTooltipCheckBox.IsChecked = Conventions.HideTooltip; // 加载隐藏提示框设置
            //    LongPressThresholdTextBox.Text = Conventions.LongPressThreshold.ToString(); // 加载长按阈值设置
            //    MouseMovePixelsTextBox.Text = Conventions.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
            //    LoopPageFlippingCheckBox.IsChecked = Conventions.LoopPageFlipping; // 加载循环翻页设置
            //});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetStackPanel"> 目标StackPanel </param>
        /// <param name="targetButton"> 目标Button </param>
        /// <param name="fatherStackPanel"> 父级StackPanel </param>
        private void ButtonStyle1_Click(StackPanel targetStackPanel, Button targetButton, StackPanel fatherStackPanel)
        {
            SetStackPanelVisibility(targetStackPanel); // 设置StackPanel可见性
            foreach (var button in MainStackPanel.Children.OfType<Button>())
            {
                button.Background = button == targetButton ?
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(SelectedButtonColor1)) :
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultButtonColor1)); // 设置Button类型1颜色
            } // 设置Button类型1颜色
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
                bool newAutoStart = settingManager.settingsCache.AutoStart; // 新的开机自启动设置

                // 更新开机自启动设置
                if (originalAutoStart != newAutoStart)
                {
                    succeed = UpdateAutostart(newAutoStart);
                    if (!succeed)
                    {
                        // 更新失败，回退到原来的设置
                        settingManager.settingsCache.AutoStart = originalAutoStart;
                    }
                }

                // 更新数据库中的设置
                db1.ApplySettings(
                    settingManager.settingsCache.AutoStart,
                    settingManager.settingsCache.ShowNotification,
                    settingManager.settingsCache.ShowAddImage,
                    settingManager.settingsCache.HideTooltip,
                    settingManager.settingsCache.LongPressThreshold,
                    settingManager.settingsCache.MouseMovePixels,
                    settingManager.settingsCache.LoopPageFlipping,
                    settingManager.settingsCache.OpenMainWindowByMiddleMouseClick,
                    settingManager.settingsCache.OpenMainWindowByX1MouseClick,
                    settingManager.settingsCache.OpenMainWindowByX2MouseClick,
                    settingManager.settingsCache.OpenMainWindowByCtrl_MiddleMouseClick,
                    settingManager.settingsCache.OpenMainWindowByCtrl_RightMouseClick,
                    settingManager.settingsCache.OpenMainWindowByMiddleMouseClickLonger,
                    settingManager.settingsCache.OpenMainWindowByRightMouseClickLonger,
                    settingManager.settingsCache.OpenMainWindowByRightMouseClick_Move,
                    settingManager.settingsCache.OpenMainWindowByCtrl,
                    settingManager.settingsCache.WindowStartupLocation
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
                        if (autostart) localMachine.SetValue(keyName, appPath); // 设置开机自启动
                        else localMachine.DeleteValue(keyName, false); // 移除开机自启动
                    }
                    else return false; // 如果无法打开注册表，返回失败
                }
                return true; // 返回设置成功
            }
            catch { return false; } // 出现异常，返回失败
        }

        // 基础设置
        private void BasicSetting_Click(object sender, RoutedEventArgs e)
        {
            ButtonStyle1_Click(BasicSettingStackPanel, BasicSetting, MainStackPanel); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void BasicSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle1_MouseLeave(sender, BasicSettingStackPanel); // 鼠标移出Button恢复Background
        }


        // 基础设置-常规
        private void Convention_Click(object sender, RoutedEventArgs e)
        {
            settingManager.ButtonStyle2_Click(Convention, BasicSettingStackPanel, ConventionGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性

            //// 加载常规设置信息
            //AutoStartCheckBox.IsChecked = settingsCache.AutoStart; // 加载开机自启动设置
            //ShowNotificationCheckBox.IsChecked = settingsCache.ShowNotification; // 加载显示启动完成提示设置
            //ShowAddImageCheckBox.IsChecked = settingsCache.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
            //HideTooltipCheckBox.IsChecked = settingsCache.HideTooltip; // 加载隐藏提示框设置
            //LongPressThresholdTextBox.Text = settingsCache.LongPressThreshold.ToString(); // 加载长按阈值设置
            //MouseMovePixelsTextBox.Text = settingsCache.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
            //LoopPageFlippingCheckBox.IsChecked = settingsCache.LoopPageFlipping; // 加载循环翻页设置
        }
        // 鼠标移出Button恢复Background
        private void Convention_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle2_MouseLeave(sender, ConventionGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-弹出面板
        private void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            settingManager.ButtonStyle2_Click(OpenMainWindow, BasicSettingStackPanel, OpenMainWindowGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性

            // 重置测试Button
            //TestButton.Content = "按键测试区";

            // 加载勾选框
            //OpenMainWindowByMiddleMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByMiddleMouseClick; // 按下中键
            //OpenMainWindowByX1MouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByX1MouseClick; // 按下X1键
            //OpenMainWindowByX2MouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByX2MouseClick; // 按下X2键
            //OpenMainWindowByCtrl_MiddleMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl_MiddleMouseClick; // Ctrl+中键单击
            //OpenMainWindowByCtrl_RightMouseClickCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl_RightMouseClick; // Ctrl+右键单击
            //OpenMainWindowByMiddleMouseClickLongerCheckBox.IsChecked = settingsCache.OpenMainWindowByMiddleMouseClickLonger; // 长按中键
            //OpenMainWindowByRightMouseClickLongerCheckBox.IsChecked = settingsCache.OpenMainWindowByRightMouseClickLonger; // 长按右键
            //OpenMainWindowByRightMouseClick_MoveCheckBox.IsChecked = settingsCache.OpenMainWindowByRightMouseClick_Move; // 按右键移动
            //OpenMainWindowByCtrlCheckBox.IsChecked = settingsCache.OpenMainWindowByCtrl; // 单击Ctrl键
            //WindowStartupLocationComboBox.SelectedIndex = settingsCache.WindowStartupLocation; // 功能面板打开位置
        }

        // 鼠标移出Button恢复Background
        private void OpenMainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle2_MouseLeave(sender, OpenMainWindowGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-黑名单
        private void Blacklist_Click(object sender, RoutedEventArgs e)
        {
            settingManager.ButtonStyle2_Click(Blacklist, BasicSettingStackPanel, BlacklistGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Blacklist_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle2_MouseLeave(sender, BlacklistGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-外观
        private void Appearance_Click(object sender, RoutedEventArgs e)
        {
            settingManager.ButtonStyle2_Click(Appearance, BasicSettingStackPanel, AppearanceGrid, ResultGrid); // 设置Button类型2样式
            //SetGridVisible(AppearanceGrid, ResultGrid); // 设置Grid可见性
        }
        // 鼠标移出Button恢复Background
        private void Appearance_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle2_MouseLeave(sender, AppearanceGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-关于Quicker
        private void AboutQuicker_Click(object sender, RoutedEventArgs e)
        {
            settingManager.ButtonStyle2_Click(AboutQuicker, BasicSettingStackPanel, AboutQuickerGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Hidden; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle2_MouseLeave(sender, AboutQuickerGrid); // 鼠标移出Button恢复Background
        }

        // 辅助功能
        private void Auxiliary_Functions_Click(object sender, RoutedEventArgs e)
        {
            ButtonStyle1_Click(Auxiliary_FunctionsStackPanel, Auxiliary_Functions, MainStackPanel); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void Auxiliary_Functions_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle1_MouseLeave(sender, Auxiliary_FunctionsStackPanel); // 鼠标移出Button恢复Background
        }

        // 工具
        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            ButtonStyle1_Click(ToolsStackPanel, Tools, MainStackPanel); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void Tools_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle1_MouseLeave(sender, ToolsStackPanel); // 鼠标移出Button恢复Background
        }

        // 关闭窗口回收资源
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e); // 调用基类的OnClosing方法
            ConventionGrid.CleanUp(); // 清除缓存对象
            GC.Collect(); // 回收资源
        }
    }
}