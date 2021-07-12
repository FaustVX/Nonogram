using System;

namespace Nonogram.WPF.Converters
{
    public class MultiplyConverter : IMultiValueConverterParametered<bool, double, string, double>
    {
        public double Convert((bool, double) values, string param)
            => values.Item2 * (values.Item1 ? int.Parse(param) : 1);
    }
}
