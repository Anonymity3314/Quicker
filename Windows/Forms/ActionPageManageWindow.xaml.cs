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
            if (obj == null) yield break; // 空对象返回空序列
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i); // 获取子元素
                if (child is T tChild) yield return tChild; // 找到类型为 T 的子元素
                foreach (var grandChild in FindVisualChildren<T>(child)) yield return grandChild; // 递归查找子元素
            }
        } // 查找子元素
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                {
                    return parent; // 找到父级控件
                }
                child = VisualTreeHelper.GetParent(child); // 获取父级控件
            }
            return null;
        } // 查找父级控件

        private Dictionary<string, ButtonData> buttonDataDict; // 按钮数据字典
        private readonly ButtonManager buttonManager; // 按钮管理器
        private readonly SettingDatabase db1; // 设置数据库
        private readonly ButtonDatabase db2; // 按钮数据库
        private Point initialMousePosition; // 初始鼠标位置
        private bool shouldHideTooltip; // 是否隐藏提示

        public ActionPageManageWindow()
        {
            InitializeComponent(); // 初始化窗口

            db1 = new SettingDatabase(); // 初始化设置数据库
            db1.Initialize(); // 初始化设置数据库

            db2 = new ButtonDatabase(); // 初始化按钮数据库
            db2.Initialize(); // 初始化按钮数据库

            buttonManager = new ButtonManager(); // 初始化按钮管理器
        }

        // 窗口加载事件
        private async void ActionPageManageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var buttonDataList = db2.GetAllButtonData(); // 获取所有按钮数据
            buttonDataDict = buttonDataList.ToDictionary(data => data.ButtonID); // 将按钮数据转换为字典

            LoadCanvas("Global"); // 加载全局画布

            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取约定
            shouldHideTooltip = Convention.HideTooltip; // 是否隐藏工具提示
        }

        // 加载动作页按钮
        private void LoadActionPageButtons()
        {

        }

        /// <summary>
        /// 加载动作页
        /// </summary>
        /// <param name="style">动作页样式</param>
        private void LoadCanvas(string style)
        {
            MainListView.Items.Clear(); // 清空总列表视图
            int TotalAntionPageIndex = GetTotalAntionPageIndex(style); // 获取总动作页索引
            for (int i = 0; i <= TotalAntionPageIndex; i++)
            {
                GenerateCanvas(i, style); // 生成动作页
            }

            switch (style)
            {
                case "Global":
                    MainBorder.Height = 224; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 272, 0, 0); // 设置添加动作页按钮边距
                    break; // 全局动作页
                case "Common":
                    MainBorder.Height = 289; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加动作页按钮边距
                    break; // 普通动作页
                default:
                    bool haveCommonStyleButton = db2.GetAllButtonData().Any(data => data.ButtonID.StartsWith($"{style}")); // 是否有 style 样式的按钮
                    if (!haveCommonStyleButton) // 如果没有 style 样式的按钮
                    {
                        var canvasCollection = FindVisualChildren<Canvas>(MainListView); // 获取动作页集合
                        foreach (Canvas canvas in canvasCollection) // 遍历Canvas集合
                        {
                            canvas.Visibility = Visibility.Hidden; // 隐藏动作页
                        }
                        MainBorder.Height = 224; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 272, 0, 0); // 设置添加动作页按钮边距
                    }
                    else
                    {
                        MainBorder.Height = 289; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加动作页按钮边距
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取总动作页索引
        /// </summary>
        /// <param name="style">动作页样式</param>
        /// <returns>总动作页索引</returns>
        private int GetTotalAntionPageIndex(string style)
        {
            int actionPageIndex = 0; // 动作页索引
            if (buttonDataDict == null || buttonDataDict.Count == 0) return 0; // 如果按钮数据字典为空，则返回
            foreach (var data in buttonDataDict.Values)
            {
                if (data.ButtonID.StartsWith(style))
                {
                    Match match = Regex.Match(data.ButtonID, @"^([a-zA-Z0-9_]+)(\d{3})$"); // 正则表达式匹配
                    string numbersStr = match.Groups[2].Value; // 数字字符串
                    int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray(); // 将数字字符串转换为整数数组
                    if (numbers[0] > actionPageIndex) actionPageIndex = numbers[0]; // 更新全局动作页索引
                }
            }
            return actionPageIndex;
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
                AllowDrop = true,
                Name = canvasName, // 画布名称
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

            Button pageButton = new Button
            {
                Width = 17.24,
                Tag = $"{style}{canvasIndex}",
                Name = $"{style}{canvasIndex}",
                Margin = new Thickness(3, 0, 0, 0),
                BorderThickness = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Left // 水平对齐方式
            };

            pageButton.PreviewMouseLeftButtonDown += buttonManager.Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            pageButton.PreviewMouseMove += Button1_PreviewMouseMove; // 鼠标移动事件
            pageButton.PreviewMouseLeftButtonUp += buttonManager.Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件

            Image image = new Image
            {
                Source = new BitmapImage(new Uri("/Resources/Images/Icons/Quicker1.ico", UriKind.Relative))
            };

            pageButton.Content = image;
            grid.Children.Add(pageButton);

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
                        buttonManager.RefreshButtonDisplay(button, data, 60, false); // 刷新按钮显示
                    }
                }
            }
            MainListView.Items.Add(dynamicCanvas); // 将画布添加到全局列表视图
        }

        // 滚动条值改变事件
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToHorizontalOffset(ScrollBar.Value); // 滚动到指定位置
        }

        // 滚动条鼠标左键按下事件
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data)
            {
                if (data.Location != null) button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD")); // 设置按钮背景颜色
                else button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 设置按钮背景颜色
            }
        }

        // 滚动条鼠标左键抬起事件
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data)
            {
                if (data.Location != null) button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
                else button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置按钮背景颜色
            }
        }

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        /// <param name="button">按钮</param>
        private void BindButtonEvents(Button button)
        {
            button.AllowDrop = true; // 允许拖放
            button.Drop += Button_Drop; // 拖放事件
            button.Click += ShowCreatActionMenu; // 点击事件
            button.MouseEnter += Button_MouseEnter; // 鼠标进入事件
            button.MouseLeave += Button_MouseLeave; // 鼠标离开事件
            button.MouseDoubleClick += ShowEditWindow; // 双击事件
            button.PreviewMouseMove += Button_PreviewMouseMove; // 鼠标移动事件
            button.MouseRightButtonDown += OpenMenu; // 右键点击事件
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件
        }

        // 拖放事件
        private void Button_Drop(object sender, DragEventArgs e)
        {
            if (sender is Button TargetButton)
            {
                buttonManager.Button_Drop(sender, e, false); // 处理拖放事件
            }
        }

        // 鼠标左键按下事件
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                buttonManager.Button_PreviewMouseLeftButtonDown(sender, e); // 处理鼠标左键按下事件
            }
        }

        // 鼠标移动事件
        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed) // 如果鼠标左键按下
            {
                buttonManager.Button_PreviewMouseMove(sender, e, true); // 处理鼠标移动事件
            }
        }

        // 鼠标左键抬起事件
        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            buttonManager.Button_PreviewMouseLeftButtonUp(sender, e); // 处理鼠标左键抬起事件
        }

        // 左键空白 Button 显示创建动作菜单
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
            int canvasIndex = MainListView.Items.Count; // 获取画布索引
            if (canvasIndex > 9) return; // 如果索引大于9，则返回

            string canvasName = null; // 画布名称
            foreach (var item in MainListView.Items)
            {
                if (item is Canvas canvas)
                {
                    canvasName = canvas.Name; // 获取 Canvas 的名称
                    break;
                }
            }
            Match matchCanvas = Regex.Match(canvasName, @"^([a-zA-Z0-9_]+)(\d{1})$"); // 正则匹配源 Button Name
            string style = matchCanvas.Groups[1].Value; // 动作页样式
            GenerateCanvas(canvasIndex, style); // 生成画布
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
            MainListView.Items.Clear(); // 清空全局列表视图
            LoadCanvas("Global"); // 加载全局画布
            MainBorder.Height = 224; // 设置主边框高度
            ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
            AddActionPageButton.Margin = new Thickness(239, 272, 0, 0); // 设置添加动作页按钮边距
        }

        // 公共按钮点击事件
        private void CommonButton_Click(object sender, RoutedEventArgs e)
        {
            MainListView.Items.Clear(); // 清空全局列表视图
            LoadCommonCanvas(); // 加载公共画布
            MainBorder.Height = 289; // 设置主边框高度
            ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
            AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加动作页按钮边距
        }

        // 加载CommonCanvas
        private void LoadCommonCanvas()
        {
            LoadCanvas("Common"); // 加载 Common 画布
        }

        // 加载任务栏动作页
        private void TaskBarButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCanvas("TaskBar"); // 加载任务栏动作页
        }

        // 加载桌面动作页
        private void DesktopButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCanvas("Desktop"); // 加载桌面动作页
        }

        // 打开创建动作菜单
        private void OpenMenu(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data && button != null) buttonManager.OpenMenu(sender, false, "OperationMenu", this); // 打开操作菜单
            else buttonManager.OpenMenu(sender, false, "CreatActionMenu", this); // 打开创建动作菜单
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void Button1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
            {
                buttonManager.Button_PreviewMouseMove(sender, e, false); // 处理鼠标移动事件
            }
        }

        // 拖拽经过目标项
        private void ListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            buttonManager.Button_PreviewDragOver(sender, e); // 处理拖放事件
        }

        // 拖拽完成
        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ButtonData"))
            {
                string sourceButtonName = e.Data.GetData("ButtonData")?.ToString(); // 获取传递的 Button Name
                if (!string.IsNullOrEmpty(sourceButtonName))
                {
                    Point point = e.GetPosition(MainListView); // 获取鼠标位置
                    var hitTestResult = VisualTreeHelper.HitTest(MainListView, point); // 获取鼠标位置的项
                    DependencyObject targetItem = hitTestResult.VisualHit; // 获取鼠标位置的项
                    Canvas targetCanvas = FindParent<Canvas>(targetItem);// 查找目标 Canvas

                    if (targetCanvas == null) return; // 如果目标 Canvas 为 null，则返回
                    string targetCanvasName = targetCanvas.Name; // 获取目标 Canvas 的 Name
                    if (sourceButtonName == targetCanvasName) return; // 如果目标 Canvas 与源 Canvas 相同，则返回

                    int sourceIndex = int.Parse(sourceButtonName.Substring(sourceButtonName.Length - 1)); // 获取源 Button 索引
                    int targetIndex = int.Parse(targetCanvasName.Substring(targetCanvasName.Length - 1)); // 获取目标 Button 索引
                    Match matchButton = Regex.Match(sourceButtonName, @"^([a-zA-Z0-9_]+)(\d{1})$"); // 正则匹配源 Button Name
                    string style = matchButton.Groups[1].Value; // 动作页样式

                    db2.SwapButtonAValues(style, sourceIndex, targetIndex); // 更新数据库 Button A 值
                    var buttonDataList = db2.GetAllButtonData(); // 获取所有按钮数据
                    buttonDataDict = buttonDataList.ToDictionary(data => data.ButtonID); // 转换为字典
                    LoadCanvas(style); // 刷新界面
                }
            }
        }

        // 查找动作页
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower(); // 获取用户输入的文本并转换为小写
            if (string.IsNullOrEmpty(searchText))
            {
                ActionPagesButtonPanel.Children.Clear(); // 清空动作页按钮面板
            }
        }

        // 关闭窗口释放图标资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 关闭窗口释放图标资源
            GC.Collect(); // 释放图标资源
        }
    }
}