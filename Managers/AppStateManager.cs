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
        public SettingDatabase Db { get; set; } // 数据库操作对象
        public ButtonManager ButtonManager { get; set; } // 按钮管理器
        public WindowManager WindowManager { get; set; } // 窗口管理器

        // 窗口状态
        public bool Locked { get; set; } // 是否锁定通用动作页
        public bool Pause { get; set; } // 是否暂停Quicker
        public bool Book { get; set; } // 是否订住主面板

        // 时间记录
        public DateTime RecordedTime { get; set; } // 记录时间
        public DateTime StartTime { get; set; } // 开始时间

        // 窗口操作对象
        public MainWindow? PreLoadMainWindow { get; set; } // 预加载窗口

        // 鼠标操作变量
        public System.Windows.Point StartPosition { get; set; } // 鼠标按下位置
        public DateTime? KeyPressStartTime { get; set; } // 鼠标按下时间

        // 定时器
        public DispatcherTimer PressTimer { get; set; } // 鼠标按下定时器
        public DispatcherTimer Timer { get; set; } // 计时器

        // 其他状态
        public string CommonState { get; set; } // 通用状态
        public float Left { get; set; } // 窗口与屏幕左边距离
        public float Top { get; set; } // 窗口与屏幕上边距离

        public AppStateManager()
        {
            Db = new SettingDatabase(); // 设置数据库
            ButtonManager = new ButtonManager(); // 按钮管理器
            WindowManager = new WindowManager(); // 窗口管理器
            Book = false; // 是否订住主面板
            Pause = false; // 是否暂停Quicker
            Locked = false; // 是否锁定通用动作页
            RecordedTime = DateTime.Now; // 记录时间
            StartTime = DateTime.Now; // 开始时间
            PreLoadMainWindow = null; // 预加载窗口
            KeyPressStartTime = null; // 鼠标按下时间
            PressTimer = new DispatcherTimer(); // 鼠标按下定时器
            Timer = new DispatcherTimer(); // 计时器
            CommonState = string.Empty; // 通用状态
            Left = 0; // 窗口与屏幕左边距离
            Top = 0; // 窗口与屏幕上边距离
        }
    }
}