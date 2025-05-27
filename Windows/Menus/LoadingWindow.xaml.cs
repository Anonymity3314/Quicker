using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        // 关闭窗口前释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            GC.Collect(); // 强制回收非托管资源
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制回收非托管资源
        }
    }
}