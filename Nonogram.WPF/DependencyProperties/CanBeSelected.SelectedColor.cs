using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public static partial class CanBeSelected
    {
        public static int GetSelectedColor(DependencyObject obj)
            => (int)obj.GetValue(SelectedColorProperty);

        public static void SetSelectedColor(DependencyObject obj, int value)
            => obj.SetValue(SelectedColorProperty, value);

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.RegisterAttached("SelectedColor", typeof(int), typeof(CanBeSelected), new PropertyMetadata(-1));
    }
}
