using System.Windows.Media.Imaging;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;

namespace Quicker.Windows
{
    public partial class ActionInformationWindow : Window
    {
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        public int CurrentButton { get; private set; } // 当前按钮ID
        public string TableName { get; private set; } // 表名

        public ActionInformationWindow(int currentbutton, string tableName)
        {
            InitializeComponent();
            CurrentButton = currentbutton;
            TableName = tableName;
            InitializeWindow(); // 初始化窗口
            WindowManager.SetWindowTopmost(this);
        }

        // 初始化信息窗口
        private void InitializeWindow()
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton, TableName); // 获取按钮数据
            IDLabel.Content = buttonData.ButtonID; // 初始化动作ID
            TitleLabel.Content = buttonData.Title; // 初始化动作名称
            UsageLabel.Content = buttonData.Description; // 初始化动作用途
            if(!string.IsNullOrEmpty(buttonData.ImagePath))
            {
                try
                {
                    Image.Source = new BitmapImage(new Uri(buttonData.ImagePath)); // 初始化动作图像
                }
                catch
                {
                    ToastManager.AddToast($"图标加载失败：按钮{buttonData.Title}的图标被移动或删除", "Error"); // 显示图标加载失败的通知
                }
            }
            CreatTimeLabel.Content = buttonData.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"); // 初始化创建时间
            LatestEditTimeLabel.Content = buttonData.LatestEditTime.ToString("yyyy-MM-dd HH:mm:ss"); // 初始化最后编辑时间
        }

        // 复制动作信息
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton, TableName); // 获取按钮数据
            string textToCopy = $"ID:{buttonData.ButtonID}\n" +
                                $"标题:{buttonData.Title}\n" +
                                $"说明:{buttonData.Description}\n" +
                                $"URI:quicker:runaction:{buttonData.ButtonID}"; // 复制的文本内容
            Clipboard.SetText(textToCopy); // 复制文本到剪贴板
            ToastManager.AddToast("已复制!", "Common"); // 显示复制成功的通知
        }

        // 关闭动作信息窗口
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法
            Image.Source = null; // 释放图像资源

            // 清理事件处理器
            CopyButton.Click -= CopyButton_Click;
            CloseButton.Click -= CloseButton_Click;

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}