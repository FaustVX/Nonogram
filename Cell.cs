using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public interface ICell
    {
        public sealed T? GetColor<T>()
            where T : notnull
            => this is ColoredCell<T> color ? color.Color : default;
        public sealed T[]? GetSeals<T>()
            where T : notnull
            => this is SealedCell<T> seal
                ? seal.Seals.ToArray()
                : this is AllColoredSealCell
                    ? System.Array.Empty<T>()
                    : null;

        public sealed bool IsColored
            => this.GetType().GetGenericTypeDefinition() == typeof(ColoredCell<>);
        public sealed bool IsEmpty
            => this is EmptyCell;
        public sealed bool IsSealed
            => this is AllColoredSealCell || this.GetType().GetGenericTypeDefinition() == typeof(SealedCell<>);
    }

    public sealed class EmptyCell : ICell
    { }

    public sealed class AllColoredSealCell : ICell
    {
        public static SealedCell<T> Without<T>(T seal, T[] possibleColors)
            where T : notnull
            => new SealedCell<T>(possibleColors.Where(s => !s.Equals(seal)));
    }

    public sealed class ColoredCell<T> : ICell
    where T : notnull
    {
        public T Color { get; }

        public ColoredCell(T color)
            => Color = color;
    }

    public sealed class SealedCell<T> : ICell
    where T : notnull
    {
        public List<T> Seals { get; }

        public ICell Remove(T seal)
        {
            if (Seals.Contains(seal))
                if (Seals.Count <= 1)
                    return new EmptyCell();
                else
                    return new SealedCell<T>(Seals.Where(s => !s.Equals(seal)));
            else
                return this;
        }

        public SealedCell(T seal)
        {
            Seals = new()
            {
                seal
            };
        }

        public SealedCell(SealedCell<T> old, T seal)
        {
            Seals = new(old.Seals)
            {
                seal
            };
        }

        public SealedCell(IEnumerable<T> seals)
            => Seals = seals.ToList();

        public SealedCell<TOther> ConvertTo<TOther>(T[] oldPossibleColors, TOther[] possibleColors)
            where TOther : notnull
            => new (Seals.Select(s => s.ConvertTo(oldPossibleColors, possibleColors, default!)));
    }
}
