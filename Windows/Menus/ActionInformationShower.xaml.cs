using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;
using System.IO;
using System;
using Quicker;

namespace Quicker.Windows
{
    public partial class ActionInformationWindow : Window
    {
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
        private WindowManager windowManager = new WindowManager(); // 窗口管理器
        public string CurrentButton { get; private set; } // 当前按钮ID

        public ActionInformationWindow(string currentbutton)
        {
            InitializeComponent();
            CurrentButton = currentbutton;
            InitializeWindow(); // 初始化窗口
            windowManager.SetWindowTopmost(this);
        }

        // 初始化信息窗口
        private void InitializeWindow()
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据
            IDLabel.Content = buttonData.ButtonID; // 初始化动作ID
            TitleLabel.Content = buttonData.ButtonName; // 初始化动作名称
            UsageLabel.Content = buttonData.Usage; // 初始化动作用途
            try
            {
                Image.Source = new BitmapImage(new Uri(buttonData.ImagePath)); // 初始化动作图像
            }
            catch
            {
                new ToastContentBuilder().AddText($"图标加载失败：按钮{buttonData.ButtonName}的图标被移动或删除").Show(); // 显示图标加载失败的通知
            }
            CreatTimeLabel.Content = buttonData.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"); // 初始化创建时间
            LatestEditTimeLabel.Content = buttonData.LatestEditTime.ToString("yyyy-MM-dd HH:mm:ss"); // 初始化最后编辑时间
        }

        // 复制动作信息
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonData buttonData = db2.GetButtonDataByID(CurrentButton); // 获取按钮数据
            string textToCopy = $"ID:{buttonData.ButtonID}\n" +
                                $"标题:{buttonData.ButtonName}\n" +
                                $"说明:{buttonData.Usage}\n" +
                                $"URI:quicker:runaction:{buttonData.ButtonID}"; // 复制的文本内容
            Clipboard.SetText(textToCopy); // 复制文本到剪贴板
            new ToastContentBuilder().AddText("已复制!").Show(); // 显示复制成功的通知
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