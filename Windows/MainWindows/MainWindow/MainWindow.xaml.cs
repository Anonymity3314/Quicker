using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using VisualTreeHelper = Quicker.Helpers.VisualTreeHelper;
using DragEventArgs = System.Windows.DragEventArgs;
using Button = System.Windows.Controls.Button;
using Panel = System.Windows.Controls.Panel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Quicker.Windows.ToolWindows;
using Quicker.Windows.AddWindows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Quicker.Windows.Menus;
using Quicker.Database.Core;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Forms;
using Quicker.Managers;
using Quicker.Helpers;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.Windows.MainWindows.MainWindow
{
    public partial class MainWindow : Window
    {
        // 常量定义
        private const string DEFAULT_STYLE = "common";
        private const string GLOBAL_STYLE = "_global";

        private SolidColorBrush SelectedBrush => (this.DataContext as MainWindowViewModel).SelectedBrush;
        private SolidColorBrush UnSelectedBrush => (this.DataContext as MainWindowViewModel).UnSelectedBrush;

        private readonly CancellationTokenSource cancellationTokenSource = new(); // 取消后台任务的令牌源
        public readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly IconManager iconManager = new(); // 图标管理器
        private readonly ActionPageDatabase db3 = new(); // 动作页面数据库
        int GloblePageIndex = 0, CommonPageIndex = 0; // 全局页面、通用页面索引
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private string CommonStyle = "common"; // 样式

        public MainWindow(string style)
        {
            CommonStyle = style ?? DEFAULT_STYLE; // 添加空值检查
            InitializeComponent(); // 初始化窗口组件
            this.DataContext = new MainWindowViewModel();
            var vm = this.DataContext as MainWindowViewModel; // 只初始化时设置一次背景图片
            iconManager.SetImageWithGifSupport(BackgroundImage, vm.BackgroundImagePath); // 设置背景图片
        }

        /// <summary>
        /// 通用类型改变事件
        /// </summary>
        /// <param name="style"> 样式名称 </param>
        public void OnCommonStyleChanged(string style)
        {
            CommonStyle = style ?? DEFAULT_STYLE; // 添加空值检查
            CommonGrid.Children.Clear(); // 清空通用网格
            SetCommonTextBlock(0); // 设置通用标签内容
            GeneratePageGrid(CommonGrid, CommonStyle, 0, 4, 4); // 生成对应样式 Grid
            GenerateButtons(); // 生成按钮

            // 固定锁定状态为 true
            AppStateManager.Locked = true;
            ((MainWindowViewModel)this.DataContext).IsLocked = true;
        }

        // 加载数据库和Button
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePageGrids(); // 初始化所有页
            CheckUpdate(); // 检查更新
            SetMainWindowState(); // 设置窗口状态
            this.Activate(); // 激活窗口
        }

        // 初始化动作页面
        private void InitializePageGrids()
        {
            var targetStyle = db2.TableExists(CommonStyle) ? CommonStyle : DEFAULT_STYLE; // 设置样式
            CommonStyle = targetStyle; // 更新样式
            GeneratePageGrid(GlobalGrid, GLOBAL_STYLE, 0, 3, 4);
            GeneratePageGrid(CommonGrid, targetStyle, 0, 4, 4);
            GenerateButtons(); // 生成按钮
            SetCommonTextBlock(0);
        }

        // 检查更新
        private void CheckUpdate()
        {
            if (!AppStateManager.HasNewVersion) // 如果没有新版本
            {
                HasNewVersionTip.Visibility = Visibility.Collapsed; // 隐藏提示
                TitlePop.Height -= 25; // 减少高度
                UpdateButton.Visibility = Visibility.Collapsed; // 隐藏更新按钮
            }
        }

        /// <summary>
        /// 设置通用标签内容
        /// </summary>
        /// <param name="GridIndex"> 通用网格索引 </param>
        private void SetCommonTextBlock(int GridIndex)
        {
            var actionPageData = db3.GetActionPageData(CommonStyle, GridIndex); // 从数据库中获取通用动作页面数据
            CommonTextBlock.Text = actionPageData.ActionPageName; // 设置通用标签内容
            CommonTextBlock.ToolTip = actionPageData.ActionPageName; // 设置通用标签提示
        }

        // 设置窗口状态
        private void SetMainWindowState()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowTopmost(this);// 设置窗口置顶
        }

        /// <summary>
        /// 将窗口定位到鼠标位置，使CenterPointGrid位于鼠标处
        /// </summary>
        public void PositionWindowAtMouse()
        {
            var mousePos = System.Windows.Forms.Cursor.Position; // 获取鼠标位置
            var centerPointScreen = CenterPointGrid.PointToScreen(new Point(0, 0)); // 获取CenterPointGrid在屏幕上的绝对位置

            // 计算窗口需要移动的偏移量
            double offsetX = mousePos.X - centerPointScreen.X;
            double offsetY = mousePos.Y - centerPointScreen.Y;

            // 调整窗口位置
            this.Left += offsetX;
            this.Top += offsetY;
        }

        // 生成页面切换 Button
        private void GenerateButtons()
        {
            // 切换通用动作页时清空
            GlobalButtonPanel.Children.Clear(); // 清空全局动作页切换按钮
            CommonButtonPanel.Children.Clear(); // 清空通用动作页切换按钮
            
            // 使用缓存获取场景数据，减少数据库查询
            var globalSceneData = db3.GetSceneData(GLOBAL_STYLE);
            var commonSceneData = db3.GetSceneData(CommonStyle);
            
            // 生成页面切换按钮，保留当前选中的按钮状态
            GeneratePageButtons(GLOBAL_STYLE, globalSceneData?.SceneCount ?? 1, GlobalPageChangeButton_Click, GlobalButtonPanel, GloblePageIndex);
            GeneratePageButtons(CommonStyle, commonSceneData?.SceneCount ?? 1, CommonPageChangeButton_Click, CommonButtonPanel, CommonPageIndex);
        }

        /// <summary>
        /// 生成页面切换按钮
        /// </summary>
        /// <param name="prefix"> 前缀 </param>
        /// <param name="totalPages"> 总页数 </param>
        /// <param name="clickHandler"> 点击事件 </param>
        /// <param name="panel"> 按钮容器 </param>
        /// <param name="selectedIndex"> 当前选中的页面索引 </param>
        private void GeneratePageButtons(string prefix, int totalPages, RoutedEventHandler clickHandler, Panel panel, int selectedIndex = 0)
        {
            if (totalPages == 1) return;
            for (int i = 0; i < totalPages; i++)
            {
                Button button = new Button
                {
                    Style = FindResource("ActionPageChangeButton") as Style,
                    Name = $"{prefix}{i}"
                };
                button.Background = (i == selectedIndex) ? SelectedBrush : UnSelectedBrush; // 根据 selectedIndex 设置按钮背景色，保留当前选中状态
                button.Click += clickHandler;
                panel.Children.Add(button);
            }
        }

        // 全局页面切换按钮点击
        private void GlobalPageChangeButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPageGrid(sender, GlobalGrid, GLOBAL_STYLE, 3, 4, GlobalButtonPanel);
        }
        // 通用页面切换按钮点击
        private void CommonPageChangeButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPageGrid(sender, CommonGrid, CommonStyle, 4, 4, CommonButtonPanel);
        }

        /// <summary>
        /// 显示目标页面并同步相关状态（提取的公共逻辑）
        /// </summary>
        /// <param name="parent">父容器</param>
        /// <param name="gridType">Grid类型</param>
        /// <param name="targetIndex">目标页面索引</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="buttonPanel">按钮面板</param>
        private void ShowTargetPage(Panel parent, string gridType, int targetIndex, int rows, int cols, Panel buttonPanel)
        {
            if (!parent.Children.OfType<Grid>().Any(g => g.Name == $"{gridType}{targetIndex}")) // 如果目标页面不存在
                GeneratePageGrid(parent, gridType, targetIndex, rows, cols); // 生成目标页面
            ShowPageGrid(parent, gridType, targetIndex, rows, cols); // 显示目标页面
            foreach (Button btn in buttonPanel.Children.OfType<Button>()) // 同步按钮背景色
                btn.Background = btn.Name == $"{gridType}{targetIndex}" ? SelectedBrush : UnSelectedBrush;
            if (gridType != GLOBAL_STYLE) SetCommonTextBlock(targetIndex); // 设置通用标签内容
            if (gridType == GLOBAL_STYLE) GloblePageIndex = targetIndex; else CommonPageIndex = targetIndex; // 同步页面索引
        }

        /// <summary>
        /// 切换到指定的页面Grid
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="parent">父容器</param>
        /// <param name="gridType">Grid类型</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="buttonPanel">按钮面板</param>
        private void SwitchToPageGrid(object sender, Panel parent, string gridType, int rows, int cols, Panel buttonPanel)
        {
            if (sender is Button clickedButton)
            {
                int pageIndex = int.Parse(clickedButton.Name.Replace($"{gridType}", ""));
                ShowTargetPage(parent, gridType, pageIndex, rows, cols, buttonPanel); // 使用公共方法
            }
        }

        /// <summary>
        /// 获取当前可见的页面Grid索引
        /// </summary>
        /// <param name="parent">父容器</param>
        /// <param name="gridType">Grid类型</param>
        /// <returns>当前可见的页面Grid索引</returns>
        private int GetVisiblePageGridIndex(Panel parent, string gridType)
        {
            foreach (Grid grid in parent.Children.OfType<Grid>())
            {
                if (grid.Visibility == Visibility.Visible && grid.Name.StartsWith(gridType))
                    return int.Parse(grid.Name.Replace(gridType, ""));
            }
            return 0;
        }

        /// <summary>
        /// 处理页面索引边界检查和循环翻页逻辑
        /// </summary>
        /// <param name="targetIndex">目标页面索引</param>
        /// <param name="totalPages">总页数</param>
        /// <param name="loop">是否循环翻页</param>
        /// <returns>处理后的页面索引，如果应该退出则返回 null</returns>
        private int? ValidateAndAdjustPageIndex(int targetIndex, int totalPages, bool loop)
        {
            if (targetIndex < 0) // 超出范围
            {
                if (loop) // 如果循环翻页
                    return totalPages - 1; // 循环翻页到最后一页
                else
                    return null; // 退出
            }
            if (targetIndex >= totalPages) // 超出范围
            {
                if (loop) // 如果循环翻页
                    return 0; // 循环翻页到第一页
                else
                    return null; // 退出
            }
            return targetIndex; // 索引在有效范围内
        }

        /// <summary>
        /// 翻页核心逻辑（根据滚动方向切换到上一页或下一页）
        /// </summary>
        /// <param name="parent">父容器</param>
        /// <param name="gridType">Grid类型</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="buttonPanel">按钮面板</param>
        /// <param name="delta">滚动变化量（正数向上滚动，负数向下滚动）</param>
        private void NavigatePage(Panel parent, string gridType, int rows, int cols, Panel buttonPanel, int delta)
        {
            int currentIndex = GetVisiblePageGridIndex(parent, gridType); // 获取当前可见页面索引
            int totalPages = db3.GetSceneData(gridType)?.SceneCount ?? 1; // 获取总动作页数
            bool loop = SettingDatabase.GetAllConventions().FirstOrDefault()?.LoopPageFlipping ?? true; // 获取是否循环翻页
            int targetIndex = delta > 0 ? currentIndex - 1 : currentIndex + 1;
            
            int? validatedIndex = ValidateAndAdjustPageIndex(targetIndex, totalPages, loop); // 验证并调整页面索引
            if (validatedIndex == null) return; // 如果应该退出，直接返回
            
            ShowTargetPage(parent, gridType, validatedIndex.Value, rows, cols, buttonPanel); // 使用公共方法显示目标页面
        }

        /// <summary>
        /// 鼠标滚轮切换页面
        /// </summary>
        /// <param name="e">鼠标滚轮事件参数</param>
        /// <param name="parent">父容器</param>
        /// <param name="gridType">Grid类型</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="buttonPanel">按钮面板</param>
        private void Grid_MouseWheel(MouseWheelEventArgs e, Panel parent, string gridType, int rows, int cols, Panel buttonPanel)
        {
            e.Handled = true; // 禁止默认滚轮事件
            NavigatePage(parent, gridType, rows, cols, buttonPanel, e.Delta); // 调用翻页核心逻辑
        }

        // 全局Grid滚轮事件
        private void GlobalGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Grid_MouseWheel(e, GlobalGrid, GLOBAL_STYLE, 3, 4, GlobalButtonPanel);
        }
        // 通用Grid滚轮事件
        private void CommonGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Grid_MouseWheel(e, CommonGrid, CommonStyle, 4, 4, CommonButtonPanel);
        }

        // 左键点击按钮时执行动作
        private async void DoAction(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            string buttonType = GetButtonType(sender); // 获取按钮类型
            if (button.Tag is ButtonData data)
            {
                if (!AppStateManager.MainWindowPinned && data.ActionType != ActionType.OpenActionPage) 
                    this.Visibility = Visibility.Collapsed; // 隐藏窗口

                if (HandleShiftKeyAction(button, buttonType)) return; // 处理Shift键动作
                using (var actionManager = new ActionManager())
                {
                    await actionManager.DoActionAsync(data, buttonType);
                }
                HandleAutoReturn(buttonType); // 处理自动返回
            }
            else
            {
                HandleEmptyButtonClick(sender, buttonType); // 处理空按钮点击
            }
        }

        /// <summary>
        /// 处理Shift键动作
        /// </summary>
        /// <param name="button">按钮对象</param>
        /// <param name="buttonType">按钮类型</param>
        /// <returns>是否已处理动作</returns>
        private bool HandleShiftKeyAction(Button button, string buttonType)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                AddActionWindow addWindow = new(int.Parse(button.Name.Replace($"{buttonType}", "")), buttonType, 0); // 创建添加窗口
                addWindow.Show(); // 显示添加窗口
                return true; // 处理Shift键动作
            }
            return false; // 未处理Shift键动作
        }

        /// <summary>
        /// 处理自动返回第一页
        /// </summary>
        /// <param name="buttonType">按钮类型</param>
        private void HandleAutoReturn(string buttonType)
        {
            bool autoReturn = db3.GetAutoReturnToFirstPage(buttonType); // 获取是否自动返回第一页
            if(autoReturn) // 如果自动返回第一页，清空按钮所在容器
            {
                if (buttonType == GLOBAL_STYLE)
                    GlobalGrid.Children.Clear(); // 清空按钮所在容器
                else
                    CommonGrid.Children.Clear(); // 清空按钮所在容器
                GeneratePageGrid(buttonType == GLOBAL_STYLE ? GlobalGrid : CommonGrid, buttonType, 0, 3, 4); // 重新生成第一页内容
            }
        }

        /// <summary>
        /// 处理空按钮点击
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="buttonType">按钮类型</param>
        private void HandleEmptyButtonClick(object sender, string buttonType)
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取配置信息
            if (Convention.ShowAddImage) // 如果显示添加按钮
                buttonManager.OpenMenu(sender, "CreatActionMenu", this, buttonType); // 点击打开菜单
        }

        // 右键按钮打开菜单
        public void OpenCreatActionMenu(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Button button) return;
            string buttonType = GetButtonType(button);

            // 检查动作是否为扩展类型
            if (button.Tag is ButtonData buttonData && buttonData.ActionType == ActionType.LoadExtension)
            {
                if (buttonManager.CheckExtensionHasContextMenu(button))  // 扩展是否支持右键菜单
                {
                    buttonManager.TryLoadExtensionContextMenu(button, buttonType, this);  // 尝试加载扩展菜单
                    return;
                }
            }

            string menuType = button.Tag is ButtonData ? "OperationMenu" : "CreatActionMenu";
            buttonManager.OpenMenu(button, menuType, this, buttonType);  // 使用默认菜单
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
                buttonManager.Button_Drop(sender, e, true, GetButtonType(sender)); // 处理拖拽事件
        }

        // 鼠标左键按下时记录初始位置
        public void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
                buttonManager.Button_PreviewMouseLeftButtonDown(sender, e); // 记录初始位置
        }

        // 鼠标移动时检查是否满足拖拽条件
        public void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
                buttonManager.Button_PreviewMouseMove(sender, e, true, GetButtonType(sender)); // 检查拖拽条件
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
            TitlePop.IsOpen = false; // 关闭菜单
            this.Visibility = Visibility.Collapsed; // 隐藏窗口
            (System.Windows.Application.Current as App)?.Shutdown(); // 通过App类正确关闭应用，确保OnExit方法被调用
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
            TitlePop.IsOpen = false; // 关闭菜单
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("ActionPageManageWindow"); // 打开动作管理窗口
        }

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        /// <param name="button">指定绑定的 Button</param>
        private void BindButtonEvents(Button button)
        {
            button.Click += DoAction; // 左键点击事件
            button.Drop += Button_Drop; // 拖拽事件
            button.PreviewDragOver += Button_PreviewDragOver; // 添加拖拽事件
            button.MouseRightButtonDown += OpenCreatActionMenu; // 右键点击事件
            button.PreviewMouseMove += Button_PreviewMouseMove; // 鼠标移动事件
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp; // 鼠标左键释放事件
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
            button.MouseEnter += Button_MouseEnter; // 鼠标移入事件
            button.MouseLeave += Button_MouseLeave; // 鼠标移出事件
        }

        // 锁定通用动作页
        private void LockCommonActionPage(object sender, RoutedEventArgs e)
        {
            AppStateManager.Locked = !AppStateManager.Locked; // 切换锁定状态
            ((MainWindowViewModel)this.DataContext).IsLocked = AppStateManager.Locked; // 更新数据绑定
        }

        // 点击标签打开动作页管理窗口
        private void CommonTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ActionPageManageWindow actionPageManageWindow = new(CommonStyle); // 创建动作页管理窗口
            actionPageManageWindow.Show(); // 显示窗口
        }

        // 右键锁定 Button 切换菜单
        private void OpenSelectActionPageMenu(object sender, MouseButtonEventArgs e)
        {
            buttonManager.OpenMenu(sender, "SelectActionPageMenu", this, GetButtonType(sender)); // 打开菜单
        }

        /// <summary>
        /// 获取按钮类型
        /// </summary>
        /// <param name="sender"> 按钮对象 </param>
        /// <returns> 按钮类型 </returns>
        private string GetButtonType(object sender)
        {
            Button button = sender as Button; // 获取按钮
            return button.Name.StartsWith(GLOBAL_STYLE) ? GLOBAL_STYLE : CommonStyle; // 获取按钮类型
        }

        // 点击按钮更新Quicker
        private void UpdateQuicker(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new(); // 创建更新窗口
            updateWindow.Show(); // 显示窗口
        }

        /// <summary>
        /// 更新按钮内容
        /// </summary>
        /// <param name="buttonID"> 按钮ID </param>
        /// <param name="tableName"> 数据库表名 </param>
        public void UpdateButtonContent(int buttonID, string tableName)
        {
            Grid fatherGrid = tableName == GLOBAL_STYLE ? GlobalGrid : CommonGrid; // 根据表名选择父网格
            int pageIndex = buttonID / 100; // 计算按钮索引
            string buttonName = $"{tableName}{buttonID}"; // 生成按钮名称
            var button = fatherGrid.Children.OfType<Grid>()
                .Where(ug => ug.Name == $"{tableName}{pageIndex}")
                .SelectMany(ug => ug.Children.OfType<Button>())
                .FirstOrDefault(b => b.Name == buttonName); // 查找按钮

            if (button == null) return; // 如果按钮不存在，直接返回
            var buttonData = db2.GetButtonDataByID(buttonID, tableName); // 从数据库中获取按钮数据
            buttonManager.RefreshButtonDisplay(button, buttonData, 60, true); // 更新按钮内容
        }

        // 拖拽动作按钮到上面删除动作
        private void CloseMainWindowButton_Drop(object sender, DragEventArgs e)
        {
            buttonManager.DeleteActionByDrag(this, CommonStyle); // 删除动作
        }

        /// <summary>
        /// 鼠标左键拖动窗口
        /// </summary>
        private void MoveMainWindow(object sender, EventArgs e)
        {
            DragMove(); // 触发窗口拖动
        }

        /// <summary>
        /// 打开设置窗口
        /// </summary>
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("SettingWindow"); // 打开设置窗口
        }

        /// <summary>
        /// 失去焦点时关闭功能面板
        /// </summary>
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            this.Activate(); // 激活窗口
            if (!AppStateManager.Pause && !buttonManager.isClosing && !AppStateManager.MainWindowPinned)
            {
                buttonManager.isClosing = true; // 设置关闭标志
                this.Close(); // 关闭窗口
            }
        }

        /// <summary>
        /// 订住功能面板
        /// </summary>
        private void BookQuicker(object sender, EventArgs e)
        {
            AppStateManager.MainWindowPinned = !AppStateManager.MainWindowPinned; // 反转 AppStateManager.Pinned
            ((MainWindowViewModel)this.DataContext).IsPinned = AppStateManager.MainWindowPinned; // 反转 ViewModel 的 IsPinned
        }

        /// <summary>
        /// 关闭功能面板
        /// </summary>
        private void CloseMainWindow(object sender, EventArgs e)
        {
            Close(); // 关闭窗口
        }

        /// <summary>
        /// 切换动作页（根据按钮数据，切换到指定类型和索引的动作页）
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenActionPage(ButtonData data)
        {
            string type = data.Data1; // 获取动作页类型
            int targetIndex = int.Parse(data.Data2); // 获取目标动作页索引
            if (type != GLOBAL_STYLE) OnCommonStyleChanged(type); // 如果切换到非全局动作页，更新样式

            Panel parent = type == GLOBAL_STYLE ? GlobalGrid : CommonGrid; // 获取当前Grid容器
            int currentGridIndex = GetVisiblePageGridIndex(parent, type); // 获取当前可见的Grid编号
            /* 可有可无的逻辑
                if (currentGridIndex == targetIndex) return; // 已在目标页，无需后续操作
             */

            bool scrollUp = currentGridIndex > targetIndex; // 判断滚动方向
            int steps = Math.Abs(currentGridIndex - targetIndex); // 需要滚动的次数
            int delta = scrollUp ? Mouse.MouseWheelDeltaForOneLine : -Mouse.MouseWheelDeltaForOneLine; // 计算滚动变化量

            // 根据类型确定参数
            int rows = type == GLOBAL_STYLE ? 3 : 4;
            int cols = 4;
            Panel buttonPanel = type == GLOBAL_STYLE ? GlobalButtonPanel : CommonButtonPanel;

            // 循环调用翻页方法
            for (int i = 0; i < steps; i++)
            {
                NavigatePage(parent, type, rows, cols, buttonPanel, delta);
            }
        }

        // 窗口关闭时强制垃圾回收
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的关闭事件
            cancellationTokenSource.Cancel(); // 取消所有后台任务
            cancellationTokenSource.Dispose();
            CleanUpEventHandlers(); // 清理事件处理器
            CleanUpGrid(GlobalGrid); // 清理全局网格
            CleanUpGrid(CommonGrid); // 清理通用网格

            CommonStyle = null; // 清理通用样式
            iconManager.Dispose(); // 释放图标管理器资源
            buttonManager.Dispose(); // 释放按钮管理器资源

            GC.Collect(); // 强制回收内存
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制回收内存
        }

        /// <summary>
        /// 清理指定Grid中的所有Grid及其子元素
        /// </summary>
        /// <param name="grid">要清理的Grid</param>
        private void CleanUpGrid(Grid grid)
        {
            foreach (Grid Grid in VisualTreeHelper.FindVisualChildren<Grid>(grid))
            {
                foreach (Button button in VisualTreeHelper.FindVisualChildren<Button>(Grid))
                {
                    // 移除所有事件处理器
                    button.Click -= DoAction; // 左键点击事件
                    button.Drop -= Button_Drop; // 拖拽事件
                    button.PreviewDragOver -= Button_PreviewDragOver; // 添加拖拽事件
                    button.MouseRightButtonDown -= OpenCreatActionMenu; // 右键点击事件
                    button.PreviewMouseMove -= Button_PreviewMouseMove; // 鼠标移动事件
                    button.PreviewMouseLeftButtonUp -= Button_PreviewMouseLeftButtonUp; // 鼠标左键释放事件
                    button.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown; // 鼠标左键按下事件
                    button.MouseEnter -= Button_MouseEnter; // 鼠标移入事件
                    button.MouseLeave -= Button_MouseLeave; // 鼠标移出事件

                    // 清理按钮内容和资源
                    button.Content = null; // 清理按钮内容
                    button.Tag = null; // 清理按钮标签
                    button.Background = null; // 清理按钮背景
                }
                Grid.Children.Clear(); // 清空Grid
            }
            grid.Children.Clear(); // 清空Grid
        }

        // 清理所有动态添加的事件处理器
        private void CleanUpEventHandlers()
        {
            foreach (Button button in GlobalButtonPanel.Children.OfType<Button>()) // 清理全局按钮面板事件
            {
                button.Click -= GlobalPageChangeButton_Click; // 全局页面切换按钮点击
            }

            foreach (Button button in CommonButtonPanel.Children.OfType<Button>()) // 清理公共按钮面板事件
            {
                button.Click -= CommonPageChangeButton_Click; // 通用页面切换按钮点击
            }
        }

        /// <summary>
        /// 动态生成一页的Grid和按钮
        /// </summary>
        /// <param name="parent">父容器（如 CommonGrid/GlobalGrid）</param>
        /// <param name="gridType">"common" 或 "_global"</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        private Grid GeneratePageGrid(Panel parent, string gridType, int pageIndex, int rows, int cols)
        {
            var grid = new Grid { Name = $"{gridType}{pageIndex}" }; // 创建Grid
            AddGridRows(grid, rows, ((MainWindowViewModel)this.DataContext).ButtonGap); // 添加行定义
            AddGridColumns(grid, cols, ((MainWindowViewModel)this.DataContext).ButtonGap); // 添加列定义
            // 添加按钮到Grid
            AddButtonsToGrid(grid, pageIndex, rows, cols, gridType, FindResource("Button") as Style, ((MainWindowViewModel)this.DataContext).ButtonSize);
            parent.Children.Add(grid); // 添加到父容器
            return grid;
        }

        /// <summary>
        /// 为Grid添加行定义（含间隔）
        /// </summary>
        /// <param name="grid">目标Grid</param>
        /// <param name="rows">行数</param>
        /// <param name="gap">行间距</param>
        private void AddGridRows(Grid grid, int rows, double gap)
        {
            for (int i = 0; i < rows * 2 - 1; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = (i % 2 == 0)
                        ? new GridLength(1, GridUnitType.Auto)
                        : new GridLength(gap)
                });
            }
        }

        /// <summary>
        /// 为Grid添加列定义（含间隔）
        /// </summary>
        /// <param name="grid">目标Grid</param>
        /// <param name="cols">列数</param>
        /// <param name="gap">列间距</param>
        private void AddGridColumns(Grid grid, int cols, double gap)
        {
            for (int j = 0; j < cols * 2 - 1; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = (j % 2 == 0)
                        ? new GridLength(1, GridUnitType.Auto)
                        : new GridLength(gap)
                });
            }
        }

        /// <summary>
        /// 向Grid中批量添加按钮
        /// </summary>
        /// <param name="grid">目标Grid</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="gridType">Grid类型</param>
        /// <param name="style">按钮样式</param>
        /// <param name="size">按钮尺寸</param>
        private void AddButtonsToGrid(Grid grid, int pageIndex, int rows, int cols, string gridType, Style style, double size)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int buttonIndex = pageIndex * 100 + (row + 1) * 10 + (col + 1);
                    string buttonName = $"{gridType}{buttonIndex}";
                    Button button = CreateButton(buttonName, style, size);
                    // 设置行列（注意：有间隔，实际行列号*2）
                    Grid.SetRow(button, row * 2);
                    Grid.SetColumn(button, col * 2);
                    // 绑定数据和事件
                    BindButtonDataAndEvents(button, buttonIndex, gridType);
                    grid.Children.Add(button);
                }
            }
        }

        /// <summary>
        /// 创建一个按钮并设置基本属性
        /// </summary>
        /// <param name="name">按钮名称</param>
        /// <param name="style">按钮样式</param>
        /// <param name="size">按钮尺寸</param>
        /// <returns>新建的Button对象</returns>
        private Button CreateButton(string name, Style style, double size)
        {
            return new Button
            {
                Name = name,
                Style = style,
                Width = size,
                Height = size
            };
        }

        /// <summary>
        /// 绑定按钮的数据和事件，并刷新显示
        /// </summary>
        /// <param name="button">目标按钮</param>
        /// <param name="buttonIndex">按钮索引</param>
        /// <param name="gridType">Grid类型</param>
        private void BindButtonDataAndEvents(Button button, int buttonIndex, string gridType)
        {
            BindButtonEvents(button); // 绑定事件
            var buttonData = db2.GetButtonDataByID(buttonIndex, gridType); // 获取按钮数据
            button.Tag = buttonData; // 绑定数据
            buttonManager.RefreshButtonDisplay(button, buttonData, 60, true); // 刷新显示
        }

        /// <summary>
        /// 显示指定页的Grid
        /// </summary>
        /// <param name="parent">父容器（如 CommonGrid/GlobalGrid）</param>
        /// <param name="gridType">"common" 或 "_global"</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        private void ShowPageGrid(Panel parent, string gridType, int pageIndex, int rows, int cols)
        {
            foreach (UIElement child in parent.Children)
            {
                if (child is Grid g && g.Name.StartsWith(gridType))
                    g.Visibility = (g.Name == $"{gridType}{pageIndex}") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 鼠标移入按钮时的处理：
        /// 1. 有Tag（有数据）的按钮执行放大动画（如允许）。
        /// 2. 无Tag（空按钮）显示添加图片（如允许）。
        /// </summary>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                var appearance = SettingDatabase.GetAllAppearanceSettings().FirstOrDefault();
                if (btn.Tag != null) // 有数据的按钮，执行放大动画
                {
                    AnimateButtonScale(btn, appearance);
                }
                else // 空按钮，显示添加图片
                {
                    var conventions = SettingDatabase.GetAllConventions().FirstOrDefault();
                    ShowAddImageOnButton(btn, appearance, conventions);
                }
            }
        }

        /// <summary>
        /// 对有Tag的按钮执行放大动画（如果设置允许）。
        /// </summary>
        /// <param name="btn">目标按钮</param>
        /// <param name="appearance">外观设置</param>
        private void AnimateButtonScale(Button btn, dynamic appearance)
        {
            if (appearance == null || !appearance.ShowActionButtonMouseOver) return; // 不显示动画
            var border = VisualTreeHelper.FindVisualChild<Border>(btn); // 获取按钮边框
            var scale = new ScaleTransform(1, 1); // 创建缩放动画
            border.RenderTransformOrigin = new Point(0.5, 0.5); // 设置缩放中心
            border.RenderTransform = scale; // 设置缩放动画

            var animX = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(100)); // 创建X轴缩放动画
            var animY = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(100)); 
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animX); // 开始动画
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
        }

        /// <summary>
        /// 对无Tag的按钮显示添加图片（如果设置允许）。
        /// </summary>
        /// <param name="btn">目标按钮</param>
        /// <param name="appearance">外观设置（用于获取按钮尺寸）</param>
        /// <param name="conventions">通用设置（用于判断是否显示图片）</param>
        private void ShowAddImageOnButton(Button btn, dynamic appearance, dynamic conventions)
        {
            if (conventions == null || !conventions.ShowAddImage) return; // 不显示图片
            double btnSize = appearance.ButtonSize; // 获取按钮尺寸
            double imageSize = btnSize / 2.0; // 计算图片尺寸
            btn.Content = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Add.png")),
                Stretch = Stretch.Uniform,
                Height = imageSize,
                Width = imageSize,
                Opacity = 0.5
            };
        }

        /// <summary>
        /// 鼠标移出按钮时的处理：
        /// 1. 有Tag（有数据）的按钮还原缩放动画。
        /// 2. 无Tag（空按钮）移除添加图片。
        /// </summary>
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag != null) // 有数据的按钮，还原缩放动画
                {
                    RestoreButtonScale(btn);
                }
                else // 空按钮，移除图片
                {
                    btn.Content = null;
                }
            }
        }

        /// <summary>
        /// 还原有Tag按钮的缩放动画。
        /// </summary>
        /// <param name="btn">目标按钮</param>
        private void RestoreButtonScale(Button btn)
        {
            var border = VisualTreeHelper.FindVisualChild<Border>(btn); // 获取按钮边框
            if (border != null)
            {
                var scale = border.RenderTransform as ScaleTransform; // 获取缩放动画
                if (scale != null)
                {
                    var animX = new DoubleAnimation(1, TimeSpan.FromMilliseconds(100)); // 缩放动画
                    var animY = new DoubleAnimation(1, TimeSpan.FromMilliseconds(100));
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, animX); // 开始动画
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
                }
            }
        }

        // 点击按钮打开搜索窗口
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow searchWindow = System.Windows.Application.Current.Windows.OfType<SearchWindow>().FirstOrDefault(); // 获取搜索窗口
            if (searchWindow == null)
            {
                searchWindow = new SearchWindow(); // 创建搜索窗口
                searchWindow.Show(); // 显示窗口
            }
            else
            {
                searchWindow.Close(); // 关闭窗口
            }
        }
    }
}