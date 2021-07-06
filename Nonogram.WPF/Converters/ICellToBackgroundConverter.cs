using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToBackgroundConverter : IMultiValueConverter
    {
        public Brush SealedBrush { get; set; } = default!;
        public Brush IgnoredBrush { get; set; } = default!;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 2
                ? (values[0], values[1]) switch
                {
                    not (ICell, bool) => default!,
                    (EmptyCell, _) => IgnoredBrush,
                    (ColoredCell<Brush> c, _) => c.Color,
                    (_, false) => SealedBrush,
                    (_, true) => IgnoredBrush,
                } : default!;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
