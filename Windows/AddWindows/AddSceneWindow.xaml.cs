using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
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
        private IconManager iconManager = new(); // 图标管理器
        private SceneData currentSceneData; // 当前场景数据

        public AddSceneWindow()
        {
            InitializeComponent();
            LoadProcessesAsync();
            AppComboBox.SelectionChanged += AppComboBox_SelectionChanged;
        }

        public event Action<bool, string?>? SceneAddCompleted; // 参数1: 是否保存, 参数2: 新场景名

        private void AppComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppComboBox.SelectedItem is ProcessItem item)
            {
                // 文件名（带后缀，小写）
                string fileNameWithExt = Path.GetFileName(item.FileName)?.ToLower();
                SceneTagTextBlock.Text = fileNameWithExt;
                // ProcessName 去除后缀
                string processNameNoExt = Path.GetFileNameWithoutExtension(item.ProcessName);
                SceneNameTextBox.Text = processNameNoExt;
                // 构造场景数据模型
                currentSceneData = new SceneData
                {
                    SceneName = processNameNoExt,
                    SceneIconPath = item.FileName, // 这里可根据实际需求保存图标路径或文件路径
                    SceneCount = 0,
                    SceneTag = fileNameWithExt,
                    AutoReturnToFirstPage = false,
                    SceneProcess = item.FileName
                };

                SaveButton.IsEnabled = true; // 启用保存按钮
            }
            else
            {
                SaveButton.IsEnabled = false;
            }
        }

        public async void LoadProcessesAsync()
        {
            // 1. 后台线程只收集文件名和进程名
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
            var processList = new List<(string FileName, string ProcessName)>(); // 创建列表
            var uniqueProcessNames = new HashSet<string>(); // 创建集合
            try
            {
                // 只获取有窗口的进程
                var processes = Process.GetProcesses()
                    .Where(p => HasWindow(p))
                    .Take(20); // 限制最大进程数

                foreach (var process in processes)
                {
                    try
                    {
                        string processFileName = process.MainModule.FileName; // 获取进程文件名
                        string fullProcessName = Path.GetFileName(processFileName); // 获取进程名

                        processList.Add((processFileName, fullProcessName)); // 添加到列表
                        if (processList.Count >= 8) break; // 如果超过8个，跳出循环
                    }
                    catch { } // 忽略异常
                    finally
                    {
                        process?.Dispose(); // 释放进程
                    }
                }
            }
            catch { } // 忽略异常
            return processList;
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
                Text = appNames,
                Margin = new Thickness(25, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(textBlock);

            button.Content = grid;
            // 你可以把button加到需要的容器里
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSceneData != null)
            {
                // 保存到数据库前，保存图标到本地文件
                if (AppComboBox.SelectedItem is ProcessItem item && item.Icon != null)
                {
                    string iconPath = iconManager.SaveIconToFile(item.Icon);
                    if (!string.IsNullOrEmpty(iconPath))
                    {
                        currentSceneData.SceneName = SceneNameTextBox.Text;
                        currentSceneData.SceneIconPath = iconPath;
                    }
                }
                var db = new Quicker.Database.Core.ActionPageDatabase();
                db.CreateAndInitTable(currentSceneData.SceneTag, currentSceneData.SceneIconPath, currentSceneData.SceneTag);
                db.UpdateSceneTable(currentSceneData.SceneTag, currentSceneData);
            }
            SceneAddCompleted?.Invoke(true, currentSceneData?.SceneTag);
            this.Close(); // 关闭窗口
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SceneAddCompleted?.Invoke(false, null);
            this.Close(); // 关闭窗口
        }
    }

    public class ProcessItem
    {
        public string FileName { get; set; }
        public string ProcessName { get; set; }
        public ImageSource Icon { get; set; }
    }
}