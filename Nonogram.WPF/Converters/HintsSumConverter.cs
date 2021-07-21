using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public class HintsSumConverter : IValueConverter
    {
        public string StringFormat { get; set; } = "{0}"; // The StringFormat of Binding doesn't works !

        public int Convert(IEnumerable<ITuple> value, string parameter)
            => int.TryParse(parameter, out var pos) ? value.Sum(t => (int)t[pos]) : -1;
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
            => string.Format(StringFormat, Convert(((IEnumerable)value).Cast<ITuple>(), (string)parameter)); // I don't know why a cast to IEnumerable has to be used instead of simple cast !
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture) => throw new System.NotImplementedException();
    }
}
