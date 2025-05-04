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
using System.Text;
using SharpHook;
using Quicker;

namespace Quicker
{
    /*
                       _ooOoo_
                      o8888888o
                      88" . "88
                      (| -_- |)
                      O\  =  /O
                   ____/`---'\____
                 .'  \\|     |//  `.
                /  \\|||  :  |||//  \
               /  _||||| -:- |||||-  \
               |   | \\\  -  /// |   |
               | \_|  ''\---/''  |   |
               \      .-\__  `-`  ___/-. /
             ___`. .'  /--.--\  `. . __
          ."" '<  `.___\_<|>_/___.'  >'"".
         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
         \  \ `-.   \_ __\ /__ _/   .-` /  /
    ======`-.____`-.___\_____/___.-`____.-'======
                      `=---='
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                佛祖保佑       永无BUG
    */
    public partial class App : System.Windows.Application
    {
        private SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        private ButtonManager buttonManager = new ButtonManager(); // 按钮管理器
        private WindowManager windowManager = new WindowManager(); // 窗口管理器
        public bool Book = false, Pause = false, Locked = false; // 是否订住、暂停、锁定
        public static DateTime RecordedTime { get; set; } // 记录时间
        public static DateTime StartTime { get; set; } // 启动时间
        private DateTime? keyPressStartTime = null; // 按键按下时的时间
        private System.Windows.Point startPosition; // 鼠标位置
        private DispatcherTimer pressTimer; // 按键计时器
        private TaskbarIcon? taskbarIcon; // 托盘图标
        private TaskPoolGlobalHook? hook; // 钩子
        private DispatcherTimer timer; // 定时器
        public string CommonState; // 通用状态
        private float Left, Top; // 窗口位置

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // 调用基类的 OnStartup 方法
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
            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取设置
            if (Convention.ShowNotification) // 如果设置中允许显示消息提醒
                new ToastContentBuilder().AddText("成功启动！").Show(); // 弹出消息提醒
        }

        // 初始化定时器
        private void InitializeTimer()
        {
            StartTime = DateTime.Now; // 记录应用启动时间
            RecordedTime = StartTime; // 记录应用记录时间

            // 初始化定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(5); // 每 5 分钟更新一次
            timer.Tick += Timer_Tick; // 每 5 分钟触发一次
            timer.Start(); // 启动定时器

            // 初始化按键计时器
            pressTimer = new DispatcherTimer();
            pressTimer.Interval = TimeSpan.FromMilliseconds(10); // 每 10 毫秒检查一次
            pressTimer.Tick += PressTimer_Tick; // 计时器回调
        }

