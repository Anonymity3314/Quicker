using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.IO;

namespace Quicker.Windows.ToolWindows
{
    public partial class ImageCropWindow : Window
    {
        public CornerRadius BorderCornerRadius { get; set; } // 边框圆角
        public double BorderHeight { get; set; } // 边框高度
        public double BorderWidth { get; set; } // 边框宽度

        private bool isDragging = false; // 是否正在拖动
        private Point mouseOffset; // 鼠标在Border内的偏移
        private bool isResizing = false; // 是否正在调整大小
        private string resizeHandle; // 当前调整大小的手柄位置
        private Point resizeStartPoint; // 开始调整大小时的鼠标位置
        private double resizeStartWidth; // 开始调整大小时的宽度
        private double resizeStartHeight; // 开始调整大小时的高度
        private double resizeStartLeft; // 开始调整大小时的左边距
        private double resizeStartTop; // 开始调整大小时的上边距
        private double aspectRatio; // 宽高比

        private double imageScale = 1.0;
        private const double ScaleStep = 0.1;
        private const double MinScale = 0.1;
        private const double MaxScale = 10.0;

        public string CroppedImagePath { get; private set; }

        public event Action<object, string> CropCompleted;

        private void OnCropCompleted(string path)
        {
            CropCompleted?.Invoke(this, path);
            this.Close();
        }

        /// <summary>
        /// 图片裁剪窗口，传递宽高比
        /// </summary>
        /// <param name="filePath">图片路径</param>
        /// <param name="aspectRatio">宽高比（宽/高）</param>
        /// <param name="cornerRadius">边框圆角</param>
        public ImageCropWindow(string filePath, double aspectRatio, CornerRadius cornerRadius)
        {
            InitializeComponent();
            this.aspectRatio = aspectRatio;
            BorderCornerRadius = cornerRadius;
            // 设置初始大小，避免CropBorder在Loaded事件之前尺寸为0
            BorderWidth = 200; // 初始宽度
            BorderHeight = 200 / aspectRatio; // 根据比例计算初始高度
            DataContext = this; // 绑定到自身
            CropImage.Source = new BitmapImage(new Uri(filePath, UriKind.Absolute));

            // 在Loaded事件中会根据ImageGrid实际大小重新调整
        }

        private void CropBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果鼠标点击的是调整手柄，不处理拖动
            if (e.OriginalSource is Rectangle)
                return;

            isDragging = true;
            // 记录鼠标在Border内的偏移
            mouseOffset = e.GetPosition(CropBorder);
            CropBorder.CaptureMouse();
            // 拖动时隐藏调整手柄
            HideResizeHandles();
        }

        private void UpdateMask()
        {
            // 获取Grid在Canvas中的位置
            double gridLeft = Canvas.GetLeft(ImageGrid);
            double gridTop = Canvas.GetTop(ImageGrid);

            // 获取CropBorder在Canvas中的位置
            double borderLeft = Canvas.GetLeft(CropBorder);
            double borderTop = Canvas.GetTop(CropBorder);

            // 计算CropBorder在Grid内的相对位置
            double left = borderLeft - gridLeft;
            double top = borderTop - gridTop;
            double width = CropBorder.ActualWidth;
            double height = CropBorder.ActualHeight;

            // 整个Grid的区域
            var gridRect = new RectangleGeometry(new Rect(0, 0, ImageGrid.ActualWidth, ImageGrid.ActualHeight));
            // 镂空区域
            var holeRect = new RectangleGeometry(new Rect(left, top, width, height), CropBorder.CornerRadius.TopLeft, CropBorder.CornerRadius.TopLeft);

            // 合成：大矩形减去小矩形
            var combined = new CombinedGeometry(GeometryCombineMode.Exclude, gridRect, holeRect);

            // 用DrawingBrush实现镂空
            var drawing = new GeometryDrawing
            {
                Geometry = combined,
                Brush = Brushes.White // OpacityMask的白色部分为不透明，黑色为透明
            };
            var brush = new DrawingBrush(drawing);
            MaskRect.OpacityMask = brush;
        }

