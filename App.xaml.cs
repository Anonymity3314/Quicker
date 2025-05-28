using Hardcodet.Wpf.TaskbarNotification;
using Quicker.Windows.Forms;
using Quicker.Windows.Menus;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
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
                MessageWindow messageWindow = new("Quicker", "Quicker已经在运行，不能启动多个实例！"); // 创建消息窗口
                messageWindow.ShowInTaskbar = true; // 显示在任务栏
                messageWindow.ShowDialog(); // 显示消息窗口
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
            updateManager.CheckForUpdate(); // 检查并安装更新
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
            }; // 创建托盘图标

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
            AppStateManager.Timer.Interval = TimeSpan.FromMinutes(5); // 每 5 分钟更新一次
            AppStateManager.Timer.Tick += Timer_Tick; // 每 5 分钟触发一次
            AppStateManager.Timer.Start(); // 启动定时器

            // 初始化按键计时器
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
            var conventions = AppStateManager.Conventions; // 获取设置
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            ProcessLongPressAndMouseMove(conventions, openMainWindowConditions); // 处理长按和鼠标移动
        }

        /// <summary>
        /// 处理长按和鼠标移动
        /// </summary>
        /// <param name="conventions"> 设置 </param>
        /// <param name="conditions"> 设置 </param>
        private void ProcessLongPressAndMouseMove(Convention conventions, OpenMainWindow conditions)
        {
            if (conditions.OpenMainWindowByMiddleMouseClickLonger ||
                conditions.OpenMainWindowByRightMouseClickLonger)
            {
                ProcessLongPress(conventions); // 处理长按事件
            }
            else if (conditions.OpenMainWindowByRightMouseClick_Move)
            {
                ProcessMouseMove(conventions); // 处理鼠标移动事件
            }
        }

        /// <summary>
        /// 处理长按事件
        /// </summary>
        /// <param name="conventions"> 设置 </param>
        private void ProcessLongPress(Convention conventions)
        {
            TimeSpan pressDuration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 计算按键按下时间
            double longPressThreshold = conventions.LongPressThreshold / 1000.0; // 将毫秒转换为秒
            if (pressDuration.TotalSeconds >= longPressThreshold)
            {
                CloseOrShowMainWindow(); // 如果按键时间超过阈值，触发功能
                AppStateManager.KeyPressStartTime = null; // 重置按键时间
                AppStateManager.PressTimer.Stop(); // 停止计时器
            }
        }

        /// <summary>
        /// 处理鼠标移动事件
        /// </summary>
        /// <param name="conventions"> 设置 </param>
        private void ProcessMouseMove(Convention conventions)
        {
            var currentPosition = new System.Windows.Point(
                System.Windows.Forms.Cursor.Position.X,
                System.Windows.Forms.Cursor.Position.Y
            ); // 获取当前鼠标位置

            double offsetX = currentPosition.X - AppStateManager.StartPosition.X; // 计算水平偏移量
            double offsetY = currentPosition.Y - AppStateManager.StartPosition.Y; // 计算垂直偏移量
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY); // 计算移动距离
            if (distance > conventions.MouseMovePixels) // 如果移动距离大于设置像素值
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
        }

        // 定时器每5min保存使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            var convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
            convention.TotalUsageTime += 300; // 每 5 分钟增加 300 秒
            SettingDatabase.SaveTotalUsageTime(convention.TotalUsageTime); // 保存总使用时长到数据库
            AppStateManager.RecordedTime = DateTime.Now; // 记录应用保存时间
        }

        // 初始化钩子
        private async Task InitializeHookAsync()
        {
            hook = new(); // 创建钩子
            hook.KeyPressed += Hook_KeyPressed; // 按键按下事件
            hook.KeyReleased += Hook_KeyReleased; // 按键松开事件
            hook.MousePressed += Hook_MousePressed; // 鼠标按下事件
            hook.MouseReleased += Hook_MouseReleased; // 鼠标松开事件
            await hook.RunAsync().ConfigureAwait(false); // 启动钩子
        }

        // 鼠标按下事件
        private void Hook_MousePressed(object? sender, MouseHookEventArgs e)
        {
            if (!CanProcessHook()) return; // 如果不能处理钩子，返回
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            bool isCtrlPressed = GetCtrlKeyState(); // 获取 Ctrl 键状态
            
            ProcessMouseButtonPress(e.Data.Button, openMainWindowConditions, isCtrlPressed); // 处理鼠标按下事件
        }

        // 鼠标松开事件
        private void Hook_MouseReleased(object? sender, MouseHookEventArgs e)
        {
            AppStateManager.PressTimer?.Stop(); // 停止计时器
            if (!AppStateManager.KeyPressStartTime.HasValue) return; // 如果按键时间未记录，返回

            var conventions = AppStateManager.Conventions; // 获取设置
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = GetPressDuration(); // 获取按键按下时间
            
            ProcessMouseButtonRelease(e.Data.Button, openMainWindowConditions, conventions, pressDuration); // 处理鼠标松开事件
        }

        // 按键按下事件
        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e)
        {
            if (!CanProcessHook()) return; // 如果不能处理钩子，返回
            if (IsKeyPressAlreadyRecorded()) return; // 如果按键已经记录，返回

            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            ProcessControlKeyPress(e.Data.KeyCode, openMainWindowConditions); // 处理按键按下事件
        }

        // 按键松开事件
        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e)
        {
            if (!AppStateManager.KeyPressStartTime.HasValue) return; // 如果按键时间未记录，返回

            var conventions = AppStateManager.Conventions; // 获取设置
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = GetPressDuration(); // 获取按键按下时间
            
            ProcessControlKeyRelease(e.Data.KeyCode, openMainWindowConditions, conventions, pressDuration); // 处理按键松开事件
        }

        /// <summary>
        /// 判断是否可以处理钩子
        /// </summary>
        /// <returns> 是否可以处理钩子 </returns>
        private bool CanProcessHook()
        {
            return !IsBannedFormQuicker() && !FullScreenDisable(); // 如果禁用Quicker或全屏禁用Quicker，返回
        }

        /// <summary>
        /// 判断是否已经记录按键按下时间
        /// </summary>
        /// <returns> 是否已经记录按键按下时间 </returns>
        private bool IsKeyPressAlreadyRecorded()
        {
            if (!AppStateManager.KeyPressStartTime.HasValue) return false; // 如果按键时间未记录，返回
            AppStateManager.KeyPressStartTime = null; // 重置按键时间
            return true; // 返回是否已经记录按键按下时间
        }

        /// <summary>
        /// 获取 Ctrl 键状态
        /// </summary>
        /// <returns> Ctrl 键状态 </returns>
        private bool GetCtrlKeyState()
        {
            bool isCtrlPressed = false; // 是否按下 Ctrl 键
            this.Dispatcher.Invoke(() =>
            {
                isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }); // 获取 Ctrl 键状态
            return isCtrlPressed; // 返回 Ctrl 键状态
        }

        /// <summary>
        /// 获取按键按下时间
        /// </summary>
        /// <returns> 按键按下时间 </returns>
        private TimeSpan GetPressDuration()
        {
            var duration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 获取按键按下时间
            AppStateManager.KeyPressStartTime = null; // 重置按键时间
            return duration; // 返回按键按下时间
        }

        /// <summary>
        /// 处理鼠标按下事件
        /// </summary>
        /// <param name="button"> 鼠标按钮 </param>
        /// <param name="conditions"> 设置 </param>
        /// <param name="isCtrlPressed"> Ctrl 键状态 </param>
        private void ProcessMouseButtonPress(SharpHook.Data.MouseButton button, OpenMainWindow conditions, bool isCtrlPressed)
        {
            switch (button)
            {
                case SharpHook.Data.MouseButton.Button2:
                    ProcessRightMouseButtonPress(conditions, isCtrlPressed);
                    break; // 右键按下
                case SharpHook.Data.MouseButton.Button3:
                    ProcessMiddleMouseButtonPress(conditions, isCtrlPressed);
                    break; // 中键按下
                case SharpHook.Data.MouseButton.Button4:
                    ProcessX1MouseButtonPress(conditions);
                    break; // X1鼠标按下
                case SharpHook.Data.MouseButton.Button5:
                    ProcessX2MouseButtonPress(conditions);
                    break; // X2鼠标按下
            }
        }

        /// <summary>
        /// 处理右键按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        /// <param name="isCtrlPressed"> Ctrl 键状态 </param>
        private void ProcessRightMouseButtonPress(OpenMainWindow conditions, bool isCtrlPressed)
        {
            if (conditions.OpenMainWindowByRightMouseClick_Move)
            {
                RecordMousePosition(); // 记录鼠标位置
                PreLoadMainWindow(true); // 预加载主窗口
            }
            else if (isCtrlPressed && conditions.OpenMainWindowByCtrl_RightMouseClick)
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
            else if (conditions.OpenMainWindowByRightMouseClickLonger)
            {
                PreLoadMainWindow(true); // 预加载主窗口
            }
        }

        /// <summary>
        /// 处理中键按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        /// <param name="isCtrlPressed"> Ctrl 键状态 </param>
        private void ProcessMiddleMouseButtonPress(OpenMainWindow conditions, bool isCtrlPressed)
        {
            if (isCtrlPressed && conditions.OpenMainWindowByCtrl_MiddleMouseClick)
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
            else if (conditions.OpenMainWindowByMiddleMouseClick)
            {
                PreLoadMainWindow(); // 预加载主窗口
            }
            else if (conditions.OpenMainWindowByMiddleMouseClickLonger)
            {
                PreLoadMainWindow(true); // 预加载主窗口
            }
        }

        /// <summary>
        /// 处理 X1 鼠标按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        private void ProcessX1MouseButtonPress(OpenMainWindow conditions)
        {
            if (conditions.OpenMainWindowByX1MouseClick)
            {
                PreLoadMainWindow(); // 预加载主窗口
            }
        }

        /// <summary>
        /// 处理 X2 鼠标按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        private void ProcessX2MouseButtonPress(OpenMainWindow conditions)
        {
            if (conditions.OpenMainWindowByX2MouseClick)
            {
                PreLoadMainWindow(); // 预加载主窗口
            }
        }

        /// <summary>
        /// 处理鼠标松开事件
        /// </summary>
        /// <param name="button"> 鼠标按钮 </param>
        /// <param name="conditions"> 设置 </param>
        /// <param name="conventions"> 设置 </param>
        /// <param name="pressDuration"> 按键按下时间 </param>
        private void ProcessMouseButtonRelease(SharpHook.Data.MouseButton button, OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            switch (button)
            {
                case SharpHook.Data.MouseButton.Button3:
                    ProcessMiddleMouseButtonRelease(conditions, conventions, pressDuration);
                    break; // 中键松开
                case SharpHook.Data.MouseButton.Button4:
                case SharpHook.Data.MouseButton.Button5:
                    ProcessXMouseButtonRelease(conditions, conventions, pressDuration);
                    break; // X鼠标松开
            }
        }

        /// <summary>
        /// 处理中键松开事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        /// <param name="conventions"> 设置 </param>
        /// <param name="pressDuration"> 按键按下时间 </param>
        private void ProcessMiddleMouseButtonRelease(OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            if (pressDuration.TotalSeconds <= conventions.LongPressThreshold &&
                conditions.OpenMainWindowByMiddleMouseClick)
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
        }

        /// <summary>
        /// 处理 X 鼠标按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        /// <param name="conventions"> 设置 </param>
        /// <param name="pressDuration"> 按键按下时间 </param>
        private void ProcessXMouseButtonRelease(OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            if ((conditions.OpenMainWindowByX1MouseClick ||
                 conditions.OpenMainWindowByX2MouseClick) &&
                pressDuration.TotalSeconds <= conventions.LongPressThreshold)
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
        }

        /// <summary>
        /// 处理 Ctrl 按键按下事件
        /// </summary>
        /// <param name="keyCode"> 按键代码 </param>
        /// <param name="conditions"> 设置 </param>
        private void ProcessControlKeyPress(SharpHook.Data.KeyCode keyCode, OpenMainWindow conditions)
        {
            if ((keyCode == SharpHook.Data.KeyCode.VcLeftControl ||
                 keyCode == SharpHook.Data.KeyCode.VcRightControl) &&
                (conditions.OpenMainWindowByCtrl_MiddleMouseClick ||
                 conditions.OpenMainWindowByCtrl_RightMouseClick ||
                 conditions.OpenMainWindowByCtrl))
            {
                PreLoadMainWindow(); // 预加载主窗口
            }

        }

        /// <summary>
        /// 处理 Ctrl 按键松开事件
        /// </summary>
        /// <param name="keyCode"> 按键代码 </param>
        /// <param name="conditions"> 设置 </param>
        /// <param name="conventions"> 设置 </param>
        /// <param name="pressDuration"> 按键按下时间 </param>
        private void ProcessControlKeyRelease(SharpHook.Data.KeyCode keyCode, OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            if ((keyCode == SharpHook.Data.KeyCode.VcLeftControl ||
                 keyCode == SharpHook.Data.KeyCode.VcRightControl) &&
                conditions.OpenMainWindowByCtrl &&
                pressDuration.TotalSeconds <= conventions.LongPressThreshold)
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
        }

        // 记录鼠标位置
        private void RecordMousePosition()
        {
            AppStateManager.StartPosition = new System.Windows.Point(
                System.Windows.Forms.Cursor.Position.X, // 鼠标位置
                System.Windows.Forms.Cursor.Position.Y // 鼠标位置
            ); // 记录鼠标位置
        }

        /// <summary>
        /// 判断是否全屏禁用Quicker
        /// </summary>
        /// <returns> 是否全屏禁用Quicker </returns>
        private bool FullScreenDisable()
        {
            var blacklistSettings = AppStateManager.BlacklistSettings; // 获取设置
            if (!blacklistSettings.IsFullScreenDisabled) return false; // 如果全屏禁用Quicker，返回

            using var windowManager = new WindowManager(); // 创建窗口管理器
            if (!windowManager.IsFullScreen()) return false; // 如果窗口不是全屏，返回

            string processName = windowManager.GetProcessName(); // 获取进程名称
            var blacklistApplications = AppStateManager.BlacklistApplications; // 获取设置
            
            if (blacklistApplications.Count == 0) return true;
            
            return !blacklistApplications.Any(p => p.ProcessName == processName && !p.IsInBlacklist);
        }

        /// <summary>
        /// 判断是否禁用Quicker
        /// </summary>
        /// <returns> 是否禁用Quicker </returns>
        private bool IsBannedFormQuicker()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            nint foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口
            if (foregroundWindow == IntPtr.Zero) return false; // 如果当前前台窗口为空，返回

            uint processId = windowManager.GetWindowProcessId(foregroundWindow);
            string processName = Process.GetProcessById((int)processId).ProcessName;

            return AppStateManager.BlacklistApplications
                .Any(p => p.ProcessName == processName && p.IsInBlacklist); // 如果进程名称在黑名单中，返回
        }

        // 弹出主窗口
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 获取主窗口
                if (mainWindow == null)
                {
                    string windowType = DetermineWindowType(); // 确定窗口类型
                    mainWindow = new MainWindow(windowType); // 创建主窗口
                    var settings = AppStateManager.OpenMainWindowConditions; // 获取设置
                    SetMainWindowPosition(mainWindow, settings.WindowStartupLocation); // 设置窗口位置
                    AppStateManager.Left = (float)mainWindow.Left; // 记录主窗口位置
                    AppStateManager.Top = (float)mainWindow.Top; // 记录主窗口位置
                    mainWindow.Show(); // 显示主窗口
                }
                else
                {
                    mainWindow.Visibility = Visibility.Visible; // 显示主窗口
                    AppStateManager.PreLoadMainWindow = null; // 清空预加载窗口
                }
            });
        }

        /// <summary>
        /// 预加载主窗口
        /// </summary>
        /// <param name="startTimer"> 是否启动计时器 </param>
        public void PreLoadMainWindow(bool startTimer = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                AppStateManager.KeyPressStartTime = DateTime.Now; // 记录按键按下时间
                if (startTimer) AppStateManager.PressTimer.Start(); // 启动按键计时器

                var actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 获取动作页面管理窗口
                if (actionPageManageWindow != null && actionPageManageWindow.WindowState != WindowState.Minimized) return; // 如果动作页面管理窗口打开，返回

                var settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 获取设置窗口
                if (settingWindow != null && settingWindow.WindowState != WindowState.Minimized) return; // 如果设置窗口打开，返回

                string windowType = DetermineWindowType(); // 确定窗口类型
                AppStateManager.PreLoadMainWindow = new MainWindow(windowType); // 创建主窗口

                var settings = AppStateManager.OpenMainWindowConditions; // 获取设置
                SetMainWindowPosition(AppStateManager.PreLoadMainWindow, settings.WindowStartupLocation); // 设置窗口位置
                AppStateManager.PreLoadMainWindow.Visibility = Visibility.Hidden; // 隐藏主窗口
                AppStateManager.Left = (float)AppStateManager.PreLoadMainWindow.Left; // 记录主窗口位置
                AppStateManager.Top = (float)AppStateManager.PreLoadMainWindow.Top; // 记录主窗口位置
            });
        }

        // 关闭或显示主窗口
        public void CloseOrShowMainWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (AppStateManager.PreLoadMainWindow == null) return; // 如果预加载窗口为空，返回
                AppStateManager.PreLoadMainWindow.Visibility = Visibility.Visible; // 显示主窗口
                AppStateManager.PreLoadMainWindow = null; // 清空预加载窗口
            });
        }

        /// <summary>
        /// 确定窗口类型
        /// </summary>
        /// <returns> 窗口类型 </returns>
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

        /// <summary>
        /// 设置主窗口位置
        /// </summary>
        /// <param name="mainWindow"> 主窗口 </param>
        /// <param name="conditions"> 窗口位置 </param>
        private void SetMainWindowPosition(MainWindow mainWindow, int conditions)
        {
            switch (conditions)
            {
                case 0:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.Manual; // 系统默认
                    break; // 系统默认
                case 1:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.Manual; // 鼠标位置
                    mainWindow.Left = System.Windows.Forms.Cursor.Position.X; // 鼠标位置
                    mainWindow.Top = System.Windows.Forms.Cursor.Position.Y; // 鼠标位置
                    break; // 窗口打开位置跟随鼠标
                case 2:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen; // 屏幕中心
                    break; // 屏幕中心
                case 3:
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // 当前窗口中心
                    break; // 当前窗口中心
                case 4:
                    if (AppStateManager.Left != null && AppStateManager.Top != null) // 上次弹出位置
                    {
                        mainWindow.Left = AppStateManager.Left; // 记录主窗口位置
                        mainWindow.Top = AppStateManager.Top; // 记录主窗口位置
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
            using var toast = new ToastManager(); // 消息提醒管理器
            try
            {
                hook?.Dispose(); // 销毁当前钩子
                hook = null; // 清空钩子
                if (AppStateManager.Pause) InitializeHookAsync(); // 重新初始化钩子
            }
            catch
            {
                toast.ShowToast("请勿频繁操作！", "Warning"); // 弹出消息提醒
            }

            var toastMessage = AppStateManager.Pause ? "Quicker已恢复" : "Quicker已暂停"; // 消息提醒
            var text = AppStateManager.Pause ? "暂停" : "恢复"; // 消息提醒
            CustomMenu customMenu = Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            customMenu.PauseQuickerTextBlock.Text = text; // 更新菜单栏文本
            ChangeTrayIcon(AppStateManager.Pause); // 切换托盘图标

            AppStateManager.Pause = !AppStateManager.Pause; // 切换暂停状态
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
            double currentSessionTime = (DateTime.Now - AppStateManager.RecordedTime).TotalSeconds; // 计算本次会话时间
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += currentSessionTime; // 增加本次会话时间
            SettingDatabase.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时间
            DisposeTaskbarIcon(); // 释放托盘图标绑定的事件
            DisposeHook(); // 释放钩子绑定的事件
            AppStateManager.Dispose(); // 释放所有资源
            SingleInstanceManager.ReleaseMutex(); // 释放互斥锁
            base.OnExit(e); // 调用基类的 OnExit 方法
        }

        // 释放托盘图标绑定的事件
        private void DisposeTaskbarIcon()
        {
            taskbarIcon.TrayLeftMouseDown -= ShowMainWindow; // 移除左键单击弹出功能面板事件
            taskbarIcon.TrayRightMouseDown -= ShowCustomMenu; // 移除右键单击弹出菜单栏事件
            taskbarIcon.TrayMouseDoubleClick -= PauseQuicker; // 移除双击暂停Quicker事件
            taskbarIcon?.Dispose(); // 释放托盘图标
            taskbarIcon = null; // 清空托盘图标
        }

        // 释放钩子绑定的事件
        private void DisposeHook()
        {
            hook.KeyPressed -= Hook_KeyPressed; // 移除按键按下事件处理器
            hook.KeyReleased -= Hook_KeyReleased; // 移除按键松开事件处理器
            hook.MousePressed -= Hook_MousePressed; // 移除鼠标按下事件处理器
            hook.MouseReleased -= Hook_MouseReleased; // 移除鼠标松开事件处理器
            hook?.Dispose(); // 释放钩子
            hook = null; // 清空钩子
        }
    }
}