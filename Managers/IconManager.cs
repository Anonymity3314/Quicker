using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Managers;
using Quicker.Windows;
using System.Windows;
using System.Net;
using System.IO;

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
        /// 获取图标
        /// </summary>
        /// <param name="appPath"> 文件路径 </param>
        /// <returns> 图标 </returns>
        public ImageSource GetIcon(string appPath)
        {
            uint flags = SHGFI_ICON | SHGFI_LARGEICON;
            nint hIcon = SHGetFileInfo(appPath, FILE_ATTRIBUTE_NORMAL, out SHFILEINFO shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);
            if (hIcon != nint.Zero)
            {
                ImageSource iconSource = Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DestroyIcon(shfi.hIcon); // 显式释放图标资源
                return iconSource;
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
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string iconFileName = GetIconFileName(filePath);
            string iconPath = Path.Combine(appDirectory, "LocalIcons", iconFileName);
            if (File.Exists(iconPath)) return iconPath;
            return null;
        }

        /// <summary>
        /// 获取图标文件名
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 图标文件名 </returns>
        public string GetIconFileName(string filePath)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(filePath); // 将文件路径转换为字节数组
            byte[] hash = SHA256.HashData(bytes); // 计算文件路径的哈希值
            return $"{BitConverter.ToString(hash).Replace("-", "").ToLower()}.png"; // 使用哈希值作为文件名
        }

        /// <summary>
        /// 保存图标到文件
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns> 图标文件路径 </returns>
        public string SaveIconToFile(ImageSource imageSource)
        {
            byte[] imageHash = GetImageHash(imageSource); // 计算图像的哈希值
            if (imageHash == null) return null; // 如果计算失败，返回 null
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // 获取应用程序目录
            string iconFileName = BitConverter.ToString(imageHash).Replace("-", "").ToLower() + ".png"; // 使用哈希值作为文件名
            string iconPath = Path.Combine(appDirectory, "LocalIcons", iconFileName); // 拼接文件路径
            if (File.Exists(iconPath)) return iconPath; // 如果文件存在，直接返回路径
            Directory.CreateDirectory(Path.GetDirectoryName(iconPath)); // 创建目录
            try
            {
                using (FileStream iconStream = new FileStream(iconPath, FileMode.Create)) // 创建文件流
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageSource)); // 将 ImageSource 转换为 BitmapFrame
                    encoder.Save(iconStream); // 保存图像到文件
                }
                return iconPath; // 返回文件路径
            }
            catch
            {
                return null; // 如果保存失败，返回 null
            }
        }

        /// <summary>
        /// 计算图像的哈希值
        /// </summary>
        /// <param name="imageSource"> 图像源 </param>
        /// <returns> 图像的哈希值 </returns>
        public byte[] GetImageHash(ImageSource imageSource)
        {
            BitmapSource bitmapSource = imageSource as BitmapSource; // 将 ImageSource 转换为 BitmapSource
            if (bitmapSource == null) return null; // 如果转换失败，返回 null
            using (MemoryStream stream = new MemoryStream()) // 创建内存流
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder(); // 创建 PNG 编码器
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource)); // 将 BitmapSource 转换为 BitmapFrame
                encoder.Save(stream); // 保存图像到内存流
                return SHA256.HashData(stream.ToArray()); // 计算内存流的哈希值
            }
        }

        /// <summary>
        /// 获取网站图标
        /// </summary>
        /// <param name="websiteUrl"> 网站地址 </param>
        /// <returns> 网站图标 </returns>
        public ImageSource GetWebsiteIcon(string websiteUrl)
        {
            LoadingWindow loadingWindow = new(); // 显示加载窗口
            loadingWindow.Show(); // 显示加载窗口

            Uri uri; // 提取域名部分
            try
            {
                uri = new Uri(websiteUrl); // 尝试解析URL
            }
            catch
            {
                new ToastContentBuilder().AddText("无效的Uri。").Show(); // 处理无效的URL
                loadingWindow?.Close(); // 关闭加载窗口
                return null; // 处理无效的URL
            }

            string apiFaviconUrl = $"https://icon.bqb.cool?url={uri.Host}"; // 拼接第三方API的URL
            using (WebClient client = new WebClient()) // 创建一个WebClient对象来下载图标
            {
                try
                {
                    byte[] iconData = client.DownloadData(apiFaviconUrl); // 下载图标的字节数组
                    BitmapImage bitmapImage = new BitmapImage(); // 将字节数组转换为BitmapImage
                    using (MemoryStream stream = new MemoryStream(iconData))
                    {
                        bitmapImage.BeginInit(); // 开始初始化BitmapImage
                        stream.Seek(0, SeekOrigin.Begin); // 定位到流的开始位置
                        bitmapImage.StreamSource = stream; // 设置流为BitmapImage的源
                        bitmapImage.EndInit(); // 结束初始化BitmapImage
                    }
                    if (IsImageEmpty(bitmapImage)) return null; // 如果图标为空，则返回 null
                    return bitmapImage; // 返回图标
                }
                catch
                {
                    new ToastContentBuilder().AddText("获取网站图标失败。").Show(); // 处理下载失败的情况
                    return null; // 返回空图标
                }
                finally
                {
                    client.Dispose(); // 释放WebClient资源
                    loadingWindow?.Close(); // 关闭加载窗口
                }
            }
        }

        // 判断获取的网站图片是否为空图片
        private bool IsImageEmpty(BitmapImage bitmapImage)
        {
            if (bitmapImage == null || bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0)
                return true; // 如果图片为空，则返回true

            try
            {
                int stride = bitmapImage.PixelWidth * 4; // 计算每行的字节数
                byte[] pixels = new byte[bitmapImage.PixelHeight * stride]; // 创建一个字节数组来存储像素数据

                // 确保图片是 32 位的 RGBA 格式
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(); // 创建FormatConvertedBitmap对象
                formatConvertedBitmap.BeginInit(); // 开始初始化FormatConvertedBitmap
                formatConvertedBitmap.Source = bitmapImage; // 设置源为BitmapImage
                formatConvertedBitmap.DestinationFormat = PixelFormats.Pbgra32; // 转换为 32 位的 RGBA 格式
                formatConvertedBitmap.EndInit(); // 结束初始化FormatConvertedBitmap

                // 检查图片是否为空
                formatConvertedBitmap.CopyPixels(pixels, stride, 0);
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte alpha = pixels[i + 3]; // 获取透明度值
                    if(alpha == 0) return alpha == 0; // 如果透明度值为 0，则返回 true
                }
                return true;
            }
            catch
            {
                return true; // 如果发生异常，假设图片为空
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