using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using Quicker.Models;
using System.Net;
using System.IO;

namespace Quicker.UserControls.AddWindow
{
    /// <summary>
    /// OpenWebsite 控件用于处理网站打开相关操作
    /// </summary>
    public partial class OpenWebsite : UserControl
    {
        #region 字段

        private Quicker.Windows.MainWindows.AddWindow _addWindow; // AddWindow 的引用
        private readonly ButtonManager _buttonManager = new(); // 按钮管理器接口
        private readonly IconManager _iconManager = new(); // 图标管理器接口
        private ButtonDatabase _buttonDb = new(); // 按钮数据库
        private const string DefaultUrlPrefix = "https://"; // 默认URL前缀
        private bool isLoading = true; // 是否正在加载

        #endregion

        #region 构造函数

        public OpenWebsite(Quicker.Windows.MainWindows.AddWindow addWindow)
        {
            _addWindow = addWindow; // 保存 AddWindow 的引用
            InitializeComponent();
        }

        #endregion

        #region 事件处理

        // UI 加载完成后执行
        private void OpenWebsite_Loaded(object sender, RoutedEventArgs e)
        {
            ExecuteChoiceAction(); // 执行对应命令
            isLoading = false; // 加载完成
        }

        // 打开菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            OpenWebsitePopup.IsOpen = true; // 打开弹出菜单
        }

        // 复制地址
        private void CopyLocation(object sender, RoutedEventArgs e)
        {
            string clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            LocationTextBox.Text = _buttonManager.ProcessLocation(clipboardText); // 设置地址栏文本
            OpenWebsitePopup.IsOpen = false; // 关闭弹出菜单
        }

        // 如果地址栏不为空，则启用保存按钮
        private void LocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _addWindow.SaveButton.IsEnabled = LocationTextBox.Text.Trim() != DefaultUrlPrefix; // 如果地址不为协议，则启用保存按钮
        }

        // 点击按钮获取网站图标
        private void GetWebsiteIconButton_Click(object sender, RoutedEventArgs e)
        {
            FetchWebsiteIcon();
        }

        // 如果是自定义浏览器，显示相关控件进行相关设置
        private void BrowserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserControlGrid == null) return;
            
            UserControlGrid.Visibility = BrowserComboBox.SelectedIndex == 8 
                ? Visibility.Visible    // 如果是自定义浏览器，则显示相关控件
                : Visibility.Collapsed; // 否则隐藏相关控件
        }

        // 关闭控件释放资源
        private void OpenWebsite_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupResources(); // 清理资源
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
                Thickness newMargin = new Thickness(0, currentMargin.Top + heightChange, 0, 0); // 创建一个新的Margin对象，只修改Top属性
                Grid1.Margin = newMargin; // 将新的Margin赋给Grid1
            }
        }

        #endregion

        #region 业务逻辑

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            if (_addWindow.Choice == 0) // 打开文件
                LoadButtonInformation(); // 编辑动作，加载动作信息
        }

        // 编辑动作 加载动作信息
        private void LoadButtonInformation()
        {
            ButtonData buttonData = _buttonDb.GetButtonDataByID(_addWindow.ButtonID, _addWindow.TableName); // 获取按钮数据
            if (buttonData.ActionType != "OpenWebsite") return; // 如果不是打开网站动作，则不执行操作

            // 设置按钮标题
            if (!string.IsNullOrWhiteSpace(buttonData.Title))
            {
                _addWindow.ButtonTitle.Visibility = Visibility.Visible;
                _addWindow.ButtonTitle.Text = buttonData.Title;
            }
            
            _addWindow.TitleTextBox.Text = buttonData.Title;

            // 设置地址和浏览器
            LocationTextBox.Text = buttonData.Location;
            if (buttonData.Data1 == "8") // 如果是自定义浏览器
            {
                BrowserLocation.Text = buttonData.Data2;
            }

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
            BrowserComboBox.SelectedIndex = int.Parse(buttonData.Data1);
            _addWindow.DescriptionTextBox.Text = buttonData.Description;
            _addWindow.UpdateTooltip();
        }

        // 获取网站图标
        private void FetchWebsiteIcon()
        {
            if (string.IsNullOrEmpty(LocationTextBox.Text)) return;
            
            // 获取网站图标和名称
            ImageSource icon = _iconManager.GetWebsiteIcon(LocationTextBox.Text);
            _addWindow.TitleTextBox.Text = _buttonManager.GetWebsiteNameFromUrl(LocationTextBox.Text);
            
            if (icon == null) return;
            
            // 设置图标
            _addWindow.ButtonImage.Source = icon;
            _addWindow.ButtonImage.Visibility = Visibility.Visible;
            _addWindow.iconPath = _iconManager.SaveIconToFile(icon);
        }

        // 保存动作
        public void Save()
        {
            // 保存图标
            _addWindow.iconPath = _addWindow.ButtonImage.Visibility == Visibility.Visible
                ? _iconManager.SaveIconToFile(_addWindow.ButtonImage.Source)
                : "";

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
                Data1 = BrowserComboBox.SelectedIndex.ToString(),
                Data2 = BrowserLocation.Text,
                Description = _addWindow.DescriptionTextBox.Text,
                CreateTime = createdTime,
                ActionType = "OpenWebsite",
                UsedTimes = usedTimes
            }; // 创建按钮数据对象
            
            // 更新数据库
            _buttonDb.UpdateAction(buttonData, _addWindow.TableName);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 解析网站地址和浏览器地址
        /// </summary>
        /// <param name="info">地址和浏览器地址</param>
        /// <returns>网站地址和浏览器地址数组</returns>
        private string[] GetLocationAndBrowser(string info)
        {
            return info.Split(';');
        }

        // 清理资源
        private void CleanupResources()
        {
            // 释放资源
            _iconManager.Dispose();
            
            // 清空引用
            _addWindow = null;
            
            // 清理UI元素
            OpenWebsitePopup.IsOpen = false;
            OpenWebsitePopup.Child = null;
            OpenWebsitePopup = null;
            LocationTextBox = null;
            GetWebsiteIconButton = null;
        }

        #endregion
    }
}