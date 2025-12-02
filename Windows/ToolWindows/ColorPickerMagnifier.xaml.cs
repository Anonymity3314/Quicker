using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;

namespace Quicker.Windows.ToolWindows
{
    public partial class ColorPickerMagnifier : Window
    {
        #region Win32 API 声明

        // 获取设备上下文
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        // 释放设备上下文
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        // 获取像素颜色
        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        // 创建兼容的设备上下文
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        // 创建兼容的位图
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        // 选择对象
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        // 删除对象
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // 删除设备上下文
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        // 复制位图
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

        #endregion

        #region 常量声明

        private const int OFFSET_DISTANCE = 20; // 窗口距离鼠标的偏移距离
        private const int SRCCOPY = 0x00CC0020;
        private const int MAGNIFICATION = 8; // 放大倍数
        private const int CAPTURE_SIZE = 25; // 捕获区域大小（像素），显示 25x25 像素区域

        #endregion

        #region 属性声明

        public Color SelectedColor { get; private set; }
        
        private bool _positionInitialized = false; // 位置是否已初始化
        private double _lastLeft = double.NaN; // 上次窗口位置
        private double _lastTop = double.NaN;

        #endregion

        public ColorPickerMagnifier()
        {
            InitializeComponent();
        }

        private void ColorPickerMagnifier_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePosition(); // 设置窗口位置在鼠标旁边
        }

        /// <summary>
        /// 更新窗口位置
        /// </summary>
        private void UpdatePosition()
        {
            var mousePos = System.Windows.Forms.Control.MousePosition;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // 计算理想位置（鼠标右下方）
            double idealLeft = mousePos.X + OFFSET_DISTANCE;
            double idealTop = mousePos.Y + OFFSET_DISTANCE;
            
            // 如果窗口超出右边界，放在鼠标左侧
            if (idealLeft + Width > screenWidth)
            {
                idealLeft = mousePos.X - Width - OFFSET_DISTANCE;
            }
            
            // 如果窗口超出下边界，放在鼠标上方
            if (idealTop + Height > screenHeight)
            {
                idealTop = mousePos.Y - Height - OFFSET_DISTANCE;
            }
            
            // 确保窗口不超出左边界和上边界
            idealLeft = Math.Max(0, idealLeft);
            idealTop = Math.Max(0, idealTop);
            
            // 只在位置真正改变时才更新，避免频繁跳动
            if (!_positionInitialized || 
                Math.Abs(idealLeft - _lastLeft) > 1 || 
                Math.Abs(idealTop - _lastTop) > 1)
            {
                Left = idealLeft;
                Top = idealTop;
                _lastLeft = idealLeft;
                _lastTop = idealTop;
                _positionInitialized = true;
            }
        }

        public void UpdateColor()
        {
            var mousePos = System.Windows.Forms.Control.MousePosition;
            UpdatePosition();

            // 获取屏幕像素颜色
            var (R, G, B) = GetPixelColor(mousePos.X, mousePos.Y);
            SelectedColor = Color.FromRgb(R, G, B);

            // 更新预览
            ColorPreviewBorder.Background = new SolidColorBrush(SelectedColor);
            HexTextBlock.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            RgbTextBlock.Text = $"RGB({SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B})";

            // 更新放大镜图像
            UpdateMagnifierImage(mousePos.X, mousePos.Y);
        }

        /// <summary>
        /// 更新放大镜图像
        /// </summary>
        /// <param name="centerX"> 中心x坐标 </param>
        /// <param name="centerY"> 中心y坐标 </param>
        private void UpdateMagnifierImage(int centerX, int centerY)
        {
            try
            {
                int size = CAPTURE_SIZE; // 捕获区域大小（像素）
                int halfSize = size / 2;
                int magnifiedSize = size * MAGNIFICATION;

                // 使用 Win32 API 捕获屏幕区域
                IntPtr screenDC = GetDC(IntPtr.Zero);
                IntPtr memDC = CreateCompatibleDC(screenDC);
                IntPtr bitmap = CreateCompatibleBitmap(screenDC, size, size);
                IntPtr oldBitmap = SelectObject(memDC, bitmap);

                // 复制屏幕区域到内存位图
                BitBlt(memDC, 0, 0, size, size, screenDC, centerX - halfSize, centerY - halfSize, SRCCOPY);

                // 创建 WPF WriteableBitmap 用于放大显示
                var writeableBitmap = new WriteableBitmap(magnifiedSize, magnifiedSize, 96, 96, PixelFormats.Bgr24, null);
                writeableBitmap.Lock();

                // 读取原始位图数据并放大
                byte[] sourceData = new byte[size * size * 3];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        uint pixel = GetPixel(memDC, x, y);
                        byte r = (byte)(pixel & 0x000000FF);
                        byte g = (byte)((pixel & 0x0000FF00) >> 8);
                        byte b = (byte)((pixel & 0x00FF0000) >> 16);

                        int sourceIndex = (y * size + x) * 3;
                        sourceData[sourceIndex] = b;
                        sourceData[sourceIndex + 1] = g;
                        sourceData[sourceIndex + 2] = r;
                    }
                }

                // 放大像素（最近邻插值）
                byte[] magnifiedData = new byte[magnifiedSize * magnifiedSize * 3];
                for (int y = 0; y < magnifiedSize; y++)
                {
                    int sourceY = y / MAGNIFICATION;
                    for (int x = 0; x < magnifiedSize; x++)
                    {
                        int sourceX = x / MAGNIFICATION;
                        int sourceIndex = (sourceY * size + sourceX) * 3;
                        int destIndex = (y * magnifiedSize + x) * 3;
                        magnifiedData[destIndex] = sourceData[sourceIndex];
                        magnifiedData[destIndex + 1] = sourceData[sourceIndex + 1];
                        magnifiedData[destIndex + 2] = sourceData[sourceIndex + 2];
                    }
                }

                // 复制放大后的数据到 WriteableBitmap
                Marshal.Copy(magnifiedData, 0, writeableBitmap.BackBuffer, magnifiedData.Length);

                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, magnifiedSize, magnifiedSize));
                writeableBitmap.Unlock();

                // 清理资源
                SelectObject(memDC, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memDC);
                ReleaseDC(IntPtr.Zero, screenDC);

                MagnifierImage.Source = writeableBitmap;
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取屏幕像素颜色
        /// </summary>
        /// <param name="x"> 像素x坐标 </param>
        /// <param name="y"> 像素y坐标 </param>
        /// <returns> RGB格式的像素颜色 </returns>
        private (byte r, byte g, byte b) GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            try
            {
                uint pixel = GetPixel(hdc, x, y);
                ReleaseDC(IntPtr.Zero, hdc);

                // GetPixel 返回的 COLORREF 格式是 0x00BBGGRR（BGR格式）
                // 低字节 = R（红色），中字节 = G（绿色），高字节 = B（蓝色）
                byte r = (byte)(pixel & 0x000000FF);        // 低字节 = R
                byte g = (byte)((pixel & 0x0000FF00) >> 8);  // 中字节 = G
                byte b = (byte)((pixel & 0x00FF0000) >> 16); // 高字节 = B

                // 返回 RGB 格式：(R, G, B)
                return (r, g, b);
            }
            catch
            {
                ReleaseDC(IntPtr.Zero, hdc);
                return (255, 255, 255); // 默认白色
            }
        }
    }
}