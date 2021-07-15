using System.Collections;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToForegroundConverter : IMultiValueConverter<ICell, IList, int, bool, Brush>
    {
        public Brush IgnoredBrush { get; set; } = default!;
        public Brush Convert((ICell, IList, int, bool) values)
            => (values.Item1, values.Item2[values.Item3], values.Item4) switch
            {
                (_, _, true) => Brushes.Transparent,
                (SealedCell<Brush> { Seals: var seals }, Brush currentColor, false) when seals.Contains(currentColor) => currentColor,
                (AllColoredSealCell, _, _) => IgnoredBrush,
                (_, _, _) => Brushes.Transparent
            };
    }
}
