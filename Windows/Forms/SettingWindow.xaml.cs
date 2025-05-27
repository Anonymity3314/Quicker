using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Shapes;
using Quicker.UserControls;
using System.Windows.Input;
using Quicker.Database;
using Quicker.Managers;
using Microsoft.Win32;
using System.Windows;

namespace Quicker.Windows.Forms
{
    public partial class SettingWindow : Window
    {
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮类型1颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮类型1颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮类型2颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮类型2颜色

        public readonly SettingManager _settingManager = new(); // 设置管理器

        public SettingWindow()
        {
            InitializeComponent(); // 初始化xaml文件
        }

        // 初始化窗口
        private void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            if (Convention.RememberLastPage)
                SetLastPage(Convention.LastPage); // 设置上一次关闭时的状态
            else
            {
                BasicSetting_Click(null, null); // 显示常规设置面板
                Convention_Click(null, null); // 显示常规设置
            }
        }

        /// <summary>
        /// 设置成上一次关闭时的状态
        /// </summary>
        /// <param name="page"> </param>
        private void SetLastPage(int page)
        {
            LoadPage1(page / 10); // 计算第几组按钮
            LoadPage2(page % 10); // 计算第几个按钮
        }

        /// <summary>
        /// 加载第一组按钮
        /// </summary>
        /// <param name="num1"> 第几组按钮 </param>
        private void LoadPage1(int num1)
        {
            switch (num1)
            {
                case 1:
                    BasicSetting_Click(null, null); // 点击基础设置按钮
                    break;
                case 2:
                    Auxiliary_Functions_Click(null, null); // 点击辅助功能按钮
                    break;
                case 3:
                    Tools_Click(null, null); // 点击工具按钮
                    break;
            }
        }

        /// <summary>
        /// 加载第二组按钮
        /// </summary>
        /// <param name="num2"> 第几组按钮 </param>
        private void LoadPage2(int num2)
        {
            switch (num2)
            {
                case 1:
                    Convention_Click(null, null); // 点击常规设置按钮
                    break;
                case 2:
                    OpenMainWindow_Click(null, null); // 点击弹出面板设置按钮
                    break;
                case 3:
                    Blacklist_Click(null, null); // 点击黑名单设置按钮
                    break;
                case 4:
                    Appearance_Click(null, null); // 点击外观设置按钮
                    break;
                case 5:
                    AboutQuicker_Click(null, null); // 点击关于按钮
                    break;
            }
        }

