using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Quicker.Windows.Menus
{
    public partial class ToastWindow : Window
    {
        private const string commonColor = "#FF326CF3";
        private const string errorColor = "#FFFF6700";
        private const string warningColor = "Red";

        private Dictionary<Border, DispatcherTimer> timerDictionary = new(); // 用于存储计时器
        private Queue<Message> messageQueue = new(); // 创建消息队列

        public ToastWindow()
        {
            InitializeComponent();
            double screenHeight = SystemParameters.WorkArea.Height; // 获取屏幕高度
            this.Height = screenHeight; // 设置窗口高度为屏幕高度
            Closed += ToastWindow_Closed; // 添加窗口关闭事件
        }

        private void ToastWindow_Closed(object sender, EventArgs e)
        {
            // 在窗口关闭时清理资源
            foreach (DispatcherTimer timer in timerDictionary.Values)
            {
                timer.Stop(); // 停止计时器
                timer.Tick -= Timer_Tick; // 移除计时器事件
            }
            timerDictionary.Clear(); // 清空字典
            GC.Collect(); // 强制进行垃圾回收
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        /// <param name="toastType"> 消息类型 </param>
        public void AddToast(string message, string toastType)
        {
            var msg = new Message { Content = message, Type = toastType }; // 创建消息对象
            messageQueue.Enqueue(msg); // 将消息添加到队列中

            CheckAndDisplayToast(); // 检查并显示消息
        }

        // 检查并显示消息
        private void CheckAndDisplayToast()
        {
            if (ToastStackPanel.Children.Count < 5 && messageQueue.Count > 0)
            {
                var msg = messageQueue.Dequeue(); // 从队列中取出消息
                ShowToast(msg); // 显示消息
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="msg"> 消息内容 </param>
        private void ShowToast(Message msg)
        {
            var border = InitializeBoarder(msg.Type); // 初始化边框
            var textblock = InitalizeToast(msg.Content); // 初始化消息
            border.Child = textblock; // 将消息添加到边框中

            // 设置初始透明度为0
            border.Opacity = 0;

            // 创建淡入动画
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.2)));
            Storyboard.SetTarget(fadeIn, border);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            Storyboard fadeInStoryboard = new Storyboard();
            fadeInStoryboard.Children.Add(fadeIn);
            fadeInStoryboard.Begin();

            // 为每个消息创建一个独立的计时器
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // 设置计时器时间为3秒
            timer.Tag = border; // 将边框对象存储在计时器的Tag属性中
            timer.Tick += Timer_Tick; // 计时器事件

            timerDictionary[border] = timer; // 将计时器存储到字典中
            timer.Start(); // 启动计时器
        }

        /// <summary>
        /// 初始化边框
        /// </summary>
        /// <param name="toastType"> 消息类型 </param>
        private Border InitializeBoarder(string toastType)
        {
            Border border = new Border()
            {
                Width = 400,
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(5)
            }; // 创建消息边框

            string color = ""; // 根据消息类型设置边框颜色
            switch (toastType)
            {
                case "Common":
                    color = commonColor;
                    break;
                case "Error":
                    color = errorColor; // 示例错误颜色
                    break;
                case "Warning":
                    color = warningColor; // 示例警告颜色
                    break;
            }
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); // 设置边框颜色
            ToastStackPanel.Children.Add(border); // 添加消息边框到消息面板
            return border; // 返回消息边框
        }

        /// <summary>
        /// 初始化消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        private TextBlock InitalizeToast(string message)
        {
            TextBlock textBlock = new TextBlock()
            {
                FontSize = 16,
                Text = message,
                Margin = new Thickness(0, 20, 0, 20),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            return textBlock; // 返回消息
        }

        // 计时器事件
        private void Timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            Border border = (Border)timer.Tag; // 获取消息边框
            if (border != null)
            {
                // 创建淡出动画
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.2)));
                Storyboard.SetTarget(fadeOut, border);
                Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
                Storyboard fadeOutStoryboard = new Storyboard();
                fadeOutStoryboard.Children.Add(fadeOut);

                // 淡出动画完成时删除消息
                fadeOut.Completed += (s, arg) =>
                {
                    DeleteToast(border); // 删除消息
                };

                fadeOutStoryboard.Begin();

                timer.Stop(); // 停止计时器
                timer.Tick -= Timer_Tick; // 移除计时器事件
                timer = null; // 释放计时器
            }
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="border"> 消息边框 </param>
        private void DeleteToast(Border border)
        {
            if (ToastStackPanel.Children.Contains(border)) // 判断消息边框是否存在于消息面板中
            {
                if (timerDictionary.ContainsKey(border))
                {
                    DispatcherTimer timer = timerDictionary[border];
                    timer.Stop(); // 停止计时器
                    timer.Tick -= Timer_Tick; // 移除计时器事件
                    timerDictionary.Remove(border); // 从字典中移除计时器
                    timer = null; // 释放计时器
                }

                ToastStackPanel.Children.Remove(border); // 从消息面板中删除消息边框
                CheckAndDisplayToast(); // 检查并显示新的消息
            }
        }

        // 关闭窗口释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            foreach (DispatcherTimer timer in timerDictionary.Values)
            {
                timer.Stop(); // 停止所有计时器
                timer.Tick -= Timer_Tick; // 移除计时器事件
            }
            timerDictionary.Clear(); // 清空字典
            GC.Collect(); // 垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次进行垃圾回收
        }
    }

    // 消息类
    public class Message
    {
        public string Content { get; set; } // 消息内容
        public string Type { get; set; } // 消息类型
    }
}