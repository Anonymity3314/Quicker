using System.Globalization;
using System.Windows.Data;

namespace Quicker.Converters
{
    /// <summary>
    /// ButtonScaleConverter：用于根据鼠标悬停、是否显示缩放、是否为动作按钮等条件，动态调整按钮的缩放比例。
    /// </summary>
    public class ButtonScaleConverter : IMultiValueConverter
    {
        /// <summary>
        /// 将多个绑定值转换为按钮的缩放比例。
        /// </summary>
        /// <param name="values">依次为：是否鼠标悬停（bool）、是否显示缩放（bool）、是否为动作按钮（object，非null即为动作按钮）</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域信息</param>
        /// <returns>缩放比例（1.05 或 1.0，类型为 double）</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 判断第一个参数是否为true（鼠标悬停）
            bool isMouseOver = values[0] is bool b && b;
            // 判断第二个参数是否为true（允许缩放）
            bool showScale = values[1] is bool s && s;
            // 判断第三个参数是否非null（为动作按钮）
            bool isAction = values[2] != null;
            // 只有在鼠标悬停、允许缩放且为动作按钮时，缩放为1.05，否则为1.0
            return (isMouseOver && showScale && isAction) ? 1.05 : 1.0;
        }

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}