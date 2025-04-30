using System.Windows;

namespace Quicker.Windows
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        private void LoadingWindow_ContentRendered(object sender, EventArgs e)
        {
            FindAppsWindow findAppsWindow = Application.Current.Windows.OfType<FindAppsWindow>().FirstOrDefault(); // 获取 FindAppsWindow 的实例
            if (findAppsWindow != null)
            {
                findAppsWindow.LoadUWPApps(); // 从 UWP 应用商店加载
                findAppsWindow.LoadFromRegistry(); // 从注册表加载
                //findAppsWindow.LoadFromCommonPaths(); // 从常用路径加载
                this.Close();
            }
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            this.ContentRendered -= LoadingWindow_ContentRendered; // 清理事件处理器

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}