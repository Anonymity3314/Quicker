using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.CommonFunctions;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Database;
using System.Windows;

namespace Quicker.Windows
{
    public partial class ActionPageManageWindow : Window
    {
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T tChild) yield return tChild;
                foreach (var grandChild in FindVisualChildren<T>(child)) yield return grandChild;
            }
        } // 查找子元素

        private int TotalGlobalAntionPageIndex, TotalCommonActionPageIndex; // 全局和公共动作页索引
        private Dictionary<string, ButtonData> buttonDataDict; // 按钮数据字典
        private bool shouldHideTooltip, isDragging = false; // 是否正在拖拽
        private readonly ButtonManager buttonManager; // 按钮管理器
        private readonly SettingDatabase db1; // 设置数据库
        private readonly ButtonDatabase db2; // 按钮数据库
        private Point initialMousePosition; // 初始鼠标位置
        private Button SourceButton; // 源按钮

        public ActionPageManageWindow()
        {
            InitializeComponent(); // 初始化窗口
            GlobalStackPanel.Children.Clear(); // 清空全局堆栈面板

            db1 = new SettingDatabase(); // 初始化设置数据库
            db1.InitializeDatabase(); // 初始化设置数据库

            db2 = new ButtonDatabase(); // 初始化按钮数据库
            db2.InitializeDatabase(); // 初始化按钮数据库

            buttonManager = new ButtonManager(); // 初始化按钮管理器
        }

        // 窗口加载事件
        private async void ActionPageManageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var buttonDataList =  db2.GetAllButtonData(); // 获取所有按钮数据
            buttonDataDict = buttonDataList.ToDictionary(data => data.ButtonID); // 将按钮数据转换为字典

            GetTotalAntionPageIndex(); // 获取全局和公共动作页索引
            LoadGlobalCanvas(); // 加载全局画布

            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取约定
            shouldHideTooltip = Convention.HideTooltip; // 是否隐藏工具提示
        }

        // 获取全局和公共动作页索引
        private void GetTotalAntionPageIndex()
        {
            TotalGlobalAntionPageIndex = 0; // 全局动作页索引
            TotalCommonActionPageIndex = 0; // 公共动作页索引
            if (buttonDataDict == null || buttonDataDict.Count == 0) return; // 如果按钮数据字典为空，则返回
            foreach (var data in buttonDataDict.Values)
            {
                string buttonID = data.ButtonID; // 按钮ID
                Match match = Regex.Match(data.ButtonID, @"^([a-zA-Z0-9_]+)(\d{3})$"); // 正则表达式匹配
                if (match.Success)
                {
                    string style = match.Groups[1].Value; // 样式
                    string numbersStr = match.Groups[2].Value; // 数字字符串
                    int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray(); // 将数字字符串转换为整数数组

                    if (style == "Global") // 如果样式为全局
                    {
                        if (numbers[0] > TotalGlobalAntionPageIndex) TotalGlobalAntionPageIndex = numbers[0]; // 更新全局动作页索引
                    }
                    else if (style == "Common") // 如果样式为公共
                    {
                        if (numbers[0] > TotalCommonActionPageIndex) TotalCommonActionPageIndex = numbers[0]; // 更新公共动作页索引
                    }
                }
            }
        }

        // 加载全局画布
        private void LoadGlobalCanvas()
        {
            for (int i = 0; i <= TotalGlobalAntionPageIndex; i++)
            {
                GenerateCanvas(i, "Global"); // 生成画布
            }
        }

        /// <summary>
        /// 生成画布
        /// </summary>
        /// <param name="canvasIndex"></param>
        /// <param name="style"></param>
        private void GenerateCanvas(int canvasIndex, string style)
        {
            string canvasName = $"{style}{canvasIndex}"; // 画布名称
            Canvas dynamicCanvas = new Canvas // 创建画布
            {
                Width = 260, // 画布宽度
                Name = canvasName, // 画布名称
                Margin = new Thickness(3, 0, 0, 0), // 画布边距
                Height = style == "Global" ? 215 : 280, // 画布高度
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Left // 水平对齐方式
            }; // 创建画布

            Grid grid = new Grid
            {
                Height = 20, // 网格高度
                Width = 260, // 网格宽度
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Left, // 水平对齐方式
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")) // 背景颜色
            }; // 创建网格

            dynamicCanvas.Children.Add(grid); // 将网格添加到画布

            double buttonSpacing = 65; // 按钮间距
            int rows = style == "Global" ? 3 : 4; // 行数
            int cols = 4; // 列数
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int buttonIndex = row * 4 + col + 1; // 按钮索引
                    string buttonName = $"{style}{canvasIndex}{row + 1}{col + 1}"; // 按钮名称

                    Button button = new Button // 创建按钮
                    {
                        Name = buttonName, // 按钮名称
                        Style = FindResource("ActionButton") as Style, // 按钮样式
                        Margin = new Thickness(col * buttonSpacing, row * buttonSpacing + grid.Height, 0, 0) // 按钮边距
                    }; // 创建按钮

                    BindButtonEvents(button); // 绑定按钮事件

                    dynamicCanvas.Children.Add(button); // 将按钮添加到画布

                    if (buttonDataDict != null && buttonDataDict.TryGetValue(buttonName, out ButtonData data))
                    {
                        RefreshButtonDisplay(button, data); // 刷新按钮显示
                        button.Tag = data; // 设置按钮标签
                    }
                }
            }

            GlobalStackPanel.Children.Add(dynamicCanvas); // 将画布添加到全局堆栈面板
        }

        /// <summary>
        /// 刷新按钮显示
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonInformation"></param>
        private void RefreshButtonDisplay(Button button, ButtonData buttonInformation)
        {
            if (buttonInformation != null)
            {
                Grid grid = new(); // 创建网格
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
                if (buttonInformation.ImagePath != "none") // 如果图标路径不为none
                {
                    try
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                        Image image = new() // 创建图像
                        {
                            Width = 30, // 图像宽度
                            Height = 30, // 图像高度
                            VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                            HorizontalAlignment = HorizontalAlignment.Center, // 水平对齐方式
                            Source = new BitmapImage(new Uri(buttonInformation.ImagePath)) // 设置图像源
                        };
                        grid.Children.Add(image); // 将图像添加到网格
                        Grid.SetRow(image, 0); // 设置图像所在行
                    }
                    catch
                    {
                        new ToastContentBuilder().AddText($"图标加载失败：按钮{buttonInformation.ButtonName}的图标被移动或删除").Show(); // 显示通知
                    }
                }

                if (!string.IsNullOrEmpty(buttonInformation.ButtonName)) // 如果按钮名称不为空
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                    TextBlock textBlock = new() // 创建文本块
                    {
                        TextWrapping = TextWrapping.NoWrap, // 文本换行方式
                        Text = buttonInformation.ButtonName, // 设置文本
                        VerticalAlignment = System.Windows.VerticalAlignment.Center, // 垂直对齐方式
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center // 水平对齐方式
                    }; // 创建文本块
                    buttonManager.AutoEllipsisTextBlock(textBlock, 60); // 设置文本块自动省略
                    grid.Children.Add(textBlock); // 将文本块添加到网格
                    Grid.SetRow(textBlock, 1); // 设置文本块所在行
                }

                button.Content = grid; // 设置按钮内容

                if (!shouldHideTooltip)
                {
                    string toolTipText = null; // 工具提示文本
                    if (!string.IsNullOrWhiteSpace(buttonInformation.ButtonName) || !string.IsNullOrWhiteSpace(buttonInformation.Usage))
                    {
                        string name = !string.IsNullOrWhiteSpace(buttonInformation.ButtonName) ? buttonInformation.ButtonName : null; // 按钮名称
                        string usage = !string.IsNullOrWhiteSpace(buttonInformation.Usage) ? buttonInformation.Usage : null; // 使用说明
                        toolTipText = (name + "\n" + usage).Trim('\n'); // 设置工具提示文本
                    }
                    button.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置工具提示
                }
            }
            else
            {
                button.Tag = null; // 设置按钮标签为空
                button.Content = null; // 设置按钮内容为空
                button.ToolTip = null; // 设置工具提示为空
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置按钮背景颜色
            }
        }

        // 滚动条值改变事件
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToHorizontalOffset(ScrollBar.Value); // 滚动到指定位置
        }

        // 滚动条鼠标左键按下事件
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ButtonData) button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD")); // 设置按钮背景颜色
                else button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 设置按钮背景颜色
            }
        }

        // 滚动条鼠标左键抬起事件
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ButtonData) button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
                else button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置按钮背景颜色
            }
        }

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        /// <param name="button"></param>
        private void BindButtonEvents(Button button)
        {
            button.AllowDrop = true; // 允许拖放
            button.Drop += Button_Drop; // 拖放事件
            button.Click += ShowCreatActionMenu; // 点击事件
            button.DragEnter += Button_DragEnter; // 拖入事件
            button.MouseEnter += Button_MouseEnter; // 鼠标进入事件
            button.MouseLeave += Button_MouseLeave; // 鼠标离开事件
            button.MouseDoubleClick += ShowEditWindow; // 双击事件
            button.PreviewMouseMove += Button_PreviewMouseMove; // 鼠标移动事件
            button.MouseRightButtonDown += OpenCreatActionMenu; // 右键点击事件
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件
        }

        // 拖放事件
        private void Button_Drop(object sender, DragEventArgs e)
        {
            if (sender is Button TargetButton)
            {
                if (TargetButton == SourceButton || SourceButton == null) return; // 如果目标按钮和源按钮相同，直接返回
                if (e.Data.GetDataPresent(typeof(ButtonData)))
                {
                    db2.ExchangeButtonID(SourceButton.Name, TargetButton.Name); // 交换按钮ID

                    var TargetData = db2.GetButtonDataByID(SourceButton.Name); // 获取目标按钮数据
                    RefreshButtonDisplay(SourceButton, TargetData); // 刷新按钮显示
                    SourceButton.Tag = TargetData; // 设置源按钮标签

                    var SourceData = db2.GetButtonDataByID(TargetButton.Name); // 获取源按钮数据
                    RefreshButtonDisplay(TargetButton, SourceData); // 刷新按钮显示
                    TargetButton.Tag = SourceData; // 设置目标按钮标签

                    buttonDataDict[SourceButton.Name] = TargetData; // 更新按钮数据字典
                    buttonDataDict[TargetButton.Name] = SourceData; // 更新按钮数据字典
                }
            }
        }

        // 拖入事件
        public void Button_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move; // 设置拖拽效果为移动
            e.Handled = true; // 标记事件已处理
        }

        // 鼠标左键按下事件
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                initialMousePosition = e.GetPosition(this); // 获取初始鼠标位置
                SourceButton = button; // 设置源按钮
                isDragging = false; // 设置拖拽状态为false
            }
        }

        // 鼠标移动事件
        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed) // 如果鼠标左键按下
            {
                Point currentPosition = e.GetPosition(this); // 获取当前鼠标位置
                double deltaX = currentPosition.X - initialMousePosition.X; // 计算X轴偏移量
                double deltaY = currentPosition.Y - initialMousePosition.Y; // 计算Y轴偏移量
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY); // 计算距离
                if (distance > 10 && !isDragging) // 如果距离大于10且未拖拽
                {
                    isDragging = true; // 设置拖拽状态为true
                    if (button.Tag is ButtonData data)
                    {
                        DragDrop.DoDragDrop(button, data, DragDropEffects.Move); // 执行拖放操作
                    }
                }
            }
        }

        // 鼠标左键抬起事件
        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // 设置拖拽状态为false
        }

        // 显示创建动作菜单
        private void ShowCreatActionMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag == null)
            {
                Point mousePosition = Mouse.GetPosition(this); // 获取鼠标位置
                double left = mousePosition.X + 310.4, top = mousePosition.Y + 596 / 3; // 计算菜单位置
                CreatActionMenu creatActionMenu = Application.Current.Windows.OfType<CreatActionMenu>().FirstOrDefault(); // 查找创建动作菜单
                creatActionMenu?.Close(); // 关闭已有菜单
                creatActionMenu = new(button.Name)
                {
                    Left = left,
                    Top = top
                }; // 创建新的菜单
                creatActionMenu.Show(); // 显示菜单
            }
        }

        // 显示编辑窗口
        private void ShowEditWindow(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                AddWindow addWindow = new AddWindow(button.Name, 0); // 创建编辑窗口
                addWindow.Show(); // 显示编辑窗口
                addWindow.Activate(); // 激活编辑窗口
            }
        }

        // 添加动作页
        private void AddActionPage(object sender, RoutedEventArgs e)
        {
            int canvasIndex = GlobalStackPanel.Children.Count; // 获取画布索引
            if (canvasIndex > 9) return; // 如果索引大于9，则返回
            GenerateCanvas(canvasIndex, "Global"); // 生成画布
        }

        // 滚动条滚动事件
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollBar.Maximum = ScrollViewer.ExtentWidth - ScrollViewer.ViewportWidth; // 设置滚动条最大值
            ScrollBar.ViewportSize = ScrollViewer.ViewportWidth; // 设置滚动条视口大小
            ScrollBar.Value = ScrollViewer.HorizontalOffset; // 设置滚动条值
        }

        // 全局按钮点击事件
        private void GlobalButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalStackPanel.Children.Clear(); // 清空全局堆栈面板
            LoadGlobalCanvas(); // 加载全局画布
            MainBorder.Margin = new Thickness(239, 31, 11, 564); // 设置主边框边距
            ScrollBar.Margin = new Thickness(240, 241.8, 10, 0); // 设置滚动条边距
            AddActionPageButton.Margin = new Thickness(239, 264, 0, 0); // 设置添加动作页按钮边距
        }

        // 公共按钮点击事件
        private void CommonButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalStackPanel.Children.Clear(); // 清空全局堆栈面板
            LoadCommonCanvas(); // 加载公共画布
            MainBorder.Margin = new Thickness(239, 31, 11, 499); // 设置主边框边距
            ScrollBar.Margin = new Thickness(240, 307.15, 10, 0); // 设置滚动条边距
            AddActionPageButton.Margin = new Thickness(239, 330.15, 0, 0); // 设置添加动作页按钮边距
        }

        // 加载公共画布
        private void LoadCommonCanvas()
        {
            for (int i = 0; i <= TotalCommonActionPageIndex; i++)
            {
                GenerateCanvas(i, "Common"); // 生成画布
            }
        }

        // 打开创建动作菜单
        private void OpenCreatActionMenu(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                buttonManager.OpenCreatActionMenu(sender, e, false); // 打开创建动作菜单
            }
        }
    }
}