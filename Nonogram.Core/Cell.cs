using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public interface ICell : IEquatable<ICell>
    {
        public T? GetColor<T>()
            where T : notnull
            => default;

        public T[]? GetSeals<T>()
            where T : notnull
            => null;

        public bool IsColored
            => false;

        public bool IsEmpty
            => this is EmptyCell;

        public bool IsSealed
            => this is AllColoredSealCell;
    }

    public sealed class EmptyCell : ICell
    {
        public bool Equals(ICell? other)
            => other is EmptyCell;
    }

    public sealed class AllColoredSealCell : ICell
    {
        public static SealedCell<T> Without<T>(T seal, T[] possibleColors)
            where T : notnull
            => new(possibleColors.Where(s => !s.Equals(seal)));

        public bool Equals(ICell? other)
            => other is AllColoredSealCell;

        public T[]? GetSeals<T>()
            where T : notnull
            => Array.Empty<T>();
    }

    public sealed class ColoredCell<T> : ICell
    where T : notnull
    {
        public T Color { get; }
        public bool IsColored => true;

        public T? GetColor()
            => Color;

        public ColoredCell(T color)
            => Color = color;

        public bool Equals(ICell? other)
            => other is ColoredCell<T> { Color: var c } && Game<T>.ColorEqualizer(c, Color);
    }

    public sealed class SealedCell<T> : ICell
    where T : notnull
    {
        public List<T> Seals { get; }
        public bool IsSealed => true;

        public T[]? GetSeals()
            => Seals.ToArray();

        public SealedCell<T> Remove(T seal)
            => new(Seals.Where(s => !s.Equals(seal)));

        public SealedCell<T> Add(T color)
            => new(this, color);

        public SealedCell(T seal)
            => Seals = new()
            {
                seal
            };

        private SealedCell(SealedCell<T> old, T seal)
            => Seals = new(old.Seals)
            {
                seal
            };

        public SealedCell(IEnumerable<T> seals)
            => Seals = seals.ToList();

        public SealedCell<TOther> ConvertTo<TOther>(T[] oldPossibleColors, TOther[] possibleColors)
            where TOther : notnull
            => new(Seals.Select(s => s.ConvertTo(oldPossibleColors, possibleColors, default!)));

        public bool Equals(ICell? other)
            => other is SealedCell<T> { Seals: var seals } && Seals.Count == seals.Count && Seals.Intersect(seals).Count() == Seals.Count;
    }
}
