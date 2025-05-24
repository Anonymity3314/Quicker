using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
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
                toast.ShowToast("获取图标失败。", "Error"); // 弹出消息提醒
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
                string iconFileName = GetIconFileName(filePath); // 获取图标文件名
                string iconPath = Path.Combine(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons", iconFileName); // 拼接图标文件路径
                return File.Exists(iconPath) ? iconPath : null; // 如果文件存在，返回路径
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("检查缓存图标失败。", "Error"); // 弹出消息提醒
                return null; // 如果出现异常，返回 null
            }
        }


        /// <summary>
        /// 获取图标文件名
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 图标文件名 </returns>
        public string GetIconFileName(string filePath)
        {
            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(filePath); // 计算文件路径的哈希值
                byte[] hash = SHA256.HashData(bytes); // 计算哈希值
                return $"{BitConverter.ToString(hash).Replace("-", "").ToLower()}.png"; // 拼接图标文件名
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("获取图标文件名失败。", "Error"); // 弹出消息提醒
                return null;
            }
        }

        /// <summary>
        /// 保存图标到文件
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns> 图标文件路径 </returns>
        public string SaveIconToFile(ImageSource imageSource)
        {
            try
            {
                byte[] imageHash = GetImageHash(imageSource); // 计算图像的哈希值
                if (imageHash == null) return null; // 如果计算哈希值失败，返回 null

                string iconFileName = BitConverter.ToString(imageHash).Replace("-", "").ToLower() + ".png"; // 拼接图标文件名
                string iconPath = Path.Combine(@"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons", iconFileName); // 拼接图标文件路径

                if (File.Exists(iconPath)) return iconPath; // 如果文件已存在，返回路径
                Directory.CreateDirectory(Path.GetDirectoryName(iconPath)); // 创建图标目录
                using (FileStream iconStream = new FileStream(iconPath, FileMode.Create)) // 创建文件流
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageSource)); // 将 ImageSource 转换为 BitmapFrame
                    encoder.Save(iconStream); // 保存 BitmapFrame 到文件流
                }

                return iconPath; // 返回图标文件路径
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("保存图标失败。", "Error"); // 弹出消息提醒
                return null; // 如果出现异常，返回 null
            }
        }

        /// <summary>
        /// 计算图像的哈希值
        /// </summary>
        /// <param name="imageSource"> 图像源 </param>
        /// <returns> 图像的哈希值 </returns>
        public byte[] GetImageHash(ImageSource imageSource)
        {
            try
            {
                BitmapSource bitmapSource = imageSource as BitmapSource; // 转换为 BitmapSource
                if (bitmapSource == null) return null; // 如果不是 BitmapSource，返回 null
                using (MemoryStream stream = new MemoryStream()) // 创建内存流
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource)); // 将 BitmapSource 转换为 BitmapFrame
                    encoder.Save(stream); // 保存 BitmapFrame 到内存流
                    return SHA256.HashData(stream.ToArray()); // 计算哈希值并返回
                }
            }
            catch
            {
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
            LoadingWindow loadingWindow = new LoadingWindow(); // 创建加载窗口
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
                        toast.ShowToast("获取网站图标失败。", "Error"); // 弹出消息提醒
                        return null; // 如果获取的网站图标为空图片，返回 null
                    }
                    return bitmapImage; // 返回网站图标
                }
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("获取网站图标失败。", "Error"); // 弹出消息提醒
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
        /// 处理图片路径，返回 BitmapImage 对象
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>处理后的 BitmapImage 对象</returns>
        public BitmapImage ProcessIcon(string filePath)
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
                    case ".ico":
                        return LoadBitmapImage(filePath); // 加载普通图片文件
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
                toast.ShowToast("处理图标失败。", "Error"); // 弹出消息提醒
                throw;
            }
        }

        /// <summary>
        /// 加载普通图片文件（如 PNG、JPG）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>加载的 BitmapImage</returns>
        private BitmapImage LoadBitmapImage(string filePath)
        {
            BitmapImage bi = new BitmapImage();
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = stream;
                    bi.EndInit();
                }
                return bi;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载图片时出错: {filePath}", ex);
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
    }
}