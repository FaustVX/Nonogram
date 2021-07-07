using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nonogram
{
    public static class Game
    {
        public static Game<T> Create<T>(T[,] pattern, T ignoredColor = default!)
            where T : notnull
            => new(pattern, ignoredColor);
    }

    public class Game<T> : IEnumerable<ICell>, INotifyPropertyChanged, INotifyCollectionChanged
    where T : notnull
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly T[,] _pattern;
        private readonly ICell[,] _grid;

        public int Width { get; }
        public int Height { get; }
        public ObservableCollection<(T color, int qty, bool validated)>[] ColHints { get; }
        public ObservableCollection<(T color, int qty, bool validated)>[] RowHints { get; }
        public T[] PossibleColors { get; }
        public T IgnoredColor { get; }
        private bool _isComplete;
        public bool IsComplete
        {
            get => _isComplete;
            private set => OnPropertyChanged(ref _isComplete, in value);
        }
        private bool _isCorrect;
        public bool IsCorrect
        {
            get => _isCorrect;
            private set => OnPropertyChanged(ref _isCorrect, in value);
        }
        private int _coloredCellCount;

        public int ColoredCellCount
        {
            get => _coloredCellCount;
            set => OnPropertyChanged(ref _coloredCellCount, in value);
        }

        private bool _autoSeal;
        public bool AutoSeal
        {
            get => _autoSeal;
            set => OnPropertyChanged(ref _autoSeal, value);
        }
        public int TotalColoredCell { get; }

        private readonly LinkedList<(int x, int y, ICell cell)> _previous = new(), _nexts = new();
        public ICell this[int x, int y]
        {
            get => _grid[y, x];
            set
            {
                value = value switch
                {
                    ColoredCell<T> { Color: var color } when color.Equals(IgnoredColor)
                        => new EmptyCell(),
                    SealedCell<T> { Seals: { Count: 0 } }
                        => new EmptyCell(),
                    SealedCell<T> { Seals: { Count: var count } seals } when count >= PossibleColors.Length
                        => new AllColoredSealCell(),
                    null => new EmptyCell(),
                    _ => value,
                };

                if (value.Equals(_grid[y, x]))
                    return;
                _previous.AddLast((x, y, _grid[y, x]));
                _nexts.Clear();
                CalculateColoredCells(x, y, value);

                OnCollectionChanged(x, y, in value);
            }
        }

        protected void OnPropertyChanged<U>(ref U storage, in U value, [CallerMemberName] string propertyName = default!)
        {
            if ((storage is IEquatable<U> comp && !comp.Equals(value)) || (!storage?.Equals(value) ?? false))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new(propertyName));
            }
        }

        protected void OnCollectionChanged(int x, int y, in ICell newItem)
        {
            var oldItem = this[x, y];
            if (!oldItem.Equals(newItem))
            {
                _grid[y, x] = newItem;
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, newItem, oldItem, x * Height + y));
            }
        }

        protected void OnCollectionReset()
            => CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));

        private void CalculateColoredCells(int x, int y, ICell value)
        {
            switch (this[x, y], value)
            {
                case ({ IsColored: true }, { IsColored: false }):
                    ColoredCellCount--;
                    break;
                case ({ IsColored: false }, { IsColored: true }):
                    ColoredCellCount++;
                    break;
            };
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

            ColHints = new ObservableCollection<(T, int, bool)>[Width];
            RowHints = new ObservableCollection<(T, int, bool)>[Height];

            PossibleColors = pattern.Cast<T>()
                .ToHashSet()
                .Where(c => !c.Equals(IgnoredColor))
                .ToArray();

            TotalColoredCell = this.GenerateCoord().Count(pos => !_pattern[pos.y, pos.x].Equals(ignoredColor));

            for (var y = 0; y < RowHints.Length; y++)
                RowHints[y] = new(CalculateHints(_pattern.GetRow(y))
                    .Where(g => PossibleColors.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false)));

            for (var x = 0; x < ColHints.Length; x++)
                ColHints[x] = new(CalculateHints(_pattern.GetCol(x))
                    .Where(g => PossibleColors.Contains(g.color))
                    .Select(g => (g.color, g.qty, validated: false)));
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
            {
                var hint = result.RowHints[x][y];
                hint.validated = RowHints[x][y].validated;
                result.RowHints[x][y] = hint;
            }

            foreach (var (x, y) in ColHints.GenerateCoord())
            {
                var hint = result.ColHints[x][y];
                hint.validated = ColHints[x][y].validated;
                result.ColHints[x][y] = hint;
            }

            return result;
        }

        public void BoxSeal()
        {
            if (IsCorrect)
                return;

            for (var x = 0; x < Width; x++)
                if (ColHints[x].Count == 0)
                    for (var y = 0; y < Height; y++)
                        ValidateHints(x, y, IgnoredColor, true);
            for (var y = 0; y < Height; y++)
                if (RowHints[y].Count == 0)
                    for (var x = 0; x < Width; x++)
                        ValidateHints(x, y, IgnoredColor, true);

            if (PossibleColors.Length > 1)
                foreach (var color in PossibleColors)
                {
                    for (var x = 0; x < Width; x++)
                        if (!ColHints[x].Any(hint => hint.color.Equals(color)))
                            for (var y = 0; y < Height; y++)
                                ValidateHints(x, y, color, true);
                    for (var y = 0; y < Height; y++)
                        if (!RowHints[y].Any(hint => hint.color.Equals(color)))
                            for (var x = 0; x < Width; x++)
                                ValidateHints(x, y, color, true);
                }
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
                (false, SealedCell<T>, false)
                    => new ColoredCell<T>(color),
                (false, AllColoredSealCell, false)
                    => AllColoredSealCell.Without(color, PossibleColors),
                (false, { IsEmpty: true }, false)
                    => new ColoredCell<T>(color),
                (false, { IsSealed: true }, true)
                    => new EmptyCell(),
                (true, { IsColored: true }, _)
                    => new EmptyCell(),
                _ => this[x, y],
            };

            ValidateHints(x, y);
        }

        public void ValidateHints(int x, int y)
        {
            if (Validate(CalculateHints(_grid.GetCol(x).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)).ToArray(), ColHints[x], PossibleColors) && AutoSeal)
                for (var i = 0; i < Height; i++)
                    if (this[x, i] is { IsColored: false })
                        this[x, i] = new AllColoredSealCell();
            if (Validate(CalculateHints(_grid.GetRow(y).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)).ToArray(), RowHints[y], PossibleColors) && AutoSeal)
                for (var i = 0; i < Width; i++)
                    if (this[i, y] is { IsColored: false })
                        this[i, y] = new AllColoredSealCell();

            IsComplete = Array.TrueForAll(ColHints, ch => ch.All(h => h.validated))
                && Array.TrueForAll(RowHints, rh => rh.All(h => h.validated));

            if (IsComplete)
            {
                foreach (var (i, j) in this.GenerateCoord())
                    if (IsSameAsPattern(i, j, treatEmptySameAsSeals: true))
                        continue;
                    else
                        return;
                IsCorrect = true;
            }

            static bool Validate((T color, int qty)[] line, IList<(T color, int qty, bool validated)> hints, T[] possibleColors)
            {
                line = line
                    .Where(g => possibleColors.Contains(g.color))
                    .ToArray();

                if (line.Zip(hints).TakeWhile(hs => (hs.First.qty, hs.First.color).Equals((hs.Second.qty, hs.Second.color))).Count() == hints.Count)
                {
                    for (var i = 0; i < hints.Count; i++)
                    {
                        var hint = hints[i];
                        hint.validated = true;
                        hints[i] = hint;
                    }
                    return true;
                }

                for (var i = 0; i < hints.Count; i++)
                {
                    var hint = hints[i];
                    hint.validated = false;
                    hints[i] = hint;
                }

                foreach (var color in possibleColors)
                {
                    var lineArray = line
                        .Where(g => g.color.Equals(color))
                        .ToArray();
                    var hintsArray = hints
                        .Select((g, i) => (g, i))
                        .Where(g => g.g.color.Equals(color))
                        .ToArray();

                    for (var i = 0; i < Math.Min(lineArray.Length, hintsArray.Length); i++)
                        if (!ValidateCell(lineArray, hintsArray, hints, i) && !ValidateCell(lineArray, hintsArray, hints, Index.FromEnd(i + 1)))
                            break;
                }

                return false;


                static bool ValidateCell((T color, int qty)[] line, IList<((T color, int qty, bool validated), int i)> array, IList<(T color, int qty, bool validated)> hints, Index i)
                {
                    var (hint, pos) = array[i];
                    var (color, qty) = line[i];
                    if (hint.qty != qty || !hint.color.Equals(color))
                        return false;
                    hint.validated = true;
                    hints[pos] = hint;
                    return true;
                }
            }
        }

        private bool IsSameAsPattern(int x, int y, bool treatEmptySameAsSeals)
            => (treatEmptySameAsSeals, _grid[y, x], _pattern[y, x]) switch
            {
                (false, { IsEmpty: true }, _)
                    => false,
                (false, AllColoredSealCell, var pattern) when pattern.Equals(IgnoredColor)
                    => true,
                (false, SealedCell<T> { Seals: var seals }, var pattern) when seals.Contains(pattern)
                    => false,
                (true, { IsColored: false }, var pattern) when pattern.Equals(IgnoredColor)
                    => true,
                (_, ColoredCell<T> { Color: var c }, var pattern) when c.Equals(pattern)
                    => true,
                _ => false,
            };

        public (int x, int y)? Tips()
        {
            if (IsCorrect)
                return null;

            var rng = new Random();
            int x, y;
            do
            {
                x = rng.Next(Width);
                y = rng.Next(Height);
            } while (IsSameAsPattern(x, y, treatEmptySameAsSeals: false));
            this[x, y] = _pattern[y, x].Equals(IgnoredColor)
                ? new AllColoredSealCell()
                : new ColoredCell<T>(_pattern[y, x]);
            ValidateHints(x, y);
            return (x, y);
        }

        public (int x, int y)? Undo()
        {
            var last = _previous.Last;
            if (last is { Value: var (x, y, cell) })
            {
                _previous.RemoveLast();
                _nexts.AddLast((x, y, _grid[y, x]));
                CalculateColoredCells(x, y, cell);
                OnCollectionChanged(x, y, in cell);
                var autoSeal = AutoSeal;
                _autoSeal = false;
                ValidateHints(x, y);
                _autoSeal = autoSeal;
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
                CalculateColoredCells(x, y, cell);
                OnCollectionChanged(x, y, in cell);
                var autoSeal = AutoSeal;
                _autoSeal = false;
                ValidateHints(x, y);
                _autoSeal = autoSeal;
                return (x, y);
            }
            return null;
        }

        public void Restart()
        {
            foreach (var (x, y) in this.GenerateCoord())
                _grid[y, x] = new EmptyCell();
            OnCollectionReset();

            foreach (var (x, y) in RowHints.GenerateCoord())
            {
                var hint = RowHints[x][y];
                hint.validated = false;
                RowHints[x][y] = hint;
            }

            foreach (var (x, y) in ColHints.GenerateCoord())
            {
                var hint = ColHints[x][y];
                hint.validated = false;
                ColHints[x][y] = hint;
            }

            _previous.Clear();
            _nexts.Clear();
            ColoredCellCount = 0;

            IsComplete = IsCorrect = false;
        }

        public IEnumerator<ICell> GetEnumerator()
            => this.GenerateCoord().Select(pos => this[pos.x, pos.y]).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
           => GetEnumerator();

        public (int x, int y) GetCoord(ICell cell)
            => this.GenerateCoord().FirstOrDefault(pos => ReferenceEquals(this[pos.x, pos.y], cell));

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
