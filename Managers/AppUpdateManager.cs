using System.Windows.Controls;
using System.ComponentModel;
using Quicker.Windows.Menus;
using System.Windows.Input;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using Quicker.Windows;
using System.Windows;
using System.Net;

public class AppUpdateManager
{
    // 静态实例，确保全局唯一
    public static AppUpdateManager Instance { get; } = new AppUpdateManager();

    // 私有构造函数，防止外部实例化
    private AppUpdateManager()
    {
        // 初始化 AutoUpdater.NET
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
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                DownloadURL = json.url,
                Mandatory = new Mandatory
                {
                    Value = json.mandatory.value,
                    UpdateMode = json.mandatory.mode
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"解析更新信息失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 检查更新的事件处理
    private void CheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (args.IsUpdateAvailable)
            {
                UpdateWindow updateWindow = new UpdateWindow(); // 创建更新窗口
                updateWindow.Show(); // 显示更新窗口
            }
        });
    }

    // 更新下载进度改变的事件处理
    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show($"下载进度：{e.ProgressPercentage}%", "下载进度", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    // 下载完成的事件处理
    private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (e.Error != null)
            {
                MessageBox.Show($"更新失败：{e.Error.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("更新下载完成，将在重启后应用更新。", "更新完成", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomMenu customMenu = Application.Current.Windows.OfType<CustomMenu>().FirstOrDefault(); // 尝试查找现有的菜单栏
                customMenu.Restart(null, null); // 重启应用程序
            }
        });
    }

    // 设置提醒时间
    public void SetRemindLater(TimeSpan remindLaterTime)
    {
        AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
        AutoUpdater.RemindLaterAt = (int)remindLaterTime.TotalMinutes;
    }
}