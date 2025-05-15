using System.Windows.Threading;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System;

namespace Quicker
{
    public class AppStateManager : IDisposable
    {
        public SettingDatabase Db { get; set; } = new SettingDatabase(); // 数据库操作对象
        public WindowManager WindowManager { get; set; } = new WindowManager(); // 窗口管理器

        public Convention Conventions { get; set; } = new Convention(); // 缓存基础设置
        public OpenMainWindow OpenMainWindowConditions { get; set; } = new OpenMainWindow(); // 缓存 OpenMainWindowConditions

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
        public System.Windows.Point StartPosition { get; set; } = new System.Windows.Point(); // 鼠标按下位置
        public DateTime? KeyPressStartTime { get; set; } = null; // 鼠标按下时间

        // 定时器
        public DispatcherTimer PressTimer { get; set; } = new DispatcherTimer(); // 鼠标按下定时器
        public DispatcherTimer Timer { get; set; } = new DispatcherTimer(); // 计时器

        // 其他状态
        public string CommonState { get; set; } = string.Empty; // 通用状态
        public float Left { get; set; } = 0; // 窗口与屏幕左边距离
        public float Top { get; set; } = 0; // 窗口与屏幕上边距离

        private bool disposed = false; // 标记是否已释放资源

        public AppStateManager()
        {
            LoadSettings(); // 加载基础设置
        }

        // 从数据库加载基础设置
        public void LoadSettings()
        {
            var conventions = Db.GetAllConventions(); // 获取所有 Convention
            Conventions = conventions[0]; // 只有一条记录
            var conditionsList = Db.GetAllOpenMainWindowConditions(); // 获取所有 OpenMainWindowConditions
            OpenMainWindowConditions = conditionsList[0]; // 只有一条记录
        }

        // 实现IDisposable接口，用于自动释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 保护方法，用于释放资源
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    if (PressTimer != null)
                    {
                        PressTimer.Stop();
                        PressTimer = null;
                    }
                    if (Timer != null)
                    {
                        Timer.Stop();
                        Timer = null;
                    }
                    if (PreLoadMainWindow != null)
                    {
                        PreLoadMainWindow.Close();
                        PreLoadMainWindow = null;
                    }
                    if (WindowManager != null)
                    {
                        WindowManager.Dispose();
                        WindowManager = null;
                    }
                    if (Db != null)
                    {
                        Db = null;
                    }
                }
                disposed = true;
            }
        }

        // 析构函数，用于释放非托管资源
        ~AppStateManager()
        {
            Dispose(false);
        }
    }
}