using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
using Quicker.Database.Core;
using System.Windows.Media;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.Windows.AddWindows
{
    public partial class AddSceneWindow : Window
    {
        public event Action<bool, string?>? SceneAddCompleted; // 参数1: 是否保存, 参数2: 新场景名
        private IconManager iconManager = new(); // 图标管理器
        private SceneData currentSceneData; // 当前场景数据

        public AddSceneWindow()
        {
            InitializeComponent();
            LoadProcessesAsync();
        }

        // 选择进程后，显示进程名和图标
        private void AppComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessItem item = AppComboBox.SelectedItem as ProcessItem; // 获取选择的进程
            string fileNameWithExt = Path.GetFileName(item.FileName)?.ToLower(); // 文件名（带后缀，小写）
            SceneTagTextBlock.Text = fileNameWithExt; // 显示文件名
            string processNameNoExt = Path.GetFileNameWithoutExtension(item.ProcessName); // ProcessName 去除后缀
            SceneNameTextBox.Text = processNameNoExt; // 显示进程名
            currentSceneData = new SceneData
            {
                SceneName = processNameNoExt,
                SceneIconPath = item.FileName,
                SceneCount = 0,
                SceneTag = processNameNoExt,
                AutoReturnToFirstPage = false,
                SceneProcess = item.FileName
            }; // 构造场景数据模型
            SaveButton.IsEnabled = true; // 启用保存按钮
        }

        /// <summary>
        /// 异步加载进程列表
        /// </summary>
        public async void LoadProcessesAsync()
        {
            var processList = await Task.Run(() =>
            {
                var items = new List<(string FileName, string ProcessName)>();
                var processes = GetFilteredProcessList();
                foreach (var (fileName, processName) in processes)
                {
                    items.Add((fileName, processName));
                }
                return items;
            });

            // 2. UI线程创建Icon
            var uiList = new List<ProcessItem>();
            foreach (var (fileName, processName) in processList)
            {
                var icon = iconManager.GetIcon(fileName); // 这行必须在UI线程
                uiList.Add(new ProcessItem
                {
                    FileName = fileName,
                    ProcessName = processName,
                    Icon = icon
                });
            }
            AppComboBox.ItemsSource = uiList;
        }

        /// <summary>
        /// 判断进程是否有窗口（包括最小化的窗口）
        /// </summary>
        /// <param name="process">进程</param>
        /// <returns>是否存在窗口</returns>
        private bool HasWindow(Process process)
        {
            return process.MainWindowHandle != IntPtr.Zero; // 判断是否有窗口句柄
        }

        /// <summary>
        /// 获取符合条件的进程列表
        /// </summary>
        /// <returns>进程信息列表</returns>
        private List<(string FileName, string ProcessName)> GetFilteredProcessList()
        {
            var processList = new List<(string FileName, string ProcessName)>();
            var db = new ActionPageDatabase();
            var allScenes = db.GetAllSceneData(); // 获取所有场景数据
            try
            {
                var processes = Process.GetProcesses().Where(p => HasWindow(p)).Take(20); // 获取进程列表，最多20个
                foreach (var process in processes)
                {
                    try
                    {
                        string processFileName = process.MainModule.FileName; // 进程文件名
                        string fullProcessName = Path.GetFileName(processFileName)?.ToLower(); // 进程名（带后缀，小写）
                        string processName = Path.GetFileNameWithoutExtension(process.ProcessName); // 进程名（去除后缀）

                        // 检查是否与数据库中已有场景重复
                        bool isDuplicate = allScenes.Any(scene =>
                            scene.SceneTag?.ToLower() == processName &&
                            scene.SceneProcess?.Equals(processFileName, StringComparison.OrdinalIgnoreCase) == true);

                        if (isDuplicate) continue; // 跳过重复的进程
                        processList.Add((processFileName, fullProcessName)); // 添加到列表
                    }
                    catch { }
                    finally
                    {
                        process?.Dispose(); // 释放资源
                    }
                }
            }
            catch { } // 忽略异常
            return processList; // 返回进程列表
        }

        /// <summary>
        /// 添加到正在运行的程序提示框中
        /// </summary>
        /// <param name="appPath">应用路径</param>
        /// <param name="isBlacklist">是否为黑名单</param>
        private void AddAppItems(string appPath, bool isBlacklist = true)
        {
            string appNames = Path.GetFileName(appPath); // 获取进程名
            Button button = new()
            {
                Style = FindResource("MenuButton") as Style,
                Tag = appNames
            };

            // 用Grid代替StackPanel，结构与ComboBox模板一致
            Grid grid = new Grid();
            Image iconImage = new()
            {
                Style = FindResource("MenuButtonImage") as Style,
                Source = iconManager.GetIcon(appPath)
            };
            grid.Children.Add(iconImage);

            TextBlock textBlock = new()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(25, 0, 0, 0),
                Text = appNames
            };
            grid.Children.Add(textBlock);

            button.Content = grid;
        }

        // 保存场景数据
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSceneData != null) // 场景数据不为空
            {
                if (AppComboBox.SelectedItem is ProcessItem item && item.Icon != null) // 保存到数据库前，保存图标到本地文件
                {
                    string iconPath = iconManager.SaveIconToFile(item.Icon); // 保存图标到本地文件
                    if (!string.IsNullOrEmpty(iconPath)) // 图标保存成功
                    {
                        currentSceneData.SceneName = SceneNameTextBox.Text; // 更新场景名
                        currentSceneData.SceneIconPath = iconPath; // 更新图标路径
                    }
                }
                var db = new ActionPageDatabase(); // 数据库连接
                db.CreateAndInitTable(currentSceneData.SceneTag, currentSceneData.SceneIconPath, currentSceneData.SceneTag); // 初始化数据表
                db.UpdateSceneTable(currentSceneData); // 更新场景数据
            }
            SceneAddCompleted?.Invoke(true, currentSceneData?.SceneTag); // 通知父窗口保存完成
            this.Close(); // 关闭窗口
        }

        // 取消场景添加
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SceneAddCompleted?.Invoke(false, null);
            this.Close(); // 关闭窗口
        }
    }

    /// <summary>
    /// 进程信息
    /// </summary>
    public class ProcessItem
    {
        public string FileName { get; set; } // 进程文件名
        public string ProcessName { get; set; } // 进程名（带后缀）
        public ImageSource Icon { get; set; } // 图标
    }
}