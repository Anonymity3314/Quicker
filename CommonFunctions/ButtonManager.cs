using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.CommonFunctions;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;
using IWshRuntimeLibrary;
using System.Diagnostics;
using System.Threading;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using System.IO;

namespace Quicker.CommonFunctions
{
    // 按钮管理器接口
    public interface IButtonManager
    {
        void Button_DragEnter(object sender, DragEventArgs e);
        void Button_PreviewDragOver(object sender, DragEventArgs e);
        void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        void Button_PreviewMouseMove(object sender, MouseEventArgs e);
        void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e);
        void UpdateButtonContent(Button button, ButtonData data, bool hideTooltip, int maxWidth);
        void AutoEllipsisTextBlock(TextBlock textBlock, int maxWidth);
    }

    // 按钮管理器类
    internal class ButtonManager : IButtonManager
    {
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) yield break; // 如果对象为空，停止枚举

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i); // 获取子元素
                if (child is T tChild) yield return tChild; // 如果是目标类型，返回
                foreach (var grandChild in FindVisualChildren<T>(child)) yield return grandChild; // 递归查找子元素的子元素
            }
        } // 递归查找所有指定类型的子元素
        public bool isClosing = false, isDragging = false, hideTooltip; // 窗口关闭和拖拽状态、隐藏提示标志
        private Point initialMousePosition; // 鼠标初始位置
        private readonly SettingDatabase db1; // 设置数据库
        private readonly ButtonDatabase db2; // 按钮数据库
        Button SourceButton; // 源按钮

        public ButtonManager()
        {
            db1 = new SettingDatabase();
            db1.InitializeDatabase();

            db2 = new ButtonDatabase();
            db2.InitializeDatabase();
        }

        public void Initialize()
        {
            var Convention = db1.GetAllConventions().FirstOrDefault();
            hideTooltip = Convention.HideTooltip;
        }

        // 设置按钮拖拽效果
        public void Button_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move; // 设置拖拽效果为移动
            e.Handled = true; // 标记事件已处理
        }

        // 设置按钮拖拽时的效果
        public void Button_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        // 鼠标左键按下时记录初始位置
        public void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                initialMousePosition = e.GetPosition(button); // 记录初始位置
                SourceButton = button; // 记录源按钮
                isDragging = false; // 初始化拖拽状态
            }
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPosition = e.GetPosition(button); // 获取当前位置
                double deltaX = currentPosition.X - initialMousePosition.X; // 计算 X 轴位移
                double deltaY = currentPosition.Y - initialMousePosition.Y; // 计算 Y 轴位移
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY); // 计算移动距离
                if (distance > 10 && !isDragging) // 如果移动距离超过 10 像素，则视为拖拽开始
                {
                    isDragging = true; // 设置拖拽状态
                    if (button.Tag is ButtonData data)
                    {
                        DragDrop.DoDragDrop(button, data, DragDropEffects.Move); // 开始拖拽操作
                    }
                }
            }
        }

        // 鼠标左键释放时重置状态
        public void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // 重置拖拽状态
        }

        // 加载 Button 数据
        public void UpdateButtonContent(Button button, ButtonData data, bool hideTooltip, int maxWidth)
        {
            if (data != null) // 如果Button的数据存在
            {
                Grid grid = new(); // 创建Grid对象
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景
                if (data.ImagePath != "none")
                {
                    try
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                        Image image = new()
                        {
                            Source = new BitmapImage(new Uri(data.ImagePath)), // 设置图像源
                            Width = 36, // 设置宽度
                            Height = 36, // 设置高度
                            VerticalAlignment = VerticalAlignment.Center, // 垂直居中
                            HorizontalAlignment = HorizontalAlignment.Center // 水平居中
                        }; // 创建图像对象
                        grid.Children.Add(image); // 添加图像到Grid
                        Grid.SetRow(image, 0); // 设置图像所在行
                    }
                    catch // 如果失败，发送信息提示
                    {
                        new ToastContentBuilder().AddText($"图标加载失败：按钮{data.ButtonName}的图标被移动或删除").Show();
                    }
                } // 如果图标路径不为none

                if (!string.IsNullOrEmpty(data.ButtonName))
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                    TextBlock textBlock = new()
                    {
                        Text = data.ButtonName, // 设置文本
                        TextWrapping = TextWrapping.NoWrap, // 设置文本换行方式
                        VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直居中
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center // 水平居中
                    }; // 创建文本块对象
                    AutoEllipsisTextBlock(textBlock, maxWidth); // 动态调整字体大小

                    grid.Children.Add(textBlock); // 添加文本块到Grid
                    Grid.SetRow(textBlock, 1); // 设置文本块所在行
                } // 如果按钮名称不为空
                button.Content = grid; // 设置按钮内容

                if (!hideTooltip)
                {
                    string toolTipText = null; // 提示文本
                    if (!string.IsNullOrWhiteSpace(data.ButtonName) || !string.IsNullOrWhiteSpace(data.Usage))
                    {
                        string name = !string.IsNullOrWhiteSpace(data.ButtonName) ? data.ButtonName : null; // 获取按钮名称
                        string usage = !string.IsNullOrWhiteSpace(data.Usage) ? data.Usage : null; // 获取按钮用途
                        toolTipText = (name + "\n" + usage).Trim('\n'); // 设置按钮提示文本
                    } // 如果按钮名称或用途不为空
                    button.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置按钮提示文本
                }
            }
            else // 如果Button的数据不存在
            {
                button.Content = null; // 清空按钮内容
                button.ToolTip = null; // 清空按钮提示文本
                button.Tag = null; // 清空按钮标签
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 重置按钮背景
            }
        }

        /// <summary>
        /// 动态调整TextBlock的字体大小以适应最大宽度
        /// </summary>
        /// <param name="textBlock">指定的TextBlock</param>
        /// <param name="maxWidth">最大宽度</param>
        public void AutoEllipsisTextBlock(TextBlock textBlock, int maxWidth)
        {
            if (string.IsNullOrEmpty(textBlock.Text)) return; // 如果文本为空，直接返回
            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
            double textWidth = textBlock.DesiredSize.Width; // 获取文本宽度           
            if (textWidth <= maxWidth) return; // 如果文本宽度小于等于最大宽度，直接返回

            string originalText = textBlock.Text; // 获取原始文本
            string ellipsis = "..."; // 设置省略号
            string truncatedText = originalText; // 初始化截断文本
            while (true) // 循环直到文本宽度小于等于最大宽度
            {
                truncatedText = truncatedText.Substring(0, truncatedText.Length - 1); // 截断文本
                string newText = truncatedText + ellipsis; // 添加省略号
                textBlock.Text = newText; // 更新 TextBlock 的文本
                textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
                double newWidth = textBlock.DesiredSize.Width; // 获取新文本宽度
                if (newWidth <= maxWidth) break; // 如果新文本宽度小于等于最大宽度，退出循环
            }
            textBlock.Text = truncatedText + ellipsis; // 更新 TextBlock 的文本
        }

        // 右键按钮打开菜单
        public void OpenCreatActionMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                isClosing = true; // 设置关闭标志
                Point mousePosition = e.GetPosition(button); // 获取鼠标位置
                double left = mousePosition.X + 310.4, top = mousePosition.Y + 596 / 3; // 计算菜单位置
                if (button.Tag is ButtonData data && button != null)
                {
                    OperationMenu operationMenu = Application.Current.Windows.OfType<OperationMenu>().FirstOrDefault(); // 查找现有的操作菜单
                    operationMenu?.Close(); // 关闭操作菜单
                    operationMenu = new(button.Name)
                    {
                        Left = left,
                        Top = top
                    }; // 设置菜单位置
                    operationMenu.ClosingOrHiding += () =>
                    {
                        isClosing = false;
                    };
                    operationMenu.Show(); // 显示操作菜单
                }
                else
                {
                    CreatActionMenu creatActionMenu = Application.Current.Windows.OfType<CreatActionMenu>().FirstOrDefault(); // 查找现有的创建动作菜单
                    creatActionMenu?.Close(); // 关闭创建动作菜单
                    creatActionMenu = new(button.Name)
                    {
                        Left = left,
                        Top = top
                    }; // 设置菜单位置
                    creatActionMenu.ClosingOrHiding += () =>
                    {
                        isClosing = false;
                    };
                    creatActionMenu.Show(); // 显示创建动作菜单
                }
            }
        }
    }
}