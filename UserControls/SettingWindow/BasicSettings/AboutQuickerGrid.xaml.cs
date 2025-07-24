using Quicker.Windows.MainWindows;
using System.Windows.Resources;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using System.Text.Json;
using Quicker.Helpers;
using System.Windows;
using System.Text;
using System.IO;

namespace Quicker.UserControls.SettingWindow.BasicSettings
{
    public partial class AboutQuickerGrid : UserControl
    {
        private const string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\TempData"; // 文件夹路径
        private WeakReference<Quicker.Windows.MainWindows.SettingWindow> weakSettingWindow; // 弱引用设置窗口
        SettingManager settingManager; // 读取设置的管理器

        public AboutQuickerGrid(Quicker.Windows.MainWindows.SettingWindow settingWindow)
        {
            InitializeComponent();
            settingManager = settingWindow._settingManager; // 创建设置管理器
            weakSettingWindow = new(settingWindow); // 保存设置窗口
            settingManager.LoadConventionsAsync(); // 初始化缓存数据
            VersionLabel.Content = $"版本：{settingManager.conventions.Version}"; // 加载版本信息
            GetTempDataSize(); // 获取临时数据大小
        }

        // 基础设置-关于Quicker-关于Quicker
        private void AboutQuickerButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(AboutQuickerButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(AboutQuickerButton, MainGrid); // 保存Button类型3边框设置
        }

        // 打开更新历史文件（从资源型JSON反序列化）
        private void OpenUpdateHistory(object sender, MouseButtonEventArgs e)
        {
            string resourceName = "VersionInfo.json"; // 资源文件名
            Uri resourceUri = new(resourceName, UriKind.Relative); // 资源URI
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri); // 通过资源URI获取资源流
            using (StreamReader reader = new(streamInfo.Stream)) // 读取资源流内容
            {
                string json = reader.ReadToEnd(); // 读取JSON字符串
                var versionInfo = JsonSerializer.Deserialize<VersionInfoRoot>(json); // 反序列化为对象
                StringBuilder sb = new();
                foreach (var v in versionInfo.Versions) // 遍历所有版本信息，拼接为文本
                {
                    sb.AppendLine($"{v.Version}\t{v.ReleaseDate}"); // 版本号和发布日期
                    var changelogs = v.Changelog.Split('\n'); // 按行分割变更日志
                    foreach (var log in changelogs)
                    {
                        sb.AppendLine(log.Trim('~', '.', '\r')); // 去除前缀符号后写入
                    }
                    sb.AppendLine(); // 每个版本之间空一行
                }

                // 写入临时文件并用记事本打开
                string tempPath = Path.GetTempPath(); // 获取临时文件夹路径
                string tempFilePath = Path.Combine(tempPath, "更新历史.txt"); // 构造临时文件路径
                File.WriteAllText(tempFilePath, sb.ToString()); // 写入临时文件
                System.Diagnostics.Process.Start("notepad.exe", tempFilePath); // 用记事本打开临时文件
            }
        }

