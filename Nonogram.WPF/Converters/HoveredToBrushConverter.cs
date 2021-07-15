using Nonogram.WPF.DependencyProperties;
using System.Windows;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class HoveredToBrushConverter : IMultiValueConverter<DependencyObject, int, Brush>
    {
        public Brush HoveredBrush { get; set; } = default!;

        public Brush Convert((DependencyObject, int) values)
            => values switch
            {
                (DependencyObject dp, int i) when ColRow.GetCol(dp) == i
                    => HoveredBrush,
                (DependencyObject dp, int i) when ColRow.GetRow(dp) == i
                    => HoveredBrush,
                (_, _)
                    => Brushes.Transparent,
            };
    }
}
