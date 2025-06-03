using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.IO;

namespace Quicker.UserControls.AddWindow
{
    /// <summary>
    /// OpenFile 控件用于处理文件、应用程序和文件夹的选择和打开
    /// </summary>
    public partial class OpenFile : UserControl
    {
        #region 字段

        private const string DefaultLocationText = "可以用英文分号隔开不同路径来添加多个文件";
        private Quicker.Windows.MainWindows.AddWindow _addWindow; // AddWindow 的引用
        private readonly ButtonManager _buttonManager = new(); // 按钮管理器
        private readonly IconManager _iconManager = new(); // 图标管理器接口
        public FindAppsWindow findAppsWindow; // FindAppsWindow 的引用
        private ButtonDatabase _buttonDb = new(); // 按钮数据库

        #endregion

        #region 构造函数

        public OpenFile(Quicker.Windows.MainWindows.AddWindow addWindow)
        {
            _addWindow = addWindow; // 保存 AddWindow 的引用
            InitializeComponent();
        }

        #endregion

        #region 事件处理

        // UI 加载完成后执行
        private void OpenFile_Loaded(object sender, RoutedEventArgs e)
        {
            ExecuteChoiceAction(); // 根据上个窗口数据执行对应命令
        }

        // 打开菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            OpenFilePopup.IsOpen = true; // 打开弹出菜单
        }

        // 选择本地应用
        private void ChooseApplications(object sender, RoutedEventArgs e)
        {
            findAppsWindow = new() { Owner = _addWindow }; // 创建 FindAppsWindow 实例，并设置所有者为当前窗口
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
                _addWindow.TitleTextBox.Text = selectedApp.Name; // 设置标题
                LocationTextBox.Text = selectedApp.Location; // 设置地址
                if (selectedApp.Icon != null)
                {
                    _addWindow.ButtonImage.Source = selectedApp.Icon; // 设置图标
                    _addWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
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
            
            if (openFileDialog.ShowDialog() == true) // 检查用户是否点击了"确定"
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // 更新UI
                LocationTextBox.Text = filePath;
                _addWindow.TitleTextBox.Text = fileName;
                _addWindow.ButtonTitle.Text = fileName;
                _buttonManager.AutoEllipsisTextBlock(_addWindow.ButtonTitle, 70);

                // 设置图标
                SetIconFromPath(filePath);
            }
        }

        // 选择打开文件夹
        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog folderDialog = new(); // 创建文件夹选择对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderDialog.SelectedPath;
                
                // 更新UI
                LocationTextBox.Text = folderPath;
                _addWindow.TitleTextBox.Text = Path.GetFileName(folderPath);
                
