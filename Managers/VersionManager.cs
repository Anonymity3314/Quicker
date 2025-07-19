using System.Text.Json;

namespace Quicker.Managers
{
    /// <summary>
    /// 版本管理工具类
    /// </summary>
    public static class VersionManager
    {
        /// <summary>
        /// 创建新的版本信息
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="downloadUrl">下载地址</param>
        /// <param name="downloadUrlWithNet">内置.NET运行时下载地址</param>
        /// <param name="changelog">更新日志</param>
        /// <param name="releaseDate">发布日期</param>
        /// <returns>版本信息对象</returns>
        public static AppUpdateManager.UpdateInfo CreateVersionInfo(
            string version,
            string downloadUrl,
            string downloadUrlWithNet,
            string changelog,
            string releaseDate)
        {
            return new AppUpdateManager.UpdateInfo
            {
                Version = version,
                DownloadUrl = downloadUrl,
                DownloadUrlWithNet = downloadUrlWithNet,
                Changelog = changelog,
                ReleaseDate = releaseDate,
                IsLatest = true
            };
        }

        /// <summary>
        /// 添加新版本到版本历史
        /// </summary>
        /// <param name="container">版本信息容器</param>
        /// <param name="newVersion">新版本信息</param>
        /// <returns>更新后的版本信息容器</returns>
        public static AppUpdateManager.UpdateInfoContainer AddNewVersion(
            AppUpdateManager.UpdateInfoContainer container,
            AppUpdateManager.UpdateInfo newVersion)
        {
            if (container == null)
            {
                container = new AppUpdateManager.UpdateInfoContainer
                {
                    Versions = new List<AppUpdateManager.UpdateInfo>()
                };
            }

            // 将之前的最新版本标记为非最新
            foreach (var version in container.Versions)
            {
                version.IsLatest = false;
            }

            // 添加新版本
            container.Versions.Add(newVersion);
            container.LatestVersion = newVersion.Version;

            // 按版本号排序（降序）
            container.Versions = container.Versions
                .OrderByDescending(v => v.Version)
                .ToList();

            return container;
        }

        /// <summary>
        /// 生成版本信息JSON文件内容
        /// </summary>
        /// <param name="container">版本信息容器</param>
        /// <returns>JSON字符串</returns>
        public static string GenerateVersionInfoJson(AppUpdateManager.UpdateInfoContainer container)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(container, options);
        }

