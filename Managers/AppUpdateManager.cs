using Quicker.Windows.Forms;
using Quicker.Managers;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Quicker;

public class AppUpdateManager : IDisposable
{
    private const string UpdateInfoUrl = "https://raw.githubusercontent.com/Anonymity3314/Quicker/main/VersionInfo.json"; // 更新信息的 URL 地址
    public UpdateInfo LatestUpdateInfo { get; private set; } // 最新版本信息
    private bool isDisposed = false; // 是否已释放资源

    public AppUpdateManager()
    {
        CheckForUpdate(); // 同步检查更新
    }

    // 同步检查更新
    public void CheckForUpdate()
    {
        LatestUpdateInfo = ReadJsonFromUrl(UpdateInfoUrl); // 读取 JSON 数据
        if (SettingDatabase.currentVersion != LatestUpdateInfo.NewVersion) // 如果当前版本不等于最新版本
        {
            AppStateManager.HasNewVersion = true; // 设置有新版本
            using var toast = new ToastManager(); // 弹窗管理器
            toast.ShowToast($"发现新版本：{LatestUpdateInfo.NewVersion}", "Common"); // 弹窗提示
        }
    }

    /// <summary>
    /// 同步读取 JSON 数据
    /// </summary>
    /// <param name="url"> JSON 数据的 URL 地址 </param>
    /// <returns> 解析后的 UpdateInfo 对象 </returns>
    public UpdateInfo ReadJsonFromUrl(string url)
    {
        try
        {
            using WebClient client = new WebClient(); // 创建 WebClient 实例
            string jsonResponse = client.DownloadString(url); // 同步下载 JSON 数据
            UpdateInfo updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(jsonResponse); // 将 JSON 数据解析为对象
            return updateInfo; // 返回解析后的对象
        }
        catch
        {
            using var toast = new ToastManager(); // 弹窗管理器
            toast.ShowToast("获取更新信息失败！", "Error"); // 弹窗提示
            return null; // 返回 null
        }
    }

    // 释放资源
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // 告知垃圾回收器不需要调用终结器
    }

    // 释放资源
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