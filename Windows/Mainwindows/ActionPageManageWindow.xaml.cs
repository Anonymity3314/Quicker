using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Quicker.Windows.EditWindows;
using Quicker.Windows.ToolWindows;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;

namespace Quicker.Windows.MainWindows
{
    public partial class ActionPageManageWindow : Window
    {
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
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
        private readonly ActionPageDatabase db3 = new(); // 动作页数据库
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private string type; // 场景类型

        public ActionPageManageWindow(string type = "Common")
        {
            InitializeComponent(); // 初始化窗口
            GenerateSceneButtons(); // 加载按钮前缀
            TypeChanged(type); // 默认加载通用场景
        }

        /// <summary>
        /// 场景类型改变事件
        /// </summary>
        /// <param name="targetType"> 目标类型 </param>
        private void TypeChanged(string targetType)
        {
            if (type == targetType) return; // 如果目标类型与当前类型相同，直接返回
            LoadingWindow loadingWindow = new(); // 创建加载窗口
            loadingWindow.Show(); // 显示加载窗口
            type = targetType; // 设置类型
            LoadCanvas(type); // 加载动作页画布
            LoadSettings(); // 加载设置
            SetButtonBackground(); // 设置场景按钮背景色
            SetSceneTitle(); // 设置场景标题
            loadingWindow?.Close(); // 关闭加载窗口
        }

        // 加载设置
        private void LoadSettings()
        {
            var autoReturn = db3.GetAutoReturnToFirstPage(type); // 获取设置
            AutoReturnToFirstPageCheckBox.IsChecked = autoReturn; // 加载设置
        }

        // 设置场景按钮背景色
        private void SetButtonBackground()
        {
            foreach (UIElement element in ActionPagesButtonPanel.Children)
            {
                Button button = element as Button; // 转换为按钮
                button.Background = button.Name == type
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9E9E9"))
                    : System.Windows.Media.Brushes.Transparent; // 设置背景色
            }
        }

