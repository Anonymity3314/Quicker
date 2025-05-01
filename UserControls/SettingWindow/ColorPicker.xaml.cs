using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Quicker.UserControls
{
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        private void UpdateColor()
        {
            if(RedSlider == null || GreenSlider == null || BlueSlider == null || AlphaSlider == null || HexValue == null) return;
            int r = (int)RedSlider.Value;
            int g = (int)GreenSlider.Value;
            int b = (int)BlueSlider.Value;
            byte a = (byte)AlphaSlider.Value;
            string hexColor = $"#{a:X2}{r:X2}{g:X2}{b:X2}";
            HexValue.Text = hexColor;
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider == null) return;

            int value = (int)slider.Value;
            string sliderName = slider.Name;

            switch (sliderName)
            {
                case "RedSlider":
                    RedValue.Text = value.ToString();
                    break;
                case "GreenSlider":
                    GreenValue.Text = value.ToString();
                    break;
                case "BlueSlider":
                    BlueValue.Text = value.ToString();
                    break;
                case "AlphaSlider":
                    AlphaValue.Text = value.ToString();
                    break;
            }

            UpdateColor();
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int value = 0;

            if (!int.TryParse(textBox.Text, out value))
            {
                value = 0;
            }

            if (value < 0) value = 0;
            if (value > 255) value = 255;

            switch (textBox.Name)
            {
                case "RedValue":
                    RedSlider.Value = value;
                    break;
                case "GreenValue":
                    GreenSlider.Value = value;
                    break;
                case "BlueValue":
                    BlueSlider.Value = value;
                    break;
                case "AlphaValue":
                    AlphaSlider.Value = value;
                    break;
            }

            UpdateColor();
        }

        private void HueSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            var hue = ((Slider)sender).Value;
            var hsvColor = Color.FromScRgb(1, (float)hue / 360, 1, 1);
            var rgbColor = Color.FromArgb(255, (byte)(hsvColor.R * 255), (byte)(hsvColor.G * 255), (byte)(hsvColor.B * 255));
            ColorCanvas.Background = new SolidColorBrush(rgbColor);
        }

        private void ColorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorCanvas);
                ColorThumb.SetValue(Canvas.LeftProperty, position.X - ColorThumb.Width / 2);
                ColorThumb.SetValue(Canvas.TopProperty, position.Y - ColorThumb.Height / 2);
                UpdateColorFromCanvas();
            }
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorCanvas);
                ColorThumb.SetValue(Canvas.LeftProperty, position.X - ColorThumb.Width / 2);
                ColorThumb.SetValue(Canvas.TopProperty, position.Y - ColorThumb.Height / 2);
                UpdateColorFromCanvas();
            }
        }

        private void ColorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateColorFromCanvas();
        }

        private void UpdateColorFromCanvas()
        {
            var x = (double)ColorThumb.GetValue(Canvas.LeftProperty);
            var y = (double)ColorThumb.GetValue(Canvas.TopProperty);

            var saturation = x / ColorCanvas.Width;
            var value = 1 - y / ColorCanvas.Height;

            RedSlider.Value = (int)(saturation * 255);
            GreenSlider.Value = (int)(value * 255);
            BlueSlider.Value = (int)(saturation * value * 255);
            AlphaSlider.Value = 255;
            UpdateColor();
        }
    }
}