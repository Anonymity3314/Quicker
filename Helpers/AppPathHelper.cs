using System.Reflection;
using System.IO;

namespace Quicker.Helpers
{
    /// <summary>
    /// 应用程序路径管理助手类
    /// 统一管理Quicker应用程序的所有路径
    /// </summary>
    public static class AppPathHelper
    {
        /// <summary>
        /// 应用程序数据根目录
        /// </summary>
        public static string AppDataRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Anonymity", "Quicker");

        /// <summary>
        /// 数据库文件夹路径
        /// </summary>
        public static string DatabaseFolder => Path.Combine(AppDataRoot, "Database");

        /// <summary>
        /// 图片文件夹根路径
        /// </summary>
        public static string ImagesFolder => Path.Combine(AppDataRoot, "Images");

        /// <summary>
        /// 本地图标文件夹路径
        /// </summary>
        public static string LocalIconsFolder => Path.Combine(ImagesFolder, "LocalIcons");

        /// <summary>
        /// 背景图片文件夹路径
        /// </summary>
        public static string BackgroundImagesFolder => Path.Combine(ImagesFolder, "BackgroundImages");

        /// <summary>
        /// 外观分享文件夹路径
        /// </summary>
        public static string SharedAppearanceFolder => Path.Combine(ImagesFolder, "SharedAppearance");

        /// <summary>
        /// 临时数据文件夹路径
        /// </summary>
        public static string TempDataFolder => Path.Combine(AppDataRoot, "TempData");

        /// <summary>
        /// 扩展文件夹路径
        /// </summary>
        public static string ExtensionsFolder => Path.Combine(AppDataRoot, "Extensions");

        /// <summary>
        /// 确保目录存在，如果不存在则创建
        /// </summary>
        /// <param name="path">目录路径</param>
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 确保所有应用程序目录都存在
        /// </summary>
        public static void EnsureAllDirectoriesExist()
        {
            EnsureDirectoryExists(AppDataRoot);
            EnsureDirectoryExists(DatabaseFolder);
            EnsureDirectoryExists(ImagesFolder);
            EnsureDirectoryExists(LocalIconsFolder);
            EnsureDirectoryExists(BackgroundImagesFolder);
            EnsureDirectoryExists(SharedAppearanceFolder);
            EnsureDirectoryExists(TempDataFolder);
            EnsureDirectoryExists(ExtensionsFolder);

            //LogDirectories(); // 记录路径
        }

        // 日志记录（修改路径部分）
        private static void LogDirectories()
        {
            var programRoot = AppDomain.CurrentDomain.BaseDirectory; // 运行时路径
            var logPath = Path.Combine(programRoot, "Directories.txt"); // 日志文件路径
            File.WriteAllLines(logPath, new[]
            {
                $"生成时间：{DateTime.Now}",
                $"运行时路径 - 程序：{Assembly.GetEntryAssembly()?.GetName().Name}",
                "",
                "[应用路径]",
                AppDataRoot,
                DatabaseFolder,
                ImagesFolder,
                LocalIconsFolder,
                BackgroundImagesFolder,
                SharedAppearanceFolder,
                TempDataFolder,
                ExtensionsFolder,
                "",
                $"[运行时信息] 程序版本：{Assembly.GetEntryAssembly()?.GetName().Version}"
            });
        }
    }
} 