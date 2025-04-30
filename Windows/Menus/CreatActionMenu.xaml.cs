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
        private readonly ButtonManager buttonManager = new ButtonManager(); // 按钮管理器
        private readonly WindowManager windowManager = new WindowManager(); // 窗口管理器
        private readonly SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        public string CurrentButton { get; private set; } // 当前按钮
        public event Action? ClosingOrHiding; // 事件

        public CreatActionMenu(string currentbutton)
        {
            InitializeComponent();
            CurrentButton = currentbutton;
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 启动软件
        private void StartApp(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new(CurrentButton, 1);
            addWindow.Show();
            buttonManager.CloseMainWindow(this);
        }

        // 打开文件
        private void OpenDocument(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new(CurrentButton, 2);
            addWindow.Show();
            buttonManager.CloseMainWindow(this);
        }

        // 打开文件夹
        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new(CurrentButton, 3);
            addWindow.Show();
            buttonManager.CloseMainWindow(this);
        }

        // 打开网址
        private void OpenWebsite(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new(CurrentButton, 4);
            addWindow.Show();
            buttonManager.CloseMainWindow(this);
        }

        // 失去焦点时隐藏
        private void Window_Deactivated(object sender, EventArgs e)
        {
            ClosingOrHiding?.Invoke();
            this.Visibility = Visibility.Hidden;
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}