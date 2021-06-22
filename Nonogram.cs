using System.Buffers;
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
            ColHints = new (T color, int qty)[Height][];
            RowHints = new (T color, int qty)[Width][];
            PossibleValue = pattern.Cast<T>().ToHashSet().Where(c => !(c?.Equals(IgnoredValue) ?? true)).ToArray();

            for (int x = 0; x < Width; x++)
                RowHints[x] = CalculateHints(GetCol(_pattern, x)).Where(g => PossibleValue.Contains(g.color)).ToArray();

            for (int y = 0; y < Height; y++)
                ColHints[y] = CalculateHints(GetRow(_pattern, y)).Where(g => PossibleValue.Contains(g.color)).ToArray();
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
                if (!(enumerator.Current?.Equals(color) ?? false))
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
