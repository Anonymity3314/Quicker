using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Quicker.UserControls
{
    public partial class ColorPicker : UserControl
    {
        private bool _updatingControls = false;
        private Color _currentColor = Colors.White;
        private double _hue = 0;
        private double _saturation = 1;
        private double _value = 1;

        // 颜色变化事件
        public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker),
                new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            picker._currentColor = (Color)e.NewValue;
            picker.UpdateControlsFromColor();
            
            // 触发颜色变化事件
            picker.RaiseSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }
        
        // 触发颜色变化事件的方法
        private void RaiseSelectedColorChanged(Color oldColor, Color newColor)
        {
            SelectedColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
        }

        public ColorPicker()
        {
            InitializeComponent();
            Loaded += ColorPicker_Loaded;
            
            // 设置默认值，防止空引用异常
            if (HueSlider != null)
                HueSlider.Value = 0;
                
            if (RedSlider != null)
                RedSlider.Value = 255;
                
            if (GreenSlider != null)
                GreenSlider.Value = 255;
                
            if (BlueSlider != null)
                BlueSlider.Value = 255;
                
            if (AlphaSlider != null)
                AlphaSlider.Value = 255;
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 确保所有控件都已初始化
                if (ColorCanvas != null && HueSlider != null && 
                    RedSlider != null && GreenSlider != null && 
                    BlueSlider != null && AlphaSlider != null)
                {
                    // 初始化色相滑块
                    HueSlider.Value = _hue;
                    
                    // 更新控件状态
                    UpdateControlsFromColor();
                    
                    // 更新色彩画布
                    UpdateColorThumbPosition();
                }
            }
            catch (Exception ex)
            {
                // 处理异常，防止崩溃
                System.Diagnostics.Debug.WriteLine($"ColorPicker初始化错误: {ex.Message}");
            }
        }

        private void UpdateControlsFromColor()
        {
            // 如果控件尚未初始化或者正在更新，则返回
            if (_updatingControls || RedSlider == null || GreenSlider == null || 
                BlueSlider == null || AlphaSlider == null || HueSlider == null || 
                ColorThumb == null || HexValue == null) 
            {
                return;
            }
            
            _updatingControls = true;

            try
            {
                // Update RGB sliders
                RedSlider.Value = _currentColor.R;
                GreenSlider.Value = _currentColor.G;
                BlueSlider.Value = _currentColor.B;
                AlphaSlider.Value = _currentColor.A;

                // Convert RGB to HSV
                RgbToHsv(_currentColor.R, _currentColor.G, _currentColor.B, out _hue, out _saturation, out _value);

                // Update Hue slider
                HueSlider.Value = _hue;

                // Update color thumb position if canvas is available
                if (ColorCanvas != null && ColorCanvas.ActualWidth > 0 && ColorCanvas.ActualHeight > 0)
                {
                    UpdateColorThumbPosition();
                }

                // Update hex value
                UpdateHexValue();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateControlsFromColor错误: {ex.Message}");
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void UpdateColorThumbPosition()
        {
            // 确保ColorCanvas和ColorThumb不为null且已正确初始化
            if (ColorCanvas == null || ColorThumb == null || 
                ColorCanvas.ActualWidth <= 0 || ColorCanvas.ActualHeight <= 0)
            {
                return;
            }

            try
            {
                double x = _saturation * ColorCanvas.ActualWidth;
                double y = (1 - _value) * ColorCanvas.ActualHeight;
                
                ColorThumb.SetValue(Canvas.LeftProperty, Math.Max(0, Math.Min(ColorCanvas.ActualWidth - ColorThumb.Width / 2, x - ColorThumb.Width / 2)));
                ColorThumb.SetValue(Canvas.TopProperty, Math.Max(0, Math.Min(ColorCanvas.ActualHeight - ColorThumb.Height / 2, y - ColorThumb.Height / 2)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateColorThumbPosition错误: {ex.Message}");
            }
        }

        private void UpdateColorFromHsv()
        {
            if (_updatingControls || RedSlider == null || GreenSlider == null || BlueSlider == null) return;
            
            _updatingControls = true;

            try
            {
                // Convert HSV to RGB
                HsvToRgb(_hue, _saturation, _value, out byte r, out byte g, out byte b);
                
                // Create new color with current alpha
                _currentColor = Color.FromArgb(_currentColor.A, r, g, b);
                SelectedColor = _currentColor;

                // Update RGB sliders
                RedSlider.Value = r;
                GreenSlider.Value = g;
                BlueSlider.Value = b;

                // Update hex value
                UpdateHexValue();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateColorFromHsv错误: {ex.Message}");
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void UpdateHexValue()
        {
            if (HexValue == null) return;
            
            try
            {
                HexValue.Text = $"#{_currentColor.A:X2}{_currentColor.R:X2}{_currentColor.G:X2}{_currentColor.B:X2}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateHexValue错误: {ex.Message}");
            }
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingControls) return;
            
            var slider = sender as Slider;
            if (slider == null) return;

            int value = (int)slider.Value;
            string sliderName = slider.Name;

            switch (sliderName)
            {
                case "RedSlider":
                    RedValue.Text = value.ToString();
                    _currentColor = Color.FromArgb(_currentColor.A, (byte)value, _currentColor.G, _currentColor.B);
                    break;
                case "GreenSlider":
                    GreenValue.Text = value.ToString();
                    _currentColor = Color.FromArgb(_currentColor.A, _currentColor.R, (byte)value, _currentColor.B);
                    break;
                case "BlueSlider":
                    BlueValue.Text = value.ToString();
                    _currentColor = Color.FromArgb(_currentColor.A, _currentColor.R, _currentColor.G, (byte)value);
                    break;
                case "AlphaSlider":
                    AlphaValue.Text = value.ToString();
                    _currentColor = Color.FromArgb((byte)value, _currentColor.R, _currentColor.G, _currentColor.B);
                    break;
            }

            SelectedColor = _currentColor;
            
            // Update HSV values
            RgbToHsv(_currentColor.R, _currentColor.G, _currentColor.B, out _hue, out _saturation, out _value);
            
            // Update color thumb position without triggering events
            _updatingControls = true;
            UpdateColorThumbPosition();
            _updatingControls = false;
            
            UpdateHexValue();
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls) return;
            
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            
            int value = 0;
            if (!int.TryParse(textBox.Text, out value))
            {
                value = 0;
            }

            value = Math.Max(0, Math.Min(255, value));

            _updatingControls = true;
            switch (textBox.Name)
            {
                case "RedValue":
                    RedSlider.Value = value;
                    _currentColor = Color.FromArgb(_currentColor.A, (byte)value, _currentColor.G, _currentColor.B);
                    break;
                case "GreenValue":
                    GreenSlider.Value = value;
                    _currentColor = Color.FromArgb(_currentColor.A, _currentColor.R, (byte)value, _currentColor.B);
                    break;
                case "BlueValue":
                    BlueSlider.Value = value;
                    _currentColor = Color.FromArgb(_currentColor.A, _currentColor.R, _currentColor.G, (byte)value);
                    break;
                case "AlphaValue":
                    AlphaSlider.Value = value;
                    _currentColor = Color.FromArgb((byte)value, _currentColor.R, _currentColor.G, _currentColor.B);
                    break;
            }
            _updatingControls = false;

            SelectedColor = _currentColor;
            
            // Update HSV values
            RgbToHsv(_currentColor.R, _currentColor.G, _currentColor.B, out _hue, out _saturation, out _value);
            
            // Update color thumb position
            UpdateColorThumbPosition();
            
            UpdateHexValue();
        }

        private void HueSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingControls) return;
            
            _hue = ((Slider)sender).Value;
            
            // Update the color canvas gradient with the new hue
            Color hueColor = GetColorFromHue(_hue);
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 0);
            gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0));
            gradientBrush.GradientStops.Add(new GradientStop(hueColor, 1));
            
            // Find the first Rectangle in ColorCanvas and update its Fill
            foreach (var child in ColorCanvas.Children)
            {
                if (child is Rectangle rect)
                {
                    rect.Fill = gradientBrush;
                    break;
                }
            }
            
            UpdateColorFromHsv();
        }

        private Color GetColorFromHue(double hue)
        {
            HsvToRgb(hue, 1, 1, out byte r, out byte g, out byte b);
            return Color.FromRgb(r, g, b);
        }

        private void ColorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorCanvas);
                UpdateColorFromCanvasPosition(position);
            }
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorCanvas);
                UpdateColorFromCanvasPosition(position);
            }
        }

        private void ColorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ColorCanvas);
            UpdateColorFromCanvasPosition(position);
        }

        private void UpdateColorFromCanvasPosition(Point position)
        {
            double canvasWidth = ColorCanvas.ActualWidth;
            double canvasHeight = ColorCanvas.ActualHeight;
            
            // Clamp position within canvas bounds
            double x = Math.Max(0, Math.Min(canvasWidth, position.X));
            double y = Math.Max(0, Math.Min(canvasHeight, position.Y));
            
            // Update thumb position
            ColorThumb.SetValue(Canvas.LeftProperty, x - ColorThumb.Width / 2);
            ColorThumb.SetValue(Canvas.TopProperty, y - ColorThumb.Height / 2);
            
            // Calculate saturation and value from position
            _saturation = x / canvasWidth;
            _value = 1 - (y / canvasHeight);
            
            UpdateColorFromHsv();
        }
        
        // RGB to HSV conversion
        private void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
        {
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;
            
            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;
            
            // Value
            v = max;
            
            // Saturation
            s = max == 0 ? 0 : delta / max;
            
            // Hue
            h = 0;
            
            if (delta == 0)
            {
                h = 0; // Achromatic (grey)
            }
            else
            {
                if (max == red)
                {
                    h = ((green - blue) / delta) % 6;
                }
                else if (max == green)
                {
                    h = (blue - red) / delta + 2;
                }
                else
                {
                    h = (red - green) / delta + 4;
                }
                
                h *= 60;
                if (h < 0) h += 360;
            }
        }
        
        // HSV to RGB conversion
        private void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;
            
            double red = 0, green = 0, blue = 0;
            
            if (h >= 0 && h < 60)
            {
                red = c; green = x; blue = 0;
            }
            else if (h >= 60 && h < 120)
            {
                red = x; green = c; blue = 0;
            }
            else if (h >= 120 && h < 180)
            {
                red = 0; green = c; blue = x;
            }
            else if (h >= 180 && h < 240)
            {
                red = 0; green = x; blue = c;
            }
            else if (h >= 240 && h < 300)
            {
                red = x; green = 0; blue = c;
            }
            else
            {
                red = c; green = 0; blue = x;
            }
            
            r = (byte)Math.Round((red + m) * 255);
            g = (byte)Math.Round((green + m) * 255);
            b = (byte)Math.Round((blue + m) * 255);
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls) return;
            
            string hexText = HexValue.Text.Trim();
            
            // Add # if missing
            if (!hexText.StartsWith("#"))
            {
                hexText = "#" + hexText;
                _updatingControls = true;
                HexValue.Text = hexText;
                HexValue.CaretIndex = hexText.Length;
                _updatingControls = false;
            }
            
            // Check if valid hex color format
            if (IsValidHexColor(hexText))
            {
                try
                {
                    Color newColor;
                    
                    // Parse the hex color
                    if (hexText.Length == 9) // With alpha: #AARRGGBB
                    {
                        newColor = (Color)ColorConverter.ConvertFromString(hexText);
                    }
                    else if (hexText.Length == 7) // Without alpha: #RRGGBB
                    {
                        var baseColor = (Color)ColorConverter.ConvertFromString(hexText);
                        newColor = Color.FromArgb(_currentColor.A, baseColor.R, baseColor.G, baseColor.B);
                    }
                    else if (hexText.Length == 4) // Short format: #RGB
                    {
                        // Convert #RGB to #RRGGBB
                        string fullHex = "#" + hexText[1] + hexText[1] + hexText[2] + hexText[2] + hexText[3] + hexText[3];
                        var baseColor = (Color)ColorConverter.ConvertFromString(fullHex);
                        newColor = Color.FromArgb(_currentColor.A, baseColor.R, baseColor.G, baseColor.B);
                    }
                    else
                    {
                        return;
                    }
                    
                    _currentColor = newColor;
                    SelectedColor = _currentColor;
                    
                    // Update controls
                    _updatingControls = true;
                    UpdateControlsFromColor();
                    _updatingControls = false;
                }
                catch
                {
                    // Invalid color format, ignore
                }
            }
        }
        
        private bool IsValidHexColor(string hexColor)
        {
            // Check for #RGB, #RRGGBB or #AARRGGBB format
            return Regex.IsMatch(hexColor, @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");
        }
    }

    // 颜色变化事件参数类
    public class ColorChangedEventArgs : EventArgs
    {
        public Color OldColor { get; }
        public Color NewColor { get; }

        public ColorChangedEventArgs(Color oldColor, Color newColor)
        {
            OldColor = oldColor;
            NewColor = newColor;
        }
    }
}