using Hardcodet.Wpf.TaskbarNotification;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using SharpHook;

namespace Quicker
{
    public partial class App : Application
    {
        private TaskPoolGlobalHook? hook; // 全局钩子
        private TaskbarIcon? taskbarIcon; // 托盘图标

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // 调用基类方法
            EnsureSingleInstance(); // 确保单例运行
            CheckAppUpdate(); // 检查应用更新
            CheckAndUpdateDatabase(); // 检查并升级数据库
            InitializeTimer(); // 初始化定时器
            InitializeTaskbar(); // 初始化托盘图标
            InitializeHookAsync(); // 初始化钩子
            ShowNotification(); // 弹出消息提醒
        }

        // 确保单例运行
        private void EnsureSingleInstance()
        {
            var version = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取数据库版本
            string mutexName = version.Version; // 互斥锁唯一标识
            bool isNewInstance = SingleInstanceManager.CheckForOtherInstances(mutexName, out _); // 检查是否是新实例
            if (!isNewInstance)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("Quicker 已经启动", "Common"); // 弹出消息提醒
                Application.Current.Shutdown(); // 如果不是新实例，关闭当前实例
                return; // 退出程序
            }
        }

        // 检查并升级数据库
        private void CheckAndUpdateDatabase()
        {
            using var databaseUpdater = new DatabaseUpdateManager(); // 数据库更新管理器
            databaseUpdater.CheckAndUpgradeDatabase(); // 检查并升级数据库
        }

        // 检查应用更新
        private void CheckAppUpdate()
        {
            using var updateManager = new AppUpdateManager(); // 创建更新管理器
        }

        // 初始化托盘图标
        private void InitializeTaskbar()
        {
            var icon = AppStateManager._trayIcon1; // 设置托盘图标
            Current.Resources["AppIcon"] = icon; // 设置应用图标
            taskbarIcon = new TaskbarIcon // 创建托盘图标
            {
                IconSource = icon, // 设置图标
                ToolTipText = "Quicker" // 设置提示文本
            };

            taskbarIcon.TrayLeftMouseDown += ShowMainWindow; // 左键单击弹出功能面板
            taskbarIcon.TrayRightMouseDown += ShowCustomMenu; // 右键单击弹出菜单栏
            taskbarIcon.TrayMouseDoubleClick += PauseQuicker; // 双击暂停Quicker
        }

        // 弹出消息提醒
        private void ShowNotification()
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
            if (Convention.ShowNotification) // 如果设置中允许显示消息提醒
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("成功启动！", "Common"); // 弹出消息提醒
            }
        }

        // 初始化定时器
        private void InitializeTimer()
        {
            AppStateManager.StartTime = DateTime.Now; // 记录应用启动时间
            AppStateManager.RecordedTime = AppStateManager.StartTime; // 记录应用记录时间

            // 初始化定时器
            AppStateManager.Timer = new DispatcherTimer();
            AppStateManager.Timer.Interval = TimeSpan.FromMinutes(5); // 每 5 分钟更新一次
            AppStateManager.Timer.Tick += Timer_Tick; // 每 5 分钟触发一次
            AppStateManager.Timer.Start(); // 启动定时器

            // 初始化按键计时器
            AppStateManager.PressTimer = new DispatcherTimer();
            AppStateManager.PressTimer.Interval = TimeSpan.FromMilliseconds(10); // 每 10 毫秒检查一次
            AppStateManager.PressTimer.Tick += PressTimer_Tick; // 计时器回调
        }

        // 按键计时器的回调
        private void PressTimer_Tick(object sender, EventArgs e)
        {
            if (!AppStateManager.KeyPressStartTime.HasValue) // 如果没有按下时间
            {
                AppStateManager.PressTimer.Stop(); // 停止计时器
                return; // 如果没有按下时间，停止计时器
            }
            var Conventions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllConventions().FirstOrDefault()
                : AppStateManager.Conventions; // 获取设置
            double LongPressThreshold = Conventions.LongPressThreshold / 1000.0; // 将毫秒转换为秒
            var OpenMainWindowConditions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                : AppStateManager.OpenMainWindowConditions; // 获取设置
            if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger ||
                OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger)
            {
                TimeSpan pressDuration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 计算按键按下时间
                if (pressDuration.TotalSeconds >= LongPressThreshold)
                {
                    CloseOrShowMainWindow(); // 如果按键时间超过阈值，触发功能
                    AppStateManager.KeyPressStartTime = null; // 重置按键时间
                    AppStateManager.PressTimer.Stop(); // 停止计时器
                }
            } // 长按中键或右键
            else if (OpenMainWindowConditions.OpenMainWindowByRightMouseClick_Move)
            {
                System.Windows.Point currentPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
                double offsetX = currentPosition.X - AppStateManager.StartPosition.X; // 计算水平偏移量
                double offsetY = currentPosition.Y - AppStateManager.StartPosition.Y; // 计算垂直偏移量
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY); // 计算移动距离
                if (distance > Conventions.MouseMovePixels) // 如果移动距离大于设置像素值
                    CloseOrShowMainWindow(); // 关闭或显示主窗口
            } // 右键移动
        }

        // 定时器每5min保存使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += 300; // 每 5 分钟增加 300 秒
            SettingDatabase.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时长到数据库
            AppStateManager.RecordedTime = DateTime.Now; // 记录应用保存时间
        }

        // 初始化钩子
        private async Task InitializeHookAsync()
        {
            hook = new TaskPoolGlobalHook(); // 创建钩子
            hook.KeyPressed += Hook_KeyPressed; // 按键按下事件
            hook.KeyReleased += Hook_KeyReleased; // 按键松开事件
            hook.MousePressed += Hook_MousePressed; // 鼠标按下事件
            hook.MouseReleased += Hook_MouseReleased; // 鼠标松开事件
            await hook.RunAsync().ConfigureAwait(false); // 确保钩子启动
        }

        // 按下鼠标快捷键时如果按键尚未被记录，记录按键按下的时间
        private void Hook_MousePressed(object? sender, MouseHookEventArgs e)
        {
            if (IsBannedFormQuicker()) return; // 如果禁用Quicker，返回
            if (FullScreenDisable()) return; // 如果全屏禁用Quicker，返回
            if (AppStateManager.KeyPressStartTime.HasValue)
            {
                AppStateManager.KeyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                : AppStateManager.OpenMainWindowConditions; // 获取设置
            bool isCtrlPressed = false; // 是否按下 Ctrl 键
            this.Dispatcher.BeginInvoke(() =>
            {
                isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }); // 在 UI 线程中获取键盘状态
            switch (e.Data.Button)
            {
                case SharpHook.Native.MouseButton.Button2:
                    if (OpenMainWindowConditions.OpenMainWindowByRightMouseClick_Move)
                    {
                        AppStateManager.StartPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
                        PreLoadMainWindow(true);
                    } // 右键移动
                    else if (isCtrlPressed && OpenMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick)
                        CloseOrShowMainWindow(); // Ctrl + 右键
                    else if (OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger)
                        PreLoadMainWindow(true); // 长按右键
                    break; // 右键
                case SharpHook.Native.MouseButton.Button3:
                    if (isCtrlPressed && OpenMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick)
                        CloseOrShowMainWindow(); // Ctrl + 中键
                    else if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick)
                        PreLoadMainWindow(); // 单击中键
                    else if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger)
                        PreLoadMainWindow(true); // 长按中键
                    break; // 中键
                case SharpHook.Native.MouseButton.Button4:
                    if (OpenMainWindowConditions.OpenMainWindowByX1MouseClick)
                        PreLoadMainWindow();
                    break; // X1键
                case SharpHook.Native.MouseButton.Button5:
                    if (OpenMainWindowConditions.OpenMainWindowByX2MouseClick)
                        PreLoadMainWindow();
                    break; // X2键
            }
        }

        // 松开鼠标满足条件弹出面板
        private void Hook_MouseReleased(object? sender, MouseHookEventArgs e)
        {
            AppStateManager.PressTimer?.Stop(); // 停止计时器
            if (!AppStateManager.KeyPressStartTime.HasValue) return;
            var Conventions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllConventions().FirstOrDefault()
                : AppStateManager.Conventions; // 获取设置
            var OpenMainWindowConditions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                : AppStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 计算按键按下和释放的时间差
            AppStateManager.KeyPressStartTime = null;
            switch (e.Data.Button)
            {
                case SharpHook.Native.MouseButton.Button3:
                    if (pressDuration.TotalSeconds <= Conventions.LongPressThreshold &&
                        OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick)
                        CloseOrShowMainWindow();
                    break; // 短按中键
                case SharpHook.Native.MouseButton.Button4: // 短按X1键
                case SharpHook.Native.MouseButton.Button5:
                    if (OpenMainWindowConditions.OpenMainWindowByX1MouseClick ||
                        OpenMainWindowConditions.OpenMainWindowByX2MouseClick)
                    {
                        if (pressDuration.TotalSeconds <= Conventions.LongPressThreshold)
                            CloseOrShowMainWindow();
                    }
                    break; // 短按X2键
            }
        }

        // 按下键盘快捷键时如果按键尚未被记录，记录按键按下的时间
        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e)
        {
            if (IsBannedFormQuicker()) return; // 如果禁用Quicker，返回
            if (FullScreenDisable()) return; // 如果全屏禁用Quicker，返回
            if (AppStateManager.KeyPressStartTime.HasValue)
            {
                AppStateManager.KeyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                : AppStateManager.OpenMainWindowConditions; // 获取设置
            switch (e.Data.KeyCode)
            {
                case SharpHook.Native.KeyCode.VcLeftControl: // 左 Ctrl 键
                case SharpHook.Native.KeyCode.VcRightControl:
                    if (OpenMainWindowConditions.OpenMainWindowByCtrl)
                        PreLoadMainWindow();
                    break; // 右 Ctrl 键
            }
        }

        // 松开按键满足条件弹出面板
        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e)
        {
            if (!AppStateManager.KeyPressStartTime.HasValue) return;
            var Conventions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllConventions().FirstOrDefault()
                : AppStateManager.Conventions; // 获取设置
            var OpenMainWindowConditions = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                : AppStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 计算按键按下和释放的时间差
            AppStateManager.KeyPressStartTime = null;
            switch (e.Data.KeyCode)
            {
                case SharpHook.Native.KeyCode.VcLeftControl: // 左 Ctrl 键
                case SharpHook.Native.KeyCode.VcRightControl:
                    if (OpenMainWindowConditions.OpenMainWindowByCtrl &&
                        pressDuration.TotalSeconds <= Conventions.LongPressThreshold)
                        CloseOrShowMainWindow();
                    break; // 右 Ctrl 键
            }
        }

        // 是否全屏禁用Quicker
        private bool FullScreenDisable()
        {
            var blacklistSettings = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllBlacklistSettings().FirstOrDefault()
                : AppStateManager.BlacklistSettings; // 获取黑名单设置
            if (!blacklistSettings.IsFullScreenDisabled) return false; // 如果没有启用全屏禁用Quicker，返回false
            using var windowManager = new WindowManager(); // 创建窗口管理器
            if (windowManager.IsFullScreen()) // 窗口最大化
            {
                string processName = windowManager.GetProcessName(); // 获取进程名
                var blacklistApplications = AppStateManager.EnableMemoryOptimization
                    ? SettingDatabase.GetAllBlacklistApplications()
                    : AppStateManager.BlacklistApplications; // 获取黑名单进程
                if (blacklistApplications.Count == 0) return true; // 没有黑名单进程，返回true表示Quicker被禁用
                if (blacklistApplications.Any(p => p.ProcessName == processName && !p.IsInBlacklist)) // 如果进程名在黑名单中
                    return false; // 返回false表示正常工作
                return true; // 返回true表示Quicker被禁用
            }
            return false; // 返回false表示正常工作
        }

        // 是否禁用Quicker
        private bool IsBannedFormQuicker()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            nint foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口句柄
            if (foregroundWindow == IntPtr.Zero) return false; // 没有前台窗口，返回false

            uint processId = windowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
            Process process = Process.GetProcessById((int)processId); // 获取进程
            string processName = process.ProcessName; // 获取进程名

            var blacklistedProcesses = AppStateManager.EnableMemoryOptimization
                ? SettingDatabase.GetAllBlacklistApplications()
                : AppStateManager.BlacklistApplications; // 获取黑名单进程
            if (blacklistedProcesses.Any(p => p.ProcessName == processName && p.IsInBlacklist)) // 如果进程名在黑名单中
                return true; // 返回true表示Quicker被禁用
            return false; // 返回false表示正常工作
        }

        // 弹出功能面板
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (mainWindow == null)
                {
                    string windowType = DetermineWindowType(); // 确定窗口类型
                    mainWindow = new MainWindow(windowType); // 创建新的功能面板
                    var settings = AppStateManager.EnableMemoryOptimization
                        ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                        : AppStateManager.OpenMainWindowConditions; // 获取设置
                    SetMainWindowPosition(mainWindow, settings.WindowStartupLocation); // 设置窗口位置
                    AppStateManager.Left = (float)mainWindow.Left; // 记录功能面板位置
                    AppStateManager.Top = (float)mainWindow.Top; // 记录功能面板位置
                    mainWindow.Show(); // 显示功能面板
                }
                else
                    mainWindow?.Activate(); // 激活现有的功能面板
            });
        }

        // 预加载主窗口
        public void PreLoadMainWindow(bool startTimer = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                AppStateManager.KeyPressStartTime = DateTime.Now; // 记录按键按下时间
                if (startTimer) AppStateManager.PressTimer.Start(); // 启动按键计时器

                ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (actionPageManageWindow != null && actionPageManageWindow.WindowState != WindowState.Minimized) return; // 如果动作窗口打开，则不打开功能面板
                SettingWindow settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (settingWindow != null && settingWindow.WindowState != WindowState.Minimized) return; // 如果设置窗口打开，则不打开功能面板

                string windowType = DetermineWindowType(); // 确定窗口类型
                AppStateManager.PreLoadMainWindow = new MainWindow(windowType); // 创建新的功能面板

                var settings = AppStateManager.EnableMemoryOptimization
                    ? SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault()
                    : AppStateManager.OpenMainWindowConditions; // 获取设置
                SetMainWindowPosition(AppStateManager.PreLoadMainWindow, settings.WindowStartupLocation); // 设置窗口位置
                AppStateManager.PreLoadMainWindow.Visibility = Visibility.Hidden; // 隐藏功能面板
                AppStateManager.Left = (float)AppStateManager.PreLoadMainWindow.Left; // 记录功能面板位置
                AppStateManager.Top = (float)AppStateManager.PreLoadMainWindow.Top; // 记录功能面板位置
            });
        }

        // 关闭或重新显示主窗口
        public void CloseOrShowMainWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (AppStateManager.PreLoadMainWindow == null) return; // 如果没有预加载窗口，返回
                AppStateManager.PreLoadMainWindow.Visibility = Visibility.Visible; // 显示功能面板
                AppStateManager.PreLoadMainWindow = null; // 清空预加载窗口
            });
        }

        // 确定窗口类型
        private string DetermineWindowType()
        {
            if (AppStateManager.Locked)
                return AppStateManager.CommonState; // 窗口类型为锁定状态
            else if (IsMouseOnTaskbar())
                return "Taskbar"; // 鼠标在任务栏上
            else if (IsMouseOnDesktop())
                return "Desktop"; // 鼠标在桌面上
            else
                return "Common"; // 鼠标在其他窗口上
        }

        // 设置窗口位置
        private void SetMainWindowPosition(MainWindow mainWindow, int conditions)
        {
            switch (conditions)
            {
                case 0:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    break; // 系统默认
                case 1:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    mainWindow.Left = System.Windows.Forms.Cursor.Position.X; // 鼠标位置
                    mainWindow.Top = System.Windows.Forms.Cursor.Position.Y; // 鼠标位置
                    break; // 窗口打开位置跟随鼠标
                case 2:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    break; // 屏幕中心
                case 3:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    break; // 当前窗口中心
                case 4:
                    if (AppStateManager.Left != null && AppStateManager.Top != null) // 上次弹出位置
                    {
                        mainWindow.Left = AppStateManager.Left;
                        mainWindow.Top = AppStateManager.Top;
                    }
                    else
                        mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    break; // 上次弹出位置
            }
        }

        /// <summary>
        /// 判断鼠标是否在任务栏上
        /// </summary>
        /// <returns> 是否在任务栏上 </returns>
        public bool IsMouseOnTaskbar()
        {
            Rect workArea = SystemParameters.WorkArea; // 获取工作区域
            double screenHeight = SystemParameters.PrimaryScreenHeight; // 获取屏幕高度
            System.Windows.Point mousePosition = new System.Windows.Point(
                System.Windows.Forms.Cursor.Position.X,
                System.Windows.Forms.Cursor.Position.Y
            ); // 获取鼠标位置
            bool isOnTaskbar = mousePosition.Y >= workArea.Height; // 鼠标在任务栏上
            return isOnTaskbar; // 返回鼠标是否在任务栏上
        }

        /// <summary>
        /// 判断鼠标是否在桌面上
        /// </summary>
        /// <returns> 是否在桌面上 </returns>
        public bool IsMouseOnDesktop()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            IntPtr foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 调用封装方法
            if (foregroundWindow == IntPtr.Zero) return true; // 没有前台窗口
            else return false; // 鼠标在桌面上
        }

        // 弹出菜单栏
        private void ShowCustomMenu(object sender, RoutedEventArgs e)
        {
            CustomMenu customMenu = Application.Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (customMenu != null)
            {
                var mousePosition = System.Windows.Forms.Control.MousePosition; // 获取鼠标位置
                var screenPosition = new System.Windows.Point(mousePosition.X, mousePosition.Y); // 获取屏幕位置
                customMenu.Visibility = Visibility.Hidden; // 隐藏菜单栏
                customMenu.Left = screenPosition.X / 2 + 350; // 设置菜单栏距离屏幕左侧的距离
                customMenu.Top = screenPosition.Y / 2 + 45; // 设置菜单栏距离屏幕顶部的位置
                customMenu.Visibility = Visibility.Visible; // 显示菜单栏
                customMenu.Activate(); // 激活菜单
            }
        }

        // 暂停Quicker
        public async void PauseQuicker(object sender, RoutedEventArgs e)
        {
            hook?.Dispose(); // 销毁当前钩子
            hook = null; // 清空钩子
            if (AppStateManager.Pause) InitializeHookAsync(); // 重新初始化钩子

            var toastMessage = AppStateManager.Pause ? "Quicker已恢复" : "Quicker已暂停"; // 消息提醒
            var text = AppStateManager.Pause ? "暂停" : "恢复"; // 消息提醒
            CustomMenu customMenu = Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            customMenu.PauseQuickerTextBlock.Text = text; // 更新菜单栏文本
            ChangeTrayIcon(AppStateManager.Pause); // 切换托盘图标

            AppStateManager.Pause = !AppStateManager.Pause; // 切换暂停状态
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.ShowToast(toastMessage, AppStateManager.Pause ? "Common" : "Success"); // 弹出消息提醒
        }

        /// <summary>
        /// 切换托盘图标
        /// </summary>
        /// <param name="isPaused"> 是否暂停 </param>
        public void ChangeTrayIcon(bool isPaused)
        {
            taskbarIcon.IconSource = isPaused ? AppStateManager._trayIcon1 : AppStateManager._trayIcon2; // 切换托盘图标
        }

        // 退出应用释放资源
        protected override void OnExit(ExitEventArgs e)
        {
            taskbarIcon?.Dispose(); // 释放托盘图标
            double currentSessionTime = (DateTime.Now - AppStateManager.RecordedTime).TotalSeconds; // 计算本次会话时间
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += currentSessionTime; // 增加本次会话时间
            SettingDatabase.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时间

            AppStateManager.Dispose(); // 释放数据库资源
            hook?.Dispose(); // 释放钩子

            SingleInstanceManager.ReleaseMutex(); // 释放互斥锁

            base.OnExit(e); // 调用基类的 OnExit 方法
        }
    }
}