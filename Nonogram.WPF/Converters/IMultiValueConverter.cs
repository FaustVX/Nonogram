using System;
using System.Globalization;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public interface IMultiValueConverter<T1, T2, TOut> : IMultiValueConverter
    {
        TOut Convert((T1, T2) values);

        object? IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 2 && values[0].CheckType<T1>(true) is var t1 && values[1].CheckType<T2>(true) is var t2
            ? Convert((t1, t2))
            : throw new Exception();

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public interface IMultiValueConverterParametered<T1, T2, TParam, TOut> : IMultiValueConverter
    {
        TOut Convert((T1, T2) values, TParam param);

        object? IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 2 && values[0].CheckType<T1>(true) is var t1 && values[1].CheckType<T2>(true) is var t2 && parameter.CheckType<TParam>(true) is var param
            ? Convert((t1, t2), param)
            : throw new Exception();

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public interface IMultiValueConverter<T1, T2, T3, TOut> : IMultiValueConverter
    {
        TOut Convert((T1, T2, T3) values);

        object? IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 3 && values[0].CheckType<T1>(true) is var t1 && values[1].CheckType<T2>(true) is var t2 && values[2].CheckType<T3>(true) is var t3
            ? Convert((t1, t2, t3))
            : throw new Exception();

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
