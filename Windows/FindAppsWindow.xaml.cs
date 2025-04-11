using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Data;
using Quicker.Database;
using Microsoft.Win32;
using System.Windows;
using System.Text;
using System.IO;

namespace Quicker.Windows
{
    public partial class FindAppsWindow : Window
    {
        private static readonly ConcurrentDictionary<string, ImageSource> iconCache = new ConcurrentDictionary<string, ImageSource>(); // 图标缓存
        private ConcurrentDictionary<string, string> linkTargetCache = new ConcurrentDictionary<string, string>(); // 快捷方式目标路径缓存
        private static WeakReference<FindAppsWindow> _findAppsWindowRef = new WeakReference<FindAppsWindow>(null); // 避免内存泄漏
        private ConcurrentDictionary<string, string> fileHashCache = new ConcurrentDictionary<string, string>(); // 文件哈希缓存
        public delegate void ApplicationSelectedEventHandler(object sender, ApplicationSelectedEventArgs e);
        private ObservableCollection<AppInfo> _allApplications = new ObservableCollection<AppInfo>(); // 所有应用
        public event ApplicationSelectedEventHandler ApplicationSelected; // 选中应用事件
        private List<AppInfo> _searchResults = new List<AppInfo>(); // 搜索结果
        private CancellationTokenSource _cancellationTokenSource; // 取消令牌源,管理异步任务
        private ICollectionView _applicationView; // ICollectionView接口
        private const int SLR_NO_UI = 0x00000001; // 在解析快捷方式时不显示用户界面
        private ScrollViewer scrollViewer; // 滚动条

        private static T FindAncestor<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            if (parent is T t) return t;
            return FindAncestor<T>(parent);
        } // 辅助方法：查找祖先元素
        public static FindAppsWindow findAppsWindow
        {
            get
            {
                if (_findAppsWindowRef.TryGetTarget(out var window))
                {
                    return window;
                }
                return null;
            }
            set
            {
                _findAppsWindowRef = new WeakReference<FindAppsWindow>(value);
            }
        } // 避免内存泄漏
        public class ApplicationSelectedEventArgs : EventArgs
        {
            public AppInfo SelectedApp { get; set; }
        } // 传递选中的应用
        public AppInfo SelectedApp { get; set; } // 选中的应用

