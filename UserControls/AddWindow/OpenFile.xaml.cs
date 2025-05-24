using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System.IO;

namespace Quicker.UserControls.AddWindow
{
    public partial class OpenFile : UserControl
    {
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly IconManager iconManager = new(); // 图标管理器接口
        public FindAppsWindow findAppsWindow; // FindAppsWindow 的静态引用
        private Quicker.AddWindow AddWindow; // AddWindow 的静态引用
        private ButtonDatabase db2 = new(); // 初始换按钮数据库

        public OpenFile(Quicker.AddWindow addWindow)
        {
            AddWindow = addWindow; // 保存 AddWindow 的静态引用
            InitializeComponent();
        }

        // UI 加载完成后执行
        private void OpenFile_Loaded(object sender, RoutedEventArgs e)
        {
            ExecuteChoiceAction(); // 根据上个窗口数据执行对应命令
        }

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            switch (AddWindow.Choice) // 根据选择执行对应命令
            {
                case 0:
                    LoadButtonInformation(); // 编辑动作，加载动作信息
                    break; // 加载动作信息
                case 1:
                    ChooseApplications(null, null); // 选择本地应用
                    break; // 选择应用程序
                case 2:
                    ChooseProcess(null, null); // 选择打开程序
                    break; // 选择文件
                case 3:
                    ChooseFolder(null, null); // 选择文件夹
                    break; // 选择文件夹
            }
        }

        // 编辑动作 加载动作信息
        private void LoadButtonInformation()
        {
            ButtonData buttonData = db2.GetButtonDataByID(AddWindow.CurrentButton, AddWindow.TableName); // 获取按钮数据
            switch(buttonData.ActionType)
            {
                case "OpenFile":
                case "OpenFiles":
                case "OpenUwpApp":
                    break; // 加载打开文件动作信息
                default:
                    return; // 其他动作类型不加载
            }

            if (!string.IsNullOrWhiteSpace(buttonData.Title))
            {
                AddWindow.ButtonTitle.Visibility = Visibility.Visible; // 显示按钮名称
                AddWindow.ButtonTitle.Text = buttonData.Title; // 显示按钮名称
            } // 如果按钮名称不为空
            AddWindow.TitleTextBox.Text = buttonData.Title; // 设置按钮名称
            LocationTextBox.Text = buttonData.Location; // 设置文件地址
            if (!string.IsNullOrEmpty(buttonData.ImagePath))
            {
                try
                {
                    AddWindow.ButtonImage.Visibility = Visibility.Visible;
                    AddWindow.ButtonImage.Source = new BitmapImage(new Uri(buttonData.ImagePath));
                }
                catch
                {
                    AddWindow.ButtonImage.Visibility = Visibility.Collapsed; // 如果加载失败，隐藏图标
                }
            } // 如果图标路径不为默认值
            RunByMessager.IsChecked = buttonData.Data1 == "True"; // 设置是否通过管理员身份运行
            TryToOpenExitingWindow.IsChecked = buttonData.Data2 == "True"; // 设置是否尝试打开已存在的窗口
            WindowStateComboBox.SelectedIndex = int.Parse(buttonData.Data3);
            AddWindow.DescriptionTextBox.Text = buttonData.Description; // 设置用途
            AddWindow.UpdateTooltip(); // 更新提示文本
        }

