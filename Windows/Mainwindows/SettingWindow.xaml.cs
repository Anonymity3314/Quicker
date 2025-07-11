using Quicker.UserControls.SettingWindow.BasicSettings;
using System.Windows.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Managers;
using Microsoft.Win32;
using System.Windows;

namespace Quicker.Windows.MainWindows
{
    public partial class SettingWindow : Window
    {
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮类型1颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮类型1颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮类型2颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮类型2颜色

        private readonly DispatcherTimer _settingsChangeTimer = new(); // 设置变化检测定时器
        public readonly SettingManager _settingManager = new(); // 设置管理器

        public SettingWindow()
        {
            InitializeComponent(); // 初始化xaml文件
            
            // 初始化设置变化检测定时器
            _settingsChangeTimer.Interval = TimeSpan.FromMilliseconds(100); // 设置检查间隔为100毫秒
            _settingsChangeTimer.Tick += SettingsChangeTimer_Tick;
        }

        #region 窗口生命周期

        // 初始化窗口
        private async void SettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _settingManager.CacheOriginalSettingsAsync(); // 缓存原始设置
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            if (Convention.RememberLastPage)
                SetLastPage(Convention.LastPage); // 设置上一次关闭时的状态
            else
            {
                BasicSettings_Click(null, null); // 显示常规设置面板
                Convention_Click(null, null); // 显示常规设置
            }
            
            // 初始化时隐藏撤销按钮，因为还没有修改
            CancelSettingsButton.Visibility = Visibility.Hidden;
            
            // 启动设置变化检测定时器
            _settingsChangeTimer.Start();
        }

        // 设置变化检测定时器回调
        private void SettingsChangeTimer_Tick(object sender, EventArgs e)
        {
            // 检查设置是否变化，更新撤销按钮可见性
            UpdateCancelButtonVisibility();
        }

        // 更新撤销按钮可见性
        private void UpdateCancelButtonVisibility()
        {
            CancelSettingsButton.Visibility = _settingManager.IsSettingsChanged() 
                ? Visibility.Visible
                : Visibility.Hidden; // 如果设置已变化，显示撤销按钮，否则隐藏
        }

        // 关闭窗口时保存最后打开的页面
        protected override void OnClosing(CancelEventArgs e)
        {
            // 停止设置变化检测定时器
            _settingsChangeTimer.Stop();
            
            base.OnClosing(e);
            SetLastPage(); // 保存最后打开的页面
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            // 停止并清理定时器资源
            _settingsChangeTimer.Stop();
            _settingsChangeTimer.Tick -= SettingsChangeTimer_Tick;

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

        #endregion

        #region 设置保存与撤销

        // 撤销修改
        private void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.RestoreOriginalSettings(); // 恢复原始设置
            RefreshCurrentSettingsDisplay(); // 刷新当前显示的界面
            CancelSettingsButton.Visibility = Visibility.Hidden; // 设置已恢复，隐藏撤销按钮
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
                    toast1.Show("设置应用失败！", "Error"); // 弹出消息提醒
                }

                // 显示设置成功通知
                string message = succeed ? "设置应用成功！" : "设置开机自启动失败！";
                using var toast2 = new ToastManager(); // 消息提醒管理器
                toast2.Show(message, "Common"); // 弹出消息提醒
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

        #endregion

        #region 页面导航与管理

        #region 页面加载与状态记忆

