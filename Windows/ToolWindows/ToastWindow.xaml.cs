using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Quicker.Windows.ToolWindows
{
    public partial class ToastWindow : Window
    {
        private const string commonColor = "#147EC9"; // 示例通用颜色
        private const string errorColor = "#F5A300"; // 示例错误颜色
        private const string warningColor = "Red"; // 示例警告颜色
        private const string successColor = "#11AD45"; // 示例成功颜色

        private Dictionary<Border, DispatcherTimer> timerDictionary = new(); // 用于存储计时器
        private Queue<Message> messageQueue = new(); // 创建消息队列

        public ToastWindow()
        {
            InitializeComponent();
            double screenHeight = SystemParameters.WorkArea.Height; // 获取屏幕高度
            Height = screenHeight; // 设置窗口高度为屏幕高度
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
            if (ToastStackPanel.Children.Count < 5) // 判断消息数量是否达到上限
            {
                if (messageQueue.Count > 0) // 判断消息队列是否有消息
                {
                    var msg = messageQueue.Dequeue(); // 从队列中取出消息
                    ShowToast(msg); // 显示消息
                }
                else // 没有消息
                {
                    Close(); // 关闭窗口
                }
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
            Grid grid = new Grid();
            border.Child = grid; // 将消息添加到边框中
            grid.Children.Add(textblock); // 将消息添加到网格中

            // 初始化关闭按钮，并传入边框对象
            var closeToastButton = InitalizeCloseButton(border);
            closeToastButton.Click += (s, e) => DeleteToast(border); // 关闭按钮点击事件
            grid.Children.Add(closeToastButton); // 将关闭按钮添加到网格中

            InitializeAnimation(border); // 初始化动画

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
                Width = 400, // 设置边框宽度
                Opacity = 0, // 设置初始不透明度
                Margin = new Thickness(0, 5, 0, 5), // 设置边距
                CornerRadius = new CornerRadius(5) // 设置边框圆角
            }; // 创建消息边框

            string color = ""; // 根据消息类型设置边框颜色
            switch (toastType)
            {
                case "Common":
                    color = commonColor; // 示例通用颜色
                    break;
                case "Error":
                    color = errorColor; // 示例错误颜色
                    break;
                case "Warning":
                    color = warningColor; // 示例警告颜色
                    break;
                case "Success":
                    color = successColor; // 示例成功颜色
                    break;
            }
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); // 设置边框颜色
            ToastStackPanel.Children.Add(border); // 添加消息边框到消息面板
            return border; // 返回消息边框
        }

        /// <summary>
        /// 初始化关闭按钮
        /// </summary>
        /// <returns> 关闭按钮 </returns>
        private Button InitalizeCloseButton(Border border)
        {
            Button button = new Button() { Style = (Style)FindResource("CloseToastButton") }; // 设置按钮样式

            // 添加鼠标进入和离开事件处理程序
            button.MouseEnter += (s, e) =>
            {
                e.Handled = true; // 阻止事件冒泡
                if (border.Background is SolidColorBrush borderBrush)
                {
                    Color originalColor = borderBrush.Color; // 获取边框颜色
                    Color darkerColor = Color.FromRgb(
                        (byte)(originalColor.R * 0.9),
                        (byte)(originalColor.G * 0.9),
                        (byte)(originalColor.B * 0.9)
                    ); // 使背景颜色更深
                    button.Background = new SolidColorBrush(darkerColor); // 设置按钮的背景色
                }
            }; // 鼠标进入事件

            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush(Colors.Transparent); // 恢复按钮的背景色
            }; // 鼠标离开事件
            return button; // 返回关闭按钮
        }

        /// <summary>
        /// 初始化消息
        /// </summary>
        /// <param name="message"> 消息内容 </param>
        private TextBlock InitalizeToast(string message)
        {
            TextBlock textBlock = new TextBlock()
            {
                FontSize = 16, // 设置字体大小
                Text = message, // 设置消息内容
                TextWrapping = TextWrapping.Wrap, // 设置消息内容自动换行
                Margin = new Thickness(20, 20, 20, 20), // 设置边距
                VerticalAlignment = VerticalAlignment.Center, // 设置垂直对齐方式
                Foreground = new SolidColorBrush(Colors.White) // 设置字体颜色
            }; // 创建消息文本框
            return textBlock; // 返回消息
        }

        // 初始化动画
        private void InitializeAnimation(Border border)
        {
            Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInAnimation"); // 获取淡入动画
            fadeInStoryboard.Begin(border); // 开始播放淡入动画
        }

        // 计时器事件
        private void Timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender; // 获取计时器
            Border border = (Border)timer.Tag; // 获取消息边框
            if (border != null)
            {
                // 获取淡出动画并开始播放
                Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutAnimation"); // 获取淡出动画
                fadeOutStoryboard.Completed += (s, arg) =>
                {
                    DeleteToast(border); // 删除消息
                }; // 淡出动画完成时删除消息
                fadeOutStoryboard.Begin(border); // 开始播放淡出动画

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
            if (timerDictionary.ContainsKey(border)) // 判断计时器是否存在
            {
                DispatcherTimer timer = timerDictionary[border]; // 获取计时器
                ToastStackPanel.Children.Remove(border); // 从消息面板中删除消息边框
                timerDictionary.Remove(border); // 从字典中移除计时器
                timer.Stop(); // 停止计时器
                timer.Tick -= Timer_Tick; // 移除计时器事件
                timer = null; // 释放计时器
            }
            CheckAndDisplayToast(); // 检查并显示新的消息
        }

        // 关闭窗口释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 关闭窗口
            messageQueue.Clear(); // 清空消息队列
            foreach (DispatcherTimer timer in timerDictionary.Values)
            {
                timer.Stop(); // 停止所有计时器
                timer.Tick -= Timer_Tick; // 移除计时器事件
            } // 释放所有计时器资源
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