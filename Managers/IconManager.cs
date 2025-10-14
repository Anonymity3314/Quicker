using Image = System.Windows.Controls.Image;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using Quicker.Windows.ToolWindows;
using System.Windows.Controls;
using System.Windows.Interop;
using Quicker.Windows.Menus;
using System.Windows.Media;
using Quicker.Database;
using System.Net.Http;
using Quicker.Helpers;
using System.Windows;
using WpfAnimatedGif;
using System.Drawing;
using System.Text;
using System.Net;
using System.IO;
using Svg;

namespace Quicker.Managers
{
    internal class IconManager
    {
        #region 字段
        
        // 静态 HttpClient 实例，用于网络请求
        private static readonly HttpClient httpClient = new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, // 启用压缩
            MaxConnectionsPerServer = 2, // 限制每个服务器的连接数
            UseProxy = false, // 禁用代理以提高性能
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true // 忽略SSL证书验证错误
        })
        {
            Timeout = TimeSpan.FromSeconds(10) // 设置超时时间
        };

        // 释放图标资源
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr hIcon); // 释放图标资源

        // 提取图标
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex); // 提取图标

        // 获取文件图标
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public nint hIcon; // 图标句柄
            public int iIcon; // 图标索引
            public uint dwAttributes; // 文件属性
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName; // 文件显示名
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName; // 文件类型名
        } // 文件信息结构体
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern nint SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags); // 获取文件信息

        // 文件图标相关常量
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080; // 文件属性
        private const uint SHGFI_LARGEICON = 0x000000000; // 大图标
        private const uint SHGFI_SMALLICON = 0x000000001; // 小图标
        private const uint SHGFI_ICON = 0x000000100; // 获取图标
        private const uint SHGFI_ICONLOCATION = 0x000001000; // 获取图标位置
        private const uint SHGFI_ATTRIBUTES = 0x000000800; // 获取文件属性

        #endregion

        /// <summary>
        /// 获取应用程序图标（优先获取原始图标）
        /// </summary>
        /// <param name="appPath"> 应用程序路径 </param>
        /// <returns> 应用图标 </returns>
        public ImageSource GetIcon(string appPath)
        {
            try
            {
                // 优先尝试获取原始图标（无压缩标识）
                ImageSource originalIcon = GetOriginalIcon(appPath);
                if (originalIcon != null)
                {
                    return originalIcon;
                }

                // 如果原始图标获取失败，回退到普通方法
                uint flags = SHGFI_ICON | SHGFI_LARGEICON; // 获取大图标
                SHFILEINFO shfi = new SHFILEINFO(); // 创建文件信息结构体
                IntPtr hIcon = SHGetFileInfo(appPath, FILE_ATTRIBUTE_NORMAL, out shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags); // 获取图标句柄
                if (hIcon != IntPtr.Zero) // 如果获取成功
                {
                    ImageSource iconSource = Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); // 将图标句柄转换为ImageSource
                    DestroyIcon(shfi.hIcon); // 释放图标资源
                    return iconSource; // 返回图标
                }
            }
            catch
            {
                ShowToast("获取图标失败。", ToastType.Error); // 显示Toast
                return null; // 如果出现异常，返回 null
            }
            return null; // 如果获取失败，返回 null
        }

        /// <summary>
        /// 获取原始图标（无压缩标识）
        /// </summary>
        /// <param name="appPath"> 应用程序路径 </param>
        /// <returns> 原始图标 </returns>
        public ImageSource GetOriginalIcon(string appPath)
        {
            try
            {
                // 方法1：尝试使用 ExtractIcon API 直接提取
                ImageSource extractedIcon = ExtractIconFromFile(appPath);
                if (extractedIcon != null)
                {
                    return extractedIcon;
                }

                // 方法2：尝试获取原始图标位置
                uint flags = SHGFI_ICONLOCATION | SHGFI_ATTRIBUTES;
                SHFILEINFO shfi = new SHFILEINFO();
                IntPtr result = SHGetFileInfo(appPath, FILE_ATTRIBUTE_NORMAL, out shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);
                if (result != IntPtr.Zero && !string.IsNullOrEmpty(shfi.szDisplayName))
                {
                    // 如果找到了原始图标位置，尝试从原始位置获取图标
                    string originalPath = shfi.szDisplayName;
                    if (File.Exists(originalPath))
                    {
                        return GetIcon(originalPath);
                    }
                }

                // 方法3：尝试从系统目录获取原始文件
                string systemPath = GetSystemOriginalPath(appPath);
                if (!string.IsNullOrEmpty(systemPath) && File.Exists(systemPath))
                {
                    return ExtractIconFromFile(systemPath);
                }

                return null;
            }
            catch
            {
                ShowToast("获取原始图标失败。", ToastType.Error);
                return null;
            }
        }

        /// <summary>
        /// 获取系统原始文件路径
        /// </summary>
        /// <param name="appPath"> 应用程序路径 </param>
        /// <returns> 系统原始路径 </returns>
        private string GetSystemOriginalPath(string appPath)
        {
            try
            {
                string fileName = Path.GetFileName(appPath);
                if (string.IsNullOrEmpty(fileName)) return null;

                // 常见的系统目录
                string[] systemPaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), fileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), fileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", fileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64", fileName)
                };

                foreach (string path in systemPaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch
            {
                // 忽略异常
            }
            return null;
        }

        /// <summary>
        /// 从文件中提取图标（使用 ExtractIcon API）
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 提取的图标 </returns>
        private ImageSource ExtractIconFromFile(string filePath)
        {
            try
            {
                // 使用 ExtractIcon API 获取原始图标
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, filePath, 0);
                if (hIcon != IntPtr.Zero && hIcon != new IntPtr(1))
                {
                    ImageSource iconSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    DestroyIcon(hIcon);
                    return iconSource;
                }
            }
            catch
            {
                return GetIcon(filePath); // 如果 ExtractIcon 失败，回退到普通方法
            }
            return null;
        }

        /// <summary>
        /// 检查缓存的图标
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 图标文件路径 </returns>
        public string CheckCachedIcon(string filePath)
        {
            try
            {
                string hash = GetFileContentHash(filePath); // 获取文件内容哈希值
                if (Directory.Exists(filePath)) // 检查是否为文件夹
                {
                    string iconPath = Path.Combine(AppPathHelper.LocalIconsFolder, $"{hash}.png"); // 拼接图标文件路径
                    return File.Exists(iconPath) ? iconPath : null; // 如果文件存在，返回路径
                }
                else // 文件使用原始扩展名
                {
                    string ext = Path.GetExtension(filePath).ToLower(); // 获取文件扩展名
                    string iconPath = Path.Combine(AppPathHelper.LocalIconsFolder, $"{hash}{ext}"); // 拼接图标文件路径
                    return File.Exists(iconPath) ? iconPath : null; // 如果文件存在，返回路径
                }
            }
            catch
            {
                ShowToast("检查缓存图标失败。", ToastType.Error); // 显示Toast
                return null; // 如果出现异常，返回 null
            }
        }

        /// <summary>
        /// 保存ImageSource为本地图标目录的PNG图片，文件名为内容哈希，避免重复
        /// </summary>
        /// <param name="imageSource">图片源</param>
        /// <returns>保存路径</returns>
        public string SaveIconToFile(ImageSource imageSource)
        {
            try
            {
                BitmapSource bitmapSource = imageSource as BitmapSource; // 将 ImageSource 转换为 BitmapSource
                if (bitmapSource == null) return null; // 如果转换失败，返回 null
                using (MemoryStream stream = new()) // 创建内存流
                {
                    PngBitmapEncoder encoder = new(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource)); // 将 BitmapSource 添加到编码器
                    encoder.Save(stream); // 保存 BitmapSource 到内存流
                    byte[] pngBytes = stream.ToArray(); // 将内存流转换为字节数组
                    // 用内容哈希命名
                    string hash = BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(pngBytes)).Replace("-", "").ToLower();
                    string targetPath = Path.Combine(AppPathHelper.LocalIconsFolder, $"{hash}.png"); // 生成目标路径
                    if (!File.Exists(targetPath)) // 如果目标路径不存在
                    {
                        File.WriteAllBytes(targetPath, pngBytes); // 写入文件
                    }
                    return targetPath; // 返回目标路径
                }
            }
            catch
            {
                ShowToast("保存图标失败。", ToastType.Error); // 显示Toast
                return null; // 如果出现异常，返回 null
            }
        }

        /// <summary>
        /// 获取网站图标
        /// </summary>
        /// <param name="websiteUrl"> 网站地址 </param>
        /// <returns> 网站图标 </returns>
        public ImageSource GetWebsiteIcon(string websiteUrl)
        {
            LoadingWindow loadingWindow = new(); // 创建加载窗口
            loadingWindow.Show(); // 显示加载窗口
            try
            {
                Uri uri = new(websiteUrl); // 创建 Uri 对象
                string apiFaviconUrl = $"https://icon.bqb.cool?url={uri.Host}"; // 拼接 API 地址
                byte[] iconData = httpClient.GetByteArrayAsync(apiFaviconUrl).GetAwaiter().GetResult(); // 使用 HttpClient 下载网站图标数据
                if (iconData == null || iconData.Length == 0) // 验证下载的数据是否为有效的图像
                {
                    ShowToast("获取网站图标失败：数据为空。", ToastType.Error);
                    return null;
                }

                if (!IsValidImageData(iconData)) // 检查图像格式
                {
                    ShowToast("获取网站图标失败：无效的图像格式。", ToastType.Error);
                    return null;
                }

                BitmapImage bitmapImage = new(); // 创建 BitmapImage 对象
                using (MemoryStream stream = new(iconData)) // 创建内存流
                {
                    bitmapImage.BeginInit(); // 开始初始化 BitmapImage
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
                    stream.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                    bitmapImage.StreamSource = stream; // 设置内存流为 BitmapImage 的源
                    bitmapImage.EndInit(); // 结束初始化 BitmapImage
                }

                if (IsImageEmpty(bitmapImage))
                {
                    ShowToast("获取网站图标失败：图标为空。", ToastType.Error); // 显示Toast
                    return null; // 如果获取的网站图标为空图片，返回 null
                }
                return bitmapImage; // 返回网站图标
            }
            catch (HttpRequestException ex)
            {
                ShowToast($"获取网站图标失败：网络错误 - {ex.Message}", ToastType.Error);
                return null;
            }
            catch (NotSupportedException ex)
            {
                ShowToast($"获取网站图标失败：不支持的图像格式 - {ex.Message}", ToastType.Error);
                return null;
            }
            catch (Exception ex)
            {
                ShowToast($"获取网站图标失败：{ex.Message}", ToastType.Error); // 显示Toast
                return null; // 如果出现异常，返回 null
            }
            finally
            {
                loadingWindow?.Close(); // 关闭加载窗口
            }
        }

        /// <summary>
        /// 判断获取的网站图片是否为空图片
        /// </summary>
        /// <param name="bitmapImage"> 网站图标 </param>
        /// <returns> 是否为空图片 </returns>
        private bool IsImageEmpty(BitmapImage bitmapImage)
        {
            try
            {
                if (bitmapImage == null || bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0)
                    return true; // 如果宽度、高度均为 0，则认为是空图片

                int stride = bitmapImage.PixelWidth * 4; // 计算每行像素的字节数
                byte[] pixels = new byte[bitmapImage.PixelHeight * stride]; // 创建字节数组
                FormatConvertedBitmap formatConvertedBitmap = new(); // 创建 FormatConvertedBitmap 对象
                formatConvertedBitmap.BeginInit(); // 开始初始化 FormatConvertedBitmap
                formatConvertedBitmap.Source = bitmapImage; // 设置 BitmapImage 作为源
                formatConvertedBitmap.DestinationFormat = PixelFormats.Pbgra32; // 转换格式
                formatConvertedBitmap.EndInit(); // 转换格式
                formatConvertedBitmap.CopyPixels(pixels, stride, 0); // 复制像素数据到字节数组
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    if (pixels[i + 3] != 0) return false; // 如果有非透明像素，则不是空图片
                }
                return true; // 所有像素的 Alpha 值均为 0，则认为是空图片
            }
            catch
            {
                return true; // 处理异常情况，返回空图片
            }
        }

        /// <summary>
        /// 处理图片路径，返回 BitmapImage 或 BitmapFrame[]（动图）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>处理后的 BitmapImage 或 BitmapFrame[]</returns>
        public object ProcessIcon(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower(); // 获取文件扩展名
                switch (extension)
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                        return LoadBitmapImage(filePath); // 加载普通图片文件
                    case ".ico":
                        return LoadBitmapImage(filePath); // ICO 也用BitmapImage加载，支持多尺寸
                    case ".gif":
                        return LoadGifImage(filePath); // 加载GIF，支持动图
                    case ".svg":
                        return LoadSvgToBitmapImage(filePath); // 加载 SVG 文件
                    case ".exe":
                        return ExtractIconFromExe(filePath); // 从 EXE 文件中提取图标
                    default:
                        throw new NotSupportedException($"不支持的文件格式: {extension}"); // 处理不支持的文件格式
                }
            }
            catch
            {
                ShowToast("处理图标失败。", ToastType.Error); // 显示Toast
                throw;
            }
        }

        /// <summary>
        /// 加载普通图片文件（如 PNG、JPG、JPEG、BMP、ICO）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>加载的 BitmapImage</returns>
        private BitmapImage LoadBitmapImage(string filePath)
        {
            BitmapImage bi = new();
            try
            {
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new(filePath, UriKind.Absolute); // 用UriSource
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载图片时出错: {filePath}", ex);
            }
        }

        /// <summary>
        /// 加载 GIF 文件，支持动图，返回第一帧（BitmapImage）或所有帧（BitmapFrame[]）
        /// </summary>
        /// <param name="filePath">GIF 文件路径</param>
        /// <returns>BitmapImage（静态）或 BitmapFrame[]（动图）</returns>
        private object LoadGifImage(string filePath)
        {
            try
            {
                using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    GifBitmapDecoder decoder = new(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    if (decoder.Frames.Count == 1) // 静态GIF，直接返回BitmapImage
                    {
                        BitmapImage bi = new();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.UriSource = new(filePath, UriKind.Absolute); // 用UriSource
                        bi.EndInit();
                        bi.Freeze();
                        return bi;
                    }
                    else // 动图，返回所有帧
                    {
                        return decoder.Frames.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载GIF图片时出错: {filePath}", ex);
            }
        }

        /// <summary>
        /// 加载 SVG 文件并转换为 BitmapImage
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>加载的 BitmapImage</returns>
        public BitmapImage LoadSvgToBitmapImage(string filePath)
        {
            try
            {
                string svgContent = File.ReadAllText(filePath);// 读取SVG文件内容
                var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);// 解析SVG内容为SvgDocument
                var bitmapSource = RenderSvgToBitmapSource(svgDocument);
                var bitmapImage = ConvertBitmapSourceToBitmapImage(bitmapSource);
                return bitmapImage;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载 SVG 文件时出错: {filePath}", ex);
            }
        }

        /// <summary>
        /// 渲染SvgDocument为BitmapSource
        /// </summary>
        /// <param name="svgDocument">SVG文档</param>
        /// <returns>BitmapSource</returns>
        private BitmapSource RenderSvgToBitmapSource(Svg.SvgDocument svgDocument)
        {
            double width = svgDocument.Width.Value;
            double height = svgDocument.Height.Value;
            DrawingVisual visual = new();
            using (DrawingContext context = visual.RenderOpen())
            {
                using (var bitmap = svgDocument.Draw())
                {
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );
                    context.DrawImage(
                        bitmapSource,
                        new(new System.Windows.Point(0, 0), new System.Windows.Size(width, height))
                    );
                }
            }
            RenderTargetBitmap rtb = new(
                (int)width,
                (int)height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32
            );
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }

        /// <summary>
        /// 将BitmapSource转换为BitmapImage
        /// </summary>
        private BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            BitmapImage bitmapImage = new(); // 创建 BitmapImage 对象
            using (var memoryStream = new MemoryStream()) // 创建内存流
            {
                PngBitmapEncoder encoder = new(); // 创建 PNG 编码器
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource)); // 将 BitmapSource 添加到编码器
                encoder.Save(memoryStream); // 保存 BitmapSource 到内存流
                bitmapImage.BeginInit(); // 开始初始化 BitmapImage
                bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray()); // 设置内存流为 BitmapImage 的源
                bitmapImage.EndInit(); // 结束初始化 BitmapImage
            }
            return bitmapImage;
        }

        /// <summary>
        /// 从 EXE 文件中提取图标
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>提取的图标作为 BitmapImage</returns>
        private BitmapImage ExtractIconFromExe(string filePath)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath); // 提取图标
                return ConvertIconToBitmapImage(icon); // 转换为 BitmapImage
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"从 EXE 文件提取图标时出错: {filePath}", ex);
            }
        }

        /// <summary>
        /// 将 Icon 转换为 BitmapImage
        /// </summary>
        /// <param name="icon">图标</param>
        /// <returns>转换后的 BitmapImage</returns>
        private BitmapImage ConvertIconToBitmapImage(Icon icon)
        {
            try
            {
                using (MemoryStream ms = new()) // 创建内存流
                {
                    icon.Save(ms); // 保存图标到内存流
                    ms.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                    BitmapImage bi = new(); // 创建 BitmapImage 对象
                    bi.BeginInit(); // 开始初始化 BitmapImage
                    bi.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
                    bi.StreamSource = ms; // 设置内存流为 BitmapImage 的源
                    bi.EndInit(); // 结束初始化 BitmapImage
                    return bi; // 返回 BitmapImage
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("转换图标时出错", ex);
            }
        }

        // 手动释放资源
        public void Dispose()
        {
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }

        /// <summary>
        /// 获取文件内容哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件内容哈希值</returns>
        public static string GetFileContentHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create(); // 创建 SHA256 哈希算法
                byte[] hash;
                if (Directory.Exists(filePath)) // 对于文件夹，使用路径字符串的哈希值
                {
                    var pathBytes = Encoding.UTF8.GetBytes(filePath); // 转换为字节数组
                    hash = sha256.ComputeHash(pathBytes); // 计算哈希值
                }
                else // 对于文件，使用文件内容的哈希值
                {
                    using var stream = File.OpenRead(filePath); // 打开文件流
                    hash = sha256.ComputeHash(stream); // 计算哈希值
                }
                return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 返回哈希值字符串
            }
            catch // 如果出现异常，使用路径字符串的哈希值作为后备方案
            {
                using var sha256 = SHA256.Create(); // 创建 SHA256 哈希算法
                var pathBytes = Encoding.UTF8.GetBytes(filePath); // 转换为字节数组
                var hash = sha256.ComputeHash(pathBytes); // 计算哈希值
                return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 返回哈希值字符串
            }
        }

        /// <summary>
        /// 保存图片文件到本地图标目录，文件名为内容哈希，避免重复
        /// </summary>
        /// <param name="filePath">图片文件路径</param>
        /// <returns>保存路径</returns>
        public string SaveImageToLocalIcons(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower(); // 获取文件扩展名
            string hash = GetFileContentHash(filePath); // 获取文件内容哈希值
            string targetPath = Path.Combine(AppPathHelper.LocalIconsFolder, $"{hash}{ext}"); // 生成目标路径
            if (!File.Exists(targetPath)) // 如果目标路径不存在
            {
                File.Copy(filePath, targetPath); // 复制文件
            }
            return targetPath; // 返回目标路径
        }

        /// <summary>
        /// 加载背景图片（支持 GIF 动图、SVG 和普通图片），自动处理 WpfAnimatedGif 兼容性。
        /// </summary>
        /// <param name="imageControl">目标 Image 控件</param>
        /// <param name="path">图片路径</param>
        public void SetImageWithGifSupport(Image imageControl, string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                ClearImage(imageControl); // 清空图片
                return;
            }
            string ext = Path.GetExtension(path).ToLower(); // 获取文件扩展名
            try
            {
                if (ext == ".gif")
                {
                    SetGifImage(imageControl, path); // 设置GIF动图
                }
                else if (ext == ".svg")
                {
                    SetSvgImage(imageControl, path); // 设置SVG图片
                }
                else
                {
                    SetNormalImage(imageControl, path); // 设置普通图片
                }
            }
            catch
            {
                ClearImage(imageControl); // 清空图片
                ShowToast("背景图片设置失败！", ToastType.Error); // 显示Toast
            }
        }

        /// <summary>
        /// 设置 GIF 动图到 Image 控件
        /// </summary>
        /// <param name="imageControl">目标 Image 控件</param>
        /// <param name="path">图片路径</param>
        private void SetGifImage(Image imageControl, string path)
        {
            var bitmap = new BitmapImage(); // 创建BitmapImage对象
            bitmap.BeginInit(); // 开始初始化BitmapImage
            bitmap.UriSource = new Uri(path, UriKind.Absolute); // 设置图片路径
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
            bitmap.EndInit(); // 结束初始化BitmapImage
            bitmap.Freeze(); // 冻结BitmapImage
            ImageBehavior.SetAnimatedSource(imageControl, bitmap); // 设置动画源
        }

        /// <summary>
        /// 设置 SVG 图片到 Image 控件
        /// </summary>
        /// <param name="imageControl">目标 Image 控件</param>
        /// <param name="path">图片路径</param>
        private void SetSvgImage(Image imageControl, string path)
        {
            ImageBehavior.SetAnimatedSource(imageControl, null); // 设置动画源为空
            var svgBitmap = LoadSvgToBitmapImage(path); // 加载SVG图片
            imageControl.Source = svgBitmap; // 设置图片源
        }

        /// <summary>
        /// 设置普通图片到 Image 控件
        /// </summary>
        /// <param name="imageControl">目标 Image 控件</param>
        /// <param name="path">图片路径</param>
        private void SetNormalImage(Image imageControl, string path)
        {
            ImageBehavior.SetAnimatedSource(imageControl, null); // 设置动画源为空
            var bitmap = new BitmapImage(); // 创建BitmapImage对象
            bitmap.BeginInit(); // 开始初始化BitmapImage
            bitmap.UriSource = new Uri(path, UriKind.Absolute); // 设置图片路径
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
            bitmap.EndInit(); // 结束初始化BitmapImage
            bitmap.Freeze(); // 冻结BitmapImage
            imageControl.Source = bitmap; // 设置图片源
        }

        /// <summary>
        /// 清空图片显示
        /// </summary>
        /// <param name="imageControl">目标 Image 控件</param>
        private void ClearImage(Image imageControl)
        {
            imageControl.Source = null; // 设置图片为空
            ImageBehavior.SetAnimatedSource(imageControl, null); // 设置动画源为空
        }

        /// <summary>
        /// 显示Toast
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">标题</param>
        private void ShowToast(string message, ToastType title = ToastType.Error)
        {
            using var toast = new ToastManager(); // 创建ToastManager对象
            toast.Show(message, title); // 显示Toast
        }

        /// <summary>
        /// 验证图像数据是否为有效的图像格式
        /// </summary>
        /// <param name="imageData">图像数据</param>
        /// <returns>是否为有效的图像</returns>
        private bool IsValidImageData(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 8)
                return false;

            // 检查常见图像格式的文件头
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (imageData.Length >= 8 && 
                imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47 &&
                imageData[4] == 0x0D && imageData[5] == 0x0A && imageData[6] == 0x1A && imageData[7] == 0x0A)
                return true;

            // JPEG: FF D8 FF
            if (imageData.Length >= 3 && 
                imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return true;

            // GIF: 47 49 46 38 (GIF8)
            if (imageData.Length >= 4 && 
                imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x38)
                return true;

            // ICO: 00 00 01 00
            if (imageData.Length >= 4 && 
                imageData[0] == 0x00 && imageData[1] == 0x00 && imageData[2] == 0x01 && imageData[3] == 0x00)
                return true;

            // BMP: 42 4D (BM)
            if (imageData.Length >= 2 && 
                imageData[0] == 0x42 && imageData[1] == 0x4D)
                return true;

            return false;
        }
    }
}