        /// <summary>
        /// 验证版本信息
        /// </summary>
        /// <param name="versionInfo">版本信息</param>
        /// <returns>验证结果</returns>
        public static (bool IsValid, string ErrorMessage) ValidateVersionInfo(AppUpdateManager.UpdateInfo versionInfo)
        {
            if (string.IsNullOrWhiteSpace(versionInfo.Version))
            {
                return (false, "版本号不能为空");
            }

            if (string.IsNullOrWhiteSpace(versionInfo.Changelog))
            {
                return (false, "更新日志不能为空");
            }

            if (string.IsNullOrWhiteSpace(versionInfo.ReleaseDate))
            {
                return (false, "发布日期不能为空");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 检查版本号是否已存在
        /// </summary>
        /// <param name="container">版本信息容器</param>
        /// <param name="version">版本号</param>
        /// <returns>是否已存在</returns>
        public static bool IsVersionExists(AppUpdateManager.UpdateInfoContainer container, string version)
        {
            return container?.Versions?.Any(v => v.Version == version) ?? false;
        }

        /// <summary>
        /// 获取版本发布统计信息
        /// </summary>
        /// <param name="container">版本信息容器</param>
        /// <returns>统计信息</returns>
        public static VersionStatistics GetVersionStatistics(AppUpdateManager.UpdateInfoContainer container)
        {
            if (container?.Versions == null || container.Versions.Count == 0)
            {
                return new VersionStatistics();
            }

            var versions = container.Versions;
            var latestVersion = versions.FirstOrDefault(v => v.IsLatest);

            return new VersionStatistics
            {
                TotalVersions = versions.Count,
                LatestVersion = latestVersion?.Version ?? "未知",
                FirstReleaseDate = versions.Min(v => v.ReleaseDate),
                LastReleaseDate = versions.Max(v => v.ReleaseDate)
            };
        }

        /// <summary>
        /// 版本统计信息
        /// </summary>
        public class VersionStatistics
        {
            public int TotalVersions { get; set; }
            public string LatestVersion { get; set; } = "未知";
            public string FirstReleaseDate { get; set; } = "未知";
            public string LastReleaseDate { get; set; } = "未知";
        }

        /// <summary>
        /// 创建示例版本信息容器
        /// </summary>
        /// <returns>示例版本信息容器</returns>
        public static AppUpdateManager.UpdateInfoContainer CreateSampleContainer()
        {
            var container = new AppUpdateManager.UpdateInfoContainer
            {
                LatestVersion = "2.3.0",
                Versions = new List<AppUpdateManager.UpdateInfo>
                {
                    CreateVersionInfo(
                        "2.3.0",
                        "https://github.com/user-attachments/files/20789775/Quicker.2.3.0.zip",
                        "https://github.com/user-attachments/files/20789775/Quicker.2.3.0.zip",
                        "~.新增了多版本更新信息管理\n~.优化了版本比较逻辑\n~.改进了更新检查机制\n~.修复了某些BUG",
                        DateTime.Now.ToString("yyyy-MM-dd")
                    ),
                    CreateVersionInfo(
                        "2.2.0",
                        "https://github.com/user-attachments/files/20789775/Quicker.2.2.0.zip",
                        "https://github.com/user-attachments/files/20789775/Quicker.2.2.0.zip",
                        "~.新增了粘贴地址时自动去除引号的功能\n~.新增了打开动作页类型的动作\n~.新增了动作使用次数统计\n~.新增了动作页编辑功能\n~.新增了检查更新功能\n~.修复了某些BUG",
                        "2025-06-18"
                    )
                }
            };

            // 设置最新版本标记
            container.Versions[0].IsLatest = true;
            container.Versions[1].IsLatest = false;

            return container;
        }

        /// <summary>
        /// 从现有版本信息创建新版本
        /// </summary>
        /// <param name="version">新版本号</param>
        /// <param name="downloadUrl">下载地址</param>
        /// <param name="downloadUrlWithNet">内置.NET运行时下载地址</param>
        /// <param name="changelog">更新日志</param>
        /// <returns>新版本信息</returns>
        public static AppUpdateManager.UpdateInfo CreateNewVersion(
            string version,
            string downloadUrl,
            string downloadUrlWithNet,
            string changelog)
        {
            return CreateVersionInfo(
                version,
                downloadUrl,
                downloadUrlWithNet,
                changelog,
                DateTime.Now.ToString("yyyy-MM-dd")
            );
        }

        /// <summary>
        /// 检查下载地址是否有效
        /// </summary>
        /// <param name="versionInfo">版本信息</param>
        /// <returns>是否有有效的下载地址</returns>
        public static bool HasValidDownloadUrls(AppUpdateManager.UpdateInfo versionInfo)
        {
            return !string.IsNullOrWhiteSpace(versionInfo.DownloadUrl) || 
                   !string.IsNullOrWhiteSpace(versionInfo.DownloadUrlWithNet);
        }

        /// <summary>
        /// 获取可用的下载地址数量
        /// </summary>
        /// <param name="versionInfo">版本信息</param>
        /// <returns>可用下载地址数量</returns>
        public static int GetAvailableDownloadCount(AppUpdateManager.UpdateInfo versionInfo)
        {
            int count = 0;
            if (!string.IsNullOrWhiteSpace(versionInfo.DownloadUrl)) count++;
            if (!string.IsNullOrWhiteSpace(versionInfo.DownloadUrlWithNet)) count++;
            return count;
        }
    }
} 