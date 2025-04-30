using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class SelectActionPageMenu : Window
    {
        public event Action? ClosingOrHiding; // 关闭或隐藏操作菜单事件

        public SelectActionPageMenu()
        {
            InitializeComponent();
        }

        // 失去焦点关闭窗口
        private void SelectActionPageMenu_Deactivated(object sender, EventArgs e)
        {
            ClosingOrHiding?.Invoke();
            this.Visibility = Visibility.Hidden; // 失去焦点关闭窗口
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            ClosingOrHiding = null; // 清理事件处理器
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
