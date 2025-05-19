using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;
using Quicker;

namespace Quicker.Windows
{
    public partial class ActionPageManageWindow : Window
    {
        private const string changeActionPageButtonImage = "/Resources/Images/Icons/Quicker1.ico";
        private const string editActionPageButtonImage = "/Resources/Images/Icons/Quicker1.ico";

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent; // 找到父级控件
                child = VisualTreeHelper.GetParent(child); // 获取父级控件
            }
            return null;
        } // 查找父级控件

        private bool isDarkModle = false, showUsedTimes = false; // 是否为暗黑模式，是否显示使用次数
        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ToastManager toastManager = new(); // 通知管理器
        private readonly ActionPageDatabase db3 = new(); // 动作页数据库
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private Point initialMousePosition; // 初始鼠标位置
        private string type; // 场景类型

        public ActionPageManageWindow()
        {
            InitializeComponent(); // 初始化窗口
            LoadBasicButtonPrefixes(); // 加载按钮前缀
            TypeChanged("Common"); // 默认加载通用场景
        }

        /// <summary>
        /// 类型改变事件
        /// </summary>
        /// <param name="targetType"> 目标类型 </param>
        private void TypeChanged(string targetType)
        {
            type = targetType; // 设置类型
            LoadCanvas(type); // 加载动作页画布
            SetButtonBackground(); // 设置场景按钮背景色
        }

        // 设置场景按钮背景色
        private void SetButtonBackground()
        {
            foreach (UIElement element in ActionPagesButtonPanel.Children)
            {
                Button button = element as Button; // 转换为按钮
                button.Background = button.Name == type
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA"))
                    : System.Windows.Media.Brushes.Transparent; // 设置背景色
            }
        }

        // 鼠标移入Button高亮显示黑名单项
        private void HightLightBlacklistItem(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 转换发送者为Button对象
            if(button.Name != type)
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
            button.Background = button.Name == type
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置背景色
        }

        // 鼠标移出Button恢复原状
        private void FadeBlacklistItem(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 转换发送者为Button对象
            button.Background = button.Name == type
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA"))
                : System.Windows.Media.Brushes.Transparent; // 设置背景色
        }

        // 加载场景按钮
        private void LoadBasicButtonPrefixes()
        {
            var buttonInfo = new[]
            {
                new { Name = "Global", Text = "全局" }, // 全局场景按钮
                new { Name = "Common", Text = "通用" }, // 通用场景按钮
                new { Name = "Taskbar", Text = "任务栏" }, // 任务栏场景按钮
                new { Name = "Desktop", Text = "桌面" } // 桌面场景按钮
            }; // 定义按钮名称和文本的映射
            var buttonClickHandlers = new[]
            {
                new RoutedEventHandler(GlobalButton_Click), // 全局场景按钮点击事件
                new RoutedEventHandler(CommonButton_Click), // 通用场景按钮点击事件
                new RoutedEventHandler(TaskBarButton_Click), // 任务栏场景按钮点击事件
                new RoutedEventHandler(DesktopButton_Click) // 桌面场景按钮点击事件
            }; // 为每个按钮创建事件处理方法

            // 动态生成按钮
            for (int i = 0; i < buttonInfo.Length; i++)
            {
                var button = new Button();
                button.Name = buttonInfo[i].Name;
                button.Style = FindResource("SceneButton") as Style; // 应用样式

                // 创建按钮内容
                Grid buttonContent = new();
                TextBlock textBlock = new()
                {
                    FontSize = 14,
                    Text = buttonInfo[i].Text,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }; // 创建 TextBlock
                buttonContent.Children.Add(textBlock);
                button.Content = buttonContent;

                // 设置点击事件
                button.Click += buttonClickHandlers[i];
                button.MouseEnter += HightLightBlacklistItem; // 鼠标移入高亮显示
                button.MouseLeave += FadeBlacklistItem; // 鼠标移出恢复原状

                // 将按钮添加到StackPanel
                ActionPagesButtonPanel.Children.Add(button);
            }
        }

        // 生成场景按钮
        private void GenerateButtons(string style)
        {

        }

        /// <summary>
        /// 加载动作页
        /// </summary>
        /// <param name="style">动作页样式</param>
        private void LoadCanvas(string style)
        {
            MainListView.Items.Clear(); // 清空总列表视图
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
                    if (db2.TableExists(style)) // 如果存在通用样式按钮数据表
                    {
                        MainBorder.Height = 289; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加动作页按钮边距
                    }
                    else
                    {
                        MainBorder.Height = 224; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 272, 0, 0); // 设置添加动作页按钮边距
                    }
                    break;
            }

            if (!db2.TableExists(style)) return; // 如果不存在按钮数据表，则返回
            var actionPageData = db3.GetSceneData(style).FirstOrDefault(); // 获取动作页数据
            for (int i = 0; i < actionPageData.SceneCount; i++)
            {
                MainListView.Items.Add(GenerateCanvas(i, style)); // 生成动作页
            }
        }

        /// <summary>
        /// 生成动作页
        /// </summary>
        /// <param name="canvasIndex"> 动作页索引 </param>
        /// <param name="style"> 动作页类型 </param>
        private Canvas GenerateCanvas(int canvasIndex, string style)
        {
            Canvas dynamicCanvas = GenerateCanva(canvasIndex, style); // 生成画布
            Grid grid = GenerateTitle(canvasIndex, style); // 生成标题
            dynamicCanvas.Children.Add(grid); // 将网格添加到画布
            Button pageButton = GenerateChangePageButton(canvasIndex, style); // 生成标题按钮
            grid.Children.Add(pageButton);
            pageButton.Content = GenerateImage(changeActionPageButtonImage); // 生成标题按钮图片

            Button editPageButton = GenerateEditActionPageButton(canvasIndex, style); // 生成编辑动作页按钮
            grid.Children.Add(editPageButton);
            editPageButton.Content = GenerateImage(editActionPageButtonImage); // 生成编辑动作页按钮图片

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

                    var data = db2.GetButtonDataByID(buttonName); // 获取按钮数据
                    buttonManager.RefreshButtonDisplay(button, data, 60, false); // 刷新按钮显示
                }
            }
            return dynamicCanvas; // 返回画布
        }

        /// <summary>
        /// 生成画布
        /// </summary>
        /// <param name="canvasIndex"> 画布索引 </param>
        /// <param name="style"> 画布类型 </param>
        /// <returns> 画布 </returns>
        private Canvas GenerateCanva(int canvasIndex, string style)
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
            return dynamicCanvas; // 返回画布
        }

        /// <summary>
        /// 生成标题
        /// </summary>
        /// <param name="canvasIndex"> 画布索引 </param>
        /// <param name="style"> 画布类型 </param>
        /// <returns> 标题 </returns>
        private Grid GenerateTitle(int canvasIndex, string style)
        {
            Grid grid = new Grid
            {
                Height = 20, // 网格高度
                Width = 260, // 网格宽度
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Left, // 水平对齐方式
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")) // 背景颜色
            }; // 创建网格
            return grid; // 返回网格
        }

        /// <summary>
        /// 生成编辑标题按钮
        /// </summary>
        /// <param name="canvasIndex"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private Button GenerateChangePageButton(int canvasIndex, string style)
        {
            Button pageButton = new Button
            {
                Width = 17.24, // 按钮宽度
                Tag = $"{style}{canvasIndex}", // 按钮标签
                Name = $"{style}{canvasIndex}", // 按钮名称
                Margin = new Thickness(3, 0, 0, 0), // 按钮边距
                BorderThickness = new Thickness(0, 0, 0, 0), // 按钮边框
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Left // 水平对齐方式
            };
            pageButton.PreviewMouseLeftButtonDown += buttonManager.Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            pageButton.PreviewMouseMove += Button1_PreviewMouseMove; // 鼠标移动事件
            pageButton.PreviewMouseLeftButtonUp += buttonManager.Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件
            return pageButton; // 返回按钮
        }

        /// <summary>
        /// 生成编辑动作页按钮
        /// </summary>
        /// <param name="canvasIndex"> 画布索引 </param>
        /// <param name="style"> 画布类型 </param>
        /// <returns> 编辑动作页按钮 </returns>
        private Button GenerateEditActionPageButton(int canvasIndex, string style)
        {
            Button editPageButton = new Button
            {
                Width = 17.24, // 按钮宽度
                Name = $"Edit{style}{canvasIndex}", // 按钮名称
                Margin = new Thickness(0, 0, 3, 0), // 按钮边距
                BorderThickness = new Thickness(0, 0, 0, 0), // 按钮边框
                VerticalAlignment = VerticalAlignment.Center, // 垂直对齐方式
                HorizontalAlignment = HorizontalAlignment.Right // 水平对齐方式
            };
            editPageButton.Click += OpenEditPopup; // 点击事件
            return editPageButton; // 返回按钮
        }

        /// <summary>
        /// 生成图片
        /// </summary>
        /// <returns> 图片 </returns>
        private Image GenerateImage(string imagePath)
        {
            Image image = new Image { Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)) };
            return image; // 返回图片
        }

        // 点击按钮编辑动作页
        private void OpenEditPopup(object sender, RoutedEventArgs e)
        {
            EditActionPagePopup.PlacementTarget = sender as Button; // 设置弹出菜单位置
            EditActionPagePopup.IsOpen = true; // 显示弹出菜单
        }

        // 滚动条值改变事件
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToHorizontalOffset(ScrollBar.Value); // 滚动到指定位置
        }

        // 鼠标移入按钮改变背景色
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data) // 如果按钮数据存在
            {
                if (isDarkModle)
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray")); // 设置按钮背景颜色
                else
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD")); // 设置按钮背景颜色
 
                if (showUsedTimes)
                    buttonManager.LoadActionUsedTimes(button, data); // 刷新按钮显示
            }
            else
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 设置按钮背景颜色
        }

        // 鼠标移出按钮还原背景色
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data) // 如果按钮有数据
            {
                if (isDarkModle)
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray")); // 设置按钮背景颜色
                else
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色

                if(showUsedTimes)
                    buttonManager.RefreshButtonDisplay(button, data, 60, false); // 刷新按钮显示
            }
            else
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置按钮背景颜色
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
            button.PreviewMouseRightButtonDown += OpenMenu; // 右键点击事件
            button.PreviewMouseMove += Button_PreviewMouseMove; // 鼠标移动事件
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件
        }

        // 拖放事件
        private void Button_Drop(object sender, DragEventArgs e)
        {
            if (sender is Button TargetButton)
                buttonManager.Button_Drop(sender, e, false); // 处理拖放事件
        }

        // 鼠标左键按下事件
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
                buttonManager.Button_PreviewMouseLeftButtonDown(sender, e); // 处理鼠标左键按下事件
        }

        // 鼠标移动事件
        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed) // 如果鼠标左键按下
                buttonManager.Button_PreviewMouseMove(sender, e, true); // 处理鼠标移动事件
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
                buttonManager.OpenMenu(sender, false, "CreatActionMenu", this); // 打开创建动作菜单
        }

        // 显示编辑窗口
        private void ShowEditWindow(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is Button button && button.Tag != null)
            {
                AddWindow addWindow = new AddWindow(button.Name, 0); // 创建编辑窗口
                addWindow.Show(); // 显示编辑窗口
                addWindow.Activate(); // 激活编辑窗口
            }
        }

        // 添加场景
        private void AddActionPage(object sender, RoutedEventArgs e)
        {
            int canvasCount = MainListView.Items.Count; // 获取画布索引
            if (canvasCount == 10) // 如果画布索引等于9
                toastManager.AddToast("当前场景数量已达上限。", "Error"); // 弹出消息提醒
            else if (canvasCount == 0)
            {
                db2.CreateButtonTable(type); // 创建按钮数据表
                db3.CreatAndInitTable(type,"",""); // 创建场景数据表
                MainListView.Items.Add(GenerateCanvas(canvasCount, type)); // 如果画布索引为0，则生成画布
                if(type != "Global")
                {
                    MainBorder.Height = 289; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加场景按钮边距
                }
            }
            else
            {
                db3.UpdateActionPageTable(type, type, "", canvasCount, ""); // 更新场景数据表
                MainListView.Items.Add(GenerateCanvas(canvasCount, type)); // 生成画布
            }
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
            TypeChanged("Global"); // 切换类型为全局场景
        }

        // 公共按钮点击事件
        private void CommonButton_Click(object sender, RoutedEventArgs e)
        {
            TypeChanged("Common"); // 加载通用场景
        }

        // 加载任务栏场景
        private void TaskBarButton_Click(object sender, RoutedEventArgs e)
        {
            TypeChanged("Taskbar"); // 设置类型为任务栏场景
        }

        // 加载桌面场景
        private void DesktopButton_Click(object sender, RoutedEventArgs e)
        {
            TypeChanged("Desktop"); // 设置类型为桌面场景
        }

        // 打开创建动作菜单
        private void OpenMenu(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // 阻止默认右键菜单
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data && button != null)
                buttonManager.OpenMenu(sender, false, "OperationMenu", this); // 打开操作菜单
            else
                buttonManager.OpenMenu(sender, false, "CreatActionMenu", this); // 打开创建动作菜单
        }

        /// <summary>
        /// 编辑动作后刷新按钮显示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        public void UpdateButton(string button)
        {
            string nums = button.Replace(type, ""); // 获取按钮编号
            int index = int.Parse(nums) / 100; // 获取按钮所在动作页索引
            var oldCanvas = MainListView.Items[index] as Canvas; // 获取旧的 Canvas
            foreach(var child in oldCanvas.Children)
            {
                if(child is Button targetButton)
                {
                    ButtonData data = db2.GetButtonDataByID(targetButton.Name); // 获取按钮数据
                    buttonManager.RefreshButtonDisplay(targetButton, data, 60, false); // 刷新按钮显示
                }
            }
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void Button1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
                buttonManager.Button_PreviewMouseMove(sender, e, false); // 处理鼠标移动事件
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

                    db2.SwapButtonAValues(type, sourceIndex, targetIndex); // 更新数据库 Button A 值
                    UpdateCanvasInListView(sourceIndex, type); // 更新 ListView 中的特定 Canvas
                    UpdateCanvasInListView(targetIndex, type); // 更新 ListView 中的特定 Canvas
                }
            }
        }

        /// <summary>
        /// 更新 ListView 中的特定 Canvas
        /// </summary>
        /// <param name="canvasIndex">画布索引</param>
        /// <param name="styleType">场景类型</param>
        private void UpdateCanvasInListView(int canvasIndex, string styleType)
        {
            var oldCanvas = MainListView.Items[canvasIndex] as Canvas; // 获取旧的 Canvas
            MainListView.Items.Remove(oldCanvas); // 从主列表视图中移除旧的 Canvas
            Canvas newCanvas = GenerateCanvas(canvasIndex, styleType); // 生成新的 Canvas
            MainListView.Items.Insert(canvasIndex, newCanvas); // 在主列表视图中插入新的 Canvas
        }

        // 查找场景按钮
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower(); // 获取用户输入的文本并转换为小写
            if (string.IsNullOrEmpty(searchText))
                ActionPagesButtonPanel.Children.Clear(); // 清空场景按钮面板
        }

        // 双击标签切换动作按钮背景色
        private void ChangeActionButtonBakground(object sender, MouseButtonEventArgs e)
        {
            isDarkModle = !isDarkModle; // 切换模式
            foreach (var item in MainListView.Items)
            {
                Canvas canvas = item as Canvas; // 获取画布
                var childrenList = canvas.Children.Cast<UIElement>().ToList(); // 将 UIElementCollection 转换为列表
                foreach (var child in childrenList)
                {
                    if (child is Button button && button.Tag is ButtonData data)
                    {
                        if (isDarkModle)
                            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray")); // 设置按钮背景颜色
                        else
                            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
                    }
                }
            }
        }

        /// <summary>
        /// 获取绑定按钮
        /// </summary>
        /// <returns> 绑定按钮 </returns>
        private Button GetBingdingButton()
        {
            EditActionPagePopup.IsOpen = false; // 关闭编辑场景弹出菜单
            return EditActionPagePopup.PlacementTarget as Button; // 获取弹出菜单所绑定按钮
        }

        // 点击按钮查看场景信息
        private void CheckActionPageInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
        }

        // 点击按钮复制场景 ID
        private void CopyActionPageIDButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            Clipboard.SetText(bingdingButton.Name.Replace("Edit", "")); // 复制文本到剪贴板
            toastManager.AddToast($"场景ID已复制到剪贴板：{bingdingButton.Name.Replace("Edit", "")}", "Common"); // 显示复制成功的通知
        }

        // 点击按钮编辑场景信息
        private void EditActionPageInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            switch (type)
            {
                case "Global":
                    toastManager.AddToast("默认全局场景信息不可修改。", "Common"); // 弹出消息提醒
                    break;
                case "Common":
                    toastManager.AddToast("默认通用场景信息不可修改。", "Common"); // 弹出消息提醒
                    break;
                default:
                    string canvasIndex = bingdingButton.Name.Replace("Edit" + type, ""); // 获取画布索引
                    EditActionPageInfoWindow editActionPageInfoWindow = new(type, canvasIndex); // 创建编辑场景信息窗口
                    editActionPageInfoWindow.Show(); // 显示编辑场景信息窗口
                    break;
            }
        }

        // 点击按钮创建打开场景动作
        private void CreatOpenActionPageActionButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            string actionPageIndex = bingdingButton.Name.Replace("Edit" + type, ""); // 获取场景名称
            string openActionPageCommand = $"OpenActionPage;{type};{actionPageIndex};OpenActionPageCommand"; // 生成打开场景指令
            Clipboard.SetText(openActionPageCommand); // 复制文本到剪贴板
            toastManager.AddToast("已创建动作并写入剪贴板，请粘贴到合适位置。", "Common"); // 显示创建成功的通知
        }

        // 点击按钮删除场景
        private void DeleteActionPageButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            // 移除画布
            Grid targetGrid = bingdingButton.Parent as Grid; // 获取目标画布
            MainListView.Items.Remove(targetGrid.Parent); // 从主列表视图中移除画布
            // 删除按钮数据页
            string targetButtonName = bingdingButton.Name.Replace("Edit", ""); // 获取目标按钮名称
            int canvadIndex = int.Parse(targetButtonName.Replace(type, "")); // 获取画布索引
            db2.DeletePageOfButtons(type, canvadIndex); // 删除按钮数据页
            // 更新场景数据表
            var sceneData = db3.GetSceneData(type).FirstOrDefault(); // 获取场景数据
            db3.UpdateActionPageTable(type, type, sceneData.SceneIconPath, MainListView.Items.Count, sceneData.SceneTag); // 更新场景数据表
            if(MainListView.Items.Count == 0)
            {
                db2.DeleteButtonTable(type); // 如果没有画布，则删除按钮数据表
                db3.DeleteActionPageTable(type, type); // 如果没有画布，则删除场景数据表
            }
            LoadCanvas(type); // 刷新界面
        }

        // 点击按钮添加场景
        private void AddSceneButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // 点击按钮编辑场景
        private void EditSceneButton_Click(object sender, RoutedEventArgs e)
        {
            switch (type)
            {
                case "Global":
                case "Common":
                case "Taskbar":
                case "Desktop":
                    toastManager.AddToast("此项不可编辑。", "Common"); // 弹出消息提醒
                    break;
                default:
                    string canvasIndex = GetBingdingButton().Name.Replace("Edit" + type, ""); // 获取画布索引
                    EditSceneWindow editSceneWindow = new(type); // 创建编辑场景窗口
                    editSceneWindow.Show(); // 显示编辑场景窗口
                    break;
            }
        }

        // 点击按钮删除场景
        private void DeleteSceneButton_Click(object sender, RoutedEventArgs e)
        {
            switch (type)
            {
                case "Global":
                case "Common":
                case "Taskbar":
                case "Desktop":
                    toastManager.AddToast("不能删除此场景。", "Common"); // 弹出消息提醒
                    break;
                default:
                    break;
            }
        }

        // 点击按钮前往顶层场景
        private void ToTopSceneButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalButton_Click(sender, e); // 切换类型为全局场景
        }

        // 右键显示/隐藏动作使用次数
        private void AddActionPageButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            showUsedTimes = !showUsedTimes; // 切换显示/隐藏使用次数
        }

        // 关闭窗口时释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            // 清理主列表视图中的所有 Canvas 及其子元素
            while (MainListView.Items.Count > 0)
            {
                if (MainListView.Items[0] is Canvas canvas)
                {
                    // 移除 Canvas 中的所有子元素
                    while (canvas.Children.Count > 0)
                    {
                        UIElement child = canvas.Children[0];
                        if (child is Grid grid)
                        {
                            // 移除 Grid 中的所有子元素
                            while (grid.Children.Count > 0)
                            {
                                UIElement gridChild = grid.Children[0];
                                if (gridChild is Button button)
                                {
                                    // 清理按钮资源
                                    button.Content = null;
                                    button.Tag = null;
                                    button.Background = null;
                                    button.Style = null;
                                }
                                grid.Children.Remove(gridChild);
                            }
                            // 清理 Grid 资源
                            grid.Children.Clear();
                        }
                        else if (child is Button button)
                        {
                            // 移除按钮的所有事件处理程序
                            button.Drop -= Button_Drop;
                            button.Click -= ShowCreatActionMenu;
                            button.MouseEnter -= Button_MouseEnter;
                            button.MouseLeave -= Button_MouseLeave;
                            button.MouseDoubleClick -= ShowEditWindow;
                            button.PreviewMouseRightButtonDown -= OpenMenu;
                            button.PreviewMouseMove -= Button_PreviewMouseMove;
                            button.PreviewMouseLeftButtonUp -= Button_PreviewMouseLeftButtonUp;
                            button.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown;
                            // 清理按钮资源
                            button.Content = null;
                            button.Tag = null;
                            button.Background = null;
                            button.Style = null;
                        }
                        canvas.Children.Remove(child);
                    }
                    // 移除 Canvas 本身
                    MainListView.Items.Remove(canvas);
                }
            }

            // 清理场景按钮面板中的所有按钮
            while (ActionPagesButtonPanel.Children.Count > 0)
            {
                UIElement child = ActionPagesButtonPanel.Children[0];
                if (child is Button button)
                {
                    // 移除按钮的所有事件处理程序
                    button.Click -= GlobalButton_Click;
                    button.Click -= CommonButton_Click;
                    button.Click -= TaskBarButton_Click;
                    button.Click -= DesktopButton_Click;
                    button.MouseEnter -= HightLightBlacklistItem;
                    button.MouseLeave -= FadeBlacklistItem;
                    // 清理按钮资源
                    button.Content = null;
                    button.Tag = null;
                    button.Background = null;
                    button.Style = null;
                }
                ActionPagesButtonPanel.Children.Remove(child);
            }

            // 释放其他资源
            toastManager?.Dispose(); // 释放消息管理器资源

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}