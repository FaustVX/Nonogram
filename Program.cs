using System;
using System.Linq;
using static Nonogram.Extensions;

namespace Nonogram
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var nonogram = Services.WebPbn.Get<ConsoleColor>(int.Parse(args[0]), (name, _) => Enum.Parse<ConsoleColor>(name, ignoreCase: true));
            Play(nonogram, ConsoleColor.DarkGray);
        }

        private static void Play(Game<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor)
        {
            Console.Clear();
            var (color, x, y) = (new Nullable<int>(), 1, 1);
            do
            {
                var (seal, undo, redo, ok) = (false, false, false, true);
                do
                {
                    Console.BackgroundColor = GetAtOrDefault(color, nonogram.PossibleColors, nonogram.IgnoredColor);
                    if (color is not null)
                        Console.ForegroundColor = nonogram.IgnoredColor;
                    System.Console.WriteLine($"X:{x}, Y:{y}");
                    Print(nonogram, validatedBackgroundColor, GetAtOrDefault(color, nonogram.PossibleColors, nonogram.IgnoredColor), (x, y));
                    Console.ResetColor();
                    (seal, color, x, y, undo, redo, ok) = ReadKey(nonogram, (color, x, y));
                    if (undo)
                        nonogram.Undo();
                    else if (redo)
                        nonogram.Redo();
                    Console.Clear();
                } while (!ok);

                nonogram.ValidateHints(x - 1, y - 1, GetAtOrDefault(color, nonogram.PossibleColors, nonogram.IgnoredColor), seal);
            } while (!nonogram.IsCorrect);
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);
            Print(nonogram, nonogram.IgnoredColor, nonogram.IgnoredColor, (-1, -1));

            static T GetAtOrDefault<T>(int? index, T[] values, T @default)
                => index is int i ? values[i] : @default;

            static (bool, int?, int, int, bool undo, bool redo, bool ok) ReadKey(Game<ConsoleColor> nonogram, (int? color, int x, int y) state)
                => ((Console.ReadKey(intercept: true), state)) switch
                {
                    ({ Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift },    (color: > 0, _, _))
                        => (false, state.color - 1, state.x, state.y, false, false, false),
                    ({ Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift },    (color: 0, _, _))
                        => (false, null, state.x, state.y, false, false, false),
                    ({ Key: ConsoleKey.Tab, Modifiers: not ConsoleModifiers.Shift },(color: null, _, _))
                        => (false, 0, state.x, state.y, false, false, false),
                    ({ Key: ConsoleKey.Tab, Modifiers: not ConsoleModifiers.Shift },(color: var c, _, _))    when c < nonogram.PossibleColors.Length - 1
                        => (false, state.color + 1, state.x, state.y, false, false, false),
                    ({ Key: ConsoleKey.LeftArrow },                                 (_, x: > 1, _))
                        => (false, state.color, state.x - 1, state.y, false, false, false),
                    ({ Key: ConsoleKey.RightArrow },                                (_, x: var x, _))        when x < nonogram.Width
                        => (false, state.color, state.x + 1, state.y, false, false, false),
                    ({ Key: ConsoleKey.UpArrow },                                   (_, _, y: > 1))
                        => (false, state.color, state.x, state.y - 1, false, false, false),
                    ({ Key: ConsoleKey.DownArrow },                                 (_, _, y: var y))        when y < nonogram.Height
                        => (false, state.color, state.x, state.y + 1, false, false, false),
                    ({ Key: ConsoleKey.Z, Modifiers: ConsoleModifiers.Control }, _)
                        => (false, state.color, state.x, state.y, true, false, false),
                    ({ Key: ConsoleKey.Y, Modifiers: ConsoleModifiers.Control }, _)
                        => (false, state.color, state.x, state.y, false, true, false),
                    ({ Key: ConsoleKey.X },                                         _)
                        => (true, state.color, state.x, state.y, false, false, true),
                    ({ Key: ConsoleKey.Spacebar or ConsoleKey.Enter },              _)
                        => (false, state.color, state.x, state.y, false, false, true),
                    _   => (false, state.color, state.x, state.y, false, false, false),
                };
        }

        private static void Print(Game<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor, ConsoleColor selectedColor, (int x, int y) pos)
        {
            var yOffset = Console.CursorTop;
            Console.ResetColor();
            var maxRow = nonogram.ColHints.Max(h => h.Length) + 1 + yOffset;
            var maxCol = nonogram.RowHints.Max(h => h.Length) + 1;

            for (var x = 0; x < nonogram.ColHints.Length; x++)
            {
                for (var i = nonogram.ColHints[x].Length - 1; i >= 0; i--)
                {
                    var hint = nonogram.ColHints[x][i];
                    Console.SetCursorPosition(maxCol + x + 1, i + yOffset);
                    Console.ForegroundColor = hint.color;
                    if (hint.validated)
                        Console.BackgroundColor = validatedBackgroundColor;
                    Console.Write(hint.qty);
                    Console.ResetColor();
                }
                WriteAt('─', maxCol + x + 1, maxRow);
                WriteAt('─', maxCol + x + 1, maxRow + 1 + nonogram.Height);
            }

            for (var y = 0; y < nonogram.RowHints.Length; y++)
            {
                for (var i = nonogram.RowHints[y].Length - 1; i >= 0; i--)
                {
                    var hint = nonogram.RowHints[y][i];
                    Console.SetCursorPosition(i, maxRow + y + 1);
                    Console.ForegroundColor = hint.color;
                    if (hint.validated)
                        Console.BackgroundColor = validatedBackgroundColor;
                    Console.Write(hint.qty);
                    Console.ResetColor();
                }
                WriteAt('│', maxCol, maxRow + y + 1);
                WriteAt('│', maxCol + 1 + nonogram.Width, maxRow + y + 1);
            }

            WriteAt('┌', maxCol, maxRow);

            WriteAt('┐', maxCol + nonogram.Width + 1, maxRow);

            WriteAt('└', maxCol, maxRow + nonogram.Height + 1);

            WriteAt('┘', maxCol + nonogram.Width + 1, maxRow + nonogram.Height + 1);

            for (var x = 0; x < nonogram.Width; x++)
                for (var y = 0; y < nonogram.Height; y++)
                {
                    Console.SetCursorPosition(x + maxCol + 1, y + maxRow + 1);
                    var (fore, @char) = nonogram[x, y] switch
                    {
                        ColoredCell<ConsoleColor> { Color: var color } when color == selectedColor => (color, '█'),
                        ColoredCell<ConsoleColor> { Color: var color } => (color, '▒'),
                        EmptyCell => (nonogram.IgnoredColor, '█'),
                        AllColoredSealCell => (selectedColor, 'X'),
                        SealedCell<ConsoleColor> { Seals: var seals } when seals.Contains(selectedColor) => (selectedColor, 'X'),
                        SealedCell<ConsoleColor> => (nonogram.IgnoredColor, 'x'),
                    };
                    Console.ForegroundColor = fore;
                    Console.Write(@char);
                }
            //█▓▒░┌┐└┘─│
            Console.ResetColor();
            Console.SetCursorPosition(pos.x + maxCol, pos.y + maxRow);
        }
    }
}
