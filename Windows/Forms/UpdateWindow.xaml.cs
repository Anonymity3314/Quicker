using Microsoft.Win32;
using Quicker.Managers;
using Quicker.Windows.Menus;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using static AppUpdateManager;

namespace Quicker.Windows.Forms
{
    public partial class UpdateWindow : Window
    {
        private string downloadUrl; // 下载地址

        public UpdateWindow()
        {
            InitializeComponent();
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
            using var folderDialog = new FolderBrowserDialog(); // 创建文件夹对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var toast = new ToastManager(); // 创建Toast提示
                var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath); // 获取文件名
                string destinationPath = Path.Combine(folderDialog.SelectedPath, fileName); // 保存路径
                DownloadWindow.GetInstance(downloadUrl, destinationPath); // 创建下载窗口
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
            //if (AppStateManager.HasNewVersion)
            //{
                using var updateManager = new AppUpdateManager(); // 创建更新管理器
                using var toast = new ToastManager(); // 创建Toast提示
                if (updateManager.LatestUpdateInfo != null)
                    LoadUpdateInfo(updateManager.LatestUpdateInfo); // 加载更新信息
                else
                    toast.ShowToast("获取更新失败！", "Common"); // 显示Toast提示
            //}
        }

        /// <summary>
        /// 加载更新信息
        /// </summary>
        /// <param name="updateInfo"> 更新信息 </param>
        private void LoadUpdateInfo(UpdateInfo updateInfo)
        {
            downloadUrl = updateInfo.DownloadUrl; // 获取下载地址
            string currentVersion = SettingDatabase.currentVersion; // 当前版本号
            string newVersion = updateInfo.NewVersion; // 最新版本号
            int count = updateInfo.Changelog.Count(c => c == '~'); // 获取更新内容的行数
            LatestVersionTextBlock1.Text = newVersion; // 显示最新版本号
            LatestVersionTextBlock2.Text = newVersion; // 显示最新版本号
            VersionTextBlock.Text = $"当前版本：{currentVersion}"; // 显示当前版本号
            VersionChangeTextBlock.Text = $"{currentVersion} -- {newVersion}"; // 显示版本号变更
            UpdateDateTextBlock.Text = updateInfo.ReleaseDate; // 显示更新日期
            UpdateInfoTextBlock.Text = updateInfo.Changelog; // 显示更新内容
            UpdateInfoGrid.Height += count * 20; // 设置更新内容的高度
            UpdateInfoBorder.Height += count * 20; // 设置更新内容的高度
        }
    }
}