using System.Runtime.InteropServices;
using System.Windows.Interop;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
using Quicker;

namespace Quicker.Windows
{
    public partial class CreatActionMenu : Window
    {
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly WindowManager windowManager = new(); // 窗口管理器
        private readonly SettingDatabase db1 = new(); // 设置数据库
        private readonly ButtonDatabase db2 = new(); // 设置管理器
        private string clipboardText; // 剪切板文本
        private bool hasChanged = false; // 是否已检查
        public string CurrentButton { get; private set; } // 当前按钮
        public event Action? ClosingOrHiding; // 事件

        public CreatActionMenu(string currentbutton)
        {
            InitializeComponent();
            SetButtonVisbility(); // 设置按钮可见性
            CurrentButton = currentbutton; // 设置当前按钮
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 设置按钮可见性
        private void SetButtonVisbility()
        {
            clipboardText = System.Windows.Clipboard.GetText(); // 获取剪贴板文本
            if(!(clipboardText.StartsWith("OpenActionPage") && clipboardText.EndsWith("OpenActionPageCommand")))
            {
                if(!hasChanged)
                {
                    MainGrid.Height -= 29; // 减少高度
                    Line1.Visibility = Visibility.Collapsed; // 隐藏分割线
                    PasteActionButton.Visibility = Visibility.Collapsed; // 隐藏复制按钮
                    StartAppButton.Margin = new Thickness(0, 5, 0, 0); // 调整按钮位置
                    hasChanged = !hasChanged;
                }
            }
            else
            {
                string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
                PasteActionTextBlock.Text = $"粘贴动作：{actionInfo[1]}{actionInfo[2]}"; // 设置文本
                if(hasChanged)
                {
                    MainGrid.Height += 29; // 增加高度
                    Line1.Visibility = Visibility.Visible; // 显示分割线
                    PasteActionButton.Visibility = Visibility.Visible; // 显示复制按钮
                    StartAppButton.Margin = new Thickness(0, 3, 0, 0); // 调整按钮位置
                    hasChanged = !hasChanged;
                }
            }
        }

        // 可见性改变时检查剪切板文本并设置按钮可见性
        private void CreatActionMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetButtonVisbility(); // 设置按钮可见性
        }

        // 复制动作
        private void PasteActionButton_Click(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            string[] actionInfo = clipboardText.Split(';'); // 解析剪切板文本
            ButtonData buttonData = new()
            {
                ButtonID = CurrentButton,
                Title = actionInfo[1] + actionInfo[2],
                Location = actionInfo[1] + ";" + actionInfo[2],
                ImagePath = "",
                Description = $"打开动作页{actionInfo[1]}{actionInfo[2]}",
                ActionType = "OpenActionPage"
            };
            db2.AddAction(buttonData); // 保存按钮数据
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 启动软件
        private void StartApp(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(CurrentButton, 1); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开文件
        private void OpenDocument(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(CurrentButton, 2); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开文件夹
        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(CurrentButton, 3); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 打开网址
        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            buttonManager.HideMainWindow(this); // 隐藏操作菜单窗口
            AddWindow addWindow = new(CurrentButton, 4); // 传递当前按钮和类型
            addWindow.Show(); // 显示窗口
            buttonManager.CloseMainWindow(this); // 关闭主窗口
        }

        // 失去焦点时隐藏
        private void CreatActionMenu_Deactivated(object sender, EventArgs e)
        {
            ClosingOrHiding?.Invoke(); // 调用事件
            this.Visibility = Visibility.Hidden; // 隐藏窗口
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件
            clipboardText = null; // 清理剪切板文本
            hasChanged = false; // 清理检查状态

            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}