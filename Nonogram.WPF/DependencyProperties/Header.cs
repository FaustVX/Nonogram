using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public static class Header
    {
        public static string GetHeader(DependencyObject obj)
            => (string)obj.GetValue(HeaderProperty);

        public static void SetHeader(DependencyObject obj, string value)
            => obj.SetValue(HeaderProperty, value);

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached("Header", typeof(string), typeof(Header), new PropertyMetadata(""));
    }
}
