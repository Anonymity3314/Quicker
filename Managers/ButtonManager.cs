using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.Windows.Menus;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using System.IO;

namespace Quicker.Managers
{
    public class ButtonManager
    {
        private static readonly SolidColorBrush HasActionBrush =
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("White"));
        private static readonly SolidColorBrush NoActionBrush =
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F3F3"));

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
        public void Button_Drop(object sender, DragEventArgs e, bool isMainWindow)
        {
            Button TargetButton = sender as Button; // 获取目标按钮
            if (TargetButton == SourceButton) return; // 如果目标按钮和源按钮相同，直接返回
            if (e.Data.GetDataPresent(typeof(ButtonData))) // 如果拖拽的是按钮
            {
                ProcessButtonDrop(TargetButton, isMainWindow); // 处理按钮拖拽
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) // 如果拖拽的是文件
            {
                ProcessFileDrop(e, TargetButton, isMainWindow); // 处理文件拖拽
            }
            else if (e.Data.GetDataPresent(DataFormats.Text)) // 如果拖拽的是文本（可能是 URL）
            {
                string text = (string)e.Data.GetData(DataFormats.Text).ToString(); // 获取文本
                if (Uri.TryCreate(text, UriKind.Absolute, out Uri url))
                    ProcessUrlDrop(TargetButton, url.ToString(), isMainWindow); // 处理 URL 拖拽
            }
        }

        /// <summary>
        /// 处理按钮拖拽到其他按钮上
        /// </summary>
        /// <param name="TargetButton"> 目标按钮 </param>
        /// <param name="buttonData"> 按钮数据 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessButtonDrop(Button TargetButton, bool isMainWindow)
        {
            if (SourceButton == null) return; // 如果源按钮为空，直接返回
            db2.ExchangeButtonID(SourceButton.Name, TargetButton.Name); // 交换按钮编号
            var SourceData = SourceButton.Tag as ButtonData; // 获取源按钮数据
            var TargetData = TargetButton.Tag as ButtonData; // 获取目标按钮数据

            RefreshButtonDisplay(SourceButton, TargetData, 60, isMainWindow); // 更新 sourceButton 的内容
            RefreshButtonDisplay(TargetButton, SourceData, 60, isMainWindow); // 更新 targetButton 的内容
            SourceButton = null; // 清空源按钮
        }

        /// <summary>
        /// 处理文件拖拽
        /// </summary>
        /// <param name="e"> 拖拽事件参数 </param>
        /// <param name="TargetButton"> 目标按钮 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessFileDrop(DragEventArgs e, Button TargetButton, bool isMainWindow)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop); // 获取文件路径
            if (filePaths.Length <= 0) return; // 如果没有文件，直接返回
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // 获取文件路径

            if (files.Length == 1) // 如果只有一个文件
                ProcessSingleFileDrop(TargetButton, files[0], isMainWindow); // 处理文件拖拽
            else // 如果有多个文件
                ProcessMultipleFileDrop(TargetButton, files, isMainWindow); // 处理多个文件拖拽

