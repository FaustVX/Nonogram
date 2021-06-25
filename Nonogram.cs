using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NonogramRow
{
    public static class Nonogram
    {
        public static Nonogram<T> Create<T>(T[,] pattern, T ignoredValue = default!)
        where T : notnull
            => new (pattern, ignoredValue);
    }

    public class Nonogram<T>
    where T : notnull
    {
        private readonly T[,] _pattern;
        private readonly T[,] _grid;
        public int Width { get; }
        public int Height { get; }
        public (T color, int qty, bool validated)[][] ColHints { get; }
        public (T color, int qty, bool validated)[][] RowHints { get; }
        public T[] PossibleValue { get; }
        public T IgnoredValue { get; }
        public T this[int x, int y]
        {
            get => _grid[y, x];
            set => _grid[y, x] = value;
        }

        public Nonogram(T[,] pattern, T ignoredValue)
        {
            _pattern = pattern;
            Width = _pattern.GetLength(1);
            Height = _pattern.GetLength(0);
            IgnoredValue = ignoredValue;
            _grid = new T[Height, Width];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _grid[y, x] = IgnoredValue;
            ColHints = new (T, int, bool)[Width][];
            RowHints = new (T, int, bool)[Height][];
            PossibleValue = pattern.Cast<T>().ToHashSet().Where(c => !c.Equals(IgnoredValue)).ToArray();

            for (int y = 0; y < RowHints.Length; y++)
                RowHints[y] = CalculateHints(GetRow(_pattern, y))
                    .Where(g => PossibleValue.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();

            for (int x = 0; x < ColHints.Length; x++)
                ColHints[x] = CalculateHints(GetCol(_pattern, x))
                    .Where(g => PossibleValue.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();
        }

        public Nonogram<TOther> ConvertTo<TOther>(TOther ignoredValue, params TOther[] possibleValue)
        where TOther : notnull
        {
            var pattern = new TOther[Height, Width];
            var grid = new TOther[Height, Width];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    pattern[y, x] = _pattern[y, x].ConvertTo(PossibleValue, possibleValue, ignoredValue);
                    grid[y, x] = _grid[y, x].ConvertTo(PossibleValue, possibleValue, ignoredValue);
                }
            var result = new Nonogram<TOther>(pattern, ignoredValue);
            result.GetType()
                .GetField(nameof(_grid), BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(result, grid);
            for (var i = 0; i < RowHints.Length; i++)
                for (var j = 0; j < RowHints[i].Length; j++)
                    result.RowHints[i][j].validated = RowHints[i][j].validated;
            for (var i = 0; i < ColHints.Length; i++)
                for (var j = 0; j < ColHints[i].Length; j++)
                    result.ColHints[i][j].validated = ColHints[i][j].validated;
            return result;
        }

        public void ValidateHints(int x, int y, T color)
        {
            if (!PossibleValue.Contains(color) && !(IgnoredValue.Equals(color)))
                return;
            _grid[y, x] = color;
            ValidateHints(x, y);
        }

        public void ValidateHints(int x, int y)
        {
            Validate(CalculateHints(GetCol(_grid, x).Select(g => g)), ColHints[x], PossibleValue);
            Validate(CalculateHints(GetRow(_grid, y).Select(g => g)), RowHints[y], PossibleValue);

            static void Validate(IEnumerable<(T color, int qty)> gridLine, (T color, int qty, bool validated)[] hints, T[] possibleColors)
            {
                var lineArray = gridLine
                    .Where(g => possibleColors.Contains(g.color))
                    .ToArray();

                for (int i = 0; i < hints.Length; i++)
                    hints[i].validated = false;

                for (int i = 0; i < Math.Min(lineArray.Length, hints.Length); i++)
                    if (!ValidateCell(lineArray, hints, i) && !ValidateCell(lineArray, hints, Index.FromEnd(i + 1)))
                        break;

                static bool ValidateCell((T color, int qty)[] lineArray, (T color, int qty, bool validated)[] hints, Index i)
                {
                    ref var hint = ref hints[i];
                    var c = lineArray[i];
                    if (hint.qty != c.qty || !hint.color.Equals(c.color))
                        return false;
                    hint.validated = true;
                    return true;
                }
            }
        }

        public static IEnumerable<(T color, int qty)> CalculateHints(IEnumerable<T> row)
        {
            using var enumerator = row.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            var color = enumerator.Current;
            var count = 1;

            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    yield return (color, count);
                    yield break;
                }
                if (!enumerator.Current.Equals(color))
                {
                    yield return (color, count);
                    color = enumerator.Current;
                    count = 1;
                    continue;
                }
                count++;
            }
        }

        public static IEnumerable<T> GetCol(T[,] array, int col)
            => GetRowCol(array, 0, i => array[i, col]);

        public static IEnumerable<T> GetRow(T[,] array, int row)
            => GetRowCol(array, 1, i => array[row, i]);

        private static IEnumerable<T> GetRowCol(T[,] array, int dimension, Func<int, T> get)
        {
            return array.GetLength(dimension) is not 0 and var length
                ? Execute(length, get)
                : Enumerable.Empty<T>();

            static IEnumerable<T> Execute(int length, Func<int, T> get)
            {
                for (int i = 0; i < length; i++)
                    yield return get(i);
            }
        }
    }
}
