using System;
using System.Globalization;
using System.Windows.Data;

namespace Quicker.Converters
{
    public class ButtonScaleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMouseOver = values[0] is bool b && b;
            bool showScale = values[1] is bool s && s;
            bool isAction = values[2] != null;
            return (isMouseOver && showScale && isAction) ? 1.05 : 1.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}