        /// <summary>
        /// 在默认浏览器中打开URL
        /// </summary>
        /// <param name="url"> URL </param>
        private void OpenUrlInDefaultBrowser(string url)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser(url); // 在默认浏览器中打开URL
        }

        // 前往图标网站www.iconfont.cn
        private void www_iconfont_cn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUrlInDefaultBrowser("https://www.iconfont.cn"); // 在默认浏览器中打开URL
        }

        // 前往图标网站icons8.com
        private void icons8_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUrlInDefaultBrowser("https://icons8.com/"); // 在默认浏览器中打开URL
        }

        // 前往图标网站fontawesome.com
        private void fontawesome_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUrlInDefaultBrowser("https://fontawesome.com/"); // 在默认浏览器中打开URL
        }

        // 前往icon11社区图标库
        private void icon11_community_github_io_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUrlInDefaultBrowser("https://icon11-community.github.io/icons/"); // 在默认浏览器中打开URL
        }

        // BUG反馈、需求
        private void FeedBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenUrlInDefaultBrowser("https://github.com/LJZ-Anonymity/Quicker/issues"); // 在默认浏览器中打开URL
        }

        // 基础设置-关于Quicker-隐私声明
        public void Privacy_StatementButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(Privacy_StatementButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(Privacy_StatementButton, MainGrid); // 保存Button类型3边框设置
        }

        /// <summary>
        /// 打开指定文件夹
        /// </summary>
        /// <param name="folderPath"> 文件夹路径 </param>
        private void OpenFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
        }

        // 前往程序数据根目录
        private void RootFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFolder(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\");
        }

        // 前往程序数据库目录
        private void DatabaseFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFolder(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Database\");
        }

        // 前往程序图标目录
        private void LocalIconsFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFolder(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\");
        }

        // 备份数据
        private void BackupDataButton_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog() { Description = "选择备份路径", UseDescriptionForTitle = true };
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderDialog.SelectedPath;
                string sourceFolderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker";
                string backupFolderPath = Path.Combine(folderPath, "QuickerData");
                if (!Directory.Exists(backupFolderPath)) Directory.CreateDirectory(backupFolderPath);
                CopyFolder(sourceFolderPath, backupFolderPath);
                using var toast = new ToastManager();
                toast.Show("操作完成！", "Success");
                OpenFolder(folderPath);
            }
        }

        /// <summary>
        /// 复制文件夹内容到目标路径
        /// </summary>
        /// <param name="sourceFolder"> 源文件夹路径 </param>
        /// <param name="targetFolder"> 目标文件夹路径 </param>
        private void CopyFolder(string sourceFolder, string targetFolder)
        {
            try
            {
                Directory.CreateDirectory(targetFolder); // 创建目标文件夹
                string[] files = Directory.GetFiles(sourceFolder); // 获取源文件夹中的所有文件
                foreach (string file in files) // 遍历并复制每个文件
                {
                    string name = Path.GetFileName(file); // 获取文件名
                    string dest = Path.Combine(targetFolder, name); // 构造目标文件路径
                    File.Copy(file, dest, true); // 复制文件并覆盖已存在的文件
                }

                string[] folders = Directory.GetDirectories(sourceFolder); // 获取源文件夹中的所有子文件夹
                foreach (string folder in folders) // 遍历并递归复制每个子文件夹
                {
                    string name = Path.GetFileName(folder); // 获取子文件夹名
                    string dest = Path.Combine(targetFolder, name); // 构造目标文件夹路径
                    CopyFolder(folder, dest); // 递归复制子文件夹
                }
            }
            catch
            {
                using var toast = new ToastManager(); // 创建 ToastManager 的实例
                toast.Show("备份失败！", "Error"); // 显示提示
            }
        }

        // 获取临时数据大小
        private async void GetTempDataSize()
        {
            TempSize.Text = await GetTempDataSizeAsync();
        }

        // 获取临时数据大小（异步）
        public async Task<string> GetTempDataSizeAsync()
        {
            if (Directory.Exists(folderPath)) // 检查文件夹是否存在
            {
                try
                {
                    long folderSize = 0;
                    string folderSizeString = "B"; // 默认单位为字节
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath); // 创建 DirectoryInfo 对象
                    var files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories); // 获取所有文件（包括子文件夹）                 
                    foreach (var file in files) // 遍历所有文件并累加大小
                    {
                        folderSize += file.Length;
                    }

                    // 使用 DataConversionManager 转换数据大小和单位
                    using var convertionManager = new DataSizeHelper();
                    folderSize = convertionManager.ConversionData((int)folderSize); // 转换数据大小
                    folderSizeString = convertionManager.ConversionUnits((int)folderSize); // 转换单位

                    if (folderSize > 0)
                        CleanTempDataButton.IsEnabled = true; // 启用清理按钮
                    return $"{folderSize} {folderSizeString}"; // 返回数据大小和单位
                }
                catch{}
            }
            return "0 B"; // 返回0字节
        }

        // 清理临时数据
        private void CleanTempDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); // 递归删除整个文件夹
                }
                Directory.CreateDirectory(folderPath); // 重新创建空文件夹
                using var toastSuccess = new ToastManager();
                toastSuccess.Show("清理完成！", "Success");
            }
            catch (Exception ex)
            {
                using var toastError = new ToastManager();
                toastError.Show("清理失败：" + ex.Message, "Error");
            }
        }

        // 控件关闭释放资源
        private void AboutQuickerGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear(); // 清理UI元素

            // 清理事件处理程序
            VersionLabel.Content = string.Empty; // 清理文本内容
            TempSize.Text = null; // 清理文本内容
        }
    }

    /// <summary>
    /// 版本信息根对象，对应VersionInfo.json的结构
    /// </summary>
    public class VersionInfoRoot
    {
        public string LatestVersion { get; set; }// 最新版本号
        public List<VersionItem> Versions { get; set; }// 所有版本的详细信息列表
    }
    /// <summary>
    /// 单个版本的信息
    /// </summary>
    public class VersionItem
    {
        public string Version { get; set; }// 版本号
        public string DownloadUrl { get; set; }// 下载地址
        public string DownloadUrlWithNet { get; set; }// 备用下载地址
        public string Changelog { get; set; }// 变更日志（多行字符串）
        public string ReleaseDate { get; set; }// 发布日期
        public bool IsLatest { get; set; }// 是否为最新版本
    }
}