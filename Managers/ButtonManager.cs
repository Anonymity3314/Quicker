using Quicker.Windows.MainWindows.MainWindow;
using System.Windows.Media.Imaging;
using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using Quicker.Models;
using System.IO;

namespace Quicker.Managers
{
    public class ButtonManager
    {
        private readonly SolidColorBrush HasActionBrush =
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("White")); // 有动作的背景色
        private readonly SolidColorBrush NoActionBrush =
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F3F3")); // 无动作的背景色

        /// <summary>
        /// 查找所有指定类型的子元素
        /// </summary>
        /// <typeparam name="T"> 目标类型 </typeparam>
        /// <param name="obj"> 目标对象 </param>
        /// <returns> 所有指定类型的子元素 </returns>
        public IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
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
        private readonly IconManager iconManager = new(); // 图标管理器
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private Point initialMousePosition; // 鼠标初始位置
        private string SourceTableName; // 数据库表名
        private Button SourceButton; // 源按钮

        public ButtonManager()
        {
            var Convention = SettingDatabase.GetAllConventions().FirstOrDefault(); // 获取所有约定
            shouldHideTooltip = Convention.HideTooltip; // 获取隐藏提示标志
        }

        // 设置按钮拖拽时的效果
        public void Button_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true; // 标记事件已处理
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 如果拖拽的是文件
                e.Effects = DragDropEffects.Copy; // 设置拖拽效果为复制
            else if (e.Data.GetDataPresent(typeof(ButtonData))) // 如果拖拽的是按钮
                e.Effects = DragDropEffects.Move; // 设置拖拽效果为移动
        }

        /// <summary>
        /// 处理文件拖拽到按钮上
        /// </summary>
        /// <param name="sender">目标按钮</param>
        /// <param name="e">拖拽事件参数</param>
        /// <param name="isMainWindow">是否为主窗口</param>
        public void Button_Drop(object sender, DragEventArgs e, bool isMainWindow, string tableName)
        {
            Button TargetButton = sender as Button; // 获取目标按钮
            if (TargetButton == SourceButton) return; // 如果目标按钮和源按钮相同，直接返回
            
            if (e.Data.GetDataPresent(typeof(ButtonData))) // 如果拖拽的是按钮
                ProcessButtonDrop(TargetButton, isMainWindow, SourceTableName, tableName); // 处理按钮拖拽
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 如果拖拽的是文件
                ProcessFileDrop(e, TargetButton, isMainWindow, tableName); // 处理文件拖拽
            else if (e.Data.GetDataPresent(DataFormats.Text)) // 如果拖拽的是文本（可能是 URL） 
                ProcessTextDrop(e, TargetButton, isMainWindow, tableName); // 处理文本拖拽
        }

        /// <summary>
        /// 处理文本拖拽
        /// </summary>
        /// <param name="e">拖拽事件参数</param>
        /// <param name="targetButton">目标按钮</param>
        /// <param name="isMainWindow">是否为主窗口</param>
        /// <param name="tableName">数据库表名</param>
        private void ProcessTextDrop(DragEventArgs e, Button targetButton, bool isMainWindow, string tableName)
        {
            string text = (string)e.Data.GetData(DataFormats.Text).ToString();
            if (Uri.TryCreate(text, UriKind.Absolute, out Uri url))
            {
                ProcessUrlDrop(url.ToString(), int.Parse(targetButton.Name.Replace(tableName, "")), tableName, isMainWindow);
            }
        }

        /// <summary>
        /// 处理按钮拖拽到其他按钮上
        /// </summary>
        /// <param name="targetButton"> 目标按钮 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        /// <param name="tableName1"> 原表名 </param>
        /// <param name="tableName2"> 目标表名 </param>
        private void ProcessButtonDrop(Button targetButton, bool isMainWindow, string tableName1, string tableName2)
        {
            if (SourceButton == null) return; // 如果源按钮为空，直接返回
            int buttonID1 = int.Parse(SourceButton.Name.Replace(tableName1, "")); // 获取源按钮ID
            int buttonID2 = int.Parse(targetButton.Name.Replace(tableName2, "")); // 获取目标按钮ID
            db2.ExchangeButtonID(buttonID1, buttonID2, tableName1, tableName2); // 交换按钮编号
            var sourceData = SourceButton.Tag as ButtonData; // 获取源按钮数据
            var targetData = targetButton.Tag as ButtonData; // 获取目标按钮数据

            if(!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) // 如果没有按住 Ctrl 键
                RefreshButtonDisplay(SourceButton, targetData, 60, isMainWindow); // 更新 sourceButton 的内容
            RefreshButtonDisplay(targetButton, sourceData, 60, isMainWindow); // 更新 targetButton 的内容
            SourceButton = null; // 清空源按钮
        }

        /// <summary>
        /// 处理文件拖拽
        /// </summary>
        /// <param name="e"> 拖拽事件参数 </param>
        /// <param name="TargetButton"> 目标按钮 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessFileDrop(DragEventArgs e, Button TargetButton, bool isMainWindow, string tableName)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // 获取文件路径
            if (files.Length <= 0) return; // 如果没有文件，直接返回

            if (files.Length == 1) // 如果只有一个文件
                ProcessSingleFileDrop(TargetButton, files[0], isMainWindow, tableName); // 处理单个文件拖拽
            else // 如果有多于一个文件
                ProcessMultipleFileDrop(TargetButton, files, isMainWindow, tableName); // 处理多个文件拖拽

            UpdateButtonDisplay(TargetButton, tableName, isMainWindow); // 更新按钮显示 
        }

        /// <summary>
        /// 更新按钮显示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="tableName"> 数据库表名 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void UpdateButtonDisplay(Button button, string tableName, bool isMainWindow)
        {
            var buttonData = db2.GetButtonDataByID(int.Parse(button.Name), tableName); // 获取按钮数据
            RefreshButtonDisplay(button, buttonData, 60, isMainWindow); // 刷新按钮显示
        }

        /// <summary>
        /// 处理单个文件拖拽
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="filePath"> 文件路径 </param>
        private void ProcessSingleFileDrop(Button button, string filePath, bool isMainWindow, string tableName)
        {
            if (IsImaege(filePath)) // 如果是图片文件
            {
                ProcessImageDrop(filePath, int.Parse(button.Name.Replace(tableName,"")), tableName); // 处理图片拖拽
                return; // 直接返回
            }
            else if (Path.GetExtension(filePath).ToLower() == ".url") // 判断是否为 .url 文件
            {
                ProcessUrlShortcutDrop(filePath, int.Parse(button.Name.Replace(tableName,"")), tableName);
                return;
            }

            ImageSource iconSource = iconManager.GetIcon(filePath); // 获取图标
            string iconPath = ""; // 默认图标路径
            if (iconSource != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(filePath); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(iconSource); // 保存图标到文件
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = int.Parse(button.Name.Replace(tableName,"")), // 获取按钮ID
                Title = fileName, // 设置按钮名称
                Location = filePath, // 设置文件路径
                ImagePath = iconPath, // 设置图标路径
                Data1 = false.ToString(), // 是否使用管理员身份运行
                Data2 = true.ToString(), // 尝试打开已存在的窗口
                Data3 = 0.ToString(), // 设置窗口状态
                Description = $"打开文件: {fileName}", // 设置用途
                CreateTime = DateTime.Now,
                ActionType = "OpenFile", // 设置动作类型
            }; // 设置按钮数据
            RefreshButtonDisplay(button, buttonData, 60, isMainWindow); // 刷新按钮
            db2.UpdateAction(buttonData, tableName); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 判断是否为图片文件
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> 是否为图片文件 </returns>
        private bool IsImaege(string filePath)
        {
            string extension = Path.GetExtension(filePath); // 获取文件地址
            return extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".gif"; // 判断是否为图片文件
        }

        /// <summary>
        /// 处理图片拖拽
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <param name="buttonID"> 按钮ID </param>
        /// <param name="tableName"> 表名 </param>
        private void ProcessImageDrop(string filePath, int buttonID, string tableName)
        {
            BitmapImage bitmap = new BitmapImage(new Uri(filePath)); // 创建 BitmapImage 对象
            string iconPath = ""; // 默认图标路径
            if (bitmap != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(filePath); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(bitmap); // 保存图标到文件
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = buttonID,
                Title = fileName,
                Location = filePath,
                ImagePath = iconPath,
                Data1 = false.ToString(), // 是否使用管理员身份运行
                Data2 = true.ToString(), // 尝试打开已存在的窗口
                Data3 = 0.ToString(),
                Description = $"打开图片: {fileName}",
                CreateTime = DateTime.Now,
                ActionType = "OpenFile",
            };
            db2.UpdateAction(buttonData, tableName); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 处理 Internet 快捷方式文件拖拽
        /// </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <param name="buttonID"> 按钮ID </param>
        /// <param name="tableName"> 表名 </param>
        private void ProcessUrlShortcutDrop(string filePath, int buttonID, string tableName)
        {
            string url = File.ReadAllText(filePath); // 读取 .url 文件内容以获取实际的 URL
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.StartsWith("URL="))
                {
                    url = line.Substring("URL=".Length).Trim();
                    break;
                }
            }
            ProcessUrlDrop(url, buttonID, tableName, true); // 调用处理 URL 的方法
        }

        /// <summary>
        /// 处理多个文件拖拽
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="filePaths"> 文件路径 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        /// <param name="tableName"> 数据库表名 </param>
        private void ProcessMultipleFileDrop(Button button, string[] filePaths, bool isMainWindow, string tableName)
        {
            string filePath = string.Join(";", filePaths); // 用" ; "分隔文件路径
            ImageSource iconSource = iconManager.GetIcon(filePaths[0]); // 获取图标
            string iconPath = ""; // 默认图标路径
            if (iconSource != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(filePaths[0]); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(iconSource); // 保存图标到文件
            }

            string fileName = Path.GetFileNameWithoutExtension(filePaths[0]); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = int.Parse(button.Name.Replace(tableName,"")), // 获取按钮ID
                Title = $"{fileName}等{filePaths.Length}个文件(夹)", // 设置按钮名称
                Location = filePath, // 设置文件路径
                ImagePath = iconPath, // 设置图标路径
                Data1 = false.ToString(), // 是否使用管理员身份运行
                Data2 = true.ToString(), // 尝试打开已存在的窗口
                Data3 = 0.ToString(), // 设置窗口状态
                Description = $"打开以{fileName}为首的多个文件:", // 设置用途
                CreateTime = DateTime.Now,
                ActionType = "OpenFiles", // 设置动作类型
            }; // 设置按钮数据
            RefreshButtonDisplay(button, buttonData, 60, isMainWindow); // 刷新按钮
            db2.UpdateAction(buttonData, tableName); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 处理 URL 拖拽
        /// </summary>
        /// <param name="url"> URL 地址 </param>
        /// <param name="buttonID"> 按钮ID </param>
        /// <param name="tableName"> 表名 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessUrlDrop(string url, int buttonID, string tableName, bool isMainWindow)
        {
            ImageSource iconSource = iconManager.GetWebsiteIcon(url); // 获取图标
            string iconPath = ""; // 默认图标路径
            if (iconSource != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(url); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(iconSource); // 保存图标到文件
            }

            ButtonData buttonData = new ButtonData
            {
                ButtonID = buttonID,
                Title = GetWebsiteNameFromUrl(url),
                Location = url,
                ImagePath = iconPath,
                Data3 = 0.ToString(),
                Description = $"打开网页: {url}",
                CreateTime = DateTime.Now,
                ActionType = "OpenWebsite",
            }; // 设置按钮数据
            db2.UpdateAction(buttonData, tableName); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 从URL中获取网站名称
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>网站名称</returns>
        public string GetWebsiteNameFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }

        /// <summary>
        /// 刷新按钮显示内容
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="maxWidth"> 最大宽度 </param>
        public void RefreshButtonDisplay(Button button, ButtonData buttonInformation, int maxWidth, bool isMainWindow)
        {
            if (buttonInformation == null || buttonInformation.Location == null)
            {
                ResetButtonDisplay(button); // 重置按钮显示
                return; // 直接返回
            }

            button.Tag = buttonInformation; // 设置按钮标签
            var grid = CreateButtonGrid(buttonInformation, maxWidth, isMainWindow); // 创建按钮网格
            button.Content = grid; // 设置按钮内容
            LoadActionTooltip(button, buttonInformation); // 加载动作提示
        }

        /// <summary>
        /// 重置按钮显示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        private void ResetButtonDisplay(Button button)
        {
            button.Content = null; // 清空按钮内容
            button.ToolTip = null; // 清空提示文本
            button.Tag = null; // 清空标签
            button.Background = NoActionBrush; // 设置背景色
        }

        /// <summary>
        /// 创建按钮网格
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="maxWidth"> 最大宽度 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private Grid CreateButtonGrid(ButtonData buttonInformation, int maxWidth, bool isMainWindow)
        {
            Grid grid = new(); // 创建网格
            if (!string.IsNullOrEmpty(buttonInformation.ImagePath)) // 如果图像路径不为空
            {
                AddImageToGrid(grid, buttonInformation, isMainWindow); // 添加图像到网格
            }

            if (!string.IsNullOrEmpty(buttonInformation.Title)) // 如果标题不为空
            {
                AddTitleToGrid(grid, buttonInformation, maxWidth); // 添加标题到网格
            }

            return grid; // 返回网格
        }

        /// <summary>
        /// 添加图像到网格
        /// </summary>
        /// <param name="grid"> 目标网格 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void AddImageToGrid(Grid grid, ButtonData buttonInformation, bool isMainWindow)
        {
            try
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                Image image = LoadActionIcon(buttonInformation, isMainWindow); // 加载动作图标
                grid.Children.Add(image); // 添加图像到网格
                Grid.SetRow(image, 0); // 设置图像行
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.Show($"图标加载失败：动作{buttonInformation.Title}的图标被移动或删除", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 添加标题到网格
        /// </summary>
        /// <param name="grid"> 目标网格 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="maxWidth"> 最大宽度 </param>
        private void AddTitleToGrid(Grid grid, ButtonData buttonInformation, int maxWidth)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
            TextBlock textBlock = LoadActionTitle(buttonInformation, maxWidth); // 加载动作标题
            grid.Children.Add(textBlock); // 添加标题到网格
            Grid.SetRow(textBlock, 1); // 设置标题行
        }

        /// <summary>
        /// 加载动作图标
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        /// <returns> 图像对象 </returns>
        private Image LoadActionIcon(ButtonData buttonInformation, bool isMainWindow)
        {
            double buttonSize = isMainWindow ? 77.6 : 65;
            double imageSize = buttonSize / 2.0;
            Image image = new()
            {
                Width = imageSize,
                Height = imageSize,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Source = new BitmapImage(new Uri(buttonInformation.ImagePath))
            };
            return image;
        }

        /// <summary>
        /// 加载动作名称
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="maxWidth"> 最大宽度 </param>
        /// <returns> 文本块对象 </returns>
        private TextBlock LoadActionTitle(ButtonData buttonInformation, int maxWidth)
        {
            TextBlock textBlock = new()
            {
                Text = buttonInformation.Title, // 设置文本
                HorizontalAlignment = HorizontalAlignment.Center, // 水平居中
            }; // 创建文本块对象
            AutoEllipsisTextBlock(textBlock, maxWidth); // 动态调整字体大小
            return textBlock; // 返回文本块对象
        }

        /// <summary>
        /// 加载动作提示
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="buttonInformation"> 按钮数据 </param>
        private void LoadActionTooltip(Button button, ButtonData buttonInformation)
        {
            if (!shouldHideTooltip)
            {
                string toolTipText = null; // 提示文本
                if (!string.IsNullOrWhiteSpace(buttonInformation.Title) || !string.IsNullOrWhiteSpace(buttonInformation.Description))
                {
                    string name = !string.IsNullOrWhiteSpace(buttonInformation.Title) ? buttonInformation.Title : null; // 获取按钮名称
                    string usage = !string.IsNullOrWhiteSpace(buttonInformation.Description) ? buttonInformation.Description : null; // 获取按钮用途
                    toolTipText = (name + "\n" + usage).Trim('\n'); // 设置按钮提示文本
                } // 如果按钮名称或用途不为空
                button.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置按钮提示文本
            }
        }

        /// <summary>
        /// 加载动作使用次数
        /// </summary>
        /// <param name="data"> 按钮数据 </param>
        public void LoadActionUsedTimes(Button button, ButtonData data)
        {
            button.Content = null; // 清空按钮内容
            TextBlock textBlock = new()
            {
                FontSize = 11, // 设置字体大小
                TextAlignment = TextAlignment.Center, // 设置文本对齐方式
                Text = "使用次数" + "\n" + data.UsedTimes.ToString() // 设置文本
            }; // 创建文本块对象
            button.Content = textBlock; // 设置按钮内容
        }

        // 鼠标左键按下时记录初始位置
        public void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button; // 获取目标按钮
            initialMousePosition = e.GetPosition(button); // 记录初始位置
            SourceButton = button; // 记录源按钮
            isDragging = false; // 初始化拖拽状态
        }

        /// <summary>
        /// 鼠标移动时检查是否满足拖拽条件
        /// </summary>
        /// <param name="sender"> 目标按钮 </param>
        /// <param name="e"> 鼠标事件参数 </param>
        /// <param name="isMainButton"> 是否为主按钮 </param>
        public void Button_PreviewMouseMove(object sender, MouseEventArgs e, bool isMainButton, string sourceTableName = null)
        {
            SourceTableName = sourceTableName; // 记录源表名
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(button); // 获取当前位置
                double deltaX = currentPosition.X - initialMousePosition.X; // 计算 X 轴位移
                double deltaY = currentPosition.Y - initialMousePosition.Y; // 计算 Y 轴位移
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY); // 计算移动距离
                if (distance > 10 && !isDragging) // 如果移动距离超过 10 像素，则视为拖拽开始
                {
                    isDragging = true; // 设置拖拽状态
                    if(isMainButton)
                    {
                        if (button.Tag is ButtonData data) // 如果是主按钮
                            DragDrop.DoDragDrop(button, data, DragDropEffects.Move); // 开始拖拽操作
                    }
                    else
                    {
                        string buttonName = button.Name; // 获取按钮名称
                        DataObject data = new DataObject();// 创建数据对象
                        data.SetData("ButtonData", buttonName); // 传递 Button 的 Name
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
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
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
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); // 测量文本块的大小
                double newWidth = textBlock.DesiredSize.Width; // 获取新文本宽度
                if (newWidth <= maxWidth) break; // 如果新文本宽度小于等于最大宽度，退出循环
            }
            textBlock.Text = truncatedText + ellipsis; // 更新 TextBlock 的文本
        }

        /// <summary>
        /// 打开指定菜单
        /// </summary>
        /// <param name="sender"> 触发菜单的按钮 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        /// <param name="targetMenu"> 要打开的菜单名称 </param>
        /// <param name="sourceWindow"> 触发菜单的窗口 </param>
        public void OpenMenu(object sender, bool isMainWindow, string targetMenu, Window sourceWindow, string tableName)
        {
            Window menu = null; // 菜单窗口
            Button button = sender as Button; // 获取触发菜单的按钮
            Point mousePosition = Mouse.GetPosition(sourceWindow); // 获取鼠标位置
            if (isMainWindow) isClosing = true; // 如果是主窗口，设置关闭标志
            switch (targetMenu)
            {
                case "OperationMenu":
                    OperationMenu operationMenu = Application.Current.Windows.OfType<OperationMenu>().FirstOrDefault(); // 查找现有的操作菜单
                    operationMenu?.Close(); // 关闭操作菜单
                    operationMenu = new(int.Parse(button.Name.Replace(tableName, "")), tableName); // 设置菜单位置
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
                    creatActionMenu = new(int.Parse(button.Name.Replace(tableName, "")), tableName); // 设置菜单位置
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
                        Left = mousePosition.X,
                        Top = mousePosition.Y
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
        /// <param name="window"> 要关闭面板窗口的窗口 </param>
        public void CloseMainWindow(Window window)
        {
            if (Application.Current == null) return; // 如果当前应用程序不存在，直接返回
            var windowList = Application.Current.Windows.OfType<MainWindow>(); // 查找所有主窗口
            foreach (var w in windowList)
                w.Close(); // 关闭所有主窗口
            window.Close(); // 关闭面板窗口
        }

        /// <summary>
        /// 隐藏面板窗口
        /// </summary>
        /// <param name="window"> 要隐藏面板窗口的窗口 </param>
        public void HideMainWindow(Window window)
        {
            var windowList = Application.Current.Windows.OfType<MainWindow>(); // 查找所有主窗口
            foreach (var w in windowList)
                w.Visibility = Visibility.Hidden; // 关闭所有主窗口
            window.Visibility = Visibility.Hidden; // 关闭面板窗口
        }

        /// <summary>
        /// 处理地址文本内容
        /// </summary>
        /// <param name="location"> 地址文本内容 </param>
        public string ProcessLocation(string location)
        {
            if (location.StartsWith("\"") && location.EndsWith("\"")) // 如果文本含有""符号，则去掉
                return location = location.Substring(1, location.Length - 2);
            return location; // 返回处理后的地址文本内容
        }

        /// <summary>
        /// 通过拖拽删除动作
        /// </summary>
        /// <param name="tableName"> 按钮类型 </param>
        /// <param name="mainWindow"> 主窗口 </param>
        public void DeleteActionByDrag(string tableName, MainWindow mainWindow)
        {
            if (SourceButton == null) return; // 如果按钮不存在，直接返回
            int buttonID = int.Parse(SourceButton.Name.Replace($"{tableName}", "")); // 获取按钮ID
            db2.DeleteAction(buttonID, tableName); // 删除动作

            mainWindow.UpdateButtonContent(buttonID, tableName); // 更新按钮内容
        }

        // 手动释放资源
        public void Dispose()
        {
            iconManager?.Dispose(); // 清理图标管理器资源
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }

        /// <summary>
        /// 从剪贴板创建文件动作
        /// </summary>
        /// <param name="buttonID">按钮ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>是否成功创建动作</returns>
        public bool CreateFileActionFromClipboard(int buttonID, string tableName)
        {
            if (!Clipboard.ContainsFileDropList()) return false;

            var fileList = Clipboard.GetFileDropList(); // 获取文件列表
            if (fileList.Count <= 0) return false;

            string filePath = fileList[0]; // 获取第一个文件路径
            if (IsImaege(filePath)) // 如果是图片文件
            {
                ProcessImageDrop(filePath, buttonID, tableName); // 处理图片拖拽
                return true;
            }
            else if (Path.GetExtension(filePath).ToLower() == ".url") // 判断是否为 .url 文件
            {
                ProcessUrlShortcutDrop(filePath, buttonID, tableName);
                return true;
            }

            ImageSource iconSource = iconManager.GetIcon(filePath); // 获取图标
            string iconPath = ""; // 默认图标路径
            if (iconSource != null) // 如果图标存在
            {
                iconPath = iconManager.CheckCachedIcon(filePath); // 检查已经保存的图标
                if (string.IsNullOrEmpty(iconPath)) // 如果不存在保存的图标
                    iconPath = iconManager.SaveIconToFile(iconSource); // 保存图标到文件
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath); // 获取文件名
            ButtonData buttonData = new ButtonData
            {
                ButtonID = buttonID, // 使用当前按钮ID
                Title = fileName, // 设置按钮名称
                Location = filePath, // 设置文件路径
                ImagePath = iconPath, // 设置图标路径
                Data1 = false.ToString(), // 是否使用管理员身份运行
                Data2 = true.ToString(), // 尝试打开已存在的窗口
                Data3 = 0.ToString(), // 设置窗口状态
                Description = $"打开文件: {fileName}", // 设置用途
                CreateTime = DateTime.Now,
                ActionType = "OpenFile", // 设置动作类型
            }; // 设置按钮数据
            db2.UpdateAction(buttonData, tableName); // 添加按钮数据到数据库
            return true;
        }
    }
}