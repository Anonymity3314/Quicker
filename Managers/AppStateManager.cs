using Quicker.Windows.MainWindows.MainWindow;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Quicker.Models.Settings;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;

namespace Quicker
{
    public static class AppStateManager
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new(); // 缓存
        private static readonly object _cacheLock = new(); // 缓存锁
        
        public static BitmapImage _trayIcon1 = new(new Uri("pack://application:,,,/Resources/Images/Quicker1.png")); // 托盘图标1
        public static BitmapImage _trayIcon2 = new(new Uri("pack://application:,,,/Resources/Images/Quicker2.png")); // 托盘图标2

        public const string DisBookIconPath = "/Resources/Images/UnpinFromDesktop.png"; // 禁订住图标路径
        public const string BookIconPath = "/Resources/Images/PinToDesktop.png"; // 订住图标路径
        public const string UnLockIconPath = "/Resources/Images/UnLocked.png"; // 解锁图标路径
        public const string LockIconPath = "/Resources/Images/Locked.png"; // 锁定图标路径

        private static List<BlacklistApplication> _blacklistApplications; // 黑名单应用
        private static OpenMainWindow _openMainWindowConditions; // 打开主窗口条件
        private static Blacklist _blacklistSettings; // 黑名单设置
        private static Convention _conventions; // 基础设置

        public static List<BlacklistApplication> BlacklistApplications 
        { 
            get => _blacklistApplications ??= SettingDatabase.GetAllBlacklistApplications(); // 获取黑名单应用
            set => _blacklistApplications = value; // 设置黑名单应用
        }

        public static OpenMainWindow OpenMainWindowConditions
        {
            get => _openMainWindowConditions ??= SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取打开主窗口条件
            set => _openMainWindowConditions = value; // 设置打开主窗口条件
        }

        public static Blacklist BlacklistSettings
        {
            get => _blacklistSettings ??= SettingDatabase.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置
            set => _blacklistSettings = value; // 设置黑名单设置
        }

        public static Convention Conventions
        {
            get => _conventions ??= SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取基础设置
            set => _conventions = value; // 设置基础设置
        }

        // 窗口状态
        public static bool Locked { get; set; } = false; // 锁定状态
        public static bool Pinned { get; set; } = false; // 订住状态
        public static bool Pause { get; set; } = false; // 暂停状态

        // 时间记录
        public static DateTime RecordedTime { get; set; } = DateTime.Now; // 记录时间
        public static DateTime StartTime { get; set; } = DateTime.Now; // 开始时间

        // 窗口操作对象
        public static MainWindow? PreLoadMainWindow { get; set; } = null; // 预加载主窗口

        // 鼠标操作变量
        public static System.Windows.Point StartPosition { get; set; } = new(); // 开始位置
        public static DateTime? MousePressStartTime { get; set; } = null; // 鼠标按下时间
        public static DateTime? KeyPressStartTime { get; set; } = null; // 按键按下时间

        // 定时器
        public static DispatcherTimer PressTimer { get; } = new(); // 按键按下定时器
        public static DispatcherTimer Timer { get; } = new(); // 定时器

        // 其他状态
        public static bool EnableMemoryOptimization { get; set; } = false; // 启用内存优化
        public static string CommonState { get; set; } = "Common"; // 普通状态
        public static bool HasNewVersion { get; set; } = false; // 是否有新版本
        public static bool OpenByMouse { get; set; } = false; // 鼠标打开
        public static float Left { get; set; } = 0; // 左
        public static float Top { get; set; } = 0; // 上

        static AppStateManager()
        {
            LoadSettings(); // 加载设置
        }

        public static void LoadSettings()
        {
            Conventions = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取基础设置
            OpenMainWindowConditions = SettingDatabase.GetAllOpenMainWindowConditions().FirstOrDefault(); // 获取打开主窗口条件
            BlacklistSettings = SettingDatabase.GetAllBlacklistSettings().FirstOrDefault(); // 获取黑名单设置
            BlacklistApplications = SettingDatabase.GetAllBlacklistApplications(); // 获取黑名单应用
            EnableMemoryOptimization = Conventions?.EnableMemoryOptimization ?? false; // 启用内存优化
            if (EnableMemoryOptimization) // 如果启用内存优化
            {
                ClearCachedData(); // 清除缓存数据
            }
        }

        // 清除缓存数据
        private static void ClearCachedData()
        {
            _conventions = null; // 基础设置
            _openMainWindowConditions = null; // 打开主窗口条件
            _blacklistSettings = null; // 黑名单设置
            _blacklistApplications = null; // 黑名单应用
            _cache.Clear();
        }

        // 释放资源
        public static void Dispose()
        {
            // 停止所有定时器
            PressTimer?.Stop(); // 停止按键按下定时器
            Timer?.Stop(); // 停止定时器

            // 关闭预加载窗口
            PreLoadMainWindow?.Close(); // 关闭预加载窗口
            PreLoadMainWindow = null; // 清空预加载窗口

            // 清除缓存数据
            ClearCachedData(); // 清除缓存数据
            _cache.Clear(); // 清空缓存
        }

        /// <summary>
        /// 获取或添加到缓存
        /// </summary>
        /// <typeparam name="T"> 类型 </typeparam>
        /// <param name="key"> 键 </param>
        /// <param name="valueFactory"> 值工厂 </param>
        /// <returns> 值 </returns>
        public static T GetOrAddToCache<T>(string key, Func<T> valueFactory)
        {
            return (T)_cache.GetOrAdd(key, _ => valueFactory()); // 获取或添加到缓存
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="key"> 键 </param>
        public static void InvalidateCache(string key)
        {
            _cache.TryRemove(key, out _); // 清除缓存
        }
    }
}