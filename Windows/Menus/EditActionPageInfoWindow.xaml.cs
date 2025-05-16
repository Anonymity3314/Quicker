using Quicker.Database;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class EditActionPageInfoWindow : Window
    {
        private readonly ActionPageDatabase db3 = new(); // 动作页数据库

        public EditActionPageInfoWindow()
        {
            InitializeComponent();
        }

        // 保存动作页信息
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 取消编辑动作页
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }
    }
}