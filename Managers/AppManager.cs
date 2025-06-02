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
    /// <summary>
    /// 应用管理器，负责加载和管理应用信息
    /// </summary>
    public class AppManager
    {
        #region 字段和常量

        private static readonly ConcurrentDictionary<string, ImageSource> _iconCache = new(); // 图标缓存
        private ConcurrentDictionary<string, string> _linkTargetCache = new(); // 快捷方式目标路径缓存
        private ConcurrentDictionary<string, string> _fileHashCache = new(); // 文件哈希缓存
        
        private const int SLR_NO_UI = 0x00000001; // 在解析快捷方式时不显示用户界面
        private const int MAX_PATH = 260; // 最大路径长度
        private const int MAX_FOLDER_DEPTH = 2; // 文件夹递归最大深度
        private const int MAX_FILE_SIZE_FOR_HASH = 1024 * 1024; // 计算哈希的最大文件大小 (1MB)

        #endregion

        #region 构造函数

        // 构造函数
        public AppManager() { }

        #endregion

        #region 公共方法

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearCache()
        {
            _iconCache.Clear();
            _linkTargetCache.Clear();
            _fileHashCache.Clear();
        }

        /// <summary>
        /// 异步加载所有应用
        /// </summary>
        /// <returns>应用信息列表</returns>
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

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            CleanupIconCache();
            _linkTargetCache?.Clear();
            _fileHashCache?.Clear();
        }

        #endregion

        #region 应用加载方法

        // 从注册表加载应用路径
        private async Task LoadFromRegistryAsync(ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
                {
                    if (key == null) return;
                    
                    string[] subKeyNames = key.GetSubKeyNames(); // 获取所有子键名称
                    
                    // 创建并行任务来处理每个注册表项
                    var loadTasks = new List<Task>();
                    foreach (string subKeyName in subKeyNames)
                    {
                        loadTasks.Add(Task.Run(async () => {
                            try
                            {
                                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) return;
                                    
                                    string path = (string)subKey.GetValue(""); // 获取默认值
                                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
                                    
                                    string appName = Path.GetFileNameWithoutExtension(path);
                                    var icon = await GetIconAsync(path);
                                    appsList.Add(new AppInfo { 
                                        Name = appName, 
                                        Location = path, 
                                        Icon = icon, 
                                        Tag = path 
                                    });
                                }
                            }
                            catch { } // 忽略单个注册表项的错误
                        }));
                    }
                    
                    // 等待所有任务完成
                    await Task.WhenAll(loadTasks);
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

            var tasks = new List<Task>();
            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path))
                {
                    tasks.Add(EnumerateFilesAsync(path, new[] { ".exe", ".lnk" }, MAX_FOLDER_DEPTH, appsList));
                }
            }
            await Task.WhenAll(tasks);
        }

        // 递归遍历文件夹（异步版本）
        private async Task EnumerateFilesAsync(string directoryPath, string[] allowedExtensions, int maxDepth, ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                // 处理当前目录中的文件
                var fileTasks = new List<Task>();
                foreach (var file in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        fileTasks.Add(LoadApplicationAsync(file, appsList));
                    }
                }
                
                // 递归处理子目录
                if (maxDepth > 0)
                {
                    var dirTasks = new List<Task>();
                    foreach (var subDir in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        dirTasks.Add(EnumerateFilesAsync(subDir, allowedExtensions, maxDepth - 1, appsList));
                    }
                    
                    if (dirTasks.Count > 0)
                    {
                        await Task.WhenAll(dirTasks);
                    }
                }
                
                if (fileTasks.Count > 0)
                {
                    await Task.WhenAll(fileTasks);
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
                string extension = Path.GetExtension(filePath).ToLower();
                
                if (extension == ".lnk") // 解析快捷方式
                {
                    appName = Path.GetFileNameWithoutExtension(filePath);
                    icon = await Task.Run(() => ShellIconFromLink(filePath));
                }
                else if (extension == ".exe") // 处理可执行文件
                {
                    appName = Path.GetFileNameWithoutExtension(filePath);
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
                    appsList.Add(new AppInfo { 
                        Name = appName, 
                        Location = location, 
                        Icon = icon, 
                        Tag = location 
                    });
                }
            }
            catch { } // 忽略异常
        }

        // 加载应用商店应用
        private async Task LoadUWPAppsAsync(ConcurrentBag<AppInfo> appsList)
        {
            try
            {
                PackageManager packageManager = new(); // 创建包管理器实例
                var packages = packageManager.FindPackagesForUser(""); // 获取所有包信息
                
                var loadTasks = new List<Task>();
                
                foreach (var package in packages)
                {
                    loadTasks.Add(Task.Run(async () => {
                        try
                        {
                            // 跳过系统组件和不需要的包
                            if (ShouldSkipPackage(package))
                            {
                                return;
                            }

                            var displayName = package.DisplayName; // 获取显示名称
                            var packageFamilyName = package.Id.FamilyName; // 获取包ID
                            
                            // 构建shell:AppsFolder协议使用的应用标识符
                            var appFolderID = $"{packageFamilyName}_{packageFamilyName.Split('_')[0]}";
                            
                            ImageSource icon = await Task.Run(() => ExtractUWPAppIcon(packageFamilyName));
                            
                            appsList.Add(new AppInfo
                            {
                                Name = displayName,
                                Location = appFolderID, // 使用shell:AppsFolder兼容的ID
                                Icon = icon,
                                Tag = "[Windows 商店应用]" // 保存包ID
                            });
                        }
                        catch { } // 忽略单个包的错误
                    }));
                }
                
                await Task.WhenAll(loadTasks);
            }
            catch { } // 忽略异常
        }

        #endregion

        #region 图标处理方法

        // 异步加载图标
        private async Task<ImageSource> GetIconAsync(string filePath)
        {
            if (_iconCache.TryGetValue(filePath, out var icon))
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
                _iconCache[filePath] = icon;
            }
            return icon;
        }

        // 获取图标的优化方法
        private ImageSource GetIcon(string filePath)
        {
            string cacheKey = GetCacheKey(filePath); // 文件哈希值作为缓存键           
            if (_iconCache.TryGetValue(cacheKey, out var icon)) // 检查缓存
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
            if (_iconCache.TryGetValue(cacheKey, out var icon))
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
                        _iconCache[cacheKey] = icon;
                        return icon;
                    }
                }

                // 如果目标路径无效，从快捷方式文件本身提取图标
                icon = ExtractIconFromPath(linkFilePath);
                if (icon != null)
                {
                    _iconCache[cacheKey] = icon;
                    return icon;
                }
            }
            catch { } // 忽略图标加载错误
            return null;
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
        /// <param name="packageFamilyName">包名</param>
        /// <returns>图标</returns>
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

                string iconPath = GetIconPathFromVisualElements(visualElements);
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
        /// 从文件路径加载图标
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>图标</returns>
        private ImageSource LoadIconFromPath(string filePath)
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    filePath = TryFindScaledIconPath(filePath);
                    
                    // 如果仍然找不到文件，返回null
                    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
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

        #endregion

        #region 辅助方法

        // 添加应用前检查是否已存在
        private bool IsAppAlreadyAdded(string appName, string location, ConcurrentBag<AppInfo> appsList)
        {
            return appsList.Any(app => 
                app.Name.Equals(appName, StringComparison.OrdinalIgnoreCase) && 
                app.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
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

                StringBuilder path = new StringBuilder(MAX_PATH); // 目标路径缓冲区
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
            if (fileInfo.Length < MAX_FILE_SIZE_FOR_HASH)
            {
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    cacheKey = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
            else
            {               
                cacheKey = $"{fileInfo.Length}_{fileInfo.LastWriteTimeUtc.Ticks}";
            }

            _fileHashCache[filePath] = cacheKey; // 添加到缓存
            return cacheKey;
        }

        // 清理图标缓存
        private void CleanupIconCache()
        {
            if (_iconCache != null)
            {
                foreach (var icon in _iconCache.Values)
                {
                    if (icon is IDisposable disposableIcon)
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

        // 判断是否应该跳过此UWP包
        private bool ShouldSkipPackage(Windows.ApplicationModel.Package package)
        {
            return package.IsFramework || 
                   package.IsResourcePackage || 
                   package.IsBundle ||
                   string.IsNullOrWhiteSpace(package.DisplayName) ||
                   package.DisplayName.Equals(package.Id.Name, StringComparison.OrdinalIgnoreCase) ||
                   package.IsDevelopmentMode ||
                   package.Id.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                   package.Id.Name.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase);
        }

        // 从VisualElements中获取图标路径
        private string GetIconPathFromVisualElements(XElement visualElements)
        {
            return visualElements.Attribute("Square150x150Logo")?.Value
                ?? visualElements.Attribute("Square44x44Logo")?.Value
                ?? visualElements.Attribute("Logo")?.Value;
        }

        // 尝试查找带有缩放限定符的图标文件
        private string TryFindScaledIconPath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            
            // 尝试常见的缩放限定符
            foreach (var scale in new[] { ".scale-100", ".scale-200", ".scale-400" })
            {
                string scaledPath = Path.Combine(directory, fileName + scale + extension);
                if (File.Exists(scaledPath))
                {
                    return scaledPath;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 获取 UWP 应用的安装目录路径
        /// </summary>
        /// <param name="packageFamilyName">包名</param>
        /// <returns>安装目录路径</returns>
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

        #endregion

        #region COM接口定义

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

        #endregion
    }

    /// <summary>
    /// 应用信息类，存储应用的基本信息
    /// </summary>
    public class AppInfo
    {
        #region 属性

        /// <summary>
        /// 应用名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 应用路径
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// 应用图标
        /// </summary>
        public ImageSource Icon { get; set; }
        
        /// <summary>
        /// 自定义数据
        /// </summary>
        public string Tag { get; set; }
        
        /// <summary>
        /// 获取应用名称的首字母，用于分组
        /// </summary>
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

        #endregion
    }
} 