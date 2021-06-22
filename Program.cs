using System;
using System.Linq;

namespace NonogramRow
{
    public static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
            var groups0 = CalculateGroups(false, true, true, false);
                groups0 = CalculateGroups(true);
                groups0 = CalculateGroups(false);
                groups0 = CalculateGroups();
            var groups1 = CalculateGroups(0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 5, 0, 2, 1);
            var nonogram = Nonogram.Create(new[,]
            {
                {0, 0, 1, 0, 0},
                {0, 1, 1, 1, 0},
                {1, 1, 1, 1, 1},
                {1, 1, 2, 1, 1},
                {1, 1, 2, 1, 1},
            });
        }

        public static int[] CalculateGroups(params bool[] row)
            => Nonogram<bool>.CalculateHints(row).Where(g => g.color).Select(g => g.qty).ToArray();

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, params T[] row)
            => Nonogram<T>.CalculateHints(row).Where(g => !(g.color?.Equals(ignored) ?? true)).ToArray();
    }
}
