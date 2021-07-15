using System.Collections.Generic;
using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public static partial class CanBeSelected
    {
        public static object? GetIsColor(DependencyObject obj)
            => obj.GetValue(IsColorProperty);

        public static void SetIsColor(DependencyObject obj, object? value)
            => obj.SetValue(IsColorProperty, value);

        // Using a DependencyProperty as the backing store for IsColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsColorProperty =
            DependencyProperty.RegisterAttached("IsColor", typeof(object), typeof(CanBeSelected), new PropertyMetadata(null, IsColorChanged));

        private static Dictionary<object, FrameworkElement> _cache = default!;

        private static void IsColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((d, e.OldValue, e.NewValue))
            {
                case (FrameworkElement elem, null, object color):
                    _cache[color] = elem;
                    break;
                case (FrameworkElement elem, object, null):
                    _cache.Remove(elem);
                    break;
            }
        }
    }
}
