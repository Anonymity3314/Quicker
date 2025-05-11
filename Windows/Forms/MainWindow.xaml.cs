using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using System.Windows.Media;
using System.Windows.Input;
using IWshRuntimeLibrary;
using System.Diagnostics;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;
using System.IO;
using Quicker;

namespace Quicker.Windows
{
    public partial class MainWindow : Window
    {
        private const string BookIconPath = "/Resources/Images/Icons/Book.ico"; // 订住图标路径
        private const string DisBookIconPath = "/Resources/Images/Icons/Disbook.ico"; // 禁用订住图标路径
        private const string LockIconPath = "/Resources/Images/Icons/Locked.ico"; // 锁定图标路径
        private const string UnLockIconPath = "/Resources/Images/Icons/UnLocked.ico"; // 解锁图标路径

        private const string SelectedPageButtonColor = "#FF8D8D8D"; // 选中页面按钮颜色
        private const string UnSelectedPageButtonColor = "#FFD3D3D3"; // 未选中页面按钮颜色

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
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); // 取消后台任务的令牌源
        private readonly ButtonManager buttonManager = new ButtonManager(); // 按钮管理器
        private readonly WindowManager windowManager = new WindowManager(); // 窗口管理器
        private readonly ActionManager actionManager = new ActionManager(); // 动作管理器
        private readonly IconManager iconManager = new IconManager(); // 图标管理器
        private readonly SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
        private string CommonStyle; // 样式
        private readonly App app; // App实例

        public MainWindow(string Style)
        {
            CommonStyle = Style; // 设置样式
            InitializeComponent(); // 初始化窗口组件
            GlobalGrid.Children.Remove(ViewGlobalCanvas); // 从主网格中移除
            CommonGrid.Children.Remove(ViewCommonCanvas); // 从主网格中移除
            app = (App.Current as App); // 获取当前 App 实例
        }

