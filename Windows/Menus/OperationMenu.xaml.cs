using System.Runtime.InteropServices;
using System.Windows.Controls;
using Quicker.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.IO;

namespace Quicker.Windows.Menus
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

        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        public event Action? ClosingOrHiding; // 关闭或隐藏操作菜单事件
        private bool close = true; // 是否关闭窗口

        public OperationMenu(int buttonID, string tableName)
        {
            InitializeComponent(); // 初始化窗口
            FirstChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            SecondChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            ButtonID = buttonID; // 设置当前按钮
            TableName = tableName; // 设置表名
            InitializeMenu(); // 初始化菜单
        }

        // 初始化菜单
        private void InitializeMenu()
        {
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            if(buttonData.ActionType == "OpenWebsite")
            {
                MainStackPanel.Children.Remove(OpenLocation); // 移除打开文件或文件夹按钮
                MainStackPanel.Height -= 25; // 设置窗口高度
                MainGrid.Height -= 25; // 设置网格高度
                ChiildGrid.Margin = new Thickness(494, 240, 0, 0); // 设置子菜单边距
            }
        }

        // 编辑动作信息
        private void EditeInformation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new AddWindow(ButtonID, TableName, 0); // 创建添加动作窗口
            addWindow.Show(); // 显示添加动作窗口
            buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
        }

        // 删除动作
        private async void DeleteAction_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            db2.DeleteAction(ButtonID, TableName); // 删除动作
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            if (mainWindow != null)
                mainWindow.UpdateButtonContent(ButtonID, TableName); // 更新主窗口按钮
            this.Close(); // 关闭窗口
        }

        // 查看动作信息
        private void CheckImformation_Click(object sender, RoutedEventArgs e)
        {
            ActionInformationWindow actionInformationWindow = new(ButtonID, TableName); // 创建动作信息窗口
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            actionInformationWindow.ShowDialog(); // 显示动作信息窗口
            this.Close(); // 关闭操作菜单窗口
        }

        // 在资源管理器中打开文件或文件夹
        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            List<string> paths = new List<string>(); // 文件或文件夹路径列表

            if (buttonData.ActionType == "OpenFile")
                paths.Add(buttonData.Location); // 添加文件路径
            else if (buttonData.ActionType == "OpenFiles")
                paths.AddRange(buttonData.Location.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p))); // 添加多个文件路径
            else
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"不支持此动作类型。", "Error"); // 弹出消息提醒
                return; // 退出
            }

            try // 检查是否所有文件都在同一个目录下
            {
                string commonDirectory = null; // 公共目录
                bool sameDirectory = true; // 是否所有文件在同一个目录下
                foreach (string path in paths) // 遍历所有文件路径
                {
                    string directory = Path.GetDirectoryName(path); // 获取文件所在目录
                    if (commonDirectory == null) // 第一次循环
                        commonDirectory = directory; // 设置公共目录
                    else if (commonDirectory != directory) // 后续循环
                    {
                        sameDirectory = false; // 设置为不同目录
                        break; // 退出循环
                    }
                }

                if (sameDirectory && paths.Count > 0)
                    OpenMultipleFilesInSameDirectory(paths); // 如果所有文件在同一个目录下，打开一个资源管理器窗口并选中所有文件
                else
                    OpenMultipleFilesInDifferentDirectories(paths); // 如果文件不在同一个目录下，分别打开多个资源管理器窗口并选中相应文件
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"打开路径失败：{ex.Message}", "Error"); // 弹出消息提醒
            }
            finally
            {
                buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
            }
        }

        // 打开同一目录下的多个文件并在资源管理器中选中
        private void OpenMultipleFilesInSameDirectory(List<string> paths)
        {
            try
            {
                string commonDirectory = Path.GetDirectoryName(paths[0]); // 获取公共目录
                IntPtr pidlFolder = ILCreateFromPathW(commonDirectory); // 获取公共目录的 PIDL
                try
                {
                    List<IntPtr> pidlItems = new List<IntPtr>(); // 文件 PIDL 列表
                    foreach (string path in paths) // 遍历所有文件路径
                    {
                        IntPtr pidlItem = ILCreateFromPathW(path); // 获取文件 PIDL
                        if (pidlItem == IntPtr.Zero) // 无法获取 PIDL
                        {
                            using var toast = new ToastManager(); // 消息提醒管理器
                            toast.ShowToast($"无法获取文件的 PIDL：{path}", "Error"); // 弹出消息提醒
                            continue; // 跳过当前文件
                        }
                        pidlItems.Add(pidlItem); // 添加 PIDL 到列表
                    }

                    IntPtr pidlArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * pidlItems.Count); // 申请内存空间
                    try
                    {
                        for (int i = 0; i < pidlItems.Count; i++) // 遍历 PIDL 列表
                        {
                            Marshal.WriteIntPtr(pidlArray, i * Marshal.SizeOf(typeof(IntPtr)), pidlItems[i]); // 写入 PIDL 到内存
                        }
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlFolder, (uint)pidlItems.Count, pidlArray, 0)); // 打开文件夹并选中文件
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pidlArray); // 释放内存空间
                        foreach (IntPtr pidlItem in pidlItems) // 释放 PIDL 资源
                        {
                            ILFree(pidlItem); // 释放 PIDL 资源
                        }
                    }
                }
                finally
                {
                    ILFree(pidlFolder); // 释放 PIDL 资源
                }
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"打开路径失败：{ex.Message}", "Error"); // 弹出消息提醒
            }
        }

        // 分别打开不同目录下的文件并在各自的资源管理器窗口中选中
        private void OpenMultipleFilesInDifferentDirectories(List<string> paths)
        {
            try
            {
                foreach (string path in paths) // 遍历所有文件路径
                {
                    IntPtr pidlList = ILCreateFromPathW(path); // 获取文件 PIDL
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0)); // 打开文件所在目录并选中文件
                    }
                    catch (Exception ex)
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.ShowToast($"打开路径失败：{ex.Message}", "Error"); // 弹出消息提醒
                    }
                    finally
                    {
                        ILFree(pidlList); // 释放 PIDL 资源
                    }
                }
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"打开路径失败：{ex.Message}", "Error"); // 弹出消息提醒
            }
        }

        // 失去焦点时关闭操作菜单
        private void OperationMenu_Deactivated(object sender, EventArgs e)
        {
            if (close) 
            {
                ClosingOrHiding?.Invoke(); // 调用关闭或隐藏事件
                using var windowMananger = new WindowManager(); // 创建窗口管理器
                windowMananger.SetMainWindowFocused(); // 关闭窗口
                this.Visibility = Visibility.Hidden; // 隐藏窗口
                using var windowManager = new WindowManager(); // 创建窗口管理器
                windowManager.CloseMenuAsync(this); // 延时关闭窗口
            }
        }

        // 鼠标移入显示子菜单
        private void OtherFunction_MouseEnter(object sender, MouseEventArgs e)
        {
            OtherFunction.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 改变背景颜色
            FirstChildGrid.Visibility = Visibility.Visible; // 显示子菜单
        }

        // 鼠标移出关闭子菜单
        private void OtherFunction_MouseLeave(object sender, MouseEventArgs e)
        {
            if (FirstChildGrid.IsMouseOver) return;
            OtherFunction.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
            FirstChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
        }

        // 鼠标移入显示子菜单
        private void CopyActionInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            CopyActionInfo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 改变背景颜色
            SecondChildGrid.Visibility = Visibility.Visible; // 显示子菜单
        }

        // 鼠标移出关闭子菜单
        private void CopyActionInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!SecondChildGrid.IsMouseOver)
            {
                CopyActionInfo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 还原背景颜色
                SecondChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            }
        }

        // 复制动作名称
        private void CopyActionName_Click(object sender, RoutedEventArgs e)
        {
            ButtonData data = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            Clipboard.SetText(data.Title); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.ShowToast("已复制。", "Success"); // 弹出消息提醒
            this.Close(); // 关闭窗口
        }

        // 复制动作ID
        private void CopyActionID_Click(object sender, RoutedEventArgs e)
        {
            ButtonData data = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            Clipboard.SetText($"{data.ButtonID}"); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.ShowToast("动作ID已复制。", "Success"); // 弹出消息提醒
            this.Close(); // 关闭窗口
        }

        // 鼠标移出关闭子菜单
        private void SecondChildGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            CopyActionInfo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 还原背景颜色
            SecondChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
        }

        // 导出动作数据到指定文件夹
        private void ExportAction_Click(object sender, RoutedEventArgs e)
        {
            close = false; // 设置关闭标识符
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            using var dialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true }; // 创建文件夹选择对话框
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // 选择文件夹
                db2.ExportActionDataToJson(TableName, ButtonID, dialog.SelectedPath); // 导出动作数据到指定文件夹
            close = true; // 设置关闭标识符
        }

        // 鼠标移出关闭子菜单
        private void FirstChildGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!MainGrid.IsMouseOver && !SecondChildGrid.IsMouseOver && !OtherFunction.IsMouseOver)
            {
                OtherFunction.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 还原背景颜色
                FirstChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            }
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件
            buttonManager.Dispose(); // 释放按钮管理器资源
            close = false; // 设置关闭标识符
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}