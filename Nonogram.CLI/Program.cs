using System;
using System.Linq;
using static Nonogram.CLI.Extensions;

namespace Nonogram.CLI
{
    public static class Program
    {
        private static ConsoleColor FromColor(System.Drawing.Color c)
        {
            var index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit
            return (ConsoleColor)index;
        }
        private static void Main(string[] args)
        {
            Options.ParseArgs(args);
            var nonogram = Options.Generate((name, _) => Enum.Parse<ConsoleColor>(name, ignoreCase: true),
            span =>
            {
                var ratio = ((Options.Resize)Options.Option).FactorReduction;
                var count = (ulong)span.Width * (ulong)span.Height;
                var (r, g, b) = span.Aggregate((r: 0UL, g: 0UL, b: 0UL),
                    (acc, col) => (acc.r + col.R, acc.g + col.G, acc.b + col.B),
                    acc => (r: (byte)(acc.r / count), g: (byte)(acc.g / count), b: (byte)(acc.b / count)));
                return FromColor(System.Drawing.Color.FromArgb(255, r, g, b));
            }, e => (ConsoleColor)e.GetNext());
            Play(nonogram, ConsoleColor.DarkGray);
        }

        private static void Play(Game<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor)
        {
            Console.Clear();
            var (color, x, y) = (new int?(0), 1, 1);
            do
            {
                bool seal, ok;
                do
                {
                    Console.BackgroundColor = GetAtOrDefault(color, nonogram.PossibleColors.Select(c => c.Value).ToArray(), nonogram.IgnoredColor);
                    if (color is not null)
                        Console.ForegroundColor = nonogram.IgnoredColor;
                    Console.WriteLine($"X:{x}, Y:{y} ({nonogram.ColoredCellCount} / {nonogram.TotalColoredCell} cells)");
                    Print(nonogram, validatedBackgroundColor, GetAtOrDefault(color, nonogram.PossibleColors.Select(c => c.Value).ToArray(), nonogram.IgnoredColor), (x, y));
                    Console.ResetColor();
                    ok = seal = false;
                    switch (ReadKey(nonogram, (color, x, y)))
                    {
                        case Color ctrl:
                            color = ctrl.Index;
                            break;
                        case X ctrl:
                            x = ctrl.Pos;
                            break;
                        case Y ctrl:
                            y = ctrl.Pos;
                            break;
                        case Undo:
                            nonogram.Undo();
                            break;
                        case Redo:
                            nonogram.Redo();
                            break;
                        case Restart:
                            nonogram.Restart();
                            break;
                        case Ok ctrl:
                            ok = true;
                            seal = ctrl.IsSealed;
                            break;
                    }
                    Console.Clear();
                } while (!ok);

                nonogram.ValidateHints(x - 1, y - 1, GetAtOrDefault(color, nonogram.PossibleColors.Select(c => c.Value).ToArray(), nonogram.IgnoredColor), seal);
            } while (!nonogram.IsCorrect);
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);
            Print(nonogram, nonogram.IgnoredColor, nonogram.IgnoredColor, (-1, -1));

            static T GetAtOrDefault<T>(int? index, T[] values, T @default)
                => index is int i ? values[i] : @default;

            static IControl? ReadKey(Game<ConsoleColor> nonogram, (int? color, int x, int y) state)
                => ((Console.ReadKey(intercept: true), state)) switch
                {
                    ({ Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift }, (color: > 0, _, _))
                        => new Color(state.color - 1),
                    ({ Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift }, (color: 0, _, _))
                        => new Color(null),
                    ({ Key: ConsoleKey.Tab, Modifiers: not ConsoleModifiers.Shift }, (color: null, _, _))
                        => new Color(0),
                    ({ Key: ConsoleKey.Tab, Modifiers: not ConsoleModifiers.Shift }, (color: var c, _, _)) when c < nonogram.PossibleColors.Length - 1
                        => new Color(state.color + 1),
                    ({ Key: ConsoleKey.LeftArrow }, (_, x: > 1, _))
                        => new X(state.x - 1),
                    ({ Key: ConsoleKey.RightArrow }, (_, x: var x, _)) when x < nonogram.Width
                        => new X(state.x + 1),
                    ({ Key: ConsoleKey.UpArrow }, (_, _, y: > 1))
                        => new Y(state.y - 1),
                    ({ Key: ConsoleKey.DownArrow }, (_, _, y: var y)) when y < nonogram.Height
                        => new Y(state.y + 1),
                    ({ Key: ConsoleKey.Z, Modifiers: ConsoleModifiers.Control }, _)
                        => new Undo(),
                    ({ Key: ConsoleKey.Y, Modifiers: ConsoleModifiers.Control }, _)
                        => new Redo(),
                    ({ Key: ConsoleKey.R, Modifiers: ConsoleModifiers.Control }, _)
                        => new Restart(),
                    ({ Key: ConsoleKey.X }, _)
                        => new Ok(isSealed: true),
                    ({ Key: ConsoleKey.Spacebar or ConsoleKey.Enter }, _)
                        => new Ok(isSealed: false),
                    _ => null,
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
                    Console.ForegroundColor = hint.Value;
                    if (hint.Validated)
                        Console.BackgroundColor = validatedBackgroundColor;
                    Console.Write(hint.Total);
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
                    Console.ForegroundColor = hint.Value;
                    if (hint.Validated)
                        Console.BackgroundColor = validatedBackgroundColor;
                    Console.Write(hint.Total);
                    Console.ResetColor();
                }
                WriteAt('│', maxCol, maxRow + y + 1);
                WriteAt('│', maxCol + 1 + nonogram.Width, maxRow + y + 1);
            }

            WriteAt('┌', maxCol, maxRow);

            WriteAt('┐', maxCol + nonogram.Width + 1, maxRow);

            WriteAt('└', maxCol, maxRow + nonogram.Height + 1);

            WriteAt('┘', maxCol + nonogram.Width + 1, maxRow + nonogram.Height + 1);

            foreach (var (x, y) in nonogram.GenerateCoord())
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
                    _ => throw new Exception(),
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
