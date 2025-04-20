using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.IO;
using System;
using Quicker;
using Quicker;

namespace Quicker.Managers
{
    internal class IconManager
    {
        // 获取文件图标
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public nint hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
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
        /// <param name="appPath">文件路径</param>
        /// <returns></returns>
        public ImageSource GetIcon(string appPath)
        {
            uint flags = SHGFI_ICON | SHGFI_LARGEICON; // 获取大图标
            nint hIcon = SHGetFileInfo(appPath, FILE_ATTRIBUTE_NORMAL, out SHFILEINFO shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags); // 获取图标句柄
            if (hIcon != nint.Zero)
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); // 创建 BitmapSource
            }
            return null; // 如果获取失败，返回 null
        }

        /// <summary>
        /// 检查缓存的图标
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
            catch { return null; } // 如果保存失败，返回 null
        }

        /// <summary>
        /// 计算图像的哈希值
        /// </summary>
        /// <param name="imageSource"></param>
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
    }
}