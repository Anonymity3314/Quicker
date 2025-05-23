using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Managers;
using Quicker.Database;
using System.Windows;
using System.IO;

namespace Quicker.Windows
{
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush SelectedBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8D8D8D")); // 选中页面按钮颜色
        private static readonly SolidColorBrush UnSelectedBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD3D3D3")); // 未选中页面按钮颜色

        private readonly CancellationTokenSource cancellationTokenSource = new(); // 取消后台任务的令牌源
        public readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly IconManager iconManager = new(); // 图标管理器
        private readonly ActionPageDatabase db3 = new(); // 动作页面数据库
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private string CommonStyle; // 样式

        public MainWindow(string style)
        {
            CommonStyle = style; // 设置样式
            InitializeComponent(); // 初始化窗口组件
        }

        /// <summary>
        /// 通用类型改变事件
        /// </summary>
        /// <param name="style"> 样式名称 </param>
        public void OnCommonStyleChanged(string style)
        {
            CommonStyle = style; // 设置样式
            CommonGrid.Children.Clear(); // 清空通用网格
            SetCommonTextBlock(0); // 设置通用标签内容
            GenerateUniformGrid(0, CommonStyle); // 生成对应样式 UniformGrid
            GenerateButtons(); // 生成按钮
            LockCommonActionPage(null, null); // 锁住通用动作页面
        }