        /// <summary>
        /// 设置成上一次关闭时的状态
        /// </summary>
        /// <param name="page"> 界面数据 </param>
        private void SetLastPage(int page)
        {
            LoadPage1(page / 100); // 计算第几组按钮
            LoadPage2(page % 100 / 10); // 计算第几个按钮
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
                    BasicSettings_Click(null, null); // 点击基础设置按钮
                    break;
                case 2:
                    Auxiliary_Functions_Click(null, null); // 点击辅助功能按钮
                    break;
                case 3:
                    Tools_Click(null, null); // 点击工具按钮
                    break;
                default:
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
                default:
                    break;
            }
        }

        #endregion

        #region 页面状态保存

        // 保存最后打开的页面
        public void SetLastPage()
        {
            int lastpage = GetCurrentPageCode();
            SettingDatabase.RecordLastPage(lastpage); // 保存最后打开的页面
        }

        /// <summary>
        /// 获取当前页面代码
        /// </summary>
        /// <returns>页面代码</returns>
        private int GetCurrentPageCode()
        {
            UIElement childElement = GetMainContentElement();
            if (childElement == null) return 0;
            return GetPageCodeByElementType(childElement); // 检查子元素的类型并返回对应的代码
        }

        /// <summary>
        /// 获取主内容区域的元素
        /// </summary>
        /// <returns>主内容元素</returns>
        private UIElement GetMainContentElement()
        {
            foreach(UIElement child in ResultGrid.Children)
            {
                if (child is Rectangle || child is Grid) continue; // 跳过 Rectangle 和 Grid 元素
                return child; // 获取第一个子元素
            }
            return null;
        }

        /// <summary>
        /// 根据元素类型获取页面代码
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>页面代码</returns>
        private int GetPageCodeByElementType(UIElement element)
        {
            if (element is ConventionGrid)
            {
                return 111; // 常规设置页面
            }
            else if (element is OpenMainWindowGrid)
            {
                return 121; // 弹出面板设置页面
            }
            else if(element is BlacklistGrid)
            {
                return 131; // 黑名单设置页面
            }
            else if (element is AppearanceGrid)
            {
                return 141; // 外观设置页面
            }
            else if (element is AboutQuickerGrid)
            {
                var grid = FindGridByName(element, "Privacy_StatementButtonGrid");
                if (grid != null && grid.Visibility == Visibility.Visible)
                {
                    return 152; // 隐私声明页面
                }
                return 151; // 关于Quicker页面
            }
            return 0;
        }

        #endregion

        #region 界面刷新与更新

        /// <summary>
        /// 刷新当前显示的设置界面
        /// </summary>
        private void RefreshCurrentSettingsDisplay()
        {
            // 获取当前显示的设置界面
            UIElement childElement = GetMainContentElement();
            if (childElement == null) return;

            // 根据当前显示的界面类型刷新显示
            ResultGrid.Children.Remove(childElement);
            ReopenCurrentPage(childElement);
        }

        /// <summary>
        /// 重新打开当前页面
        /// </summary>
        /// <param name="currentElement">当前页面元素</param>
        private void ReopenCurrentPage(UIElement currentElement)
        {
            if (currentElement is ConventionGrid)
            {
                Convention_Click(null, null);
            }
            else if (currentElement is OpenMainWindowGrid)
            {
                OpenMainWindow_Click(null, null);
            }
            else if (currentElement is BlacklistGrid)
            {
                Blacklist_Click(null, null);
            }
            else if (currentElement is AppearanceGrid)
            {
                Appearance_Click(null, null);
            }
            else if (currentElement is AboutQuickerGrid)
            {
                AboutQuicker_Click(null, null);
            }
        }

        #endregion

        #region 辅助工具方法

        /// <summary>
        /// 递归查找名为指定名称的 Grid
        /// </summary>
        /// <param name="parent"> 父级元素 </param>
        /// <param name="name"> Grid 名称 </param>
        /// <returns> 名为指定名称的 Grid </returns>
        private Grid FindGridByName(DependencyObject parent, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i); // 获取子元素
                if (child is Grid grid && grid.Name == name)
                {
                    return grid; // 找到 Grid 元素
                }
                var result = FindGridByName(child, name); // 递归查找
                if (result != null)
                {
                    return result; // 找到 Grid 元素
                }
            }
            return null; // 没找到 Grid 元素
        }

        #endregion

        #endregion

        #region 基础设置按钮事件

        // 基础设置
        private void BasicSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.ButtonStyle1_Click(BasicSettingsStackPanel, BasicSettings, MainStackPanel, MenuGrid); // 设置Button类型1样式
        }
        // 鼠标移出Button恢复Background
        private void BasicSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            _settingManager.ButtonStyle1_MouseLeave(sender, BasicSettingsStackPanel); // 鼠标移出Button恢复Background
        }


        // 基础设置-常规
        private void Convention_Click(object sender, RoutedEventArgs e)
        {
            var ConventionGrid = new ConventionGrid(this) { Name = "ConventionGrid" }; // 创建常规设置Grid            
            _settingManager.ButtonStyle2_Click(Convention, BasicSettingsStackPanel, ConventionGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Visible; // 设置SettingButtonsGrid可见性
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
            _settingManager.ButtonStyle2_Click(OpenMainWindow, BasicSettingsStackPanel, OpenMainWindowGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Visible; // 设置SettingButtonsGrid可见性
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
            _settingManager.ButtonStyle2_Click(Blacklist, BasicSettingsStackPanel, BlacklistGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Visible; // 设置SettingButtonsGrid可见性
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
            _settingManager.ButtonStyle2_Click(Appearance, BasicSettingsStackPanel, AppearanceGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Visible; // 设置SettingButtonsGrid可见性
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
            _settingManager.ButtonStyle2_Click(AboutQuicker, BasicSettingsStackPanel, AboutQuickerGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Collapsed; // 设置SettingButtonsGrid可见性
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            if (Convention.RememberLastPage && Convention.LastPage % 10 == 2) 
            {
                AboutQuickerGrid.Privacy_StatementButton_Click(null, null); // 显示隐私声明
            }
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid(this) { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, AboutQuickerGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        #endregion

        #region 辅助功能和工具按钮事件

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

        #endregion
    }
}