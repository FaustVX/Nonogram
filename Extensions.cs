using System.Collections.Generic;
using System.Linq;

namespace NonogramRow
{
    public static class Extensions
    {
        public static int[] CalculateGroups(params bool[] row)
            => CalculateGroups(row.AsEnumerable());

        public static int[] CalculateGroups(IEnumerable<bool> row)
            => Nonogram<bool>.CalculateHints(row).Where(g => g.color).Select(g => g.qty).ToArray();

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, params T[] row)
            => CalculateGroups(ignored, row.AsEnumerable());

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, IEnumerable<T> row)
            => Nonogram<T>.CalculateHints(row).Where(g => !(g.color?.Equals(ignored) ?? true)).ToArray();
    }
}
