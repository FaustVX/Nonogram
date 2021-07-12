using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToBackgroundConverter : IMultiValueConverter<ICell, Brush?, bool, Brush>
    {
        public Brush SealedBrush { get; set; } = default!;
        public Brush IgnoredBrush { get; set; } = default!;

        public Brush Convert((ICell, Brush?, bool) values)
            => values switch
            {
                (EmptyCell, _, _) => IgnoredBrush,
                (ColoredCell<Brush> c, _, _) => c.Color,
                (SealedCell<Brush> { Seals: var seals }, Brush brush, false) when !seals.Contains(brush) => IgnoredBrush,
                (_, _, false) => SealedBrush,
                (_, _, true) => IgnoredBrush,
            };
    }
}
