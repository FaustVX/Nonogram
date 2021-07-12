using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public interface IValueConverter<in TIn, out TOut> : IValueConverter
    {
        TOut Convert(TIn value);

        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value.CheckType<TIn>(true) is var @in ? Convert(@in) : throw new Exception();

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public interface IValueConverter<in TIn, in TParam, out TOut> : IValueConverter
    {
        private static readonly MethodInfo? _parse = typeof(TParam).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
        TOut Convert(TIn value, TParam parameter);

        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value.CheckType<TIn>(true), parameter.CheckType<TParam, string>(true)) switch
            {
                (TIn @in, TParam param)
                    => Convert(@in, param),
                (TIn @in, string s) when _parse?.Invoke(null, new[] { s }) is TParam param
                    => Convert(@in, param),
                _ => throw new Exception(),
            };

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
