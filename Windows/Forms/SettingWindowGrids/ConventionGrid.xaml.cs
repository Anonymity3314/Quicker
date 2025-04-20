using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;

namespace Quicker.Windows.Forms.SettingWindowGrids
{
    public partial class ConventionGrid : UserControl
    {
        private readonly SettingDatabase db1; // 设置数据库
        private double currentSessionTime; // 当次应用使用时长
        private double totalUsageTime; // 总使用时长
        private DispatcherTimer timer; // 定时器
        SettingWindow settingWindow; // 父窗体

        public ConventionGrid(SettingWindow window)
        {
            InitializeComponent();
            settingWindow = window; // 保存父窗体
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

        private void CheckUpdateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settingWindow.OpenWebsite("https://github.com/Anonymity3314/Quicker"); // 打开更新页面
        }

        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settingWindow.CheckBox_Click(sender, e); // 调用父类方法
        }

        // 文本框内容改变事件
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settingWindow.TextBox_TextChanged(sender, e); // 调用父类方法
        }
    }
}