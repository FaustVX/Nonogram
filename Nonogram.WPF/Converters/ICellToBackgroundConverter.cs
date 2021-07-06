using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToBackgroundConverter : IValueConverter
    {
        public Brush SealedBrush { get; set; } = default!;
        public Brush IgnoredBrush { get; set; } = default!;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                EmptyCell => IgnoredBrush,
                ColoredCell<Brush> c => c.Color,
                _ => SealedBrush,
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
