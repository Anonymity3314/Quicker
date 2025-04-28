using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Diagnostics;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System.IO;
using Quicker;

namespace Quicker.UserControls
{
    public partial class BlacklistGrid : System.Windows.Controls.UserControl
    {
        private WindowManager windowManager = new WindowManager(); // 窗口管理器
        private IconManager iconManager = new IconManager(); // 图标管理器
        SettingDatabase db1 = new SettingDatabase(); // 设置数据库
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
            settingManager.LoadBlacklistSettingsAsync(); // 加载设置
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                FullScreenDisableCheckBox.IsChecked = settingManager.blacklistSettings.FullScreenDisable; // 设置全屏禁用复选框
                //ApplyBlacklistToExpandHotkeysCheckBox.IsChecked = settingManager.blacklistSettings.ApplyBlacklistToExpandHotkeys; // 设置应用黑名单到快捷键扩展复选框
            }); // 刷新UI

            var blacklistApps = db1.GetAllBlacklistApplications(); // 获取黑名单应用
            Dictionary<string, BlacklistApplication> blacklistDict = new Dictionary<string, BlacklistApplication>(); // 创建字典
            foreach (var app in blacklistApps) // 遍历黑名单应用
            {
                blacklistDict[app.ApplicationName] = app; // 将应用添加到字典中
            }

            // 从字典中添加到黑名单列表
            foreach (var KeyValuePair in blacklistDict)
            {
                AddBlacklistItem(KeyValuePair.Value.ApplicationName, KeyValuePair.Value.IsFolder); // 添加到黑名单列表
            }
        }

        // 拖动按钮选择屏幕上要禁用的应用
        private void BlacklistDragButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 将选中的文件添加到黑名单
        private void BlacklistAddButton_Click(object sender, RoutedEventArgs e)
        {
            ExistAppsStackPanel.Children.Clear(); // 清空列表
            ExistAppsPop.Height = 4; // 重置高度
            Process[] processes = Process.GetProcesses(); // 获取所有进程
            var uniqueProcesses = new Dictionary<string, Process>(); // 创建字典，用于存放唯一的进程
            foreach (Process process in processes) // 遍历所有进程
            {
                try
                {
                    string processFileName = process.MainModule.FileName; // 获取进程文件名
                    string fullProcessName = Path.GetFileName(processFileName); // 获取进程名

                    if (!HasWindow(process)) continue; // 判断是否有窗口
                    if (uniqueProcesses.ContainsKey(fullProcessName)) continue; // 判断是否已经存在相同的进程名
                    uniqueProcesses[fullProcessName] = process; // 添加到字典中
                    AddAppItems(processFileName); // 添加到列表中
                }
                catch { } // 忽略异常
            }
            AddAppItems("从计算机选择程序..."); // 添加到列表中
            uniqueProcesses.Clear(); // 清空字典
            ExistAppsPop.IsOpen = true; // 打开正在运行的程序提示框
        }

        /// <summary>
        /// 判断进程是否有窗口（包括最小化的窗口）
        /// </summary>
        /// <param name="process">进程对象</param>
        /// <returns>是否有窗口</returns>
        private bool HasWindow(Process process)
        {
            return process.MainWindowHandle != IntPtr.Zero; // 判断是否有窗口句柄
        }

        /// <summary>
        /// 添加到正在运行的程序提示框中
        /// </summary>
        /// <param name="appNames"> 进程名 </param>
        private void AddAppItems(string appPath)
        {
            string appNames = Path.GetFileName(appPath); // 获取进程名
            ExistAppsPop.Height += 26; // 设置高度
            System.Windows.Controls.Button button = new()
            {
                Tag = appNames,
                Height = 25, // 设置高度
                Width = 250, // 设置宽度
                Style = (Style)System.Windows.Application.Current.Resources["MenuButton"] // 设置按钮样式
            }; // 创建按钮
            button.Click += AddToBlacklistButton_Click; // 绑定按钮点击事件
            ExistAppsStackPanel.Children.Add(button); // 添加到父容器StackPanel

            StackPanel stackPanel = new()
            {
                Width = 250, // 设置宽度
                Orientation = System.Windows.Controls.Orientation.Horizontal, // 设置为横向排列
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left // 设置水平对齐方式
            }; // 创建StackPanel
            button.Content = stackPanel; // 设置按钮内容

            System.Windows.Controls.Image iconImage = new()
            {
                Width = 16, // 设置宽度
                Height = 16, // 设置高度
                Margin = new Thickness(5, 0, 5, 0), // 设置外边距
                Source = iconManager.GetIcon(appPath),
                VerticalAlignment = VerticalAlignment.Center, // 垂直居中
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center // 水平居中
            }; // 创建图标
            stackPanel.Children.Add(iconImage); // 添加到StackPanel

            TextBlock textBlock = new() { Text = appNames }; // 创建TextBlock
            stackPanel.Children.Add(textBlock); // 添加进程名称
        }

        // 将选中的进程添加到黑名单
        private void AddToBlacklistButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button; // 转换发送者为按钮对象
            if(button.Tag.ToString() == "从计算机选择程序...") // 如果是选择文件
                SelectLocalFile(); // 选择本地文件
            else
            {
                db1.ApplyBlacklistApplication(button.Tag.ToString(), button.Tag.ToString(), true, false); // 添加到设置中
                AddBlacklistItem(button.Tag.ToString(), false); // 添加到黑名单
            }
            ExistAppsPop.IsOpen = false; // 关闭提示框
        }

        // 选择本地文件
        private void SelectLocalFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "可执行程序(*.exe)|*.exe|任意文件(*.*)|*.*" // 设置文件类型过滤器
            };

            if (openFileDialog.ShowDialog() == true) // 检查用户是否点击了“确定”
            {
                string filePath = openFileDialog.FileName; // 获取选择的文件路径
                string processName = Path.GetFileNameWithoutExtension(filePath); // 获取进程名
                db1.ApplyBlacklistApplication(processName, processName, true, false); // 添加到设置中
                AddBlacklistItem(processName, false); // 添加到黑名单
            }
        }

        // 添加未知应用
        private void UnknownProcessButton_Click(object sender, RoutedEventArgs e)
        {
            AddDirectoryButton.Margin = new Thickness(295, 230, 0, 0); // 调整按钮位置
            UnknownProcessButton.Visibility = Visibility.Collapsed; // 隐藏按钮
            db1.ApplyBlacklistApplication("unknown-proc.exe", "unknown-proc.exe", true, false); // 添加到设置中
            AddBlacklistItem("unknown-proc.exe", false); // 添加到黑名单
        }

        // 将选中文件夹里的 .exe 文件添加到黑名单
        private void AddDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog()) // 创建文件夹选择对话框
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SettingWindow settingWindow = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault(); // 尝试查找现有的设置窗口
                    LoadingWindow loadingWindow = new() { Owner = settingWindow }; // 创建加载窗口
                    loadingWindow.Show(); // 显示加载窗口
                    string selectedPath = dialog.SelectedPath; // 获取选择的文件夹路径
                    AddBlacklistItem(selectedPath, true); // 添加完整路径到黑名单
                    foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories)) // 遍历文件夹中的所有文件
                    {
                        if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)) // 如果扩展名为.exe
                        {
                            string processName = file.Substring(selectedPath.Length + 1); // 获取进程名
                            db1.ApplyBlacklistApplication(selectedPath, processName, true, true); // 添加到设置中
                        }
                    }
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

            if (isFolder) // 如果是文件夹
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
                Tag = process,
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
            var process = button.Tag.ToString(); // 获取进程名
            db1.DeleteBlacklistApplication(process); // 从设置中删除进程
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
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            settingManager.CheckBox_Click(sender);
        }

        // 选择在禁用Quicker时仍然能够打开Quicker的应用
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