                // 设置图标
                SetIconFromPath(folderPath);
            }
        }

        // 复制地址
        private void CopyLocation(object sender, RoutedEventArgs e)
        {
            string clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            LocationTextBox.Text = _buttonManager.ProcessLocation(clipboardText); // 设置地址栏文本
        }

        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isEmpty = LocationTextBox.Text == DefaultLocationText || string.IsNullOrWhiteSpace(LocationTextBox.Text);
            if (isEmpty)
            {
                LocationTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C8C8C")); // 字体颜色
                LocationTextBox.FontSize = 11; // 字体大小
                _addWindow.SaveButton.IsEnabled = false; // 禁用保存按钮
            }
            else
            {
                LocationTextBox.Foreground = new SolidColorBrush(Colors.Black); // 字体颜色
                LocationTextBox.FontSize = 12; // 字体大小
                _addWindow.SaveButton.IsEnabled = true; // 启用保存按钮
            }
        }

        // 获得焦点时隐藏提示
        private void LocationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LocationTextBox.Text == DefaultLocationText)
                LocationTextBox.Text = "";
            LocationTextBox.Foreground = new SolidColorBrush(Colors.Black); // 字体颜色
            LocationTextBox.FontSize = 12; // 字体大小
        }

        // 失去焦点时显示提示
        private void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                LocationTextBox.Text = DefaultLocationText;
                LocationTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8C8C8C"));
                LocationTextBox.FontSize = 11;
            }
        }

        // 移除控件清理资源
        private void OpenFile_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources();
        }

        #endregion

        #region 业务逻辑

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            switch (_addWindow.Choice) // 根据选择执行对应命令
            {
                case 0:
                    LoadButtonInformation(); // 编辑动作，加载动作信息
                    break;
                case 1:
                    ChooseApplications(null, null); // 选择本地应用
                    break;
                case 2:
                    ChooseProcess(null, null); // 选择打开程序
                    break;
                case 3:
                    ChooseFolder(null, null); // 选择文件夹
                    break;
            }
        }

        // 编辑动作 加载动作信息
        private void LoadButtonInformation()
        {
            ButtonData buttonData = _buttonDb.GetButtonDataByID(_addWindow.ButtonID, _addWindow.TableName); // 获取按钮数据
            
            // 检查动作类型是否为打开文件相关
            switch (buttonData.ActionType)
            {
                case "OpenFile":
                case "OpenFiles":
                case "OpenUwpApp":
                    break; // 加载打开文件动作信息
                default:
                    return; // 其他动作类型不加载
            }

            // 设置UI控件
            if (!string.IsNullOrWhiteSpace(buttonData.Title))
            {
                _addWindow.ButtonTitle.Visibility = Visibility.Visible;
                _addWindow.ButtonTitle.Text = buttonData.Title;
            }
            
            _addWindow.TitleTextBox.Text = buttonData.Title;
            LocationTextBox.Text = buttonData.Location;
            
            // 设置图标
            if (!string.IsNullOrEmpty(buttonData.ImagePath))
            {
                try
                {
                    _addWindow.ButtonImage.Visibility = Visibility.Visible;
                    _addWindow.ButtonImage.Source = new BitmapImage(new Uri(buttonData.ImagePath));
                }
                catch
                {
                    _addWindow.ButtonImage.Visibility = Visibility.Collapsed; // 如果加载失败，隐藏图标
                }
            }
            
            // 设置其他选项
            RunByMessager.IsChecked = buttonData.Data1 == "True";
            TryToOpenExitingWindow.IsChecked = buttonData.Data2 == "True";
            WindowStateComboBox.SelectedIndex = int.Parse(buttonData.Data3);
            _addWindow.DescriptionTextBox.Text = buttonData.Description;
            _addWindow.UpdateTooltip(); // 更新提示文本
        }

        // 保存动作
        public void Save()
        {
            bool runByMessager = RunByMessager.IsChecked == true;
            bool tryToOpenExitingWindow = TryToOpenExitingWindow.IsChecked == true;
            int windowState = WindowStateComboBox.SelectedIndex != -1 ? WindowStateComboBox.SelectedIndex : 0;
            
            // 保存图标
            _addWindow.iconPath = _addWindow.ButtonImage.Visibility == Visibility.Visible
                ? _iconManager.SaveIconToFile(_addWindow.ButtonImage.Source)
                : "";

            // 确定动作类型
            string actionType = DetermineActionType();

            // 获取旧数据并保留创建时间
            var oldData = _buttonDb.GetButtonDataByID(_addWindow.ButtonID, _addWindow.TableName);
            DateTime createdTime = oldData != null ? oldData.CreateTime : DateTime.Now;
            var buttonData = new ButtonData
            {
                ButtonID = _addWindow.ButtonID,
                Title = _addWindow.TitleTextBox.Text,
                Location = LocationTextBox.Text,
                ImagePath = _addWindow.iconPath,
                Data1 = runByMessager.ToString(),
                Data2 = tryToOpenExitingWindow.ToString(),
                Data3 = windowState.ToString(),
                Description = _addWindow.DescriptionTextBox.Text,
                CreateTime = createdTime,
                LatestEditTime = DateTime.Now,
                ActionType = actionType
            }; // 创建按钮数据对象
            
            // 更新数据库
            _buttonDb.UpdateAction(buttonData, _addWindow.TableName);
        }

        #endregion

        #region 辅助方法

        // 设置图标
        private void SetIconFromPath(string path)
        {
            string cachedIconPath = _iconManager.CheckCachedIcon(path);
            
            if (!string.IsNullOrEmpty(cachedIconPath))
            {
                _addWindow.ButtonImage.Source = new BitmapImage(new Uri(cachedIconPath));
                _addWindow.ButtonImage.Visibility = Visibility.Visible;
            }
            else
            {
                ImageSource iconSource = _iconManager.GetIcon(path);
                if (iconSource != null)
                {
                    _addWindow.ButtonImage.Source = iconSource;
                    _addWindow.ButtonImage.Visibility = Visibility.Visible;
                }
                else
                {
                    using var toast = new ToastManager();
                    toast.Show("图标提取失败!", "Error");
                }
            }
        }

        // 确定动作类型
        private string DetermineActionType()
        {
            if (LocationTextBox.Text.Contains(";"))
            {
                return "OpenFiles"; // 如果地址栏包含分号，则设置为打开多个文件动作
            }
            string pattern = @"^[A-Za-z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*$";
            bool isValidPath = Regex.IsMatch(LocationTextBox.Text, pattern);
            return isValidPath ? "OpenFile" : "OpenUwpApp";
        }

        // 清理资源
        private void CleanupResources()
        {
            if (findAppsWindow != null)
            {
                findAppsWindow.ApplicationSelected -= OnApplicationSelected;
                findAppsWindow = null;
            }
            
            _buttonManager.Dispose();
            _iconManager.Dispose();
            
            // 重置UI控件状态
            LocationTextBox.Text = "";
            WindowStateComboBox.SelectedIndex = -1;
            RunByMessager.IsChecked = false;
            TryToOpenExitingWindow.IsChecked = false;
            
            // 清空引用
            _addWindow = null;
            _buttonDb = null;
        }

        #endregion
    }
}