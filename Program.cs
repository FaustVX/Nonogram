using System;
using System.Collections.Generic;
using System.Linq;

namespace NonogramRow
{
#if TEST
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    [TestClass]
    public class Program
    {
        [TestMethod]
        public void GeneratedRows()
        {
            Random rng = new();

            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Enumerable.Range(0, GetRng()).Select(_ => GetRng()).ToArray();
                var row = GenerateRow(groups);
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
            }

            bool[] GenerateRow(int[] groups)
            {
                var row = Enumerable.Repeat(false, GetRng());
                foreach (var group in groups)
                    row = row.Concat(Enumerable.Repeat(true, group)).Concat(Enumerable.Repeat(false, GetRng()));
                return row.ToArray();
            }

            int GetRng()
                => rng.Next(1, 5);
        }

        [TestMethod]
        public void EmptyGroup()
        {
            Random rng = new();

            var groups = Array.Empty<bool>();
            var row = Enumerable.Repeat(false, GetRng()).ToArray();
            var generated = CalculateGroups(row);
            CollectionAssert.AreEqual(groups, generated);

            int GetRng()
                => rng.Next(1, 5);
        }

        [TestMethod]
        public void FailWithStuckGroups()
        {
            Random rng = new();

            var groups = new[] { 1, 1 };
            var row = new[] { false, true, true, false };
            var generated = CalculateGroups(row);
            CollectionAssert.AreNotEqual(groups, generated);
        }
#else
    public static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
            var groups = CalculateGroups(false, true, true, false);
        }
#endif
        private static int[] CalculateGroups(params bool[] row)
        {
            return Calculate(row).ToArray();

            static IEnumerable<int> Calculate(bool[] row)
            {
                var last = -1;

                while (true)
                {
                    var first = row.FirstIndexOf(true, last);
                    if (!first.HasValue)
                        break;
                    last = row.FirstIndexOf(false, first.Value) ?? 0;
                    yield return last - first.Value;
                }
            }
        }
    }

    public static class Extensions
    {
        public static int? FirstIndexOf<T>(this T[] source, T value, int searchAfter)
        {
            for (var i = searchAfter + 1; i < source.Length; i++)
                if (source[i]?.Equals(value) ?? false)
                    return i;
            return null;
        }
    }
}
