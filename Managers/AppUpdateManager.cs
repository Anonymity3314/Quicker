using Quicker.Windows.Forms;
using AutoUpdaterDotNET;
using Quicker.Managers;
using Newtonsoft.Json;
using System.Net;
using Quicker;

public class AppUpdateManager
{
    public static AppUpdateManager Instance { get; } = new();
    public static UpdateInfo LatestUpdateInfo { get; private set; }

    private AppUpdateManager()
    {
        AutoUpdater.ParseUpdateInfoEvent += ParseUpdateInfoEvent;
        AutoUpdater.CheckForUpdateEvent += CheckForUpdateEvent;
        AutoUpdater.Start("https://raw.githubusercontent.com/Anonymity3314/Quicker/main/VersionInfo.json");
    }

    // 解析更新信息的事件处理
    private void ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
    {
        try
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            LatestUpdateInfo = new UpdateInfo
            {
                NewVersion = json.version, // 当前版本
                DownloadUrl = json.url, // 下载地址
                Changelog = json.changelog, // 更新日志
                ReleaseDate = json.releaseDate // 发布日期
            };
        }
        catch
        {
            using var toast = new ToastManager(); // 弹窗管理器
            toast.ShowToast("解析更新信息失败!", "Error"); // 弹窗提示
        }
    }

    // 检查更新的事件处理
    private void CheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.IsUpdateAvailable)
        {
            AppStateManager.HasNewVersion = true; // 设置有新版本
            using var toast = new ToastManager(); // 弹窗管理器
            toast.ShowToast($"发现新版本：{args.CurrentVersion}", "Common"); // 弹窗提示
        }
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