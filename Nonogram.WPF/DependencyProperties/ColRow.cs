using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public class ColRow
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
            DependencyProperty.RegisterAttached("Row", typeof(int), typeof(ColRow), new PropertyMetadata(-1, static (_, _) => { }, static (_, _) => _row++));

        public static int GetCol(DependencyObject obj)
            => (int)obj.GetValue(ColProperty);

        public static void SetCol(DependencyObject obj, int value)
            => obj.SetValue(ColProperty, value);

        // Using a DependencyProperty as the backing store for Col.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColProperty =
            DependencyProperty.RegisterAttached("Col", typeof(int), typeof(ColRow), new PropertyMetadata(-1, static (_, _) => { }, static (_, _) => _col++));


        public static int GetHoverX(DependencyObject obj)
            => (int)obj.GetValue(HoverXProperty);

        public static void SetHoverX(DependencyObject obj, int value)
            => obj.SetValue(HoverXProperty, value);

        // Using a DependencyProperty as the backing store for HoverX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverXProperty =
            DependencyProperty.RegisterAttached("HoverX", typeof(int), typeof(ColRow), new PropertyMetadata(0));



        public static int GetHoverY(DependencyObject obj)
            => (int)obj.GetValue(HoverYProperty);

        public static void SetHoverY(DependencyObject obj, int value)
            => obj.SetValue(HoverYProperty, value);

        // Using a DependencyProperty as the backing store for HoverY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverYProperty =
            DependencyProperty.RegisterAttached("HoverY", typeof(int), typeof(ColRow), new PropertyMetadata(0));


    }
}
