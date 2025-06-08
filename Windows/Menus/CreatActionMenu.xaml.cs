using Quicker.Windows.MainWindows;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Quicker.Windows.Menus
{
    public partial class CreatActionMenu : Window
    {
        #region 字段和属性
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ButtonDatabase db2 = new(); // 设置管理器
        private string clipboardText; // 剪切板文本
        private bool hasChanged = false; // 是否已检查
        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名
        public event Action? ClosingOrHiding; // 事件
        private bool haveAction = false; // 是否有动作
        private bool close = true; // 是否正在关闭
        private readonly IconManager iconManager = new(); // 图标管理器
        #endregion

        #region 初始化
        public CreatActionMenu(int buttonID, string tableName)
        {
            InitializeComponent();
            SetButtonVisbility(); // 设置按钮可见性
            ButtonID = buttonID; // 设置当前按钮
            TableName = tableName; // 设置表名
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 窗口加载时设置窗口位置
        private void CreatActionMenu_Loaded(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowPositionNearMouse(this); // 设置窗口位置
        }
        #endregion

        #region 按钮可见性管理
        // 设置按钮可见性
        private void SetButtonVisbility()
        {
            SetPasteActionButtonVisibility();
            SetCreatOpenFileActionButtonVisibility();
        }

        // 设置粘贴动作按钮可见性
        private void SetPasteActionButtonVisibility()
        {
            clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            if (!clipboardText.EndsWith("QuickerCommand")) // 剪切板文本不是快捷指令
            {
                if (!hasChanged)
                {
                    MainGrid.Height -= 32; // 减少高度
                    Line1.Visibility = Visibility.Collapsed; // 隐藏分割线
                    PasteActionButton.Visibility = Visibility.Collapsed; // 隐藏粘贴按钮
                    hasChanged = !hasChanged;
                }
            }
            else if (clipboardText.StartsWith("OpenActionPage"))
            {
                string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
                PasteActionTextBlock.Text = $"粘贴动作：{actionInfo[1]}{actionInfo[2]}"; // 设置文本
                if (hasChanged)
                {
                    MainGrid.Height += 32; // 增加高度
                    Line1.Visibility = Visibility.Visible; // 显示分割线
                    PasteActionButton.Visibility = Visibility.Visible; // 显示粘贴按钮
                    hasChanged = !hasChanged;
                }
            }
            else if (clipboardText.StartsWith("CopyAction") ||
                clipboardText.StartsWith("CutAction")) // 剪切板文本是动作
            {
                string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
                var buttonData = db2.GetButtonDataByID(int.Parse(actionInfo[2]), actionInfo[1]); // 获取按钮数据
                PasteActionTextBlock.Text = $"粘贴动作：{buttonData.Title}"; // 设置文本
                if (hasChanged)
                {
                    MainGrid.Height += 32; // 增加高度
                    Line1.Visibility = Visibility.Visible; // 显示分割线
                    PasteActionButton.Visibility = Visibility.Visible; // 显示粘贴按钮
                    hasChanged = !hasChanged;
                }
            }
        }

        // 设置创建打开文件动作按钮可见性
        private void SetCreatOpenFileActionButtonVisibility()
        {
            if (!Clipboard.ContainsFileDropList())
            {
                CreatOpenFileActionButton.Visibility = Visibility.Collapsed; // 隐藏按钮
                MainGrid.Height -= 25; // 减少高度
            }
            else
            {
                var fileList = Clipboard.GetFileDropList(); // 获取文件列表
                if (fileList.Count > 0)
                {
                    string filePath = fileList[0]; // 获取第一个文件路径
                    string fileName = System.IO.Path.GetFileName(filePath); // 获取文件名
                    string buttonText = $"创建动作：打开[{fileName}]"; // 设置文本
                    CreatOpenFileActionButton.ToolTip = $"创建打开文件或文件夹{filePath}的动作"; // 设置提示
                    CreatOpenFileActionTextBlock.Text = buttonText; // 设置文本

                    //// 计算文本长度并调整按钮宽度
                    //var formattedText = new FormattedText(
                    //    buttonText,
                    //    System.Globalization.CultureInfo.CurrentCulture,
                    //    FlowDirection.LeftToRight,
                    //    new Typeface(CreatOpenFileActionTextBlock.FontFamily, CreatOpenFileActionTextBlock.FontStyle, CreatOpenFileActionTextBlock.FontWeight, CreatOpenFileActionTextBlock.FontStretch),
                    //    CreatOpenFileActionTextBlock.FontSize,
                    //    Brushes.Black,
                    //    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    //double textWidth = formattedText.Width;
                    //if (textWidth > 150) // 如果文本宽度超过150像素
                    //{
                    //    double newButtonWidth = textWidth + 50; // 设置按钮宽度为文本宽度加上一些边距
                    //    CreatOpenFileActionButton.Width = newButtonWidth; // 设置按钮宽度
                    //    MainGrid.Width = Math.Max(MainGrid.Width, newButtonWidth + 20); // 调整主网格宽度
                        
                    //    // 调整MainGrid的Margin，保持窗口布局平衡
                    //    double newRightMargin = 758 - MainGrid.Width - 9; // 窗口宽度(758) - 网格宽度 - 左边距(9)
                    //    MainGrid.Margin = new Thickness(9, 8, newRightMargin, 211); // 保持上边距(8)和下边距(211)不变

                    //    // 调整所有按钮内部Grid的宽度和对齐方式
                    //    foreach (Button button in FindVisualChildren<Button>(MainGrid))
                    //    {
                    //        if (button.Content is Grid buttonGrid)
                    //        {
                    //            buttonGrid.Width = button == CreatOpenFileActionButton ? newButtonWidth - 10 : 168;
                    //            buttonGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    //        }
                    //    }
                    //}
                }
            }
        }
        #endregion

        #region 动作管理
        // 粘贴动作
        private void PasteActionButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden; // 隐藏窗口
            PasteAction(); // 粘贴动作
            UpdateUIAfterActionChange(); // 更新UI
            Close(); // 关闭窗口
        }

        // 粘贴动作
        private void PasteAction()
        {
            string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
            switch (actionInfo[0])
            {
                case "CopyAction":
                case "CutAction":
                    HandleCopyOrCutAction(actionInfo);
                    break;
                case "OpenActionPage":
                    HandleOpenActionPage(actionInfo);
                    break;
            }
        }

        // 处理复制或剪切动作
        private void HandleCopyOrCutAction(string[] actionInfo)
        {
            var buttonData = db2.GetButtonDataByID(int.Parse(actionInfo[2]), actionInfo[1]); // 获取按钮数据
            ButtonData buttonData1 = new()
            {
                ButtonID = ButtonID,
                Title = buttonData.Title,
                Location = buttonData.Location,
                ImagePath = buttonData.ImagePath,
                Data1 = buttonData.Data1,
                Data2 = buttonData.Data2,
                Data3 = buttonData.Data3,
                Description = buttonData.Description,
                CreateTime = DateTime.Now,
                ActionType = buttonData.ActionType,
                UsedTimes = buttonData.UsedTimes
            }; // 创建按钮数据
            db2.UpdateAction(buttonData1, TableName); // 保存按钮数据
            if(actionInfo[0] == "CutAction")
            {
                db2.DeleteAction(int.Parse(actionInfo[2]), actionInfo[1]); // 删除动作
                UpdateMainWindowButton(int.Parse(actionInfo[2]), actionInfo[1]); // 更新主窗口按钮
            }
        }

        // 处理打开动作页
        private void HandleOpenActionPage(string[] actionInfo)
        {
            ButtonData buttonData3 = new()
            {
                ButtonID = ButtonID,
                Title = actionInfo[1] + actionInfo[2],
                Location = "",
                Data1 = actionInfo[1],
                Data2 = actionInfo[2],
                ImagePath = "",
                Description = $"打开动作页{actionInfo[1]}{actionInfo[2]}",
                CreateTime = DateTime.Now,
                ActionType = "OpenActionPage",
                UsedTimes = 0
            }; // 创建按钮数据
            db2.UpdateAction(buttonData3, TableName); // 保存按钮数据
        }

        // 更新UI（动作改变后）
        private void UpdateUIAfterActionChange()
        {
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            UpdateMainWindowButton(ButtonID, TableName); // 更新主窗口按钮
        }

        // 更新主窗口按钮
        private void UpdateMainWindowButton(int buttonID, string tableName)
        {
            var mainWindowList = Application.Current.Windows.OfType<MainWindow>(); // 尝试查找主窗口
            if (mainWindowList != null)
            {
                foreach (MainWindow mainWindow in mainWindowList) // 遍历主窗口列表
                {
                    mainWindow.UpdateButtonContent(buttonID, tableName); // 更新主窗口按钮
                }
            }
        }
        #endregion

        #region 动作创建
        // 启动软件
        private void StartApp(object sender, RoutedEventArgs e)
        {
            haveAction = true; // 有动作
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(ButtonID, TableName, 1); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开文件
        private void OpenDocument(object sender, RoutedEventArgs e)
        {
            haveAction = true; // 有动作
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(ButtonID, TableName, 2); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开文件夹
        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            haveAction = true; // 有动作
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(ButtonID, TableName, 3); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开网址
        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            haveAction = true; // 有动作
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(ButtonID, TableName, 4); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 创建打开文件动作
        private void CreatOpenFileAction(object sender, RoutedEventArgs e)
        {
            if (buttonManager.CreateFileActionFromClipboard(ButtonID, TableName))
            {
                UpdateUIAfterActionChange(); // 更新UI
                Close(); // 关闭窗口
            }
        }

        /// <summary>
        /// 判断是否为图片文件
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 是否为图片文件 </returns>
        private bool IsImaege(string filePath)
        {
            string extension = Path.GetExtension(filePath); // 获取文件地址
            return extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".gif"; // 判断是否为图片文件
        }

        /// <summary>
        /// 处理图片拖拽
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        private void ProcessImageDrop(string filePath)
        {
            BitmapImage bitmap = new BitmapImage(new Uri(filePath)); // 创建 BitmapImage 对象
            string iconPath = ""; // 默认图标路径
            if (bitmap != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(filePath); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(bitmap); // 保存图标到文件
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = ButtonID,
                Title = fileName,
                Location = filePath,
                ImagePath = iconPath,
                Data1 = false.ToString(), // 是否使用管理员身份运行
                Data2 = true.ToString(), // 尝试打开已存在的窗口
                Data3 = 0.ToString(),
                Description = $"打开图片: {fileName}",
                CreateTime = DateTime.Now,
                ActionType = "OpenFile",
            };
            db2.UpdateAction(buttonData, TableName); // 添加按钮数据到数据库
            UpdateUIAfterActionChange(); // 更新UI
        }

        // 点击按钮导入动作数据
        private void ImportActionData(object sender, RoutedEventArgs e)
        {
            close = false;
            Microsoft.Win32.OpenFileDialog openFileDialog = new(); // 创建文件对话框
            openFileDialog.Filter = "动作数据文件|*.json"; // 设置文件类型
            if (openFileDialog.ShowDialog() == true) // 显示文件对话框并选择文件
                db2.ImportJsonDataToList(TableName, openFileDialog.FileName, ButtonID); // 导入动作数据
            UpdateUIAfterActionChange(); // 更新UI
            close = true; // 关闭窗口
            this.Close(); // 关闭窗口
        }

        // 加载扩展
        private void LoadExtension(object sender, RoutedEventArgs e)
        {
            haveAction = true; // 有动作
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(ButtonID, TableName, 5); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }
        #endregion

        #region 窗口事件处理
        // 失去焦点时隐藏
        private void CreatActionMenu_Deactivated(object sender, EventArgs e)
        {
            if (close) 
            {
                ClosingOrHiding?.Invoke(); // 调用事件
                if (!haveAction)
                {
                    using var windowMananger = new WindowManager(); // 创建窗口管理器
                    windowMananger.SetMainWindowFocused(); // 关闭窗口
                }
                this.Visibility = Visibility.Hidden; // 隐藏窗口
                using var windowManager = new WindowManager(); // 创建窗口管理器
                windowManager.CloseMenuAsync(this); // 延时关闭窗口
            }
        }
        #endregion

        #region 资源释放
        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件
            clipboardText = null; // 清理剪切板文本
            hasChanged = false; // 清理检查状态
            clipboardText = null; // 清理剪切板文本
            buttonManager.Dispose(); // 释放按钮管理器
            ButtonID = 0; // 清理当前按钮
            TableName = null; // 清理表名
            haveAction = false; // 清理是否有动作
            close = false; // 清理关闭状态

            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
        #endregion

        // 辅助方法：查找指定类型的所有子元素
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
}