        // 打开菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            OpenFilePopup.IsOpen = true; // 打开弹出菜单
        }

        // 选择本地应用
        private void ChooseApplications(object sender, RoutedEventArgs e)
        {
            findAppsWindow = new() { Owner = AddWindow }; // 创建 FindAppsWindow 实例，并设置所有者为当前窗口
            findAppsWindow.ApplicationSelected += OnApplicationSelected; // 订阅 ApplicationSelected 事件
            findAppsWindow.ShowDialog(); // 显示为模式对话框
        }

        // 处理选中的应用
        private void OnApplicationSelected(object sender, FindAppsWindow.ApplicationSelectedEventArgs e)
        {
            findAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
            AppInfo selectedApp = e.SelectedApp; // 获取选中的应用信息
            if (selectedApp != null)
            {
                // 更新控件数据
                AddWindow.TitleTextBox.Text = selectedApp.Name; // 设置标题
                LocationTextBox.Text = selectedApp.Location; // 设置地址

                if(selectedApp.Icon!= null)
                {
                    AddWindow.ButtonImage.Source = selectedApp.Icon; // 设置图标
                    AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                }
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
                AddWindow.TitleTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // 获取文件名
                AddWindow.ButtonTitle.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // 设置按钮标题
                buttonManager.AutoEllipsisTextBlock(AddWindow.ButtonTitle, 70); // 调整字体大小

                string cachedIconPath = iconManager.CheckCachedIcon(openFileDialog.FileName); // 检查缓存图标
                if (!string.IsNullOrEmpty(cachedIconPath)) // 如果缓存图标存在
                {
                    AddWindow.ButtonImage.Source = new BitmapImage(new Uri(cachedIconPath)); // 设置图标
                    AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                }
                else
                {
                    ImageSource iconSource = iconManager.GetIcon(openFileDialog.FileName); // 获取图标
                    if (iconSource != null)
                    {
                        AddWindow.ButtonImage.Source = iconSource; // 设置图标
                        AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    }
                    else
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.ShowToast("图标提取失败!","Error"); // 弹出消息提醒
                    }
                }
            }
        }

        // 选择打开文件夹
        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog folderDialog = new(); // 创建文件夹选择对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LocationTextBox.Text = folderDialog.SelectedPath; // 获取文件夹路径
                AddWindow.TitleTextBox.Text = Path.GetFileName(folderDialog.SelectedPath); // 获取文件夹名称

                string cachedIconPath = iconManager.CheckCachedIcon(folderDialog.SelectedPath); // 检查缓存图标
                if (!string.IsNullOrEmpty(cachedIconPath))
                {
                    AddWindow.ButtonImage.Source = new BitmapImage(new Uri(cachedIconPath)); // 设置图标
                    AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                }
                else
                {
                    ImageSource folderIcon = iconManager.GetIcon(folderDialog.SelectedPath); // 获取文件夹图标
                    if (folderIcon != null)
                    {
                        AddWindow.ButtonImage.Source = folderIcon; // 设置图标
                        AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    }
                    else
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.ShowToast("图标提取失败!","Error"); // 弹出消息提醒
                    }
                }
            }
        }

        // 复制地址
        private void CopyLocation(object sender, RoutedEventArgs e)
        {
            string clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            LocationTextBox.Text = buttonManager.ProcessLocation(clipboardText); // 设置地址栏文本
        }

        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LocationTextBox.Text == "可以用英文分号隔开不同路径来添加多个文件" || string.IsNullOrWhiteSpace(LocationTextBox.Text)) 
            {
                LocationTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C8C8C"));
                LocationTextBox.FontSize = 11;
                AddWindow.SaveButton.IsEnabled = false; // 禁用保存按钮
            } // 如果地址栏为空或默认提示，则禁用保存按钮
            else
            {
                LocationTextBox.Foreground = new SolidColorBrush(Colors.Black);
                LocationTextBox.FontSize = 12;
                AddWindow.SaveButton.IsEnabled = true; // 启用保存按钮
            } // 否则启用保存按钮
        }

        // 保存动作
        public void Save()
        {
            bool runbymessager = RunByMessager.IsChecked == true; // 是否通过管理员身份运行
            bool trytoopenexitingwindow = TryToOpenExitingWindow.IsChecked == true; // 是否尝试打开已存在的窗口
            int windowState = 0; // 窗口状态
            if (WindowStateComboBox.SelectedIndex != -1) windowState = WindowStateComboBox.SelectedIndex; // 获取窗口状态
            AddWindow.iconPath = AddWindow.ButtonImage.Visibility == Visibility.Visible
                ? iconManager.SaveIconToFile(AddWindow.ButtonImage.Source)
                : ""; // 如果图标可见，则保存图标，否则设置为默认值

            string actionType = "OpenFile"; // 默认动作类型
            if(LocationTextBox.Text.Contains(";"))
            {
                actionType = "OpenFiles"; // 如果地址栏包含分号，则设置为打开多个文件动作
            }
            else
            {
                string pattern = @"^[A-Za-z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*$"; // 正则表达式，用于检查地址是否为有效路径
                bool isValidPath = Regex.IsMatch(LocationTextBox.Text, pattern); // 检查地址是否为有效路径
                if (!isValidPath)
                    actionType = "OpenUwpApp"; // 如果地址栏不是有效路径，则设置为打开 UWP 应用
            }

            var oldData = db2.GetButtonDataByID(AddWindow.CurrentButton, AddWindow.TableName); // 获取旧数据
            DateTime createdTime = DateTime.Now;
            if (oldData != null)  createdTime= oldData.CreateTime; // 获取创建时间
            var buttonData = new ButtonData
            {
                ButtonID = AddWindow.CurrentButton,
                Title = AddWindow.TitleTextBox.Text,
                Location = LocationTextBox.Text,
                ImagePath = AddWindow.iconPath,
                Data1 = runbymessager.ToString(),
                Data2 = trytoopenexitingwindow.ToString(),
                Data3 = windowState.ToString(),
                Description = AddWindow.DescriptionTextBox.Text,
                CreateTime = createdTime,
                LatestEditTime = DateTime.Now,
                ActionType = actionType
            }; // 创建按钮数据对象
            db2.UpdateAction(buttonData, AddWindow.TableName); // 添加或更新动作
        }

        // 获得焦点时隐藏提示
        private void LocationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(LocationTextBox.Text == "可以用英文分号隔开不同路径来添加多个文件")
                LocationTextBox.Text = "";
            LocationTextBox.Foreground = new SolidColorBrush(Colors.Black);
            LocationTextBox.FontSize = 12;
        }

        // 失去焦点时显示提示
        private void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                LocationTextBox.Text = "可以用英文分号隔开不同路径来添加多个文件";
                LocationTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C8C8C"));
                LocationTextBox.FontSize = 11;
            }
        }

        // 移除控件清理资源
        private void OpenFile_Unloaded(object sender, RoutedEventArgs e)
        {
            if (findAppsWindow != null) // 取消事件订阅
            {
                findAppsWindow.ApplicationSelected -= OnApplicationSelected;
                findAppsWindow = null;
            }
            buttonManager.Dispose(); // 释放资源
            iconManager.Dispose(); // 释放资源
            LocationTextBox.Text = ""; // 清空地址栏
            WindowStateComboBox.SelectedIndex = -1; // 清空窗口状态下拉框
            RunByMessager.IsChecked = false; // 取消勾选
            TryToOpenExitingWindow.IsChecked = false; // 取消勾选
            AddWindow = null; // 清空引用
            db2 = null; // 释放资源
        }
    }
}