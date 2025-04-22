using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Forms.SettingWindowGrids
{
    public partial class ConventionGrid : UserControl
    {
        private readonly SettingDatabase db1; // 设置数据库
        private double currentSessionTime; // 当次应用使用时长
        SettingManager settingManager; // 设置管理器
        private double totalUsageTime; // 总使用时长
        private DispatcherTimer timer; // 定时器

        public ConventionGrid()
        {
            InitializeComponent();
            db1 = new SettingDatabase(); // 创建设置数据库
            settingManager = new SettingManager(); // 创建设置管理器

            InitializeAsync(); // 异步初始化
        }

        // 异步初始化方法
        private async void InitializeAsync()
        {
            await LoadSettingsAsync(); // 异步加载设置
            await LoadUsageTimeAsync(); // 异步加载使用时长
        }

        // 异步加载设置
        private async Task LoadSettingsAsync()
        {
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息
            Application.Current.Dispatcher.Invoke(() =>
            {
                AutoStartCheckBox.IsChecked = Conventions.AutoStart; // 加载开机自启动设置
                ShowNotificationCheckBox.IsChecked = Conventions.ShowNotification; // 加载显示启动完成提示设置
                ShowAddImageCheckBox.IsChecked = Conventions.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
                HideTooltipCheckBox.IsChecked = Conventions.HideTooltip; // 加载隐藏提示框设置
                LongPressThresholdTextBox.Text = Conventions.LongPressThreshold.ToString(); // 加载长按阈值设置
                MouseMovePixelsTextBox.Text = Conventions.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
                LoopPageFlippingCheckBox.IsChecked = Conventions.LoopPageFlipping; // 加载循环翻页设置
            });
        }

        // 异步加载使用时长
        private async Task LoadUsageTimeAsync()
        {
            DateTime currentTime = DateTime.Now;
            var Conventions = db1.GetAllConventions().FirstOrDefault(); // 获取设置信息            
            totalUsageTime = Conventions.TotalUsageTime + (currentTime - App.RecordedTime).TotalSeconds; // 加载总使用时长
            currentSessionTime = (currentTime - App.StartTime).TotalSeconds; // 更新当次应用使用时长
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) }; // 创建定时器
            timer.Tick += Timer_Tick; // 定时器每秒更新使用时长
            timer.Start(); // 启动定时器
            Application.Current.Dispatcher.Invoke(() => // 更新界面显示
            {
                // 当次应用使用时长
                var currentSessionTimeSpan = TimeSpan.FromSeconds(currentSessionTime);
                double currentSessionHours = currentSessionTimeSpan.TotalHours;
                CurrentUsingTimeTextBlock.Text = $"{currentSessionHours:0}:{currentSessionTimeSpan:mm}:{currentSessionTimeSpan:ss}";

                // 总使用时长
                var totalTimeSpan = TimeSpan.FromSeconds(totalUsageTime);
                double totalHours = totalTimeSpan.TotalHours;
                TotalUsageTimeTextBlock.Text = $"{totalHours:0}:{totalTimeSpan:mm}:{totalTimeSpan:ss}";
            });
        }

        // 定时器每秒更新使用时长
        private void Timer_Tick(object sender, EventArgs e)
        {
            currentSessionTime += 1; // 更新当次应用使用时长
            totalUsageTime += 1; // 更新总使用时长
            Application.Current.Dispatcher.Invoke(() => // 更新界面显示
            {
                // 当次应用使用时长
                var currentSessionTimeSpan = TimeSpan.FromSeconds(currentSessionTime);
                double currentSessionHours = currentSessionTimeSpan.TotalHours;
                CurrentUsingTimeTextBlock.Text = $"{currentSessionHours:0}:{currentSessionTimeSpan:mm}:{currentSessionTimeSpan:ss}";

                // 总使用时长
                var totalTimeSpan = TimeSpan.FromSeconds(totalUsageTime);
                double totalHours = totalTimeSpan.TotalHours;
                TotalUsageTimeTextBlock.Text = $"{totalHours:0}:{totalTimeSpan:mm}:{totalTimeSpan:ss}";
            });
        }

        // 打开网站检查更新
        private void CheckUpdateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settingManager.OpenWebsite("https://github.com/Anonymity3314/Quicker"); // 打开更新页面
        }

        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender); // 调用父类方法
        }

        // 文本框内容改变事件
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settingManager.TextBox_TextChanged(sender); // 调用父类方法
        }

        // 窗体关闭事件
        private void ConventionGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop(); // 停止定时器
            totalUsageTime = 0; // 清空总使用时长
            currentSessionTime = 0; // 清空当次应用使用时长
            AutoStartCheckBox = null;
            ShowNotificationCheckBox = null;
            ShowAddImageCheckBox = null;
            HideTooltipCheckBox = null;
            LongPressThresholdTextBox = null;
            MouseMovePixelsTextBox = null;
            LoopPageFlippingCheckBox = null;
            CurrentUsingTimeTextBlock = null;
            TotalUsageTimeTextBlock = null;
            CheckUpdateButton = null;
            GC.Collect(); // 垃圾回收
        }
    }
}