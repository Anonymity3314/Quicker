using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using Quicker.Database.Core;
using System.Windows.Media;
using Quicker.Managers;
using System.Windows;
using Quicker.Models;
using WpfAnimatedGif;
using System.IO;

namespace Quicker.UserControls.AddWindow
{
    /// <summary>
    /// OpenFile 控件用于处理文件、应用程序和文件夹的选择和打开
    /// </summary>
    public partial class OpenFile : UserControl
    {
        #region 字段

        private Quicker.Windows.AddWindows.AddActionWindow _addWindow; // AddWindow 的引用
        private readonly ButtonManager _buttonManager = new(); // 按钮管理器
        private readonly IconManager _iconManager = new(); // 图标管理器接口
        public FindAppsWindow findAppsWindow; // FindAppsWindow 的引用
        private ButtonDatabase _buttonDb = new(); // 按钮数据库
        private bool isLoading = true; // 是否正在加载

        #endregion

        #region 构造函数

        public OpenFile(Quicker.Windows.AddWindows.AddActionWindow addWindow)
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
            isLoading = false; // 加载完成
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
                UpdateUIWithAppInfo(selectedApp); // 更新UI
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="appInfo">应用信息</param>
        private void UpdateUIWithAppInfo(AppInfo appInfo)
        {
            _addWindow.TitleTextBox.Text = appInfo.Name; // 设置标题
            LocationTextBox.Text = appInfo.Location; // 设置地址
            if (appInfo.Icon != null)
            {
                _addWindow.ButtonImage.Source = appInfo.Icon; // 设置图标
                _addWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
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
                findAppsWindow?.Close(); // 关闭 FindAppsWindow
                string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName); // 获取文件名
                UpdateUIWithFileInfo(openFileDialog.FileName, fileName); // 更新UI
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名</param>
        private void UpdateUIWithFileInfo(string filePath, string fileName)
        {
            LocationTextBox.Text = filePath; // 设置地址
            _addWindow.TitleTextBox.Text = fileName; // 设置标题
            _addWindow.ButtonTitle.Text = fileName; // 设置按钮标题
            _buttonManager.AutoEllipsisTextBlock(_addWindow.ButtonTitle, 70); // 自动省略文本
            SetIconFromPath(filePath); // 设置图标
        }

        // 选择打开文件夹
        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog folderDialog = new(); // 创建文件夹选择对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProcessSelectedFolder(folderDialog.SelectedPath); // 处理选中的文件夹
            }
        }

        /// <summary>
        /// 处理选中的文件夹
        /// </summary>
        /// <param name="folderPath">文件夹路径</param> 
        private void ProcessSelectedFolder(string folderPath)
        {
            LocationTextBox.Text = folderPath; // 设置地址
            _addWindow.TitleTextBox.Text = Path.GetFileName(folderPath); // 设置标题
            SetIconFromPath(folderPath); // 设置图标
        }

        // 复制地址
        private void CopyLocation(object sender, RoutedEventArgs e)
        {
            string clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            string processedLocation = _buttonManager.ProcessLocation(clipboardText); // 处理地址
            ProcessLocationFromClipboard(processedLocation); // 处理剪贴板中的地址
            OpenFilePopup.IsOpen = false; // 关闭弹出菜单
        }

        // 重新提取文件图标
        private void ReGetFileIcon(object sender, RoutedEventArgs e)
        {
            SetIconFromPath(LocationTextBox.Text); // 设置图标
            OpenFilePopup.IsOpen = false; // 关闭弹出菜单
        }

