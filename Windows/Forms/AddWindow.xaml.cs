using System.Text.RegularExpressions;
using Quicker.UserControls.AddWindow;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using System.Media;
using System.IO;

namespace Quicker
{
    public partial class AddWindow : Window
    {
        private readonly ButtonManager buttonManager = new ButtonManager(); // 按钮管理器
        private readonly IconManager iconManager = new IconManager(); // 图标管理器
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
        private SelectImageWindow selectImageWindow; // SelectImageWindow 的实例引用
        private FindAppsWindow findAppsWindow; // FindAppsWindow 的实例引用
        private bool isLoading = true; // 是否正在加载
        public TextBlock ButtonTitle; // 按钮标题
        public Image ButtonImage; // 按钮图片
        public string iconPath; // 图标路径

        public string CurrentButton { get; private set; } // 当前按钮
        public int Choice { get; private set; } // 选择添加动作类型

        public AddWindow(string currentbutton, int choice)
        {
            CurrentButton = currentbutton; // 当前按钮
            Choice = choice; // 选择添加动作类型
            InitializeComponent(); // 初始化窗口组件
            ExecuteChoiceAction(); // 执行对应命令
        }

        // 初始化标题和Button视图，并根据上个窗口数据执行对应命令
        private void AddWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTitle(); // 初始化标题
            InitializeButtonView(); // 初始化Button视图
            SetWindowHeight(ChoiceComboBox.SelectedIndex); // 设置窗口高度
            isLoading = false; // 加载完成
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
                            Title = $"新动作--默认第{numbers[0] + 1}页{numbers[1]}行{numbers[2]}列--编辑动作";
                            break; // 默认动作
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
            switch (Choice) // 根据选择执行对应命令
            {
                case 0: // 编辑动作
                    LoadActionInfo(); // 加载动作信息
                    break; // 编辑动作, 加载动作信息
                case 1: // 启动软件
                case 2: // 打开文件
                case 3: // 打开文件夹
                    ChoiceComboBox.SelectedIndex = 0;
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加 OpenFile 控件到布局中
                    break; // 选择文件
                case 4:
                    ChoiceComboBox.SelectedIndex = 1;
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加 OpenWebsite 控件到布局中
                    break; // 选择网址
            }
        }

        // 编辑动作，加载动作信息
        private void LoadActionInfo()
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据
            switch (buttonData.ActionType)
            {
                case "OpenFile":
                case "OpenFiles":
                    ChoiceComboBox.SelectedIndex = 0;
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加 OpenFile 控件到布局中
                    break;
                case "OpenWebsite":
                    ChoiceComboBox.SelectedIndex = 1;
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加 OpenWebsite 控件到布局中
                    break;
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

        // 处理选中的应用
        private void OnApplicationSelected(object sender, FindAppsWindow.ApplicationSelectedEventArgs e)
        {
            AppInfo selectedApp = e.SelectedApp; // 获取选中的应用信息
            if (selectedApp != null)
            {
                // 更新控件数据
                TitleTextBox.Text = selectedApp.Name; // 设置标题

                // 设置图标
                ButtonImage.Source = selectedApp.Icon; // 设置图标
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
                findAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
            }
        }

        // 保存动作
        private void Save()
        {
            switch(ChoiceComboBox.SelectedIndex)
            {
                case 0:
                    OpenFile openFile = (OpenFile)ActionInfoGrid.Children[0]; // 获取 OpenFile 控件
                    openFile.Save(); // 保存打开文件动作
                    break;
                case 1:
                    OpenWebsite openWebsite = (OpenWebsite)ActionInfoGrid.Children[0]; // 获取 OpenWebsite 控件
                    openWebsite.Save(); // 保存打开网址动作
                    break;
            }
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
            if (e.Key == Key.S)
                Save(); // 保存动作
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
                buttonManager.AutoEllipsisTextBlock(ButtonTitle, 70); // 更新按钮名称
            }
            else
                ButtonTitle.Visibility = Visibility.Collapsed; // 隐藏标题
            UpdateTooltip(); // 更新提示文本
        }

        // 更新提示文本
        public void UpdateTooltip()
        {
            string toolTipText = null; // 提示文本
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text) || !string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                string name = !string.IsNullOrWhiteSpace(TitleTextBox.Text) ? TitleTextBox.Text : null; // 获取按钮名称
                string usage = !string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? DescriptionTextBox.Text : null; // 获取按钮用途
                toolTipText = (name + "\n" + usage).Trim('\n'); // 设置按钮提示文本
            } // 如果按钮名称或用途不为空
            ButtonView.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置按钮提示文本
        }

        // 如果FindAppsWindow存在，则闪烁窗口并响起提示音
        private void AddWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (findAppsWindow != null)
            {
                SystemSounds.Beep.Play(); // 播放提示音
                findAppsWindow.Focus(); // 设置焦点
            }
        }

        // 选择本地图片
        private void SelectImage(object sender, RoutedEventArgs e)
        {
            selectImageWindow = new SelectImageWindow(); // 创建 SelectImageWindow 实例
            selectImageWindow.ImageConfirmed += OnImageConfirmed; // 订阅 ImageConfirmed 事件
            selectImageWindow.Owner = this; // 设置所有者为当前窗口
            selectImageWindow.ShowDialog(); // 显示为模式对话框
        }

        // 处理选择的图片
        private void OnImageConfirmed(object sender, string selectedImagePath)
        {           
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                ButtonImage.Source = iconManager.ProcessIcon(selectedImagePath); // 设置图标
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
            }
        }

        // 更新提示文本
        private void UsageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTooltip(); // 更新提示文本
        }

        // 下拉框选项改变时，执行对应命令
        private void ChoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading) return; // 如果正在加载，则不执行命令
            ActionInfoGrid.Children.Clear(); // 清除子控件
            switch (ChoiceComboBox.SelectedIndex)
            {
                case 0:
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加控件
                    SetWindowHeight(0); // 设置窗口高度
                    break; // 编辑动作
                case 1:
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加控件
                    SetWindowHeight(1); // 设置窗口高度
                    break; // 选择应用程序
            }
        }

        // 设置窗口高度
        private void SetWindowHeight(int choice)
        {
            switch(choice)
            {
                case 0:
                    this.Height = 450; // 打开文件的窗口高度
                    break; // 打开文件
                case 1:
                    this.Height = 370; // 打开网址的窗口高度
                    break; // 打开网站
            }
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            if (findAppsWindow != null)
            {
                findAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
                findAppsWindow = null; // 清理静态引用
            }
            if (selectImageWindow != null)
            {
                selectImageWindow.ImageConfirmed -= OnImageConfirmed; // 取消事件订阅
                selectImageWindow = null; // 清理静态引用
            }

            // 清理控件资源
            if (ButtonImage != null)
            {
                ButtonImage.Source = null; // 释放图片资源
                ButtonImage = null; // 清理引用
            }
            if (ButtonTitle != null)
                ButtonTitle = null; // 清理引用
            iconPath = null;

            ButtonView.Content = null; // 清空 ButtonView 的内容

            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}