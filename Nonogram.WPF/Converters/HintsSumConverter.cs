using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class HintsSumConverter : IValueConverter
    {
        public string StringFormat { get; set; } = "{0}"; // The StringFormat of Binding doesn't works !

        public static int Convert(IEnumerable<Game<Brush>.Hint> value)
            => value.Sum(h => h.Total);
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
            => string.Format(StringFormat, Convert(((IEnumerable)value).Cast<Game<Brush>.Hint>())); // I don't know why a cast to IEnumerable has to be used instead of simple cast !
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture) => throw new System.NotImplementedException();
    }
}
