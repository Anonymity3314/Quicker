using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.IO;
using Quicker;

namespace Quicker.Windows
{
    public partial class OperationMenu : Window
    {
        // 打开文件夹并选中文件
        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList); // 释放资源
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath); //创建指定文件路径
        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags); // 打开文件夹并选中文件
        public string CurrentButton { get; private set; } // 当前按钮
        private readonly ButtonManager buttonManager = new ButtonManager(); // 按钮管理器
        private readonly WindowManager windowManager = new WindowManager(); // 窗口管理器
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
        public event Action? ClosingOrHiding; // 关闭或隐藏操作菜单事件

        public OperationMenu(string currentbutton)
        {
            InitializeComponent(); // 初始化窗口
            CurrentButton = currentbutton; // 设置当前按钮
            InitializeMenu(); // 初始化菜单
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 初始化菜单
        private void InitializeMenu()
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据
            if(buttonData.ActionType == "OpenWebsite")
            {
                MainStackPanel.Children.Remove(OpenLocation); // 移除打开文件或文件夹按钮
                MainStackPanel.Height -= 25; // 设置窗口高度
                MainGrid.Height -= 25; // 设置网格高度
            }
        }

        // 编辑动作信息
        private void EditeInformation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new AddWindow(CurrentButton, 0); // 创建添加动作窗口
            addWindow.Show(); // 显示添加动作窗口
            buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
        }

        // 删除动作
        private async void DeleteAction_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            db2.DeleteAction(CurrentButton); // 删除动作
            buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
        }

        // 查看动作信息
        private void CheckImformation_Click(object sender, RoutedEventArgs e)
        {
            ActionInformationWindow actionInformationWindow = new(CurrentButton);
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            actionInformationWindow.Owner = mainWindow; // 设置父窗口
            actionInformationWindow.ShowDialog();
            this.Close();
        }

        // 在资源管理器中打开文件或文件夹
        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据
            List<string> paths = new List<string>(); // 文件或文件夹路径列表

            if (buttonData.ActionType == "OpenFile")
            {
                paths.Add(buttonData.Location);
            }
            else if (buttonData.ActionType == "OpenFiles")
            {
                paths.AddRange(buttonData.Location.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)));
            }
            else
            {
                new ToastContentBuilder().AddText("不支持此动作类型").Show();
                return;
            }

            try // 检查是否所有文件都在同一个目录下
            {
                string commonDirectory = null;
                bool sameDirectory = true;
                foreach (string path in paths)
                {
                    string directory = Path.GetDirectoryName(path);
                    if (commonDirectory == null)
                        commonDirectory = directory;
                    else if (commonDirectory != directory)
                    {
                        sameDirectory = false;
                        break;
                    }
                }

                if (sameDirectory && paths.Count > 0)
                    // 如果所有文件在同一个目录下，打开一个资源管理器窗口并选中所有文件
                    OpenMultipleFilesInSameDirectory(paths);
                else
                    // 如果文件不在同一个目录下，分别打开多个资源管理器窗口并选中相应文件
                    OpenMultipleFilesInDifferentDirectories(paths);
            }
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText($"打开路径失败：{ex.Message}").Show();
            }
            finally
            {
                buttonManager.CloseMainWindow(this);
            }
        }

        // 打开同一目录下的多个文件并在资源管理器中选中
        private void OpenMultipleFilesInSameDirectory(List<string> paths)
        {
            try
            {
                string commonDirectory = Path.GetDirectoryName(paths[0]);
                IntPtr pidlFolder = ILCreateFromPathW(commonDirectory);
                try
                {
                    List<IntPtr> pidlItems = new List<IntPtr>();
                    foreach (string path in paths)
                    {
                        IntPtr pidlItem = ILCreateFromPathW(path);
                        if (pidlItem == IntPtr.Zero)
                        {
                            new ToastContentBuilder().AddText($"无法获取文件的 PIDL：{path}").Show();
                            continue;
                        }
                        pidlItems.Add(pidlItem);
                    }

                    IntPtr pidlArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * pidlItems.Count);
                    try
                    {
                        for (int i = 0; i < pidlItems.Count; i++)
                        {
                            Marshal.WriteIntPtr(pidlArray, i * Marshal.SizeOf(typeof(IntPtr)), pidlItems[i]);
                        }

                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlFolder, (uint)pidlItems.Count, pidlArray, 0));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pidlArray);
                        foreach (IntPtr pidlItem in pidlItems)
                        {
                            ILFree(pidlItem);
                        }
                    }
                }
                finally
                {
                    ILFree(pidlFolder);
                }
            }
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText($"打开路径失败：{ex.Message}").Show();
            }
        }

        // 分别打开不同目录下的文件并在各自的资源管理器窗口中选中
        private void OpenMultipleFilesInDifferentDirectories(List<string> paths)
        {
            try
            {
                foreach (string path in paths)
                {
                    IntPtr pidlList = ILCreateFromPathW(path);
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                    }
                    catch (Exception ex)
                    {
                        new ToastContentBuilder().AddText($"打开路径失败：{ex.Message}").Show();
                    }
                    finally
                    {
                        ILFree(pidlList);
                    }
                }
            }
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText($"打开路径失败：{ex.Message}").Show();
            }
        }

        // 失去焦点时关闭操作菜单
        private void OperationMenu_Deactivated(object sender, EventArgs e)
        {
            ClosingOrHiding?.Invoke();
            this.Visibility = Visibility.Hidden;
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}