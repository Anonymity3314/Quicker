using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Quicker.Converters
{
    public class ButtonBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var tag = values[0];
            var isMouseOver = (bool)values[1];
            var actionColor = values[2]?.ToString();
            var actionHoverColor = values[3]?.ToString();
            var blankColor = values[4]?.ToString();
            var blankHoverColor = values[5]?.ToString();

            bool isAction = tag != null;
            string colorStr = isAction
                ? (isMouseOver ? actionHoverColor : actionColor)
                : (isMouseOver ? blankHoverColor : blankColor);

            // 防御性处理
            if (string.IsNullOrWhiteSpace(colorStr))
                colorStr = "#FFF3F3F3"; // 默认色

            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(colorStr);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}