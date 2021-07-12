using System;
using System.Runtime.CompilerServices;

namespace Nonogram.WPF.Converters
{
    public class TupleConverter : IValueConverter<ITuple, string, object?>
    {
        public object? Convert(ITuple value, string parameter)
            => int.TryParse(parameter, out var i)
                ? value[i]
                : throw new ArgumentException("Parameter must be an integer", nameof(parameter));
    }
}
