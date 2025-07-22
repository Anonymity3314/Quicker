using Quicker.Windows.EditWindows;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class EditSceneMenu : Window
    {
        private string SceneTag { get; set; } // 场景标签
        public EditSceneMenu(string sceneTag)
        {
            InitializeComponent();
            SceneTag = sceneTag; // 设置场景标签

        }

        private void EditSceneMenu_Loaded(object sender, RoutedEventArgs e)
        {
            WindowManager windowManager = new(); // 实例化窗口管理器
            windowManager.SetWindowPositionNearMouse(this); // 设置窗口位置
        }

        // 点击按钮后打开编辑场景窗口
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditSceneWindow editSceneWindow = new(SceneTag); // 实例化编辑场景窗口
            editSceneWindow.ShowDialog(); // 显示编辑场景窗口
        }

        // 点击空白处关闭菜单
        private void EditSceneMenu_Deactivated(object sender, EventArgs e)
        {
            Close(); // 点击空白处关闭菜单
        }
    }
}