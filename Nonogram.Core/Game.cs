using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nonogram
{
    public static class Game
    {
        private struct StreamEnumerator : IEnumerator<byte>
        {
            public StreamEnumerator(Stream stream)
                => (Stream, Current) = (stream, 0);

            public Stream Stream { get; }

            public byte Current { get; set; }

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            { }
            public bool MoveNext()
            {
                var i = Stream.ReadByte();
                Current = (byte)i;
                return i >= 0;
            }
            public void Reset()
            { }
        }

        public static Game<T> Create<T>(T[,] pattern, T ignoredColor = default!)
            where T : notnull
            => new(pattern, ignoredColor);

        public static Game<T> Load<T>(Stream patternSave, Func<IEnumerator<byte>, T> deserializer, bool loadGame)
            where T : notnull
        {
            var version = patternSave.ReadByte();
            if (version is not 1)
                throw new FormatException($"Version: {version} not supported");

            using var enumerator = new StreamEnumerator(patternSave);
            var (width, height) = (patternSave.ReadByte(), patternSave.ReadByte());
            var colors = Enumerable.Range(0, patternSave.ReadByte()).Select(_ => deserializer(enumerator)).ToArray();
            var pattern = new T[height, width];
            foreach (var (x, y) in pattern.GenerateCoord())
                pattern[y, x] = colors[patternSave.ReadByte()];

            var game = Create(pattern, colors[0]);
            if (loadGame && patternSave.Position < patternSave.Length)
            {
                foreach (var (x, y) in pattern.GenerateCoord())
                    (game._grid[y, x], game._coloredCellCount) = (patternSave.ReadByte(), game._coloredCellCount) switch
                    {
                        (0, var count) => (new EmptyCell(), count),
                        (1, var count) => (new AllColoredSealCell(), count),
                        (2, var count) => (new SealedCell<T>(Enumerable.Range(0, patternSave.ReadByte()).Select(_ => colors[patternSave.ReadByte()])), count),
                        (3, var count) => CreateColoredCell(game, colors[patternSave.ReadByte()], count),
                    };

                for (var x = 0; x < width; x++)
                    game.ValidateCol(x);
                for (var y = 0; y < height; y++)
                    game.ValidateRow(y);
            }
            return game;

            static (ICell, int) CreateColoredCell(Game<T> game, T color, int count)
            {
                game.PossibleColors.First(c => Game<T>.ColorEqualizer(color, c.Value)).Current++;
                return (new ColoredCell<T>(color), count + 1);
            }
        }
    }

    public class Game<T> : IEnumerable<ICell>, INotifyPropertyChanged, INotifyCollectionChanged, IUndoRedo
    where T : notnull
    {
        public class Color : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; }

            private int _current;
            public int Current
            {
                get => _current;
                set
                {
                    this.OnPropertyChanged(ref _current, in value, PropertyChanged);
                    Validated = Current == Total;
                }
            }

            public int Total { get; }

            private bool _validated;
            public bool Validated
            {
                get => _validated;
                private set => this.OnPropertyChanged(ref _validated, in value, PropertyChanged);
            }

            public Color(T value, int total)
                => (Value, Total) = (value, total);

            public override int GetHashCode()
                => Value.GetHashCode();

            public override bool Equals(object? obj)
                => obj is Color c && ColorEqualizer(c.Value, Value);
        }

        public class HintGroup : INotifyPropertyChanged, IEnumerable<Hint>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public Hint[] Hints { get; init; }

            public int Length => Hints.Length;

            public Hint this[Index index]
                => Hints[index];

            public HintGroup(Game<T>.Hint[] hints)
                => Hints = hints;

            private bool _isGroupInvalid;
            public bool IsGroupInvalid
            {
                get => _isGroupInvalid;
                set => this.OnPropertyChanged(ref _isGroupInvalid, in value, PropertyChanged);
            }

            public IEnumerator<Game<T>.Hint> GetEnumerator()
                => ((IEnumerable<Game<T>.Hint>)Hints).GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        public class Hint : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; }

            public int Total { get; }

            private bool _validated;
            public bool Validated
            {
                get => _validated;
                set => this.OnPropertyChanged(ref _validated, in value, PropertyChanged);
            }

            public Hint(T value, int total)
                => (Value, Total) = (value, total);

            public override int GetHashCode()
                => (Value, Total).GetHashCode();

            public override bool Equals(object? obj)
                => obj is Hint c && c.Total == Total && ColorEqualizer(c.Value, Value);
        }

        private abstract class HistoryFrame
        {
            public class SingleCell : HistoryFrame
            {
                public int X { get; }
                public int Y { get; }
                public ICell Cell { get; }

                public SingleCell(int x, int y, Game<T> game)
                    => (X, Y, Cell) = (x, y, game[x, y]);

                public override Game<T>.HistoryFrame.SingleCell Rebuild(Game<T> game)
                    => new(X, Y, game);

                public override void Apply(Game<T> game, LinkedList<HistoryFrame> first, LinkedList<HistoryFrame> last)
                {
                    base.Apply(game, first, last);
                    game.CalculateColoredCells(X, Y, Cell);
                    game.OnCollectionChanged(X, Y, Cell);
                    var autoSeal = game.AutoSeal;
                    game._autoSeal = false;
                    game.ValidateHints(X, Y);
                    game._autoSeal = autoSeal;
                }
            }

            public abstract HistoryFrame Rebuild(Game<T> game);
            public virtual void Apply(Game<T> game, LinkedList<HistoryFrame> first, LinkedList<HistoryFrame> last)
            {
                first.RemoveLast();
                last.AddLast(Rebuild(game));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public static Func<T, T, bool> ColorEqualizer { get; set; } = typeof(T) == typeof(IEquatable<T>) ? (a, b) => ((IEquatable<T>)a).Equals(b) : EqualityComparer<T>.Default.Equals;
        public static Func<T, byte[]> ColorSerializer { get; set; }

        private readonly T[,] _pattern;
        internal readonly ICell[,] _grid;

        public int Width { get; }
        public int Height { get; }
        public HintGroup[] ColHints { get; }
        public HintGroup[] RowHints { get; }
        public Color[] PossibleColors { get; }
        public T IgnoredColor { get; }
        private bool _isComplete;
        public bool IsComplete
        {
            get => _isComplete;
            private set => this.OnPropertyChanged(ref _isComplete, in value, PropertyChanged);
        }
        private bool _isCorrect;
        public bool IsCorrect
        {
            get => _isCorrect;
            private set => this.OnPropertyChanged(ref _isCorrect, in value, PropertyChanged);
        }
        internal int _coloredCellCount;

        public int ColoredCellCount
        {
            get => _coloredCellCount;
            set
            {
                this.OnPropertyChanged(ref _coloredCellCount, in value, PropertyChanged);
                PropertyChanged?.Invoke(this, new(nameof(Percent)));
            }
        }

        public double Percent => ColoredCellCount / (double)TotalColoredCell;

        private bool _autoSeal = true;
        public bool AutoSeal
        {
            get => _autoSeal;
            set => this.OnPropertyChanged(ref _autoSeal, value, PropertyChanged);
        }
        public int TotalColoredCell { get; }

        private readonly LinkedList<HistoryFrame> _previous = new(), _nexts = new();
        public ICell this[int x, int y]
        {
            get => _grid[y, x];
            set
            {
                value = value switch
                {
                    ColoredCell<T> { Color: var color } when ColorEqualizer(color, IgnoredColor)
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
                _previous.AddLast(new HistoryFrame.SingleCell(x, y, this));
                _nexts.Clear();
                CalculateColoredCells(x, y, value);

                OnCollectionChanged(x, y, in value);
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
                case ( ColoredCell<T> { Color: var current }, { IsColored: false }):
                {
                    ColoredCellCount--;
                    PossibleColors.First(t => ColorEqualizer(t.Value, current)).Current--;
                    break;
                }
                case ({ IsColored: false }, ColoredCell<T> { Color: var next }):
                {
                    ColoredCellCount++;
                    PossibleColors.First(t => ColorEqualizer(t.Value, next)).Current++;
                    break;
                }
                case (ColoredCell<T> { Color: var current }, ColoredCell<T> { Color: var next }):
                {
                    PossibleColors.First(t => ColorEqualizer(t.Value, current)).Current--;
                    PossibleColors.First(t => ColorEqualizer(t.Value, next)).Current++;
                    break;
                }
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

            ColHints = new HintGroup[Width];
            RowHints = new HintGroup[Height];

            var rng = new Random(0);
            PossibleColors = pattern.Cast<T>()
                .ToHashSet()
                .Where(c => !ColorEqualizer(c, IgnoredColor))
                .OrderBy(_ => rng.Next())
                .Select(c => new Color(c, _pattern.Cast<T>().Count(p => ColorEqualizer(p, c))))
                .ToArray();

            TotalColoredCell = PossibleColors.Sum(c => c.Total);

            for (var y = 0; y < RowHints.Length; y++)
                RowHints[y] = new(CalculateHints(_pattern.GetRow(y))
                    .Where(g => PossibleColors.Any(c => ColorEqualizer(g.color, c.Value)))
                    .Select(g => new Hint(g.color, g.qty))
                    .ToArray());

            for (var x = 0; x < ColHints.Length; x++)
                ColHints[x] = new(CalculateHints(_pattern.GetCol(x))
                    .Where(g => PossibleColors.Any(c => ColorEqualizer(g.color, c.Value)))
                    .Select(g => new Hint(g.color, g.qty))
                    .ToArray());
        }

        public Game<TOther> ConvertTo<TOther>(TOther ignoredColor, params TOther[] possibleColors)
        where TOther : notnull
        {
            var pattern = new TOther[Height, Width];
            var grid = new ICell[Height, Width];

            foreach (var (x, y) in this.GenerateCoord())
            {
                pattern[y, x] = _pattern[y, x].ConvertTo(PossibleColors.Select(c => c.Value).ToArray(), possibleColors, ignoredColor);
                grid[y, x] = _grid[x, y] switch
                {
                    ColoredCell<T> { Color: var color } => new ColoredCell<TOther>(color.ConvertTo(PossibleColors.Select(c => c.Value).ToArray(), possibleColors, ignoredColor)),
                    AllColoredSealCell => new AllColoredSealCell(),
                    SealedCell<T> seals => seals.ConvertTo(PossibleColors.Select(c => c.Value).ToArray(), possibleColors),
                    _ => new EmptyCell()
                };
            }

            var result = new Game<TOther>(pattern, ignoredColor);

            result.GetType()
                .GetField(nameof(_grid), BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(result, grid);

            foreach (var (x, y) in RowHints.GenerateCoord(h => h.Hints))
                result.RowHints[x].Hints[y].Validated = RowHints[x].Hints[y].Validated;

            foreach (var (x, y) in ColHints.GenerateCoord(h => h.Hints))
                result.ColHints[x].Hints[y].Validated = ColHints[x].Hints[y].Validated;

            return result;
        }

        public void BoxSeal()
        {
            if (IsCorrect)
                return;

            for (var x = 0; x < Width; x++)
                if (ColHints[x].Length == 0)
                    for (var y = 0; y < Height; y++)
                        ValidateHints(x, y, IgnoredColor, true);
                else if (ColHints[x][0].Total == Height)
                    for (var y = 0; y < Height; y++)
                        ValidateHints(x, y, ColHints[x][0].Value, false);
            for (var y = 0; y < Height; y++)
                if (RowHints[y].Length == 0)
                    for (var x = 0; x < Width; x++)
                        ValidateHints(x, y, IgnoredColor, true);
                else if (RowHints[y][0].Total == Width)
                    for (var x = 0; x < Width; x++)
                        ValidateHints(x, y, RowHints[y][0].Value, false);

            if (PossibleColors.Length > 1)
                foreach (var color in PossibleColors)
                {
                    for (var x = 0; x < Width; x++)
                        if (!ColHints[x].Any(hint => ColorEqualizer(hint.Value, color.Value)))
                            for (var y = 0; y < Height; y++)
                                Seal(x, y, color.Value);
                    for (var y = 0; y < Height; y++)
                        if (!RowHints[y].Any(hint => ColorEqualizer(hint.Value, color.Value)))
                            for (var x = 0; x < Width; x++)
                                Seal(x, y, color.Value);
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
            if (this[x, y] is { IsEmpty: true } or { IsSealed: true })
                ValidateHints(x, y, color, seal: true);
        }

        public void ValidateHints(int x, int y, T color, bool seal)
        {
            if (IsCorrect && !ColorEqualizer(color, IgnoredColor))
                return;
            if (!PossibleColors.Any(c => ColorEqualizer(color, c.Value)) && !ColorEqualizer(IgnoredColor, color))
                return;

            this[x, y] = (seal, cell: this[x, y], isIgnored: ColorEqualizer(color, IgnoredColor)) switch
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
                    => AllColoredSealCell.Without(color, PossibleColors.Select(c => c.Value)),
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

        public void ValidateCol(int x)
        {
            if (Validate(CalculateHints(_grid.GetCol(x).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)).ToArray(), ColHints[x], PossibleColors) && AutoSeal)
                for (var i = 0; i < Height; i++)
                    if (this[x, i] is { IsColored: false })
                        this[x, i] = new AllColoredSealCell();
        }

        public void ValidateRow(int y)
        {
            if (Validate(CalculateHints(_grid.GetRow(y).Select(g => g is ColoredCell<T> color ? color.Color : IgnoredColor)).ToArray(), RowHints[y], PossibleColors) && AutoSeal)
                for (var i = 0; i < Width; i++)
                    if (this[i, y] is { IsColored: false })
                        this[i, y] = new AllColoredSealCell();
        }

        public void ValidateHints(int x, int y)
        {
            ValidateCol(x);
            ValidateRow(y);

            if (AutoSeal && this[x, y].GetColor() is T color)
            {
                var any = false;
                if (RowHints[y].Where(h => ColorEqualizer(color, h.Value)).All(All) && any)
                    for (var i = 0; i < Width; i++)
                        if (!this[i, y].IsColored)
                            Seal(i, y, color);
                any = false;
                if (ColHints[x].Where(h => ColorEqualizer(color, h.Value)).All(All) && any)
                    for (var i = 0; i < Height; i++)
                        if (!this[x, i].IsColored)
                            Seal(x, i, color);

                bool All(Hint h)
                {
                    any |= h.Validated;
                    return h.Validated;
                }
            }

            IsComplete = ColoredCellCount == TotalColoredCell
                && Array.TrueForAll(ColHints, ch => ch.All(h => h.Validated))
                && Array.TrueForAll(RowHints, rh => rh.All(h => h.Validated));

            if (IsComplete)
            {
                foreach (var (i, j) in this.GenerateCoord())
                    if (IsSameAsPattern(i, j, treatEmptySameAsSeals: true))
                        continue;
                    else
                        return;
                IsCorrect = true;
            }
        }

        private static bool Validate((T color, int qty)[] line, HintGroup hints, IEnumerable<Color> possibleColors)
        {
            line = line
                .Where(g => possibleColors.Any(c => ColorEqualizer(g.color, c.Value)))
                .ToArray();
            hints.IsGroupInvalid = hints.Length < line.Length;

            if (line.Zip(hints).TakeWhile(hs => (hs.First.qty, hs.First.color).Equals((hs.Second.Total, hs.Second.Value))).Count() == hints.Length)
            {
                for (var i = 0; i < hints.Length; i++)
                {
                    var hint = hints[i];
                    hint.Validated = true;
                }
                return true;
            }

            for (var i = 0; i < hints.Length; i++)
            {
                var hint = hints[i];
                hint.Validated = false;
            }

            foreach (var color in possibleColors)
            {
                var lineArray = line
                    .Where(g => ColorEqualizer(g.color, color.Value))
                    .ToArray();
                var hintsArray = hints
                    .Select((g, i) => (g, i))
                    .Where(g => ColorEqualizer(g.g.Value, color.Value))
                    .ToArray();

                for (var i = 0; i < Math.Min(lineArray.Length, hintsArray.Length); i++)
                    if (!ValidateCell(lineArray, hintsArray, hints, i) && !ValidateCell(lineArray, hintsArray, hints, Index.FromEnd(i + 1)))
                        break;
            }

            return false;


            static bool ValidateCell((T color, int qty)[] line, IList<(Hint, int i)> array, HintGroup hints, Index i)
            {
                var (hint, pos) = array[i];
                var (color, qty) = line[i];
                if (hint.Total != qty || !ColorEqualizer(hint.Value, color))
                    return false;
                hint.Validated = true;
                return true;
            }
        }

        private bool IsSameAsPattern(int x, int y, bool treatEmptySameAsSeals)
            => (treatEmptySameAsSeals, _grid[y, x], _pattern[y, x]) switch
            {
                (false, { IsEmpty: true }, _)
                    => false,
                (false, AllColoredSealCell, var pattern) when ColorEqualizer(pattern, IgnoredColor)
                    => true,
                (false, SealedCell<T> { Seals: var seals }, var pattern) when seals.Contains(pattern)
                    => false,
                (true, { IsColored: false }, var pattern) when ColorEqualizer(pattern, IgnoredColor)
                    => true,
                (_, ColoredCell<T> { Color: var c }, var pattern) when ColorEqualizer(c, pattern)
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
            this[x, y] = ColorEqualizer(_pattern[y, x], IgnoredColor)
                ? new AllColoredSealCell()
                : new ColoredCell<T>(_pattern[y, x]);
            ValidateHints(x, y);
            return (x, y);
        }

        public void Undo()
        {
            var last = _previous.Last;
            if (last is { Value: HistoryFrame.SingleCell frame })
                frame.Apply(this, _previous, _nexts);
        }

        public void Redo()
        {
            var last = _nexts.Last;
            if (last is { Value: HistoryFrame.SingleCell frame })
                frame.Apply(this, _nexts, _previous);
        }

        public void Restart()
        {
            foreach (var (x, y) in this.GenerateCoord())
                _grid[y, x] = new EmptyCell();
            OnCollectionReset();

            foreach (var (x, y) in RowHints.GenerateCoord(h => h.Hints))
                RowHints[x][y].Validated = false;

            foreach (var (x, y) in ColHints.GenerateCoord(h => h.Hints))
                ColHints[x][y].Validated = false;

            _previous.Clear();
            _nexts.Clear();
            ColoredCellCount = 0;

            IsComplete = IsCorrect = false;
        }

        public byte[] SavePattern()
        {
            var colors = PossibleColors.Select(c => c.Value).Prepend(IgnoredColor).Select((c, i) => (c, i: (byte)i)).ToArray();
            return new[]{ (byte)1, (byte)Width, (byte)Height, (byte)colors.Length }
                .Concat(ColorSerializer(IgnoredColor))
                .Concat(PossibleColors.Select(c => c.Value).SelectMany(ColorSerializer))
                .Concat(this.GenerateCoord()
                    .Select(pos => _pattern[pos.y, pos.x])
                    .Select(c => colors.First(col => ColorEqualizer(col.c, c)).i))
                .ToArray();
        }

        public byte[] SaveGame()
        {
            var colors = PossibleColors.Select(c => c.Value).Prepend(IgnoredColor).Select((c, i) => (c, i: (byte)i)).ToArray();
            return SavePattern()
            .Concat(this.GenerateCoord()
                .Select(pos => this[pos.x, pos.y])
                .SelectMany(cell => cell switch
                {
                    EmptyCell => new byte[]{ 0 },
                    AllColoredSealCell => new byte[]{ 1 },
                    SealedCell<T> { Seals: var s } => new byte[]{ 2, (byte)s.Count }.Concat(s.Select(c => colors.First(col => ColorEqualizer(col.c, c)).i)),
                    ColoredCell<T> { Color: T c } => new byte[]{ 3, colors.First(col => ColorEqualizer(col.c, c)).i },
                }))
            .ToArray();
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
                if (!ColorEqualizer(enumerator.Current, color))
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
