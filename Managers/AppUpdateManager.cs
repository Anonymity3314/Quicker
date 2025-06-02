using Quicker.Managers;
using System.Text.Json;
using System.Net;
using System.IO;
using Quicker;

public class AppUpdateManager : IDisposable
{
    private const string UpdateInfoUrl = "https://raw.githubusercontent.com/LJZ-Anonymity/Quicker/Quicker/InfoData/VersionInfo.json"; // 更新信息的 URL 地址
    public UpdateInfo LatestUpdateInfo { get; private set; } // 最新版本信息
    private bool isDisposed = false; // 是否已释放资源

    // 同步检查更新
    public void CheckForUpdate()
    {
        ReadJsonFromUrl(); // 读取 JSON 数据
        using var toast = new ToastManager(); // 弹窗管理器
        if (LatestUpdateInfo != null)  // 如果有最新版本信息
        {
            if (SettingDatabase.currentVersion != LatestUpdateInfo.NewVersion) // 如果当前版本不等于最新版本
            {
                AppStateManager.HasNewVersion = true; // 设置有新版本
                toast.Show($"发现新版本：{LatestUpdateInfo.NewVersion}", "Common"); // 弹窗提示
            }
        }
        else
        {
            toast.Show("获取更新信息失败！", "Error"); // 弹窗提示
        }
    }

    // 同步读取 JSON 数据
    public void ReadJsonFromUrl()
    {
        try
        {
            using WebClient client = new(); // 创建 WebClient 实例
            string jsonResponse = client.DownloadString(UpdateInfoUrl); // 同步下载 JSON 数据
            LatestUpdateInfo = JsonSerializer.Deserialize<UpdateInfo>(jsonResponse); // 反序列化 JSON 数据
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

    // 更新信息类
    public class UpdateInfo
    {
        public string NewVersion { get; set; } // 版本号
        public string DownloadUrl { get; set; } // 下载地址
        public string Changelog { get; set; } // 更新日志
        public string ReleaseDate { get; set; } // 发布日期
    }
}