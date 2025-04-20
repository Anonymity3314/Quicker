using System.Windows.Controls;
using System.Windows;

namespace Quicker.Windows.Forms.SettingWindowGrids
{
    public partial class AppearanceGrid : UserControl
    {
        public AppearanceGrid()
        {
            InitializeComponent();
        }

        // 鼠标移入界面显示滚动条
        private void ScrollViewer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceButtonGridScrollBar.Visibility = System.Windows.Visibility.Visible; // 显示滚动条
        }

        // 鼠标移出界面隐藏滚动条
        private void ScrollViewer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AppearanceButtonGridScrollBar.Visibility = System.Windows.Visibility.Hidden; // 隐藏滚动条
        }
    }
}