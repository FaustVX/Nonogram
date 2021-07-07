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
            => values.Length is 3
                ? (values[0], values[1], values[2]) switch
                {
                    not (ICell, Brush or null, bool) => default!,
                    (EmptyCell, _, _) => IgnoredBrush,
                    (ColoredCell<Brush> c, _, _) => c.Color,
                    (SealedCell<Brush> { Seals: var seals }, Brush brush, false) when !seals.Contains(brush) => IgnoredBrush,
                    (_, _, false) => SealedBrush,
                    (_, _, true) => IgnoredBrush,
                } : default!;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
