using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using Quicker.CommonFunctions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Forms;
using System.Diagnostics;
using SharpHook.Logging;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using System.Media;
using System.IO;

namespace Quicker
{
    public partial class AddWindow : Window
    {
        public static SelectImageWindow SelectImageWindow; // SelectImageWindow 的静态引用
        public static FindAppsWindow FindAppsWindow; // FindAppsWindow 的静态引用
        private readonly ButtonDatabase db2; // ButtonDatabase
        private TextBlock ButtonTitle; // ButtonTitle
        ButtonManager buttonManager; // 按钮管理器接口
        private Image ButtonImage; // ButtonImage
        IconManager iconManager; // 图标管理器接口
        string iconPath; // 图标路径

        public string CurrentButton { get; private set; } // 当前按钮
        public int Choice { get; private set; } // 选择添加动作类型

        public AddWindow(string currentbutton, int choice)
        {
            InitializeComponent(); // 初始化窗口组件

            iconManager = new IconManager(); // 初始化图标管理器
            buttonManager = new ButtonManager(); // 初始化按钮管理器

            CurrentButton = currentbutton; // 当前按钮
            Choice = choice; // 选择添加动作类型

            db2 = new ButtonDatabase(); // 初始化数据库
        }

        // 初始化标题和Button视图，并根据上个窗口数据执行对应命令
        private void AddWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTitle();
            InitializeButtonView();
            ExecuteChoiceAction();
        }

        // 初始化标题
        private void InitializeTitle()
        {
            Match match = Regex.Match(CurrentButton, @"^([a-zA-Z0-9_]+)(\d{3})$"); // 匹配按钮名称和末尾的3个数字
            if (match.Success)
            {
                string buttonName = match.Groups[1].Value; // 获取按钮名称
                string numbersStr = match.Groups[2].Value; // 获取3个数字
                int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray(); // 转换为整数数组
                if (Choice != 0) // 如果不是编辑动作
                {
                    switch(buttonName)
                    {
                        case "Global":
                            Title = $"新动作--默认全局动作第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break; // 默认全局动作
                        case "TaskBar":
                            Title = $"新动作--默认任务栏动作第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break; // 默认任务栏动作
                        case "Desktop":
                            Title = $"新动作--默认桌面动作第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break; // 默认桌面动作
                        case "Common":
                            Title = $"新动作--默认桌面动作第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break;
                        default:
                            Title = $"新动作--{buttonName} 第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break; // 默认动作
                    }
                }
                else // 如果是编辑动作
                {
                    ButtonData buttonData = db2.GetButtonDataByID(CurrentButton);
                    Title = $"新动作--第{numbers[0] +1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                }
            }
        }

        // 初始化Button视图
        private void InitializeButtonView()
        {
            Grid grid = new() // 创建一个新的Grid
            {
                Name = "ButtonView", // 设置名称
                VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直居中
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center // 水平居中
            }; // 设置对齐方式
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义

            ButtonImage = new() // 创建一个新的Image
            {
                Width = 36, // 设置宽度
                Height = 36, // 设置高度
                Name = "ButtonImage", // 设置名称
                Visibility = Visibility.Collapsed, // 初始隐藏
                VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直居中
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center // 水平居中
            }; // 设置对齐方式
            grid.Children.Add(ButtonImage); // 添加到Grid
            Grid.SetRow(ButtonImage, 0); // 设置行索引

            ButtonTitle = new() // 创建一个新的TextBlock
            {
                Name = "ButtonTitle", // 设置名称
                Visibility = Visibility.Collapsed, // 初始隐藏
                TextWrapping = TextWrapping.NoWrap, // 不换行
                VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直居中
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center, // 水平居中
            };
            grid.Children.Add(ButtonTitle); // 添加到Grid
            Grid.SetRow(ButtonTitle, 1); // 设置行索引

            ButtonView.Content = grid; // 设置ButtonView的内容为Grid
        }

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            switch (Choice)
            {
                case 0:
                    LoadButtonInformation();
                    break; // 加载动作信息
                case 1:
                    ChooseApplications(null, null);
                    break; // 选择应用程序
                case 2:
                    ChooseProcess(null, null);
                    break; // 选择文件
                case 3:
                    ChooseFolder(null, null);
                    break; // 选择文件夹
                case 4:
                    ChooseWebsite();
                    break; // 选择网址
            }
        }

        // 关闭添加动作窗口
        private void CloseAddWindow(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 管理本地图标
        private void ManageLocalIcons(object sender, RoutedEventArgs e)
        {
            string localIconsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalIcons"); // 动态生成路径
            if (!Directory.Exists(localIconsPath)) Directory.CreateDirectory(localIconsPath); // 如果文件夹不存在，创建它
            Process.Start(new ProcessStartInfo
            {
                FileName = localIconsPath, // 打开文件夹
                UseShellExecute = true // 使用系统外壳程序打开
            });
        }

        // 删除图标
        private void DeleteImage(object sender, RoutedEventArgs e)
        {
            ButtonImage.Source = null; // 清空图标
            ButtonImage.Visibility = Visibility.Collapsed; // 隐藏图标
        }

        // 打开选择菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true; // 打开弹出菜单
        }

