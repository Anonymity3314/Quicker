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
    }
}
