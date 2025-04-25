using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Forms;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using Quicker;
using System.IO;

namespace Quicker.UserControls
{
    public partial class BlacklistGrid : System.Windows.Controls.UserControl
    {
        SettingManager settingManager; // 设置管理器

        public BlacklistGrid()
        {
            InitializeComponent();

            SettingWindow settingWindow = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

            });
        }

        private void BlacklistDragButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BlacklistAddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnknownProcessButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 将选中文件夹里的 .exe 文件添加到黑名单
        private void AddDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog()) // 创建文件夹选择对话框
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath; // 获取选择的文件夹路径
                    foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories)) // 遍历文件夹中的所有文件
                    {
                        if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)) // 如果扩展名为.exe
                        {
                            string processName = Path.GetFileNameWithoutExtension(file);
                            BlacklistListView.Items.Add(new TextBlock { Text = processName });
                        }
                    }
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender);
        }

        private void BlacklistSelectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 鼠标移入Grid显示滚动条
        private void ShowBlacklistScrollBar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BlacklistScrollBar.Visibility = Visibility.Visible; // 显示滚动条
        }

        // 鼠标移出Grid隐藏滚动条
        private void HideBlacklistScrollBar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BlacklistScrollBar.Visibility = Visibility.Hidden; // 隐藏滚动条
        }

        // 同步滚动条
        private void BlacklistScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BlacklistStackScrollViewer.ScrollToHorizontalOffset(BlacklistScrollBar.Value); // 滚动到指定位置
        }
        private void BlacklistStackScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            BlacklistScrollBar.Maximum = BlacklistStackScrollViewer.ExtentWidth - BlacklistStackScrollViewer.ViewportWidth; // 设置滚动条最大值
            BlacklistScrollBar.ViewportSize = BlacklistStackScrollViewer.ViewportWidth; // 设置滚动条视口大小
            BlacklistScrollBar.Value = BlacklistStackScrollViewer.HorizontalOffset; // 设置滚动条值
        }
    }
}