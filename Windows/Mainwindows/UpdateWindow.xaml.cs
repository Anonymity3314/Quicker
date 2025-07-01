using Quicker.Windows.ToolWindows;
using static AppUpdateManager;
using Quicker.Windows.Menus;
using System.Windows.Media;
using System.Windows.Forms;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;
using System.IO;

namespace Quicker.Windows.MainWindows
{
    /// <summary>
    /// UpdateWindow 窗口用于检查和下载软件更新
    /// </summary>
    public partial class UpdateWindow : Window
    {
        #region 字段

        private const string _releaseUrl = "https://github.com/Anonymity3314/Quicker/releases"; // 发布页面地址
        private const string _downLoadPath = "C:\\Users\\LENOVO\\Downloads"; // 下载路径
        private string _downloadUrlWithNet; // 下载地址
        private string _downloadUrl; // 下载地址

        #endregion

        #region 构造函数

        public UpdateWindow()
        {
            InitializeComponent();
            CheckNewVersion(); // 检查新版本
            Activate(); // 激活窗口
        }

        #endregion

        #region 事件处理

        // 窗口加载时获取更新信息
        private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VersionTextBlock.Text = $"当前版本：{SettingDatabase.currentVersion}"; // 显示当前版本号
        }

        // 点击按钮关闭窗口
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // 关闭窗口
        }

        // 点击按钮下载
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            SelectDownloadLocation();
        }

        // 前往下载地址查看详细信息
        private void LatestVersionTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenReleaseWebPage();
        }

        // 窗口关闭清理资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类方法
            CleanupResources();
        }

        #endregion

        #region 业务逻辑

        // 检查新版本
        private void CheckNewVersion()
        {
            if (AppStateManager.HasNewVersion)
            {
                LoadLatestVersionInfo();
            }
            else
            {
                DisplayNoUpdateAvailable();
            }
        }

        // 加载最新版本信息
        private void LoadLatestVersionInfo()
        {
            using var updateManager = new AppUpdateManager(); // 创建更新管理器
            updateManager.ReadJsonFromUrl(); // 读取更新信息
            
            if (updateManager.LatestUpdateInfo != null)
            {
                LoadUpdateInfo(updateManager.LatestUpdateInfo); // 加载更新信息
            }
            else
            {
                ShowUpdateFailedMessage();
            }
        }

        // 显示更新失败消息
        private void ShowUpdateFailedMessage()
        {
            using var toast = new ToastManager(); // 创建Toast提示
            toast.Show("获取更新失败！", "Common"); // 显示Toast提示
        }

        // 显示无更新可用信息
        private void DisplayNoUpdateAvailable()
        {
            // 更新UI显示
            TitleTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D4D")); // 设置标题颜色
            LatestVersionTextBlock1.Text = SettingDatabase.currentVersion; // 显示当前版本号
            LatestVersionTextBlock1.FontWeight = FontWeights.Normal; // 设置字体粗细
            DownloadButton.Visibility = Visibility.Collapsed; // 隐藏下载按钮
            DownloadWithFrameButton.Visibility = Visibility.Collapsed; // 隐藏下载按钮
            TitleTextBlock.Text = "暂无新版本。"; // 更新标题文本
            LineRectangle.Width = 350; // 调整分界线宽度
            
            // 调整窗口大小和位置
            Height = 266; // 设置窗口高度
            Width = 400; // 设置窗口宽度
            LocateCenter(); // 窗口居中
        }

        /// <summary>
        /// 加载更新信息
        /// </summary>
        /// <param name="updateInfo">更新信息</param>
        private void LoadUpdateInfo(UpdateInfo updateInfo)
        {
            _downloadUrl = updateInfo.DownloadUrl; // 保存下载地址
            _downloadUrlWithNet = updateInfo.DownloadUrlWithNet; // 保存下载地址
            string newVersion = updateInfo.NewVersion; // 获取最新版本号
            
            // 计算更新内容的行数并调整UI
            int changelogLineCount = updateInfo.Changelog.Count(c => c == '~');
            
            // 更新UI显示
            LatestVersionTextBlock1.Text = newVersion; // 显示最新版本号
            LatestVersionTextBlock2.Text = newVersion; // 显示最新版本号
            VersionChangeTextBlock.Text = $"{SettingDatabase.currentVersion} -- {newVersion}"; // 显示版本号变更
            UpdateDateTextBlock.Text = updateInfo.ReleaseDate; // 显示更新日期
            UpdateInfoTextBlock.Text = updateInfo.Changelog; // 显示更新内容
            
            // 调整UI元素高度
            UpdateInfoGrid.Height += changelogLineCount * 18; // 设置更新内容的高度
            UpdateInfoBorder.Height += changelogLineCount * 18; // 设置更新内容的高度
        }

        // 下载不内置.NET框架的版本
        private void SelectDownloadLocation()
        {
            var fileName = Path.GetFileName(new Uri(_downloadUrl).AbsolutePath); // 获取文件名
            string destinationPath = Path.Combine(_downLoadPath, fileName); // 构建保存路径

            var downloadWindow = DownloadWindow.Create(_downloadUrl, destinationPath); // 创建下载窗口
            downloadWindow.Show(); // 显示下载窗口
        }

        // 下载内置.NET框架的版本
        private void DownloadWithFrameButton_Click(object sender, RoutedEventArgs e)
        {
            var fileName = Path.GetFileName(new Uri(_downloadUrlWithNet).AbsolutePath); // 获取文件名
            string destinationPath = Path.Combine(_downLoadPath, fileName); // 构建保存路径

            var downloadWindow = DownloadWindow.Create(_downloadUrlWithNet, destinationPath); // 创建下载窗口
            downloadWindow.Show(); // 显示下载窗口
        }

        // 打开发布页面
        private void OpenReleaseWebPage()
        {
            using var actionManager = new ActionManager(); // 创建动作管理器
            actionManager.LaunchDefaultBrowser(_releaseUrl); // 打开下载地址
        }

        #endregion

        #region 辅助方法

        // 窗口居中
        private void LocateCenter()
        {
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight; // 获取屏幕高度
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth; // 获取屏幕宽度
            double windowHeight = Height; // 获取窗口高度
            double windowWidth = Width; // 获取窗口宽度
            
            Left = (screenWidth / 2) - (windowWidth / 2); // 计算水平居中位置
            Top = (screenHeight / 2) - (windowHeight / 2); // 计算垂直居中位置
        }

        // 清理资源
        private void CleanupResources()
        {
            // 清理字段
            _downloadUrl = null;

            // 清理UI元素引用
            VersionTextBlock.Text = null;
            LatestVersionTextBlock1.Text = null;
            LatestVersionTextBlock2.Text = null;
            VersionChangeTextBlock.Text = null;
            UpdateDateTextBlock.Text = null;
            UpdateInfoTextBlock.Text = null;

            // 清理数据绑定
            Content = null;
            DataContext = null;

            // 强制垃圾回收
            GC.Collect();
        }

        #endregion
    }
}