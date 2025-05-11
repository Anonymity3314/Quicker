using Microsoft.Toolkit.Uwp.Notifications;
using Hardcodet.Wpf.TaskbarNotification;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using SharpHook;
using Quicker;

namespace Quicker
{
    public partial class App : System.Windows.Application
    {
        public AppStateManager _appStateManager = new AppStateManager(); // 应用状态管理器
        private TaskbarIcon? taskbarIcon; // 托盘图标
        private TaskPoolGlobalHook? hook; // 钩子
        private BitmapImage _trayIcon1;
        private BitmapImage _trayIcon2;

        public App()
        {
            _trayIcon1 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker1.ico", UriKind.Relative)); // 运行时的图标
            _trayIcon2 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker2.ico", UriKind.Relative)); // 暂停时的图标
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // 调用基类方法

            string mutexName = "Quicker 2.1.4"; // 互斥锁唯一标识
            bool isNewInstance = SingleInstanceManager.CheckForOtherInstances(mutexName, out _); // 检查是否是新实例
            if (!isNewInstance)
            {
                Application.Current.Shutdown(); // 如果不是新实例，关闭当前实例
                return; // 退出程序
            }

            // 初始化应用
            InitializeTimer(); // 初始化定时器
            InitializeTaskbar(); // 初始化托盘图标
            InitializeHookAsync(); // 初始化钩子
            ShowNotification(); // 弹出消息提醒
        }

        // 初始化托盘图标
        private void InitializeTaskbar()
        {
            var icon = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker1.ico", UriKind.Relative)); // 设置托盘图标
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
            var Convention = _appStateManager.Db.GetAllConventions().FirstOrDefault(); // 获取设置
            if (Convention.ShowNotification) // 如果设置中允许显示消息提醒
                new ToastContentBuilder().AddText("成功启动！").Show(); // 弹出消息提醒
        }

        // 初始化定时器
        private void InitializeTimer()
        {
            _appStateManager.StartTime = DateTime.Now; // 记录应用启动时间
            _appStateManager.RecordedTime = _appStateManager.StartTime; // 记录应用记录时间

            // 初始化定时器
            _appStateManager.Timer = new DispatcherTimer();
            _appStateManager.Timer.Interval = TimeSpan.FromMinutes(5); // 每 5 分钟更新一次
            _appStateManager.Timer.Tick += Timer_Tick; // 每 5 分钟触发一次
            _appStateManager.Timer.Start(); // 启动定时器

            // 初始化按键计时器
            _appStateManager.PressTimer = new DispatcherTimer();
            _appStateManager.PressTimer.Interval = TimeSpan.FromMilliseconds(10); // 每 10 毫秒检查一次
            _appStateManager.PressTimer.Tick += PressTimer_Tick; // 计时器回调
        }

