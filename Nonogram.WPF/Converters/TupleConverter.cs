using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public class TupleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ITuple tuple)
                if (parameter is string idx && int.TryParse(idx, out var i))
                    return tuple[i]!;
                else
                    throw new ArgumentException("Parameter must be an integer", nameof(parameter));
            else
                return default!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