        // 在窗口加载和每次拖动后都调用
        private void ImageCropWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMask(); // 更新蒙版
        }

        // 移动鼠标时，更新CropBorder的位置
        private void CropBorder_MouseMove(object sender, MouseEventArgs e)
        {
            // 如果正在调整大小，不处理拖动
            if (isResizing)
                return;

            if (isDragging)
            {
                // 获取Grid在Canvas中的位置
                double gridLeft = Canvas.GetLeft(ImageGrid);
                double gridTop = Canvas.GetTop(ImageGrid);

                // 鼠标在Canvas中的位置
                var position = e.GetPosition((Canvas)CropBorder.Parent);

                // 计算新的Left/Top
                double newLeft = position.X - mouseOffset.X;
                double newTop = position.Y - mouseOffset.Y;

                // 限制在Grid区域内
                newLeft = Math.Max(gridLeft, Math.Min(newLeft, gridLeft + ImageGrid.ActualWidth - CropBorder.ActualWidth));
                newTop = Math.Max(gridTop, Math.Min(newTop, gridTop + ImageGrid.ActualHeight - CropBorder.ActualHeight));

                Canvas.SetLeft(CropBorder, newLeft);
                Canvas.SetTop(CropBorder, newTop);

                UpdateMask();
            }
        }

        // 鼠标松开时，停止拖动
        private void CropBorder_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            CropBorder.ReleaseMouseCapture();
            // 拖动结束后显示调整手柄
            ShowResizeHandles();
            UpdateMask();
        }

        /// <summary>
        /// 调整大小手柄的鼠标按下事件
        /// </summary>
        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResizing = true;
            var handle = sender as FrameworkElement;
            resizeHandle = handle.Tag.ToString();
            
            // 记录开始调整时的状态
            resizeStartPoint = e.GetPosition((Canvas)CropBorder.Parent);
            resizeStartWidth = CropBorder.ActualWidth;
            resizeStartHeight = CropBorder.ActualHeight;
            resizeStartLeft = Canvas.GetLeft(CropBorder);
            resizeStartTop = Canvas.GetTop(CropBorder);
            
            handle.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 调整大小手柄的鼠标移动事件
        /// </summary>
        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizing) return;

            var currentPoint = e.GetPosition((Canvas)CropBorder.Parent);
            double deltaX = currentPoint.X - resizeStartPoint.X;
            double deltaY = currentPoint.Y - resizeStartPoint.Y;

            // 获取Grid边界
            double gridLeft = Canvas.GetLeft(ImageGrid);
            double gridTop = Canvas.GetTop(ImageGrid);
            double gridRight = gridLeft + ImageGrid.ActualWidth;
            double gridBottom = gridTop + ImageGrid.ActualHeight;

            double newWidth = resizeStartWidth;
            double newHeight = resizeStartHeight;
            double newLeft = resizeStartLeft;
            double newTop = resizeStartTop;

            // 根据不同的手柄位置计算新的尺寸和位置
            switch (resizeHandle)
            {
                case "TopLeft":
                    // 左上角：同时调整宽度、高度和位置
                    newWidth = Math.Max(50, resizeStartWidth - deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    newLeft = resizeStartLeft + (resizeStartWidth - newWidth);
                    newTop = resizeStartTop + (resizeStartHeight - newHeight);
                    break;
                case "TopRight":
                    // 右上角：调整宽度、高度和上边距
                    newWidth = Math.Max(50, resizeStartWidth + deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    newTop = resizeStartTop + (resizeStartHeight - newHeight);
                    break;
                case "BottomLeft":
                    // 左下角：调整宽度、高度和左边距
                    newWidth = Math.Max(50, resizeStartWidth - deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    newLeft = resizeStartLeft + (resizeStartWidth - newWidth);
                    break;
                case "BottomRight":
                    // 右下角：调整宽度和高度
                    newWidth = Math.Max(50, resizeStartWidth + deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    break;
                case "Top":
                    // 上边中点：调整高度和上边距
                    newHeight = Math.Max(50, resizeStartHeight - deltaY);
                    newWidth = newHeight * aspectRatio; // 保持比例
                    newTop = resizeStartTop + (resizeStartHeight - newHeight);
                    break;
                case "Bottom":
                    // 下边中点：调整高度
                    newHeight = Math.Max(50, resizeStartHeight + deltaY);
                    newWidth = newHeight * aspectRatio; // 保持比例
                    break;
                case "Left":
                    // 左边中点：调整宽度和左边距
                    newWidth = Math.Max(50, resizeStartWidth - deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    newLeft = resizeStartLeft + (resizeStartWidth - newWidth);
                    break;
                case "Right":
                    // 右边中点：调整宽度
                    newWidth = Math.Max(50, resizeStartWidth + deltaX);
                    newHeight = newWidth / aspectRatio; // 保持比例
                    break;
            }

            // 边界检查并重新计算保持比例的尺寸
            if (newLeft < gridLeft)
            {
                newLeft = gridLeft;
                newWidth = resizeStartWidth - (gridLeft - resizeStartLeft);
                newHeight = newWidth / aspectRatio;
            }
            if (newTop < gridTop)
            {
                newTop = gridTop;
                newHeight = resizeStartHeight - (gridTop - resizeStartTop);
                newWidth = newHeight * aspectRatio;
            }
            if (newLeft + newWidth > gridRight)
            {
                newWidth = gridRight - newLeft;
                newHeight = newWidth / aspectRatio;
            }
            if (newTop + newHeight > gridBottom)
            {
                newHeight = gridBottom - newTop;
                newWidth = newHeight * aspectRatio;
            }

            // 确保最小尺寸
            if (newWidth < 50)
            {
                newWidth = 50;
                newHeight = newWidth / aspectRatio;
            }
            if (newHeight < 50)
            {
                newHeight = 50;
                newWidth = newHeight * aspectRatio;
            }

            // 应用新的尺寸和位置
            CropBorder.Width = newWidth;
            CropBorder.Height = newHeight;
            Canvas.SetLeft(CropBorder, newLeft);
            Canvas.SetTop(CropBorder, newTop);

            // 更新绑定属性
            BorderWidth = newWidth;
            BorderHeight = newHeight;

            UpdateMask();
        }

        /// <summary>
        /// 调整大小手柄的鼠标松开事件
        /// </summary>
        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            isResizing = false;
            var handle = sender as FrameworkElement;
            handle.ReleaseMouseCapture();
            UpdateMask();
        }

        /// <summary>
        /// 隐藏所有调整手柄
        /// </summary>
        private void HideResizeHandles()
        {
            ResizeHandleTopLeft.Fill = Brushes.Transparent;
            ResizeHandleTopRight.Fill = Brushes.Transparent;
            ResizeHandleBottomLeft.Fill = Brushes.Transparent;
            ResizeHandleBottomRight.Fill = Brushes.Transparent;
            ResizeHandleTop.Fill = Brushes.Transparent;
            ResizeHandleBottom.Fill = Brushes.Transparent;
            ResizeHandleLeft.Fill = Brushes.Transparent;
            ResizeHandleRight.Fill = Brushes.Transparent;
        }

        /// <summary>
        /// 显示所有调整手柄
        /// </summary>
        private void ShowResizeHandles()
        {
            ResizeHandleTopLeft.Fill = Brushes.Red;
            ResizeHandleTopRight.Fill = Brushes.Red;
            ResizeHandleBottomLeft.Fill = Brushes.Red;
            ResizeHandleBottomRight.Fill = Brushes.Red;
            ResizeHandleTop.Fill = Brushes.Red;
            ResizeHandleBottom.Fill = Brushes.Red;
            ResizeHandleLeft.Fill = Brushes.Red;
            ResizeHandleRight.Fill = Brushes.Red;
        }

        // 鼠标滚轮更改图片缩放
        private void CropImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                imageScale += ScaleStep;
            else
                imageScale -= ScaleStep;

            imageScale = Math.Max(MinScale, Math.Min(MaxScale, imageScale));

            CropImage.LayoutTransform = new ScaleTransform(imageScale, imageScale);

            UpdateMask();
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. 计算裁剪区域在原图上的坐标
            // 获取图片的 BitmapSource
            var bitmapSource = CropImage.Source as BitmapSource;
            if (bitmapSource == null)
            {
                MessageBox.Show("图片加载失败！");
                return;
            }

            // 获取Grid和Border的相对位置
            double gridLeft = Canvas.GetLeft(ImageGrid);
            double gridTop = Canvas.GetTop(ImageGrid);
            double borderLeft = Canvas.GetLeft(CropBorder);
            double borderTop = Canvas.GetTop(CropBorder);

            // Border在Grid内的相对位置
            double cropX = borderLeft - gridLeft;
            double cropY = borderTop - gridTop;
            double cropWidth = CropBorder.ActualWidth;
            double cropHeight = CropBorder.ActualHeight;

            // 2. 计算显示区域和原图的缩放关系
            // 由于Image的Stretch=Uniform，需考虑缩放和偏移
            double imgControlWidth = ImageGrid.ActualWidth;
            double imgControlHeight = ImageGrid.ActualHeight;
            double bmpWidth = bitmapSource.PixelWidth;
            double bmpHeight = bitmapSource.PixelHeight;

            double scale = Math.Min(imgControlWidth / bmpWidth, imgControlHeight / bmpHeight);

            // 图片实际显示的大小
            double showWidth = bmpWidth * scale;
            double showHeight = bmpHeight * scale;

            // 图片在控件中的偏移（居中）
            double offsetX = (imgControlWidth - showWidth) / 2;
            double offsetY = (imgControlHeight - showHeight) / 2;

            // Border在原图上的像素坐标
            double x = (cropX - offsetX) / scale;
            double y = (cropY - offsetY) / scale;
            double w = cropWidth / scale;
            double h = cropHeight / scale;

            // 边界修正
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            w = Math.Min(bmpWidth - x, w);
            h = Math.Min(bmpHeight - y, h);

            if (w <= 0 || h <= 0)
            {
                MessageBox.Show("裁剪区域无效！");
                return;
            }

            // 3. 裁剪
            var cropped = new CroppedBitmap(bitmapSource, new Int32Rect((int)x, (int)y, (int)w, (int)h));

            // 4. 生成文件名
            string dir = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\BackgroundImages";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds():x}.png";
            string filePath = System.IO.Path.Combine(dir, fileName);

            // 5. 保存为PNG
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(cropped));
                encoder.Save(fileStream);
            }
            CroppedImagePath = filePath; // 保存路径
            OnCropCompleted(CroppedImagePath); // 调用事件
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}