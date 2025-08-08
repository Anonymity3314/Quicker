using VisualTreeHelper = Quicker.Helpers.VisualTreeHelper;
using Quicker.UserControls.SettingWindow.BasicSettings;
using Quicker.UserControls.SettingWindow.Tools;
using System.Windows.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Shapes;
using Quicker.Database.Core;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Reflection;
using Quicker.Managers;
using Microsoft.Win32;
using Quicker.Helpers;
using System.Windows;
using System.IO;

namespace Quicker.Windows.MainWindows
{
    public partial class SettingWindow : Window
    {
        private const string DefaultButtonColor1 = "#FFE0E0E0"; // 默认按钮类型1颜色
        private const string SelectedButtonColor1 = "#FFF4F4F4"; // 选中按钮类型1颜色
        private const string DefaultButtonColor2 = "#FFF0F0F0"; // 默认按钮类型2颜色
        private const string SelectedButtonColor2 = "#FFFAFAFA"; // 选中按钮类型2颜色

        public readonly SettingManager _settingManager = new(); // 设置管理器
        private bool _hasAppliedSettings = false; // 标记是否已经应用过设置

        public SettingWindow()
        {
            InitializeComponent(); // 初始化xaml文件
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
        }

        // 关闭窗口时保存最后打开的页面
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SetLastPage(); // 保存最后打开的页面
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

        #endregion

        #region 设置保存与撤销

        // 撤销修改
        private async void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.RestoreOriginalSettings(); // 恢复原始设置
            RefreshCurrentSettingsDisplay(); // 刷新当前显示的界面
            await SaveSettingsAsync(); // 只保存设置，不显示信息
            CancelSettingsButton.Visibility = Visibility.Hidden; // 设置已恢复，隐藏撤销按钮
        }

        // 应用设置
        private async void ApplySettings(object sender, RoutedEventArgs e)
        {
            _hasAppliedSettings = true; // 标记已经应用过设置
            if (_settingManager.IsSettingsChanged()) // 检查是否需要显示撤销按钮
            {
                CancelSettingsButton.Visibility = Visibility.Visible;
            }

            var result = await SaveSettingsAsync(); // 保存设置并获取结果
            ShowSaveResultMessage(result); // 显示保存结果信息

            // 保存设置后刷新托盘图标
            RefreshTrayIconForBothStates();
        }

