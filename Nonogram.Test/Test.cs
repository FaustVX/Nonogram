using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using static Nonogram.Extensions;

namespace Nonogram.Test
{
    [TestClass]
    public class Test
    {
        public Test()
        {
            var methods = string.Join(", ", GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false) is not { Length: > 0 })
                .Select(m => m.Name));
            if (!string.IsNullOrEmpty(methods))
                Assert.Fail($"'{methods}' won't be unit tested");
        }

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
            for (var i = GetRng() - 1; i >= 0 ; i--)
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
            for (var i = GetRng() - 1; i >= 0 ; i--)
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
            for (var i = GetRng() - 1; i >= 0 ; i--)
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
            for (var i = GetRng() - 1; i >= 0 ; i--)
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
            for (var i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = Array.Empty<bool>();
                var row = Enumerable.Repeat(false, GetRng()).ToArray();
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
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
            for (var i = GetRng() - 1; i >= 0 ; i--)
            {
                var groups = new[] { GetRng() };
                var row = Enumerable.Repeat(true, groups[0]).ToArray();
                var generated = CalculateGroups(row);
                CollectionAssert.AreEqual(groups, generated, $"Iteration {i}");
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

            var nonogram = Game.Create(new[,]
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

            var nonogram = Game.Create(new[,]
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

            nonogram.ValidateHints(2, 0, 1, false);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1Validated_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 0, 0, false);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 2, 1, false);
            CollectionAssert.AreEqual(groups1, nonogram.RowHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.RowHints[1]);
            CollectionAssert.AreEqual(groups1Validated, nonogram.RowHints[2]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[0]);
            CollectionAssert.AreEqual(groupsEmpty, nonogram.ColHints[1]);
            CollectionAssert.AreEqual(groups1Validated_1, nonogram.ColHints[2]);

            nonogram.ValidateHints(2, 0, 1, false);
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
            var nonogram = Game.Create( new[,]
            {
                { 0, 1, 2 },
                { 3, 4, 5 }
            });

            Assert.AreEqual(3, nonogram.Width);
            Assert.AreEqual(2, nonogram.Height);
            nonogram.ValidateHints(2, 1, 1, false);
        }

        [TestMethod]
        public void CheckIsComplete()
        {
            var nonogram = Game.Create(new[,]
            {
                {0, 1},
                {0, 1},
            });

            nonogram.ValidateHints(0, 0, 0, false);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(1, 0, 1, false);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(0, 1, 1, true);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(0, 0, 0, true);
            Assert.AreEqual(false, nonogram.IsComplete);
            Assert.AreEqual(false, nonogram.IsCorrect);
            nonogram.ValidateHints(1, 1, 1, false);
            Assert.AreEqual(true, nonogram.IsComplete);
            Assert.AreEqual(true, nonogram.IsCorrect);
        }

        [TestMethod]
        public void TestSeals()
        {
            var nonogram = Game.Create( new[,]
            {
                { 0, 1, 2 },
                { 3, 4, 5 }
            });

            var x = 0;
            var y = 0;
            Assert.IsInstanceOfType(nonogram[x, y], typeof(EmptyCell));
            nonogram.ValidateHints(x, y, 1, false);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(ColoredCell<int>));
            Assert.AreEqual(1, ((ColoredCell<int>)nonogram[x, y]).Color);
            nonogram.ValidateHints(x, y, 1, true);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(EmptyCell));
            nonogram.ValidateHints(x, y, 1, true);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(SealedCell<int>));
            Assert.AreEqual(1, ((SealedCell<int>)nonogram[x, y]).Seals[0]);

            y = 1;
            nonogram.ValidateHints(x, y, 1, true);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(SealedCell<int>));
            Assert.AreEqual(1, ((SealedCell<int>)nonogram[x, y]).Seals[0]);
            nonogram.ValidateHints(x, y, 2, true);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(SealedCell<int>));
            Assert.AreEqual(1, ((SealedCell<int>)nonogram[x, y]).Seals[0]);
            Assert.AreEqual(2, ((SealedCell<int>)nonogram[x, y]).Seals[1]);
            nonogram.ValidateHints(x, y, 3, true);
            nonogram.ValidateHints(x, y, 4, true);
            nonogram.ValidateHints(x, y, 5, true);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(AllColoredSealCell));

            nonogram.ValidateHints(x, y, 5, false);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(SealedCell<int>));
            Assert.AreEqual(1, ((SealedCell<int>)nonogram[x, y]).Seals[0]);
            Assert.AreEqual(2, ((SealedCell<int>)nonogram[x, y]).Seals[1]);
            Assert.AreEqual(3, ((SealedCell<int>)nonogram[x, y]).Seals[2]);
            Assert.AreEqual(4, ((SealedCell<int>)nonogram[x, y]).Seals[3]);
            nonogram.ValidateHints(x, y, 4, false);
            nonogram.ValidateHints(x, y, 3, false);
            nonogram.ValidateHints(x, y, 2, false);
            nonogram.ValidateHints(x, y, 1, false);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(EmptyCell));

            nonogram.ValidateHints(x, y, 1, false);
            Assert.IsInstanceOfType(nonogram[x, y], typeof(ColoredCell<int>));
        }

        [TestMethod]
        public void CheckUndoRedo()
        {
            var nonogram = Game.Create(new[,]
            {
                { 0, 1 },
                { 2, 3 },
            });

            var x = 1;
            var y = 0;
            var currentCell = nonogram[x, y];
            nonogram.ValidateHints(x, y, 1, seal: false);
            var nextCell = nonogram[x, y];

            Assert.IsInstanceOfType(nonogram[x, y], typeof(ColoredCell<int>));
            CollectionAssert.AreEqual(nonogram.ColHints[x], new[] { (1, 1, true), (3, 1, false) });

            nonogram.Undo();
            Assert.IsTrue(object.ReferenceEquals(currentCell, nonogram[x, y]));
            CollectionAssert.AreEqual(nonogram.ColHints[x], new[] { (1, 1, false), (3, 1, false) });

            nonogram.Redo();
            Assert.IsTrue(object.ReferenceEquals(nextCell, nonogram[x, y]));
            CollectionAssert.AreEqual(nonogram.ColHints[x], new[] { (1, 1, true), (3, 1, false) });
        }

        [TestMethod]
        public void CheckResizeArray()
        {
            var array0 = new[,]
            {
                { 1,1,2,2 },
                { 1,1,2,2 },
                { 3,3,4,4 },
                { 3,3,4,4 },
            }.ReduceArray(2, 2, s => s[0, 0]);
            Assert.AreEqual(2, array0.GetLength(0));
            Assert.AreEqual(2, array0.GetLength(1));
            Assert.AreEqual(1, array0[0, 0]);
            Assert.AreEqual(2, array0[0, 1]);
            Assert.AreEqual(3, array0[1, 0]);
            Assert.AreEqual(4, array0[1, 1]);

            var i = 0;
            var array1 = new[,]
            {
                { i++, i++, i++, i++, i++ },
                { i++, i++, i++, i++, i++ },
                { i++, i++, i++, i++, i++ },
                { i++, i++, i++, i++, i++ },
                { i++, i++, i++, i++, i++ },
            }.ReduceArray(2, 2, s => ((int)s.Average()));
            Assert.AreEqual(2, array1.GetLength(0));
            Assert.AreEqual(2, array1.GetLength(1));

            var array2 = array0.ReduceArray(1, 1, s => (s.Average()));
            Assert.AreEqual(1, array2.GetLength(0));
            Assert.AreEqual(1, array2.GetLength(1));
            Assert.AreEqual(2.5, array2[0, 0], 0);
        }
    }
}
