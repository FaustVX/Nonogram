using System;
using System.Collections.Generic;
using System.Linq;
using static NonogramRow.Extensions;

namespace NonogramRow
{
    public static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
            var groups0 = CalculateGroups();
                groups0 = CalculateGroups(Enumerable.Repeat(false, 5));
                groups0 = CalculateGroups(Enumerable.Repeat(true, 5));
                groups0 = CalculateGroups(false, true, true, true, true, true, false, true, true, true, true);
                groups0 = CalculateGroups(true, true, false, true, false, false, true, true, true, false, false);
                groups0 = CalculateGroups(false, false, false, false, true, true, false, false, true, false, true, true, true);
                groups0 = CalculateGroups(true, false, true, false, true, false, true, false, true, false, true, false, true, false, true);
            var groups1 = CalculateGroups(0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 5, 0, 2, 1);
            var nonogram = Nonogram.Create(new[,]
            {
                {0, 0, 1, 0, 0},
                {0, 1, 1, 1, 0},
                {1, 1, 1, 1, 1},
                {1, 1, 2, 1, 1},
                {1, 1, 2, 1, 1},
            }).ConvertTo(Console.BackgroundColor, ConsoleColor.White, ConsoleColor.Red);
            Play(nonogram, ConsoleColor.DarkGray);
            foreach (var group in Nonogram<char>.CalculateHints(GetCharFromConsole()))
                System.Console.WriteLine($"\b\n'{group.color}': {group.qty:00}");
        }

        private static IEnumerable<char> GetCharFromConsole()
        {
            while (Console.ReadKey() is { KeyChar: var key, Key: not ConsoleKey.Escape })
                yield return key;
        }

        private static void Play(Nonogram<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor)
        {
            var selectedColor = 0;
            do
            {
                Print(nonogram, validatedBackgroundColor, nonogram.PossibleValue[selectedColor]);

                Console.BackgroundColor = nonogram.IgnoredValue;
                Console.Write($"  {0}  ");
                for (int i = 0; i < nonogram.PossibleValue.Length; i++)
                {
                    Console.BackgroundColor = nonogram.PossibleValue[i];
                    Console.ForegroundColor = nonogram.IgnoredValue;
                    Console.Write($"  {i+1}  ");
                }
                Console.ResetColor();
                Console.WriteLine();
                var selected = Ask<int?>("Chose Color", SelectColor);
                selectedColor = selected is int s ? s : selectedColor;
                Print(nonogram, validatedBackgroundColor, nonogram.PossibleValue[selectedColor]);
                var x = Ask("X", (string i, out int o) => int.TryParse(i, out o) && o >= 1 && o <= nonogram.Width) - 1;
                var y = Ask("Y", (string i, out int o) => int.TryParse(i, out o) && o >= 1 && o <= nonogram.Height) - 1;
                nonogram.ValidateHints(x, y, selected is null ? nonogram.IgnoredValue : nonogram.PossibleValue[selectedColor]);
            } while (!nonogram.IsCorrect);

            bool SelectColor(string input, out int? output)
            {
                if (!int.TryParse(input, out var o))
                {
                    output = default;
                    return false;
                }
                if (o is 0)
                {
                    output = null;
                    return true;
                }
                if (o >= 1 && o <= nonogram.PossibleValue.Length)
                {
                    output = o - 1;
                    return true;
                }
                output = default;
                return false;
            }
        }

        private static void Print(Nonogram<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor, ConsoleColor selectedColor)
        {
            //█▓▒░┌┐└┘─│
            var maxRow = nonogram.ColHints.Max(h => h.Length) + 1;
            var maxCol = nonogram.RowHints.Max(h => h.Length) + 1;
            Console.Clear();

            for (var x = 0; x < nonogram.ColHints.Length; x++)
            {
                for (var i = nonogram.ColHints[x].Length - 1; i >= 0; i--)
                {
                    var hint = nonogram.ColHints[x][i];
                    Console.SetCursorPosition(maxCol + x + 1, i);
                    Console.ForegroundColor = hint.color;
                    if (hint.validated)
                        Console.BackgroundColor = validatedBackgroundColor;
                    Console.Write(hint.qty);
                    Console.ResetColor();
                }
                Console.SetCursorPosition(maxCol + x + 1, maxRow);
                Console.Write('─');
                Console.SetCursorPosition(maxCol + x + 1, maxRow + 1 + nonogram.Height);
                Console.Write('─');
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
                Console.SetCursorPosition(maxCol, maxRow + y + 1);
                Console.Write('│');
                Console.SetCursorPosition(maxCol + 1 + nonogram.Width, maxRow + y + 1);
                Console.Write('│');
            }

            for (int x = 0; x < nonogram.Width; x++)
                for (int y = 0; y < nonogram.Height; y++)
                {
                    Console.SetCursorPosition(x + maxCol + 1, y + maxRow + 1);
                    Console.ForegroundColor = nonogram[x, y];
                    Console.Write(nonogram[x, y] == selectedColor ? '█' : '▒');
                }
            Console.ResetColor();
            Console.SetCursorPosition(0, Console.CursorTop + 2);
        }
    }
}
