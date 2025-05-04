using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Diagnostics;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.Net;
using System.IO;

namespace Quicker.UserControls.AddWindow
{
    public partial class OpenWebsite : UserControl
    {
        private readonly IconManager iconManager = new IconManager(); // 图标管理器接口
        private ButtonDatabase db2 = new ButtonDatabase(); // 初始换按钮数据库
        private Quicker.AddWindow AddWindow; // AddWindow 的静态引用

        public OpenWebsite(Quicker.AddWindow addWindow)
        {
            AddWindow = addWindow; // 保存 AddWindow 的引用
            InitializeComponent();
            ExecuteChoiceAction();
        }

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            if (AddWindow.Choice == 0) // 打开文件
                LoadButtonInformation(); // 编辑动作，加载动作信息
        }

        // 编辑动作 加载动作信息
        private void LoadButtonInformation()
        {
            ButtonData buttonData = db2.GetButtonDataByID(AddWindow.CurrentButton); // 获取按钮数据
            if (!string.IsNullOrWhiteSpace(buttonData.Title))
            {
                AddWindow.ButtonTitle.Visibility = Visibility.Visible; // 显示按钮名称
                AddWindow.ButtonTitle.Text = buttonData.Title; // 显示按钮名称
            } // 如果按钮名称不为空
            AddWindow.TitleTextBox.Text = buttonData.Title; // 设置按钮名称
            if (buttonData.WindowState == 8) // 如果是自定义浏览器
            {
                string[] locationAndBrowser = GetLocationAndBrowser(buttonData.Location); // 解析地址和浏览器
                LocationTextBox.Text = locationAndBrowser[0]; // 设置地址栏文本
                BrowserLocation.Text = locationAndBrowser[1]; // 设置浏览器地址
            }
            else
                LocationTextBox.Text = buttonData.Location; // 设置文件地址

            if (buttonData.ImagePath != "none")
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
            BrowserComboBox.SelectedIndex = buttonData.WindowState; // 设置浏览器类型
            AddWindow.DescriptionTextBox.Text = buttonData.Description; // 设置用途
            AddWindow.UpdateTooltip(); // 更新提示文本
        }

        /// <summary>
        /// 解析网站地址和浏览器地址
        /// </summary>
        /// <param name="info"> 地址和浏览器地址 </param>
        /// <returns> 网站地址和浏览器地址数组 </returns>
        private string[] GetLocationAndBrowser(string info)
        {
            string[] locationAndBrowser = info.Split(';'); // 将文本内容按照分号分隔
            return locationAndBrowser;
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

        // 点击按钮获取网站图标
        private void GetWebsiteIconButton_Click(object sender, RoutedEventArgs e)
        {
            GetWebsiteIcon(LocationTextBox.Text); // 获取网站图标
            AddWindow.TitleTextBox.Text = GetWebsiteNameFromUrl(LocationTextBox.Text); // 获取网站名称
        }

        // 获取网站名称
        private string GetWebsiteNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch { Debug.WriteLine("无效的URI：未能解析主机名。"); }
            return null;
        }

        // 获取网站图标
        private void GetWebsiteIcon(string websiteUrl)
        {
            Uri uri; // 提取域名部分
            try
            {
                uri = new Uri(websiteUrl);
            }
            catch
            {
                Debug.WriteLine("无效的URI：未能解析主机名。"); // 处理无效的URL
                return;
            }

            string apiFaviconUrl = $"https://icon.bqb.cool?url={uri.Host}"; // 拼接第三方API的URL
            using (WebClient client = new WebClient()) // 创建一个WebClient对象来下载图标
            {
                try
                {
                    byte[] iconData = client.DownloadData(apiFaviconUrl); // 下载图标的字节数组
                    BitmapImage bitmapImage = new BitmapImage(); // 将字节数组转换为BitmapImage
                    using (MemoryStream stream = new MemoryStream(iconData))
                    {
                        bitmapImage.BeginInit(); // 开始初始化BitmapImage
                        stream.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                        bitmapImage.StreamSource = stream; // 设置流为BitmapImage的源
                        bitmapImage.EndInit(); // 结束初始化BitmapImage
                    }
                    AddWindow.ButtonImage.Source = bitmapImage; // 使用BitmapImage作为图像源
                    AddWindow.ButtonImage.Visibility = Visibility.Visible; // 显示按钮的图像
                }
                catch{ Debug.WriteLine("获取站点信息失败"); }
            }
        }

        // 保存动作
        public void Save()
        {
            AddWindow.iconPath = AddWindow.ButtonImage.Visibility == Visibility.Visible
                ? iconManager.SaveIconToFile(AddWindow.ButtonImage.Source)
                : "none"; // 如果图标可见，则保存图标，否则设置为默认值

            var buttonData = new ButtonData
            {
                ButtonID = AddWindow.CurrentButton,
                Title = AddWindow.TitleTextBox.Text,
                Location = BrowserComboBox.SelectedIndex == 8
                    ? LocationTextBox.Text + ";" + BrowserLocation.Text
                    : LocationTextBox.Text,
                ImagePath = AddWindow.iconPath,
                WindowState = BrowserComboBox.SelectedIndex,
                Description = AddWindow.DescriptionTextBox.Text,
                CreateTime = DateTime.Now,
                LatestEditTime = DateTime.Now,
                ActionType = "OpenWebsite"
            }; // 创建按钮数据对象
            (AddWindow.Choice != 0 ? (Action<ButtonData>)db2.AddAction : db2.UpdateAction)(buttonData); // 添加或更新动作
        }

        // 如果是自定义浏览器，显示相关控件进行相关设置
        private void BrowserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserControlGrid == null) return;
            if (BrowserComboBox.SelectedIndex == 8)
                UserControlGrid.Visibility = Visibility.Visible; // 如果是自定义浏览器，则显示相关控件
            else
                UserControlGrid.Visibility = Visibility.Collapsed; // 否则隐藏相关控件
        }

        // 关闭控件释放资源
        private void OpenWebsite_Unloaded(object sender, RoutedEventArgs e)
        {
            iconManager.Dispose(); // 释放图标管理器资源
            AddWindow = null; // 清空静态引用
            OpenWebsitePopup.IsOpen = false; // 关闭弹出菜单
            OpenWebsitePopup.Child = null; // 清空弹出菜单内容
            OpenWebsitePopup.IsOpen = false; // 关闭弹出菜单
            OpenWebsitePopup = null; // 清空弹出菜单对象
            LocationTextBox = null; // 清空地址栏对象
            GetWebsiteIconButton = null; // 清空获取图标按钮对象
        }
    }
}