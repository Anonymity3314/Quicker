using System.Windows.Controls;
using Quicker.UserControls;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using Quicker;

namespace Quicker.UserControls
{
    public partial class BlacklistGrid : UserControl
    {
        SettingManager settingManager; // 设置管理器

        public BlacklistGrid()
        {
            InitializeComponent();

            SettingWindow settingWindow = Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
            settingManager = settingWindow.settingManager; // 创建设置管理器
            
            InitializeAsync(); // 异步初始化
        }

        // 异步初始化方法
        private async void InitializeAsync()
        {
            await LoadSettingsAsync(); // 异步加载设置
        }

        // 异步加载设置
        private async Task LoadSettingsAsync()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

            });
        }
    }
}