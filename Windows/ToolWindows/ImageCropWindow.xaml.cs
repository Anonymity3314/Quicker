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

        private double imageScale = 1.0;
        private const double ScaleStep = 0.1;
        private const double MinScale = 0.1;
        private const double MaxScale = 10.0;

        /// <summary>
        /// 图片裁剪窗口
        /// </summary>
        /// <param name="filePath"> 图片路径 </param>
        /// <param name="width"> 宽度 </param>
        /// <param name="height"> 高度 </param>
        /// <param name="cornerRadius"> 边框圆角 </param>
        public ImageCropWindow(string filePath, double width, double height, CornerRadius cornerRadius)
        {
            InitializeComponent();
            BorderWidth = width;
            BorderHeight = height;
            BorderCornerRadius = cornerRadius;
            DataContext = this; // 绑定到自身
            CropImage.Source = new BitmapImage(new Uri(filePath, UriKind.Absolute));

            // 注册鼠标滚轮事件
            CropImage.MouseWheel += CropImage_MouseWheel;
        }

        private void CropBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            // 记录鼠标在Border内的偏移
            mouseOffset = e.GetPosition(CropBorder);
            CropBorder.CaptureMouse();
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
            MaskRect.Width = ImageGrid.ActualWidth;
            MaskRect.Height = ImageGrid.ActualHeight;
            UpdateMask();
        }

        // 移动鼠标时，更新CropBorder的位置
        private void CropBorder_MouseMove(object sender, MouseEventArgs e)
        {
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
            UpdateMask();
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
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}