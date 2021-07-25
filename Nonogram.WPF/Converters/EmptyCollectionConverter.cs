using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class EmptyCollectionConverter : IValueConverter<ICollection<Game<Brush>.Hint>?, IEnumerable<Game<Brush>.Hint>>
    {
        public IEnumerable<Game<Brush>.Hint> Convert(ICollection<Game<Brush>.Hint>? value)
            => value is not { Count: > 0 } ? Enumerable.Repeat(new Game<Brush>.Hint(null!, 0) { Validated = true }, 1) : value;
    }
}