        /// <summary>
        /// 处理剪贴板中的地址
        /// </summary>
        /// <param name="location">地址</param>
        private void ProcessLocationFromClipboard(string location)
        {
            LocationTextBox.Text = location; // 设置地址
            SetIconFromPath(location); // 设置图标
            _addWindow.TitleTextBox.Text = Directory.Exists(location)
                ? Path.GetFileName(location) // 如果是文件夹，则设置为文件夹名
                : Path.GetFileNameWithoutExtension(location); // 否则设置为文件名
        }

        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _addWindow.SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(LocationTextBox.Text);
        }

        // 移除控件清理资源
        private void OpenFile_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources();
        }

        // 调整窗口高度
        private void LocationTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isLoading) return; // 防止在加载过程中调整大小
            if (_addWindow != null)
            {
                double heightChange = e.NewSize.Height - e.PreviousSize.Height; // 计算文本框高度变化量
                _addWindow.Height += heightChange; // 调整窗口高度
                Thickness currentMargin = Grid1.Margin; // 获取当前的Margin
                Thickness newMargin = new Thickness(93, currentMargin.Top + heightChange, 0, 0); // 创建一个新的Margin对象，只修改Top属性
                Grid1.Margin = newMargin; // 将新的Margin赋给Grid1
            }
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
            if (!IsOpenFileAction(buttonData.ActionType))
                return;
            SetButtonTitleAndLocation(buttonData); // 设置标题和地址
            SetButtonImageSafe(buttonData.ImagePath); // 设置图标
            SetOtherOptions(buttonData); // 设置其他选项
        }

        /// <summary>
        /// 判断是否为打开文件相关的动作类型
        /// </summary>
        /// <param name="actionType">动作类型</param>
        /// <returns>是否为打开文件相关的动作类型</returns>
        private bool IsOpenFileAction(string actionType)
        {
            return actionType == "OpenFile" || actionType == "OpenFiles" || actionType == "OpenUwpApp";
        }

        /// <summary>
        /// 设置标题和地址栏
        /// </summary>
        /// <param name="buttonData">按钮数据</param>
        private void SetButtonTitleAndLocation(ButtonData buttonData)
        {
            if (!string.IsNullOrWhiteSpace(buttonData.Title))
            {
                _addWindow.ButtonTitle.Visibility = Visibility.Visible; // 显示按钮标题
                _addWindow.ButtonTitle.Text = buttonData.Title; // 设置按钮标题
            }
            _addWindow.TitleTextBox.Text = buttonData.Title; // 设置标题
            LocationTextBox.Text = buttonData.Location; // 设置地址
        }

        /// <summary>
        /// 安全设置图标（带异常处理）
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        private void SetButtonImageSafe(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    _addWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    SetButtonImageFromPath(imagePath); // 调用主方法
                }
                catch
                {
                    _addWindow.ButtonImage.Visibility = Visibility.Collapsed; // 如果加载失败，隐藏图标
                }
            }
        }

        /// <summary>
        /// 设置其他选项（如复选框、下拉框、描述等）
        /// </summary>
        /// <param name="buttonData">按钮数据</param>
        private void SetOtherOptions(ButtonData buttonData)
        {
            RunByMessager.IsChecked = buttonData.Data1 == "True"; // 设置运行方式
            TryToOpenExitingWindow.IsChecked = buttonData.Data2 == "True"; // 设置是否尝试打开已存在的窗口
            WindowStateComboBox.SelectedIndex = int.Parse(buttonData.Data3); // 设置窗口状态
            _addWindow.DescriptionTextBox.Text = buttonData.Description; // 设置描述
            _addWindow.UpdateTooltip(); // 更新提示文本
        }

        /// <summary>
        /// 根据图片路径设置ButtonImage，自动识别SVG和普通图片
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        private void SetButtonImageFromPath(string imagePath)
        {
            string ext = Path.GetExtension(imagePath).ToLower(); // 获取文件扩展名
            if (ext == ".svg") // 如果扩展名是SVG
            {
                SetSvgButtonImage(imagePath); // 设置SVG图片
            }
            else // 其它格式
            {
                SetBitmapButtonImage(imagePath); // 设置普通图片
            }
            _addWindow.iconPath = imagePath; // 同步iconPath
        }

        /// <summary>
        /// 设置SVG图片到ButtonImage
        /// </summary>
        /// <param name="imagePath">SVG图片路径</param>
        private void SetSvgButtonImage(string imagePath)
        {
            var iconManager = new IconManager(); // 创建 IconManager 实例
            _addWindow.ButtonImage.Source = iconManager.LoadSvgToBitmapImage(imagePath); // 加载SVG图片
        }

        /// <summary>
        /// 设置普通图片（含GIF动图）到ButtonImage
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        private void SetBitmapButtonImage(string imagePath)
        {
            var bitmap = new BitmapImage(new Uri(imagePath)); // 创建 BitmapImage 实例
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(_addWindow.ButtonImage, bitmap); // 支持GIF动图
        }

        // 保存动作
        public void Save()
        {
            bool runByMessager = RunByMessager.IsChecked == true;
            bool tryToOpenExitingWindow = TryToOpenExitingWindow.IsChecked == true;
            int windowState = WindowStateComboBox.SelectedIndex != -1 ? WindowStateComboBox.SelectedIndex : 0;

            // 保存图标
            _addWindow.iconPath = _addWindow.ButtonImage.Visibility == Visibility.Visible
                ? _addWindow.SaveIconToLocal()
                : "";

            // 确定动作类型
            string actionType = DetermineActionType();

            // 获取旧数据并保留创建时间
            var oldData = _buttonDb.GetButtonDataByID(_addWindow.ButtonID, _addWindow.TableName);
            DateTime createdTime = oldData != null ? oldData.CreateTime : DateTime.Now;
            int usedTimes = oldData != null ? oldData.UsedTimes : 0;
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
                ActionType = actionType,
                UsedTimes = usedTimes
            }; // 创建按钮数据对象
            
            // 更新数据库
            _buttonDb.UpdateAction(buttonData, _addWindow.TableName);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 根据路径设置图标
        /// </summary>
        /// <param name="path">路径</param>
        private void SetIconFromPath(string path)
        {
            string cachedIconPath = _iconManager.CheckCachedIcon(path); // 检查缓存图标
            if (!string.IsNullOrEmpty(cachedIconPath))
            {
                _addWindow.SetButtonImage(cachedIconPath); // 设置图标
            }
            else
            {
                ImageSource iconSource = _iconManager.GetIcon(path); // 获取图标
                if (iconSource != null)
                {
                    _addWindow.ButtonImage.Source = iconSource; // 设置图标
                    _addWindow.ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    _addWindow.iconPath = ""; // 清空旧的图标路径，保证保存时从当前图像源导出
                }
                else
                {
                    using var toast = new ToastManager(); // 创建 ToastManager 实例
                    toast.Show("图标提取失败!", ToastType.Error); // 显示错误提示
                }
            }
        }

        /// <summary>
        /// 确定动作类型
        /// </summary>
        /// <returns>动作类型</returns>
        private string DetermineActionType()
        {
            if (LocationTextBox.Text.Contains(";"))
            {
                return "OpenFiles"; // 如果地址栏包含分号，则设置为打开多个文件动作
            }
            string pattern = @"^[A-Za-z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*$";
            bool isValidPath = Regex.IsMatch(LocationTextBox.Text, pattern); // 检查路径是否有效
            return isValidPath ? "OpenFile" : "OpenUwpApp"; // 返回动作类型
        }

        // 清理资源
        private void CleanupResources()
        {
            if (findAppsWindow != null)
            {
                findAppsWindow.ApplicationSelected -= OnApplicationSelected;
                findAppsWindow = null;
            }

            _buttonManager.Dispose(); // 释放按钮管理器
            _iconManager.Dispose(); // 释放图标管理器

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