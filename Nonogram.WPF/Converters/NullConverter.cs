using System;
using System.Globalization;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public class NullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is false ? parameter : null!;
    }
}
