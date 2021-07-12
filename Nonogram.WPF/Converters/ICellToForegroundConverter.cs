using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class ICellToForegroundConverter : IMultiValueConverter<ICell, Brush, bool, Brush>
    {
        public Brush IgnoredBrush { get; set; } = default!;
        public Brush Convert((ICell, Brush, bool) values)
            => values switch
            {
                (_, _, true) => Brushes.Transparent,
                (SealedCell<Brush> { Seals: var seals }, Brush currentColor, false) when seals.Contains(currentColor) => currentColor,
                (AllColoredSealCell, _, _) => IgnoredBrush,
                (_, _, _) => Brushes.Transparent
            };
    }
}
