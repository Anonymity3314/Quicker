using System.Windows.Controls;
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
            int r = (int)RedSlider.Value;
            int g = (int)GreenSlider.Value;
            int b = (int)BlueSlider.Value;
            byte a = (byte)AlphaSlider.Value;
            string hexColor = $"#{a:X2}{r:X2}{g:X2}{b:X2}";// 更新十六进制颜色值
            HexValue.Text = hexColor;
            // 更新颜色预览
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            int value = (int)slider.Value;
            string sliderName = slider.Name;
            switch (sliderName) // 根据滑块的名称更新相应的文本框
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
            switch(textBox.Name)
            {
                case "RedValue":
                    if(int.TryParse(RedValue.Text,out int r)) // 如果r的值为数字
                    {
                        if (0 <= r && r <= 255)
                        {
                            return;
                        } // 如果r的值在设计范围之内，缓存后直接返回
                    }
                    else // 如果r的值不是数字，那么将TextBox的值改为缓存中的值
                    {

                    }
                    break;
                case "GreenValue":
                    if (int.TryParse(RedValue.Text, out int g)) // 如果g的值为数字
                    {
                        if (0 <= g && g <= 255)
                        {
                            return;
                        } // 如果g的值在设计范围之内，缓存后直接返回
                    }
                    else // 如果g的值不是数字，那么将TextBox的值改为缓存中的值
                    {

                    }
                    break;
                case "BlueValue":
                    if (int.TryParse(RedValue.Text, out int b)) // 如果b的值为数字
                    {
                        if (0 <= b && b <= 255)
                        {
                            return;
                        } // 如果b的值在设计范围之内，缓存后直接返回
                    }
                    else // 如果b的值不是数字，那么将TextBox的值改为缓存中的值
                    {

                    }
                    break;
                case "AlphaValue":
                    if (int.TryParse(RedValue.Text, out int a)) // 如果a的值为数字
                    {
                        if (0 <= a && a <= 255)
                        {
                            return;
                        } // 如果a的值在设计范围之内，缓存后直接返回
                    }
                    else // 如果a的值不是数字，那么将TextBox的值改为缓存中的值
                    {

                    }
                    break;
            }
        }


    }
}