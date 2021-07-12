using System;

namespace Nonogram.WPF
{
    public static class Extensions
    {
        public static T? CheckType<T>(this object? value, bool allowNull)
            => (value, allowNull) switch
            {
                (null, true) => default,
                (T t, _) => t,
                _ => throw new NotImplementedException(),
            };
        public static object? CheckType<T1, T2>(this object? value, bool allowNull)
            => (value, allowNull) switch
            {
                (null, true) => default,
                (T1 t, _) => t,
                (T2 t, _) => t,
                _ => throw new NotImplementedException(),
            };
    }
}
