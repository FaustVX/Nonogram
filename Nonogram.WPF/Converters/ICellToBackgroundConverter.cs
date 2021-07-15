using System.Collections;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToBackgroundConverter : IMultiValueConverter<ICell, IList, int, bool, Brush>
    {
        public Brush SealedBrush { get; set; } = default!;
        public Brush IgnoredBrush { get; set; } = default!;

        public Brush Convert((ICell, IList, int, bool) values)
            => (values.Item1, values.Item2[values.Item3], values.Item4) switch
            {
                (EmptyCell, _, _) => IgnoredBrush,
                (ColoredCell<Brush> c, _, _) => c.Color,
                (SealedCell<Brush> { Seals: var seals }, Brush brush, false) when !seals.Contains(brush) => IgnoredBrush,
                (_, _, false) => SealedBrush,
                (_, _, true) => IgnoredBrush,
            };
    }
}