        // 加载数据库和Button
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeActionPages(); // 初始化动作页
            InitializeButtons(); // 初始化按钮
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowTopmost(this);// 设置窗口置顶
            this.Activate(); // 激活窗口
        }

        /// 初始化动作页面
        private void InitializeActionPages()
        {
            GlobalGrid.Children.Remove(ViewGlobalUniformGrid); // 从主网格中移除
            CommonGrid.Children.Remove(ViewCommonUniformGrid); // 从主网格中移除
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenerateUniformGrid(0, "Global"); // 生成全局 UniformGrid
                var targetStyle = db2.TableExists(CommonStyle) ? CommonStyle : (CommonStyle = "Common"); // 设置样式
                GenerateUniformGrid(0, targetStyle); // 生成对应样式 UniformGrid
                GenerateButtons(); // 生成按钮
                SetCommonTextBlock(0); // 设置通用标签内容
            }); // 在主线程中执行
        }

        // 初始化按钮
        private void InitializeButtons()
        {
            // 加载BookButton图标
            string iconPath = AppStateManager.Book ? AppStateManager.BookIconPath : AppStateManager.DisBookIconPath; // 获取图标路径
            BitmapImage bookImage = new BitmapImage(new Uri(iconPath, UriKind.Relative)); // 创建图标对象
            Book.Source = bookImage; // 设置Book按钮的图标

            // 加载LockButton图标
            string lockIconPath = AppStateManager.Locked ? AppStateManager.LockIconPath : AppStateManager.UnLockIconPath; // 获取图标路径
            BitmapImage lockImage = new BitmapImage(new Uri(lockIconPath, UriKind.Relative)); // 创建图标对象
            Lock.Source = lockImage; // 设置Lock按钮的图标

            HasNewVersionTip.Visibility = AppStateManager.HasNewVersion
                ? Visibility.Visible
                : Visibility.Collapsed; // 设置是否有新版本提示
        }

        // 生成页面切换 Button
        private void GenerateButtons()
        {
            GlobalButtonPanel.Children.Clear(); // 清空全局动作页切换按钮
            CommonButtonPanel.Children.Clear(); // 清空通用动作页切换按钮
            var globalSceneData = db3.GetSceneData("Global").FirstOrDefault(); // 从数据库中获取全局动作页面数据
            var commonSceneData = db3.GetSceneData(CommonStyle).FirstOrDefault(); // 从数据库中获取通用动作页面数据
            GeneratePageButtons("Global", globalSceneData.SceneCount, SwitchToGlobalUniformGrid, GlobalActionPageChangeButton_MouseEnter, GlobalActionPageChangeButton_MouseLeave, GlobalButtonPanel); // 生成全局页面切换按钮
            GeneratePageButtons(CommonStyle, commonSceneData.SceneCount, SwitchToCommonUniformGrid, CommonActionPageChangeButton_MouseEnter, CommonActionPageChangeButton_MouseLeave, CommonButtonPanel); // 生成通用页面切换按钮
        }

        /// <summary>
        /// 生成页面切换按钮
        /// </summary>
        /// <param name="prefix"> 按钮名称前缀 </param>
        /// <param name="totalPages"> 总页面数 </param>
        /// <param name="clickHandler"> 点击事件处理程序 </param>
        /// <param name="mouseEnterHandler"> 鼠标进入事件处理程序 </param>
        /// <param name="mouseLeaveHandler"> 鼠标离开事件处理程序 </param>
        /// <param name="panel"> 按钮所属的面板</param>
        private void GeneratePageButtons(string prefix, int totalPages, RoutedEventHandler clickHandler, MouseEventHandler mouseEnterHandler, MouseEventHandler mouseLeaveHandler, Panel panel)
        {
            if (totalPages == 1) return; // 如果只有一个页面，不生成按钮
            for (int i = 0; i < totalPages; i++)
            {
                Button button = new Button
                {
                    Style = FindResource("ActionPageChangeButton") as Style, // 设置按钮样式
                    Name = $"{prefix}{i}" // 设置按钮名称
                }; // 创建按钮对象
                if (i == 0) button.Background = SelectedBrush; // 设置当前按钮颜色

                // 添加事件处理程序
                button.Click += clickHandler; // 绑定点击事件
                button.MouseEnter += mouseEnterHandler; // 绑定鼠标进入事件
                button.MouseLeave += mouseLeaveHandler; // 绑定鼠标离开事件

                panel.Children.Add(button); // 添加到面板
            }
        }

        // 切换到全局UniformGrid
        private void SwitchToGlobalUniformGrid(object sender, RoutedEventArgs e)
        {
            SwitchToUniformGrid(sender, MainGrid, "Global"); // 切换到全局UniformGrid
        }

        // 切换到通用UniformGrid
        private void SwitchToCommonUniformGrid(object sender, RoutedEventArgs e)
        {
            SwitchToUniformGrid(sender, CommonGrid, CommonStyle); // 切换到通用UniformGrid
        }

        /// <summary>
        /// 切换到指定的UniformGrid
        /// </summary>
        /// <param name="sender"> 触发事件的对象 </param>
        /// <param name="targetGrid"> 目标Grid </param>
        /// <param name="style"> 样式名称 </param>
        private void SwitchToUniformGrid(object sender, Grid targetGrid, string style)
        {
            if (sender is Button clickedButton)
            {
                int uniformGridIndex = int.Parse(clickedButton.Name.Replace($"{style}", "")); // 获取UniformGrid索引
                string targetUniformGridName = $"{style}{uniformGridIndex}"; // 生成目标UniformGrid名称
                UniformGrid targetUniformGrid = buttonManager.FindVisualChildren<UniformGrid>(targetGrid).FirstOrDefault(c => c.Name == targetUniformGridName); // 查找目标UniformGrid

                // 如果目标UniformGrid不存在，动态生成
                if (targetUniformGrid == null)
                {
                    GenerateUniformGrid(uniformGridIndex, style); // 动态生成UniformGrid
                    targetUniformGrid = buttonManager.FindVisualChildren<UniformGrid>(targetGrid).FirstOrDefault(c => c.Name == targetUniformGridName); // 查找目标UniformGrid
                }
                targetUniformGrid.Visibility = Visibility.Visible; // 设置目标UniformGrid可见
                foreach (UniformGrid uniformGrid in buttonManager.FindVisualChildren<UniformGrid>(targetGrid)) // 隐藏其他UniformGrid
                {
                    if (uniformGrid.Name.StartsWith($"{style}") && uniformGrid != targetUniformGrid)
                        uniformGrid.Visibility = Visibility.Collapsed; // 隐藏其他UniformGrid
                }

                if(style != "Global") SetCommonTextBlock(uniformGridIndex); // 设置通用标签内容
            }
        }

        // 设置标签内容
        private void SetCommonTextBlock(int uniformGridIndex)
        {
            var actionPageData = db3.GetActionPageData(CommonStyle, uniformGridIndex); // 从数据库中获取动作页面数据
            CommonTextBlock.Text = actionPageData.ActionPageName; // 设置标签内容
            CommonTextBlock.ToolTip = actionPageData.ActionPageName; // 设置标签提示
        }

        // 移动功能面板
        private void MoveMainWindow(object sender, EventArgs e)
        {
            DragMove(); // 触发窗口拖动
        }

        // 订住功能面板
        private void BookQuicker(object sender, EventArgs e)
        {
            AppStateManager.Book = !AppStateManager.Book; // 更新数据库中的设置
            BitmapImage bookimage = new(); // 创建图像对象
            bookimage.BeginInit(); // 开始初始化
            bookimage.UriSource = AppStateManager.Book
                ? new Uri(AppStateManager.BookIconPath, UriKind.Relative) // 设置为订住样式
                : new Uri(AppStateManager.DisBookIconPath, UriKind.Relative); // 设置为不订住样式
            bookimage.EndInit(); // 结束初始化
            Book.Source = bookimage; // 更新Book按钮图标
        }

        // 打开设置窗口
        private void OpenSettingWindow(object sender, RoutedEventArgs e)
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("SettingWindow"); // 打开设置窗口
        }

        // 关闭功能面板
        private void CloseMainWindow(object sender, EventArgs e)
        {
            buttonManager.isClosing = true; // 设置关闭标志
            this.Close(); // 关闭窗口
        }

        // 失去焦点时关闭功能面板
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            ActionInformationWindow actionInformationWindow = App.Current.Windows.OfType<ActionInformationWindow>().FirstOrDefault(); // 查找ActionInformationWindow
            if (actionInformationWindow != null) // 如果存在ActionInformationWindow
                this.Activate(); // 激活窗口
            else if (!AppStateManager.Pause && !buttonManager.isClosing && !AppStateManager.Book)
            {
                buttonManager.isClosing = true; // 设置关闭标志
                this.Close(); // 关闭窗口
            }
            else
                this.Activate(); // 激活窗口
        }

        // 鼠标移入Button改变外观
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            if (button.Tag is ButtonData)
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD")); // 改变按钮背景颜色
                button.RenderTransform = new ScaleTransform(1.05, 1.05); // 放大按钮
                UniformGrid.SetZIndex(button, 1); // 调整按钮层级
            }
            else
            {
                var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取配置信息
                if (Convention?.ShowAddImage == true) // 如果显示添加按钮
                    button.Content = new Image { Style = FindResource("AddActionImage") as Style }; // 设置按钮内容
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA")); // 改变按钮背景颜色
            }
        }
        private void GlobalActionPageChangeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseEnter(sender, "Global", "#FFB9B9B9"); // 改变按钮颜色
        }
        private void CommonActionPageChangeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseEnter(sender, CommonStyle, "#FFB9B9B9"); // 改变按钮颜色
        }
        /// <summary>
        /// 鼠标移入Button改变外观
        /// </summary>
        /// <param name="sender"> 按钮 </param>
        /// <param name="prefix"> 按钮名称前缀 </param>
        /// <param name="color"> 按钮颜色 </param>
        private void PageChangeButton_MouseEnter(object sender, string prefix, string color)
        {
            if (sender is Button button)
            {
                int uniformGridIndex = int.Parse(button.Name.Replace($"{prefix}", "")); // 获取UniformGrid索引
                string targetUniformGridName = $"{prefix}{uniformGridIndex}"; // 生成目标UniformGrid名称
                UniformGrid targetUniformGrid = null; // 初始化目标UniformGrid

                var grid = prefix == "Global" ? MainGrid : CommonGrid; // 根据前缀选择不同的Grid
                foreach (UniformGrid uniformGrid in buttonManager.FindVisualChildren<UniformGrid>(grid)) // 查找目标UniformGrid
                {
                    if (uniformGrid.Name == targetUniformGridName)
                    {
                        targetUniformGrid = uniformGrid; // 找到目标UniformGrid
                        break;
                    }
                }

                if (targetUniformGrid == null)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
                    return;
                }

                if (targetUniformGrid.Visibility != Visibility.Visible)
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); // 改变按钮背景颜色
            }
        }

        // 鼠标移出Button还原外观
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            if (button.Tag is ButtonData)
            {
                UniformGrid.SetZIndex(button, 0); // 还原按钮层级
                button.RenderTransform = new ScaleTransform(1, 1); // 还原按钮大小
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); // 还原背景颜色
            }
            else
            {
                button.Content = null; // 清空按钮内容
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3")); // 还原背景颜色
            }
        }
        private void GlobalActionPageChangeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseLeave(sender, "Global"); // 还原按钮颜色
        }
        private void CommonActionPageChangeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PageChangeButton_MouseLeave(sender, CommonStyle); // 还原按钮颜色
        }
        /// <summary>
        /// 鼠标移出Button还原外观
        /// </summary>
        /// <param name="sender">按钮</param>
        /// <param name="prefix">按钮名称前缀</param>
        private void PageChangeButton_MouseLeave(object sender, string prefix)
        {
            if (sender is Button button)
            {
                int uniformGridIndex = int.Parse(button.Name.Replace($"{prefix}", "")); // 获取UniformGrid索引
                string targetUniformGridName = $"{prefix}{uniformGridIndex}"; // 生成目标UniformGrid名称
                UniformGrid targetUniformGrid = null; // 初始化目标UniformGrid

                var grid = prefix == "Global" ? MainGrid : CommonGrid; // 根据前缀选择不同的Grid
                foreach (UniformGrid uniformGrid in buttonManager.FindVisualChildren<UniformGrid>(grid)) // 查找目标UniformGrid
                {
                    if (uniformGrid.Name != targetUniformGridName) continue; // 如果不是目标UniformGrid，跳过
                    targetUniformGrid = uniformGrid; // 找到目标UniformGrid
                    break;
                }

                if (targetUniformGrid == null) // 如果目标UniformGrid不存在
                    button.Background = UnSelectedBrush; // 还原按钮背景颜色
                else if (targetUniformGrid.Visibility != Visibility.Visible) // 如果目标UniformGrid不可见
                    button.Background = UnSelectedBrush; // 还原按钮背景颜色
            }
        }

        // 左键点击按钮时执行动作
        private void DoAction(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            string buttonType = GetButtonType(sender); // 获取按钮类型
            if (button.Tag is ButtonData data)
            {
                if (!AppStateManager.Book && data.ActionType != "OpenActionPage") 
                    this.Visibility = Visibility.Collapsed; // 隐藏窗口

                DoAction(data); // 执行动作
                db2.IncreaseActionUsedTimes(data.ButtonID, buttonType); // 增加动作使用次数

                bool autoReturn = db3.GetAutoReturnToFirstPage(buttonType); // 获取是否自动返回第一页
                if(autoReturn) // 如果自动返回第一页，清空按钮所在容器
                {
                    if (buttonType == "Global")
                        GlobalGrid.Children.Clear();
                    else
                        CommonGrid.Children.Clear();
                    GenerateUniformGrid(0, buttonType); // 重新生成第一页内容
                }
            }
            else
            {
                var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取配置信息
                if (Convention.ShowAddImage) // 如果显示添加按钮
                    buttonManager.OpenMenu(sender, true, "CreatActionMenu", this, buttonType); // 点击打开菜单
            }
        }

        /// <summary>
        /// 执行按钮动作
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        private void DoAction(ButtonData data)
        {
            using var actionManager = new ActionManager(); // 创建 ActionManager 的实例
            switch (data.ActionType)
            {
                case "OpenFile":
                    actionManager.OpenFile(data); // 打开文件
                    break; // 打开文件、文件夹
                case "OpenWebsite":
                    actionManager.OpenWebsite(data); // 打开网站
                    break; // 打开网站
                case "OpenFiles":
                    actionManager.OpenFiles(data); // 打开多个文件
                    break; // 打开多个文件
                case "OpenUwpApp":
                    actionManager.OpenUwpApp(data); // 打开UWP应用
                    break; // 打开UWP应用
                case "OpenActionPage":
                    OpenActionPage(data); // 打开动作页
                    break; // 打开动作页
            }
        }

        /// <summary>
        /// 切换动作页
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void OpenActionPage(ButtonData data)
        {
            string type = data.Data1; // 获取动作页类型
            int index = int.Parse(data.Data2); // 获取动作页索引
            if (type != "Global") OnCommonStyleChanged(type); // 如果切换到非全局动作页，更新样式
            int currentUniformGridIndex = GetVisibleUniformGridIndex(type); // 获取当前可见的UniformGrid编号
            if (currentUniformGridIndex > index) // 如果当前UniformGrid编号大于目标UniformGrid编号
                for (int i = currentUniformGridIndex; i > index; i--)
                    SwitchToPreviousUniformGrid(i, type); // 向前切换UniformGrid
            else // 如果当前UniformGrid编号小于目标UniformGrid编号
                for (int i = currentUniformGridIndex; i < index; i++)
                    SwitchToNextUniformGrid(i, type); // 向后切换UniformGrid
        }

        // 右键按钮打开菜单
        public void OpenCreatActionMenu(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button; // 获取Button对象
            buttonManager.OpenMenu(sender, true, button.Tag is ButtonData ? "OperationMenu" : "CreatActionMenu", this, GetButtonType(sender)); // 打开操作菜单
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
                buttonManager.Button_Drop(sender, e, true, GetButtonType(sender)); // 处理拖拽事件
            }
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
            {
                buttonManager.Button_PreviewMouseMove(sender, e, true, GetButtonType(sender)); // 检查拖拽条件
            }
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
            System.Windows.Application.Current.Shutdown(); // 退出程序
        }

        // 打开动作管理窗口
        private void OpenActionPageManageWindow(object sender, RoutedEventArgs e)
        {
            TitlePop.IsOpen = false; // 关闭菜单
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.OpenTargetWindow("ActionPageManageWindow"); // 打开动作管理窗口
        }

        // 滚轮进行全局动作页翻页
        private void GolbalGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeVisibleUniformGrid(e, "Global"); // 滚轮进行全局动作页翻页
        }

        /// <summary>
        /// 获取当前可见的UniformGrid编号
        /// </summary>
        /// <param name="type"></param>
        /// <returns> 当前可见的UniformGrid编号 </returns>
        private int GetVisibleUniformGridIndex(string type)
        {
            var uniformGridCollection = type == "Global" // 根据是否是全局UniformGrid选择集合
                ? buttonManager.FindVisualChildren<UniformGrid>(GlobalGrid) // 查找GlobalGrid下的UniformGrid集合
                : buttonManager.FindVisualChildren<UniformGrid>(CommonGrid); // 查找CommonGrid下的UniformGrid集合
            foreach (UniformGrid uniformGrid in uniformGridCollection) // 遍历UniformGrid集合
            {
                if (uniformGrid.Visibility == Visibility.Visible)
                    return int.Parse(uniformGrid.Name.Replace(type, "")); // 如果匹配成功，返回UniformGrid编号
            }
            return 0; // 默认返回0
        }

        /// <summary>
        /// 滑动滚轮更改当前可见 UniformGrid
        /// </summary>
        /// <param name="e">鼠标滚轮事件参数</param>
        /// <param name="style">UniformGrid 类型</param>
        private void ChangeVisibleUniformGrid(MouseWheelEventArgs e, string style)
        {
            e.Handled = true; // 标记事件已处理
            int delta = e.Delta; // 获取鼠标滚轮的增量值
            int currentUniformGridIndex = GetVisibleUniformGridIndex(style); // 获取当前可见的UniformGrid编号
            if (delta > 0) SwitchToPreviousUniformGrid(currentUniformGridIndex, style); // 向上滚动，切换到上一页
            else SwitchToNextUniformGrid(currentUniformGridIndex, style); // 向下滚动，切换到下一页
        }

        /// <summary>
        /// 切换到上一页
        /// </summary>
        /// <param name="currentUniformGridIndex">当前可见的UniformGrid编号</param>
        /// <param name="style">UniformGrid 类型</param>
        private void SwitchToPreviousUniformGrid(int currentUniformGridIndex, string style)
        {
            SwitchUniformGrid(currentUniformGridIndex, style, false); // 向上滚动，切换到上一页
        }

        /// <summary>
        /// 切换到下一页
        /// </summary>
        /// <param name="currentUniformGridIndex">当前可见的UniformGrid编号</param>
        /// <param name="style">UniformGrid 类型</param>
        private void SwitchToNextUniformGrid(int currentUniformGridIndex, string style)
        {
            SwitchUniformGrid(currentUniformGridIndex, style, true); // 向下滚动，切换到下一页
        }

        /// <summary>
        /// 切换UniformGrid
        /// </summary>
        /// <param name="currentUniformGridIndex"> 当前可见的UniformGrid编号 </param>
        /// <param name="style"> UniformGrid 类型 </param>
        /// <param name="isNext"> 是否向下滚动 </param>
        private void SwitchUniformGrid(int currentUniformGridIndex, string style, bool isNext)
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取设置数据
            int targetUniformGridIndex = isNext ? currentUniformGridIndex + 1 : currentUniformGridIndex - 1; // 计算目标UniformGrid编号
            var sceneData = db3.GetSceneData(style).FirstOrDefault(); // 从数据库中获取动作页数据
            if (targetUniformGridIndex == sceneData.SceneCount || targetUniformGridIndex < 0) // 如果目标UniformGrid编号超出范围
            {
                if (Convention.LoopPageFlipping) // 如果循环翻页
                    targetUniformGridIndex = isNext ? 0 : sceneData.SceneCount; // 循环到第一页或最后一页
                else return; // 如果不循环翻页，直接返回
            }

            string targetUniformGridName = $"{style}{targetUniformGridIndex}"; // 生成目标UniformGrid名称
            UniformGrid targetUniformGrid = buttonManager.FindVisualChildren<UniformGrid>(style == "Global" ? GlobalGrid : CommonGrid)
                .FirstOrDefault(c => c.Name == targetUniformGridName); // 查找目标UniformGrid

            if (targetUniformGrid == null) // 如果目标UniformGrid不存在
            {
                GenerateUniformGrid(targetUniformGridIndex, style); // 动态生成UniformGrid
                targetUniformGrid = buttonManager.FindVisualChildren<UniformGrid>(style == "Global" ? GlobalGrid : CommonGrid)
                    .FirstOrDefault(c => c.Name == targetUniformGridName); // 查找目标UniformGrid
            }

            targetUniformGrid.Visibility = Visibility.Visible; // 设置目标UniformGrid可见
            string currentUniformGridName = $"{style}{currentUniformGridIndex}"; // 生成当前UniformGrid名称
            UniformGrid currentUniformGrid = buttonManager.FindVisualChildren<UniformGrid>(style == "Global" ? GlobalGrid : CommonGrid)
                .FirstOrDefault(c => c.Name == currentUniformGridName); // 查找当前UniformGrid
            currentUniformGrid.Visibility = Visibility.Collapsed; // 隐藏当前UniformGrid

            if (style != "Global") SetCommonTextBlock(targetUniformGridIndex); // 设置通用动作页标签
        }

        /// <summary>
        /// 生成UniformGrid
        /// </summary>
        /// <param name="uniformGridIndex"> 要生成的页面索引 </param>
        /// <param name="style"> UniformGrid 类型 </param>
        public void GenerateUniformGrid(int uniformGridIndex, string style)
        {
            int rows = style == "Global" ? 3 : 4, cols = 4; // 行数和列数
            UniformGrid newUniformGrid = new UniformGrid
            {
                Rows = rows, // 设置行数
                Columns = cols, // 设置列数
                Name = $"{style}{uniformGridIndex}", // 设置名称
            }; // 创建UniformGrid对象

            if (style == "Global")
            {
                GlobalGrid.Children.Add(newUniformGrid); // 添加到主Grid
                newUniformGrid.IsVisibleChanged += GlobalUniformGrid_IsVisibleChanged; // 添加可见性变化事件
            }
            else
            {
                CommonGrid.Children.Add(newUniformGrid); // 添加到公共Grid
                newUniformGrid.IsVisibleChanged += CommonUniformGrid_IsVisibleChanged; // 添加可见性变化事件
            }

            Panel ParentPanel = style == "Global" ? GlobalButtonPanel : CommonButtonPanel; // 根据样式选择父面板
            foreach (var button in ParentPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
            {
                button.Background = button.Name.Contains($"{uniformGridIndex}") // 判断是否是当前按钮
                    ? SelectedBrush
                    : UnSelectedBrush; // 设置当前按钮颜色
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int buttonIndex = uniformGridIndex * 100 + (row + 1) * 10 + (col + 1); // 按钮索引
                    string buttonName = $"{style}{buttonIndex}"; // 按钮名称
                    Style styleResource = FindResource("Button") as Style; // 按钮样式
                    Button button = CreateButton(buttonName, styleResource, row, col); // 创建按钮
                    newUniformGrid.Children.Add(button); // 添加按钮到UniformGrid
                    var buttonData = db2.GetButtonDataByTableName(style); // 从数据库中获取按钮数据
                    foreach (var data in buttonData)
                    {
                        if (data.ButtonID == buttonIndex)
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
        /// <param name="row">Button 的行</param>
        /// <param name="col">Button的列</param>
        /// <returns>生成的 Button</returns>
        private Button CreateButton(string name, Style style,int row = 0, int col = 0)
        {
            Button button = new Button
            {
                Name = name, // 设置名称
                Style = style, // 设置样式
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
        private void GlobalUniformGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UniformGrid uniformGrid && uniformGrid.IsVisible)
            {
                int uniformGridIndex = int.Parse(uniformGrid.Name.Replace("Global", "")); // 获取UniformGrid索引
                foreach (var button in GlobalButtonPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
                {
                    button.Background = button.Name.Contains($"{uniformGridIndex}") // 判断是否是当前按钮
                        ? SelectedBrush
                        : UnSelectedBrush; // 设置当前按钮颜色
                } // 设置所有按钮的颜色
            }
        }

        // 锁定通用动作页
        private void LockCommonActionPage(object sender, RoutedEventArgs e)
        {
            AppStateManager.Locked = !AppStateManager.Locked; // 切换锁定状态
            string lockIconPath = AppStateManager.Locked ? AppStateManager.LockIconPath : AppStateManager.UnLockIconPath; // 获取图标路径
            BitmapImage lockImage = new BitmapImage(new Uri(lockIconPath, UriKind.Relative)); // 创建 BitmapImage 对象
            Lock.Source = lockImage; // 设置图标
            if (AppStateManager.Locked) AppStateManager.CommonState = CommonStyle; // 设置锁定状态
        }

        // 滚轮进行通用动作页翻页
        private void CommonGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeVisibleUniformGrid(e, CommonStyle); // 调用切换 UniformGrid 方法
        }

        // 通用动作页可见性与切换按钮背景绑定
        private void CommonUniformGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UniformGrid uniformGrid && uniformGrid.IsVisible)
            {
                int uniformGridIndex = int.Parse(uniformGrid.Name.Replace($"{CommonStyle}", "")); // 获取UniformGrid索引
                foreach (var button in CommonButtonPanel.Children.OfType<Button>()) // 遍历所有按钮，重置颜色
                {
                    button.Background = button.Name.Contains($"{uniformGridIndex}") // 判断是否是当前按钮
                        ? SelectedBrush
                        : UnSelectedBrush; // 设置当前按钮颜色
                } // 设置所有按钮的颜色
            }
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
            buttonManager.OpenMenu(sender, true, "SelectActionPageMenu", this, GetButtonType(sender)); // 打开菜单
        }

        /// <summary>
        /// 获取按钮类型
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private string GetButtonType(object sender)
        {
            Button button = sender as Button; // 获取按钮
            return button.Name.StartsWith("Global") ? "Global" : CommonStyle; // 获取按钮类型
        }

        // 点击按钮更新Quicker
        private void UpdateQuicker(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new(); // 创建更新窗口
            updateWindow.Show(); // 显示窗口
        }

        // 窗口关闭时强制垃圾回收
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的关闭事件
            cancellationTokenSource.Cancel(); // 取消所有后台任务
            cancellationTokenSource.Dispose();
            CleanUpEventHandlers(); // 清理事件处理器
            CleanUpUniformGrid(MainGrid); // 清理全局网格
            CleanUpUniformGrid(CommonGrid); // 清理通用网格
            Book.Source = null; // 订住按钮图片
            Lock.Source = null; // 锁定按钮图片

            iconManager.Dispose(); // 释放图标管理器资源
            buttonManager.Dispose(); // 释放按钮管理器资源

            GC.Collect(); // 强制回收内存
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制回收内存
        }

        /// <summary>
        /// 清理指定Grid中的所有UniformGrid及其子元素
        /// </summary>
        /// <param name="grid">要清理的Grid</param>
        private void CleanUpUniformGrid(Grid grid)
        {
            foreach (UniformGrid uniformGrid in buttonManager.FindVisualChildren<UniformGrid>(grid))
            {
                foreach (Button button in buttonManager.FindVisualChildren<Button>(uniformGrid))
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

                // 移除UniformGrid事件
                uniformGrid.IsVisibleChanged -= GlobalUniformGrid_IsVisibleChanged;
                uniformGrid.IsVisibleChanged -= CommonUniformGrid_IsVisibleChanged;

                uniformGrid.Children.Clear(); // 清空UniformGrid
            }
            grid.Children.Clear(); // 清空Grid
        }

        // 清理所有动态添加的事件处理器
        private void CleanUpEventHandlers()
        {
            foreach (Button button in GlobalButtonPanel.Children.OfType<Button>()) // 清理全局按钮面板事件
            {
                button.Click -= SwitchToGlobalUniformGrid;
                button.MouseEnter -= GlobalActionPageChangeButton_MouseEnter;
                button.MouseLeave -= GlobalActionPageChangeButton_MouseLeave;
            }

            foreach (Button button in CommonButtonPanel.Children.OfType<Button>()) // 清理公共按钮面板事件
            {
                button.Click -= SwitchToCommonUniformGrid;
                button.MouseEnter -= CommonActionPageChangeButton_MouseEnter;
                button.MouseLeave -= CommonActionPageChangeButton_MouseLeave;
            }
        }
    }
}