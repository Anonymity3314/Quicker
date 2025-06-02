using System.Windows;

namespace Quicker.Windows.ToolWindows
{
    public partial class EditSceneWindow : Window
    {
        public EditSceneWindow(string sceneType)
        {
            InitializeComponent();
        }

        // 点击按钮保存场景信息
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 点击按钮关闭窗口
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }
    }
}