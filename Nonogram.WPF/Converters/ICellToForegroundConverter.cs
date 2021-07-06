using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToForegroundConverter : IMultiValueConverter
    {
        public Brush IgnoredBrush { get; set; } = default!;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 2
                ? (values[0], values[1]) switch
                {
                    (SealedCell<Brush> { Seals: var seals }, Brush currentColor) when seals.Contains(currentColor) => currentColor,
                    (AllColoredSealCell, _) => IgnoredBrush,
                    _ => Brushes.Transparent
                } : default!;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
