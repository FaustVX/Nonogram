using System.Collections.Generic;
using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class SelectedColorConverter : IMultiValueConverter<IList<Game<Brush>.Color>, int, Brush>
    {
        public Brush Convert((IList<Game<Brush>.Color>, int) values)
            => values.Item1?[values.Item2].Value;
    }
}
