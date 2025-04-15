using System.Runtime.InteropServices;
using Quicker.CommonFunctions;
using System.Windows.Interop;
using Quicker.Database;
using System.Windows;

namespace Quicker.Windows
{
    public partial class CreatActionMenu : Window
    {
        public string CurrentButton { get; private set; } // 当前按钮
        private readonly ButtonManager buttonManager; // 按钮管理器
        private readonly WindowManager windowManager; // 窗口管理器
        private readonly SettingDatabase db1; // 设置数据库
        public event Action? ClosingOrHiding; // 事件

        public CreatActionMenu(string currentbutton)
        {
            InitializeComponent();
            CurrentButton = currentbutton;

            db1 = new SettingDatabase();
            buttonManager = new ButtonManager();
            windowManager = new WindowManager();
            windowManager.SetWindowTopmost(this);
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
    }
}
