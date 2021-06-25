using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nonogram
{
    public static class Game
    {
        public static Game<T> Create<T>(T[,] pattern, T ignoredColor = default!)
        where T : notnull
            => new (pattern, ignoredColor);
    }

    public class Game<T>
    where T : notnull
    {
        private readonly T[,] _pattern;
        private readonly T[,] _grid;
        public int Width { get; }
        public int Height { get; }
        public (T color, int qty, bool validated)[][] ColHints { get; }
        public (T color, int qty, bool validated)[][] RowHints { get; }
        public T[] PossibleColors { get; }
        public T IgnoredColor { get; }
        public bool IsComplete { get; private set; }
        public bool IsCorrect { get; private set; }
        public T this[int x, int y]
        {
            get => _grid[y, x];
            set => _grid[y, x] = value;
        }

        public Game(T[,] pattern, T ignoredColor)
        {
            _pattern = pattern;
            Width = _pattern.GetLength(1);
            Height = _pattern.GetLength(0);
            IgnoredColor = ignoredColor;

            _grid = new T[Height, Width];
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    _grid[y, x] = IgnoredColor;

            ColHints = new (T, int, bool)[Width][];
            RowHints = new (T, int, bool)[Height][];

            PossibleColors = pattern.Cast<T>()
                .ToHashSet()
                .Where(c => !c.Equals(IgnoredColor))
                .ToArray();

            for (var y = 0; y < RowHints.Length; y++)
                RowHints[y] = CalculateHints(_pattern.GetRow(y))
                    .Where(g => PossibleColors.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();

            for (var x = 0; x < ColHints.Length; x++)
                ColHints[x] = CalculateHints(_pattern.GetCol(x))
                    .Where(g => PossibleColors.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false))
                    .ToArray();
        }

        public Game<TOther> ConvertTo<TOther>(TOther ignoredColor, params TOther[] possibleColors)
        where TOther : notnull
        {
            var pattern = new TOther[Height, Width];
            var grid = new TOther[Height, Width];

            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                {
                    pattern[y, x] = _pattern[y, x].ConvertTo(PossibleColors, possibleColors, ignoredColor);
                    grid[y, x] = _grid[y, x].ConvertTo(PossibleColors, possibleColors, ignoredColor);
                }

            var result = new Game<TOther>(pattern, ignoredColor);

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
            if (IsCorrect && !color.Equals(IgnoredColor))
                return;
            if (!PossibleColors.Contains(color) && !(IgnoredColor.Equals(color)))
                return;

            _grid[y, x] = color;

            ValidateHints(x, y);
        }

        public void ValidateHints(int x, int y)
        {
            Validate(CalculateHints(_grid.GetCol(x).Select(g => g)), ColHints[x], PossibleColors);
            Validate(CalculateHints(_grid.GetRow(y).Select(g => g)), RowHints[y], PossibleColors);

            IsComplete = (Array.TrueForAll(ColHints, ch => Array.TrueForAll(ch, h => h.validated))
                && Array.TrueForAll(RowHints, rh => Array.TrueForAll(rh, h => h.validated)));

            if (IsComplete)
            {
                for (var i = 0; i < Width; i++)
                    for (var j = 0; j < Height; j++)
                        if (!_grid[i, j].Equals(_pattern[i, j]))
                            return;
                IsCorrect = true;
            }

            static void Validate(IEnumerable<(T color, int qty)> line, (T color, int qty, bool validated)[] hints, T[] possibleColors)
            {
                var lineArray = line
                    .Where(g => possibleColors.Contains(g.color))
                    .ToArray();

                for (var i = 0; i < hints.Length; i++)
                    hints[i].validated = false;

                for (var i = 0; i < Math.Min(lineArray.Length, hints.Length); i++)
                    if (!ValidateCell(lineArray, hints, i) && !ValidateCell(lineArray, hints, Index.FromEnd(i + 1)))
                        break;

                static bool ValidateCell((T color, int qty)[] line, (T color, int qty, bool validated)[] hints, Index i)
                {
                    ref var hint = ref hints[i];
                    var c = line[i];
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
    }
}
