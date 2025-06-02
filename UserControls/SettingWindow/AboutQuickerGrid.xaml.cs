using Quicker.Windows.MainWindows;
using System.Windows.Resources;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;
using System.IO;

namespace Quicker.UserControls
{
    public partial class AboutQuickerGrid : UserControl
    {
        private const string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\TempData"; // 文件夹路径
        private WeakReference<SettingWindow> weakSettingWindow; // 弱引用设置窗口
        SettingManager settingManager; // 读取设置的管理器

        public AboutQuickerGrid(SettingWindow settingWindow)
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

        // 打开更新历史文件
        private void OpenUpdateHistory(object sender, MouseButtonEventArgs e)
        {
            string resourceName = "InformationData/UpdateHistory.txt"; // 确保资源名称正确
            Uri resourceUri = new Uri(resourceName, UriKind.Relative); // 构造资源URI
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri); // 获取资源流信息
            using (StreamReader reader = new StreamReader(streamInfo.Stream)) // 读取资源流
            {
                string content = reader.ReadToEnd(); // 读取资源内容
                string tempPath = Path.GetTempPath(); // 获取临时文件夹路径
                string tempFilePath = Path.Combine(tempPath, "更新历史.txt"); // 构造临时文件路径
                File.WriteAllText(tempFilePath, content); // 写入临时文件
                System.Diagnostics.Process.Start("notepad.exe", tempFilePath); // 使用系统默认文本编辑器打开临时文件
            }
        }

        // 前往图标网站www.iconfont.cn
        private void www_iconfont_cn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser("https://www.iconfont.cn"); // 打开图标网站www.iconfont.cn
        }

        // 前往图标网站icons8.com
        private void icons8_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser("https://icons8.com/"); // 打开图标网站icons8.com
        }

        // 前往图标网站fontawesome.com
        private void fontawesome_com_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser("https://fontawesome.com/"); // 打开图标网站fontawesome.com
        }

        // 前往icon11社区图标库
        private void icon11_community_github_io_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser("https://icon11-community.github.io/icons/"); // 前往icon11社区图标库
        }

        // BUG反馈、需求
        private void FeedBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            actionManager.LaunchDefaultBrowser("https://github.com/LJZ-Anonymity/Quicker/issues"); // 前往Github反馈
        }

        // 基础设置-关于Quicker-隐私声明
        public void Privacy_StatementButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(Privacy_StatementButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(Privacy_StatementButton, MainGrid); // 保存Button类型3边框设置
        }

        // 前往程序数据根目录
        private void RootFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\"; // 文件夹路径
            OpenFolder(folderPath); // 打开文件夹
        }

        // 前往程序数据库目录
        private void DatabaseFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Database\"; // 文件夹路径
            OpenFolder(folderPath); // 打开文件夹
        }

        // 前往程序图标目录
        private void LocalIconsFolderPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 文件夹路径
            OpenFolder(folderPath); // 打开文件夹
        }

        // 备份数据
        private void BackupDataButton_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog() { Description = "选择备份路径" }; // 创建文件夹对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderDialog.SelectedPath; // 获取选择的路径
                string sourceFolderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker"; // 源文件夹路径
                string backupFolderPath = Path.Combine(folderPath, "QuickerData"); // 构造备份文件夹路径
                if (!Directory.Exists(backupFolderPath)) Directory.CreateDirectory(backupFolderPath); // 创建备份文件夹
                CopyFolder(sourceFolderPath, backupFolderPath); // 复制文件夹内容到目标路径
                using var toast = new ToastManager(); // 创建 ToastManager 的实例
                toast.Show("操作完成！", "Success"); // 显示提示
                OpenFolder(folderPath); // 打开文件夹
            }
        }

        /// <summary>
        /// 打开程序文件夹
        /// </summary>
        /// <param name="folderPath"> 文件夹路径 </param>
        private void OpenFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); // 创建文件夹
            Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true }); // 打开文件夹
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
            if (Directory.Exists(folderPath))
            {
                try
                {
                    long folderSize = 0;
                    string folderSizeString = "B"; // 默认单位为字节
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                    // 获取所有文件（包括子文件夹）
                    var files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);

                    // 遍历所有文件并累加大小
                    foreach (var file in files)
                    {
                        folderSize += file.Length;
                    }

                    // 使用 DataConversionManager 转换数据大小和单位
                    using var convertionManager = new DataConversionManager();
                    folderSize = convertionManager.ConversionData((int)folderSize);
                    folderSizeString = convertionManager.ConversionUnits((int)folderSize);

                    return $"{folderSize}{folderSizeString}";
                }
                catch (Exception ex)
                {
                    // 记录异常信息
                    Debug.WriteLine($"获取临时数据大小时出错: {ex.Message}");
                }
            }
            return "0B";
        }

        // 清理临时数据
        private void CleanTempDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath); // 创建 DirectoryInfo 对象
                foreach (FileInfo file in directoryInfo.GetFiles()) // 遍历并删除所有文件
                {
                    file.Delete(); // 删除文件
                }
                foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories()) // 遍历并删除所有子文件夹
                {
                    subDirectory.Delete(true); // 递归删除子文件夹
                }
            }
            using var toast = new ToastManager(); // 创建 ToastManager 的实例
            toast.Show("清理完成！", "Success"); // 显示提示
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
}