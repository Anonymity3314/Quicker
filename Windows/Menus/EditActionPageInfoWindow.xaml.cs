using Quicker.Database;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class EditActionPageInfoWindow : Window
    {
        private readonly ActionPageDatabase db3 = new(); // 动作页数据库
        private readonly string actionPageIndex; // 动作页索引
        private readonly string actionPageType; // 动作页类型

        public EditActionPageInfoWindow(string actionPageType, string actionPageIndex)
        {
            InitializeComponent();
            this.actionPageType = actionPageType;
            this.actionPageIndex = actionPageIndex;
        }

        // 加载动作页信息
        private void EditActionPageInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var actionPage = db3.GetActionPageData(actionPageType).FirstOrDefault();
            ActionPageTag.Text = actionPage.ActionPageTag;
            string[] actionPageNames = actionPage.ActionPageName.Split(';');
            ActionPageName.Text = actionPageNames[int.Parse(actionPageIndex)];
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