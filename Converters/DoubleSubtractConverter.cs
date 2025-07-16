namespace Quicker.Converters
{
    /// <summary>
    /// DoubleSubtractConverter：用于将double类型的值减去指定参数，常用于XAML绑定时对宽高等数值进行微调。
    /// 宽度绑定为 ActualWidth-0.6，实现阴影Border略小于内容Border。
    /// </summary>
    public class DoubleSubtractConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// 将value（double）减去parameter（double），返回结果。
        /// </summary>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double d && parameter != null && double.TryParse(parameter.ToString(), out double sub))
            {
                return d - sub; // 减去参数
            }
            return value; // 错误值
        }
        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new System.NotImplementedException();
    }
}