            var TargetData = db2.GetButtonDataByID(TargetButton.Name); // 获取目标按钮数据
            RefreshButtonDisplay(TargetButton, TargetData, 60, isMainWindow); // 更新目标按钮的内容
        }

        /// <summary>
        /// 处理单个文件拖拽
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="filePath"> 文件路径 </param>
        private void ProcessSingleFileDrop(Button button, string filePath, bool isMainWindow)
        {
            if (IsImaege(filePath)) // 如果是图片文件
            {
                ProcessImageDrop(filePath, button, isMainWindow); // 处理图片拖拽
                return; // 直接返回
            }
            else if (Path.GetExtension(filePath).ToLower() == ".url") // 判断是否为 .url 文件
            {
                ProcessUrlShortcutDrop(button, filePath, isMainWindow);
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
                ButtonID = button.Name, // 获取按钮ID
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
            db2.UpdateAction(buttonData); // 添加按钮数据到数据库
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
        /// <param name="e"> 拖拽事件参数 </param>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessImageDrop(string filePath, Button button, bool isMainWindow)
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
                ButtonID = button.Name,
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
            RefreshButtonDisplay(button, buttonData, 60, isMainWindow); // 刷新按钮
            db2.UpdateAction(buttonData); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 处理 Internet 快捷方式文件拖拽
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="filePath"> 文件路径 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessUrlShortcutDrop(Button button, string filePath, bool isMainWindow)
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
            ProcessUrlDrop(button, url, isMainWindow, filePath); // 调用处理 URL 的方法
        }

        /// <summary>
        /// 处理多个文件拖拽
        /// </summary>
        /// <param name="button"></param>
        /// <param name="filePaths"></param>
        /// <param name="isMainWindow"></param>
        private void ProcessMultipleFileDrop(Button button, string[] filePaths, bool isMainWindow)
        {
            // 用“；”分隔文件路径
            string filePath = string.Join(";", filePaths);
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
                ButtonID = button.Name, // 获取按钮ID
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
            db2.UpdateAction(buttonData); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 处理 URL 拖拽
        /// </summary>
        /// <param name="button"> 目标按钮 </param>
        /// <param name="url"> URL 地址 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        private void ProcessUrlDrop(Button button, string url, bool isMainWindow, string filePath = null)
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
                ButtonID = button.Name,
                Title = filePath == null ? GetWebsiteNameFromUrl(url) : Path.GetFileNameWithoutExtension(filePath),
                Location = url,
                ImagePath = iconPath,
                Data3 = 0.ToString(),
                Description = $"打开网页: {url}",
                CreateTime = DateTime.Now,
                ActionType = "OpenWebsite",
            }; // 设置按钮数据
            RefreshButtonDisplay(button, buttonData, 60, isMainWindow); // 刷新按钮
            db2.UpdateAction(buttonData); // 添加按钮数据到数据库
        }

        /// <summary>
        /// 获取网站名称
        /// </summary>
        /// <param name="url"> 网站地址 </param>
        /// <returns> 网站名称 </returns>
        public string GetWebsiteNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url); // 尝试解析URL
                string host = uri.Host; // 获取主机名
                if (host.Contains('.')) // 如果主机名包含子域名，则只保留顶级域名
                {
                    string[] parts = host.Split('.');
                    if (parts.Length > 1)
                    {
                        // 移除常见的顶级域名后缀
                        string[] commonTlds = { "com", "cn", "net", "org", "gov", "edu", "info", "biz", "co", "me", "io", "app" };
                        bool hasCommonTld = false; // 是否包含常见的顶级域名后缀

                        foreach (string tld in commonTlds)
                        {
                            if (parts[parts.Length - 1].Equals(tld, StringComparison.OrdinalIgnoreCase))
                            {
                                hasCommonTld = true; // 发现常见的顶级域名后缀
                                break;
                            }
                        }

                        if (hasCommonTld) // 如果包含常见的顶级域名后缀
                        {
                            if (parts.Length > 2)
                                host = string.Join(".", parts, parts.Length - 2, 2); // 保留二级域名
                            else
                                host = parts[0]; // 如果只有顶级域名，如 example.com
                        }
                        else
                            host = parts[0]; // 如果不包含常见的顶级域名后缀，则取第一个部分
                    }
                }

                return host;
            }
            catch
            {
                ToastManager.AddToast("无效的URI：未能解析主机名。", "Error"); // 处理无效的URL
                return ""; // 返回空字符
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
            if (buttonInformation != null) // 如果Button的数据存在
            {
                if(buttonInformation.Location == null) return; // 如果文件路径不存在，直接返回
                button.Tag = buttonInformation; // 更新按钮标签
                button.Background = HasActionBrush; // 设置按钮背景

                Grid grid = new(); // 创建Grid对象
                if (!string.IsNullOrEmpty(buttonInformation.ImagePath))
                {
                    try
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                        Image image = LoadActionIcon(buttonInformation, isMainWindow); // 创建图像对象
                        grid.Children.Add(image); // 添加图像到Grid
                        Grid.SetRow(image, 0); // 设置图像所在行
                    }
                    catch // 如果失败，发送信息提示
                    {
                        ToastManager.AddToast($"图标加载失败：按钮{buttonInformation.Title}的图标被移动或删除", "Error");
                    }
                } // 如果图标路径不为空

                if (!string.IsNullOrEmpty(buttonInformation.Title))
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 添加行定义
                    TextBlock textBlock = LoadActionTitle(buttonInformation, maxWidth); // 创建文本块对象
                    grid.Children.Add(textBlock); // 添加文本块到Grid
                    Grid.SetRow(textBlock, 1); // 设置文本块所在行
                } // 如果按钮名称不为空
                button.Content = grid; // 设置按钮内容

                LoadActionTooltip(button, buttonInformation); // 加载动作提示
            }
            else // 如果Button的数据不存在
            {
                button.Content = null; // 清空按钮内容
                button.ToolTip = null; // 清空按钮提示文本
                button.Tag = null; // 清空按钮标签
                button.Background = NoActionBrush; // 重置按钮背景
            }
        }

        /// <summary>
        /// 加载动作图标
        /// </summary>
        /// <param name="buttonInformation"> 按钮数据 </param>
        /// <param name="isMainWindow"> 是否为主窗口 </param>
        /// <returns> 图像对象 </returns>
        private Image LoadActionIcon(ButtonData buttonInformation, bool isMainWindow)
        {
            Image image = new()
            {
                Width = isMainWindow ? 36 : 30, // 设置宽度
                Height = isMainWindow ? 36 : 30, // 设置高度
                VerticalAlignment = VerticalAlignment.Center, // 垂直居中
                HorizontalAlignment = HorizontalAlignment.Center, // 水平居中
                Source = new BitmapImage(new Uri(buttonInformation.ImagePath)) // 设置图像源
            }; // 创建图像对象
            return image; // 返回图像对象
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
                TextWrapping = TextWrapping.NoWrap, // 设置文本换行方式
                VerticalAlignment = VerticalAlignment.Center, // 垂直居中
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
        public void Button_PreviewMouseMove(object sender, MouseEventArgs e, bool isMainButton)
        {
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
        public void OpenMenu(object sender, bool isMainWindow, string targetMenu, Window sourceWindow)
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
                    operationMenu = new(button.Name)
                    {
                        Left = mousePosition.X,
                        Top = mousePosition.Y
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
                        Left = mousePosition.X,
                        Top = mousePosition.Y
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
            if (location.StartsWith("\"") && location.EndsWith("\"")) // 如果文本含有“”符号，则去掉
                return location = location.Substring(1, location.Length - 2);
            return location; // 返回处理后的地址文本内容
        }

        // 手动释放资源
        public void Dispose()
        {
            iconManager?.Dispose(); // 清理图标管理器资源
            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}