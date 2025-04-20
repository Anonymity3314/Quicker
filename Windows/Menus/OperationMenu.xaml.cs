using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Quicker.Database;
using Quicker.Managers;
using System.Windows;
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
        private readonly ButtonManager buttonManager; // 按钮管理器
        private readonly WindowManager windowManager; // 窗口管理器
        public event Action? ClosingOrHiding; // 关闭或隐藏操作菜单事件
        private readonly ButtonDatabase db2; // 按钮数据库

        public OperationMenu(string currentbutton)
        {
            InitializeComponent(); // 初始化窗口
            CurrentButton = currentbutton; // 设置当前按钮

            db2 = new ButtonDatabase(); // 初始化按钮数据库
            buttonManager = new ButtonManager(); // 初始化按钮管理器
            windowManager = new WindowManager(); // 初始化窗口管理器
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        // 编辑动作信息
        private void EditeInformation_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow(CurrentButton, 0);
            addWindow.Show();
            buttonManager.CloseMainWindow(this);
        }

        // 删除动作
        private async void DeleteAction_Click(object sender, RoutedEventArgs e)
        {
            db2.DeleteAction(CurrentButton);
            buttonManager.CloseMainWindow(this);
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
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton);
            string filePath = buttonData.Location; // 指定文件路径
            filePath = System.IO.Path.GetFullPath(filePath); // 获取文件的绝对路径
            IntPtr pidlList = ILCreateFromPathW(filePath); // 打开文件夹并选中文件
            if (pidlList != IntPtr.Zero)
            {
                try // 打开文件夹并选中文件
                {
                    Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                }
                finally // 释放资源
                {
                    ILFree(pidlList);
                }
            }

            buttonManager.CloseMainWindow(this);
        }

        // 失去焦点时关闭操作菜单
        private void OperationMenu_Deactivated(object sender, EventArgs e)
        {
            ClosingOrHiding?.Invoke();
            this.Visibility = Visibility.Hidden;
        }        
    }
}