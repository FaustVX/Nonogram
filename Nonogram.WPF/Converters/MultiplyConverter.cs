using System;

namespace Nonogram.WPF.Converters
{
    public class MultiplyConverter : IMultiValueConverterParametered<bool, double, string, double>, IValueConverter<double, string, double>
    {
        public double Convert((bool, double) values, string param)
            => values.Item2 * (values.Item1 ? double.Parse(param) : 1);

        public double Convert(double value, string parameter)
            => value * double.Parse(parameter);
    }
}
