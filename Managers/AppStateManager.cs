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
        public  DatabaseUpdateManager DatabaseUpdateManager { get; set; } = new(); // 更新管理器
        public WindowManager WindowManager { get; set; } = new(); // 窗口管理器
        public ToastManager ToastManager { get; set; } = new(); // 通知管理器

        public Convention Conventions { get; set; } = new(); // 缓存基础设置
        public OpenMainWindow OpenMainWindowConditions { get; set; } = new(); // 缓存 OpenMainWindowConditions

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
        public DispatcherTimer PressTimer { get; set; } = new(); // 鼠标按下定时器
        public DispatcherTimer Timer { get; set; } = new(); // 计时器

        // 其他状态
        public string CommonState { get; set; } = string.Empty; // 通用状态
        public float Left { get; set; } = 0; // 窗口与屏幕左边距离
        public float Top { get; set; } = 0; // 窗口与屏幕上边距离


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

        // 释放资源
        public void Dispose()
        {
            // 释放托管资源
            if (PressTimer != null)
            {
                PressTimer.Stop(); // 停止鼠标按下定时器
                PressTimer = null; // 清空鼠标按下定时器
            }
            if (Timer != null)
            {
                Timer.Stop(); // 停止计时器
                Timer = null; // 清空计时器
            }
            if (PreLoadMainWindow != null)
            {
                PreLoadMainWindow.Close(); // 关闭预加载窗口
                PreLoadMainWindow = null; // 清空预加载窗口
            }
            if (WindowManager != null)
            {
                WindowManager.Dispose(); // 释放窗口管理器资源
                WindowManager = null; // 清空窗口管理器
            }
            if (ToastManager != null)
            {
                ToastManager.Dispose(); // 释放通知管理器资源
                ToastManager = null; // 清空通知管理器
            }
            if (DatabaseUpdateManager != null)
            {
                DatabaseUpdateManager.Dispose(); // 释放更新管理器资源
                DatabaseUpdateManager = null; // 清空更新管理器
            }
            if (Db != null)
            {
                Db = null; // 清空数据库操作对象
            }
        }
    }
}