        // 按键计时器的回调
        private void PressTimer_Tick(object sender, EventArgs e)
        {
            if (!keyPressStartTime.HasValue) // 如果没有按下时间
            {
                pressTimer.Stop(); // 停止计时器
                return; // 如果没有按下时间，停止计时器
            }
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置
            double LongPressThreshold = Conventions.LongPressThreshold / 1000.0; // 将毫秒转换为秒
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
            if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger ||
                OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger)
            {
                TimeSpan pressDuration = DateTime.Now - keyPressStartTime.Value; // 计算按键按下时间
                if (pressDuration.TotalSeconds >= LongPressThreshold)
                {
                    this.Dispatcher.Invoke(CloseOrShowMainWindow); // 如果按键时间超过阈值，触发功能
                    keyPressStartTime = null; // 重置按键时间
                    pressTimer.Stop(); // 停止计时器
                }
            } // 长按中键或右键
            else if (OpenMainWindowConditions.OpenMainWindowByRightMouseClick_Move)
            {
                System.Windows.Point currentPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
                double offsetX = currentPosition.X - startPosition.X; // 计算水平偏移量
                double offsetY = currentPosition.Y - startPosition.Y; // 计算垂直偏移量
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY); // 计算移动距离
                if (distance > Conventions.MouseMovePixels) // 如果移动距离大于设置像素值
                    this.Dispatcher.Invoke(CloseOrShowMainWindow); // 关闭或显示主窗口
            } // 右键移动
        }

        // 定时器每5min保存使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += 300; // 每 5 分钟增加 300 秒
            db1.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时长到数据库
            RecordedTime = DateTime.Now; // 记录应用保存时间
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
            if (keyPressStartTime.HasValue)
            {
                keyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
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
                        startPosition = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // 获取当前鼠标位置
                        keyPressStartTime = DateTime.Now;
                        PreLoadMainWindow();
                        pressTimer.Start();
                        break;
                    } // 右键移动
                    if (isCtrlPressed && OpenMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick)
                    {
                        CloseOrShowMainWindow();
                        break;
                    } // Ctrl + 右键
                    if (OpenMainWindowConditions.OpenMainWindowByRightMouseClickLonger)
                    {
                        keyPressStartTime = System.DateTime.Now;
                        PreLoadMainWindow();
                        pressTimer.Start();
                    } // 长按右键
                    break; // 右键
                case SharpHook.Native.MouseButton.Button3:
                    if (isCtrlPressed && OpenMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick)
                    {
                        CloseOrShowMainWindow();
                        break;
                    } // Ctrl + 中键
                    if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick)
                    {
                        keyPressStartTime = DateTime.Now;
                        PreLoadMainWindow();
                        break;
                    } // 短按中键
                    if (OpenMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger)
                    {
                        keyPressStartTime = System.DateTime.Now;
                        PreLoadMainWindow();
                        pressTimer.Start();
                    } // 长按中键
                    break; // 中键
                case SharpHook.Native.MouseButton.Button4:
                    if (OpenMainWindowConditions.OpenMainWindowByX1MouseClick)
                    {
                        keyPressStartTime = DateTime.Now;
                        PreLoadMainWindow();
                    }
                    break; // X1键
                case SharpHook.Native.MouseButton.Button5:
                    if (OpenMainWindowConditions.OpenMainWindowByX2MouseClick)
                    {
                        keyPressStartTime = DateTime.Now;
                        PreLoadMainWindow();
                    }
                    break; // X2键
            }
        }

        // 松开鼠标满足条件弹出面板
        private void Hook_MouseReleased(object? sender, MouseHookEventArgs e)
        {
            pressTimer?.Stop(); // 停止计时器
            if (!keyPressStartTime.HasValue) return;
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
            TimeSpan pressDuration = DateTime.Now - keyPressStartTime.Value; // 计算按键按下和释放的时间差
            keyPressStartTime = null;
            switch (e.Data.Button)
            {
                case SharpHook.Native.MouseButton.Button3:
                    if (pressDuration.TotalSeconds <= 0.3 &&
                        OpenMainWindowConditions.OpenMainWindowByMiddleMouseClick)
                    {
                        this.Dispatcher.Invoke(CloseOrShowMainWindow);
                    }
                    break; // 短按中键
                case SharpHook.Native.MouseButton.Button4: // 短按X1键
                case SharpHook.Native.MouseButton.Button5:
                    if (OpenMainWindowConditions.OpenMainWindowByX1MouseClick ||
                        OpenMainWindowConditions.OpenMainWindowByX2MouseClick)
                    {
                        if (pressDuration.TotalSeconds <= 0.3)
                        {
                            this.Dispatcher.Invoke(CloseOrShowMainWindow);
                        }
                    }
                    break; // 短按X2键
            }
        }

        // 按下键盘快捷键时如果按键尚未被记录，记录按键按下的时间
        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e)
        {
            if (IsBannedFormQuicker()) return; // 如果禁用Quicker，返回
            if (FullScreenDisable()) return; // 如果全屏禁用Quicker，返回
            if (keyPressStartTime.HasValue)
            {
                keyPressStartTime = null; // 重置按键时间
                return; // 返回
            } // 如果按键已经被记录，停止记录
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
            switch (e.Data.KeyCode)
            {
                case SharpHook.Native.KeyCode.VcLeftControl: // 左 Ctrl 键
                case SharpHook.Native.KeyCode.VcRightControl:
                    if (OpenMainWindowConditions.OpenMainWindowByCtrl)
                    {
                        keyPressStartTime = DateTime.Now;
                        PreLoadMainWindow();
                    }
                    break; // 右 Ctrl 键
            }
        }

        // 松开按键满足条件弹出面板
        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e)
        {
            if (!keyPressStartTime.HasValue) return;
            var OpenMainWindowConditions = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
            TimeSpan pressDuration = DateTime.Now - keyPressStartTime.Value; // 计算按键按下和释放的时间差
            keyPressStartTime = null;
            switch (e.Data.KeyCode)
            {
                case SharpHook.Native.KeyCode.VcLeftControl: // 左 Ctrl 键
                case SharpHook.Native.KeyCode.VcRightControl:
                    if (OpenMainWindowConditions.OpenMainWindowByCtrl)
                    {
                        if (pressDuration.TotalSeconds <= 0.3) // 如果按键时间小于 0.3 秒
                        {
                            this.Dispatcher.Invoke(CloseOrShowMainWindow);
                        }
                    }
                    break; // 右 Ctrl 键
            }
        }

        // 是否全屏禁用Quicker
        private bool FullScreenDisable()
        {
            var blacklistSettings = db1.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置
            if (!blacklistSettings.IsFullScreenDisabled) return false; // 如果没有启用全屏禁用Quicker，返回false
            nint foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口句柄
            if (foregroundWindow == IntPtr.Zero) return false; // 没有前台窗口，返回false
            if (windowManager.IsFullScreen()) // 窗口最大化
            {
                uint processId = windowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
                Process process = Process.GetProcessById((int)processId); // 获取进程
                string processName = process.ProcessName; // 获取进程名
                var blacklistApplications = db1.GetAllBlacklistApplications(); // 获取黑名单进程
                if (blacklistApplications.Count == 0) return true; // 没有黑名单进程，返回true表示Quicker被禁用
                if (blacklistApplications.Any(p => p.ProcessName == processName && !p.IsInBlacklist)) // 如果进程名在黑名单中
                    return false; // 返回false表示正常工作
            }
            return false; // 返回false表示正常工作
        }

        // 是否禁用Quicker
        private bool IsBannedFormQuicker()
        {
            nint foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口句柄
            if (foregroundWindow == IntPtr.Zero) return false; // 没有前台窗口，返回false

            uint processId = windowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
            Process process = Process.GetProcessById((int)processId); // 获取进程
            string processName = process.ProcessName; // 获取进程名

            var blacklistedProcesses = db1.GetAllBlacklistApplications(); // 获取黑名单进程
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
        public void PreLoadMainWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找现有的功能面板
                if (mainWindow == null)
                {
                    ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                    if (actionPageManageWindow != null && actionPageManageWindow.WindowState != WindowState.Minimized) return; // 如果动作窗口打开，则不打开功能面板
                    SettingWindow settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                    if (settingWindow != null && settingWindow.WindowState != WindowState.Minimized) return; // 如果设置窗口打开，则不打开功能面板

                    string windowType = DetermineWindowType(); // 确定窗口类型
                    mainWindow = new MainWindow(windowType); // 创建新的功能面板

                    var settings = db1.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取设置
                    SetMainWindowPosition(mainWindow, settings.WindowStartupLocation); // 设置窗口位置
                    mainWindow.Visibility = Visibility.Hidden; // 隐藏功能面板
                    Left = (float)mainWindow.Left; // 记录功能面板位置
                    Top = (float)mainWindow.Top; // 记录功能面板位置
                }
            });
        }

        // 关闭或重新显示主窗口
        public void CloseOrShowMainWindow()
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找现有的功能面板
            if(mainWindow == null) return; // 如果没有主窗口，则不处理
            if (mainWindow.Visibility == Visibility.Hidden)
            {
                mainWindow.Visibility = Visibility.Visible; // 显示功能面板
                mainWindow.Activate(); // 激活功能面板
            }
            else
            {
                if (!Book) mainWindow.Close(); // 关闭功能面板
            }
        }

        // 确定窗口类型
        private string DetermineWindowType()
        {
            if (Locked && CommonState != null) return CommonState; // 窗口类型为锁定状态
            else if (IsMouseOnTaskbar()) return "Taskbar"; // 鼠标在任务栏上
            else if (IsMouseOnDesktop()) return "Desktop"; // 鼠标在桌面上
            else return "Common"; // 鼠标在其他窗口上
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
                    if (Left != null && Top != null) // 上次弹出位置
                    {
                        mainWindow.Left = Left;
                        mainWindow.Top = Top;
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
            IntPtr foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 调用封装方法
            if (foregroundWindow == IntPtr.Zero) return true; // 没有前台窗口
            else return false; // 鼠标在桌面上
        }

        // 弹出菜单栏
        private void ShowCustomMenu(object sender, RoutedEventArgs e)
        {
            CustomMenu customMenu = Application.Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            var mousePosition = System.Windows.Forms.Control.MousePosition; // 获取鼠标位置
            var screenPosition = new System.Windows.Point(mousePosition.X, mousePosition.Y); // 获取屏幕位置
            /* 终极版本数据
                      customMenu.Left = screenPosition.X / 2 + 340;
                      customMenu.Top = screenPosition.Y / 2 - 110;*/
            customMenu.Visibility = Visibility.Hidden; // 隐藏菜单栏
            customMenu.Left = screenPosition.X / 2 + 340;
            customMenu.Top = screenPosition.Y / 2 + 65;
            customMenu.Visibility = Visibility.Visible; // 显示菜单栏
            customMenu.Activate();
        }

        // 暂停Quicker
        public async void PauseQuicker(object sender, RoutedEventArgs e)
        {
            var toastMessage = Pause ? "Quicker已恢复" : "Quicker已暂停"; // 消息提醒
            var text = Pause ? "暂停" : "恢复"; // 消息提醒
            var icon1 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker1.ico", UriKind.Relative));
            var icon2 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker2.ico", UriKind.Relative));
            CustomMenu customMenu = Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
            customMenu.PauseQuickerTextBlock.Text = text; // 更新菜单栏文本
            ChangeTrayIcon(Pause ? icon1 : icon2); // 切换托盘图标

            hook?.Dispose(); // 销毁当前钩子
            hook = null; // 清空钩子
            if (Pause) InitializeHookAsync(); // 重新初始化钩子

            Pause = !Pause; // 切换暂停状态
            new ToastContentBuilder().AddText(toastMessage).Show(); // 弹出消息提醒
        }

        // 切换托盘图标
        public void ChangeTrayIcon(BitmapImage newIcon)
        {
            taskbarIcon.IconSource = newIcon; // 切换托盘图标
        }

        // 退出应用释放资源
        protected override void OnExit(ExitEventArgs e)
        {
            double currentSessionTime = (DateTime.Now - RecordedTime).TotalSeconds; // 计算本次会话时间
            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取设置
            Convention.TotalUsageTime += currentSessionTime; // 增加本次会话时间
            db1.SaveTotalUsageTime(Convention.TotalUsageTime); // 保存总使用时间

            timer?.Stop(); // 停止定时器            
            hook?.Dispose(); // 释放钩子           
            taskbarIcon?.Dispose(); // 释放托盘图标                      
            MainWindow?.Close(); // 关闭主窗口

            base.OnExit(e); // 调用基类的 OnExit 方法
        }
    }
}