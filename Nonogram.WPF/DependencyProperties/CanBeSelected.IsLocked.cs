using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Nonogram.WPF.DependencyProperties
{
    public static partial class CanBeSelected
    {
        public static bool GetIsLocked(DependencyObject obj)
            => (bool)(bool?)obj.GetValue(IsLockedProperty);

        public static void SetIsLocked(DependencyObject obj, bool value)
            => obj.SetValue(IsLockedProperty, value);

        // Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.RegisterAttached("IsLocked", typeof(bool?), typeof(CanBeSelected), new PropertyMetadata(null, IsLockedChanged));

        private static void IsLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((d, e.OldValue, e.NewValue))
            {
                case (FrameworkElement elem, null, bool):
                    elem.MouseRightButtonUp += SetSelected;
                    break;
                case (FrameworkElement elem, bool, null):
                    elem.MouseRightButtonUp -= SetSelected;
                    break;
            }
        }

        private static void SetSelected(object sender, MouseButtonEventArgs e)
        {
            if (GetIsLocked((DependencyObject)sender) || _cache.Values.Count(e => GetIsLocked(e)) < _colors.Count - 1)
            {
                SetIsLocked((DependencyObject)sender, !GetIsLocked((DependencyObject)sender));
                e.Handled = true;
            }
        }
    }
}
