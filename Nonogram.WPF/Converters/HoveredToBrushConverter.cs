using Nonogram.WPF.Controls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class HoveredToBrushConverter : IMultiValueConverter
    {
        public Brush HoveredBrush { get; set; } = default!;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length is 2
            ? (values[0], values[1]) switch
            {
                (DependencyObject dp, int i) when ColRow.GetCol(dp) == i
                    => HoveredBrush,
                (DependencyObject dp, int i) when ColRow.GetRow(dp) == i
                    => HoveredBrush,
                (DependencyObject, int)
                    => Brushes.Transparent,
            } : default!;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
