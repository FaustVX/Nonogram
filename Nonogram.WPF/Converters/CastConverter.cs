using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Nonogram.WPF.Converters
{
    public class CastConverter : IValueConverter
    {
        private readonly static Dictionary<(Type @in, Type @out), MethodInfo> _converter = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, parameter as Type ?? targetType);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, parameter as Type ?? targetType);

        private static object Convert(object? value, Type targetType)
        {
            try
            {
                if (targetType.IsConstructedGenericType && targetType.GetGenericTypeDefinition() is Type openType && openType == typeof(Nullable<>))
                    targetType = targetType.GenericTypeArguments[0];
                return System.Convert.ChangeType(value, targetType)!;
            }
            catch (InvalidCastException) when (value is not null)
            {
                return Cast(value.GetType(), targetType).Invoke(null, new[] { value })!;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        private static MethodInfo Cast(Type @in, Type @out)
            => _converter.TryGetValue((@in, @out), out var method) ? method : _converter[(@in, @out)] = (GetCast(@in, @in, @out) ?? GetCast(@out, @in, @out)!);

        private static MethodInfo? GetCast(Type type, Type @in, Type @out)
            => type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name is "op_Implicit" or "op_Explicit")
                .Where(m => m.GetParameters() is { Length: 1 } param && param[0].ParameterType == @in)
                .FirstOrDefault(m => m.ReturnType == @out);
    }
}
