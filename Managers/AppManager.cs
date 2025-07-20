using System.Runtime.InteropServices.WindowsRuntime;
using WinRTSize = Windows.Foundation.Size;
using WinRTRect = Windows.Foundation.Rect;
using System.Runtime.InteropServices;
using WpfSize = System.Windows.Size;
using WpfRect = System.Windows.Rect;
using System.Collections.Concurrent;
using Windows.Management.Deployment;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Media;
using Windows.Foundation;
using Quicker.Helpers;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Windows;
using System.Text;
using System.IO;
using Windows.ApplicationModel.Core;

namespace Quicker.Managers
{
    public class AppManager
    {
        private static readonly ConcurrentDictionary<string, WeakReference<ImageSource>> _iconCache = new();
        private static readonly Timer _cleanupTimer;
        private readonly ConcurrentDictionary<string, string> _linkTargetCache = new();
        private readonly ConcurrentDictionary<string, string> _fileHashCache = new();
        private const int SLR_NO_UI = 0x00000001; // 在解析快捷方式时不显示用户界面

        static AppManager()
        {
            // 每5分钟清理一次过期的图标缓存
            _cleanupTimer = new Timer(CleanupIconCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        private static void CleanupIconCache(object state)
        {
            var expiredKeys = _iconCache.Keys
                .Where(key => !_iconCache[key].TryGetTarget(out _))
                .ToList();
                
            foreach (var key in expiredKeys)
            {
                _iconCache.TryRemove(key, out _);
            }
        }

        public AppManager() { }

        // 清除缓存
        public void ClearCache()
        {
            _iconCache.Clear();
            _linkTargetCache.Clear();
            _fileHashCache.Clear();
        }

        /// <summary>
        /// 加载所有应用（注册表、UWP等）
        /// </summary>
        /// <returns> 应用列表 </returns>
        public async Task<List<AppInfo>> LoadAllApplicationsAsync()
        {
            var allApps = new ConcurrentBag<AppInfo>(); // 应用列表
            var tasks = new List<Task>
            {
                LoadFromRegistryAsync(allApps),
                LoadUWPAppsAsync(allApps)
            };
            await Task.WhenAll(tasks);
            return allApps.OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>
        /// 加载注册表中的应用
        /// </summary>
        /// <param name="appsList"> 应用列表 </param>
        private async Task LoadFromRegistryAsync(ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                foreach (var subKeyName in GetRegistryAppSubKeys())
                {
                    await TryAddRegistryAppAsync(subKeyName, appsList);
                }
            }
            catch { }
        }

        /// <summary>
        /// 获取注册表中所有应用子键名
        /// </summary>
        private IEnumerable<string> GetRegistryAppSubKeys()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
            {
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
        }

        /// <summary>
        /// 尝试将注册表中的应用添加到列表
        /// </summary>
        private async Task TryAddRegistryAppAsync(string subKeyName, ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{subKeyName}"))
                {
                    string path = (string)subKey?.GetValue("");
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        string appName = Path.GetFileNameWithoutExtension(path);
                        var icon = await GetIconAsync(path);
                        AddAppInfo(appsList, appName, path, icon, path);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 添加应用信息到列表
        /// </summary>
        private void AddAppInfo(ConcurrentBag<AppInfo> appsList, string name, string location, ImageSource icon, string tag)
        {
            if (!IsAppAlreadyAdded(name, location, appsList))
            {
                appsList.Add(new AppInfo { Name = name, Location = location, Icon = icon, Tag = tag });
            }
        }

        // 从常见路径加载 .exe 或 .lnk 文件
        private async Task LoadFromCommonPathsAsync(ConcurrentBag<AppInfo> appsList)
        {
            string[] commonPaths = new[] // 要扫描的路径
            {
                "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs"    // 系统开始菜单程序
            };

            foreach (var path in commonPaths) // 遍历当前路径下的所有文件和2级子文件夹
            {
                await EnumerateFilesAsync(path, new[] { ".exe", ".lnk" }, 2, appsList);
            } // 遍历当前路径下的所有文件和2级子文件夹
        }

        // 递归遍历至2级文件夹（异步版本）
        private async Task EnumerateFilesAsync(string directoryPath, string[] allowedExtensions, int maxDepth, ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        await LoadApplicationAsync(file, appsList); // 加载单个应用
                    }
                }

                if (maxDepth > 0)
                {
                    foreach (var subDir in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        await EnumerateFilesAsync(subDir, allowedExtensions, maxDepth - 1, appsList); // 递归遍历
                    }
                }
            }
            catch { } // 忽略异常
        }

        // 加载单个应用（异步版本）
        private async Task LoadApplicationAsync(string filePath, ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                string appName;
                ImageSource icon = null;
                string location = filePath; // 默认使用文件本身的路径
                if (Path.GetExtension(filePath).ToLower() == ".lnk") // 解析文件扩展名
                {
                    appName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名（去掉 .lnk 扩展）
                    icon = await Task.Run(() => ShellIconFromLink(filePath)); // 获取快捷方式的图标
                }
                else if (Path.GetExtension(filePath).ToLower() == ".exe")
                {
                    appName = Path.GetFileNameWithoutExtension(filePath); // .exe 文件直接处理
                    icon = await GetIconAsync(filePath);
                }
                else
                {
                    return; // 跳过其他类型的文件
                }

                if (string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(location))
                {
                    return; // 跳过无效的文件
                }

                if (!IsAppAlreadyAdded(appName, location, appsList))
                {
                    appsList.Add(new AppInfo { Name = appName, Location = location, Icon = icon, Tag = location }); // 添加到应用列表
                }
            }
            catch { } // 忽略异常
        }

