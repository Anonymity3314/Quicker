using System.Windows.Threading;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System;

namespace Quicker
{
    public class AppStateManager
    {
        public SettingDatabase Db { get; set; } = new(); // 数据库操作对象
        public DatabaseUpdateManager DatabaseUpdateManager { get; set; } = new(); // 更新管理器
        public TemporaryDatabase Temporary { get; set; } = new(); // 临时数据库
        public WindowManager WindowManager { get; set; } = new(); // 窗口管理器

        public List<BlacklistApplication> BlacklistApplications { get; set; } = new(); // 缓存黑名单应用
        public OpenMainWindow OpenMainWindowConditions { get; set; } = new(); // 缓存 OpenMainWindowConditions
        public Blacklist BlacklistSettings { get; set; } = new(); // 缓存黑名单设置
        public Convention Conventions { get; set; } = new(); // 缓存基础设置
        
        /// <summary>
        /// 从临时数据库获取 BlacklistApplications
        /// </summary>
        /// <returns> BlacklistApplications 对象 </returns>
        public List<BlacklistApplication> GetBlacklistApplications()
        {
            return Temporary.GetBlacklistApplications(); // 从临时数据库获取 BlacklistApplications
        }

        /// <summary>
        /// 从临时数据库获取 OpenMainWindowConditions
        /// </summary>
        /// <returns> OpenMainWindowConditions 对象 </returns>
        public OpenMainWindow GetOpenMainWindowConditions()
        {
            return Temporary.GetOpenMainWindowConditions(); // 从临时数据库获取 OpenMainWindowConditions
        }

        /// <summary>
        /// 从临时数据库获取 BlacklistSettings
        /// </summary>
        /// <returns> BlacklistSettings 对象 </returns>
        public Blacklist GetBlacklistSettings()
        {
            return Temporary.GetBlacklistSettings(); // 从临时数据库获取 BlacklistSettings
        }

        /// <summary>
        /// 从临时数据库获取 Convention
        /// </summary>
        /// <returns> Convention 对象 </returns>
        public Convention GetConvention()
        {
            return Temporary.GetConvention(); // 从临时数据库获取 Convention
        }

        // 窗口状态
        public bool Locked { get; set; } = false; // 是否锁定通用动作页
        public bool Pause { get; set; } = false; // 是否暂停Quicker
        public bool Book { get; set; } = false; // 是否订住主面板

        // 时间记录
        public DateTime RecordedTime { get; set; } = DateTime.Now; // 记录时间
        public DateTime StartTime { get; set; } = DateTime.Now; // 开始时间

        // 窗口操作对象
        public MainWindow? PreLoadMainWindow { get; set; } = null; // 预加载窗口

        // 鼠标操作变量
        public System.Windows.Point StartPosition { get; set; } = new(); // 鼠标按下位置
        public DateTime? KeyPressStartTime { get; set; } = null; // 鼠标按下时间

        // 定时器
        public DispatcherTimer PressTimer { get; set; } = new(); // 鼠标按下定时器
        public DispatcherTimer Timer { get; set; } = new(); // 计时器

        // 其他状态
        public string CommonState { get; set; } = string.Empty; // 通用状态
        public float Left { get; set; } = 0; // 窗口与屏幕左边距离
        public float Top { get; set; } = 0; // 窗口与屏幕上边距离
        public bool EnableMemoryOptimization { get; set; } = false; // 是否启用内存优化

        public AppStateManager()
        {
            LoadSettings(); // 加载设置
        }

        // 从数据库加载设置并插入临时数据库
        public void LoadSettings()
        {
            var conventions = Db.GetAllConventions(); // 获取所有 Convention
            Conventions = conventions[0]; // 只有一条记录
            var conditionsList = Db.GetAllOpenMainWindowConditions(); // 获取所有 OpenMainWindowConditions
            OpenMainWindowConditions = conditionsList[0]; // 只有一条记录
            var blacklistList = Db.GetAllBlacklistSettings(); // 获取所有 BlacklistSettings
            BlacklistSettings = blacklistList[0]; // 只有一条记录
            BlacklistApplications = Db.GetAllBlacklistApplications(); // 获取所有 BlacklistApplications

            var setting = conventions.FirstOrDefault(); // 获取设置
            EnableMemoryOptimization = setting.EnableMemoryOptimization; // 设置是否启用内存优化
            if (EnableMemoryOptimization) // 如果启用内存优化
            {
                // 将数据插入临时数据库
                Temporary.InsertConvention(Conventions); // 插入 Convention
                Temporary.InsertOpenMainWindowConditions(OpenMainWindowConditions); // 插入 OpenMainWindowConditions
                Temporary.InsertBlacklistSettings(BlacklistSettings); // 插入 BlacklistSettings
                foreach (var app in BlacklistApplications) // 插入 BlacklistApplications
                {
                    Temporary.InsertBlacklistApplication(app); // 插入 BlacklistApplications
                }

                // 清空 AppState 中的缓存数据
                Conventions = null; // 清空 Convertions
                OpenMainWindowConditions = null; // 清空 OpenMainWindowConditions
                BlacklistSettings = null; // 清空 BlacklistSettings
                BlacklistApplications = null; // 清空 BlacklistApplications
            }
        }

        // 释放托管资源
        public void Dispose()
        {
            PressTimer.Stop(); // 停止鼠标按下定时器
            PressTimer = null; // 清空鼠标按下定时器
            Timer.Stop(); // 停止计时器
            Timer = null; // 清空计时器
            PreLoadMainWindow?.Close(); // 关闭预加载窗口
            PreLoadMainWindow = null; // 清空预加载窗口
            WindowManager?.Dispose(); // 释放窗口管理器资源
            WindowManager = null; // 清空窗口管理器
            DatabaseUpdateManager?.Dispose(); // 释放更新管理器资源
            DatabaseUpdateManager = null; // 清空更新管理器
            Db = null; // 清空数据库操作对象
            Temporary = null; // 清空临时数据库
        }
    }
}