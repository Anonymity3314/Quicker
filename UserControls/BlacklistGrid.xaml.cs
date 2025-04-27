using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Forms;
using System.Windows.Media;
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

        public BlacklistGrid()
        {
            InitializeComponent();
            buttonManager = new ButtonManager(); // 创建按钮管理器
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

        // 将选中的文件添加到黑名单
        private void BlacklistAddButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "可执行程序(*.exe)|*.exe|任意文件(*.*)|*.*" // 设置文件类型过滤器
            };

            if (openFileDialog.ShowDialog() == true) // 检查用户是否点击了“确定”
            {
                string filePath = openFileDialog.FileName; // 获取选择的文件路径
                string processName = Path.GetFileNameWithoutExtension(filePath); // 获取进程名
                AddBlacklistItem(processName, false); // 添加到黑名单
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
                    SettingWindow settingWindow = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                    LoadingWindow loadingWindow = new()
                    {
                        Owner = settingWindow, // 设置父窗口
                    }; // 创建加载窗口
                    loadingWindow.Show(); // 显示加载窗口
                    string selectedPath = dialog.SelectedPath; // 获取选择的文件夹路径
                    AddBlacklistItem(selectedPath, true); // 添加完整路径到黑名单
                    /*
                    foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories)) // 遍历文件夹中的所有文件
                    {
                        if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)) // 如果扩展名为.exe
                        {

                        }
                    }*/
                    loadingWindow.Close(); // 关闭加载窗口
                }
            }
        }

        // 向黑名单中添加进程
        private void AddBlacklistItem(string process, bool isFolder)
        {
            Border border = new()
            {
                Height = 25, // 设置高度
                Tag = false,
                CornerRadius = new CornerRadius(3), // 设置圆角
                Margin = new Thickness(2, 2, 2, 0), // 设置外边距
                Background = System.Windows.Media.Brushes.Transparent // 设置背景色
            }; // 创建Border
            border.MouseEnter += HightLightBlacklistItem; // 绑定鼠标移入事件
            border.MouseLeave += FadeBlacklistItem; // 绑定鼠标移出事件
            border.MouseDown += SelectBlacklistItem; // 绑定鼠标按下事件

            Grid grid = new()
            {
                Height = 25, // 设置高度
                Margin = new Thickness(2, 0, 2, 0), // 设置外边距
                Background = System.Windows.Media.Brushes.Transparent // 设置背景色
            }; // 创建Grid
            border.Child = grid; // 设置Border内容

            StackPanel stackPanel = new()
            {
                Margin = new Thickness(2, 0, 0, 0), // 设置内边距
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = System.Windows.Controls.Orientation.Horizontal, // 设置为横向排列
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            }; // 创建StackPanel
            grid.Children.Add(stackPanel); // 添加StackPanel

            TextBlock textBlock = new()
            {
                Text = process,
                VerticalAlignment = VerticalAlignment.Center
            }; // 创建TextBlock
            stackPanel.Children.Add(textBlock); // 添加进程名称

            if(isFolder)
            {
                System.Windows.Controls.Label label = new()
                {
                    FontSize = 11,
                    Content = "文件夹",
                    Margin = new Thickness(2, 0, 0, 0), // 设置内边距
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.LightGray
                }; // 创建Label
                stackPanel.Children.Add(label); // 添加Label
            }

            System.Windows.Controls.Button button = new()
            {
                ToolTip = "删除此应用",
                Style = (Style)System.Windows.Application.Current.Resources["DeleteBlacklistItem"] // 设置按钮样式
            }; // 创建按钮
            button.Click += DeleteFromBlacklist; // 绑定删除事件
            grid.Children.Add(button); // 添加按钮

            Image image = new()
            {
                Source = new BitmapImage(new Uri("/Resources/Images/Icons/DeleteImage.ico", UriKind.Relative)),
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            }; // 创建图标
            button.Content = image; // 添加图标

            BlacklistStackPanel.Children.Add(border); // 添加到父容器StackPanel
        }

        // 从黑名单中删除进程
        private void DeleteFromBlacklist(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button; // 转换发送者为按钮对象
            var grid = button.Parent as Grid; // 获取按钮的父容器（Grid）
            var border = grid.Parent as Border; // 获取Grid的父容器（Border）
            BlacklistStackPanel.Children.Remove(border); // 将Grid从父容器StackPanel中移除
        }

        // 鼠标移入Border高亮显示黑名单项
        private void HightLightBlacklistItem(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border border = sender as Border; // 转换发送者为Border对象
            if (!(bool)border.Tag) // 如果Border没有被选中
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置背景色
        }

        // 鼠标移出Border恢复原状
        private void FadeBlacklistItem(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border border = sender as Border; // 转换发送者为Border对象
            if (!(bool)border.Tag) // 如果Border没有被选中
                border.Background = System.Windows.Media.Brushes.Transparent; // 设置背景色
        }

        // 鼠标按下Border选中黑名单项
        private void SelectBlacklistItem(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border targetBorder = sender as Border; // 转换发送者为Border对象
            targetBorder.Tag = true; // 标记为已选中
            foreach (var border in BlacklistStackPanel.Children.OfType<Border>())
            { 
                if(border == targetBorder)
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 设置背景色
                else
                    border.Background = System.Windows.Media.Brushes.Transparent; // 设置背景色
            }
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
            BlacklistStackScrollViewer.ScrollToVerticalOffset(BlacklistScrollBar.Value); // 滚动到指定位置
        }
        private void BlacklistStackScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            BlacklistScrollBar.Maximum = BlacklistStackScrollViewer.ExtentHeight - BlacklistStackScrollViewer.ViewportHeight; // 设置滚动条最大值
            BlacklistScrollBar.ViewportSize = BlacklistStackScrollViewer.ViewportHeight; // 设置滚动条视口大小
            BlacklistScrollBar.Value = BlacklistStackScrollViewer.VerticalOffset; // 设置滚动条值
        }
    }
}