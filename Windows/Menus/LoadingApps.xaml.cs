using System.Windows;

namespace Quicker.Windows
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FindAppsWindow findAppsWindow = Application.Current.Windows.OfType<FindAppsWindow>().FirstOrDefault(); // 获取 FindAppsWindow 的实例
            findAppsWindow.LoadUWPApps(); // 从 UWP 应用商店加载
            findAppsWindow.LoadFromRegistry(); // 从注册表加载
            //findAppsWindow.LoadFromCommonPaths(); // 从常用路径加载
            this.Close();
        }
    }
}