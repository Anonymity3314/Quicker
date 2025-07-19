using System.Runtime.InteropServices;
using System.Diagnostics;
using IWshRuntimeLibrary;
using Quicker.Database;
using Microsoft.Win32;
using Quicker.Windows;
using Quicker.Extend;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.Managers
{
    public class ActionManager : IDisposable
    {
        private const string tempDir = "C:\\Users\\LENOVO\\AppData\\Roaming\\Anonymity\\Quicker\\TempData"; // 临时目录
        private const string extensionDir = "C:\\Users\\LENOVO\\AppData\\Roaming\\Anonymity\\Quicker\\Extensions"; // 扩展目录
        private bool isDisposed = false; // 是否释放

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenFile(ButtonData data)
        {
            if (!Directory.Exists(tempDir)) // 如果临时目录不存在，则创建
                Directory.CreateDirectory(tempDir);

            if (data.Data2 == "true") // 如果尝试打开已存在的窗口
            {
                string windowTitle = System.IO.Path.GetFileNameWithoutExtension(data.Location);
                using var windowManager = new WindowManager(); // 创建窗口管理器
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
                    string fileDirectory = Path.GetExtension(data.Location).Equals(".lnk", StringComparison.OrdinalIgnoreCase)
                        ? Path.GetDirectoryName(GetShortcutTargetPath(data.Location))
                        : Path.GetDirectoryName(data.Location); // 获取文件所在的目录

                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = targetPath, // 设置启动文件路径
                        UseShellExecute = data.Data1 == "True", // 是否使用系统默认方式运行
                        Verb = data.Data1 == "True" ? "runas" : null, // 管理员权限运行
                        WorkingDirectory = fileDirectory ?? tempDir, // 设置工作目录为文件所在的目录，如果获取失败则使用临时目录
                        WindowStyle = int.Parse(data.Data3) switch
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
                    using var toast = new ToastManager(); // 消息提醒管理器
                    toast.Show($"打开失败：{ex}", "Error"); // 弹出消息提醒
                }
            } // 如果是快捷方式或者可执行文件
            else
            {
                try
                {
                    string fileDirectory = Path.GetDirectoryName(data.Location); // 获取文件所在的目录
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = data.Location,
                        WorkingDirectory = fileDirectory ?? tempDir, // 设置工作目录为文件所在的目录，如果获取失败则使用临时目录
                        UseShellExecute = true
                    }; // 创建进程启动信息
                    Process.Start(startInfo); // 启动进程
                }
                catch (Exception ex)
                {
                    using var toast = new ToastManager(); // 消息提醒管理器
                    toast.Show($"打开失败：{ex}", "Error"); // 弹出消息提醒
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

        /// <summary>
        /// 用指定方式打开指定网站
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenWebsite(ButtonData data)
        {
            try
            {
                switch (int.Parse(data.Data1))
                {
                    case 0:
                        LaunchDefaultBrowser(data.Location);
                        break; // 打开默认浏览器
                    case 1:
                        LaunchInternetExplorer(data.Location);
                        break; // 打开IE浏览器
                    case 2:
                        LaunchMicrosoftEdge(data.Location);
                        break; // 打开Edge浏览器
                    case 3:
                        LaunchEdgeAppMode(data.Location);
                        break; // 打开Edge浏览器，并以App模式打开
                    case 4:
                        LaunchEdgeInPrivateMode(data.Location);
                        break; // 打开Edge浏览器，并以InPrivate模式打开
                    case 5:
                        LaunchChrome(data.Location);
                        break; // 打开Chrome浏览器
                    case 6:
                        LaunchChromeAppMode(data.Location);
                        break; // 打开Chrome浏览器，并以APP模式打开
                    case 7:
                        LaunchChromeIncognitoMode(data.Location);
                        break; // 打开Chrome浏览器,并以无痕模式打开
                    case 8:
                        LaunchCustomBrowser(data.Location);
                        break; // 通过用户指定浏览器打开
                }
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show($"打开网站失败：{ex.Message}", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 打开默认浏览器
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        public void LaunchDefaultBrowser(string website)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = website,
                UseShellExecute = true
            }); // 启动默认浏览器
        }

        /// <summary>
        /// 打开IE浏览器
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchInternetExplorer(string website)
        {
            Process.Start("ieplore.exe", website); // 启动IE浏览器
        }

        /// <summary>
        /// 打开Microsoft Edge浏览器
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchMicrosoftEdge(string website)
        {
            Process.Start("microsoft-edge:" + website); // 启动Edge浏览器
        }

        /// <summary>
        /// 打开Edge浏览器，并以App模式打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchEdgeAppMode(string website)
        {
            string edgePath = GetEdgeBrowserPath(); // 获取Edge浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = edgePath,
                Arguments = $"--app={website}",
                UseShellExecute = true
            }); // 启动Edge浏览器，并以App模式打开
        }

        /// <summary>
        /// 打开Edge浏览器，并以InPrivate模式打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchEdgeInPrivateMode(string website)
        {
            string edgePath = GetEdgeBrowserPath(); // 获取Edge浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = edgePath,
                Arguments = $"--inprivate {website}",
                UseShellExecute = true
            }); // 启动Edge浏览器，并以InPrivate模式打开
        }

        /// <summary>
        /// 打开Chrome浏览器
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchChrome(string website)
        {
            string chromePath = GetChromeBrowserPath(); // 获取Chrome浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = website,
                UseShellExecute = true
            }); // 启动Chrome浏览器
        }

        /// <summary>
        /// 打开Chrome浏览器，并以App模式打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchChromeAppMode(string website)
        {
            string chromePath = GetChromeBrowserPath(); // 获取Chrome浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--app={website}",
                UseShellExecute = true
            }); // 启动Chrome浏览器，并以App模式打开
        }

        /// <summary>
        /// 打开Chrome浏览器，并以无痕模式打开指定网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchChromeIncognitoMode(string website)
        {
            string chromePath = GetChromeBrowserPath(); // 获取Chrome浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"-incognito {website}",
                UseShellExecute = true
            }); // 启动Chrome浏览器，并以无痕模式打开
        }

        /// <summary>
        /// 通过用户指定浏览器打开网站
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchCustomBrowser(string location)
        {
            string[] processNames = location.Split(';'); // 将文本内容按照分号分隔
            string website = processNames[0]; // 获取网站地址
            string browserPath = processNames[1]; // 获取浏览器路径
            Process.Start(new ProcessStartInfo
            {
                FileName = browserPath,
                Arguments = website,
                UseShellExecute = true
            }); // 启动用户指定浏览器打开网站
        }

        // 获取Edge浏览器路径
        private string GetEdgeBrowserPath()
        {
            try // 通过注册表获取Edge浏览器路径
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\CurrentVersion\App Model}");

                if (key != null)
                {
                    object value = key.GetValue("Edge"); // 获取Edge浏览器路径
                    if (value != null)
                        return value.ToString(); // 返回Edge浏览器路径
                }
            }
            catch { }
            return @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"; // 如果注册表中找不到，使用默认路径
        }

        // 获取Chrome浏览器路径
        private string GetChromeBrowserPath()
        {
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // 默认路径
            if (!System.IO.File.Exists(chromePath))
            {
                chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"; // 32位路径
            }
            return chromePath; // 返回Chrome浏览器路径
        }

        /// <summary>
        /// 打开多个文件
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenFiles(ButtonData data)
        {
            string[] files = data.Location.Split(';'); // 将文本内容按照分号分隔
            foreach (string file in files)
            {
                OpenFile(new ButtonData { Location = file, Data2 = false.ToString() }); // 打开文件
            }
        }

        /// <summary>
        /// 打开UWP应用
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenUwpApp(ButtonData data)
        {
            try
            {
                // 使用shell:AppsFolder协议启动UWP应用
                string appPath = $"shell:AppsFolder\\{data.Location}";
                ShellExecute(IntPtr.Zero, "open", appPath, null, null, 1);
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show($"打开UWP应用失败：{ex.Message}", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 加载扩展
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void LoadExtension(ButtonData data)
        {
            string dllPath = data.Location; // 直接获取DLL文件路径
            if (!System.IO.File.Exists(dllPath)) // 判断DLL文件是否存在
            {
                using var toast = new ToastManager();
                toast.Show($"扩展文件不存在: {dllPath}", "Error");
                return;
            }
            else
            {
                ModuleLoader moduleLoader = new(); // 创建模块加载器
                moduleLoader.LoadModule(dllPath); // 加载扩展
            }
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void DoAction(ButtonData data)
        {
            if (data.ActionType == "OpenFile")
            {
                OpenFile(data);
            }
            else if (data.ActionType == "OpenWebsite")
            {
                OpenWebsite(data);
            }
            else if (data.ActionType == "OpenFiles")
            {
                OpenFiles(data);
            }
            else if (data.ActionType == "OpenUwpApp")
            {
                OpenUwpApp(data);
            }
            else if (data.ActionType == "LoadExtension")
            {
                LoadExtension(data);
            }
        }

        // 实现IDisposable接口
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
            if (!isDisposed) isDisposed = true;
        }

        // 析构函数
        ~ActionManager()
        {
            Dispose(false);
        }
    }
}