        // 应用设置
        private async void ApplySettings(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => // 在后台线程中执行保存操作
            {
                bool succeed = true; // 保存成功标志
                var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
                bool originalAutoStart = Convention.AutoStart; // 保存原始的开机自启动设置
                bool newAutoStart = _settingManager.conventions.AutoStart; // 新的开机自启动设置
                if (originalAutoStart != newAutoStart)// 更新开机自启动设置
                    succeed = UpdateAutostart(newAutoStart);

                if (_settingManager.conventions != null)
                    SettingDatabase.ApplyConventionSettings(
                        succeed
                            ? _settingManager.conventions.AutoStart
                            : Convention.AutoStart,
                        _settingManager.conventions.ShowNotification,
                        _settingManager.conventions.ShowAddImage,
                        _settingManager.conventions.HideTooltip,
                        _settingManager.conventions.LongPressThreshold,
                        _settingManager.conventions.MouseMovePixels,
                        _settingManager.conventions.LoopPageFlipping,
                        _settingManager.conventions.RememberLastPage,
                        _settingManager.conventions.EnableMemoryOptimization); // 更新常规设置
                if (_settingManager.openMainWindowConditions != null)
                    SettingDatabase.ApplyOpenMainWindowSettings(
                        _settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClick,
                    _settingManager.openMainWindowConditions.OpenMainWindowByX1MouseClick,
                    _settingManager.openMainWindowConditions.OpenMainWindowByX2MouseClick,
                    _settingManager.openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick,
                    _settingManager.openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick,
                    _settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger,
                    _settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClickLonger,
                    _settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClick_Move,
                    _settingManager.openMainWindowConditions.OpenMainWindowByCtrl,
                    _settingManager.openMainWindowConditions.WindowStartupLocation
                    ); // 更新弹出面板设置
                if (_settingManager.blacklistSettings != null)
                    SettingDatabase.ApplyBlacklistSettings(
                        _settingManager.blacklistSettings.FullScreenDisable,
                        _settingManager.blacklistSettings.ApplyBlacklistToExpandHotkeys); // 更新黑名单设置

                try
                {
                    AppStateManager.LoadSettings(); // 刷新弹出面板设置
                }
                catch
                {
                    using var toast1 = new ToastManager(); // 消息提醒管理器
                    toast1.ShowToast("设置应用失败！", "Error"); // 弹出消息提醒
                }

                // 显示设置成功通知
                string message = succeed ? "设置应用成功！" : "设置开机自启动失败！";
                using var toast2 = new ToastManager(); // 消息提醒管理器
                toast2.ShowToast(message, "Common"); // 弹出消息提醒
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
                        if (autostart)
                            localMachine.SetValue(keyName, appPath); // 设置开机自启动
                        else
                            localMachine.DeleteValue(keyName, false); // 移除开机自启动
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
            _settingManager.ButtonStyle1_Click(BasicSettingStackPanel, BasicSetting, MainStackPanel, MenuGrid); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void BasicSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            _settingManager.ButtonStyle1_MouseLeave(sender, BasicSettingStackPanel); // 鼠标移出Button恢复Background
        }


        // 基础设置-常规
        private void Convention_Click(object sender, RoutedEventArgs e)
        {
            var ConventionGrid = new ConventionGrid(this) { Name = "ConventionGrid" }; // 创建常规设置Grid            
            _settingManager.ButtonStyle2_Click(Convention, BasicSettingStackPanel, ConventionGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Convention_MouseLeave(object sender, MouseEventArgs e)
        {
            var ConventionGrid = new ConventionGrid(this) { Name = "ConventionGrid" }; // 创建常规设置Grid  
            _settingManager.ButtonStyle2_MouseLeave(sender, ConventionGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-弹出面板
        private void OpenMainWindow_Click(object sender, RoutedEventArgs e)
        {
            var OpenMainWindowGrid = new OpenMainWindowGrid(this) { Name = "OpenMainWindowGrid" }; // 创建弹出面板设置Grid
            _settingManager.ButtonStyle2_Click(OpenMainWindow, BasicSettingStackPanel, OpenMainWindowGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void OpenMainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var OpenMainWindowGrid = new OpenMainWindowGrid(this) { Name = "OpenMainWindowGrid" }; // 创建弹出面板设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, OpenMainWindowGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-黑名单
        private void Blacklist_Click(object sender, RoutedEventArgs e)
        {
            var BlacklistGrid = new BlacklistGrid(this) { Name = "BlacklistGrid" }; // 创建黑名单设置Grid
            _settingManager.ButtonStyle2_Click(Blacklist, BasicSettingStackPanel, BlacklistGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Blacklist_MouseLeave(object sender, MouseEventArgs e)
        {
            var BlacklistGrid = new BlacklistGrid(this) { Name = "BlacklistGrid" }; // 创建黑名单设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, BlacklistGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-外观
        private void Appearance_Click(object sender, RoutedEventArgs e)
        {
            var AppearanceGrid = new AppearanceGrid(this) { Name = "AppearanceGrid" }; // 创建外观设置Grid
            _settingManager.ButtonStyle2_Click(Appearance, BasicSettingStackPanel, AppearanceGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Visible; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void Appearance_MouseLeave(object sender, MouseEventArgs e)
        {
            var AppearanceGrid = new AppearanceGrid(this) { Name = "AppearanceGrid" }; // 创建外观设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, AppearanceGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-关于Quicker
        private void AboutQuicker_Click(object sender, RoutedEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid(this) { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            _settingManager.ButtonStyle2_Click(AboutQuicker, BasicSettingStackPanel, AboutQuickerGrid, ResultGrid); // 设置Button类型2样式
            ApplySettingsButton.Visibility = Visibility.Collapsed; // 设置ApplySettingsButton可见性
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid(this) { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, AboutQuickerGrid, ResultGrid); // 鼠标移出Button恢复Background
        }


        // 辅助功能
        private void Auxiliary_Functions_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.ButtonStyle1_Click(Auxiliary_FunctionsStackPanel, Auxiliary_Functions, MainStackPanel, MenuGrid); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void Auxiliary_Functions_MouseLeave(object sender, MouseEventArgs e)
        {
            _settingManager.ButtonStyle1_MouseLeave(sender, Auxiliary_FunctionsStackPanel); // 鼠标移出Button恢复Background
        }


        // 工具
        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.ButtonStyle1_Click(ToolsStackPanel, Tools, MainStackPanel, MenuGrid); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void Tools_MouseLeave(object sender, MouseEventArgs e)
        {
            _settingManager.ButtonStyle1_MouseLeave(sender, ToolsStackPanel); // 鼠标移出Button恢复Background
        }

        // 关闭窗口时保存最后打开的页面
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SetLastPage(); // 保存最后打开的页面
        }

        // 保存最后打开的页面
        public void SetLastPage()
        {
            int lastpage = 0;
            UIElement childElement = null;
            foreach(UIElement child in ResultGrid.Children)
            {
                if (child is Rectangle || child is Button) continue; // 跳过 Rectangle 和 Button 元素
                childElement = child; // 获取第一个子元素
                break;
            }
            // 检查子元素的类型
            if (childElement is ConventionGrid)
            {
                lastpage = 11; // 常规设置页面
            }
            else if (childElement is OpenMainWindowGrid)
            {
                lastpage = 12; // 弹出面板设置页面
            }
            else if(childElement is BlacklistGrid)
            {
                lastpage = 13; // 黑名单设置页面
            }
            else if (childElement is AppearanceGrid)
            {
                lastpage = 14; // 外观设置页面
            }
            else if (childElement is AboutQuickerGrid)
            {
                lastpage = 15; // 关于Quicker页面
            }
            SettingDatabase.RecordLastPage(lastpage); // 保存最后打开的页面
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            _settingManager.Dispose(); // 清空缓存

            foreach (var child in MenuGrid.Children.OfType<StackPanel>()) // 清理用户控件
            {
                child.Children.Clear(); // 清空 StackPanel 的子元素
            }
            ResultGrid.Children.Clear(); // 清空 ResultGrid 的子元素

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}