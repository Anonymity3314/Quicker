using Quicker.Windows.MainWindows;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class CreatActionMenu : Window
    {
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ButtonDatabase db2 = new(); // 设置管理器
        private string clipboardText; // 剪切板文本
        private bool hasChanged = false; // 是否已检查
        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名
        public event Action? ClosingOrHiding; // 事件
        private bool haveAction = false; // 是否有动作
        private bool close = true; // 是否正在关闭

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

        // 设置按钮可见性
        private void SetButtonVisbility()
        {
            clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            if(!(clipboardText.StartsWith("OpenActionPage") && clipboardText.EndsWith("OpenActionPageCommand")))
            {
                if(!hasChanged)
                {
                    MainGrid.Height -= 33; // 减少高度
                    Line1.Visibility = Visibility.Collapsed; // 隐藏分割线
                    PasteActionButton.Visibility = Visibility.Collapsed; // 隐藏粘贴按钮
                    hasChanged = !hasChanged;
                }
            }
            else
            {
                string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
                PasteActionTextBlock.Text = $"粘贴动作：{actionInfo[1]}{actionInfo[2]}"; // 设置文本
                if(hasChanged)
                {
                    MainGrid.Height += 33; // 增加高度
                    Line1.Visibility = Visibility.Visible; // 显示分割线
                    PasteActionButton.Visibility = Visibility.Visible; // 显示粘贴按钮
                    hasChanged = !hasChanged;
                }
            }
        }

        // 可见性改变时检查剪切板文本并设置按钮可见性
        private void CreatActionMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetButtonVisbility(); // 设置按钮可见性
        }

        // 粘贴动作
        private void PasteActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden; // 隐藏窗口
            string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
            ButtonData buttonData = new()
            {
                ButtonID = ButtonID,
                Title = actionInfo[1] + actionInfo[2],
                Location = "",
                Data1 = actionInfo[1],
                Data2 = actionInfo[2],
                ImagePath = "",
                Description = $"打开动作页{actionInfo[1]}{actionInfo[2]}",
                ActionType = "OpenActionPage"
            }; // 创建按钮数据
            db2.UpdateAction(buttonData, TableName); // 保存按钮数据
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            if (mainWindow != null)
                mainWindow.UpdateButtonContent(ButtonID, TableName); // 更新主窗口按钮
            this.Close(); // 关闭窗口
        }

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

        // 点击按钮导入动作数据
        private void ImportActionData(object sender, RoutedEventArgs e)
        {
            close = false;
            Microsoft.Win32.OpenFileDialog openFileDialog = new(); // 创建文件对话框
            openFileDialog.Filter = "动作数据文件|*.json"; // 设置文件类型
            if (openFileDialog.ShowDialog() == true) // 显示文件对话框并选择文件
                db2.ImportJsonDataToList(TableName, openFileDialog.FileName, ButtonID); // 导入动作数据
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); // 尝试查找主窗口
            if (mainWindow != null)
                mainWindow.UpdateButtonContent(ButtonID, TableName); // 更新主窗口按钮
            close = true; // 关闭窗口
            this.Close(); // 关闭窗口
        }

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
    }
}