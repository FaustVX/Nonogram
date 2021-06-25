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
            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Array.Empty<bool>();
                var row = Enumerable.Repeat(false, GetRng()).ToArray();
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated);
            }
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
            for (int i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = new[] { GetRng() };
                var row = Enumerable.Repeat(true, groups[0]).ToArray();
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated);
            }
        }

        [TestMethod]
        public void FailWithStuckGroups()
        {
            var groups = new[] { 1, 1 };
            var row = new[] { false, true, true, false };
            var generated = CalculateGroups(row);
            CollectionAssert.AreNotEqual(groups, generated);
        }

        [TestMethod]
        public void CreationHints()
        {
            var groupsEmpty = Array.Empty<(int color, int qty, bool validated)>();
            var groups1 = new[] { (1, 1, false) };
            var groups1_1 = new[] { (1, 1, false), (1, 1, false) };

            var nonogram = Nonogram.Create(new[,]
            {
                {0, 0, 1},
                {0, 0, 0},
                {0, 0, 1},
            });

            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1_1, nonogram.ColHints[2]);
        }

        [TestMethod]
        public void ValidationHints()
        {
            var groupsEmpty = Array.Empty<(int color, int qty, bool validated)>();
            var groups1 = new[] { (1, 1, false) };
            var groups1Validated = new[] { (1, 1, true) };
            var groups1_1 = new[] { (1, 1, false), (1, 1, false) };
            var groups1Validated_1 = new[] { (1, 1, true), (1, 1, false) };
            var groups1Validated_1Validated = new[] { (1, 1, true), (1, 1, true) };

            var nonogram = Nonogram.Create(new[,]
            {
                {0, 0, 1},
                {0, 0, 0},
                {0, 0, 1},
            });

            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 0, 1);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1Validated_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 0, 0);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 2, 1);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1Validated_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 0, 1);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1Validated_1Validated, nonogram.ColHints[2]);
        }

        [TestMethod]
        public void CheckXYCoord()
        {
            var nonogram = Nonogram.Create( new[,]
            {
                { 0, 1, 2 },
                { 3, 4, 5 }
            });

            Assert.AreEqual(3, nonogram.Width);
            Assert.AreEqual(2, nonogram.Height);
            nonogram.ValidateHints(2, 1, 1);
        }

        public void CheckIsComplete()
        {
            var nonogram = Nonogram.Create(new[,]
            {
                {0, 1},
                {0, 1},
            });

            nonogram.ValidateHints(0, 0, 0);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(1, 0, 1);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(1, 1, 1);
            Assert.AreEqual(true, nonogram.IsComplete);
            Assert.AreEqual(true, nonogram.IsCorrect);
        }
    }
}
