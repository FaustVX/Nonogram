using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public static class FileExtension
    {
        public static string GetExtension(DependencyObject obj)
            => (string)obj.GetValue(ExtensionProperty);

        public static void SetExtension(DependencyObject obj, string value)
            => obj.SetValue(ExtensionProperty, value);

        // Using a DependencyProperty as the backing store for Extension.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtensionProperty =
            DependencyProperty.RegisterAttached("Extension", typeof(string), typeof(FileExtension), new PropertyMetadata("All Files|*.*"));
    }
}