        // 添加应用前检查是否已存在
        private bool IsAppAlreadyAdded(string appName, string location, ConcurrentBag<AppInfo> appsList)
        {
            return appsList.Any(app =>
                app.Name.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                app.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 加载UWP应用
        /// </summary>
        private async Task LoadUWPAppsAsync(ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                var packages = GetUwpPackages();
                var tasks = packages.Select(package => ProcessUwpPackageAsync(package, appsList));
                await Task.WhenAll(tasks);
            }
            catch { }
        }

        /// <summary>
        /// 获取所有UWP包
        /// </summary>
        private List<Package> GetUwpPackages()
        {
            PackageManager packageManager = new();
            return packageManager.FindPackagesForUser("").ToList();
        }

        /// <summary>
        /// 处理单个UWP包
        /// </summary>
        private async Task ProcessUwpPackageAsync(Package package, ConcurrentBag<AppInfo> appsList)
        {
            if (!IsValidUwpPackage(package)) return;
            var appEntries = await package.GetAppListEntriesAsync();
            foreach (var appEntry in appEntries)
            {
                await AddUwpAppEntryAsync(appEntry, package, appsList);
            }
        }

        /// <summary>
        /// 判断UWP包是否有效
        /// </summary>
        private bool IsValidUwpPackage(Package package)
        {
            return !(package.IsFramework || package.IsResourcePackage || package.IsBundle ||
                     string.IsNullOrWhiteSpace(package.DisplayName) ||
                     package.DisplayName.Equals(package.Id.Name, StringComparison.OrdinalIgnoreCase) ||
                     package.IsDevelopmentMode ||
                     package.Id.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                     package.Id.Name.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 添加UWP应用条目到列表
        /// </summary>
        private async Task AddUwpAppEntryAsync(AppListEntry appEntry, Package package, ConcurrentBag<AppInfo> appsList)
        {
            string appUserModelId = appEntry.AppUserModelId;
            string displayName = appEntry.DisplayInfo.DisplayName;
            ImageSource icon = await GetUwpAppIconAsync(appEntry, package.Id.FamilyName);
            AddAppInfo(appsList, displayName, appUserModelId, icon, "[Windows 商店应用]");
        }

        /// <summary>
        /// 获取UWP应用图标（优先大图标）
        /// </summary>
        private async Task<ImageSource> GetUwpAppIconAsync(AppListEntry appEntry, string packageFamilyName)
        {
            try
            {
                var streamRef = appEntry.DisplayInfo.GetLogo(new WinRTSize(400, 400));
                using (var randomAccessStream = await streamRef.OpenReadAsync())
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = randomAccessStream.AsStream();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    if (bitmap.PixelWidth < 256)
                    {
                        var icon2 = ExtractUWPAppIcon(packageFamilyName);
                        return icon2 ?? bitmap;
                    }
                    return bitmap;
                }
            }
            catch
            {
                return ExtractUWPAppIcon(packageFamilyName);
            }
        }

        /// <summary>
        /// 将 BitmapSource 放大到指定尺寸
        /// </summary>
        private BitmapSource ResizeBitmap(BitmapSource source, int width, int height)
        {
            if (source == null) return null;
            var group = new DrawingGroup();
            group.Children.Add(new ImageDrawing(source, new WpfRect(0, 0, width, height)));
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(group);
            }
            var resizedImage = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            resizedImage.Render(drawingVisual);
            resizedImage.Freeze();
            return resizedImage;
        }

        // 异步加载图标
        private async Task<ImageSource> GetIconAsync(string filePath)
        {
            if (_iconCache.TryGetValue(filePath, out var weakRef) && 
                weakRef.TryGetTarget(out var cachedIcon))
            {
                return cachedIcon;
            }

            ImageSource icon = null;
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
                _iconCache[filePath] = new WeakReference<ImageSource>(icon);
            }
            return icon;
        }