        // 加载数据库和Button
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenerateCanvas(0, "Global"); // 生成全局 Canvas
                if (db2.TableExists(CommonStyle))
                {
                    var commonButtonData = db2.GetButtonDataByPrefix(CommonStyle); // 从数据库中获取通用样式按钮数据
                    bool hasCommonButtons = commonButtonData.Any(data => data.ButtonID.StartsWith(CommonStyle)); // 判断是否有通用样式按钮数据
                    if (hasCommonButtons) GenerateCanvas(0, CommonStyle); // 如果有按钮，生成通用 Canvas
                }
                else if (CommonStyle == "Taskbar" || CommonStyle == "Desktop")
                {
                    CommonStyle = "Common"; // 设置样式为通用样式
                    GenerateCanvas(0, CommonStyle); // 生成通用 Canvas
                }
                GenerateButtons(); // 生成按钮
            }); // 在主线程中执行

            // 加载BookButton图标
            string iconPath = app._appStateManager.Book ? BookIconPath : DisBookIconPath; // 获取图标路径
            BitmapImage bookImage = new BitmapImage(new Uri(iconPath, UriKind.Relative)); // 创建图标对象
            Book.Source = bookImage; // 设置Book按钮的图标

            // 加载LockButton图标
            string lockIconPath = app._appStateManager.Locked ? LockIconPath : UnLockIconPath; // 获取图标路径
            BitmapImage lockImage = new BitmapImage(new Uri(lockIconPath, UriKind.Relative)); // 创建图标对象
            Lock.Source = lockImage; // 设置Lock按钮的图标

            windowManager.SetWindowTopmost(this);// 设置窗口置顶
            SetCommonLabel(); // 设置通用标签
        }

        // 获取总的页面数
        private int GetTotalAntionPageIndex(string targetStyle)
        {
            int TotalAntionPageIndex = 0; // 重置页面索引
            if (!db2.TableExists(targetStyle)) return TotalAntionPageIndex; // 如果不存在该样式的按钮数据表，返回
            var buttonData = db2.GetButtonDataByPrefix(targetStyle); // 从数据库中获取按钮数据
            foreach (var data in buttonData)
            {
                string buttonID = data.ButtonID; // 获取按钮ID
                Match match = Regex.Match(data.ButtonID, @"^([a-zA-Z0-9_]+)(\d{3})$"); // 匹配按钮名称和末尾的3个数字
                if (match.Success)
                {
                    string style = match.Groups[1].Value; // 获取按钮名称
                    string numbersStr = match.Groups[2].Value; // 获取3个数字
                    int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray(); // 转换为整数数组
                    if (style == targetStyle) // 如果是全局按钮
                    {
                        if (numbers[0] > TotalAntionPageIndex) TotalAntionPageIndex = numbers[0]; // 更新全局页面索引
                    }
                }
            }
            return TotalAntionPageIndex;
        }

        // 生成页面切换 Button
        private void GenerateButtons()
        {
            GeneratePageButtons("Global", GetTotalAntionPageIndex("Global"), SwitchToGlobalCanvas, GlobalActionPageChangeButton_MouseEnter, GlobalActionPageChangeButton_MouseLeave, GlobalButtonPanel); // 生成全局页面切换按钮
            GeneratePageButtons(CommonStyle, GetTotalAntionPageIndex(CommonStyle), SwitchToCommonCanvas, CommonActionPageChangeButton_MouseEnter, CommonActionPageChangeButton_MouseLeave, CommonButtonPanel); // 生成通用页面切换按钮
        }

        /// <summary>
        /// 生成页面切换按钮
        /// </summary>
        /// <param name="prefix">按钮名称前缀</param>
        /// <param name="totalPages">总页面数</param>
        /// <param name="clickHandler">点击事件处理程序</param>
        /// <param name="mouseEnterHandler">鼠标进入事件处理程序</param>
        /// <param name="mouseLeaveHandler">鼠标离开事件处理程序</param>
        /// <param name="panel">按钮所属的面板</param>
        private void GeneratePageButtons(string prefix, int totalPages, RoutedEventHandler clickHandler, MouseEventHandler mouseEnterHandler, MouseEventHandler mouseLeaveHandler, Panel panel)
        {
            if (totalPages <= 0) return; // 如果没有页面，直接返回
            for (int i = 0; i <= totalPages; i++)
            {
                Button button = new Button
                {
                    Name = $"{prefix}{i}", // 设置按钮名称
                    Margin = new Thickness(2.5, 0, 2.5, 0), // 设置按钮边距
                    Style = FindResource("ActionPageChangeButton") as Style // 设置按钮样式
                };
                if (i == 0) button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF8D8D8D")); // 设置当前按钮颜色

                // 添加事件处理程序
                button.Click += clickHandler;
                button.MouseEnter += mouseEnterHandler;
                button.MouseLeave += mouseLeaveHandler;

                panel.Children.Add(button); // 添加到面板
            }
        }

        // 切换到全局Canvas
        private void SwitchToGlobalCanvas(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(sender, e, MainGrid, "Global"); // 切换到全局Canvas
        }

        // 切换到通用Canvas
        private void SwitchToCommonCanvas(object sender, RoutedEventArgs e)
        {
            SwitchToCanvas(sender, e, CommonGrid, CommonStyle); // 切换到通用Canvas
        }

        /// <summary>
        /// 切换到指定的Canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="targetGrid"></param>
        /// <param name="Style"></param>
        private void SwitchToCanvas(object sender, RoutedEventArgs e, Grid targetGrid, string Style)
        {
            if (sender is Button clickedButton)
            {
                int canvasIndex = int.Parse(clickedButton.Name.Replace($"{Style}", "")); // 获取Canvas索引
                string targetCanvasName = $"{Style}{canvasIndex}"; // 生成目标Canvas名称
                Canvas targetCanvas = FindVisualChildren<Canvas>(targetGrid).FirstOrDefault(c => c.Name == targetCanvasName); // 查找目标Canvas

                // 如果目标Canvas不存在，动态生成
                if (targetCanvas == null)
                {
                    GenerateCanvas(canvasIndex, Style); // 动态生成Canvas
                    targetCanvas = FindVisualChildren<Canvas>(targetGrid).FirstOrDefault(c => c.Name == targetCanvasName); // 查找目标Canvas
                }
                targetCanvas.Visibility = Visibility.Visible; // 设置目标Canvas可见
                foreach (Canvas canvas in FindVisualChildren<Canvas>(targetGrid)) // 隐藏其他Canvas
                {
                    if (canvas.Name.StartsWith($"{Style}") && canvas != targetCanvas)
                        canvas.Visibility = Visibility.Collapsed; // 隐藏其他Canvas
                }
            }
        }

        // 设置标签内容
        private void SetCommonLabel()
        {
            switch (CommonStyle)
            {
                case "Common":
                    break; // 如果是通用类型，不设置标签内容
                case "Taskbar":
                    CommonLabel.Content = "任务栏"; // 设置标签内容
                    break; // 如果是任务栏类型，设置标签内容为任务栏
                case "Desktop":
                    CommonLabel.Content = "桌面"; // 设置标签内容
                    break; // 如果是桌面类型，设置标签内容为桌面
                default:
                    CommonLabel.Content = $"{CommonStyle}"; // 设置标签内容
                    break; // 如果是其他类型，设置对应标签内容
            }
        }

        // 移动功能面板
        private void MoveMainWindow(object sender, EventArgs e)
        {
            DragMove(); // 触发窗口拖动
        }

        // 订住功能面板
        private void BookQuicker(object sender, EventArgs e)
        {
            app._appStateManager.Book = !app._appStateManager.Book; // 更新数据库中的设置
            BitmapImage bookimage = new(); // 创建图像对象
            bookimage.BeginInit(); // 开始初始化
            if (app._appStateManager.Book)
                bookimage.UriSource = new Uri("/Resources/Images/Icons/Book.ico", UriKind.Relative); // 设置为订住样式
            else
                bookimage.UriSource = new Uri("/Resources/Images/Icons/Disbook.ico", UriKind.Relative); // 设置为不订住样式
            bookimage.EndInit(); // 结束初始化
            Book.Source = bookimage; // 更新Book按钮图标
        }

        // 打开设置窗口
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            windowManager.OpenTargetWindow("SettingWindow"); // 打开设置窗口
        }

        // 关闭功能面板
        private void CloseMainWindow(object sender, EventArgs e)
        {
            if (!buttonManager.isClosing)
            {
                buttonManager.isClosing = true; // 设置关闭标志
                this.Close(); // 关闭窗口
            }
        }

        // 失去焦点时关闭功能面板
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            if (!app._appStateManager.Pause && !buttonManager.isClosing && !app._appStateManager.Book)
            {
                buttonManager.isClosing = true; // 设置关闭标志
                this.Close(); // 关闭窗口
            }
        }

        // 鼠标移入Button改变外观
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            if (button.Tag is ButtonData data && data.Location != null)
            {
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#BEE6FD")); // 改变背景颜色
                button.RenderTransform = new ScaleTransform(1.05, 1.05); // 改变按钮大小
                Canvas.SetZIndex(button, 1); // 改变按钮层级
            }
            else
            {
                var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取配置信息
                if (Convention.ShowAddImage)
                {
                    System.Windows.Controls.Image image = new()
                    {
                        Source = new BitmapImage(new Uri("/Resources/Images/Icons/Add.ico", UriKind.Relative)), // 设置图像源
                        Width = 36, // 宽为36
                        Height = 36, // 高为36
                        VerticalAlignment = VerticalAlignment.Center, // 垂直居中
                        HorizontalAlignment = HorizontalAlignment.Center // 水平居中
                    };
                    button.Content = image; // 设置按钮内容
                }
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEAEAEA")); // 改变背景颜色
            } // 如果Button的目标地址不存在
        }
        private void GlobalActionPageChangeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseEnter(sender, e, "Global", "#FFB9B9B9"); // 改变按钮颜色
        }
        private void CommonActionPageChangeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseEnter(sender, e, CommonStyle, "#FFB9B9B9"); // 改变按钮颜色
        }
        /// <summary>
        /// 鼠标移入Button改变外观
        /// </summary>
        /// <param name="sender">按钮</param>
        /// <param name="e">事件参数</param>
        /// <param name="prefix">按钮名称前缀</param>
        /// <param name="color">按钮颜色</param>
        private void PageChangeButton_MouseEnter(object sender, MouseEventArgs e, string prefix, string color)
        {
            if (sender is Button button)
            {
                int canvasIndex = int.Parse(button.Name.Replace($"{prefix}", "")); // 获取Canvas索引
                string targetCanvasName = $"{prefix}{canvasIndex}"; // 生成目标Canvas名称
                Canvas targetCanvas = null; // 初始化目标Canvas

                var grid = prefix == "Global" ? MainGrid : CommonGrid; // 根据前缀选择不同的Grid
                foreach (Canvas canvas in FindVisualChildren<Canvas>(grid)) // 查找目标Canvas
                {
                    if (canvas.Name == targetCanvasName)
                    {
                        targetCanvas = canvas; // 找到目标Canvas
                        break;
                    }
                }

                if (targetCanvas == null)
                {
                    button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
                    return;
                }

                if (targetCanvas.Visibility != Visibility.Visible)
                    button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
            }
        }

        // 鼠标移出Button还原外观
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            if (button.Tag is ButtonData data && data.Location != null)
            {
                Canvas.SetZIndex(button, 0); // 还原按钮层级
                button.RenderTransform = new ScaleTransform(1, 1); // 还原按钮大小
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("White")); // 还原背景颜色
            }
            else
            {
                button.Content = null; // 清空按钮内容
                button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F3F3")); // 还原背景颜色
            }
        }
        private void GlobalActionPageChangeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseLeave(sender, e, "Global", "#FFD3D3D3"); // 还原按钮颜色
        }
        private void CommonActionPageChangeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseLeave(sender, e, CommonStyle, "#FFD3D3D3"); // 还原按钮颜色
        }
        /// <summary>
        /// 鼠标移出Button还原外观
        /// </summary>
        /// <param name="sender">按钮</param>
        /// <param name="e">事件参数</param>
        /// <param name="prefix">按钮名称前缀</param>
        /// <param name="color">按钮颜色</param>
        private void PageChangeButton_MouseLeave(object sender, MouseEventArgs e, string prefix, string color)
        {
            if (sender is Button button)
            {
                int canvasIndex = int.Parse(button.Name.Replace($"{prefix}", "")); // 获取Canvas索引
                string targetCanvasName = $"{prefix}{canvasIndex}"; // 生成目标Canvas名称
                Canvas targetCanvas = null; // 初始化目标Canvas

                var grid = prefix == "Global" ? MainGrid : CommonGrid; // 根据前缀选择不同的Grid
                foreach (Canvas canvas in FindVisualChildren<Canvas>(grid)) // 查找目标Canvas
                {
                    if (canvas.Name == targetCanvasName)
                    {
                        targetCanvas = canvas; // 找到目标Canvas
                        break;
                    }
                }

                if (targetCanvas == null)
                {
                    button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
                    return;
                }

                if (targetCanvas.Visibility != Visibility.Visible)
                    button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
            }
        }

        // 左键点击按钮时执行动作
        private void DoAction(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Tag is ButtonData data)
            {
                switch(data.ActionType)
                {
                    case "OpenFile":
                        actionManager.OpenFile(data); // 调用 ActionManager 的 OpenFile 方法打开文件
                        break; // 打开文件、文件夹
                    case "OpenWebsite":
                        actionManager.OpenWebsite(data); // 调用 ActionManager 的 OpenFolder 方法打开文件夹
                        break; // 打开网站
                    case "OpenFiles":
                        actionManager.OpenFiles(data); // 调用 ActionManager 的 OpenFiles 方法打开多个文件
                        break; // 打开多个文件
                }
            }
            else
            {
                var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取配置信息
                if (Convention.ShowAddImage) // 如果不显示添加按钮，直接返回
                    buttonManager.OpenMenu(sender, true, "CreatActionMenu", this); // 打开菜单
            }
        }

        // 右键按钮打开菜单
        public void OpenCreatActionMenu(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            buttonManager.OpenMenu(sender, true, button.Tag is ButtonData data ? "OperationMenu" : "CreatActionMenu", this); // 打开操作菜单
        }

        // 添加关闭标志防止报错
        private void MainWindow_Closing(object sender, EventArgs e)
        {
            buttonManager.isClosing = true; // 设置关闭标志
        }

        // 允许拖拽
        private void Button_PreviewDragOver(object sender, DragEventArgs e)
        {
            buttonManager.Button_PreviewDragOver(sender, e); // 允许拖拽
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
                buttonManager.Button_Drop(sender, e, true); // 处理拖拽事件
            }
        }

        // 鼠标左键按下时记录初始位置
        public void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                buttonManager.Button_PreviewMouseLeftButtonDown(sender, e); // 记录初始位置
            }
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
                buttonManager.Button_PreviewMouseMove(sender, e, true); // 检查拖拽条件
        }

        // 鼠标左键释放时重置状态
        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            buttonManager.Button_PreviewMouseLeftButtonUp(sender, e); // 重置状态
        }

        // 打开标题菜单
        private void OpenTitleMenu(object sender, RoutedEventArgs e)
        {
            TitlePop.IsOpen = true; // 打开菜单
        }

        // 退出Quicker
        private void QuitQuicker(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown(); // 退出程序
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
            windowManager.OpenTargetWindow("ActionPageManageWindow"); // 打开动作管理窗口
        }

        // 滚轮进行全局动作页翻页
        private void GolbalGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeVisibleCanvas(e, "Global"); // 滚轮进行全局动作页翻页
        }

        /// <summary>
        /// 获取当前可见的Canvas编号
        /// </summary>
        /// <param name="Style"></param>
        /// <returns></returns>
        private int GetVisibleCanvasIndex(string Style)
        {
            var canvasCollection = Style == "Global" // 根据是否是全局Canvas选择集合
                ? FindVisualChildren<Canvas>(MainGrid) // 查找MainGrid下的Canvas集合
                : FindVisualChildren<Canvas>(CommonGrid); // 查找CommonGrid下的Canvas集合
            string canvasPrefix = $"{Style}"; // Canvas前缀
            string pattern = $@"^{Style}(\d+)$"; // 正则表达式模式
            Regex regex = new Regex(pattern); // 创建正则表达式对象

            foreach (Canvas canvas in canvasCollection) // 遍历Canvas集合
            {
                if (canvas.Visibility == Visibility.Visible)
                {
                    Match match = regex.Match(canvas.Name); // 匹配Canvas名称
                    if (match.Success) return int.Parse(match.Groups[1].Value); // 如果匹配成功，返回Canvas编号
                }
            }
            return 0; // 默认返回0
        }

        /// <summary>
        /// 滑动滚轮更改当前可见 Canvas
        /// </summary>
        /// <param name="e">鼠标滚轮事件参数</param>
        /// <param name="style">Canvas 类型</param>
        private void ChangeVisibleCanvas(MouseWheelEventArgs e, string style)
        {
            e.Handled = true; // 标记事件已处理
            int delta = e.Delta; // 获取鼠标滚轮的增量值
            int currentCanvasIndex = GetVisibleCanvasIndex(style); // 获取当前可见的Canvas编号
            if (delta > 0) SwitchToPreviousCanvas(currentCanvasIndex, style); // 向上滚动，切换到上一页
            else SwitchToNextCanvas(currentCanvasIndex, style); // 向下滚动，切换到下一页
        }

        /// <summary>
        /// 切换到上一页
        /// </summary>
        /// <param name="currentCanvasIndex">当前可见的Canvas编号</param>
        /// <param name="style">Canvas 类型</param>
        private void SwitchToPreviousCanvas(int currentCanvasIndex, string style)
        {
            SwitchCanvas(currentCanvasIndex, style, false); // 向上滚动，切换到上一页
        }

        /// <summary>
        /// 切换到下一页
        /// </summary>
        /// <param name="currentCanvasIndex">当前可见的Canvas编号</param>
        /// <param name="style">Canvas 类型</param>
        private void SwitchToNextCanvas(int currentCanvasIndex, string style)
        {
            SwitchCanvas(currentCanvasIndex, style, true); // 向下滚动，切换到下一页
        }

        /// <summary>
        /// 切换Canvas
        /// </summary>
        /// <param name="currentCanvasIndex"> 当前可见的Canvas编号 </param>
        /// <param name="style"> Canvas 类型 </param>
        /// <param name="isNext"> 是否向下滚动 </param>
        private void SwitchCanvas(int currentCanvasIndex, string style, bool isNext)
        {
            var Convention = db1.GetAllConventions().FirstOrDefault(); // 获取设置数据
            int targetCanvasIndex = isNext ? currentCanvasIndex + 1 : currentCanvasIndex - 1; // 计算目标Canvas编号
            if ((isNext && targetCanvasIndex > GetTotalAntionPageIndex(style)) || (!isNext && targetCanvasIndex < 0)) // 如果目标Canvas编号超出范围
            {
                if (Convention.LoopPageFlipping) // 如果循环翻页
                    targetCanvasIndex = isNext ? 0 : GetTotalAntionPageIndex(style); // 循环到第一页或最后一页
                else return; // 如果不循环翻页，直接返回
            }

            string targetCanvasName = $"{style}{targetCanvasIndex}"; // 生成目标Canvas名称
            Canvas targetCanvas = FindVisualChildren<Canvas>(style == "Global" ? MainGrid : CommonGrid)
                .FirstOrDefault(c => c.Name == targetCanvasName); // 查找目标Canvas

            if (targetCanvas == null) // 如果目标Canvas不存在
            {
                GenerateCanvas(targetCanvasIndex, style); // 动态生成Canvas
                targetCanvas = FindVisualChildren<Canvas>(style == "Global" ? MainGrid : CommonGrid)
                    .FirstOrDefault(c => c.Name == targetCanvasName); // 查找目标Canvas
            }

            targetCanvas.Visibility = Visibility.Visible; // 设置目标Canvas可见
            string currentCanvasName = $"{style}{currentCanvasIndex}"; // 生成当前Canvas名称
            Canvas currentCanvas = FindVisualChildren<Canvas>(style == "Global" ? MainGrid : CommonGrid)
                .FirstOrDefault(c => c.Name == currentCanvasName); // 查找当前Canvas
            currentCanvas.Visibility = Visibility.Collapsed; // 隐藏当前Canvas
        }

        /// <summary>
        /// 生成Canvas
        /// </summary>
        /// <param name="canvasIndex"> 要生成的页面索引 </param>
        /// <param name="style"> Canvas 类型 </param>
        private void GenerateCanvas(int canvasIndex, string style)
        {
            string canvasName = $"{style}{canvasIndex}"; // Canvas名称
            bool haveButton = CheckActionPageHasButton(canvasIndex, style); // 检查页面是否有按钮
            if (haveButton) GenerateButtons(new Canvas(), canvasIndex, style); // 如果动作页有按钮，生成按钮
        }

        /// <summary>
        /// 检查页面是否有按钮
        /// </summary>
        /// <param name="canvasIndex"> 要检查的页面索引 </param>
        /// <param name="style"> Canvas 类型 </param>
        /// <returns> 页面是否有按钮 </returns>
        private bool CheckActionPageHasButton(int canvasIndex, string style)
        {
            if (canvasIndex == 0) return true;
            string pattern = $@"^{style}{canvasIndex}(\d)(\d)(\d)$"; // 正则表达式模式
            Regex regex = new Regex(pattern); // 创建正则表达式对象
            var buttonData = db2.GetButtonDataByPrefix(style); // 从数据库中获取按钮数据
            foreach (var data in buttonData) // 遍历按钮数据字典
            {
                string buttonID = data.ButtonID; // 获取按钮ID
                Match match = Regex.Match(data.ButtonID, @"^([a-zA-Z0-9_]+)(\d{3})$"); // 匹配按钮名称和末尾的3个数字
                if (match.Success)
                {
                    string numbersStr = match.Groups[2].Value; // 获取3个数字
                    int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray(); // 转换为整数数组

                    int b = numbers[0]; // 提取 b 值
                    if (b >= canvasIndex) return true; // 如果 b 值大于等于当前页面索引，则表示该页面有按钮
                }
            }
            return false; // 如果没有找到符合条件的按钮，返回 false
        }

        /// <summary>
        /// 生成按钮
        /// </summary>
        /// <param name="canvas"> Canvas 对象 </param>
        /// <param name="canvasIndex"> Canvas 索引 </param>
        /// <param name="style"> Canvas 类型 </param>
        private void GenerateButtons(Canvas canvas, int canvasIndex, string style)
        {
            string canvasName = $"{style}{canvasIndex}"; // Canvas名称
            Canvas newCanvas = new Canvas { Name = canvasName }; // 创建Canvas对象

            if (style == "Global")
            {
                GlobalGrid.Children.Add(newCanvas); // 添加到主Grid
                newCanvas.IsVisibleChanged += GlobalCanvas_IsVisibleChanged; // 添加可见性变化事件
            }
            else
            {
                CommonGrid.Children.Add(newCanvas); // 添加到公共Grid
                newCanvas.IsVisibleChanged += CommonCanvas_IsVisibleChanged; // 添加可见性变化事件
            }

            Panel ParentPanel = style == "Global" ? GlobalButtonPanel : CommonButtonPanel; // 根据样式选择父面板
            foreach (var button in ParentPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
            {
                button.Background = button.Name.Contains($"{canvasIndex}")
                    ? new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SelectedPageButtonColor))
                    : new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(UnSelectedPageButtonColor)); // 设置当前按钮颜色
            }

            double buttonSpacing = 77.6; // 按钮间距
            int rows = style == "Global" ? 3 : 4, cols = 4; // 行数和列数
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int buttonIndex = row * 4 + col + 1; // 按钮在Canvas中的位置
                    string buttonName = $"{style}{canvasIndex}{row + 1}{col + 1}"; // 按钮名称
                    Margin = new Thickness(col * buttonSpacing, row * buttonSpacing, 0, 0); // 按钮布局
                    Style styleResource = FindResource("Button") as Style; // 按钮样式
                    Button button = CreateButton(buttonName, styleResource, Margin, row, col); // 创建按钮
                    newCanvas.Children.Add(button); // 添加按钮到Canvas

                    var buttonData = db2.GetButtonDataByPrefix(style); // 从数据库中获取按钮数据
                    foreach (var data in buttonData)
                    {
                        if (data.ButtonID == button.Name)
                            buttonManager.RefreshButtonDisplay(button, data, 60, true); // 更新按钮内容
                    }
                }
            }
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        /// <param name="name">Button 的名称</param>
        /// <param name="style">Button 的样式</param>
        /// <param name="margin">Button 的布局</param>
        /// <param name="row">Button 的行</param>
        /// <param name="col">Button的列</param>
        /// <returns>生成的 Button</returns>
        private Button CreateButton(string name, Style style, Thickness margin, int row = 0, int col = 0)
        {
            Button button = new Button
            {
                Name = name, // 设置名称
                Style = style, // 设置样式
                Margin = margin, // 设置布局
                AllowDrop = true, // 允许拖拽
            };

            if (row == 3 && col == 0) button.Style = FindResource("SpecialButton1") as Style; // 设置特殊样式
            else if (row == 3 && col == 3) button.Style = FindResource("SpecialButton2") as Style; // 设置特殊样式

            BindButtonEvents(button); // 绑定按钮事件
            return button; // 返回创建的按钮
        }

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        /// <param name="button">指定绑定的 Button</param>
        private void BindButtonEvents(Button button)
        {
            button.Click += DoAction; // 左键点击事件
            button.Drop += Button_Drop; // 拖拽事件
            button.MouseEnter += Button_MouseEnter; // 鼠标移入事件
            button.MouseLeave += Button_MouseLeave; // 鼠标移出事件
            button.PreviewDragOver += Button_PreviewDragOver; // 添加拖拽事件
            button.MouseRightButtonDown += OpenCreatActionMenu; // 右键点击事件
            button.PreviewMouseMove += Button_PreviewMouseMove; // 鼠标移动事件
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp; // 鼠标左键释放事件
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
        }

        // 全局动作页可见性与切换按钮背景绑定
        private void GlobalCanvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Canvas canvas && canvas.IsVisible)
            {
                int canvasIndex = int.Parse(canvas.Name.Replace("Global", "")); // 获取Canvas索引
                foreach (var button in GlobalButtonPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
                {
                    button.Background = button.Name.Contains($"{canvasIndex}") // 判断是否是当前按钮
                        ? new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SelectedPageButtonColor))
                        : new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(UnSelectedPageButtonColor)); // 设置当前按钮颜色
                } // 设置所有按钮的颜色
            }
        }

        // 锁定通用动作页
        private void LockCommonActionPage(object sender, RoutedEventArgs e)
        {
            app._appStateManager.Locked = !app._appStateManager.Locked; // 切换锁定状态
            string lockIconPath = app._appStateManager.Locked ? LockIconPath : UnLockIconPath; // 获取图标路径
            BitmapImage lockImage = new BitmapImage(new Uri(lockIconPath, UriKind.Relative)); // 创建 BitmapImage 对象
            Lock.Source = lockImage; // 设置图标
            if (app._appStateManager.Locked) app._appStateManager.CommonState = CommonStyle; // 设置锁定状态
        }

        // 滚轮进行通用动作页翻页
        private void CommonGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeVisibleCanvas(e, CommonStyle); // 调用切换 Canvas 方法
        }

        // 通用动作页可见性与切换按钮背景绑定
        private void CommonCanvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Canvas canvas && canvas.IsVisible)
            {
                int canvasIndex = int.Parse(canvas.Name.Replace($"{CommonStyle}", "")); // 获取Canvas索引
                foreach (var button in CommonButtonPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
                {
                    button.Background = button.Name.Contains($"{canvasIndex}") // 判断是否是当前按钮
                        ? new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SelectedPageButtonColor))
                        : new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(UnSelectedPageButtonColor)); // 设置当前按钮颜色
                } // 设置所有按钮的颜色
            }
        }

        // 右键锁定 Button 切换菜单
        private void OpenSelectActionPageMenu(object sender, MouseButtonEventArgs e)
        {
            //buttonManager.OpenMenu(sender, true, "SelectActionPageMenu", this); // 打开菜单
            //ToastWindow toast = new ToastWindow();
            //toast.Show();
        }

        // 窗口关闭时强制垃圾回收
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的关闭事件
            cancellationTokenSource.Cancel(); // 取消所有后台任务
            cancellationTokenSource.Dispose();
            CleanUpEventHandlers(); // 清理事件处理器
            CleanUpCanvas(MainGrid); // 清理全局网格
            CleanUpCanvas(CommonGrid); // 清理通用网格
            Book.Source = null; // 订住按钮图片
            Lock.Source = null; // 锁定按钮图片

            actionManager.Dispose(); // 释放动作管理器资源
            windowManager.Dispose(); // 释放窗口管理器资源
            iconManager.Dispose(); // 释放图标管理器资源
            buttonManager.Dispose(); // 释放按钮管理器资源

            GC.Collect(); // 强制回收内存
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制回收内存
        }

        /// <summary>
        /// 清理指定Grid中的所有Canvas及其子元素
        /// </summary>
        /// <param name="grid">要清理的Grid</param>
        private void CleanUpCanvas(Grid grid)
        {
            foreach (Canvas canvas in FindVisualChildren<Canvas>(grid))
            {
                foreach (Button button in FindVisualChildren<Button>(canvas))
                {
                    // 移除所有事件处理器
                    button.Click -= DoAction;
                    button.Drop -= Button_Drop;
                    button.MouseEnter -= Button_MouseEnter;
                    button.MouseLeave -= Button_MouseLeave;
                    button.PreviewDragOver -= Button_PreviewDragOver;
                    button.MouseRightButtonDown -= OpenCreatActionMenu;
                    button.PreviewMouseMove -= Button_PreviewMouseMove;
                    button.PreviewMouseLeftButtonUp -= Button_PreviewMouseLeftButtonUp;
                    button.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown;

                    // 清理按钮内容和资源
                    button.Content = null;
                    button.Tag = null;
                    button.Background = null;
                }

                // 移除Canvas事件
                canvas.IsVisibleChanged -= GlobalCanvas_IsVisibleChanged;
                canvas.IsVisibleChanged -= CommonCanvas_IsVisibleChanged;

                canvas.Children.Clear(); // 清空Canvas
            }
            grid.Children.Clear(); // 清空Grid
        }

        // 清理所有动态添加的事件处理器
        private void CleanUpEventHandlers()
        {
            foreach (Button button in GlobalButtonPanel.Children.OfType<Button>()) // 清理全局按钮面板事件
            {
                button.Click -= SwitchToGlobalCanvas;
                button.MouseEnter -= GlobalActionPageChangeButton_MouseEnter;
                button.MouseLeave -= GlobalActionPageChangeButton_MouseLeave;
            }

            foreach (Button button in CommonButtonPanel.Children.OfType<Button>()) // 清理公共按钮面板事件
            {
                button.Click -= SwitchToCommonCanvas;
                button.MouseEnter -= CommonActionPageChangeButton_MouseEnter;
                button.MouseLeave -= CommonActionPageChangeButton_MouseLeave;
            }

            // 移除窗口事件
            this.Loaded -= MainWindow_Loaded;
            this.Closing -= MainWindow_Closing;
            this.Deactivated -= MainWindow_Deactivated;
            this.MouseLeftButtonDown -= MoveMainWindow;

            // 移除网格事件
            GlobalGrid.MouseWheel -= CommonGrid_MouseWheel;
            CommonGrid.MouseWheel -= CommonGrid_MouseWheel;
        }
    }
}