using Microsoft.Win32;
using Quicker.Managers;
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
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var toast = new ToastManager();
                Dispatcher.Invoke(() => DownloadButton.IsEnabled = false);

                // 安全协议配置（必须在WebClient实例化前设置）
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls13 |
                    (SecurityProtocolType)12288; // .NET 4.0+的Tls13兼容写法

                // 智能证书验证
                ServicePointManager.ServerCertificateValidationCallback = (senderCert, cert, chain, errors) =>
                {
                    // 允许特定域名的自签名证书
                    if (cert.Issuer.Contains("Internal CA") && senderCert is HttpWebRequest req && req.Address.Host.EndsWith(".yourdomain.com"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };

                var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
                string destinationPath = Path.Combine(folderDialog.SelectedPath, fileName);

                var webClient = new WebClient();

                // 完整请求头配置
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
                webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                webClient.Headers.Add("Cache-Control", "no-cache");

                // 下载进度跟踪（调试用）
                webClient.DownloadProgressChanged += (s, args) =>
                {
                    Debug.WriteLine($"已下载 {args.BytesReceived} bytes / 总计 {args.TotalBytesToReceive} bytes");
                };

                // 下载完成回调
                webClient.DownloadFileCompleted += (s, args) =>
                {
                    try
                    {
                        Dispatcher.Invoke(() => DownloadButton.IsEnabled = true);

                        if (args.Error != null)
                        {
                            var errorBuilder = new StringBuilder($"下载失败：{args.Error.Message}");

                            // 获取服务器响应状态
                            if (webClient.ResponseHeaders != null)
                            {
                                errorBuilder.AppendLine($"\nHTTP状态码：{webClient.ResponseHeaders["Status"]}");
                                errorBuilder.AppendLine($"内容类型：{webClient.ResponseHeaders["Content-Type"]}");
                            }

                            toast.ShowToast(errorBuilder.ToString(), "Error");
                        }
                        else
                        {
                            toast.ShowToast($"{fileName} 下载完成", "Success");

                            // 验证文件完整性
                            var fileInfo = new FileInfo(destinationPath);
                            if (fileInfo.Length == 0)
                            {
                                toast.ShowToast("警告：下载文件大小为0字节", "Warning");
                            }
                        }
                    }
                    finally
                    {
                        (s as IDisposable)?.Dispose(); // 安全释放资源
                    }
                };

                try
                {
                    // 异步下载
                    webClient.DownloadFileAsync(new Uri(downloadUrl), destinationPath);

                }
                catch (WebException ex)
                {
                    // 处理特定错误代码
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        toast.ShowToast($"服务器返回错误：{(int)response.StatusCode} {response.StatusDescription}", "Error");
                    }
                    else
                    {
                        toast.ShowToast($"网络错误：{ex.Message}", "Error");
                    }
                    Dispatcher.Invoke(() => DownloadButton.IsEnabled = true);
                }
                catch (Exception ex)
                {
                    toast.ShowToast($"系统错误：{ex.Message}", "Error");
                    Dispatcher.Invoke(() => DownloadButton.IsEnabled = true);
                }
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
            UpdateInfoGrid.Height += count * 21; // 设置更新内容的高度
            UpdateInfoBorder.Height += count * 21; // 设置更新内容的高度
        }
    }
}