        // 选择本地应用
        private void ChooseApplications(object sender, RoutedEventArgs e)
        {
            FindAppsWindow = new(); // 创建 FindAppsWindow 实例
            FindAppsWindow.ApplicationSelected += OnApplicationSelected; // 订阅 ApplicationSelected 事件
            FindAppsWindow.Owner = this; // 设置所有者为当前窗口
            FindAppsWindow.ShowDialog(); // 显示为模式对话框
        }

        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LocationTextBox.Text)) SaveButton.IsEnabled = true; // 启用保存按钮
            else SaveButton.IsEnabled = false; // 禁用保存按钮
        }

        // 处理选中的应用
        private void OnApplicationSelected(object sender, FindAppsWindow.ApplicationSelectedEventArgs e)
        {
            AppInfo selectedApp = e.SelectedApp; // 获取选中的应用信息
            if (selectedApp != null)
            {
                // 更新控件数据
                TitleTextBox.Text = selectedApp.Name; // 设置标题
                LocationTextBox.Text = selectedApp.Location; // 设置地址

                // 设置图标
                ButtonImage.Source = selectedApp.Icon; // 设置图标
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
                FindAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
            }
        }

        // 选择打开程序
        public void ChooseProcess(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "任意文件(*.*)|*.*|可执行程序(*.exe)|*.exe" // 设置文件类型过滤器
            };

            if (openFileDialog.ShowDialog() == true) // 检查用户是否点击了“确定”
            {
                LocationTextBox.Text = openFileDialog.FileName; // 获取文件路径
                TitleTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // 获取文件名
                ButtonTitle.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // 设置按钮标题
                buttonManager.AutoEllipsisTextBlock(ButtonTitle, 70); // 调整字体大小

                string cachedIconPath = iconManager.CheckCachedIcon(openFileDialog.FileName); // 检查缓存图标
                if (!string.IsNullOrEmpty(cachedIconPath)) // 如果缓存图标存在
                {
                    ButtonImage.Source = new BitmapImage(new Uri(cachedIconPath)); // 设置图标
                    ButtonImage.Visibility = Visibility.Visible; // 显示图标
                }
                else
                {
                    ImageSource iconSource = iconManager.GetIcon(openFileDialog.FileName); // 获取图标
                    if (iconSource != null)
                    {
                        ButtonImage.Source = iconSource; // 设置图标
                        ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    }
                    else new ToastContentBuilder().AddText("图标提取失败!").Show(); // 显示通知
                }
            }
        }

        // 选择打开文件夹
        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using FolderBrowserDialog folderDialog = new(); // 创建文件夹选择对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LocationTextBox.Text = folderDialog.SelectedPath; // 获取文件夹路径
                TitleTextBox.Text = Path.GetFileName(folderDialog.SelectedPath); // 获取文件夹名称
                buttonManager.AutoEllipsisTextBlock(ButtonTitle, 70); // 调整字体大小

                string cachedIconPath = iconManager.CheckCachedIcon(folderDialog.SelectedPath); // 检查缓存图标
                if (!string.IsNullOrEmpty(cachedIconPath))
                {
                    ButtonImage.Source = new BitmapImage(new Uri(cachedIconPath)); // 设置图标
                    ButtonImage.Visibility = Visibility.Visible; // 显示图标
                }
                else
                {
                    ImageSource folderIcon = iconManager.GetIcon(folderDialog.SelectedPath); // 获取文件夹图标
                    if (folderIcon != null)
                    {
                        ButtonImage.Source = folderIcon; // 设置图标
                        ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    }
                    else new ToastContentBuilder().AddText("图标提取失败!").Show(); // 显示通知
                }
            }
        }

        // 复制地址
        private void CopyLocation(object sender, RoutedEventArgs e)
        {
            string clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            LocationTextBox.Text = clipboardText; // 设置地址栏文本
        }

        // 选择打开网址
        private void ChooseWebsite()
        {
            ChoiceComboBox.SelectedIndex = 1; // 设置选择框为网址
        }

        // 保存动作
        private void Save()
        {
            bool runbymessager = RunByMessager.IsChecked == true; // 是否通过管理员身份运行
            bool trytoopenexitingwindow = TryToOpenExitingWindow.IsChecked == true; // 是否尝试打开已存在的窗口
            int windowState = 0; // 窗口状态
            if (WindowStateComboBox.SelectedIndex != -1) windowState = WindowStateComboBox.SelectedIndex; // 获取窗口状态

            // 处理图标路径
            iconPath = ButtonImage.Visibility == Visibility.Visible ? iconManager.SaveIconToFile(ButtonImage.Source) : "none"; // 如果图标可见，则保存图标，否则设置为默认值
            var buttonData = new ButtonData
            {
                ButtonID = CurrentButton,
                ButtonName = TitleTextBox.Text,
                Location = LocationTextBox.Text,
                ImagePath = iconPath,
                RunByMessager = runbymessager,
                TryToOpenExitingWindow = trytoopenexitingwindow,
                WindowState = windowState,
                Usage = UsageTextBox.Text,
                CreateTime = DateTime.Now,
                LatestEditTime = DateTime.Now
            }; // 创建按钮数据对象
            (Choice != 0 ? (Action<ButtonData>)db2.AddAction : db2.UpdateAction)(buttonData); // 添加或更新动作
            this.Close(); // 关闭窗口
        }

        // 点击保存按钮保存动作
        private void SaveAction(object sender, RoutedEventArgs e)
        {
            Save(); // 保存动作
        }

        // 按下 S 键保存动作
        private void SaveAction(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S) Save(); // 保存动作
        }

        // 选择已有图标
        private void AddImage(object sender, RoutedEventArgs e)
        {
            SelectImage(sender, e); // 选择本地图片
        }

        // 更改ButtonName
        private void UpdateTitle(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ButtonTitle.Visibility = Visibility.Visible; // 显示标题
                ButtonTitle.Text = TitleTextBox.Text; // 更新标题
            }
            else ButtonTitle.Visibility = Visibility.Collapsed; // 隐藏标题
            UpdateTooltip(); // 更新提示文本
        }

        // 编辑动作 加载动作信息
        private void LoadButtonInformation()
        {
            Match buttonid = Regex.Match(CurrentButton, @"\d+"); // 获取按钮ID
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据

            if (!string.IsNullOrWhiteSpace(buttonData.ButtonName))
            {
                ButtonTitle.Visibility = Visibility.Visible; // 显示按钮名称
                ButtonTitle.Text = buttonData.ButtonName; // 显示按钮名称
            } // 如果按钮名称不为空
            TitleTextBox.Text = buttonData.ButtonName; // 设置按钮名称
            LocationTextBox.Text = buttonData.Location; // 设置文件地址
            if (buttonData.ImagePath != "none")
            {
                try
                {
                    ButtonImage.Visibility = Visibility.Visible;
                    ButtonImage.Source = new BitmapImage(new Uri(buttonData.ImagePath));
                }
                catch
                {
                    ButtonImage.Visibility = Visibility.Collapsed; // 如果加载失败，隐藏图标
                }
            } // 如果图标路径不为默认值
            RunByMessager.IsChecked = buttonData.RunByMessager; // 设置是否通过管理员身份运行
            TryToOpenExitingWindow.IsChecked = buttonData.TryToOpenExitingWindow; // 设置是否尝试打开已存在的窗口
            switch (buttonData.WindowState)
            {
                case 0:
                    break; // 默认
                case 1:
                    WindowStateComboBox.SelectedIndex = 1;
                    break; // 最大化
                case 2:
                    WindowStateComboBox.SelectedIndex = 2;
                    break; // 最小化
            } // 设置窗口状态
            UsageTextBox.Text = buttonData.Usage; // 设置用途
            UpdateTooltip(); // 更新提示文本
        }

        // 更新提示文本
        private void UpdateTooltip()
        {
            string toolTipText = null; // 提示文本
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text) || !string.IsNullOrWhiteSpace(UsageTextBox.Text))
            {
                string name = !string.IsNullOrWhiteSpace(TitleTextBox.Text) ? TitleTextBox.Text : null; // 获取按钮名称
                string usage = !string.IsNullOrWhiteSpace(UsageTextBox.Text) ? UsageTextBox.Text : null; // 获取按钮用途
                toolTipText = (name + "\n" + usage).Trim('\n'); // 设置按钮提示文本
            } // 如果按钮名称或用途不为空
            ButtonView.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置按钮提示文本
        }

        // 如果FindAppsWindow存在，则闪烁窗口并响起提示音
        private void AddWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FindAppsWindow != null)
            {
                SystemSounds.Beep.Play(); // 播放提示音
                FindAppsWindow.Focus(); // 设置焦点
            }
        }

        // 选择本地图片
        private void SelectImage(object sender, RoutedEventArgs e)
        {
            SelectImageWindow selectImageWindow = new(); // 创建 SelectImageWindow 实例
            selectImageWindow.ImageConfirmed += OnImageConfirmed; // 订阅 ImageConfirmed 事件
            selectImageWindow.Owner = this; // 设置所有者为当前窗口
            selectImageWindow.ShowDialog(); // 显示为模式对话框
        }

        // 处理选择的图片
        private void OnImageConfirmed(object sender, string e)
        {           
            if (!string.IsNullOrEmpty(e))
            {
                ButtonImage.Source = new BitmapImage(new Uri(e)); // 设置图标
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
            }
        }

        // 更新提示文本
        private void UsageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTooltip(); // 更新提示文本
        }

        // 关闭窗口前，取消事件订阅
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            if (FindAppsWindow != null)
            {
                FindAppsWindow.ApplicationSelected -= OnApplicationSelected;
            } // 取消事件订阅
            if (SelectImageWindow != null)
            {
                SelectImageWindow.ImageConfirmed -= OnImageConfirmed;
            } // 取消事件订阅
            ButtonImage.Source = null; // 释放图标资源
            ButtonTitle = null; // 释放托管资源
            ButtonImage = null; // 释放托管资源
            FindAppsWindow = null; // 释放托管资源
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待所有终止线程完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}