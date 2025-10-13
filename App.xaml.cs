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
using KeyboardHookEventArgs = SharpHook.KeyboardHookEventArgs;
using VisualTreeHelper = Quicker.Helpers.VisualTreeHelper;
using MouseHookEventArgs = SharpHook.MouseHookEventArgs;
using MouseButton = SharpHook.Data.MouseButton;
using Quicker.Windows.MainWindows.MainWindow;
using Hardcodet.Wpf.TaskbarNotification;
using KeyCode = SharpHook.Data.KeyCode;
using Quicker.Windows.MainWindows;
using Quicker.Windows.ToolWindows;
using Quicker.Database.Upgrade;
using Quicker.Models.Settings;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using Quicker.Database.Core;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using Quicker.Helpers;
using System.Windows;
using SharpHook.Data;
using SharpHook;
using System.IO;

namespace Quicker
{
    public partial class App : Application
    {
        // 常量定义
        private const string DEFAULT_STYLE = "common";
        private const string TASKBAR_STYLE = "taskbar";
        private const string DESKTOP_STYLE = "desktop";

        private Action<string, string> _trayIconChangedHandler; // 托盘图标改变事件
        private string _previewRunningPath; // 预览运行图标路径
        private string _previewPausedPath; // 预览暂停图标路径
        private TaskPoolGlobalHook? hook; // 全局钩子
        private TaskbarIcon? taskbarIcon; // 托盘图标
       

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); // 调用基类方法
            InitializeAppDirectories(); // 初始化应用程序目录
            EnsureSingleInstance(); // 确保单例运行
            CheckAppUpdate(); // 检查应用更新
            CheckAndUpdateDatabase(); // 检查并升级数据库
            RestoreGlobalFontFamilyFromDatabase(); // 恢复全局字体设置
            InitializeTimer(); // 初始化定时器
            InitializeTaskbar(); // 初始化托盘图标
            InitializeHookAsync(); // 初始化钩子
            ShowNotification(); // 弹出消息提醒

