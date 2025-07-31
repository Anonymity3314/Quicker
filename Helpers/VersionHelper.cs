namespace Quicker.Helpers
{
    /// <summary>
    /// 版本号比较工具类
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// 比较两个版本号
        /// </summary>
        /// <param name="version1">版本号1</param>
        /// <param name="version2">版本号2</param>
        /// <returns>
        /// -1: version1 < version2
        ///  0: version1 == version2
        ///  1: version1 > version2
        /// </returns>
        public static int CompareVersions(string version1, string version2)
        {
            try
            {
                string cleanVersion1 = NormalizeVersion(version1);
                string cleanVersion2 = NormalizeVersion(version2);
                if (Version.TryParse(cleanVersion1, out Version v1) && 
                    Version.TryParse(cleanVersion2, out Version v2))
                {
                    return v1.CompareTo(v2);
                }

                // 如果版本号解析失败，回退到字符串比较
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // 异常情况下回退到字符串比较
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 检查是否有新版本可用
        /// </summary>
        /// <param name="currentVersion">当前版本号</param>
        /// <param name="newVersion">新版本号</param>
        /// <returns>如果有新版本返回true，否则返回false</returns>
        public static bool IsNewVersionAvailable(string currentVersion, string newVersion)
        {
            return CompareVersions(currentVersion, newVersion) < 0;
        }

        /// <summary>
        /// 标准化版本号格式，确保包含四个部分
        /// </summary>
        /// <param name="versionString">原始版本号字符串</param>
        /// <returns>标准化后的版本号字符串</returns>
        private static string NormalizeVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return "0.0.0.0";

            // 确保版本号格式正确（补全缺失的部分）
            var parts = versionString.Split('.');
            if (parts.Length < 2)
            {
                versionString += ".0";
            }
            if (parts.Length < 3)
            {
                versionString += ".0";
            }
            if (parts.Length < 4)
            {
                versionString += ".0";
            }

            return versionString;
        }
    }
} 