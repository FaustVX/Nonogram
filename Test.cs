using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static NonogramRow.Extensions;

namespace NonogramRow
{
    [TestClass]
    public class Test
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
        public void EmptyRow()
        {
            var groups = Array.Empty<bool>();
            var row = Array.Empty<bool>();
            var generated = CalculateGroups(row);
            CollectionAssert.AreEqual(groups, generated);
        }

        [TestMethod]
        public void SingleGroupTrimmed()
        {
            var groups = new[] { GetRng() };
            var row = Enumerable.Repeat(true, groups[0]).ToArray();
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
    }
}
