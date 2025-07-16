using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Quicker.Resources.Styles
{
    /// <summary>
    /// 提供附加属性 ParentBackground，用于在控件中传递父级背景色
    /// 以便子控件能够访问和使用父控件的背景色
    /// </summary>
    public static class ParentBackgroundHelper
    {
        /// <summary>
        /// 附加属性 ParentBackground，用于存储父控件的背景画刷。
        /// </summary>
        public static readonly DependencyProperty ParentBackgroundProperty =
            DependencyProperty.RegisterAttached("ParentBackground", typeof(Brush), typeof(ParentBackgroundHelper), new PropertyMetadata(null));

        /// <summary>
        /// 设置指定元素的 ParentBackground 附加属性。
        /// </summary>
        /// <param name="element">要设置属性的 UI 元素。</param>
        /// <param name="value">要设置的背景画刷。</param>
        public static void SetParentBackground(UIElement element, Brush value)
        {
            element.SetValue(ParentBackgroundProperty, value);
        }
        /// <summary>
        /// 获取指定元素的 ParentBackground 附加属性。
        /// </summary>
        /// <param name="element">要获取属性的 UI 元素。</param>
        /// <returns>父控件的背景画刷。</returns>
        public static Brush GetParentBackground(UIElement element)
        {
            return (Brush)element.GetValue(ParentBackgroundProperty);
        }
    }
}