        // 从快捷方式文件中获取目标路径
        private string GetTargetPathFromLinkFile(string linkFilePath)
        {
            // 检查缓存
            if (_linkTargetCache.TryGetValue(linkFilePath, out var targetPath))
            {
                return targetPath; // 如果缓存中有目标路径，直接返回
            }

            try
            {
                IShellLink shellLink = (IShellLink)new ShellLink(); // 创建ShellLink实例
                shellLink.SetPath(linkFilePath); // 设置快捷方式路径

                StringBuilder path = new StringBuilder(260); // 目标路径缓冲区
                int flags = 0; // 解析标志
                shellLink.GetPath(path, path.Capacity, out flags, SLR_NO_UI); // 获取目标路径
                targetPath = path.ToString(); // 获取目标路径

                // 缓存解析结果
                _linkTargetCache[linkFilePath] = targetPath; // 添加到缓存
                return targetPath; // 返回目标路径
            }
            catch
            {
                return null; // 解析失败返回空
            }
        }

        // 获取图标的优化方法
        private ImageSource GetIcon(string filePath)
        {
            string cacheKey = GetCacheKey(filePath); // 文件哈希值作为缓存键           
            if (_iconCache.TryGetValue(cacheKey, out var weakRef) && 
                weakRef.TryGetTarget(out var cachedIcon)) // 检查缓存
            {
                return cachedIcon;
            }
            return null; // 如果图标不存在于缓存中，返回空
        }

        // 如果快捷方式的目标路径无效，使用快捷方式文件本身的图标
        private ImageSource ShellIconFromLink(string linkFilePath)
        {
            // 检查缓存
            string cacheKey = GetCacheKey(linkFilePath);
            if (_iconCache.TryGetValue(cacheKey, out var weakRef) && 
                weakRef.TryGetTarget(out var cachedIcon))
            {
                return cachedIcon;
            }

            try
            {
                // 获取快捷方式的目标路径
                string targetPath = GetTargetPathFromLinkFile(linkFilePath);
                if (!string.IsNullOrEmpty(targetPath))
                {
                    // 从目标路径提取图标
                    var icon1 = ExtractIconFromPath(targetPath);
                    if (icon1 != null)
                    {
                        _iconCache[cacheKey] = new WeakReference<ImageSource>(icon1);
                        return icon1;
                    }
                }

                // 如果目标路径无效，从快捷方式文件本身提取图标
                var icon2 = ExtractIconFromPath(linkFilePath);
                if (icon2 != null)
                {
                    _iconCache[cacheKey] = new WeakReference<ImageSource>(icon2);
                    return icon2;
                }
            }
            catch { } // 忽略图标加载错误
            return null;
        }

