using System.Reflection;

namespace Quicker.Helpers
{
    /// <summary>
    /// 应用版本号统一获取工具类
    /// </summary>
    public static class AppVersionHelper
    {
        /// <summary>
        /// 获取当前应用版本号字符串（用于显示和逻辑比较）
        /// 格式统一为三段：主.次.补丁（例如 2.3.0）
        /// </summary>
        public static string CurrentVersion
        {
            get
            {
                try
                {
                    // 尝试获取入口程序集（WPF 程序的 exe）
                    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                    var version = assembly?.GetName().Version;
                    if (version == null)
                    {
                        return "0.0.0";
                    }

                    // 统一返回三段版本：主.次.补丁
                    int major = version.Major;
                    int minor = version.Minor;
                    int build = version.Build >= 0 ? version.Build : 0;

                    return $"{major}.{minor}.{build}";
                }
                catch // 兜底，避免因为反射异常导致程序崩溃
                {
                    return "0.0.0";
                }
            }
        }
    }
}