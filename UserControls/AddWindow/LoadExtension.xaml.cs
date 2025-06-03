using System.Windows.Controls;

namespace Quicker.UserControls.AddWindow
{
    public partial class LoadExtension : UserControl
    {
        private Quicker.Windows.MainWindows.AddWindow _addWindow; // AddWindow 的引用

        public LoadExtension(Quicker.Windows.MainWindows.AddWindow addWindow)
        {
            _addWindow = addWindow; // 保存 AddWindow 的引用
            InitializeComponent();
        }
    }
}