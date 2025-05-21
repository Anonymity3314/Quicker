using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;

namespace Quicker
{
    public static class AppStateManager
    {
        public static BitmapImage _trayIcon1 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker1.ico", UriKind.Relative)); // 运行时的图标
        public static BitmapImage _trayIcon2 = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker2.ico", UriKind.Relative)); // 暂停时的图标

        public static readonly string DisBookIconPath = "/Resources/Images/Icons/Disbook.ico"; // 禁用订住图标路径
        public static readonly string UnLockIconPath = "/Resources/Images/Icons/UnLocked.ico"; // 解锁图标路径
        public static readonly string LockIconPath = "/Resources/Images/Icons/Locked.ico"; // 锁定图标路径
        public static readonly string BookIconPath = "/Resources/Images/Icons/Book.ico"; // 订住图标路径

        public static List<BlacklistApplication> BlacklistApplications { get; set; } = new(); // 缓存黑名单应用
        public static OpenMainWindow OpenMainWindowConditions { get; set; } = new(); // 缓存 OpenMainWindowConditions
        public static Blacklist BlacklistSettings { get; set; } = new(); // 缓存黑名单设置
        public static Convention Conventions { get; set; } = new(); // 缓存基础设置

        // 窗口状态
        public static bool Locked { get; set; } = false; // 是否锁定通用动作页
        public static bool Pause { get; set; } = false; // 是否暂停 Quicker
        public static bool Book { get; set; } = false; // 是否订住主面板

        // 时间记录
        public static DateTime RecordedTime { get; set; } = DateTime.Now; // 记录时间
        public static DateTime StartTime { get; set; } = DateTime.Now; // 开始时间

        // 窗口操作对象
        public static MainWindow? PreLoadMainWindow { get; set; } = null; // 预加载窗口

        // 鼠标操作变量
        public static System.Windows.Point StartPosition { get; set; } = new(); // 鼠标按下位置
        public static DateTime? KeyPressStartTime { get; set; } = null; // 鼠标按下时间

        // 定时器
        public static DispatcherTimer PressTimer { get; set; } = new(); // 鼠标按下定时器
        public static DispatcherTimer Timer { get; set; } = new(); // 计时器

        // 其他状态
        public static bool EnableMemoryOptimization { get; set; } = false; // 是否启用内存优化
        public static string CommonState { get; set; } = "Common"; // 通用状态
        public static float Left { get; set; } = 0; // 窗口与屏幕左边距离
        public static float Top { get; set; } = 0; // 窗口与屏幕上边距离

        static AppStateManager()
        {
            LoadSettings(); // 加载设置
        }

        // 从数据库加载设置并插入临时数据库
        public static void LoadSettings()
        {
            Conventions = SettingDatabase.GetAllConventions().FirstOrDefault(); // 加载基础设置
            OpenMainWindowConditions = SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault(); // 加载 OpenMainWindowConditions
            BlacklistSettings = SettingDatabase.GetAllBlacklistSettings().FirstOrDefault(); // 加载黑名单设置
            BlacklistApplications = SettingDatabase.GetAllBlacklistApplications(); // 加载黑名单应用
            EnableMemoryOptimization = Conventions.EnableMemoryOptimization; // 是否启用内存优化
            if (EnableMemoryOptimization) // 启用内存优化
                ClearCachedData(); // 清除缓存数据
        }

        // 清除缓存数据
        private static void ClearCachedData()
        {
            Conventions = null; // 清除基础设置缓存
            OpenMainWindowConditions = null; // 清除 OpenMainWindowConditions 缓存
            BlacklistSettings = null; // 清除黑名单设置缓存
            BlacklistApplications = null; // 清除黑名单应用缓存
        }

        // 释放托管资源
        public static void Dispose()
        {
            ClearCachedData(); // 清除缓存数据
            PressTimer.Stop(); // 停止鼠标按下定时器
            PressTimer = null; // 清除鼠标按下定时器
            Timer.Stop(); // 停止计时器
            Timer = null; // 清除计时器
            PreLoadMainWindow?.Close(); // 关闭预加载窗口
            PreLoadMainWindow = null; // 清除预加载窗口资源
        }
    }
}