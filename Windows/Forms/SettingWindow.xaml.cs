using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.UserControls;
using System.Windows.Input;
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
        public SettingManager settingManager; // 设置管理器

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
                stackpanel.Visibility = stackpanel == childrenstackpanel ? Visibility.Visible : Visibility.Hidden; // 设置StackPanel可见性
            }
        }

        // 初始化窗口
        private async void InitializeWindow()
        {
            SetStackPanelVisibility(BasicSettingStackPanel); // 设置默认显示的StackPanel
            UserControl ConventionGrid = new ConventionGrid { Name = "ConventionGrid" }; // 创建常规设置Grid
            settingManager.ButtonStyle2_Click(Convention, BasicSettingStackPanel, ConventionGrid, ResultGrid); // 设置默认显示的Grid
        }

        /// <summary>
        /// 设置Button类型1样式
        /// </summary>
        /// <param name="targetStackPanel"> 目标StackPanel </param>
        /// <param name="targetButton"> 目标Button </param>
        private void ButtonStyle1_Click(StackPanel targetStackPanel, Button targetButton)
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
                if (originalAutoStart != newAutoStart)// 更新开机自启动设置
                {
                    succeed = UpdateAutostart(newAutoStart);
                    if (!succeed)
                        settingManager.settingsCache.AutoStart = originalAutoStart; // 更新失败，回退到原来的设置
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
                new ToastContentBuilder().AddText(message).Show(); // 显示通知
            });
        }

        /// <summary>
        /// 更新开机自启动设置
        /// </summary>
        /// <param name="autostart"> 开机自启动设置 </param>
        /// <returns> 保存成功标志 </returns>
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
            ButtonStyle1_Click(BasicSettingStackPanel, BasicSetting); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void BasicSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle1_MouseLeave(sender, BasicSettingStackPanel); // 鼠标移出Button恢复Background
        }


        // 基础设置-常规
        private void Convention_Click(object sender, RoutedEventArgs e)
        {
            var ConventionGrid = new ConventionGrid { Name = "ConventionGrid" }; // 创建常规设置Grid            
            settingManager.ButtonStyle2_Click(Convention, BasicSettingStackPanel, ConventionGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Convention_MouseLeave(object sender, MouseEventArgs e)
        {
            var ConventionGrid = new ConventionGrid { Name = "ConventionGrid" }; // 创建常规设置Grid  
            settingManager.ButtonStyle2_MouseLeave(sender, ConventionGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-弹出面板
        private void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            var OpenMainWindowGrid = new OpenMainWindowGrid { Name = "OpenMainWindowGrid" }; // 创建弹出面板设置Grid
            settingManager.ButtonStyle2_Click(OpenMainWindow, BasicSettingStackPanel, OpenMainWindowGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void OpenMainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var OpenMainWindowGrid = new OpenMainWindowGrid { Name = "OpenMainWindowGrid" }; // 创建弹出面板设置Grid
            settingManager.ButtonStyle2_MouseLeave(sender, OpenMainWindowGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-黑名单
        private void Blacklist_Click(object sender, RoutedEventArgs e)
        {
            var BlacklistGrid = new BlacklistGrid { Name = "BlacklistGrid" }; // 创建黑名单设置Grid
            settingManager.ButtonStyle2_Click(Blacklist, BasicSettingStackPanel, BlacklistGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Blacklist_MouseLeave(object sender, MouseEventArgs e)
        {
            var BlacklistGrid = new BlacklistGrid { Name = "BlacklistGrid" }; // 创建黑名单设置Grid
            settingManager.ButtonStyle2_MouseLeave(sender, BlacklistGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-外观
        private void Appearance_Click(object sender, RoutedEventArgs e)
        {
            var AppearanceGrid = new AppearanceGrid { Name = "AppearanceGrid" }; // 创建外观设置Grid
            settingManager.ButtonStyle2_Click(Appearance, BasicSettingStackPanel, AppearanceGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Appearance_MouseLeave(object sender, MouseEventArgs e)
        {
            var AppearanceGrid = new AppearanceGrid { Name = "AppearanceGrid" }; // 创建外观设置Grid
            settingManager.ButtonStyle2_MouseLeave(sender, AppearanceGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-关于Quicker
        private void AboutQuicker_Click(object sender, RoutedEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            settingManager.ButtonStyle2_Click(AboutQuicker, BasicSettingStackPanel, AboutQuickerGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Hidden; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            settingManager.ButtonStyle2_MouseLeave(sender, AboutQuickerGrid, ResultGrid); // 鼠标移出Button恢复Background
        }


        // 辅助功能
        private void Auxiliary_Functions_Click(object sender, RoutedEventArgs e)
        {
            ButtonStyle1_Click(Auxiliary_FunctionsStackPanel, Auxiliary_Functions); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void Auxiliary_Functions_MouseLeave(object sender, MouseEventArgs e)
        {
            settingManager.ButtonStyle1_MouseLeave(sender, Auxiliary_FunctionsStackPanel); // 鼠标移出Button恢复Background
        }


        // 工具
        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            ButtonStyle1_Click(ToolsStackPanel, Tools); // 设置Button类型1样式
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
            settingManager.ClearCache(); // 清空缓存
            GC.Collect(); // 回收资源
        }
    }
}