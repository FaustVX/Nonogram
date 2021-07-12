using System.Collections;
using System.Linq;

namespace Nonogram.WPF.Converters
{
    public class EmptyCollectionConverter : IValueConverter<ICollection?, IEnumerable>
    {
        public IEnumerable Convert(ICollection? value)
            => value is not { Count: > 0 } ? Enumerable.Repeat(((object?)null, 0, true), 1) : value;
    }
}
