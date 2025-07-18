using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;

namespace Quicker.Converters
{
    /// <summary>
    /// 颜色加深转换器。将颜色字符串转换为加深后的 SolidColorBrush。
    /// 用于将颜色变暗，factor 越小越深。
    /// </summary>
    public class DarkerColorConverter : IValueConverter
    {
        /// <summary>
        /// 加深系数，取值范围 0~1，越小越深。默认 0.8。
        /// </summary>
        public double Factor { get; set; } = 0.8;

        /// <summary>
        /// 将颜色字符串转换为加深后的 SolidColorBrush。
        /// </summary>
        /// <param name="value">颜色字符串（如 #FFAABBCC）</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">区域性</param>
        /// <returns>加深后的 SolidColorBrush</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                byte r = (byte)(color.R * Factor);
                byte g = (byte)(color.G * Factor);
                byte b = (byte)(color.B * Factor);
                return new SolidColorBrush(Color.FromArgb(color.A, r, g, b));
            }
            if (value is string colorStr)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorStr);
                    byte r = (byte)(color.R * Factor);
                    byte g = (byte)(color.G * Factor);
                    byte b = (byte)(color.B * Factor);
                    return new SolidColorBrush(Color.FromArgb(color.A, r, g, b));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        /// <param name="value">颜色</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">区域性</param>
        /// <returns>抛出 NotImplementedException</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}