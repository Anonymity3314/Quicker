using Quicker.Windows.MainWindows.MainWindow;
using Quicker.Windows.FloatingWindows;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Quicker.Windows.MainWindows;
using Quicker.Windows.ToolWindows;
using System.Windows.Controls;
using Quicker.Database.Core;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Managers;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.Windows.Menus
{
    public partial class OperationMenu : Window
    {
        #region Win32 API
        // 打开文件夹并选中文件
        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList); // 释放资源
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath); //创建指定文件路径
        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags); // 打开文件夹并选中文件
        #endregion

        #region 属性和字段
        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名
        public bool IsMainWindow { get; private set; } // 是否为主窗口
        public Window FatherWindow { get; private set; } // 父窗口

        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        public event Action? ClosingOrHiding; // 关闭或隐藏操作菜单事件
        private bool close = true; // 是否关闭窗口
        #endregion

        #region 初始化

        /// <summary>
        /// 操作菜单
        /// </summary>
        /// <param name="buttonID">按钮ID</param>
        /// <param name="tableName">表名</param>
        public OperationMenu(int buttonID, string tableName, Window window = null, bool isMainWindow = true)
        {
            InitializeComponent(); // 初始化窗口
            FirstChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            SecondChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
            ButtonID = buttonID; // 设置当前按钮
            TableName = tableName; // 设置表名
            IsMainWindow = isMainWindow; // 设置是否为主窗口
            FatherWindow = window; // 设置父窗口
            InitializeMenu(); // 初始化菜单
        }

        // 窗口加载时设置窗口位置
        private void OperationMenu_Loaded(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowPositionNearMouse(this); // 设置窗口位置

            // 获取按钮的绝对位置
            Point otherFunctionPoint = OtherFunction.TransformToAncestor(this).Transform(new Point(0, 0));
            Point exportActionPoint = ExportAction.TransformToAncestor(this).Transform(new Point(0, 0));
            Point copyActionInfoPoint = CopyActionInfo.TransformToAncestor(this).Transform(new Point(0, 0));
            Point checkImformationPoint = CheckImformation.TransformToAncestor(this).Transform(new Point(0, 0));
            
            // 设置 FirstChildGrid 的边距，使其与 OtherFunction 按钮对齐
            FirstChildGrid.Margin = new Thickness(0, otherFunctionPoint.Y - exportActionPoint.Y - 2, 0, 0);
            // 设置 SecondChildGrid 的边距，使其与 CopyActionInfo 按钮对齐
            SecondChildGrid.Margin = new Thickness(130, copyActionInfoPoint.Y - checkImformationPoint.Y + 65, 0, 0);
        }

        // 初始化菜单
        private void InitializeMenu()
        {
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            if (buttonData == null) // 按钮数据不存在
            {
                UpdateMainWindowButton(); // 更新主窗口按钮
                return; // 退出
            }
            AdjustUIForButtonType(buttonData); // 根据按钮类型调整界面
            AdjustUIForClipboard(); // 根据剪贴板内容调整界面
            AdjustUIForPreviousWindow(); // 根据上一个窗口调整界面
        }

        // 更新主窗口按钮
        private void UpdateMainWindowButton()
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            if (mainWindow != null)
                mainWindow.UpdateButtonContent(ButtonID, TableName); // 更新主窗口按钮
        }

        /// <summary>
        /// 根据按钮类型调整界面
        /// </summary>
        /// <param name="buttonData">按钮数据</param>
        private void AdjustUIForButtonType(ButtonData buttonData)
        {
            if (buttonData.ActionType == "OpenWebsite")
            {
                RemoveOpenLocationButton();
            }
        }

        // 根据剪贴板内容调整界面
        private void AdjustUIForClipboard()
        {
            if (!Clipboard.ContainsImage()) // 剪贴板不包含图像
            {
                RemovePasteIconButton();
            }
        }

        // 根据上一个窗口调整界面
        private void AdjustUIForPreviousWindow()
        {
            if (IsMainWindow) // 如果是主窗口
            {
                MainStackPanel.Children.Remove(CloseFloatButton); // 移除关闭浮动按钮
                MainStackPanel.Children.Remove(Rectangle1); // 移除分割线
                EditeInformation.Margin = new Thickness(0, 5, 0, 0); // 调整编辑信息按钮位置
                MainGrid.Height -= 32; // 设置网格高度
            }
        }

        // 移除打开位置按钮
        private void RemoveOpenLocationButton()
        {
            MainStackPanel.Children.Remove(OpenLocation); // 移除打开文件或文件夹按钮
            MainGrid.Height -= 25; // 设置网格高度
        }

        // 移除粘贴图标按钮
        private void RemovePasteIconButton()
        {
            MainStackPanel.Children.Remove(PasteIcon); // 移除粘贴图标按钮
            MainGrid.Height -= 25; // 设置网格高度
        }

        #endregion

        #region 动作管理
        // 编辑动作信息
        private void EditeInformation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new AddWindow(ButtonID, TableName, 0); // 创建添加动作窗口
            addWindow.Show(); // 显示添加动作窗口
            buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
        }

        // 复制动作
        private void CopyAction_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            Clipboard.SetText($"{ButtonID}"); // 复制文本到剪贴板
            Clipboard.SetText($"CopyAction;{TableName};{ButtonID};QuickerCommand"); // 复制文本到剪贴板
            this.Close(); // 关闭窗口
        }

        // 剪切动作
        private void CutAction_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            Clipboard.SetText($"CutAction;{TableName};{ButtonID};QuickerCommand"); // 复制文本到剪贴板
            this.Close(); // 关闭窗口
        }

        // 删除动作
        private void DeleteAction_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            db2.DeleteAction(ButtonID, TableName); // 删除动作
            UpdateUIAfterActionDelete(); // 更新UI
            this.Close(); // 关闭窗口
        }

        // 悬浮动作
        private void SuspendAction_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            if (buttonData != null) // 按钮数据不为空
            {
                FloatingActionWindow floatingActionWindow = new(ButtonID, TableName); // 创建悬浮动作窗口
                floatingActionWindow.Show(); // 显示悬浮动作窗口
            }
            this.Close(); // 关闭窗口
        }

        // 更新UI（删除动作后）
        private void UpdateUIAfterActionDelete()
        {
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            var mainWindowList = Application.Current.Windows.OfType<MainWindow>(); // 尝试查找主窗口
            if (mainWindowList != null)
            {
                foreach (MainWindow mainWindow in mainWindowList) // 遍历主窗口列表
                {
                    mainWindow.UpdateButtonContent(ButtonID, TableName); // 更新主窗口按钮
                }
            }
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

        // 点击按钮关闭悬浮动作窗口
        private void CloseFloatButton_Click(object sender, RoutedEventArgs e)
        {
            FatherWindow.Close(); // 关闭父窗口
            this.Close(); // 关闭窗口
        }

        #endregion

        #region 信息复制
        // 粘贴剪贴板图标为动作图标
        private void PasteIcon_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage()) // 剪贴板包含图像
            {
                BitmapSource bitmapSource = Clipboard.GetImage(); // 获取图像
                if (bitmapSource != null) // 图像不为空
                {
                    var iconManager = new IconManager(); // 创建图标管理器
                    string iconPath = iconManager.SaveIconToFile(bitmapSource); // 保存图像到文件
                    if (!string.IsNullOrEmpty(iconPath)) // 图像路径不为空
                    {
                        ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
                        if (buttonData != null) // 按钮数据不为空
                        {
                            buttonData.ImagePath = iconPath; // 更新按钮数据
                            db2.UpdateAction(buttonData, TableName); // 更新按钮数据
                            UpdateUIAfterActionDelete(); // 更新UI
                        }
                    }
                    this.Close(); // 关闭窗口
                }
            }
        }
        
        // 查看动作信息
        private void CheckImformation_Click(object sender, RoutedEventArgs e)
        {
            ActionInfoWindow actionInfoWindow = new(ButtonID, TableName); // 创建动作信息窗口
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            actionInfoWindow.ShowDialog(); // 显示动作信息窗口
            this.Close(); // 关闭操作菜单窗口
        }

        // 复制动作名称
        private void CopyActionName_Click(object sender, RoutedEventArgs e)
        {
            ButtonData data = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            Clipboard.SetText(data.Title); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show("已复制。", "Success"); // 弹出消息提醒
            this.Close(); // 关闭窗口
        }

        // 复制动作ID
        private void CopyActionID_Click(object sender, RoutedEventArgs e)
        {
            ButtonData data = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            Clipboard.SetText($"{data.ButtonID}"); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show("动作ID已复制。", "Success"); // 弹出消息提醒
            this.Close(); // 关闭窗口
        }
        #endregion

        #region 文件操作
        // 在资源管理器中打开文件或文件夹
        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            List<string> paths = GetFilePaths(buttonData); // 获取文件路径列表
            
            if (paths == null)
                return; // 退出

            try // 检查是否所有文件都在同一个目录下
            {
                if (AreFilesInSameDirectory(paths))
                    OpenMultipleFilesInSameDirectory(paths); // 如果所有文件在同一个目录下，打开一个资源管理器窗口并选中所有文件
                else
                    OpenMultipleFilesInDifferentDirectories(paths); // 如果文件不在同一个目录下，分别打开多个资源管理器窗口并选中相应文件
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"打开路径失败：{ex.Message}");
            }
            finally
            {
                buttonManager.CloseMainWindow(this); // 关闭操作菜单窗口
            }
        }

        /// <summary>
        /// 获取文件路径列表
        /// </summary>
        /// <param name="buttonData">按钮数据</param>
        /// <returns>文件路径列表</returns>
        private List<string> GetFilePaths(ButtonData buttonData)
        {
            List<string> paths = new List<string>(); // 文件或文件夹路径列表
            switch (buttonData.ActionType)
            {
                case "OpenFile":
                case "LoadExtension":
                    paths.Add(buttonData.Location); // 添加文件路径
                    break;
                case "OpenFiles":
                    paths.AddRange(buttonData.Location.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p))); // 添加多个文件路径
                    break;
                default:
                    ShowErrorMessage($"不支持此动作类型。"); // 显示错误消息
                    return null; // 返回空
            }
            return paths;
        }

        /// <summary>
        /// 检查文件是否在同一目录
        /// </summary>
        /// <param name="paths">文件路径列表</param>
        /// <returns>是否在同一目录</returns>
        private bool AreFilesInSameDirectory(List<string> paths)
        {
            if (paths.Count == 0)
                return false;

            string commonDirectory = null; // 公共目录
            foreach (string path in paths) // 遍历所有文件路径
            {
                string directory = Path.GetDirectoryName(path); // 获取文件所在目录
                if (commonDirectory == null) // 第一次循环
                    commonDirectory = directory; // 设置公共目录
                else if (commonDirectory != directory) // 后续循环
                {
                    return false; // 不在同一目录
                }
            }
            return true;
        }

        /// <summary>
        /// 打开同一目录下的多个文件并在资源管理器中选中
        /// </summary>
        /// <param name="paths">文件路径列表</param>
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
                            ShowErrorMessage($"无法获取文件的 PIDL：{path}");
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
                ShowErrorMessage($"打开路径失败：{ex.Message}"); // 显示错误消息
            }
        }

        /// <summary>
        /// 分别打开不同目录下的文件并在各自的资源管理器窗口中选中
        /// </summary>
        /// <param name="paths">文件路径列表</param>
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
                        ShowErrorMessage($"打开路径失败：{ex.Message}"); // 显示错误消息
                    }
                    finally
                    {
                        ILFree(pidlList); // 释放 PIDL 资源
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"打开路径失败：{ex.Message}"); // 显示错误消息
            }
        }
        #endregion

        #region UI交互
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

        // 鼠标移出关闭子菜单
        private void SecondChildGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            CopyActionInfo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 还原背景颜色
            SecondChildGrid.Visibility = Visibility.Collapsed; // 隐藏子菜单
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
        #endregion

        #region 辅助方法
        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="message">消息</param>
        private void ShowErrorMessage(string message)
        {
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show(message, "Error"); // 弹出消息提醒
        }
        #endregion

        #region 资源释放
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
        #endregion
    }
}