using Microsoft.Toolkit.Uwp.Notifications;
using IWshRuntimeLibrary;
using System.Diagnostics;
using Quicker.Database;
using System.IO;

namespace Quicker.Managers
{
    internal class ActionManager
    {
        WindowManager windowManager = new WindowManager(); // 窗口管理器

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenFile(ButtonData data)
        {
            if (data.TryToOpenExitingWindow) // 如果尝试打开已存在的窗口
            {
                string windowTitle = System.IO.Path.GetFileNameWithoutExtension(data.Location);
                windowManager.TryToOpenExitingWindow(windowTitle);
            }

            if (Path.GetExtension(data.Location).Equals(".lnk", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(data.Location).Equals(".exe", StringComparison.OrdinalIgnoreCase)) // 如果是快捷方式或者可执行文件
            {
                string targetPath = Path.GetExtension(data.Location).Equals(".lnk", StringComparison.OrdinalIgnoreCase)
                    ? GetShortcutTargetPath(data.Location)
                    : data.Location; // 获取快捷方式目标路径

                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = targetPath, // 设置启动文件路径
                        UseShellExecute = data.RunByMessager, // 是否使用系统默认方式运行
                        Verb = data.RunByMessager ? "runas" : null, // 管理员权限运行
                        WindowStyle = data.WindowState switch
                        {
                            0 => ProcessWindowStyle.Normal,
                            1 => ProcessWindowStyle.Minimized,
                            2 => ProcessWindowStyle.Maximized
                        } // 设置窗口状态
                    }; // 创建进程启动信息
                    Process.Start(processStartInfo); // 启动进程
                }
                catch (Exception ex)
                {
                    new ToastContentBuilder().AddText($"打开失败：{ex}").Show(); // 显示错误提示
                }
            } // 如果是快捷方式或者可执行文件
            else
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = data.Location,
                        UseShellExecute = true
                    }; // 创建进程启动信息
                    Process.Start(startInfo); // 启动进程
                }
                catch (Exception ex)
                {
                    new ToastContentBuilder().AddText($"打开失败：{ex}").Show();
                }
            } // 使用系统默认方式打开文件
        }

        /// <summary>
        /// 获取快捷方式的目标路径
        /// </summary>
        /// <param name="shortcutFilePath"> 快捷方式文件路径 </param>
        /// <returns> 目标路径 </returns>
        private string GetShortcutTargetPath(string shortcutFilePath)
        {
            WshShell shell = new WshShell(); // 创建WshShell对象
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath); // 创建快捷方式对象
            return shortcut.TargetPath; // 获取快捷方式的目标路径
        }

        // 手动释放资源
        public void Dispose()
        {
            if (windowManager != null) // 释放COM对象
            {
                // 释放窗口管理器中的资源
                windowManager.Dispose();
                windowManager = null;
            }

            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}