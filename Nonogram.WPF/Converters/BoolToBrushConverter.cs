using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush? True { get; set; }
        public Brush? False { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? True : False;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
