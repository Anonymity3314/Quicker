using Quicker.Managers;
using Quicker.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Quicker.Windows.Menus
{
    public partial class DownloadWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;
        private string _downloadUrl, _downloadPath; // 下载地址和保存路径
        private static DownloadWindow _instance; // 静态实例变量
        private bool _isClosed; // 标记窗口是否已经关闭
        private long _totalBytes; // 总字节数
        private long _downloadedBytes; // 已下载字节数
        private double _downloadSpeed; // 下载速度 (字节/秒)
        private DateTime _lastSpeedUpdate; // 上次速度更新时间
        private DispatcherTimer _updateTimer; // 更新UI的定时器

        private DownloadWindow(string downloadUrl, string downloadPath)
        {
            InitializeComponent();
            _downloadUrl = downloadUrl;
            _downloadPath = downloadPath;
            _isClosed = false;
            this.Closed += (sender, e) => { _isClosed = true; }; // 窗口关闭时标记为已关闭
            InitializeUpdateTimer(); // 初始化定时器
        }

        // 获取窗口实例的静态方法
        public static DownloadWindow GetInstance(string downloadUrl, string downloadPath)
        {
            if (_instance == null || _instance._isClosed)
            {
                _instance = new DownloadWindow(downloadUrl, downloadPath);
            }
            else
            {
                var toast = new ToastManager(); // 创建Toast提示
                toast.ShowToast("下载中，请勿重复操作", "Common"); // 显示Toast提示
                _instance.EnableDownloadButton(false); // 通过实例禁用下载按钮
            }
            return _instance;
        }

        private void InitializeUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(200); // 每200毫秒更新一次
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_totalBytes > 0)
            {
                double progress = (double)_downloadedBytes / _totalBytes * 100; // 计算进度百分比
                DownloadProgressBar.Value = progress; // 更新进度条
                DownloadSpeed.Text = $"{_downloadSpeed:F2} KB/s"; // 更新下载速度
                DownloadSize.Text = $"{_downloadedBytes / (1024.0 * 1024.0):F2} MB/{_totalBytes / (1024.0 * 1024.0):F2} MB";
            }
        }

        // 窗口加载完成后设置窗口位置
        private void DownloadWindow_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.WorkArea.Width; // 屏幕宽度
            double screenHeight = SystemParameters.WorkArea.Height; // 屏幕高度
            this.Left = screenWidth - 300; // 窗口距离屏幕右侧300像素
            this.Top = screenHeight - 93.8; // 窗口距离屏幕下侧93.8像素
            LoadDownloadInfo(); // 加载下载信息
            StartDownload(); // 开始下载
        }

        // 开始下载
        private async void StartDownload()
        {
            await StartDownloadAsync(); // 异步开始下载
        }

        // 鼠标进入窗口还原透明度
        private void DownloadWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Opacity = 1; // 还原透明度
        }

        // 鼠标离开窗口降低透明度
        private void DownloadWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Opacity = 0.5; // 设置透明度为0.5
        }

        private void LoadDownloadInfo()
        {
            DownloadFileName.Text = Path.GetFileName(new Uri(_downloadUrl).AbsolutePath); // 获取文件名
        }

        // 异步开始下载
        private async Task StartDownloadAsync()
        {
            try
            {
                EnableDownloadButton(false); // 禁用下载按钮
                var toast = new ToastManager(); // 创建Toast提示
                var fileName = Path.GetFileName(new Uri(_downloadUrl).AbsolutePath); // 获取文件名

                _cancellationTokenSource = new CancellationTokenSource(); // 创建取消令牌源
                var cancellationToken = _cancellationTokenSource.Token;

                using (var httpClient = new HttpClient()) // 创建HttpClient对象，自动处理释放
                {
                    var response = await httpClient.GetAsync(new Uri(_downloadUrl));
                    _totalBytes = response.Content.Headers.ContentLength ?? 0;

                    // 获取文件流
                    using (var contentStream = await httpClient.GetStreamAsync(new Uri(_downloadUrl)))
                    {
                        // 检查文件流是否为空
                        if (contentStream == null || contentStream.Length == 0)
                        {
                            toast.ShowToast($"文件流为空", "Error");
                            EnableDownloadButton(true); // 启用下载按钮
                            return;
                        }

                        // 创建文件保存路径
                        var directory = Path.GetDirectoryName(_downloadPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // 保存文件
                        using (var fileStream = new FileStream(_downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[8192]; // 缓冲区大小
                            int bytesRead;
                            _lastSpeedUpdate = DateTime.Now;
                            _downloadedBytes = 0;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    toast.ShowToast("下载已取消", "Info");
                                    return;
                                }

                                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                _downloadedBytes += bytesRead;

                                // 计算下载速度
                                var currentTime = DateTime.Now;
                                var timeDiff = (currentTime - _lastSpeedUpdate).TotalSeconds;
                                if (timeDiff >= 1)
                                {
                                    _downloadSpeed = _downloadedBytes / timeDiff;
                                    _lastSpeedUpdate = currentTime;
                                }
                            }
                        }
                    }
                }

                toast.ShowToast($"{fileName} 下载完成", "Success"); // 显示Toast提示
                EnableDownloadButton(true); // 启用下载按钮

                // 验证文件完整性
                var downloadedFileInfo = new FileInfo(_downloadPath); // 获取文件信息
                if (downloadedFileInfo.Length == 0)
                {
                    toast.ShowToast("警告：下载文件大小为0字节", "Warning");
                }
            }
            catch (OperationCanceledException)
            {
                // 捕获取消异常
                var toast = new ToastManager(); // 创建Toast提示
                toast.ShowToast("下载已取消", "Info");
            }
            catch (WebException ex) // 处理特定错误
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse; // 获取服务器响应
                    var toast = new ToastManager();
                    toast.ShowToast($"服务器返回错误：{(int)response.StatusCode} {response.StatusDescription}", "Error"); // 显示Toast提示
                }
                else
                {
                    var toast = new ToastManager();
                    toast.ShowToast($"网络错误：{ex.Message}", "Error"); // 显示Toast提示
                }
                EnableDownloadButton(true); // 启用下载按钮
            }
            catch (Exception ex)
            {
                var toast = new ToastManager();
                toast.ShowToast($"系统错误：{ex.Message}", "Error"); // 显示Toast提示
                EnableDownloadButton(true); // 启用下载按钮
            }
        }

        /// <summary>
        /// 启用或禁用下载按钮
        /// </summary>
        /// <param name="enable"> 是否启用 </param>
        private void EnableDownloadButton(bool enable)
        {
            var updateWindows = Application.Current.Windows.OfType<UpdateWindow>(); // 获取所有UpdateWindow
            if (updateWindows.Count() > 0) // 如果有UpdateWindow，则禁用下载按钮
                foreach (var window in updateWindows)
                    window.DownloadButton.IsEnabled = enable;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel(); // 取消下载
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnClosed(EventArgs e)
        {
            _downloadPath = null; // 释放下载路径
            _downloadUrl = null; // 释放下载地址
            _instance = null; // 释放静态实例变量
            _isClosed = true; // 标记窗口已关闭

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose(); // 清理取消令牌源
            }

            base.OnClosed(e);
        }
    }
}