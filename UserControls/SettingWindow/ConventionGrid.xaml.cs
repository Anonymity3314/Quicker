using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using Quicker;
using Quicker.Windows.Forms;

namespace Quicker.UserControls
{
    public partial class ConventionGrid : UserControl
    {
        private WeakReference<SettingWindow> weakSettingWindow; // 弱引用设置窗口
        private double currentSessionTime; // 当次应用使用时长
        SettingManager settingManager; // 设置管理器
        private double totalUsageTime; // 总使用时长
        private DispatcherTimer timer; // 定时器


        public ConventionGrid(SettingWindow settingWindow)
        {
            InitializeComponent();
            weakSettingWindow = new(settingWindow); // 保存设置窗口
            settingManager = settingWindow._settingManager; // 获取设置管理器
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
            settingManager.LoadConventionsAsync(); // 初始化缓存数据
            Application.Current.Dispatcher.Invoke(() =>
            {
                VersionLabel.Content = $"当前版本：{settingManager.conventions.Version}"; // 加载版本信息
                AutoStartCheckBox.IsChecked = settingManager.conventions.AutoStart; // 加载开机自启动设置
                ShowNotificationCheckBox.IsChecked = settingManager.conventions.ShowNotification; // 加载显示启动完成提示设置
                ShowAddImageCheckBox.IsChecked = settingManager.conventions.ShowAddImage; // 加载左键点击空白按钮时显示创建动作菜单设置
                HideTooltipCheckBox.IsChecked = settingManager.conventions.HideTooltip; // 加载隐藏提示框设置
                LongPressThresholdTextBox.Text = settingManager.conventions.LongPressThreshold.ToString(); // 加载长按阈值设置
                MouseMovePixelsTextBox.Text = settingManager.conventions.MouseMovePixels.ToString(); // 加载鼠标移动像素设置
                LoopPageFlippingCheckBox.IsChecked = settingManager.conventions.LoopPageFlipping; // 加载循环翻页设置
                RememberLastPageCheckBox.IsChecked = settingManager.conventions.RememberLastPage; // 加载记住设置窗口中最后打开的页面
                EnableMemoryOptimizationCheckBox.IsChecked = settingManager.conventions.EnableMemoryOptimization; // 加载启用内存优化设置
            });
        }

        // 异步加载使用时长
        private async Task LoadUsageTimeAsync()
        {
            DateTime currentTime = DateTime.Now; // 获取当前时间
            var Conventions = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置信息
            totalUsageTime = Conventions.TotalUsageTime + (currentTime - AppStateManager.RecordedTime).TotalSeconds; // 加载总使用时长
            currentSessionTime = (currentTime - AppStateManager.StartTime).TotalSeconds; // 更新当次应用使用时长
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
        private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new(); // 创建更新窗口
            updateWindow.Show(); // 显示更新窗口
        }

        // 勾选框点击事件
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender); // 调用父类方法
        }

        // 文本框内容改变事件
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settingManager.TextBox_TextChanged(sender); // 调用父类方法
        }

        // 窗体关闭释放资源
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