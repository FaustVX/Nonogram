using System.Linq;

namespace NonogramRow
{
    public static class Extensions
    {
        public static int? FirstIndexOfDifferent<T>(this T[] source, T value, int searchAfter)
        {
            for (var i = searchAfter; i < source.Length; i++)
                if (!(source[i]?.Equals(value) ?? true))
                    return i;
            return null;
        }

        public static int[] CalculateGroups(params bool[] row)
            => Nonogram<bool>.CalculateHints(row).Where(g => g.color).Select(g => g.qty).ToArray();

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, params T[] row)
            => Nonogram<T>.CalculateHints(row).Where(g => !(g.color?.Equals(ignored) ?? true)).ToArray();
    }
}
