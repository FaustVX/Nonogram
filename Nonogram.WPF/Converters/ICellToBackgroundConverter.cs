using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToBackgroundConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var window = (MainWindow)parameter;
            return value switch
            {
                EmptyCell => window.Nonogram.IgnoredColor,
                ColoredCell<Brush> c => c.Color,
                AllColoredSealCell => window.CurrentColor,
                SealedCell<Brush> seal when seal.Seals.Contains(window.CurrentColor) => window.CurrentColor,
                _ => value,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