        // 操作Windows快捷方式(.lnk文件)
        [ComImport]
        [Guid("0000010c-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellLink
        {
            // 获取快捷方式的目标路径
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out int pFlags, int fResolve);
            // 获取快捷方式的ID列表
            void GetIDList(out IntPtr ppidl);
            // 设置快捷方式的ID列表
            void SetIDList(IntPtr pidl);
            // 获取快捷方式的描述
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            // 设置快捷方式的描述
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            // 获取快捷方式的工作目录
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            // 设置快捷方式的工作目录
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            // 获取快捷方式的参数
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            // 设置快捷方式的参数
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            // 获取快捷方式的热键
            void GetHotkey(out short pwHotkey);
            // 设置快捷方式的热键
            void SetHotkey(short wHotkey);
            // 获取快捷方式的显示命令
            void GetShowCmd(out int piShowCmd);
            // 设置快捷方式的显示命令
            void SetShowCmd(int iShowCmd);
            // 获取快捷方式的图标位置
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            // 设置快捷方式的图标位置
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            // 设置快捷方式的相对路径
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            // 解析快捷方式
            void Resolve(IntPtr hwnd, int fFlags);
            // 设置快捷方式的路径
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        // 操作快捷方式
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ProgId("Shell.Application")]
        public class ShellLink
        {
        }

        public FindAppsWindow()
        {
            InitializeComponent(); // 初始化窗口的UI组件

            SearchTextBox.Focus(); // 让搜索框获得焦点
            AddWindow.FindAppsWindow = this; // 设置静态字段，方便在其他窗口中引用
            _cancellationTokenSource = new CancellationTokenSource(); // 初始化 CancellationTokenSource

            // 绑定 ItemsSource 并创建视图
            _applicationView = CollectionViewSource.GetDefaultView(_allApplications);
            ApplicationsListView.ItemsSource = _applicationView;
        }

        // UI加载完成后加载应用
        private void LoadApplications(object sender, EventArgs e)
        {
            LoadApplications(); // 调用加载应用的方法
        }

        // 加载应用
        private void LoadApplications()
        {
            iconCache.Clear();
            _allApplications.Clear(); // 清空原始数据源
            LoadingWindow loadingWindow = new(); // 显示加载窗口
            loadingWindow.Owner = this; // 设置加载窗口的所有者
            loadingWindow.ShowDialog(); // 显示加载窗口
            Application.Current.Dispatcher.Invoke(() =>
            {
                _applicationView.Refresh();
            }); // 刷新视图
            InitializeScrollBar(); // 初始化滚动条
        }

        // 初始化滚动条
        private void InitializeScrollBar()
        {
            scrollViewer = GetScrollViewer(ApplicationsListView); // 获取ListView的ScrollViewer

            // 初始化纵向滚动条
            VerticalScrollBar.Maximum = scrollViewer.ScrollableHeight; // 设置最大值
            VerticalScrollBar.ViewportSize = scrollViewer.ViewportHeight; // 设置视口大小
            VerticalScrollBar.Value = scrollViewer.VerticalOffset; // 设置当前值

            // 初始化横向滚动条
            HorizontalScrollBar.Maximum = scrollViewer.ScrollableWidth; // 设置最大值
            HorizontalScrollBar.ViewportSize = scrollViewer.ViewportWidth; // 设置视口大小
            HorizontalScrollBar.Value = scrollViewer.HorizontalOffset; // 设置当前值
        }

        // 获取ScrollViewer
        private ScrollViewer GetScrollViewer(Visual visual)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = VisualTreeHelper.GetChild(visual, i) as Visual; // 获取子元素
                if (child != null) // 如果子元素不为空
                {
                    ScrollViewer scrollViewer = child as ScrollViewer; // 尝试转换为ScrollViewer
                    if (scrollViewer != null) // 如果是ScrollViewer
                    {
                        return scrollViewer; // 返回ScrollViewer
                    }
                    scrollViewer = GetScrollViewer(child); // 递归查找
                    if (scrollViewer != null) // 如果找到ScrollViewer
                    {
                        return scrollViewer; // 返回ScrollViewer
                    }
                }
            }
            return null; // 未找到返回空
        }

        // 从注册表加载应用路径
        public async void LoadFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
                {
                    if (key != null)
                    {
                        string[] subKeyNames = key.GetSubKeyNames(); // 获取所有子键名称
                        foreach (string subKeyName in subKeyNames)
                        {
                            try
                            {
                                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey != null)
                                    {
                                        string path = (string)subKey.GetValue(""); // 获取默认值
                                        if (!string.IsNullOrEmpty(path))
                                        {
                                            // 检查目标文件是否存在
                                            if (File.Exists(path))
                                            {
                                                string appName = Path.GetFileNameWithoutExtension(path); // 获取文件名（去掉扩展名）
                                                _allApplications.Add(new AppInfo { Name = appName, Location = path, Icon = await GetIconAsync(path) }); // 添加到应用列表
                                            }
                                        }
                                    }
                                }
                            }
                            catch { } // 忽略单个注册表项的错误
                        }
                    }
                }
            }
            catch { } // 忽略注册表加载的总体错误
        }

        // 异步加载图标
        private async Task<ImageSource> GetIconAsync(string filePath)
        {
            if (iconCache.TryGetValue(filePath, out var icon))
            {
                return icon;
            }

            try
            {
                using (var iconEx = await Task.Run(() => System.Drawing.Icon.ExtractAssociatedIcon(filePath)))
                {
                    if (iconEx != null)
                    {
                        icon = Imaging.CreateBitmapSourceFromHIcon(
                            iconEx.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions()
                        );
                    }
                }
            }
            catch { }

            if (icon != null)
            {
                iconCache[filePath] = icon;
            }
            return icon;
        }

        // 从常见路径加载 .exe 或 .lnk 文件
        public void LoadFromCommonPaths()
        {
            string[] commonPaths = new[] // 要扫描的路径
            {
                "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs"    // 系统开始菜单程序
            };

            foreach (var path in commonPaths) // 遍历当前路径下的所有文件和2级子文件夹
            {
                EnumerateFiles(path, new[] { ".exe", ".lnk" }, 2);
            } // 遍历当前路径下的所有文件和2级子文件夹
        }

        // 递归遍历至2级文件夹
        private void EnumerateFiles(string directoryPath, string[] allowedExtensions, int maxDepth)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        LoadApplication(file); // 加载单个应用
                    }
                }

                if (maxDepth > 0)
                {
                    foreach (var subDir in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        EnumerateFiles(subDir, allowedExtensions, maxDepth - 1); // 递归遍历
                    }
                }
            }
            catch { } // 忽略异常
        }

        // 加载单个应用
        private void LoadApplication(string filePath)
        {
            try
            {
                string appName;
                ImageSource icon;
                string location = filePath; // 默认使用文件本身的路径
                if (Path.GetExtension(filePath).ToLower() == ".lnk") // 解析文件扩展名
                {
                    appName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名（去掉 .lnk 扩展）
                    icon = ShellIconFromLink(filePath); // 获取快捷方式的图标
                }
                else if (Path.GetExtension(filePath).ToLower() == ".exe")
                {
                    appName = Path.GetFileNameWithoutExtension(filePath); // .exe 文件直接处理
                    icon = GetIcon(filePath);
                }
                else
                {
                    return; // 跳过其他类型的文件
                }

                if (string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(location))
                {
                    return; // 跳过无效的文件
                }

                _allApplications.Add(new AppInfo { Name = appName, Location = location, Icon = icon }); // 添加到应用列表
            }
            catch { } // 忽略异常
        }

        // 从快捷方式文件中获取目标路径
        private string GetTargetPathFromLinkFile(string linkFilePath)
        {
            // 检查缓存
            if (linkTargetCache.TryGetValue(linkFilePath, out var targetPath))
            {
                return targetPath;
            }

            try
            {
                IShellLink shellLink = (IShellLink)new ShellLink();
                shellLink.SetPath(linkFilePath);

                StringBuilder path = new StringBuilder(260);
                int flags = 0;
                shellLink.GetPath(path, path.Capacity, out flags, SLR_NO_UI);
                targetPath = path.ToString();

                // 缓存解析结果
                linkTargetCache[linkFilePath] = targetPath;
                return targetPath;
            }
            catch
            {
                return null;
            }
        }

        // 获取图标的优化方法
        private ImageSource GetIcon(string filePath)
        {
            string cacheKey = GetCacheKey(filePath); // 文件哈希值作为缓存键           
            if (iconCache.TryGetValue(cacheKey, out var icon)) // 检查缓存
            {
                return icon;
            }
            return null; // 如果图标不存在于缓存中，返回空
        }

        // 如果快捷方式的目标路径无效，使用快捷方式文件本身的图标
        private ImageSource ShellIconFromLink(string linkFilePath)
        {
            // 检查缓存
            string cacheKey = GetCacheKey(linkFilePath);
            if (iconCache.TryGetValue(cacheKey, out var icon))
            {
                return icon;
            }

            try
            {
                // 获取快捷方式的目标路径
                string targetPath = GetTargetPathFromLinkFile(linkFilePath);
                if (!string.IsNullOrEmpty(targetPath))
                {
                    // 从目标路径提取图标
                    icon = ExtractIconFromPath(targetPath);
                    if (icon != null)
                    {
                        iconCache[cacheKey] = icon;
                        return icon;
                    }
                }

                // 如果目标路径无效，从快捷方式文件本身提取图标
                icon = ExtractIconFromPath(linkFilePath);
                if (icon != null)
                {
                    iconCache[cacheKey] = icon;
                    return icon;
                }
            }
            catch { } // 忽略图标加载错误
            return null;
        }

        // 获取文件的哈希值作为缓存键
        private string GetCacheKey(string filePath)
        {           
            if (fileHashCache.TryGetValue(filePath, out var cacheKey)) // 检查文件是否已缓存
            {
                return cacheKey;
            }
                       
            var fileInfo = new FileInfo(filePath); // 获取文件的最后修改时间
            if (!fileInfo.Exists)
            {
                return null;
            }

            // 如果文件较小，计算哈希值；否则使用文件大小和最后修改时间作为缓存键
            if (fileInfo.Length < 1024 * 1024)
            {
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    cacheKey = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            } // 小于1MB的文件计算哈希值
            else
            {               
                cacheKey = $"{fileInfo.Length}_{fileInfo.LastWriteTimeUtc.Ticks}";
            } // 对于较大的文件，使用文件大小和最后修改时间作为缓存键

            fileHashCache[filePath] = cacheKey; // 添加到缓存
            return cacheKey;
        }

        // 从文件路径提取图标
        private ImageSource ExtractIconFromPath(string filePath)
        {
            try
            {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon != null)
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions()
                        );
                    }
                }
            }
            catch { } // 忽略图标加载错误
            return null;
        }

        // 文本框内容改变时，进行查找
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchApps(); // 调用搜索方法
        }

        // 通过应用名称在ListView中查找应用
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchApps(); // 调用搜索方法
        }

        // 输入应用名查找应用
        private void SearchApps()
        {
            string searchText = SearchTextBox.Text.Trim().ToLower(); // 获取搜索文本
            Application.Current.Dispatcher.Invoke(() => // 在UI线程中执行
            {
                _applicationView.Filter = item => // 过滤器
                {
                    if (string.IsNullOrEmpty(searchText)) // 如果搜索文本为空，返回所有应用
                    {
                        return true;
                    }

                    AppInfo app = item as AppInfo; // 获取应用信息
                    return app != null && app.Name.ToLower().Contains(searchText); // 检查应用名称是否包含搜索文本
                };
            });
        }

        // 刷新ListView
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadApplications(); // 重新加载应用
        }

        // 在文件管理器中定位应用
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = Application.Current.Windows.OfType<AddWindow>().FirstOrDefault(); // 获取AddWindow实例
            addWindow.ChooseProcess(sender, e); // 调用选择进程的方法
        }

        // 拖动到正在运行的窗口上定位应用


        // 鼠标移入ListView显示滚动条
        private void ShowScrollBar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VerticalScrollBar.Visibility = Visibility.Visible; // 显示纵向滚动条
            HorizontalScrollBar.Visibility = Visibility.Visible; // 显示横向滚动条
        }

        // 鼠标移出ListView隐藏滚动条
        private void HideScrollBar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed; // 隐藏纵向滚动条
            HorizontalScrollBar.Visibility = Visibility.Collapsed; // 隐藏横向滚动条
        }

        // 递归查找VisualChild
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i); // 获取子元素
                if (child is T t)
                {
                    return t; // 返回匹配的子元素
                }

                T foundChild = FindVisualChild<T>(child); // 递归查找
                if (foundChild != null)
                {
                    return foundChild; // 返回匹配的子元素
                }
            }
            return null; // 未找到返回空
        }

        // 选中软件后启用保存Button
        private void ApplicationsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = ApplicationsListView.SelectedItem as AppInfo;
            SelectedApp = selectedItem; // 选中的应用
            SaveButton.IsEnabled = true; // 启用保存按钮
        }

        // 双击选中应用直接保存
        private void SaveApplication(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ApplicationsListView.Items.Count > 0)
            {
                SaveApp(sender, e); // 保存程序
            }
        }

        // 保存程序
        private void SaveApp(object sender, RoutedEventArgs e)
        {
            ApplicationSelected?.Invoke(this, new ApplicationSelectedEventArgs { SelectedApp = SelectedApp }); // 触发事件，通知其他窗口应用被选中
            this.Close(); // 关闭窗口
        }

        // 关闭FindAppsWindow
        private void CloseThisWindow(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 复制应用地址
        private void CopyLocation(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource); // 获取右键点击的 ListViewItem
            if (listViewItem != null)
            {
                AppInfo selectedApp = listViewItem.DataContext as AppInfo; // 获取选中的应用
                if (selectedApp != null && !string.IsNullOrEmpty(selectedApp.Location))
                {                    
                    string directoryPath = Path.GetDirectoryName(selectedApp.Location); // 提取文件所在目录路径
                    if (!string.IsNullOrEmpty(directoryPath))
                    {                        
                        Clipboard.SetText(directoryPath); // 复制目录路径到剪贴板                        
                        new ToastContentBuilder().AddText("文件夹路径已复制到剪贴板！").Show(); // 通知用户已复制
                    }
                }
            }
        }

        // 外部滚动条的滚动事件
        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollViewer.ScrollToVerticalOffset(VerticalScrollBar.Value);
        }

        private void HorizontalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollViewer.ScrollToHorizontalOffset(HorizontalScrollBar.Value);
        }

        // ScrollViewer的滚动事件
        private void ListViewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (scrollViewer != null)
            {
                // 更新纵向滚动条
                VerticalScrollBar.Maximum = scrollViewer.ScrollableHeight; // 设置最大值
                VerticalScrollBar.ViewportSize = scrollViewer.ViewportHeight; // 设置视口大小
                VerticalScrollBar.Value = scrollViewer.VerticalOffset; // 设置当前值

                // 更新横向滚动条
                HorizontalScrollBar.Maximum = scrollViewer.ScrollableWidth; // 设置最大值
                HorizontalScrollBar.ViewportSize = scrollViewer.ViewportWidth; // 设置视口大小
                HorizontalScrollBar.Value = scrollViewer.HorizontalOffset; // 设置当前值
            }
        }

        // 关闭该窗口时清除内存占用
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类方法
            _allApplications.Clear(); // 清空应用列表
            iconCache.Clear(); // 清空图标缓存
            ApplicationSelected = null; // 清空事件
            AddWindow.FindAppsWindow = null; // 清空静态字段

            // 释放其他资源
            foreach (var icon in iconCache.Values)
            {
                if (icon is IDisposable disposableIcon)
                {
                    disposableIcon.Dispose(); // 释放图标资源
                }
            }

            foreach (var app in _allApplications)
            {
                if (app.Icon is IDisposable disposableAppIcon)
                {
                    disposableAppIcon.Dispose(); // 释放应用图标资源
                }
            }
        }
    }

    // 定义应用信息类
    public class AppInfo
    {
        public string Name { get; set; } // 应用名称
        public string Location { get; set; } // 应用路径
        public ImageSource Icon { get; set; } // 应用图标
    }
}