        /// <summary>
        /// 异步保存设置
        /// </summary>
        /// <returns>保存结果信息</returns>
        private async Task<(bool autostartSuccess, bool settingsLoadSuccess)> SaveSettingsAsync()
        {
            return await Task.Run(() =>
            {
                bool autostartSuccess = true; // 开机自启动设置成功标志
                var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
                bool originalAutoStart = Convention.AutoStart; // 保存原始的开机自启动设置
                bool newAutoStart = _settingManager.conventions.AutoStart; // 新的开机自启动设置
                if (originalAutoStart != newAutoStart)// 更新开机自启动设置
                    autostartSuccess = UpdateAutostart(newAutoStart);

                if (_settingManager.conventions != null)
                    SettingDatabase.ApplyConventionSettings(
                        autostartSuccess
                            ? _settingManager.conventions.AutoStart
                            : Convention.AutoStart,
                        _settingManager.conventions.ShowNotification,
                        _settingManager.conventions.ShowAddImage,
                        _settingManager.conventions.HideTooltip,
                        _settingManager.conventions.LongPressThreshold,
                        _settingManager.conventions.MouseMovePixels,
                        _settingManager.conventions.LoopPageFlipping,
                        _settingManager.conventions.RememberLastPage,
                        _settingManager.conventions.EnableMemoryOptimization,
                        _settingManager.conventions.TrayIconPathRunning,
                        _settingManager.conventions.TrayIconPathPaused
                    ); // 更新常规设置
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
                        _settingManager.blacklistSettings.IsFullScreenDisabled,
                        _settingManager.blacklistSettings.IsBlacklistEnabledForExtendedHotkey
                    ); // 更新黑名单设置

                bool settingsLoadSuccess = true; // 设置加载成功标志
                try
                {
                    AppStateManager.LoadSettings(); // 刷新弹出面板设置
                }
                catch
                {
                    settingsLoadSuccess = false; // 设置加载失败
                }

                return (autostartSuccess, settingsLoadSuccess);
            });
        }

        /// <summary>
        /// 显示保存结果消息
        /// </summary>
        /// <param name="result">保存结果</param>
        private void ShowSaveResultMessage((bool autostartSuccess, bool settingsLoadSuccess) result)
        {
            string message;
            if (!result.autostartSuccess)
            {
                message = "设置开机自启动失败！";
            }
            else if (!result.settingsLoadSuccess)
            {
                message = "设置应用失败！";
            }
            else
            {
                message = "已保存设置！";
            }
            
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show(message, ToastType.Common); // 弹出消息提醒
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
                // 获取应用程序可执行文件路径
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                string keyName = "Quicker"; // 注册表中的键名            
                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; // 获取注册表路径

                // 使用HKEY_CURRENT_USER，不需要管理员权限
                using (RegistryKey currentUser = Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (currentUser != null)
                    {
                        if (autostart)
                        {
                            // 检查路径是否有效
                            if (File.Exists(appPath))
                            {
                                currentUser.SetValue(keyName, appPath); // 设置开机自启动
                            }
                            else
                            {
                                // 如果主模块路径不存在，尝试使用程序集路径
                                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                                if (File.Exists(assemblyPath))
                                {
                                    currentUser.SetValue(keyName, assemblyPath);
                                }
                                else
                                {
                                    return false; // 无法找到有效的应用程序路径
                                }
                            }
                        }
                        else
                        {
                            currentUser.DeleteValue(keyName, false); // 移除开机自启动
                        }
                    }
                    else 
                    {
                        return false; // 如果无法打开注册表，返回失败
                    }
                }
                return true; // 返回设置成功
            }
            catch (Exception ex)
            {
                return false; // 出现异常，返回失败
            }
        }

        /// <summary>
        /// 刷新两种状态下的托盘图标（运行/暂停）
        /// </summary>
        public void RefreshTrayIconForBothStates()
        {
            var conventions = _settingManager.conventions;
            Quicker.Managers.AppStateManager.NotifyTrayIconChanged(
                conventions.TrayIconPathRunning,
                conventions.TrayIconPathPaused
            );
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
            switch (page / 100)
            {
                case 1:
                    BasicSettings_Click(null, null); // 点击基础设置按钮
                    LoadBasicSettingPages(page % 100 / 10); // 计算第几个按钮
                    break;
                case 2:
                    Auxiliary_Functions_Click(null, null); // 点击辅助功能按钮
                    break;
                case 3:
                    Tools_Click(null, null); // 点击工具按钮
                    LoadToolPages(page % 100 / 10); // 计算第几个按钮
                    break;
                default:
                    BasicSettings_Click(null, null); // 点击基础设置按钮
                    LoadBasicSettingPages(1); // 显示第一个按钮
                    break;
            }
        }

        /// <summary>
        /// 加载第二组按钮
        /// </summary>
        /// <param name="num2"> 第几组按钮 </param>
        private void LoadBasicSettingPages(int num2)
        {
            switch (num2)
            {
                case 1:
                    Convention_Click(null, null);
                    break; // 点击常规设置按钮
                case 2:
                    OpenMainWindow_Click(null, null);
                    break; // 点击弹出面板设置按钮
                case 3:
                    FunctionShortcutKeys_Click(null, null);
                    break; // 点击功能快捷键设置按钮
                case 4:
                    Blacklist_Click(null, null);
                    break; // 点击黑名单设置按钮
                case 5:
                    Appearance_Click(null, null);
                    break; // 点击外观设置按钮
                case 6:
                    AboutQuicker_Click(null, null);
                    break; // 点击关于按钮
                default:
                    Convention_Click(null, null);
                    break; // 点击常规设置按钮
            }
        }

        /// <summary>
        /// 加载第二组按钮
        /// </summary>
        /// <param name="num2"> 第几组按钮 </param>
        private void LoadToolPages(int num2)
        {
            switch (num2)
            {
                case 1:
                    ManageExtensions_Click(null, null);
                    break; // 点击扩展管理按钮
                default:
                    ManageExtensions_Click(null, null);
                    break; // 点击扩展管理按钮
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
            else if(element is FunctionShortcutKeysGrid)
            {
                return 131; // 功能快捷键设置界面
            }
            else if (element is BlacklistGrid)
            {
                return 141; // 黑名单设置页面
            }
            else if (element is AppearanceGrid)
            {
                return 151; // 外观设置页面
            }
            else if (element is AboutQuickerGrid)
            {
                var grid = VisualTreeHelper.FindGridByName(element, "Privacy_StatementButtonGrid");
                if (grid != null && grid.Visibility == Visibility.Visible)
                {
                    return 162; // 隐私声明页面
                }
                return 161; // 关于Quicker页面
            }
            else if (element is ExtensionManagementGrid)
            {
                return 311; // 扩展管理页面
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
        /// 通用打开自定义控件方法（辅助工具方法）
        /// </summary>
        /// <typeparam name="T">控件类型（如 ConventionGrid）</typeparam>
        /// <param name="button">按钮</param>
        /// <param name="stackPanel">StackPanel</param>
        /// <param name="resultGrid">ResultGrid</param>
        /// <param name="settingButtonsGridVisibility">SettingButtonsGrid 的可见性</param>
        private void OpenCustomGrid(Button button, UserControl grid, Grid resultGrid, Visibility settingButtonsGridVisibility = Visibility.Visible)
        {
            if (grid is FrameworkElement fe) // 如果grid是FrameworkElement类型
                fe.Name = grid.GetType().Name; // 设置grid的名称
            _settingManager.ButtonStyle2_Click(button, grid, resultGrid, MenuGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = settingButtonsGridVisibility; // 设置SettingButtonsGrid的可见性
        }

        #endregion

        #endregion

        #region 基础设置

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
            OpenCustomGrid(Convention, new ConventionGrid(this), ResultGrid);
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
            OpenCustomGrid(OpenMainWindow, OpenMainWindowGrid, ResultGrid); // 设置Button类型2样式
            SettingButtonsGrid.Visibility = Visibility.Visible; // 设置SettingButtonsGrid可见性
        }
        // 鼠标移出Button恢复Background
        private void OpenMainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            var OpenMainWindowGrid = new OpenMainWindowGrid(this) { Name = "OpenMainWindowGrid" }; // 创建弹出面板设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, OpenMainWindowGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-功能快捷键
        private void FunctionShortcutKeys_Click(object sender, RoutedEventArgs e)
        {
            OpenCustomGrid(FunctionShortcutKeys, new FunctionShortcutKeysGrid(this), ResultGrid);
        }
        // 鼠标移出Button恢复Background
        private void FunctionShortcutKeys_MouseLeave(object sender, MouseEventArgs e)
        {
            var FunctionShortcutKeysGrid = new FunctionShortcutKeysGrid(this) { Name = "FunctionShortcutKeysGrid" }; // 创建功能快捷键设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, FunctionShortcutKeysGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        // 基础设置-黑名单
        private void Blacklist_Click(object sender, RoutedEventArgs e)
        {
            OpenCustomGrid(Blacklist, new BlacklistGrid(this), ResultGrid);
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
            OpenCustomGrid(Appearance,  new AppearanceGrid(this), ResultGrid);
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
            OpenCustomGrid(AboutQuicker, new AboutQuickerGrid(this), ResultGrid, Visibility.Collapsed);
            // 下面的特殊逻辑可以保留
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            if (Convention.RememberLastPage && Convention.LastPage % 10 == 2) 
            {
                if (ResultGrid.Children.OfType<AboutQuickerGrid>().FirstOrDefault() is AboutQuickerGrid aboutGrid)
                    aboutGrid.Privacy_StatementButton_Click(null, null); // 显示隐私声明
            }
        }
        // 鼠标移出Button恢复Background
        private void AboutQuicker_MouseLeave(object sender, MouseEventArgs e)
        {
            var AboutQuickerGrid = new AboutQuickerGrid(this) { Name = "AboutQuickerGrid" }; // 创建关于Quicker设置Grid
            _settingManager.ButtonStyle2_MouseLeave(sender, AboutQuickerGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        #endregion

        #region 辅助功能

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

        #endregion

        #region 工具
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

        // 工具-管理扩展
        private void ManageExtensions_Click(object sender, RoutedEventArgs e)
        {
            OpenCustomGrid(ManageExtensions, new ExtensionManagementGrid(), ResultGrid, Visibility.Visible);
        }
        // 鼠标移出Button恢复Background
        private void ManageExtensions_MouseLeave(object sender, MouseEventArgs e)
        {
            var ExtensionManagementGrid = new ExtensionManagementGrid() { Name = "ExtensionManagementGrid" }; // 创建常规设置Grid  
            _settingManager.ButtonStyle2_MouseLeave(sender, ExtensionManagementGrid, ResultGrid); // 鼠标移出Button恢复Background
        }

        #endregion

        #region 搜索与高亮

        /// <summary>
        /// 查找按钮内的TextBlock
        /// </summary>
        /// <param name="btn">按钮</param>
        /// <returns>TextBlock</returns>
        private TextBlock FindTextBlockInButton(Button btn)
        {
            if (btn.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is TextBlock tb)
                        return tb; // 返回文本框
                }
            }
            return null;
        }

        /// <summary>
        /// 克隆一个按钮（用于Popup搜索结果），并设置高亮关键字。
        /// </summary>
        /// <param name="original">原始按钮</param>
        /// <param name="keyword">需要高亮的关键字</param>
        /// <returns>克隆后的按钮（带高亮）</returns>
        private Button CloneButton(Button original, string keyword = "")
        {
            var newBtn = new Button // 新建一个Button实例
            {
                Content = CloneButtonContent(original.Content, keyword), // 克隆Content并高亮
                HorizontalAlignment = original.HorizontalAlignment,
                VerticalAlignment = original.VerticalAlignment,
                ToolTip = original.ToolTip,
                Padding = original.Padding,
                Margin = original.Margin,
                Style = original.Style, // 沿用原按钮的样式
                Tag = original.Name // 用于后续识别按钮类型
            };
            newBtn.Click += SearchResultButton_Click; // 绑定点击事件，点击后跳转到对应设置页
            return newBtn; // 返回按钮
        }

        /// <summary>
        /// 克隆按钮的Content（主入口，根据类型分发）。
        /// </summary>
        /// <param name="content">原始按钮的Content</param>
        /// <param name="keyword">需要高亮的关键字</param>
        /// <returns>克隆后的Content对象</returns>
        private object CloneButtonContent(object content, string keyword = "")
        {
            if (content is Grid grid) // 如果是Grid类型，调用专用克隆方法
                return CloneGridContent(grid, keyword);
            return content; // 其它类型直接返回
        }

        /// <summary>
        /// 克隆Grid及其所有子元素（Image、TextBlock等）。
        /// </summary>
        /// <param name="grid">原始Grid</param>
        /// <param name="keyword">需要高亮的关键字</param>
        /// <returns>克隆后的Grid</returns>
        private Grid CloneGridContent(Grid grid, string keyword)
        {
            var newGrid = new Grid { Width = grid.Width };
            foreach (var child in grid.Children)
            {
                if (child is Image img)
                    newGrid.Children.Add(CloneImageContent(img)); // 克隆Image控件
                else if (child is TextBlock tb)
                    newGrid.Children.Add(CloneTextBlockContent(tb, keyword)); // 克隆TextBlock并高亮
            }
            return newGrid;
        }

        /// <summary>
        /// 克隆Image控件，复制其常用属性。
        /// </summary>
        /// <param name="img">原始Image控件</param>
        /// <returns>克隆后的Image控件</returns>
        private Image CloneImageContent(Image img)
        {
            return new Image
            {
                Source = img.Source,
                Width = img.Width,
                Height = img.Height,
                Margin = img.Margin,
                HorizontalAlignment = img.HorizontalAlignment,
                VerticalAlignment = img.VerticalAlignment
            };
        }

        /// <summary>
        /// 克隆TextBlock控件，并用TextBlockHelper高亮关键字。
        /// </summary>
        /// <param name="tb">原始TextBlock控件</param>
        /// <param name="keyword">需要高亮的关键字</param>
        /// <returns>克隆后的TextBlock控件</returns>
        private TextBlock CloneTextBlockContent(TextBlock tb, string keyword)
        {
            var newTb = new TextBlock
            {
                Margin = tb.Margin,
                HorizontalAlignment = tb.HorizontalAlignment,
                VerticalAlignment = tb.VerticalAlignment
            };
            // 设置高亮
            TextBlockHelper.SetHighlight(newTb, new HighlightTextData
            {
                Text = tb.Text,
                Keyword = keyword
            });
            return newTb;
        }

        // Popup中副本按钮的点击事件，根据Tag跳转到对应设置页面。
        private void SearchResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string btnName)
            {
                SearchResultPopup.IsOpen = false; // 关闭Popup
                // 按钮Name映射到pageCode，调用SetLastPage实现完整跳转（包括侧边栏高亮等）
                int pageCode = btnName switch
                {
                    "Convention" => 111,
                    "OpenMainWindow" => 121,
                    "FunctionShortcutKeys" => 131,
                    "Blacklist" => 141,
                    "Appearance" => 151,
                    "AboutQuicker" => 161,
                    "ManageExtensions" => 311,
                    _ => 0
                };
                if (pageCode != 0)
                {
                    SetLastPage(pageCode);
                }
            }
        }

        // 搜索框内容变化事件，动态生成高亮的按钮副本并显示在Popup中。
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim(); // 获取搜索框输入的关键字
            if (string.IsNullOrEmpty(keyword)) // 如果关键字为空，关闭Popup并返回
            {
                SearchResultPopup.IsOpen = false; // 关闭Popup
                return;
            }

            SearchResultPanel.Children.Clear(); // 清空原有按钮
            foreach (var stackPanel in MenuGrid.Children.OfType<StackPanel>()) // 遍历菜单栏的StackPanel
            {
                foreach (var child in stackPanel.Children) // 遍历StackPanel的子元素
                {
                    Button btn = child as Button; // 获取Button
                    var textBlock = FindTextBlockInButton(btn); // 获取Button内的TextBlock
                    string text = textBlock?.Text ?? ""; // 获取Button内的文本
                    if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) // 如果文本包含关键字，克隆按钮并添加到Popup中
                    {
                        var clone = CloneButton(btn, keyword); // 克隆按钮并高亮关键字
                        SearchResultPanel.Children.Add(clone); // 添加到Popup中
                    }
                }
            }
            SearchResultPopup.IsOpen = SearchResultPanel.Children.Count > 0; // 如果有按钮，打开Popup
        }

        #endregion
    }
}