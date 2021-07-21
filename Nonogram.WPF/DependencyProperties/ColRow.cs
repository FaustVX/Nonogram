using System.Windows;

namespace Nonogram.WPF.DependencyProperties
{
    public class ColRow
    {
        public static class Helper
        {
            public static int SetRow(DependencyObject obj)
            {
                var window = GetSource(obj)!;
                var max = GetMaxRow(window);
                SetMaxRow(window, max + 1);
                return max;
            }

            public static int SetCol(DependencyObject obj)
            {
                var window = GetSource(obj)!;
                var max = GetMaxCol(window);
                SetMaxCol(window, max + 1);
                return max;
            }

            public static void Reset(Window window)
            {
                SetMaxCol(window, 0);
                SetMaxRow(window, 0);
            }
        }

        public static Window? GetSource(DependencyObject obj)
            => (Window?)obj.GetValue(SourceProperty);

        public static void SetSource(DependencyObject obj, Window? value)
            => obj.SetValue(SourceProperty, value);

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(Window), typeof(ColRow), new PropertyMetadata(null));


        public static int GetRow(DependencyObject obj)
            => (int)obj.GetValue(RowProperty);

        public static void SetRow(DependencyObject obj, int value)
            => obj.SetValue(RowProperty, value);

        // Using a DependencyProperty as the backing store for Row.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.RegisterAttached("Row", typeof(int), typeof(ColRow), new PropertyMetadata(-1, static (_, _) => { }, static (d, _) => Helper.SetRow(d)));

        public static int GetCol(DependencyObject obj)
            => (int)obj.GetValue(ColProperty);

        public static void SetCol(DependencyObject obj, int value)
            => obj.SetValue(ColProperty, value);

        // Using a DependencyProperty as the backing store for Col.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColProperty =
            DependencyProperty.RegisterAttached("Col", typeof(int), typeof(ColRow), new PropertyMetadata(-1, static (_, _) => { }, static (d, _) => Helper.SetCol(d)));


        public static int GetMaxRow(DependencyObject obj)
            => (int)obj.GetValue(MaxRowProperty);

        public static void SetMaxRow(DependencyObject obj, int value)
            => obj.SetValue(MaxRowProperty, value);

        // Using a DependencyProperty as the backing store for MaxRow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxRowProperty =
            DependencyProperty.RegisterAttached("MaxRow", typeof(int), typeof(ColRow), new PropertyMetadata(0));



        public static int GetMaxCol(DependencyObject obj)
            => (int)obj.GetValue(MaxColProperty);

        public static void SetMaxCol(DependencyObject obj, int value)
            => obj.SetValue(MaxColProperty, value);

        // Using a DependencyProperty as the backing store for MaxCol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColProperty =
            DependencyProperty.RegisterAttached("MaxCol", typeof(int), typeof(ColRow), new PropertyMetadata(0));




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
