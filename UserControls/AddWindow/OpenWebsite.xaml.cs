using System.Windows.Controls;
using System.Windows;

namespace Quicker.UserControls.AddWindow
{
    public partial class OpenWebsite : UserControl
    {
        public OpenWebsite(Quicker.AddWindow addWindow)
        {
            InitializeComponent();
        }


        // 打开菜单
        private void OpenContextMenu(object sender, RoutedEventArgs e)
        {
            OpenWebsitePopup.IsOpen = true; // 打开弹出菜单
        }
    }
}