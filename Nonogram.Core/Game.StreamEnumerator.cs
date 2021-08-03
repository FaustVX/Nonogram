using System.Collections.Generic;
using System.IO;

namespace Nonogram
{
    public static partial class Game
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
    }
}