        // 获取文件的哈希值作为缓存键
        private string GetCacheKey(string filePath)
        {
            if (_fileHashCache.TryGetValue(filePath, out var cacheKey)) // 检查文件是否已缓存
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

            _fileHashCache[filePath] = cacheKey; // 添加到缓存
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

        /// <summary>
        /// 获取 UWP 应用的图标
        /// </summary>
        /// <param name="packageFamilyName"> 包名 </param>
        /// <returns> 图标 </returns>
        private ImageSource ExtractUWPAppIcon(string packageFamilyName)
        {
            try
            {
                string installPath = GetUWPAppInstallPath(packageFamilyName);
                if (string.IsNullOrEmpty(installPath))
                    return null;

                string manifestPath = Path.Combine(installPath, "AppxManifest.xml");
                if (!File.Exists(manifestPath))
                    return null;

                XDocument doc = XDocument.Load(manifestPath);
                XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                var visualElements = doc.Descendants(ns + "VisualElements").FirstOrDefault();
                if (visualElements == null)
                    return null;

                // 优先级列表
                string[] iconAttrs = { "Square310x310Logo", "Square256x256Logo", "Square150x150Logo", "Square44x44Logo", "Logo" };
                foreach (var attr in iconAttrs)
                {
                    string iconPath = visualElements.Attribute(attr)?.Value;
                    if (string.IsNullOrEmpty(iconPath))
                        continue;

                    iconPath = iconPath.Replace('/', '\\');
                    string directory = Path.GetDirectoryName(Path.Combine(installPath, iconPath));
                    string fileName = Path.GetFileNameWithoutExtension(iconPath);
                    string extension = Path.GetExtension(iconPath);

                    // 优先查找 scale-400, scale-200, scale-100
                    string[] scales = { ".scale-400", ".scale-200", ".scale-100", "" };
                    foreach (var scale in scales)
                    {
                        string scaledPath = Path.Combine(directory, fileName + scale + extension);
                        if (File.Exists(scaledPath))
                        {
                            return LoadIconFromPath(scaledPath);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 UWP 应用的安装目录路径
        /// </summary>
        /// <param name="packageFamilyName"> 包名 </param>
        /// <returns> 安装目录路径 </returns>
        private string GetUWPAppInstallPath(string packageFamilyName)
        {
            try
            {
                var packageManager = new PackageManager(); // 创建包管理器实例
                var package = packageManager.FindPackageForUser("", packageFamilyName); // 获取包信息
                if (package == null)
                    return null; // 包不存在
                return package.InstalledLocation.Path; // 获取包的安装目录路径
            }
            catch
            {
                return null; // 包不存在或其他错误
            }
        }

        /// <summary>
        /// 从文件路径加载图标
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 图标 </returns>
        private ImageSource LoadIconFromPath(string filePath)
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    // 尝试查找带有缩放限定符的文件
                    string directory = Path.GetDirectoryName(filePath);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);

                    // 尝试常见的缩放限定符
                    foreach (var scale in new[] { ".scale-100", ".scale-200", ".scale-400" })
                    {
                        string scaledPath = Path.Combine(directory, fileName + scale + extension);
                        if (File.Exists(scaledPath))
                        {
                            filePath = scaledPath;
                            break;
                        }
                    }

                    // 如果仍然找不到文件，返回null
                    if (!File.Exists(filePath))
                        return null;
                }

                // 使用文件流加载图像
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze(); // 使图像可以跨线程使用
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载图标失败: {ex.Message}");
                return null;
            }
        }

        // 清理资源
        public void Cleanup()
        {
            CleanupIconCache();
            _linkTargetCache?.Clear();
            _fileHashCache?.Clear();
        }

        // 清理图标缓存
        private void CleanupIconCache()
        {
            if (_iconCache != null)
            {
                foreach (var weakRef in _iconCache.Values)
                {
                    if (weakRef.TryGetTarget(out var icon) && icon is IDisposable disposableIcon)
                    {
                        try
                        {
                            disposableIcon.Dispose(); // 释放图标
                        }
                        catch { }
                    }
                }
                _iconCache.Clear(); // 清空图标缓存
            }
        }

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
        public class ShellLink { }
    }

    // 定义应用信息类
    public class AppInfo : INotifyPropertyChanged
    {
        public string Name { get; set; } // 应用名称
        public string Location { get; set; } // 应用路径
        public ImageSource Icon { get; set; } // 应用图标
        public string Tag { get; set; } // 自定义数据

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // 获取应用名称的首字母，用于分组
        public string FirstLetter
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return "#";

                char first = char.ToUpper(Name[0]);
                if (char.IsLetter(first))
                    return first.ToString();
                else
                    return "#";
            }
        }

        public HighlightTextData NameAndSearchText => new HighlightTextData { Text = Name, Keyword = SearchText };
        public HighlightTextData TagAndSearchText => new HighlightTextData { Text = Tag, Keyword = SearchText };
    }
}