using System.Buffers;
using System.Collections.Generic;
using System.Linq;

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
        public T[,] Grid { get; }
        public int Width { get; }
        public int Height { get; }
        public (T color, int qty, bool validated)[][] ColHints { get; }
        public (T color, int qty, bool validated)[][] RowHints { get; }
        public T[] PossibleValue { get; }
        public T IgnoredValue { get; }

        public Nonogram(T[,] pattern, T ignoredValue)
        {
            _pattern = pattern;
            Width = _pattern.GetLength(0);
            Height = _pattern.GetLength(1);
            IgnoredValue = ignoredValue;
            Grid = new T[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Grid[x, y] = IgnoredValue;
            ColHints = new (T, int, bool)[Height][];
            RowHints = new (T, int, bool)[Width][];
            PossibleValue = pattern.Cast<T>().ToHashSet().Where(c => !c.Equals(IgnoredValue)).ToArray();

            for (int x = 0; x < Width; x++)
                RowHints[x] = CalculateHints(GetCol(_pattern, x))
                    .Where(g => PossibleValue.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();

            for (int y = 0; y < Height; y++)
                ColHints[y] = CalculateHints(GetRow(_pattern, y))
                    .Where(g => PossibleValue.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();
        }

        public void ValidateHints(int x, int y, T color)
        {
            if (!PossibleValue.Contains(color) && !(IgnoredValue.Equals(color)))
                return;
            Grid[x, y] = color;
            ValidateHints(x, y);
        }

        public void ValidateHints(int x, int y)
        {
            Validate(CalculateHints(GetCol(Grid, x).Select(g => g)), ColHints[x], PossibleValue);
            Validate(CalculateHints(GetRow(Grid, y).Select(g => g)), RowHints[y], PossibleValue);

            static void Validate(IEnumerable<(T color, int qty)> gridLine, (T color, int qty, bool validated)[] hints, T[] possibleColors)
            {
                var lineArray = gridLine
                    .Where(g => possibleColors.Contains(g.color))
                    .ToArray();

                for (int i = 0; i < hints.Length; i++)
                    hints[i].validated = false;

                for (int i = 0; i < System.Math.Min(lineArray.Length, hints.Length); i++)
                    if (!ValidateCell(lineArray, hints, i))
                        break;
                for (int i = System.Math.Min(lineArray.Length, hints.Length) - 1; i >= 0 ; i--)
                    if (!ValidateCell(lineArray, hints, i))
                        break;

                static bool ValidateCell((T color, int qty)[] lineArray, (T color, int qty, bool validated)[] hints, int i)
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
            => GetRowCol(array, 1, i => array[col, i]);

        public static IEnumerable<T> GetRow(T[,] array, int row)
            => GetRowCol(array, 0, i => array[i, row]);

        private static IEnumerable<T> GetRowCol(T[,] array, int dimension, System.Func<int, T> get)
        {
            return array.GetLength(dimension) is not 0 and var length
                ? Execute(length, get)
                : Enumerable.Empty<T>();

            static IEnumerable<T> Execute(int length, System.Func<int, T> get)
            {
                for (int i = 0; i < length; i++)
                    yield return get(i);
            }
        }
    }
}