        // 按键计时器的回调
        private void PressTimer_Tick(object sender, EventArgs e)
        {
            if (!_appStateManager.KeyPressStartTime.HasValue) // 如果没有按下时间
            {
                _appStateManager.PressTimer.Stop(); // 停止计时器
                return; // 如果没有按下时间，停止计时器
            }
            var Conventions = _appStateManager.Db.GetAllConventions().FirstOrDefault(); // 获取设置
            double LongPressThreshold = Conventions.LongPressThreshold / 1000.0; // 将毫秒转换为秒
            var OpenMainWindowConditions = _appStateManager.OpenMainWindowConditions; // 获取设置
            if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger ||
                OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger)
            {
                TimeSpan pressDuration = DateTime.Now - _appStateManager.KeyPressStartTime.Value; // 计算按键按下时间
                if (pressDuration.TotalSeconds >= LongPressThreshold)
                {
                    CloseOrShowMainWindow(); // 如果按键时间超过阈值，触发功能
                    _appStateManager.KeyPressStartTime = null; // 重置按键时间
                    _appStateManager.PressTimer.Stop(); // 停止计时器
                }
            } // 长按中键或右键
            else if (OpenMainWindowConditions.OpenMainWindowByRightMouseClick_Move)
            {
                System.Windows.Point currentPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
                double offsetX = currentPosition.X - _appStateManager.StartPosition.X; // 计算水平偏移量
                double offsetY = currentPosition.Y - _appStateManager.StartPosition.Y; // 计算垂直偏移量
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY); // 计算移动距离
                if (distance > Conventions.MouseMovePixels) // 如果移动距离大于设置像素值
                    CloseOrShowMainWindow(); // 关闭或显示主窗口
            } // 右键移动
        }

        // 定时器每5min保存使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            var Convention = _appStateManager.Db.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += 300; // 每 5 分钟增加 300 秒
            _appStateManager.Db.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时长到数据库
            _appStateManager.RecordedTime = DateTime.Now; // 记录应用保存时间
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
            if (_appStateManager.KeyPressStartTime.HasValue)
            {
                _appStateManager.KeyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = _appStateManager.OpenMainWindowConditions; // 获取设置
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
                        _appStateManager.StartPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
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
            _appStateManager.PressTimer?.Stop(); // 停止计时器
            if (!_appStateManager.KeyPressStartTime.HasValue) return;
            var OpenMainWindowConditions = _appStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = DateTime.Now - _appStateManager.KeyPressStartTime.Value; // 计算按键按下和释放的时间差
            _appStateManager.KeyPressStartTime = null;
            switch (e.Data.Button)
            {
                case SharpHook.Native.MouseButton.Button3:
                    if (pressDuration.TotalSeconds <= 0.3 &&
                        OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick)
                        CloseOrShowMainWindow();
                    break; // 短按中键
                case SharpHook.Native.MouseButton.Button4: // 短按X1键
                case SharpHook.Native.MouseButton.Button5:
                    if (OpenMainWindowConditions.OpenMainWindowByX1MouseClick ||
                        OpenMainWindowConditions.OpenMainWindowByX2MouseClick)
                    {
                        if (pressDuration.TotalSeconds <= 0.3)
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
            if (_appStateManager.KeyPressStartTime.HasValue)
            {
                _appStateManager.KeyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = _appStateManager.OpenMainWindowConditions; // 获取设置
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
            if (!_appStateManager.KeyPressStartTime.HasValue) return;
            var OpenMainWindowConditions = _appStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = DateTime.Now - _appStateManager.KeyPressStartTime.Value; // 计算按键按下和释放的时间差
            _appStateManager.KeyPressStartTime = null;
            switch (e.Data.KeyCode)
            {
                case SharpHook.Native.KeyCode.VcLeftControl: // 左 Ctrl 键
                case SharpHook.Native.KeyCode.VcRightControl:
                    if (OpenMainWindowConditions.OpenMainWindowByCtrl)
                    {
                        if (pressDuration.TotalSeconds <= 0.3) // 如果按键时间小于 0.3 秒
                        {
                            CloseOrShowMainWindow();
                        }
                    }
                    break; // 右 Ctrl 键
            }
        }

        // 是否全屏禁用Quicker
        private bool FullScreenDisable()
        {
            var blacklistSettings = _appStateManager.Db.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置
            if (!blacklistSettings.IsFullScreenDisabled) return false; // 如果没有启用全屏禁用Quicker，返回false
            if (_appStateManager.WindowManager.IsFullScreen()) // 窗口最大化
            {
                string processName = _appStateManager.WindowManager.GetProcessName(); // 获取进程名
                var blacklistApplications = _appStateManager.Db.GetAllBlacklistApplications(); // 获取黑名单进程
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
            nint foregroundWindow = _appStateManager.WindowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口句柄
            if (foregroundWindow == IntPtr.Zero) return false; // 没有前台窗口，返回false

            uint processId = _appStateManager.WindowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
            Process process = Process.GetProcessById((int)processId); // 获取进程
            string processName = process.ProcessName; // 获取进程名

            var blacklistedProcesses = _appStateManager.Db.GetAllBlacklistApplications(); // 获取黑名单进程
            if (blacklistedProcesses.Any(p => p.ProcessName == processName && p.IsInBlacklist)) // 如果进程名在黑名单中
                return true; // 返回true表示Quicker被禁用
            return false; // 返回false表示正常工作
        }

        // 弹出功能面板
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            PreLoadMainWindow(); // 预加载主窗口
            CloseOrShowMainWindow(); // 关闭或重新显示主窗口
        }

        // 预加载主窗口
        public void PreLoadMainWindow(bool startTimer = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                _appStateManager.KeyPressStartTime = DateTime.Now; // 记录按键按下时间
                if (startTimer) _appStateManager.PressTimer.Start(); // 启动按键计时器

                ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (actionPageManageWindow != null && actionPageManageWindow.WindowState != WindowState.Minimized) return; // 如果动作窗口打开，则不打开功能面板
                SettingWindow settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                if (settingWindow != null && settingWindow.WindowState != WindowState.Minimized) return; // 如果设置窗口打开，则不打开功能面板

                string windowType = DetermineWindowType(); // 确定窗口类型
                _appStateManager.PreLoadMainWindow = new MainWindow(windowType); // 创建新的功能面板

                var settings = _appStateManager.Db.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
                SetMainWindowPosition(_appStateManager.PreLoadMainWindow, settings.WindowStartupLocation); // 设置窗口位置
                _appStateManager.PreLoadMainWindow.Visibility = Visibility.Hidden; // 隐藏功能面板
                _appStateManager.Left = (float)_appStateManager.PreLoadMainWindow.Left; // 记录功能面板位置
                _appStateManager.Top = (float)_appStateManager.PreLoadMainWindow.Top; // 记录功能面板位置
            });
        }

        // 关闭或重新显示主窗口
        public void CloseOrShowMainWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (_appStateManager.PreLoadMainWindow == null) return; // 如果没有预加载窗口，返回
                _appStateManager.PreLoadMainWindow.Visibility = Visibility.Visible; // 显示功能面板
                _appStateManager.PreLoadMainWindow = null; // 清空预加载窗口
            });
        }

        // 确定窗口类型
        private string DetermineWindowType()
        {
            if (_appStateManager.Locked && _appStateManager.CommonState != null)
                return _appStateManager.CommonState; // 窗口类型为锁定状态
            else if
                 (IsMouseOnTaskbar()) return "Taskbar"; // 鼠标在任务栏上
            else if
                (IsMouseOnDesktop()) return "Desktop"; // 鼠标在桌面上
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
                    if (_appStateManager.Left != null && _appStateManager.Top != null) // 上次弹出位置
                    {
                        mainWindow.Left = _appStateManager.Left;
                        mainWindow.Top = _appStateManager.Top;
                    }
                    else
                        mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    break; // 上次弹出位置
            }
        }

        // 判断鼠标是否在任务栏上
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

        // 判断鼠标是否在桌面上
        public bool IsMouseOnDesktop()
        {
            IntPtr foregroundWindow = _appStateManager.WindowManager.GetCurrentForegroundWindow(); // 调用封装方法
            if (foregroundWindow == IntPtr.Zero) return true; // 没有前台窗口
            else return false; // 鼠标在桌面上
        }

        // 弹出菜单栏
        private void ShowCustomMenu(object sender, RoutedEventArgs e)
        {
            CustomMenu customMenu = Application.Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            var mousePosition = System.Windows.Forms.Control.MousePosition; // 获取鼠标位置
            var screenPosition = new System.Windows.Point(mousePosition.X, mousePosition.Y); // 获取屏幕位置
            customMenu.Visibility = Visibility.Hidden; // 隐藏菜单栏
            customMenu.Left = screenPosition.X / 2 + 340;
            customMenu.Top = screenPosition.Y / 2 + 65;
            customMenu.Visibility = Visibility.Visible; // 显示菜单栏
            customMenu.Activate();
        }

        // 暂停Quicker
        public async void PauseQuicker(object sender, RoutedEventArgs e)
        {
            var toastMessage = _appStateManager.Pause ? "Quicker已恢复" : "Quicker已暂停"; // 消息提醒
            var text = _appStateManager.Pause ? "暂停" : "恢复"; // 消息提醒
            CustomMenu customMenu = Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            customMenu.PauseQuickerTextBlock.Text = text; // 更新菜单栏文本
            ChangeTrayIcon(_appStateManager.Pause); // 切换托盘图标

            hook?.Dispose(); // 销毁当前钩子
            hook = null; // 清空钩子
            if (_appStateManager.Pause) await InitializeHookAsync(); // 重新初始化钩子

            _appStateManager.Pause = !_appStateManager.Pause; // 切换暂停状态
            new ToastContentBuilder().AddText(toastMessage).Show(); // 弹出消息提醒
        }

        /// <summary>
        /// 切换托盘图标
        /// </summary>
        /// <param name="isPaused"> 是否暂停 </param>
        public void ChangeTrayIcon(bool isPaused)
        {
            taskbarIcon.IconSource = isPaused ? _trayIcon1 : _trayIcon2; // 切换托盘图标
        }

        // 退出应用释放资源
        protected override void OnExit(ExitEventArgs e)
        {
            double currentSessionTime = (DateTime.Now - _appStateManager.RecordedTime).TotalSeconds; // 计算本次会话时间
            var Convention = _appStateManager.Db.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += currentSessionTime; // 增加本次会话时间
            _appStateManager.Db.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时间

            _appStateManager.Timer?.Stop(); // 停止定时器
            hook?.Dispose(); // 释放钩子
            taskbarIcon?.Dispose(); // 释放托盘图标
            _appStateManager.PreLoadMainWindow?.Close(); // 关闭主窗口

            SingleInstanceManager.ReleaseMutex(); // 释放互斥锁

            base.OnExit(e); // 调用基类的 OnExit 方法
        }
    }
}