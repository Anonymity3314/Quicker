using static AppUpdateManager;
using Quicker.Windows.Menus;
using System.Windows.Media;
using System.Windows.Forms;
using Quicker.Managers;
using System.Windows;
using System.IO;

namespace Quicker.Windows.Forms
{
    public partial class UpdateWindow : Window
    {
        private string downloadUrl; // 下载地址

        public UpdateWindow()
        {
            InitializeComponent();
            CheckNewVersion(); // 检查新版本
            this.Activate(); // 激活窗口
        }

        // 点击按钮关闭窗口
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 点击按钮下载
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog() { Description = "选择下载路径" }; // 创建文件夹对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var toast = new ToastManager(); // 创建Toast提示
                var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath); // 获取文件名
                string destinationPath = Path.Combine(folderDialog.SelectedPath, fileName); // 保存路径
                var downloadWindow = DownloadWindow.GetInstance(downloadUrl, destinationPath); // 创建下载窗口
                downloadWindow.Show(); // 确保调用 Show 方法显示窗口
            }
        }

        // 前往下载地址查看详细信息
        private void LatestVersionTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建动作管理器
            actionManager.LaunchDefaultBrowser("https://github.com/Anonymity3314/Quicker/releases"); // 打开下载地址
        }

        // 获取更新信息
        private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VersionTextBlock.Text = $"当前版本：{SettingDatabase.currentVersion}"; // 显示当前版本号
        }

        // 检查新版本
        private void CheckNewVersion()
        {
            if (AppStateManager.HasNewVersion)
            {
                using var updateManager = new AppUpdateManager(); // 创建更新管理器
                updateManager.ReadJsonFromUrl(); // 读取更新信息
                if (updateManager.LatestUpdateInfo != null)
                    LoadUpdateInfo(updateManager.LatestUpdateInfo); // 加载更新信息
                else
                {
                    using var toast = new ToastManager(); // 创建Toast提示
                    toast.ShowToast("获取更新失败！", "Common"); // 显示Toast提示
                }
            }
            else
            {
                TitleTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D4D")); // 显示标题颜色
                LatestVersionTextBlock1.Text = SettingDatabase.currentVersion; // 显示当前版本号
                LatestVersionTextBlock1.FontWeight = FontWeights.Normal; // 显示最新版本号
                DownloadButton.Visibility = Visibility.Collapsed; // 隐藏下载按钮
                TitleTextBlock.Text = "暂无新版本。"; // 显示标题
                LineRectangle.Width = 350; // 调整分界线宽度
                this.LocateCenter(); // 窗口居中
                this.Height = 266; // 隐藏更新信息
                this.Width = 400; // 调整窗口大小
            }
        }

        // 窗口居中
        private void LocateCenter()
        {
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight; // 获取屏幕高度
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth; // 获取屏幕宽度
            double windowHeight = this.Height; // 获取窗口高度
            double windowWidth = this.Width; // 获取窗口宽度
            this.Left = (screenWidth / 2) - (windowWidth / 2); // 窗口居中
            this.Top = (screenHeight / 2) - (windowHeight / 2); // 窗口居中
        }

        /// <summary>
        /// 加载更新信息
        /// </summary>
        /// <param name="updateInfo"> 更新信息 </param>
        private void LoadUpdateInfo(UpdateInfo updateInfo)
        {
            downloadUrl = updateInfo.DownloadUrl; // 获取下载地址
            string newVersion = updateInfo.NewVersion; // 最新版本号
            int count = updateInfo.Changelog.Count(c => c == '~'); // 获取更新内容的行数
            LatestVersionTextBlock1.Text = newVersion; // 显示最新版本号
            LatestVersionTextBlock2.Text = newVersion; // 显示最新版本号
            VersionChangeTextBlock.Text = $"{SettingDatabase.currentVersion} -- {newVersion}"; // 显示版本号变更
            UpdateDateTextBlock.Text = updateInfo.ReleaseDate; // 显示更新日期
            UpdateInfoTextBlock.Text = updateInfo.Changelog; // 显示更新内容
            UpdateInfoGrid.Height += count * 18; // 设置更新内容的高度
            UpdateInfoBorder.Height += count * 18; // 设置更新内容的高度
        }

        // 窗口关闭清理资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类方法

            downloadUrl = null; // 清理下载地址

            VersionTextBlock.Text = null; // 清理版本号
            LatestVersionTextBlock1.Text = null; // 清理最新版本号
            LatestVersionTextBlock2.Text = null; // 清理最新版本号
            VersionChangeTextBlock.Text = null; // 清理版本号变更
            UpdateDateTextBlock.Text = null; // 清理更新日期
            UpdateInfoTextBlock.Text = null; // 清理更新内容

            this.Content = null;
            this.DataContext = null; // 清理数据绑定

            GC.Collect(); // 回收资源
        }
    }
}