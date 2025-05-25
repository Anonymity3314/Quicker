using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace Quicker.Windows
{
    public partial class SelectImageWindow : System.Windows.Window
    {
        public ObservableCollection<ImageItem> ImageItems { get; set; } = new ObservableCollection<ImageItem>(); // 图片项集合
        public string SelectedImagePath { get; private set; } = string.Empty; // 选中的图片路径
        public event EventHandler<string> ImageConfirmed; // 图片确认事件
        private ScrollViewer listViewScrollViewer; // 图片列表的 ScrollViewer

        public SelectImageWindow()
        {
            InitializeComponent();
        }

        // 窗口加载事件
        private void SelectImageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ImageListView.ItemsSource = ImageItems; // 设置图片列表的数据源
            LoadImagesFromFolder(); // 从文件夹加载图片
            listViewScrollViewer = FindScrollViewer(ImageListView); // 查找 ScrollViewer
            var itemsPanel = FindVisualChild<WrapPanel>(ImageListView); // 查找 WrapPanel            
            listViewScrollViewer.Content = itemsPanel; // 设置 ScrollViewer 的内容为 WrapPanel
            UpdateScrollBarProperties(); // 初始化滚动条               
        }

        // 更新滚动条的属性
        private void UpdateScrollBarProperties()
        {
            if (listViewScrollViewer == null || VerticalScrollBar == null) return; // 如果 ScrollViewer 或 VerticalScrollBar 为 null，返回
            var itemsPanel = FindVisualChild<WrapPanel>(ImageListView); // 查找 WrapPanel
            if (itemsPanel == null) return;// 计算每行显示的项数

            double contentHeight = 0; // WrapPanel 的内容高度
            int itemsPerRow = CalculateItemsPerRow(itemsPanel); // 计算每行显示的项数
            for (int i = 0; i < itemsPanel.Children.Count; i++)
            {
                var child = itemsPanel.Children[i]; // 获取子元素
                if (child is FrameworkElement element) // 如果子元素是 FrameworkElement
                    contentHeight = Math.Max(contentHeight, element.DesiredSize.Height); // 计算 WrapPanel 的内容高度
            }
            contentHeight *= Math.Ceiling((double)itemsPanel.Children.Count / itemsPerRow); // 计算 WrapPanel 的内容高度

            itemsPanel.Height = contentHeight; // 确保 WrapPanel 的高度正确

            VerticalScrollBar.Maximum = contentHeight; // 设置滚动条的最大值
            VerticalScrollBar.ViewportSize = listViewScrollViewer.ViewportHeight; // 设置滚动条的视窗大小
            VerticalScrollBar.Value = listViewScrollViewer.VerticalOffset; // 设置滚动条的当前值
        }

        // 计算每行显示的项数
        private int CalculateItemsPerRow(WrapPanel itemsPanel)
        {
            if (itemsPanel == null || itemsPanel.Children.Count == 0)
                return 1; // 如果 WrapPanel 为空，返回 1
            double panelWidth = itemsPanel.ActualWidth; // 获取 WrapPanel 的宽度
            if (panelWidth == 0)
                return 1; // 如果 WrapPanel 的宽度为 0，返回 1
            double itemWidth = ((FrameworkElement)itemsPanel.Children[0]).ActualWidth; // 假设所有项的宽度相同
            if (itemWidth == 0)
                return 1; // 如果项的宽度为 0，返回 1
            return (int)(panelWidth / itemWidth); // 计算每行显示的项数
        }

        // 窗口大小变化时，更新滚动条的属性
        private void SelectImageWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollBarProperties(); // 窗口大小变化时，更新滚动条的属性
        }

        // 更新滚动条的值
        private void ListViewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (listViewScrollViewer == null || VerticalScrollBar == null)
                return; // 如果 ScrollViewer 或 VerticalScrollBar 为 null，返回
            VerticalScrollBar.Value = listViewScrollViewer.VerticalOffset; // 更新滚动条的值
        }

        // 外部滚动条的值变化事件
        private void ExternalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == VerticalScrollBar)
            {
                listViewScrollViewer.ScrollToVerticalOffset(e.NewValue); // 更新 ScrollViewer 的位置
                var itemsPanel = FindVisualChild<WrapPanel>(ImageListView); // 查找 WrapPanel
                itemsPanel.InvalidateArrange(); // 重新布局
                itemsPanel.InvalidateMeasure(); // 重新测量
            }
        }

        // 查找 ScrollViewer
        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) // 遍历子元素
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i); // 获取子元素
                if (child is ScrollViewer)
                    return (ScrollViewer)child; // 返回 ScrollViewer
                else
                {
                    ScrollViewer scrollViewer = FindScrollViewer(child); // 递归查找
                    if (scrollViewer != null) // 如果找到 ScrollViewer
                        return scrollViewer; // 返回 ScrollViewer
                }
            }
            return null; // 未找到返回空
        }

        // 从文件夹加载图片
        private void LoadImagesFromFolder()
        {
            string targetFolderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 文件夹路径
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath); // 创建文件夹
                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(targetFolderPath)); // 获取文件夹信息
                return; // 返回
            } // 如果文件夹不存在，则创建文件夹
            string[] supportedExtensions = { ".png", ".ico", ".jpg", ".jpeg", ".bmp", ".gif" }; // 支持的图片格式
            string[] imageFiles = Directory.GetFiles(targetFolderPath); // 获取文件夹中的所有文件
            int maxImagesToLoad = 45; // 一次加载的最大图片数量
            int count = 0;
            foreach (string file in imageFiles) // 遍历文件夹中的所有文件
            {
                if (count >= maxImagesToLoad) break; // 达到最大数量时停止加载
                FileInfo fileInfo = new FileInfo(file); // 获取文件信息
                if (supportedExtensions.Contains(fileInfo.Extension.ToLower())) // 检查文件扩展名
                {
                    var imageItem = new ImageItem // 创建图片项
                    {
                        FilePath = file, // 文件路径
                        FileName = fileInfo.Name, // 文件名
                        ImageSource = LoadImage(file) // 加载图片
                    };
                    ImageItems.Add(imageItem); // 添加到图片项集合
                    count++; // 增加计数器
                }
            } // 遍历完成后，更新 ScrollBar 的属性
        }

        // 加载图片
        private BitmapImage LoadImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null; // 如果文件不存在，返回空
                BitmapImage bi = new BitmapImage(); // 创建 BitmapImage 对象
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bi.BeginInit(); // 开始初始化图片
                    bi.CacheOption = BitmapCacheOption.OnLoad; // 设置缓存选项
                    bi.StreamSource = stream; // 设置图片流
                    bi.EndInit(); // 加载图片
                }
                return bi; // 返回图片
            }
            catch{ return null; } // 加载失败返回空
        }

        // 关闭窗口
        private void CancelSelect(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 选择本地图标
        private void SelectLocalIcons(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter =
                "所有支持的文件 (*.png;*.ico;*.svg;*.exe;*.jpg)|*.png;*.ico;*.svg;*.exe;*.jpg|" +
                "PNG 图片 (*.png)|*.png|" +
                "Icon 文件 (*.ico)|*.ico|" +
                "Exe 文件 (*.exe)|*.exe|" +
                "Svg 图标 (*.svg)|*.svg|" +
                "Jpg 文件 (*.jpg)|*.jpg",
                Multiselect = false, // 不允许多选
                Title = "选择图片文件"
            }; // 创建文件选择对话框
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length != 0)
                {
                    SelectedImagePath = openFileDialog.FileName; // 设置选中的图片路径
                    AddIcon(sender, e); // 触发图片确认事件
                }
            }
        }

        // 按下键盘事件
        private void SelectImageWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.S)
                AddIcon(sender, e); // 按下 S 键添加图标
        }

        // 添加图标
        private void AddIcon(object sender, RoutedEventArgs e)
        {
            ImageConfirmed?.Invoke(this, SelectedImagePath); // 触发图片确认事件
            this.Close(); // 关闭窗口
        }

        // 双击确认图片
        private void ConfirmImage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedImage = ImageListView.SelectedItem as ImageItem; // 获取选中的图片
            if (selectedImage != null)
            {
                SelectedImagePath = selectedImage.FilePath; // 设置选中的图片路径
                AddIcon(sender, e); // 触发图片确认事件
            }
        }

        // 选择图片
        private void SelectImage(object sender, SelectionChangedEventArgs e)
        {
            var selectedImage = ImageListView.SelectedItem as ImageItem; // 获取选中的图片
            if (selectedImage != null)
            {
                SelectedImagePath = selectedImage.FilePath; // 设置选中的图片路径
                ConfirmButton.IsEnabled = true; // 启用确认按钮
            }
        }

        // 递归查找VisualChild
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i); // 获取子元素
                if (child is T t)
                    return t; // 返回匹配的子元素

                T foundChild = FindVisualChild<T>(child); // 递归查找
                if (foundChild != null)
                    return foundChild; // 返回匹配的子元素
            }
            return null; // 未找到返回空
        }

        // 鼠标移入视图显示 Scrollbar
        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VerticalScrollBar.Visibility = Visibility.Visible; // 显示滚动条
        }
         // 鼠标移出视图隐藏 Scrollbar
        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed; // 隐藏滚动条
        }

        private int CalculateRowCount()
        {
            if (ImageListView == null || ImageListView.Items == null || ImageListView.Items.Count == 0)
                return 0; // 如果图片列表为空，返回 0

            Panel itemsPanel = FindVisualChild<Panel>(ImageListView); // 查找 ItemsPanel
            if (itemsPanel == null)
                return 0; // 如果 ItemsPanel 为 null，返回 0

            double itemsPanelWidth = itemsPanel.ActualWidth; // 获取 ItemsPanel 的宽度
            if (itemsPanelWidth <= 0)
                return 0; // 如果 ItemsPanel 的宽度为 0，返回 0

            double itemWidth = 40; // 假设每项宽度为 40 像素
            int itemsPerRow = (int)(itemsPanelWidth / itemWidth); // 计算每行显示的项数
            int totalItems = ImageListView.Items.Count; // 获取总项数

            // 计算行数，向上取整
            int rowCount = (int)Math.Ceiling((double)totalItems / itemsPerRow);
            return rowCount; // 返回行数
        }

        // 管理本地图标
        private void ManageLocalIcons(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 文件夹路径
            if (!Directory.Exists(folderPath))  Directory.CreateDirectory(folderPath); // 创建文件夹
            Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true }); // 打开文件夹
        }

        // 继续加载图片
        private void ContinueLoadImages(object sender, RoutedEventArgs e)
        {
            string targetFolderPath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 文件夹路径
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath); // 创建文件夹
                return; // 如果文件夹不存在，则创建文件夹
            }

            string[] supportedExtensions = { ".png", ".ico", ".jpg", ".jpeg", ".bmp", ".gif" }; // 支持的图片格式
            string[] imageFiles = Directory.GetFiles(targetFolderPath); // 获取文件夹中的所有文件

            int continueLoadCount = 27; // 每次继续加载的图片数量
            int startIndex = ImageItems.Count; // 当前已加载的图片数量
            int endIndex = Math.Min(startIndex + continueLoadCount, imageFiles.Length); // 计算结束索引

            for (int i = startIndex; i < endIndex; i++)
            {
                string file = imageFiles[i]; // 获取文件路径
                FileInfo fileInfo = new FileInfo(file); // 获取文件信息
                if (supportedExtensions.Contains(fileInfo.Extension.ToLower())) // 检查文件扩展名
                {
                    var imageItem = new ImageItem // 创建图片项
                    {
                        FilePath = file, // 文件路径
                        FileName = fileInfo.Name, // 文件名
                        ImageSource = LoadImage(file) // 加载图片
                    };
                    ImageItems.Add(imageItem); // 添加到图片项集合
                }
            }
            UpdateScrollBarProperties(); // 更新滚动条的属性
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            foreach (var imageItem in ImageItems) // 遍历图片项集合
            {
                imageItem.ImageSource = null; // 释放图片资源
            }
            ImageItems.Clear(); // 清空图片项集合

            // 清理事件订阅
            ImageConfirmed = null; // 清空事件

            // 释放其他资源
            listViewScrollViewer = null; // 清理 ScrollViewer 引用
            ImageListView.ItemsSource = null; // 清空 ListView 数据源
            SelectedImagePath = null; // 清理选中图片路径
            listViewScrollViewer = null; // 清理 ScrollViewer 引用

            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    // 图片项的数据模型
    public class ImageItem
    {
        public string FilePath { get; set; } // 文件路径
        public string FileName { get; set; } // 文件名
        public BitmapImage ImageSource { get; set; } // 图片源
    }
}