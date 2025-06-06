using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using Windows.Management.Deployment;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Windows;
using System.Text;
using System.IO;

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

        // 加载所有应用
        public async Task<List<AppInfo>> LoadAllApplicationsAsync()
        {
            var allApps = new ConcurrentBag<AppInfo>();

            // 使用任务列表来跟踪所有加载任务
            var tasks = new List<Task>();

            // 从注册表加载应用
            tasks.Add(LoadFromRegistryAsync(allApps));

            // 从常见路径加载应用
            //tasks.Add(LoadFromCommonPathsAsync(allApps));

            // 从UWP应用商店加载应用
            //tasks.Add(LoadUWPAppsAsync(allApps));

            // 等待所有加载任务完成
            await Task.WhenAll(tasks);

            // 按应用名称排序并返回
            return allApps.OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        // 从注册表加载应用路径
        private async Task LoadFromRegistryAsync(ConcurrentBag<AppInfo> appsList)
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
                                                var icon = await GetIconAsync(path);
                                                appsList.Add(new AppInfo { Name = appName, Location = path, Icon = icon, Tag = path }); // 添加到应用列表
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

        // 加载应用商店应用
        private async Task LoadUWPAppsAsync(ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                PackageManager packageManager = new(); // 创建包管理器实例
                var packages = packageManager.FindPackagesForUser(""); // 获取所有包信息
                foreach (var package in packages)
                {
                    try
                    {
                        if (package.IsFramework || package.IsResourcePackage || package.IsBundle ||
                            string.IsNullOrWhiteSpace(package.DisplayName) ||
                            package.DisplayName.Equals(package.Id.Name, StringComparison.OrdinalIgnoreCase) ||
                            package.IsDevelopmentMode ||
                            package.Id.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                            package.Id.Name.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // 跳过框架包、资源包、系统组件、没有显示名称或显示名称是包ID、开发模式安装的应用、Microsoft系统应用
                        }

                        var displayName = package.DisplayName; // 获取显示名称
                        var packageFamilyName = package.Id.FamilyName; // 获取包ID

                        // 构建shell:AppsFolder协议使用的应用标识符
                        var appFolderID = packageFamilyName + "_" + packageFamilyName.Split('_')[0];

                        ImageSource icon = await Task.Run(() => ExtractUWPAppIcon(packageFamilyName)); // 获取应用图标

                        appsList.Add(new AppInfo
                        {
                            Name = displayName,
                            Location = appFolderID, // 使用shell:AppsFolder兼容的ID
                            Icon = icon,
                            Tag = "[Windows 商店应用]" // 保存包ID
                        }); // 添加到应用列表
                    }
                    catch { } // 忽略单个包的错误
                }
            }
            catch { } // 忽略异常
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
                // 获取 UWP 应用的安装目录路径
                string installPath = GetUWPAppInstallPath(packageFamilyName);
                if (string.IsNullOrEmpty(installPath))
                    return null;

                // 从 AppxManifest.xml 文件中提取图标路径
                string manifestPath = Path.Combine(installPath, "AppxManifest.xml");
                if (!File.Exists(manifestPath))
                    return null;

                XDocument doc = XDocument.Load(manifestPath);
                XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                var visualElements = doc.Descendants(ns + "VisualElements").FirstOrDefault();
                if (visualElements == null)
                    return null;

                string iconPath = visualElements.Attribute("Square150x150Logo")?.Value
                               ?? visualElements.Attribute("Square44x44Logo")?.Value
                               ?? visualElements.Attribute("Logo")?.Value; // 优先使用 150x150 图标，其次使用 44x44 图标，最后使用 Logo 图标

                if (string.IsNullOrEmpty(iconPath))
                    return null; // 图标路径为空

                // 构建完整的图标文件路径
                iconPath = iconPath.Replace('/', '\\'); // 统一使用反斜杠作为路径分隔符
                string fullIconPath = Path.Combine(installPath, iconPath); // 构建完整的图标文件路径
                return LoadIconFromPath(fullIconPath);// 加载图标
            }
            catch
            {
                return null; // 忽略图标加载错误
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
    public class AppInfo
    {
        public string Name { get; set; } // 应用名称
        public string Location { get; set; } // 应用路径
        public ImageSource Icon { get; set; } // 应用图标
        public string Tag { get; set; } // 自定义数据

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
    }
}