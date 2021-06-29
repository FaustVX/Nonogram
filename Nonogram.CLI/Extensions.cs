using System;

namespace Nonogram.CLI
{
    public static class Extensions
    {
        public static void WriteAt(char c, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(c);
        }
    }
}
