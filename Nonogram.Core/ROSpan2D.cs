using System;
using System.Collections;
using System.Collections.Generic;

namespace Nonogram
{
    public readonly struct ROSpan2D<T> : IEnumerable<T>
    {
        private readonly T[,] _array;
        public readonly int Top, Left;
        public readonly int Width, Height;

        public readonly ref T this[int x, int y]
        {
            get
            {
                if (x < Width && y < Height && x >= 0 && y >= 0)
                    return ref _array[y + Top, x + Left];
                throw new Exception();
            }
        }

        public ROSpan2D<T> Offset(int x, int y)
            => new (_array, x + Left, y + Top, Width, Height);

        public ROSpan2D(T[,] array, int left, int top, int width, int height)
            => (_array, Left, Top, Width, Height) = (array, Clamp(left, array, 1), Clamp(top, array, 0), Clamp(width, array, 1), Clamp(height, array, 0));

        private static int Clamp(int value, Array array, int dimension)
            => Math.Clamp(value, 0, array.GetLength(dimension));

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static implicit operator ROSpan2D<T>(T[,] array)
            => new (array, 0, 0, array.GetLength(1), array.GetLength(0));

        private struct Enumerator : IEnumerator<T>
        {
            private readonly ROSpan2D<T> _this;
            private int x, y;

            public Enumerator(ROSpan2D<T> span)
                => (_this, x, y) = (span, -1, 0);

            public T Current
                => _this[x, y];

            object IEnumerator.Current
                => Current!;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (++x >= _this.Width)
                    if (++y >= _this.Height)
                        return false;
                    else
                        x = 0;
                return true;
            }

            public void Reset()
            { }
        }
    }
}
