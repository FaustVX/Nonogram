using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class EmptyCollectionConverter : IValueConverter<Game<Brush>.HintGroup?, IEnumerable<Game<Brush>.Hint>>
    {
        public IEnumerable<Game<Brush>.Hint> Convert(Game<Brush>.HintGroup? value)
            => value is not { Length: > 0 } ? Enumerable.Repeat(new Game<Brush>.Hint(null!, 0) { Validated = true }, 1) : value;
    }
}
