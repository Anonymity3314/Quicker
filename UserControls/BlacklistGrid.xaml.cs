using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Forms;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System.IO;
using Quicker;

namespace Quicker.UserControls
{
    public partial class BlacklistGrid : System.Windows.Controls.UserControl
    {
        SettingManager settingManager; // 设置管理器
        ButtonManager buttonManager; // 按钮管理器
        Window fatherWindow; // 父窗口

        public BlacklistGrid()
        {
            InitializeComponent();
            buttonManager = new ButtonManager(); // 创建按钮管理器
            SettingWindow settingWindow = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
            settingManager = settingWindow.settingManager; // 创建设置管理器
            fatherWindow = settingWindow; // 设置父窗口
            
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

        // 将选中的文件添加到黑名单
        private void BlacklistAddButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "可执行程序(*.exe)|*.exe|任意文件(*.*)|*.*" // 设置文件类型过滤器
            };

            if (openFileDialog.ShowDialog() == true) // 检查用户是否点击了“确定”
            {

            }
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
                    LoadingWindow loadingWindow = new()
                    {
                        Owner = fatherWindow, // 设置父窗口
                    }; // 创建加载窗口
                    loadingWindow.Show(); // 显示加载窗口
                    foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories)) // 遍历文件夹中的所有文件
                    {
                        if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)) // 如果扩展名为.exe
                        {
                            string processName = Path.GetFileNameWithoutExtension(file);
                            AddToBlacklist(processName); // 添加到黑名单
                        }
                    }
                    loadingWindow.Close(); // 关闭加载窗口
                }
            }
        }

        // 向黑名单中添加进程
        private void AddToBlacklist(string processName)
        {
            Grid grid = new()
            {
                Height = 25, // 设置高度
                Margin = new Thickness(2, 2, 2, 2), // 设置外边距
                Background = System.Windows.Media.Brushes.White // 设置背景色
            }; // 创建Grid
            TextBlock textBlock = new()
            {
                Text = processName,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(2, 0, 0, 0) // 设置内边距
            }; // 创建TextBlock
            grid.Children.Add(textBlock); // 添加进程名称

            System.Windows.Controls.Button button = new()
            {
                Content = "删除",
                Width = 25,
                Margin = new Thickness(2, 2, 2, 2),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            }; // 创建按钮
            button.Click += DeleteFromBlacklist; // 绑定删除事件
            grid.Children.Add(button); // 添加按钮

            BlacklistStackPanel.Children.Add(grid); // 添加到父容器StackPanel
        }

        // 从黑名单中删除进程
        private void DeleteFromBlacklist(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button; // 转换发送者为按钮对象
            var grid = button.Parent as Grid; // 获取按钮的父容器（Grid）
            BlacklistStackPanel.Children.Remove(grid); // 将Grid从父容器StackPanel中移除
        }

        // 勾选框点击事件
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