using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public class BoolToTConverter<T> : IValueConverter
        where T : notnull
    {
        public BoolToTConverter()
        { }

        public BoolToTConverter(T @true, T @false)
        {
            True = @true;
            False = @false;
        }

        public BoolToTConverter(string @true, string @false)
        {
            Parse ??= typeof(T).GetMethod("Parse", new[] { typeof(string) });
            True = (T)Parse!.Invoke(null, new[] { @true })!;
            False = (T)Parse!.Invoke(null, new[] { @false })!;
        }

        public T True { get; set; } = default!;
        public T False { get; set; } = default!;

        private static MethodInfo? Parse;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? True : False;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