            this.MainWindow = null; // 设置主窗口为null，避免应用程序在没有窗口时退出
        }

        // 初始化应用程序目录
        private void InitializeAppDirectories()
        {
            AppPathHelper.EnsureAllDirectoriesExist();
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
            }
        }

        // 检查并升级数据库
        private void CheckAndUpdateDatabase()
        {
            using var databaseUpdater = new DatabaseUpdateManager(); // 数据库更新管理器
            databaseUpdater.CheckAndUpgradeDatabase(); // 检查并升级数据库
        }

        // 检查应用更新
        private async void CheckAppUpdate()
        {
            await Task.Run(() =>
            {
                using var updateManager = new AppUpdateManager(); // 创建更新管理器
                updateManager.CheckForUpdate(); // 检查并安装更新
            });
        }

        // 初始化托盘图标
        private void InitializeTaskbar()
        {
            var icon = AppStateManager.TrayIcon; // 设置托盘图标
            Current.Resources["AppIcon"] = icon; // 设置应用图标
            taskbarIcon = new TaskbarIcon // 创建托盘图标
            {
                IconSource = icon, // 设置图标
                ToolTipText = "Quicker" // 设置提示文本
            }; // 创建托盘图标

            taskbarIcon.TrayLeftMouseDown += ShowMainWindow; // 左键单击弹出功能面板
            taskbarIcon.TrayRightMouseDown += ShowCustomMenu; // 右键单击弹出菜单栏
            taskbarIcon.TrayMouseDoubleClick += PauseQuicker; // 双击暂停Quicker

            _trayIconChangedHandler = (running, paused) =>
            {
                _previewRunningPath = running;
                _previewPausedPath = paused;
                taskbarIcon.IconSource = AppStateManager.GetTrayIcon(_previewRunningPath, _previewPausedPath, AppStateManager.Pause);
            };
            AppStateManager.TrayIconChanged += _trayIconChangedHandler;
        }

        // 弹出消息提醒
        private async void ShowNotification()
        {
            await Task.Run(() =>
            {
                var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置
                if (Convention.ShowNotification) // 如果设置中允许显示消息提醒
                {
                    using var toast = new ToastManager(); // 消息提醒管理器
                    toast.Show("成功启动！", ToastType.Common); // 弹出消息提醒
                }
            }); // 异步执行
        }

        // 重载全局字体设置
        private void RestoreGlobalFontFamilyFromDatabase()
        {
            // 读取 Appearance 设置
            var appearance = SettingDatabase.GetAllAppearanceSettings().FirstOrDefault();
            if (appearance == null)
            {
                using var toast = new ToastManager();
                toast.Show("获取字体设置失败！", ToastType.Error);
                return;
            }

            // 获取系统字体列表
            var fontFamilies = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();
            fontFamilies.Add("(系统默认)"); // 保持和界面一致

            // 判断索引是否为最后一项（即"系统默认"）
            string font1 = (appearance.Font1 >= 0 && appearance.Font1 < fontFamilies.Count - 1) ? fontFamilies[appearance.Font1] : null;
            string font2 = (appearance.Font2 >= 0 && appearance.Font2 < fontFamilies.Count - 1) ? fontFamilies[appearance.Font2] : null;
            var fontFamily = (!string.IsNullOrEmpty(font1), !string.IsNullOrEmpty(font2)) switch
            {
                (true, true) => new FontFamily($"{font1}, {font2}"),
                (true, false) => new FontFamily(font1),
                (false, true) => new FontFamily(font2),
                _ => new FontFamily("微软雅黑")
            };
            Current.Resources["GlobalFontFamily"] = fontFamily; // 设置全局资源
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
            if (pressDuration.TotalMilliseconds >= conventions.LongPressThreshold) // 直接使用毫秒比较
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

        /// <summary>
        /// 触发按键按下事件
        /// 供 GlobalHookHelper 手动触发
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RaiseMousePressed(MouseButton btn, int x, int y)
        {
            if (!CanProcessHook()) return;
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions;
            if (GetCtrlKeyState() &&
                ((btn == MouseButton.Button2 && openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick) ||
                (btn == MouseButton.Button3 && openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick)))
            {
                CloseOrShowMainWindow();
                return;
            }
            if (AppStateManager.MousePressStartTime.HasValue) return;
            ProcessMouseButtonPress(btn, openMainWindowConditions);
        }

        // 鼠标按下事件
        private void Hook_MousePressed(object? sender, MouseHookEventArgs e)
        {
            if (!CanProcessHook()) return; // 如果不能处理钩子，返回
            // 优先处理Ctrl+鼠标的情况
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            if (GetCtrlKeyState() &&
                ((e.Data.Button == MouseButton.Button2 && openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick) ||
                (e.Data.Button == MouseButton.Button3 && openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick)))
            {
                CloseOrShowMainWindow();
                return;
            }

            // 其他情况
            if (AppStateManager.MousePressStartTime.HasValue) return; // 如果鼠标按下时间已记录，返回
            ProcessMouseButtonPress(e.Data.Button, openMainWindowConditions); // 处理鼠标按下事件
        }

        // 鼠标松开事件
        private async void Hook_MouseReleased(object? sender, MouseHookEventArgs e)
        {
            AppStateManager.PressTimer?.Stop(); // 停止计时器
            if (!AppStateManager.MousePressStartTime.HasValue) return; // 如果鼠标按下时间未记录，返回
            var pressDuration = GetMousePressDuration(); // 获取鼠标按下时间
            var conventions = AppStateManager.Conventions; // 获取设置
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            ProcessMouseButtonRelease(e.Data.Button, openMainWindowConditions, conventions, pressDuration); // 处理鼠标松开事件
        }

        // 按键按下事件
        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e)
        {
            if (!CanProcessHook()) return; // 如果不能处理钩子，返回
            if (AppStateManager.KeyPressStartTime.HasValue) return; // 如果按键已经记录，返回

            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            ProcessControlKeyPress(e.Data.KeyCode, openMainWindowConditions); // 处理按键按下事件
        }

        // 按键松开事件
        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e)
        {
            if (!AppStateManager.KeyPressStartTime.HasValue) return; // 如果按键时间未记录，返回
            var conventions = AppStateManager.Conventions; // 获取设置
            var openMainWindowConditions = AppStateManager.OpenMainWindowConditions; // 获取设置
            TimeSpan pressDuration = GetKeyPressDuration(); // 获取按键按下时间

            if (!IsOtherKeyDown())// 如果没有除Ctrl以外的其他按键被按下，处理按键松开事件
                ProcessControlKeyRelease(e.Data.KeyCode, openMainWindowConditions, conventions, pressDuration); // 处理按键松开事件
        }

        /// <summary>
        /// 判断是否有除Ctrl以外的其他任意按键被按下
        /// </summary>
        /// <returns>有则返回true，否则返回false</returns>
        private bool IsOtherKeyDown()
        {
            bool otherKeyDown = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                // 使用LINQ检查是否有任何常用按键被按下
                otherKeyDown = new[]
                {
                    // 字母键 A-Z
                    Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J,
                    Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T,
                    Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z,
                    // 数字键 0-9
                    Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
                    // 功能键 F1-F12
                    Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                    // 导航和编辑键
                    Key.Space, Key.Enter, Key.Tab, Key.Escape, Key.Back, Key.Delete, Key.Insert,
                    Key.Up, Key.Down, Key.Left, Key.Right, Key.Home, Key.End, Key.PageUp, Key.PageDown,
                    Key.PrintScreen, Key.Scroll, Key.Pause,
                    // 数字键盘
                    Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4,
                    Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9,
                    Key.Multiply, Key.Add, Key.Separator, Key.Subtract, Key.Decimal, Key.Divide
                }.Any(key => Keyboard.IsKeyDown(key));
            }));
            return otherKeyDown;
        }

        /// <summary>
        /// 判断是否可以处理钩子
        /// </summary>
        /// <returns> 是否可以处理钩子 </returns>
        private bool CanProcessHook()
        {
            return !IsBannedFormQuicker() && !FullScreenDisable() && !IsMouseOnTestButton(); // 如果禁用Quicker或全屏禁用Quicker或鼠标在测试按键区域上，返回
        }

        /// <summary>
        /// 检查鼠标是否在测试按键区域上
        /// </summary>
        /// <returns> 鼠标是否在测试按键区域上 </returns>
        private bool IsMouseOnTestButton()
        {
            bool isMouseOnTestButton = false;
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    var mousePosition = System.Windows.Forms.Cursor.Position; // 获取当前鼠标位置
                    var settingWindows = Application.Current.Windows.OfType<SettingWindow>(); // 检查是否有设置窗口打开
                    foreach (var settingWindow in settingWindows)
                    {
                        if (settingWindow.IsVisible)
                        {
                            var testButton = VisualTreeHelper.FindButtonByName(settingWindow, "TestButton"); // 递归查找名为 "TestButton" 的按钮
                            if (testButton != null)
                            {
                                var buttonPoint = testButton.PointFromScreen(new System.Windows.Point(mousePosition.X, mousePosition.Y)); // 将屏幕坐标转换为按钮坐标
                                if (buttonPoint.X >= 0 && buttonPoint.X <= testButton.ActualWidth &&
                                    buttonPoint.Y >= 0 && buttonPoint.Y <= testButton.ActualHeight) // 检查鼠标是否在按钮范围内
                                {
                                    isMouseOnTestButton = true; // 设置鼠标在测试按钮上
                                    break; // 找到后退出循环
                                }
                            }
                        }
                    }
                }));
            }
            catch { }
            return isMouseOnTestButton; // 返回鼠标是否在测试按钮上
        }

        /// <summary>
        /// 获取 Ctrl 键状态
        /// </summary>
        /// <returns> Ctrl 键状态 </returns>
        private bool GetCtrlKeyState()
        {
            bool isCtrlPressed = false; // 是否按下 Ctrl 键
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); // 获取 Ctrl 键状态
                })); // 获取 Ctrl 键状态
            }
            catch { }
            return isCtrlPressed; // 返回 Ctrl 键状态
        }

        /// <summary>
        /// 获取鼠标按下时间
        /// </summary>
        /// <returns> 鼠标按下时间 </returns>
        private TimeSpan GetMousePressDuration()
        {
            var duration = DateTime.Now - AppStateManager.MousePressStartTime.Value; // 计算鼠标按下时间
            AppStateManager.MousePressStartTime = null; // 重置鼠标按下时间
            bool ctrlDown = false; // 是否按下Ctrl键
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); // 获取Ctrl键状态
                }));
            }
            catch { }
            if (!ctrlDown)
            {
                AppStateManager.KeyPressStartTime = null; // 重置按键按下时间
            }
            return duration; // 返回鼠标按下时间
        }

        /// <summary>
        /// 获取按键按下时间
        /// </summary>
        /// <returns> 按键按下时间 </returns>
        private TimeSpan GetKeyPressDuration()
        {
            var duration = DateTime.Now - AppStateManager.KeyPressStartTime.Value; // 计算按键按下时间
            AppStateManager.KeyPressStartTime = null; // 重置按键按下时间
            // 只有没有鼠标按下时才清空
            if (!IsAnyMouseButtonDown())
            {
                AppStateManager.MousePressStartTime = null; // 重置鼠标按下时间
            }
            return duration; // 返回按键按下时间
        }

        /// <summary>
        /// 判断是否有鼠标按下
        /// </summary>
        /// <returns> 是否有鼠标按下 </returns>
        private bool IsAnyMouseButtonDown()
        {
            return System.Windows.Forms.Control.MouseButtons != System.Windows.Forms.MouseButtons.None; // 返回鼠标按下状态
        }

        /// <summary>
        /// 处理鼠标按下事件
        /// </summary>
        /// <param name="button"> 鼠标按钮 </param>
        /// <param name="conditions"> 设置 </param>
        private void ProcessMouseButtonPress(MouseButton button, OpenMainWindow conditions)
        {
            switch (button)
            {
                case MouseButton.Button2:
                    ProcessRightMouseButtonPress(conditions);
                    break; // 右键按下
                case MouseButton.Button3:
                    ProcessMiddleMouseButtonPress(conditions);
                    break; // 中键按下
                case MouseButton.Button4:
                    ProcessX1MouseButtonPress(conditions);
                    break; // X1鼠标按下
                case MouseButton.Button5:
                    ProcessX2MouseButtonPress(conditions);
                    break; // X2鼠标按下
            }
        }

        /// <summary>
        /// 处理右键按下事件
        /// </summary>
        /// <param name="conditions"> 设置 </param>
        private void ProcessRightMouseButtonPress(OpenMainWindow conditions)
        {
            if (conditions.OpenMainWindowByRightMouseClick_Move)
            {
                RecordMousePosition(); // 记录鼠标位置
                PreLoadMainWindow(true); // 预加载主窗口
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
        private void ProcessMiddleMouseButtonPress(OpenMainWindow conditions)
        {
            if (conditions.OpenMainWindowByMiddleMouseClick)
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
        private void ProcessMouseButtonRelease(MouseButton button, OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            switch (button)
            {
                case MouseButton.Button3:
                    ProcessMiddleMouseButtonRelease(conditions, conventions, pressDuration);
                    break; // 中键松开
                case MouseButton.Button4:
                case MouseButton.Button5:
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
            if (pressDuration.TotalMilliseconds <= conventions.LongPressThreshold &&
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
                pressDuration.TotalMilliseconds <= conventions.LongPressThreshold) // 使用毫秒比较
            {
                CloseOrShowMainWindow(); // 关闭或显示主窗口
            }
        }

        /// <summary>
        /// 处理 Ctrl 按键按下事件
        /// </summary>
        /// <param name="keyCode"> 按键代码 </param>
        /// <param name="conditions"> 设置 </param>
        private void ProcessControlKeyPress(KeyCode keyCode, OpenMainWindow conditions)
        {
            if ((keyCode == KeyCode.VcLeftControl ||
                 keyCode == KeyCode.VcRightControl) &&
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
        private void ProcessControlKeyRelease(KeyCode keyCode, OpenMainWindow conditions, Convention conventions, TimeSpan pressDuration)
        {
            if ((keyCode == KeyCode.VcLeftControl ||
                 keyCode == KeyCode.VcRightControl) &&
                conditions.OpenMainWindowByCtrl &&
                pressDuration.TotalMilliseconds <= conventions.LongPressThreshold)
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
            try
            {
                using var windowManager = new WindowManager(); // 创建窗口管理器
                nint foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口
                if (foregroundWindow == IntPtr.Zero) return false; // 如果当前前台窗口为空，返回

                uint processId = windowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
                string processName = Process.GetProcessById((int)processId).ProcessName; // 获取进程名称

                return AppStateManager.BlacklistApplications
                    .Any(p => p.ProcessName == processName && p.IsInBlacklist); // 如果进程名称在黑名单中，返回
            }
            catch
            {
                return false; // 如果发生异常，返回 false
            }
        }

        // 弹出主窗口
        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            PreLoadMainWindow(); // 预加载主窗口
            CloseOrShowMainWindow(); // 关闭或显示主窗口
        }

        /// <summary>
        /// 预加载主窗口
        /// </summary>
        /// <param name="startTimer"> 是否启动计时器 </param>
        public void PreLoadMainWindow(bool startTimer = false)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                DateTime dateTime = DateTime.Now; // 获取当前时间
                AppStateManager.KeyPressStartTime = dateTime; // 记录鼠标按下时间
                AppStateManager.MousePressStartTime = dateTime; // 记录鼠标按下时间
                if (startTimer) AppStateManager.PressTimer.Start(); // 启动按键计时器

                string windowType = DetermineWindowType() ?? DEFAULT_STYLE; // 确保不为null
                AppStateManager.PreLoadMainWindow = new MainWindow(windowType); // 创建主窗口

                var settings = AppStateManager.OpenMainWindowConditions; // 获取设置
                SetMainWindowPosition(AppStateManager.PreLoadMainWindow, settings.WindowStartupLocation); // 设置窗口位置
                AppStateManager.PreLoadMainWindow.Visibility = Visibility.Hidden; // 隐藏主窗口
                AppStateManager.Left = (float)AppStateManager.PreLoadMainWindow.Left; // 记录主窗口位置
                AppStateManager.Top = (float)AppStateManager.PreLoadMainWindow.Top; // 记录主窗口位置
            }));
        }

        // 关闭或显示主窗口
        public void CloseOrShowMainWindow()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (AppStateManager.PreLoadMainWindow == null) return; // 如果预加载窗口为空，返回
                var mainWindowList = Application.Current.Windows.OfType<MainWindow>(); // 获取主窗口列表
                if (mainWindowList.Any(window => window.Visibility == Visibility.Visible)) // 是否有可见窗口
                {
                    AppStateManager.PreLoadMainWindow.Close(); // 有可见窗口且处于固定状态，关闭预加载窗口
                    if (!AppStateManager.MainWindowPinned) // 如果是暂停状态，关闭所有主窗口
                        foreach (var window in mainWindowList)
                            window.Close(); // 关闭其他窗口
                }
                else // 否则显示预加载窗口
                    AppStateManager.PreLoadMainWindow.Visibility = Visibility.Visible;
                AppStateManager.PreLoadMainWindow = null; // 清空预加载窗口
            }));
        }

        /// <summary>
        /// 确定窗口类型
        /// </summary>
        /// <returns> 窗口类型 </returns>
        private string DetermineWindowType()
        {
            if (AppStateManager.Locked)
            {
                return AppStateManager.CommonState ?? DEFAULT_STYLE;
            }

            using var windowManager = new WindowManager(); // 使用单个WindowManager实例来处理所有窗口相关操作
            // 先检查进程场景类型
            string processSceneType = GetProcessSceneType(windowManager);
            if (!string.IsNullOrEmpty(processSceneType))
                return processSceneType;

            // 再检查鼠标位置类型
            string mouseLocationType = GetMouseLocationType(windowManager);
            return mouseLocationType;
        }

        /// <summary>
        /// 获取鼠标位置类型
        /// </summary>
        /// <param name="windowManager">窗口管理器实例</param>
        /// <returns>鼠标位置类型</returns>
        private string GetMouseLocationType(WindowManager windowManager)
        {
            if (windowManager.IsMouseOnTaskbar())
                return TASKBAR_STYLE; // 鼠标在任务栏上
            else if (windowManager.IsMouseOnDesktop())
                return DESKTOP_STYLE; // 鼠标在桌面上
            else
                return DEFAULT_STYLE; // 鼠标在其他窗口上
        }

        /// <summary>
        /// 获取进程对应的场景类型
        /// </summary>
        /// <param name="windowManager">窗口管理器实例</param>
        /// <returns>如果找到对应的场景类型则返回，否则返回空字符串</returns>
        private string GetProcessSceneType(WindowManager windowManager)
        {
            string result = string.Empty;
            IntPtr foregroundWindow = windowManager.GetCurrentForegroundWindow(); // 获取当前前台窗口
            if (foregroundWindow == IntPtr.Zero) return result; // 无前台窗口
            try
            {
                uint processId = windowManager.GetWindowProcessId(foregroundWindow); // 获取窗口进程ID
                using var process = Process.GetProcessById((int)processId); // 获取进程

                // 访问 MainModule 可能因权限不足抛出 Win32Exception（拒绝访问）或进程退出等异常
                string processFilePath = string.Empty;
                try
                {
                    processFilePath = process.MainModule.FileName; // 获取进程文件路径
                }
                catch
                {
                    // 无法访问主模块则保持默认结果
                }

                if (!string.IsNullOrEmpty(processFilePath))
                {
                    string processFileName = Path.GetFileNameWithoutExtension(processFilePath).ToLower(); // 获取进程文件名（不含后缀）

                    var db = new ActionPageDatabase(); // 检查是否存在以 文件名+"Scene" 命名的表
                    if (db.SceneExists(processFileName))
                    {
                        var scene = db.GetSceneData(processFileName); // 获取场景数据
                        if (scene != null)
                        {
                            // 路径相同则返回文件名（带后缀）
                            if (string.Equals(scene.SceneProcess, processFilePath, StringComparison.OrdinalIgnoreCase) && scene.SceneCount > 0)
                            {
                                result = processFileName;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 包含：Win32Exception(拒绝访问)、ArgumentException(进程不存在)、InvalidOperationException 等
                // 任意异常均保持默认结果
            }

            return result;
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
                    mainWindow.PositionWindowAtMouse(); // 窗口打开位置跟随鼠标
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

        // 弹出菜单栏
        private void ShowCustomMenu(object sender, RoutedEventArgs e)
        {
            var customMenu = new CustomMenu(); // 创建新菜单
            customMenu.ShowWithAnimation(); // 使用淡入动画显示菜单栏
            customMenu.Activate(); // 激活菜单
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
                toast.Show("请勿频繁操作！", ToastType.Warning); // 弹出消息提醒
            }

            var toastMessage = AppStateManager.Pause ? "Quicker已恢复" : "Quicker已暂停"; // 消息提醒
            var text = AppStateManager.Pause ? "暂停" : "恢复"; // 消息提醒
            AppStateManager.Pause = !AppStateManager.Pause; // 切换暂停状态
            taskbarIcon.IconSource = AppStateManager.TrayIcon; // 切换托盘图标
            toast.Show(toastMessage, AppStateManager.Pause ? ToastType.Common : ToastType.Success); // 弹出消息提醒
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
            if (_trayIconChangedHandler != null)
                AppStateManager.TrayIconChanged -= _trayIconChangedHandler; // 解绑托盘图标事件
            taskbarIcon?.Dispose(); // 释放托盘图标
            taskbarIcon = null; // 清空托盘图标
        }

        // 释放钩子绑定的事件
        private void DisposeHook()
        {
            if (hook != null)
            {
                hook.MouseReleased -= Hook_MouseReleased; // 移除鼠标松开事件处理器
                hook.MousePressed -= Hook_MousePressed; // 移除鼠标按下事件处理器
                hook.KeyReleased -= Hook_KeyReleased; // 移除按键松开事件处理器
                hook.KeyPressed -= Hook_KeyPressed; // 移除按键按下事件处理器
                hook.Dispose(); // 释放钩子
                hook = null; // 清空钩子
            }
        }
    }
}