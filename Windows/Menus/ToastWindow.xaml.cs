using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class ToastWindow : Window
    {
        public ToastWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        /// <param name="ToastType"> 消息类型 </param>
        public void AddToast(string message, string toastType)
        {
            InitializeBoarder(toastType);
        }

        /// <summary>
        /// 初始化边框
        /// </summary>
        /// <param name="toastType"> 消息类型 </param>
        private void InitializeBoarder(string toastType)
        {
            Border border = new Border()
            {
                Width = 400,
                Height = 100,
                CornerRadius = new CornerRadius(5),
            };

            string color = "";
            switch (toastType)
            {
                case "Common":
                    break;
                case "Error":
                    break;
                case "Warning":
                    break;
            }
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

            ToastStackPanel.Children.Add(border);
        }

        /// <summary>
        /// 初始化消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        private void InitalizeToast(string message)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = message,
            };
        }
    }
}