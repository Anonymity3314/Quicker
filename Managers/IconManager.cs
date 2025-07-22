using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Media.Imaging;
using Quicker.Windows.ToolWindows;
using System.Windows.Controls;
using System.Windows.Interop;
using Quicker.Windows.Menus;
using System.Windows.Media;
using Quicker.Database;
using System.Windows;
using WpfAnimatedGif;
using System.Drawing;
using System.Net;
using System.IO;
using Svg;

namespace Quicker.Managers
{
    internal class IconManager
    {
        // 释放图标资源
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr hIcon); // 释放图标资源

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

        /// <summary>
        /// 获取应用程序图标
        /// </summary>
        /// <param name="appPath"> 应用程序路径 </param>
        /// <returns> 应用图标 </returns>
        public ImageSource GetIcon(string appPath)
        {
            try
            {
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
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("获取图标失败。", "Error"); // 弹出消息提醒
                return null; // 如果出现异常，返回 null
            }
            return null; // 如果获取失败，返回 null
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
                string ext = Path.GetExtension(filePath).ToLower(); // 获取文件扩展名
                string hash = GetFileContentHash(filePath); // 获取文件内容哈希值
                string iconPath = Path.Combine(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons", $"{hash}{ext}"); // 拼接图标文件路径
                return File.Exists(iconPath) ? iconPath : null; // 如果文件存在，返回路径
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("检查缓存图标失败。", "Error"); // 弹出消息提醒
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
                using (MemoryStream stream = new MemoryStream()) // 创建内存流
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource)); // 将 BitmapSource 添加到编码器
                    encoder.Save(stream); // 保存 BitmapSource 到内存流
                    byte[] pngBytes = stream.ToArray(); // 将内存流转换为字节数组
                    // 用内容哈希命名
                    string hash = BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(pngBytes)).Replace("-", "").ToLower();
                    string saveDir = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons"; // 保存路径
                    string targetPath = Path.Combine(saveDir, $"{hash}.png"); // 生成目标路径
                    if (!File.Exists(targetPath)) // 如果目标路径不存在
                    {
                        File.WriteAllBytes(targetPath, pngBytes); // 写入文件
                    }
                    return targetPath; // 返回目标路径
                }
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("保存图标失败。", "Error"); // 弹出消息提醒
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
                Uri uri = new Uri(websiteUrl); // 创建 Uri 对象
                string apiFaviconUrl = $"https://icon.bqb.cool?url={uri.Host}"; // 拼接 API 地址
                using (WebClient client = new WebClient()) // 创建 WebClient 对象
                {
                    byte[] iconData = client.DownloadData(apiFaviconUrl); // 下载网站图标数据
                    BitmapImage bitmapImage = new BitmapImage(); // 创建 BitmapImage 对象
                    using (MemoryStream stream = new MemoryStream(iconData)) // 创建内存流
                    {
                        bitmapImage.BeginInit(); // 开始初始化 BitmapImage
                        stream.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                        bitmapImage.StreamSource = stream; // 设置内存流为 BitmapImage 的源
                        bitmapImage.EndInit(); // 结束初始化 BitmapImage
                    }

                    if (IsImageEmpty(bitmapImage))
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.Show("获取网站图标失败。", "Error"); // 弹出消息提醒
                        return null; // 如果获取的网站图标为空图片，返回 null
                    }
                    return bitmapImage; // 返回网站图标
                }
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("获取网站图标失败。", "Error"); // 弹出消息提醒
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
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(); // 创建 FormatConvertedBitmap 对象
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
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("处理图标失败。", "Error"); // 弹出消息提醒
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
            BitmapImage bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(filePath, UriKind.Absolute); // 用UriSource
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
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    GifBitmapDecoder decoder = new GifBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    if (decoder.Frames.Count == 1) // 静态GIF，直接返回BitmapImage
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.UriSource = new Uri(filePath, UriKind.Absolute); // 用UriSource
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
                string svgContent = File.ReadAllText(filePath); // 读取 SVG 文件内容
                SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent); // 使用 SvgDocument 解析 SVG 内容
                // 计算 SVG 图像的尺寸
                double width = svgDocument.Width.Value; // 宽度
                double height = svgDocument.Height.Value; // 高度
                DrawingVisual visual = new DrawingVisual(); // 创建一个用于渲染的 Visual
                using (DrawingContext context = visual.RenderOpen())
                {
                    Bitmap bitmap = svgDocument.Draw(); // 将 SVG 图像绘制到 Visual 上下文中
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );
                    context.DrawImage(
                        bitmapSource,
                        new System.Windows.Rect(new System.Windows.Point(0, 0), new System.Windows.Size(width, height))
                    );
                }
                // 创建 RenderTargetBitmap 并渲染 Visual
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)width,
                    (int)height,
                    96, // DPI X
                    96, // DPI Y
                    PixelFormats.Pbgra32
                );
                rtb.Render(visual); // 渲染 Visual
                rtb.Freeze(); // 冻结 RenderTargetBitmap

                BitmapImage bitmapImage = new BitmapImage(); // 创建 BitmapImage 对象
                using (var memoryStream = new MemoryStream()) // 创建内存流
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create(rtb)); // 将 RenderTargetBitmap 转换为 BitmapFrame
                    encoder.Save(memoryStream); // 保存图像到内存流
                    bitmapImage.BeginInit(); // 开始初始化 BitmapImage
                    bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray()); // 设置内存流为 BitmapImage 的源
                    bitmapImage.EndInit(); // 结束初始化 BitmapImage
                }
                return bitmapImage; // 返回 BitmapImage
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载 SVG 文件时出错: {filePath}", ex);
            }
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
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath); // 提取图标
                return ConvertIconToBitmapImage(icon); // 转换为 BitmapImage
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"从 EXE 文件提取图标时出错: {filePath}", ex);
            }
        }

        /// <summary>
        /// 将 System.Drawing.Icon 转换为 BitmapImage
        /// </summary>
        /// <param name="icon">图标</param>
        /// <returns>转换后的 BitmapImage</returns>
        private BitmapImage ConvertIconToBitmapImage(System.Drawing.Icon icon)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream()) // 创建内存流
                {
                    icon.Save(ms); // 保存图标到内存流
                    ms.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                    BitmapImage bi = new BitmapImage(); // 创建 BitmapImage 对象
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
            using var stream = File.OpenRead(filePath); // 打开文件流
            using var sha256 = System.Security.Cryptography.SHA256.Create(); // 创建SHA256哈希算法
            var hash = sha256.ComputeHash(stream); // 计算哈希值
            return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 返回哈希值
        }

        /// <summary>
        /// 保存图片文件到本地图标目录，文件名为内容哈希，避免重复
        /// </summary>
        /// <param name="filePath">图片文件路径</param>
        /// <returns>保存路径</returns>
        public string SaveImageToLocalIcons(string filePath)
        {
            string saveDir = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons"; // 保存路径
            string ext = Path.GetExtension(filePath).ToLower(); // 获取文件扩展名
            string hash = GetFileContentHash(filePath); // 获取文件内容哈希值
            string targetPath = Path.Combine(saveDir, $"{hash}{ext}"); // 生成目标路径
            if (!File.Exists(targetPath)) // 如果目标路径不存在
            {
                File.Copy(filePath, targetPath, true); // 复制文件
            }
            return targetPath; // 返回目标路径
        }
    }
}