using System.Windows;

namespace Nonogram.WPF.Controls
{
    public class ColRow : DependencyObject
    {
        private static int _row, _col;

        public static void Reset()
            => _row = _col = 0;

        public static int GetRow(DependencyObject obj)
            => (int)obj.GetValue(RowProperty);

        public static void SetRow(DependencyObject obj, int value)
            => obj.SetValue(RowProperty, value);

        // Using a DependencyProperty as the backing store for Row.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.RegisterAttached("Row", typeof(int), typeof(ColRow), new PropertyMetadata(-1, (_, _) => { }, (_, _) => _row++));

        public static int GetCol(DependencyObject obj)
            => (int)obj.GetValue(ColProperty);

        public static void SetCol(DependencyObject obj, int value)
            => obj.SetValue(ColProperty, value);

        // Using a DependencyProperty as the backing store for Col.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColProperty =
            DependencyProperty.RegisterAttached("Col", typeof(int), typeof(ColRow), new PropertyMetadata(-1, (_, _) => { }, (_, _) => _col++));


    }
}
