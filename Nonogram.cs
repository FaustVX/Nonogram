using System.Collections.Generic;
using System.Linq;

namespace NonogramRow
{
    public static class Nonogram
    {
        public static Nonogram<T> Create<T>(T[,] pattern, T ignoredValue = default!)
            => new (pattern, ignoredValue);
    }

    public class Nonogram<T>
    {
        private readonly T[,] _pattern;
        public T[,] Grid { get; }
        public int Width { get; }
        public int Height { get; }
        public (T color, int qty)[][] ColHints { get; }
        public (T color, int qty)[][] RowHints { get; }
        public T[] PossibleValue { get; }

        public Nonogram(T[,] pattern, T ignoredValue)
        {
            _pattern = pattern;
            Width = _pattern.GetLength(0);
            Height = _pattern.GetLength(1);
            Grid = new T[Width, Height];
            ColHints = new (T color, int qty)[Height][];
            RowHints = new (T color, int qty)[Width][];
            PossibleValue = pattern.Cast<T>().ToHashSet().Where(c => !(c?.Equals(ignoredValue) ?? true)).ToArray();

            for (int x = 0; x < Width; x++)
                RowHints[x] = CalculateHints(GetCol(_pattern, x)).Where(g => PossibleValue.Contains(g.color)).ToArray();

            for (int y = 0; y < Height; y++)
                ColHints[y] = CalculateHints(GetRow(_pattern, y)).Where(g => PossibleValue.Contains(g.color)).ToArray();
        }

        public static IEnumerable<(T color, int qty)> CalculateHints(T[] row)
        {
            if (row.Length == 0)
                yield break;

            var last = 0;

            while (true)
            {
                var color = row[last];
                var lastIndex = row.FirstIndexOfDifferent(color, last);
                if (!lastIndex.HasValue)
                {
                    yield return (color, row.Length - last);
                    break;
                }
                yield return (color, lastIndex.GetValueOrDefault() - last);
                last = lastIndex.GetValueOrDefault();
            }
        }

        public static T[] GetCol(T[,] array, int col)
            => GetRowCol(array, 1, i => array[col, i]);

        public static T[] GetRow(T[,] array, int row)
            => GetRowCol(array, 0, i => array[i, row]);

        private static T[] GetRowCol(T[,] array, int dimension, System.Func<int, T> get)
        {
            var length = array.GetLength(dimension);
            if (length == 0)
                return System.Array.Empty<T>();

            var result = new T[length];
            for (int i = 0; i < length; i++)
                result[i] = get(i);

            return result;
        }
    }
}