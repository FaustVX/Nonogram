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

    public class Game<T> : IEnumerable<ICell>
    where T : notnull
    {
        private readonly T[,] _pattern;
        private readonly ICell[,] _grid;
        public int Width { get; }
        public int Height { get; }
        public (T color, int qty, bool validated)[][] ColHints { get; }
        public (T color, int qty, bool validated)[][] RowHints { get; }
        public T[] PossibleColors { get; }
        public T IgnoredColor { get; }
        public bool IsComplete { get; private set; }
        public bool IsCorrect { get; private set; }
        private readonly LinkedList<(int x, int y, ICell cell)> _previous = new (), _nexts = new ();
        public ICell this[int x, int y]
        {
            get => _grid[y, x];
            set
            {
                if (value.Equals(_grid[y, x]))
                    return;
                _previous.AddLast((x, y, _grid[y, x]));
                _nexts.Clear();
                _grid[y, x] = value switch
                {
                    ColoredCell<T> { Color: var color } when color.Equals(IgnoredColor)
                        => new EmptyCell(),
                    SealedCell<T> { Seals: { Count: 0 } }
                        => new EmptyCell(),
                    SealedCell<T> { Seals: { Count: var count } seals } when count >= PossibleColors.Length
                        => new AllColoredSealCell(),
                    null=> new EmptyCell(),
                    _   => value,
                };
            }
        }

        public Game(T[,] pattern, T ignoredColor)
        {
            _pattern = pattern;
            Width = _pattern.GetLength(1);
            Height = _pattern.GetLength(0);
            IgnoredColor = ignoredColor;

            _grid = new ICell[Height, Width];
            foreach (var (x, y) in this.GenerateCoord())
                _grid[y, x] = new EmptyCell();

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
            var grid = new ICell[Height, Width];

            foreach (var (x, y) in this.GenerateCoord())
            {
                pattern[y, x] = _pattern[y, x].ConvertTo(PossibleColors, possibleColors, ignoredColor);
                grid[y, x] = _grid[x, y] switch
                {
                    ColoredCell<T> { Color: var color } => new ColoredCell<TOther>(color.ConvertTo(PossibleColors, possibleColors, ignoredColor)),
                    AllColoredSealCell => new AllColoredSealCell(),
                    SealedCell<T> seals => seals.ConvertTo(PossibleColors, possibleColors),
                    _ => new EmptyCell()
                };
            }

            var result = new Game<TOther>(pattern, ignoredColor);

            result.GetType()
                .GetField(nameof(_grid), BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(result, grid);

            foreach (var (x, y) in RowHints.GenerateCoord())
                result.RowHints[x][y].validated = RowHints[x][y].validated;

            foreach (var (x, y) in ColHints.GenerateCoord())
                result.ColHints[x][y].validated = ColHints[x][y].validated;

            return result;
        }

        public void SetColor(int x, int y, T color)
        {
            if (this[x, y].IsEmpty)
                ValidateHints(x, y, color, seal: false);
        }

        public void Clear(int x, int y)
        {
            var cell = this[x, y];
            if (!cell.IsEmpty)
                ValidateHints(x, y, IgnoredColor, cell.IsColored);
        }

        public void Seal(int x, int y, T color)
        {
            if (this[x, y].IsEmpty)
                ValidateHints(x, y, color, seal: true);
        }

        public void ValidateHints(int x, int y, T color, bool seal)
        {
            if (IsCorrect && !color.Equals(IgnoredColor))
                return;
            if (!PossibleColors.Contains(color) && !(IgnoredColor.Equals(color)))
                return;

            this[x, y] = (seal, cell: this[x, y], isIgnored: color.Equals(IgnoredColor)) switch
            {
                (true, { IsEmpty: true }, false) when PossibleColors.Length is 1
                    => new AllColoredSealCell(),
                (true, { IsEmpty: true }, false)
                    => new SealedCell<T>(color),
                (true, { IsEmpty: true }, true)
                    => new AllColoredSealCell(),
                (true, { IsSealed: true }, true)
                    => new AllColoredSealCell(),
                (true, SealedCell<T> seals, _) when !seals.Seals.Contains(color)
                    => seals.Add(color),
                (false, SealedCell<T> seals, _) when seals.Seals.Contains(color)
                    => seals.Remove(color),
                (false, AllColoredSealCell, false)
                    => AllColoredSealCell.Without(color, PossibleColors),
                (false, { IsEmpty: true }, false)
                    => new ColoredCell<T>(color),
                (false, { IsSealed: true }, true)
                    => new EmptyCell(),
                (true, { IsColored: true }, _)
                    => new EmptyCell(),
                _   => this[x, y],
            };

            ValidateHints(x, y);
        }

        public void ValidateHints(int x, int y)
        {
            Validate(CalculateHints(_grid.GetCol(x).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)), ColHints[x], PossibleColors);
            Validate(CalculateHints(_grid.GetRow(y).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)), RowHints[y], PossibleColors);

            IsComplete = (Array.TrueForAll(ColHints, ch => Array.TrueForAll(ch, h => h.validated))
                && Array.TrueForAll(RowHints, rh => Array.TrueForAll(rh, h => h.validated)));

            if (IsComplete)
            {
                foreach (var (i, j) in this.GenerateCoord())
                    switch ((_grid[j, i], _pattern[j, i]))
                    {
                        case (not ColoredCell<T>, var pattern) when pattern.Equals(IgnoredColor):
                            continue;
                        case (ColoredCell<T> { Color: var c }, var pattern) when c.Equals(pattern):
                            continue;
                        default:
                            return;
                    }
                IsCorrect = true;
            }

            static void Validate(IEnumerable<(T color, int qty)> line, (T color, int qty, bool validated)[] hints, T[] possibleColors)
            {
                var lineArray = line
                    .Where(g => possibleColors.Contains(g.color))
                    .ToArray();

                if (lineArray.Intersect(hints.Select(g => (g.color, g.qty))).Count() == hints.Length)
                {
                    for (var i = 0; i < hints.Length; i++)
                        hints[i].validated = true;
                    return;
                }

                for (var i = 0; i < hints.Length; i++)
                    hints[i].validated = false;

                for (var i = 0; i < Math.Min(lineArray.Length, hints.Length); i++)
                    if (!ValidateCell(lineArray, hints, i) && !ValidateCell(lineArray, hints, Index.FromEnd(i + 1)))
                        break;

                static bool ValidateCell((T color, int qty)[] line, (T color, int qty, bool validated)[] hints, Index i)
                {
                    ref var hint = ref hints[i];
                    var (color, qty) = line[i];
                    if (hint.qty != qty || !hint.color.Equals(color))
                        return false;
                    hint.validated = true;
                    return true;
                }
            }
        }

        public (int x, int y)? Undo()
        {
            var last = _previous.Last;
            if (last is { Value: var (x, y, cell) })
            {
                _previous.RemoveLast();
                _nexts.AddLast((x, y, _grid[y, x]));
                _grid[y, x] = cell;
                ValidateHints(x, y);
                return (x, y);
            }
            return null;
        }

        public (int x, int y)? Redo()
        {
            var last = _nexts.Last;
            if (last is { Value: var (x, y, cell) })
            {
                _nexts.RemoveLast();
                _previous.AddLast((x, y, _grid[y, x]));
                _grid[y, x] = cell;
                ValidateHints(x, y);
                return (x, y);
            }
            return null;
        }

        public void Restart()
        {
            foreach (var (x, y) in this.GenerateCoord())
                _grid[y, x] = new EmptyCell();

            foreach (var (x, y) in RowHints.GenerateCoord())
                RowHints[x][y].validated = false;

            foreach (var (x, y) in ColHints.GenerateCoord())
                ColHints[x][y].validated = false;

            _previous.Clear();
            _nexts.Clear();

            IsComplete = IsCorrect = false;
        }

        public IEnumerator<ICell> GetEnumerator()
            => this.GenerateCoord().Select(pos => this[pos.x, pos.y]).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
           => GetEnumerator();

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