        // 鼠标移入Button高亮按钮
        private void HightLightBlacklistItem(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 转换发送者为Button对象
            button.Background = button.Name == type
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DFDFDF"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")); // 设置背景色
        }

        // 鼠标移出Button恢复原状
        private void FadeBlacklistItem(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 转换发送者为Button对象
            button.Background = button.Name == type
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9E9E9"))
                : System.Windows.Media.Brushes.Transparent; // 设置背景色
        }

        // 生成场景按钮
        private void GenerateSceneButtons()
        {
            var sceneData = db3.GetAllSceneData(); // 获取所有场景数据
            var defaultScenes = new List<string> { "Global", "Common", "Taskbar", "Desktop" }; // 默认场景顺序
            var otherScenes = new List<SceneData>(); // 其他场景
            foreach (var data in sceneData) // 遍历场景数据
            {
                if (defaultScenes.Contains(data.SceneName)) // 如果是默认场景，按照指定顺序插入到列表中
                {
                    int index = defaultScenes.IndexOf(data.SceneName); // 获取索引
                    if (index < 0) index = defaultScenes.Count; // 如果索引不存在，则设置为默认列表末尾
                    while (otherScenes.Count <= index) // 填充空项
                    {
                        otherScenes.Add(null); // 添加空项
                    }
                    otherScenes[index] = data; // 插入数据
                }
                else
                    otherScenes.Add(data); // 否则添加到其他场景列表中
            }
            otherScenes = otherScenes.Where(s => s != null).ToList(); // 过滤空项
            foreach (var data in otherScenes) // 添加默认场景和其他场景
            {
                CreateSceneButton(data); // 创建场景按钮
            }
        }

        /// <summary>
        /// 创建场景按钮
        /// </summary>
        /// <param name="sceneData"> 场景数据 </param>
        private void CreateSceneButton(SceneData sceneData)
        {
            var button = new Button()
            {
                Style = FindResource("SceneButton") as Style, // 应用样式
                Name = sceneData.SceneName // 按钮名称
            };
            Grid buttonContent = CreateButtonContent(sceneData); // 使用模块化方法创建按钮内容
            button.Content = buttonContent; // 设置按钮内容
            SetupButtonEvents(button); // 使用模块化方法设置按钮事件
            ActionPagesButtonPanel.Children.Add(button); // 将按钮添加到StackPanel
        }

        /// <summary>
        /// 创建场景按钮内容
        /// </summary>
        /// <param name="sceneData"> 场景数据 </param>
        /// <returns> 按钮内容 </returns>
        private Grid CreateButtonContent(SceneData sceneData)
        {
            Grid buttonContent = new() { Style = FindResource("SceneButtonContentGrid") as Style }; // 创建网格
            Image image = new()
            {
                Source = new BitmapImage(new Uri(sceneData.SceneIconPath, UriKind.Relative)), // 设置按钮图片
                Style = FindResource("SceneButtonImage") as Style // 应用样式
            }; // 创建图片
            buttonContent.Children.Add(image); // 添加到网格
            TextBlock sceneName = new()
            {
                Style = FindResource("SceneNameTextBlock") as Style, // 应用样式
                Text = db3.GetSceneTitle(sceneData) // 文本内容
            }; // 创建 TextBlock
            buttonContent.Children.Add(sceneName); // 添加到网格
            TextBlock sceneTag = new()
            {
                Style = FindResource("SceneTagTextBlock") as Style, // 应用样式
                Text = sceneData.SceneTag, // 文本内容
            }; // 创建 TextBlock
            buttonContent.Children.Add(sceneTag); // 添加到网格
            return buttonContent; // 返回按钮内容
        }

        /// <summary>
        /// 设置场景按钮事件
        /// </summary>
        /// <param name="button"> 按钮 </param>
        private void SetupButtonEvents(Button button)
        {
            button.Click += new RoutedEventHandler(ChanceSceneButton_Click); // 设置按钮点击事件
            button.DragEnter += ChangeSceneButtonBackground; // 设置拖拽事件
            button.DragLeave += ResetSceneButtonBackground; // 还原背景色
            button.MouseEnter += HightLightBlacklistItem; // 鼠标移入高亮显示
            button.MouseLeave += FadeBlacklistItem; // 鼠标移出恢复原状
            button.Drop += ChangeActionPageScene; // 设置拖拽事件
        }

        // 设置场景按钮背景色
        private void ChangeSceneButtonBackground(object sender, DragEventArgs e)
        {
            Button button = sender as Button; // 转换为按钮
            if (button.Name == type)
                button.AllowDrop = false; // 不允许拖拽
            else
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9E9E9")); // 设置按钮背景色

            var targetSceneData = db3.GetSceneData(button.Name).FirstOrDefault(); // 获取目标场景数据
            if (targetSceneData.SceneCount == 10)
            {
                button.AllowDrop = false; // 不允许拖拽
            }
        }

        // 还原背景色
        private void ResetSceneButtonBackground(object sender, DragEventArgs e)
        {
            Button button = sender as Button; // 转换为按钮
            if (button.Name != type)
            {
                button.Background = System.Windows.Media.Brushes.Transparent; // 还原背景色
                var targetSceneData = db3.GetSceneData(button.Name).FirstOrDefault(); // 添加场景数量检查，只有未达到上限时才允许拖拽
                if (targetSceneData.SceneCount < 10)
                {
                    button.AllowDrop = true; // 允许拖拽
                }
            }
        }

        // 设置场景标题
        private void SetSceneTitle()
        {
            SceneTitleStackPanel.Children.Clear(); // 清空标题StackPanel
            var sceneInfo = db3.GetSceneData(type).FirstOrDefault(); // 获取场景信息
            SceneImage.Source = new BitmapImage(new Uri(sceneInfo.SceneIconPath, UriKind.Relative)); // 设置场景图片
            string sceneTitleText = db3.GetSceneTitle(sceneInfo); // 获取场景标题
            TextBlock sceneTitle = new()
            {
                Style = FindResource("SceneTitleTextBlock") as Style, // 应用样式
                Text = sceneTitleText // 场景名称
            }; // 创建场景标题
            SceneTitleStackPanel.Children.Add(sceneTitle); // 添加到标题StackPanel
            TextBlock sceneDescription = new()
            {
                Style = FindResource("SceneDescriptionTextBlock") as Style, // 应用样式
                Text = sceneInfo.SceneTag // 场景标签
            }; // 创建场景描述
            SceneTitleStackPanel.Children.Add(sceneDescription); // 添加到标题StackPanel
        }

        /// <summary>
        /// 加载动作页
        /// </summary>
        /// <param name="style">动作页样式</param>
        private void LoadCanvas(string style)
        {
            MainListView.Items.Clear(); // 清空总列表视图
            UpdateUI(); // 更新UI布局
            if (!db2.TableExists(style)) return; // 如果不存在按钮数据表，则返回
            var actionPageData = db3.GetSceneData(style).FirstOrDefault(); // 获取动作页数据
            for (int i = 0; i < actionPageData.SceneCount; i++)
            {
                MainListView.Items.Add(GenerateCanvas(i, style)); // 生成动作页
            }
        }

        // 更新UI布局
        public void UpdateUI()
        {
            switch (type)
            {
                case "Global":
                    MainBorder.Height = 224; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 264, 0, 0); // 设置添加动作页按钮边距
                    AutoReturnToFirstPageCheckBox.Margin = new Thickness(535, 268, 0, 0); // 设置自动返回到第一页复选框边距
                    break; // 全局动作页
                case "Common":
                    MainBorder.Height = 289; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 329, 0, 0); // 设置添加动作页按钮边距
                    AutoReturnToFirstPageCheckBox.Margin = new Thickness(535, 333, 0, 0); // 设置自动返回到第一页复选框边距
                    break; // 普通动作页
                default:
                    if (db2.TableExists(type)) // 如果存在通用样式按钮数据表
                    {
                        MainBorder.Height = 289; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 329, 0, 0); // 设置添加动作页按钮边距
                        AutoReturnToFirstPageCheckBox.Margin = new Thickness(535, 333, 0, 0); // 设置自动返回到第一页复选框边距
                    }
                    else
                    {
                        MainBorder.Height = 224; // 设置主边框高度
                        ScrollBar.Margin = new Thickness(239, 250, 10, 0); // 设置滚动条边距
                        AddActionPageButton.Margin = new Thickness(239, 264, 0, 0); // 设置添加动作页按钮边距
                        AutoReturnToFirstPageCheckBox.Margin = new Thickness(535, 268, 0, 0); // 设置自动返回到第一页复选框边距
                    }
                    break;
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

            Button editPageButton = GenerateEditActionPageButton(canvasIndex, style); // 生成编辑动作页按钮
            grid.Children.Add(editPageButton);

            TextBlock actionPageName = GenerateActionPageName(canvasIndex); // 生成动作页名称
            grid.Children.Add(actionPageName);
    
            int rows = style == "Global" ? 3 : 4; // 行数
            int cols = 4; // 列数
            UniformGrid uniformGrid = new()
            {
                Margin = new Thickness(0, 20, 0, 0),
                Columns = cols,
                Rows = rows,
            };
            dynamicCanvas.Children.Add(uniformGrid);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    string buttonName = $"{style}{canvasIndex}{row + 1}{col + 1}"; // 按钮名称
                    Button button = new Button // 创建按钮
                    {
                        Style = FindResource("ActionButton") as Style, // 按钮样式
                        Name = buttonName // 按钮名称
                    }; // 创建按钮
                    BindButtonEvents(button); // 绑定按钮事件
                    uniformGrid.Children.Add(button); // 将按钮添加到画布

                    var data = db2.GetButtonDataByID(int.Parse(buttonName.Replace(style, "")), type); // 获取按钮数据
                    buttonManager.RefreshButtonDisplay(button, data, 60, false); // 刷新按钮显示

                    if (button.Tag is ButtonData) // 如果按钮有数据
                        button.Background = isDarkModle
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
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
                Style = FindResource("ActionPageCanvas") as Style,
                Height = style == "Global" ? 215 : 280, // 画布高度
                Name = canvasName, // 画布名称
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
            Grid grid = new Grid { Style = FindResource("ActionPageTitleGrid") as Style }; // 创建网格
            return grid; // 返回网格
        }

        /// <summary>
        /// 生成编辑标题按钮
        /// </summary>
        /// <param name="canvasIndex"></param>
        /// <param name="style"></param>
        /// <returns> 编辑标题按钮 </returns>
        private Button GenerateChangePageButton(int canvasIndex, string style)
        {
            Button pageButton = new Button
            {
                Style = FindResource("ActionChangePageButton") as Style,
                Name = $"{style}{canvasIndex}", // 按钮名称
            }; // 创建按钮
            pageButton.PreviewMouseMove += ChangePageButton_PreviewMouseMove; // 鼠标移动事件
            pageButton.PreviewMouseLeftButtonUp += buttonManager.Button_PreviewMouseLeftButtonUp; // 鼠标左键抬起事件
            pageButton.PreviewMouseLeftButtonDown += buttonManager.Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
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
                Style = FindResource("ActionPageEditButton") as Style,
                Name = $"Edit{style}{canvasIndex}", // 按钮名称
            }; // 创建按钮
            editPageButton.Click += OpenEditPopup; // 点击事件
            editPageButton.MouseDoubleClick += EditActionPageInfoButton_Click; // 双击事件
            return editPageButton; // 返回按钮
        }

        /// <summary>
        /// 生成动作页名称
        /// </summary>
        /// <param name="actionPageIndex"> 动作页索引 </param>
        /// <returns> 动作页名称 </returns>
        private TextBlock GenerateActionPageName(int actionPageIndex)
        {
            var actionPageInfo = db3.GetActionPageData(type, actionPageIndex); // 获取动作页信息
            TextBlock textBlock = new TextBlock
            {
                Style = FindResource("ActionPageNameTextBlock") as Style,
                Text = actionPageInfo.ActionPageName // 动作页名称
            }; // 创建文本块
            return textBlock; // 返回文本块
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
                button.Background = isDarkModle
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD")); // 设置按钮背景颜色

                if (showUsedTimes) // 如果显示使用次数
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
                if(showUsedTimes) // 如果显示使用次数
                    buttonManager.RefreshButtonDisplay(button, data, 60, false); // 刷新按钮显示
                button.Background = isDarkModle
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
            }
            else
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 设置按钮背景颜色
        }

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        /// <param name="button"> 按钮 </param>
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
                buttonManager.Button_Drop(sender, e, false, type); // 处理拖放事件
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
                buttonManager.Button_PreviewMouseMove(sender, e, true, type); // 处理鼠标移动事件
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
                buttonManager.OpenMenu(sender, false, "CreatActionMenu", this, type); // 打开创建动作菜单
        }

        // 显示编辑窗口
        private void ShowEditWindow(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // 阻止默认双击事件
            if (sender is Button button && button.Tag != null)
            {
                AddWindow addWindow = new AddWindow(int.Parse(button.Name.Replace(type, "")), type, 0); // 创建编辑窗口
                addWindow.Show(); // 显示编辑窗口
                addWindow.Activate(); // 激活编辑窗口
            }
        }

        // 添加动作页
        private void AddActionPage(object sender, RoutedEventArgs e)
        {
            int canvasCount = MainListView.Items.Count; // 获取画布索引
            if (canvasCount == 10) // 如果画布索引等于9
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("当前场景动作页数量已达上限。", "Error"); // 弹出消息提醒
            }
            else if (canvasCount == 0)
            {
                db2.CreateButtonTable(type); // 创建按钮数据表
                db3.CreateAndInitTable(type, "", ""); // 创建场景数据表
                MainListView.Items.Add(GenerateCanvas(canvasCount, type)); // 如果画布索引为0，则生成画布
                if(type != "Global")
                {
                    MainBorder.Height = 289; // 设置主边框高度
                    ScrollBar.Margin = new Thickness(239, 315, 10, 0); // 设置滚动条边距
                    AddActionPageButton.Margin = new Thickness(239, 337, 0, 0); // 设置添加场景按钮边距
                    AutoReturnToFirstPageCheckBox.Margin = new Thickness(535, 341, 0, 0); // 设置自动返回到第一页复选框边距
                }
            }
            else
            {
                db3.UpdateSceneCount(type, canvasCount + 1); // 更新场景数据表
                var actionPageInfo = GetActionPageInfo(); // 获取动作页信息
                db3.UpdateActionPageTable(type, type + canvasCount.ToString(), actionPageInfo.ActionPageName); // 更新动作页数据表
                MainListView.Items.Add(GenerateCanvas(canvasCount, type)); // 生成画布
            }
        }

        /// <summary>
        /// 设置动作页信息
        /// </summary>
        /// <returns> 动作页信息 </returns>
        private ActionPageInfo GetActionPageInfo()
        {
            string actionPageProcess = "", actionPageName = ""; // 设置动作页信息
            switch (type)
            {
                case "Global":
                    actionPageProcess = "Default"; // 设置动作页所属应用程序名称
                    actionPageName = "默认全局动作页"; // 设置动作页名称
                    break;
                case "Common":
                    actionPageProcess = "Default"; // 设置动作页所属应用程序名称
                    actionPageName = "默认"; // 设置动作页名称
                    break;
                case "Desktop":
                    actionPageProcess = "Windows桌面"; // 设置动作页所属应用程序名称
                    actionPageName = $"桌面 #{MainListView.Items.Count}"; // 设置动作页名称
                    break;
                case "Taskbar":
                    actionPageProcess = "Windows任务栏"; // 设置动作页所属应用程序名称
                    actionPageName = $"任务栏 #{MainListView.Items.Count}"; // 设置动作页名称
                    break;
                default:
                    break;
            }
            ActionPageInfo actionPageInfo = new ActionPageInfo
            {
                ActionPageProcess = actionPageProcess,
                ActionPageName = actionPageName,
            }; // 创建动作页信息对象
            return actionPageInfo; // 返回动作页信息对象
        }

        // 滚动条滚动事件
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollBar.Maximum = ScrollViewer.ExtentWidth - ScrollViewer.ViewportWidth; // 设置滚动条最大值
            ScrollBar.ViewportSize = ScrollViewer.ViewportWidth; // 设置滚动条视口大小
            ScrollBar.Value = ScrollViewer.HorizontalOffset; // 设置滚动条值
        }

        // 点击按钮切换场景类型
        private void ChanceSceneButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // 获取按钮
            var sceneInfo = db3.GetSceneData(button.Name).FirstOrDefault(); // 获取场景信息
            TypeChanged(sceneInfo.SceneName); // 切换类型为全局场景
        }

        // 打开创建动作菜单
        private void OpenMenu(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // 阻止默认右键菜单
            Button button = sender as Button; // 获取按钮
            if (button.Tag is ButtonData data && button != null)
                buttonManager.OpenMenu(sender, false, "OperationMenu", this, type); // 打开操作菜单
            else
                buttonManager.OpenMenu(sender, false, "CreatActionMenu", this, type); // 打开创建动作菜单
        }

        /// <summary>
        /// 编辑动作后刷新按钮显示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        public void UpdateButton(int button)
        {
            int index = button / 100; // 获取按钮所在动作页索引
            var oldCanvas = MainListView.Items[index] as Canvas; // 获取旧的 Canvas
            UniformGrid targetGrid = oldCanvas.Children.Cast<UIElement>().Where(c => c is UniformGrid).First() as UniformGrid; // 获取目标网格
            foreach(var childs in targetGrid.Children)
            {
                if(childs is Button targetButton && targetButton.Name.Contains(button.ToString()))
                {
                    ButtonData data = db2.GetButtonDataByID(int.Parse(targetButton.Name.Replace(type, "")), type); // 获取按钮数据
                    buttonManager.RefreshButtonDisplay(targetButton, data, 60, false); // 刷新按钮显示
                }
            }
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void ChangePageButton_PreviewMouseMove(object sender, MouseEventArgs e)
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
                    db3.SwapActionPage(type, sourceIndex, targetIndex); // 更新数据库动作页数据
                    UpdateCanvasInListView(sourceIndex, type); // 更新 ListView 中的特定 Canvas
                    UpdateCanvasInListView(targetIndex, type); // 更新 ListView 中的特定 Canvas
                }
            }
        }

        /// <summary>
        /// 更新 ListView 中的特定 Canvas
        /// </summary>
        /// <param name="canvasIndex"> 画布索引 </param>
        /// <param name="styleType"> 场景类型 </param>
        public void UpdateCanvasInListView(int canvasIndex, string styleType)
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
                GenerateSceneButtons(); // 如果文本为空，则加载默认按钮前缀
        }

        // 双击标签切换动作按钮背景色
        private void ChangeActionButtonBakground(object sender, MouseButtonEventArgs e)
        {
            isDarkModle = !isDarkModle; // 切换模式
            foreach (var item in MainListView.Items)
            {
                Canvas canvas = item as Canvas; // 获取画布
                UniformGrid targetGrid = canvas.Children.Cast<UIElement>().Where(c => c is UniformGrid).First() as UniformGrid; // 获取目标网格
                foreach (var childs in targetGrid.Children)
                {
                    if (childs is Button button && button.Tag is ButtonData data)
                    {
                        button.Background = isDarkModle
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 设置按钮背景颜色
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
            EditActionPagePopup.IsOpen = false; // 关闭编辑动作页弹出菜单
            return EditActionPagePopup.PlacementTarget as Button; // 获取弹出菜单所绑定按钮
        }

        // 点击按钮查看动作页信息
        private void CheckActionPageInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            int actionPageIndex = int.Parse(bingdingButton.Name.Replace($"Edit{type}", "")); // 获取动作页 ID
            ActionPageData actionPageData = db3.GetActionPageData(type, actionPageIndex); // 获取动作页信息
            string actionPageSize = db3.GetActionPageSize(type, actionPageIndex); // 获取动作页大小
            using var convertion = new DataConversionManager(); // 数据转换管理器
            string actionPageInfo = $"ID：{actionPageData.DefaultActionPageName}\n" +
                                    $"最后修改：{actionPageData.LastEditTime}\n" +
                                    $"大小：{actionPageSize}"; // 创建消息窗口
            MessageWindow messageWindow = new("动作页信息", actionPageInfo); // 创建消息窗口
            messageWindow.ShowDialog(); // 显示消息窗口
        }

        // 点击按钮复制动作页 ID
        private void CopyActionPageIDButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            Clipboard.SetText(bingdingButton.Name.Replace("Edit", "")); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show($"动作页ID已复制到剪贴板：{bingdingButton.Name.Replace("Edit", "")}", "Common"); // 弹出消息提醒
        }

        // 点击按钮编辑动作页信息
        private void EditActionPageInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            switch (type)
            {
                case "Global":
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.Show("默认全局动作页信息不可修改。", "Common"); // 弹出消息提醒
                    }
                    break;
                case "Common":
                    {
                        using var toast = new ToastManager(); // 消息提醒管理器
                        toast.Show("默认通用动作页信息不可修改。", "Common"); // 弹出消息提醒
                    }
                    break;
                default:
                    string canvasIndex = bingdingButton.Name.Replace("Edit" + type, ""); // 获取画布索引
                    EditActionPageInfoWindow editActionPageInfoWindow = new(type, canvasIndex); // 创建编辑动作页信息窗口
                    editActionPageInfoWindow.ShowDialog(); // 显示编辑动作页信息窗口
                    break;
            }
        }

        // 点击按钮创建打开动作页动作
        private void CreatOpenActionPageActionButton_Click(object sender, RoutedEventArgs e)
        {
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            string actionPageIndex = bingdingButton.Name.Replace("Edit" + type, ""); // 获取场景名称
            string openActionPageCommand = $"OpenActionPage;{type};{actionPageIndex};QuickerCommand"; // 生成打开动作页指令
            Clipboard.SetText(openActionPageCommand); // 复制文本到剪贴板
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show("已创建动作并写入剪贴板，请粘贴到合适位置。", "Common"); // 弹出消息提醒
        }

        // 点击按钮删除动作页
        private void DeleteActionPageButton_Click(object sender, RoutedEventArgs e)
        {
            EditActionPagePopup.IsOpen = false; // 关闭编辑动作页弹出菜单
            Button bingdingButton = GetBingdingButton(); // 获取绑定按钮
            // 移除画布
            Grid targetGrid = bingdingButton.Parent as Grid; // 获取目标画布
            MainListView.Items.Remove(targetGrid.Parent); // 从主列表视图中移除画布
            // 删除按钮数据页
            string targetButtonName = bingdingButton.Name.Replace("Edit", ""); // 获取目标按钮名称
            int canvadIndex = int.Parse(targetButtonName.Replace(type, "")); // 获取画布索引
            db2.DeletePageOfButtons(type, canvadIndex); // 删除按钮数据页
            db3.DeleteActionPage(type, canvadIndex); // 删除动作页数据表
            if(MainListView.Items.Count == 0)
            {
                db2.DeleteButtonTable(type); // 如果没有画布，则删除按钮数据表
                db3.DeleteActionPageTable(type); // 删除动作页数据表
                if (!new List<string> { "Global", "Common", "Taskbar", "Desktop" }.Contains(type)) // 如果不是默认场景
                    db3.DeleteSceneTable(type); // 删除场景数据表
            }
            LoadCanvas(type); // 刷新界面
        }

        // 点击按钮添加场景
        private void AddSceneButton_Click(object sender, RoutedEventArgs e)
        {
            using var toast = new ToastManager(); // 消息提醒管理器
            toast.Show("功能开发中。", "Common"); // 弹出消息提醒
        }

        // 点击按钮编辑场景
        private void EditSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (new List<string> { "Global", "Common", "Taskbar", "Desktop" }.Contains(type)) // 如果是默认场景
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("此项不可编辑。", "Common"); // 弹出消息提醒
            }
            else
            {
                string canvasIndex = GetBingdingButton().Name.Replace("Edit" + type, ""); // 获取画布索引
                EditSceneWindow editSceneWindow = new(type); // 创建编辑场景窗口
                editSceneWindow.Show(); // 显示编辑场景窗口
            }
        }

        // 点击按钮删除场景
        private void DeleteSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (new List<string> { "Global", "Common", "Taskbar", "Desktop" }.Contains(type)) // 如果是默认场景
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show("此项不可删除。", "Common"); // 弹出消息提醒
            }
            else
            {

            }
        }

        // 点击按钮前往顶层场景
        private void ToTopSceneButton_Click(object sender, RoutedEventArgs e)
        {
            TypeChanged("Global"); // 切换类型为全局场景
        }

        // 右键显示/隐藏动作使用次数
        private void AddActionPageButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            showUsedTimes = !showUsedTimes; // 切换显示/隐藏使用次数
        }

        // 滚动滚轮移动视图
        private void MainStackPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // 标记事件为已处理，防止默认行为
            int delta = e.Delta < 0 ? 30 : -30; // 获取滚轮滚动方向
            ScrollBar.Value += delta; // 调整滚动条值
        }

        // 点击勾选框更改设置
        private void AutoReturnToFirstPageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            db3.SetAutoReturnToFirstPage(type, AutoReturnToFirstPageCheckBox.IsChecked == true); // 更改设置
        }

        // 将动作页拖拽按钮拖放到其他场景按钮上给动作页切换场景
        private void ChangeActionPageScene(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ButtonData"))
            {
                Button targetButton = sender as Button; // 获取目标按钮
                var targetSceneData = db3.GetSceneData(targetButton.Name).FirstOrDefault(); // 获取目标场景数据
                string sourceButtonName = e.Data.GetData("ButtonData")?.ToString(); // 获取传递的 Button Name
                if (!string.IsNullOrEmpty(sourceButtonName))
                {
                    int sourceIndex = int.Parse(sourceButtonName.Replace(type, "")); // 获取源动作页索引
                    if (targetSceneData.SceneCount == 0)
                    {
                        db2.CreateButtonTable(targetSceneData.SceneName); // 创建按钮数据表
                        db3.CreateActionPageTable(targetSceneData.SceneName); // 创建动作页数据表
                    }
                    var buttons = db2.GetPagesOfButtons(type, sourceIndex); // 获取源动作页按钮数据
                    foreach (var button in buttons)
                    {
                        ButtonData newButtonData = new()
                        {
                            ButtonID = targetSceneData.SceneCount * 100 + button.ButtonID % 100,
                            Title = button.Title,
                            Location = button.Location,
                            ImagePath = button.ImagePath,
                            Data1 = button.Data1,
                            Data2 = button.Data2,
                            Data3 = button.Data3,
                            Description = button.Description,
                            CreateTime = button.CreateTime,
                            LatestEditTime = DateTime.Now,
                            ActionType = button.ActionType,
                            UsedTimes = button.UsedTimes
                        }; // 创建新按钮数据
                        db2.UpdateAction(newButtonData, targetSceneData.SceneName); // 更新按钮数据
                    }
                    var sourceActionPageData = db3.GetActionPageData(type, sourceIndex); // 获取源动作页数据
                    db3.UpdateActionPageTable(targetSceneData.SceneName, targetSceneData.SceneName + targetSceneData.SceneCount.ToString(), GetActionPageName(targetSceneData.SceneName, sourceActionPageData.ActionPageName)); // 更新动作页数据表
                    db3.UpdateSceneCount(targetSceneData.SceneName, targetSceneData.SceneCount + 1); // 更新场景数据表
                    db3.DeleteActionPage(type, sourceIndex); // 删除源动作页
                    db2.DeletePageOfButtons(type, sourceIndex); // 删除源动作页按钮数据
                    MainListView.Items.RemoveAt(sourceIndex); // 从主列表视图中移除画布

                    if (MainListView.Items.Count == 0)
                    {
                        db2.DeleteButtonTable(type); // 如果没有画布，则删除按钮数据表
                        db3.DeleteActionPageTable(type); // 删除动作页数据表
                        if (!new List<string> { "Global", "Common", "Taskbar", "Desktop" }.Contains(type)) // 如果不是默认场景
                            db3.DeleteSceneTable(type); // 删除场景数据表
                    }
                    TypeChanged(targetSceneData.SceneName); // 刷新界面
                }
            }
        }

        /// <summary>
        /// 获取动作页名称
        /// </summary>
        /// <param name="sceneType"> 场景类型 </param>
        /// <param name="actionPageName"> 动作页名称 </param>
        /// <returns> 动作页名称 </returns>
        private string GetActionPageName(string sceneType, string actionPageName)
        {
            switch (sceneType)
            {
                case "Global":
                    return "默认全局动作页";
                case "Common":
                    return "默认";
                default:
                    return actionPageName;
            }
        }

        // 关闭窗口时释放资源
        protected override void OnClosed(EventArgs e)
        {
            type = null; // 清理场景类型
            isDarkModle = false; // 清理是否为暗黑模式
            showUsedTimes = false; // 清理是否显示使用次数
            CleanupMainListView(); // 清理主列表视图
            CleanupActionPagesButtonPanel(); // 清理场景按钮面板
            buttonManager?.Dispose(); // 清理按钮管理器

            base.OnClosed(e); // 调用基类的 OnClosed 方法
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }

        // 清理主列表视图
        private void CleanupMainListView()
        {
            while (MainListView.Items.Count > 0)
            {
                if (MainListView.Items[0] is Canvas canvas)
                {
                    CleanupCanvas(canvas); // 清理画布
                    MainListView.Items.Remove(canvas); // 移除画布
                }
            }
        }

        // 清理画布
        private void CleanupCanvas(Canvas canvas)
        {
            while (canvas.Children.Count > 0)
            {
                UIElement child = canvas.Children[0];
                if (child is Grid grid)
                {
                    CleanupGrid(grid); // 清理网格
                }
                else if (child is Button button)
                {
                    CleanupButton(button); // 清理按钮
                }
                canvas.Children.Remove(child); // 移除画布子元素
            }
        }

        /// <summary>
        /// 清理网格
        /// </summary>
        /// <param name="grid"> 网格 </param>
        private void CleanupGrid(Grid grid)
        {
            while (grid.Children.Count > 0)
            {
                UIElement gridChild = grid.Children[0];
                if (gridChild is Button button)
                {
                    CleanupButton(button); // 清理按钮
                }
                grid.Children.Remove(gridChild); // 移除网格子元素
            }
        }

        /// <summary>
        /// 清理按钮
        /// </summary>
        /// <param name="button"> 按钮 </param>
        private void CleanupButton(Button button)
        {
            // 解绑所有事件
            button.Drop -= Button_Drop; // 解绑按钮拖拽事件
            button.Click -= ShowCreatActionMenu; // 解绑按钮点击事件
            button.MouseEnter -= Button_MouseEnter; // 解绑按钮鼠标进入事件
            button.MouseLeave -= Button_MouseLeave; // 解绑按钮鼠标离开事件
            button.MouseDoubleClick -= ShowEditWindow; // 解绑按钮鼠标双击事件
            button.PreviewMouseRightButtonDown -= OpenMenu; // 解绑按钮鼠标右键点击事件
            button.PreviewMouseMove -= Button_PreviewMouseMove; // 解绑按钮鼠标移动事件
            button.PreviewMouseLeftButtonUp -= Button_PreviewMouseLeftButtonUp; // 解绑按钮鼠标左键释放事件
            button.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown; // 解绑按钮鼠标左键按下事件

            // 清理按钮资源
            button.Content = null; // 清理按钮内容
            button.Tag = null; // 清理按钮标签
            button.Background = null; // 清理按钮背景
            button.Style = null; // 清理按钮样式
        }

        // 清理场景按钮面板
        private void CleanupActionPagesButtonPanel()
        {
            while (ActionPagesButtonPanel.Children.Count > 0)
            {
                UIElement child = ActionPagesButtonPanel.Children[0];
                if (child is Button button)
                {
                    // 解绑事件
                    button.Click -= ChanceSceneButton_Click; // 解绑按钮点击事件
                    button.MouseEnter -= HightLightBlacklistItem; // 解绑按钮鼠标进入事件
                    button.MouseLeave -= FadeBlacklistItem; // 解绑按钮鼠标离开事件

                    // 清理按钮资源
                    button.Content = null; // 清理按钮内容
                    button.Tag = null; // 清理按钮标签
                    button.Background = null; // 清理按钮背景
                    button.Style = null; // 清理按钮样式
                }
                ActionPagesButtonPanel.Children.Remove(child); // 移除场景按钮面板子元素
            }
        }
    }

    // 动作页信息类
    public class ActionPageInfo
    {
        public string ActionPageProcess { get; set; } // 动作页所属应用程序名称
        public string ActionPageName { get; set; } // 动作页名称
    }
}