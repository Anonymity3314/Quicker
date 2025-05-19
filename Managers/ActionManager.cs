using IWshRuntimeLibrary;
using System.Diagnostics;
using Quicker.Windows;
using Quicker.Database;
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace Quicker.Managers
{
    internal class ActionManager
    {
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenFile(ButtonData data)
        {
            if (data.Data2 == "true") // 如果尝试打开已存在的窗口
            {
                string windowTitle = System.IO.Path.GetFileNameWithoutExtension(data.Location);
                WindowManager.TryToOpenExitingWindow(windowTitle);
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
                        UseShellExecute = data.Data1 == "true", // 是否使用系统默认方式运行
                        Verb = data.Data1 == "true" ? "runas" : null, // 管理员权限运行
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
                    ToastManager.AddToast($"打开失败：{ex}", "Error"); // 显示错误提示
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
                    ToastManager.AddToast($"打开失败：{ex}", "Error"); // 显示错误提示
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
                switch (int.Parse(data.Data3))
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
                ToastManager.AddToast($"打开网站失败：{ex.Message}", "Error"); // 显示错误提示
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
            });
        }

        /// <summary>
        /// 打开IE浏览器
        /// </summary>
        /// <param name="website"> 网站地址 </param>
        private void LaunchInternetExplorer(string website)
        {
            Process.Start("iexplore.exe", website); // 启动IE浏览器
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
                string appProtocol = $"microsoft-{data.Location}://"; // UWP应用的协议
                Process.Start(new ProcessStartInfo
                {
                    FileName = appProtocol,
                    UseShellExecute = true
                }); // 启动UWP应用
            }
            catch (Exception ex)
            {
                ToastManager.AddToast($"打开UWP应用失败：{ex.Message}", "Error"); // 显示错误提示
            }
        }

        // 手动释放资源
        public void Dispose()
        {
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}