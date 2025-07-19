using Quicker.Database.Core;
using Quicker.Managers;
using System.Text.Json;
using Quicker.Helpers;
using System.Net;
using System.IO;

public class AppUpdateManager : IDisposable
{
    private const string UpdateInfoUrl = "https://raw.githubusercontent.com/LJZ-Anonymity/Quicker/Quicker/InfoData/VersionInfo.json"; // 更新信息的 URL 地址
    public UpdateInfoContainer LatestUpdateInfo { get; private set; } // 最新版本信息容器
    private bool isDisposed = false; // 是否已释放资源

    // 同步检查更新
    public void CheckForUpdate()
    {
        ReadJsonFromUrl(); // 读取 JSON 数据
        using var toast = new ToastManager(); // 弹窗管理器
        if (LatestUpdateInfo != null)  // 如果有最新版本信息
        {
            var latestVersion = GetLatestVersion(); // 获取最新版本信息
            if (latestVersion != null && VersionHelper.IsNewVersionAvailable(SettingDatabase.currentVersion, latestVersion.Version)) // 使用VersionHelper进行版本比较
            {
                AppStateManager.HasNewVersion = true; // 设置有新版本
                toast.Show($"发现新版本：{latestVersion.Version}", "Common"); // 弹窗提示
            }
        }
        else
        {
            toast.Show("获取更新信息失败！", "Error"); // 弹窗提示
        }
    }

    /// <summary>
    /// 获取最新版本信息
    /// </summary>
    /// <returns>最新版本信息</returns>
    public UpdateInfo GetLatestVersion()
    {
        if (LatestUpdateInfo?.Versions == null || LatestUpdateInfo.Versions.Count == 0)
            return null;
            
        return LatestUpdateInfo.Versions.FirstOrDefault(v => v.IsLatest) ?? 
               LatestUpdateInfo.Versions.OrderByDescending(v => v.Version).First();
    }

    /// <summary>
    /// 获取指定版本信息
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>指定版本信息</returns>
    public UpdateInfo GetVersionInfo(string version)
    {
        if (LatestUpdateInfo?.Versions == null)
            return null;
            
        return LatestUpdateInfo.Versions.FirstOrDefault(v => v.Version == version);
    }

    /// <summary>
    /// 获取版本历史记录
    /// </summary>
    /// <param name="count">返回的版本数量，默认返回所有版本</param>
    /// <returns>版本历史记录</returns>
    public List<UpdateInfo> GetVersionHistory(int count = 0)
    {
        if (LatestUpdateInfo?.Versions == null)
            return new List<UpdateInfo>();
            
        var versions = LatestUpdateInfo.Versions.OrderByDescending(v => v.Version).ToList();
        
        if (count > 0 && count < versions.Count)
        {
            return versions.Take(count).ToList();
        }
        
        return versions;
    }

    // 同步读取 JSON 数据
    public void ReadJsonFromUrl()
    {
        try
        {
            using WebClient client = new(); // 创建 WebClient 实例
            string jsonResponse = client.DownloadString(UpdateInfoUrl); // 同步下载 JSON 数据
            LatestUpdateInfo = JsonSerializer.Deserialize<UpdateInfoContainer>(jsonResponse); // 反序列化 JSON 数据
        }
        catch { }
    }

    // 释放资源
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // 告知垃圾回收器不需要调用终结器
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"> 是否释放托管资源 </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            LatestUpdateInfo = null; // 释放资源
            isDisposed = true;
        }
    }

    ~AppUpdateManager()
    {
        Dispose(false);
    }

    // 更新信息容器类
    public class UpdateInfoContainer
    {
        public string LatestVersion { get; set; } // 最新版本号
        public List<UpdateInfo> Versions { get; set; } = new(); // 版本列表
    }

    // 更新信息类
    public class UpdateInfo
    {
        public string Version { get; set; } // 版本号
        public string DownloadUrl { get; set; } // 下载地址
        public string DownloadUrlWithNet { get; set; } // 下载地址（内置.NET 运行时）
        public string Changelog { get; set; } // 更新日志
        public string ReleaseDate { get; set; } // 发布日期
        public bool IsLatest { get; set; } // 是否为最新版本
    }
}