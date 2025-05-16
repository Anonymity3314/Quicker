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
            this.actionPageType = actionPageType; // 动作页类型
            this.actionPageIndex = actionPageIndex; // 动作页索引
        }

        // 加载动作页信息
        private void EditActionPageInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var actionPage = db3.GetActionPageData(actionPageType).FirstOrDefault(); // 获取动作页数据
            ActionPageTag.Text = actionPage.ActionPageTag; // 设置动作页标签
            string[] actionPageNames = actionPage.ActionPageName.Split(';'); // 提前动作页信息
            ActionPageName.Text = actionPageNames[int.Parse(actionPageIndex)]; // 设置动作页名称
        }

        // 保存动作页信息
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var actionPage = db3.GetActionPageData(actionPageType).FirstOrDefault();
            db3.UpdateActionPageTable(actionPageType, actionPage.ActionPageType, actionPage.ActionPageIconPath, actionPage.ActionPageCount, actionPage.ActionPageTag, ActionPageName.Text);
            this.Close(); // 关闭窗口
        }

        // 取消编辑动作页
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }
    }
}