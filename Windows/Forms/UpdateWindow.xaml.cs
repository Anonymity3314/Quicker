using System.Windows.Forms;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Forms
{
    public partial class UpdateWindow : Window
    {
        private string destinationPath; // 下载地址

        public UpdateWindow()
        {
            InitializeComponent();
        }

        // 点击按钮关闭窗口
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 点击按钮下载
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                destinationPath = folderDialog.SelectedPath;
        }

        // 前往下载地址查看详细信息
        private void LatestVersionTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        // 获取更新信息
        private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!AppStateManager.HasNewVersion) 
            //{
            //    if (updateInfo != null)
            //    {
            //        using var toast = new ToastManager(); // 创建Toast提示
            //        toast.ShowToast($"版本: {updateInfo.NewVersion}", "Common"); // 显示Toast提示
            //        toast.ShowToast($"下载地址: {updateInfo.DownloadUrl}", "Common"); // 显示Toast提示
            //        toast.ShowToast($"更新日志: {updateInfo.Changelog}", "Common"); // 显示Toast提示
            //        toast.ShowToast($"更新日期: {updateInfo.ReleaseDate}", "Common"); // 显示Toast提示
            //    }
            //    else
            //    {
            //        using var toast = new ToastManager(); // 创建Toast提示
            //        toast.ShowToast("获取更新失败！", "Common"); // 显示Toast提示
            //    }
            //}
        }
    }
}