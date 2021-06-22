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
        private readonly Random rng = new();

        private bool[] GenerateRow(int[] groups, bool trimmStart, bool trimmEnd)
        {
            var row = trimmStart ? Enumerable.Empty<bool>() : Enumerable.Repeat(false, GetRng());
            foreach (var group in groups[trimmEnd ? ..^1 : ..^0])
                row = row.Concat(Enumerable.Repeat(true, group)).Concat(Enumerable.Repeat(false, GetRng()));
            if (trimmEnd)
                row = row.Concat(Enumerable.Repeat(true, groups[^1]));
            return row.ToArray();
        }

        private int GetRng()
            => rng.Next(1, 5);

        [TestMethod]
        public void GeneratedRows()
        {

            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Enumerable.Range(0, GetRng()).Select(_ => GetRng()).ToArray();
                var row = GenerateRow(groups, false, false);
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
            }
        }

        [TestMethod]
        public void GeneratedRowsTrimmedStart()
            {
            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Enumerable.Range(0, GetRng()).Select(_ => GetRng()).ToArray();
                var row = GenerateRow(groups, true, false);
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
            }
            }

        [TestMethod]
        public void GeneratedRowsTrimmedEnd()
        {
            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Enumerable.Range(0, GetRng()).Select(_ => GetRng()).ToArray();
                var row = GenerateRow(groups, false, true);
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
            }
        }

        [TestMethod]
        public void GeneratedRowsTrimmed()
        {
            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Enumerable.Range(0, GetRng()).Select(_ => GetRng()).ToArray();
                var row = GenerateRow(groups, true, true);
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
            }
        }

        [TestMethod]
        public void EmptyGroup()
        {
            var groups = Array.Empty<bool>();
            var row = Enumerable.Repeat(false, GetRng()).ToArray();
            var generated = CalculateGroups(row);
            CollectionAssert.AreEqual(groups, generated);
        }

        [TestMethod]
        public void FailWithStuckGroups()
        {
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
