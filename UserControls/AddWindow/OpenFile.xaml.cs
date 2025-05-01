using Microsoft.Toolkit.Uwp.Notifications;
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
        private readonly ButtonManager buttonManager = new ButtonManager(); // 按钮管理器接口
        private readonly IconManager iconManager = new IconManager(); // 图标管理器接口
        public static FindAppsWindow FindAppsWindow; // FindAppsWindow 的静态引用
        private Quicker.AddWindow AddWindow; // AddWindow 的静态引用

        public OpenFile(Quicker.AddWindow addWindow)
        {
            InitializeComponent();
            AddWindow = addWindow; // 保存 AddWindow 的静态引用
        }

        // 打开菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            OpenFilePopup.IsOpen = true; // 打开弹出菜单
        }

        // 选择本地应用
        private void ChooseApplications(object sender, RoutedEventArgs e)
        {
            FindAppsWindow = new() { Owner = AddWindow }; // 创建 FindAppsWindow 实例，并设置所有者为当前窗口
            FindAppsWindow.ApplicationSelected += OnApplicationSelected; // 订阅 ApplicationSelected 事件
            FindAppsWindow.ShowDialog(); // 显示为模式对话框
        }

        // 处理选中的应用
        private void OnApplicationSelected(object sender, FindAppsWindow.ApplicationSelectedEventArgs e)
        {
            AppInfo selectedApp = e.SelectedApp; // 获取选中的应用信息
            if (selectedApp != null)
            {
                // 更新控件数据
                AddWindow.TitleTextBox.Text = selectedApp.Name; // 设置标题
                LocationTextBox.Text = selectedApp.Location; // 设置地址

                // 设置图标
                AddWindow.ButtonImage.Source = selectedApp.Icon; // 设置图标
                AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
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
                    else new ToastContentBuilder().AddText("图标提取失败!").Show(); // 显示通知
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
                buttonManager.AutoEllipsisTextBlock(AddWindow.ButtonTitle, 70); // 调整字体大小

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


        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LocationTextBox.Text))
                AddWindow.SaveButton.IsEnabled = true; // 启用保存按钮
            else
                AddWindow.SaveButton.IsEnabled = false; // 禁用保存按钮
        }

        // 保存动作
        private void Save()
        {
            bool runbymessager = RunByMessager.IsChecked == true; // 是否通过管理员身份运行
            bool trytoopenexitingwindow = TryToOpenExitingWindow.IsChecked == true; // 是否尝试打开已存在的窗口
            int windowState = 0; // 窗口状态
            if (WindowStateComboBox.SelectedIndex != -1) windowState = WindowStateComboBox.SelectedIndex; // 获取窗口状态

            var buttonData = new ButtonData
            {
                Location = LocationTextBox.Text,
                RunByMessager = runbymessager,
                TryToOpenExitingWindow = trytoopenexitingwindow,
                WindowState = windowState,
                CreateTime = DateTime.Now,
                LatestEditTime = DateTime.Now,
                Type = "OpenFile"
            }; // 创建按钮数据对象
        }
    }
}