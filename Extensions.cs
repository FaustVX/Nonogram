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
            where T : notnull
            => CalculateGroups(ignored, row.AsEnumerable());

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, IEnumerable<T> row)
            where T : notnull
            => Nonogram<T>.CalculateHints(row).Where(g => !g.color.Equals(ignored)).ToArray();

        public static TOut ConvertTo<TIn, TOut>(this TIn value, TIn[] array, TOut[] possibleValues, TOut ignoredValue)
        {
            for (int i = 0; i < array.Length; i++)
                if(object.Equals(value, array[i]))
                    return possibleValues[i];
            return ignoredValue;
        }
    }
}
