using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SolidColorBrush brush ? brush.Color : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
