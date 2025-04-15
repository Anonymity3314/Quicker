using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.CommonFunctions;
using System.Windows.Interop;
using Quicker.Windows.Menus;
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
    internal class ButtonManager
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
        public bool isClosing = false, isDragging, shouldHideTooltip; // 窗口关闭和拖拽状态、隐藏提示标志
        private readonly IconManager iconManager; // 图标管理器
        private readonly SettingDatabase db1; // 设置数据库
        private readonly ButtonDatabase db2; // 按钮数据库
        private Point initialMousePosition; // 鼠标初始位置
        private Button SourceButton; // 源按钮

        public ButtonManager()
        {
            db1 = new SettingDatabase(); // 初始化设置数据库
            db1.InitializeDatabase(); // 初始化设置数据库

            db2 = new ButtonDatabase(); // 初始化按钮数据库
            db2.InitializeDatabase(); // 初始化按钮数据库

            iconManager = new IconManager(); // 初始化图标管理器

            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取所有约定
            shouldHideTooltip = Convention.HideTooltip; // 获取隐藏提示标志
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
            e.Handled = true; // 标记事件已处理
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // 设置拖拽效果为复制
            }
        }

        /// <summary>
        /// 处理文件拖拽到按钮上
        /// </summary>
        /// <param name="sender">目标按钮</param>
        /// <param name="e">拖拽事件参数</param>
        public void Button_Drop(object sender, DragEventArgs e)
        {
            if (sender is Button TargetButton)
            {
                if (TargetButton == SourceButton || SourceButton == null) return; // 如果目标按钮和源按钮相同，直接返回
                if (e.Data.GetDataPresent(typeof(ButtonData))) // 获取拖拽数据
                {
                    db2.ExchangeButtonID(SourceButton.Name, TargetButton.Name); // 交换按钮编号

                    var TargetData = db2.GetButtonDataByID(SourceButton.Name); // 获取源按钮数据
                    RefreshButtonDisplay(SourceButton, TargetData, 60); // 更新 sourceButton 的内容
                    SourceButton.Tag = TargetData; // 更新 sourceButton 的标签

                    var SourceData = db2.GetButtonDataByID(TargetButton.Name); // 获取目标按钮数据
                    RefreshButtonDisplay(TargetButton, SourceData, 60); // 更新 targetButton 的内容
                    TargetButton.Tag = SourceData; // 更新 targetButton 的标签
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 如果拖拽的是文件
                {
                    string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop); // 获取文件路径
                    if (filePaths.Length > 0)
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // 获取文件路径
                        if (files.Length > 0)
                        {
                            string filePath = files[0]; // 获取第一个文件的路径
                            ProcessFileDrop(TargetButton, filePath); // 处理文件拖拽

                            var TargetData = db2.GetButtonDataByID(TargetButton.Name); // 获取目标按钮数据
                            RefreshButtonDisplay(TargetButton, TargetData, 60); // 更新目标按钮的内容
                            TargetButton.Tag = TargetData; // 更新 targetButton 的标签
                        }
                    }
                }

                SourceButton = null; // 清空源按钮
            }
        }

        /// <summary>
        /// 处理文件拖拽
        /// </summary>
        /// <param name="button">目标按钮</param>
        /// <param name="filePath">文件路径</param>
        private void ProcessFileDrop(Button button, string filePath)
        {
            ImageSource iconSource = iconManager.GetIcon(filePath); // 获取图标
            string iconPath = "none"; // 默认图标路径
            if (iconSource != null)
            {
                iconPath = iconManager.CheckCachedIcon(filePath); // 检查缓存的图标
                if (string.IsNullOrEmpty(iconPath))
                {
                    iconPath = iconManager.SaveIconToFile(iconSource); // 保存图标到文件
                }
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = button.Name, // 获取按钮ID
                ButtonName = fileName, // 设置按钮名称
                Location = filePath, // 设置文件路径
                ImagePath = iconPath, // 设置图标路径
                RunByMessager = false, // 是否使用管理员身份运行
                TryToOpenExitingWindow = true, // 尝试打开已存在的窗口
                WindowState = 0, // 设置窗口状态
                Usage = $"打开文件: {fileName}", // 设置用途
                CreateTime = DateTime.Now, // 设置创建时间
                LatestEditTime = DateTime.Now // 设置最新编辑时间
            };
            RefreshButtonDisplay(button, buttonData, 60); // 更新按钮内容
            db2.AddAction(buttonData); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 更新按钮内容
        /// </summary>
        /// <param name="button">目标按钮</param>
        /// <param name="buttonInformation">按钮数据</param>
        /// <param name="shouldHideTooltip">是否隐藏提示</param>
        public void RefreshButtonDisplay(Button button, ButtonData buttonInformation, int maxWidth)
        {
            if (buttonInformation != null) // 如果Button的数据存在
            {
                Grid grid = new(); // 创建Grid对象
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("White")); // 设置按钮背景
                if (buttonInformation.ImagePath != "none")
                {
                    try
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                        System.Windows.Controls.Image image = new()
                        {
                            Width = 36, // 设置宽度
                            Height = 36, // 设置高度
                            VerticalAlignment = VerticalAlignment.Center, // 垂直居中
                            HorizontalAlignment = HorizontalAlignment.Center, // 水平居中
                            Source = new BitmapImage(new Uri(buttonInformation.ImagePath)) // 设置图像源
                        }; // 创建图像对象
                        grid.Children.Add(image); // 添加图像到Grid
                        Grid.SetRow(image, 0); // 设置图像所在行
                    }
                    catch // 如果失败，发送信息提示
                    {
                        new ToastContentBuilder().AddText($"图标加载失败：按钮{buttonInformation.ButtonName}的图标被移动或删除").Show();
                    }
                } // 如果图标路径不为none

                if (!string.IsNullOrEmpty(buttonInformation.ButtonName))
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                    TextBlock textBlock = new()
                    {
                        Text = buttonInformation.ButtonName, // 设置文本
                        TextWrapping = TextWrapping.NoWrap, // 设置文本换行方式
                        VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直居中
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center, // 水平居中
                    }; // 创建文本块对象
                    AutoEllipsisTextBlock(textBlock, maxWidth); // 动态调整字体大小

                    grid.Children.Add(textBlock); // 添加文本块到Grid
                    Grid.SetRow(textBlock, 1); // 设置文本块所在行
                } // 如果按钮名称不为空
                button.Content = grid; // 设置按钮内容

                if (!shouldHideTooltip)
                {
                    string toolTipText = null; // 提示文本
                    if (!string.IsNullOrWhiteSpace(buttonInformation.ButtonName) || !string.IsNullOrWhiteSpace(buttonInformation.Usage))
                    {
                        string name = !string.IsNullOrWhiteSpace(buttonInformation.ButtonName) ? buttonInformation.ButtonName : null; // 获取按钮名称
                        string usage = !string.IsNullOrWhiteSpace(buttonInformation.Usage) ? buttonInformation.Usage : null; // 获取按钮用途
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
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F3F3")); // 重置按钮背景
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

        /// <summary>
        /// 打开指定菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="isMainWindow">是否为主面板</param>
        /// <param name="targetMenu">目标菜单</param>
        public void OpenMenu(object sender, bool isMainWindow, string targetMenu, Window sourceWindow)
        {
            Window menu = null;
            Button button = sender as Button;
            GeneralTransform transform = button.TransformToVisual(sourceWindow);
            Point position = transform.Transform(new Point(0, 0));
            if (isMainWindow) isClosing = true; // 如果是主窗口，设置关闭标志
            switch (targetMenu)
            {
                case "OperationMenu":
                    OperationMenu operationMenu = Application.Current.Windows.OfType<OperationMenu>().FirstOrDefault(); // 查找现有的操作菜单
                    operationMenu?.Close(); // 关闭操作菜单
                    operationMenu = new(button.Name)
                    {
                        Left = position.X,
                        Top = position.Y
                    }; // 设置菜单位置
                    if (isMainWindow)
                    {
                        operationMenu.ClosingOrHiding += () =>
                        {
                            isClosing = false; // 关闭标志
                        }; // 关闭或隐藏操作菜单事件
                    } // 如果是主窗口
                    operationMenu.Show(); // 显示操作菜单
                    break;
                case "CreatActionMenu":
                    CreatActionMenu creatActionMenu = Application.Current.Windows.OfType<CreatActionMenu>().FirstOrDefault(); // 查找现有的创建动作菜单
                    creatActionMenu?.Close(); // 关闭创建动作菜单
                    creatActionMenu = new(button.Name)
                    {
                        Left = position.X,
                        Top = position.Y
                    }; // 设置菜单位置
                    if (isMainWindow)
                    {
                        creatActionMenu.ClosingOrHiding += () =>
                        {
                            isClosing = false; // 关闭标志
                        }; // 关闭或隐藏创建动作菜单事件
                    } // 如果是主窗口
                    creatActionMenu.Show(); // 显示创建动作菜单
                    break;
                case "SelectActionPageMenu":
                    SelectActionPageMenu selectActionPageMenu = Application.Current.Windows.OfType<SelectActionPageMenu>().FirstOrDefault(); // 查找现有的选择动作页菜单
                    selectActionPageMenu?.Close();
                    selectActionPageMenu = new()
                    {
                        Left = position.X,
                        Top = position.Y
                    };
                    selectActionPageMenu.ClosingOrHiding += () =>
                    {
                        isClosing = false;
                    };
                    selectActionPageMenu.Show();

                    break;
            }
        }

        /// <summary>
        /// 关闭面板窗口
        /// </summary>
        /// <param name="window">要关闭面板窗口的窗口</param>
        public void CloseMainWindow(Window window)
        {
            MainWindow mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.Close();
            window.Close();
        }
    }
}