﻿using System;
using System.Collections.Generic;
using System.Linq;
using static Nonogram.Extensions;

namespace Nonogram
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
            var groups1 = CalculateGroups(ignored: 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 5, 0, 2, 1);
            var nonogram = Game.Create(new[,]
            {
                {0, 0, 1, 0, 0},
                {0, 1, 1, 1, 0},
                {1, 1, 1, 1, 1},
                {1, 1, 2, 1, 1},
                {1, 1, 2, 1, 1},
            }).ConvertTo(Console.BackgroundColor, ConsoleColor.White, ConsoleColor.Red);
            Play(nonogram, ConsoleColor.DarkGray);
            foreach (var group in Game<char>.CalculateHints(GetCharFromConsole()))
                System.Console.WriteLine($"\b\n'{group.color}': {group.qty:00}");
        }

        private static IEnumerable<char> GetCharFromConsole()
        {
            while (Console.ReadKey() is { KeyChar: var key, Key: not ConsoleKey.Escape })
                yield return key;
        }

        private static void Play(Game<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor)
        {
            var selectedColor = 0;
            do
            {
                Print(nonogram, validatedBackgroundColor, nonogram.PossibleColors[selectedColor]);

                Console.BackgroundColor = nonogram.IgnoredColor;
                Console.Write($"  {0}  ");
                for (var i = 0; i < nonogram.PossibleColors.Length; i++)
                {
                    Console.BackgroundColor = nonogram.PossibleColors[i];
                    Console.ForegroundColor = nonogram.IgnoredColor;
                    Console.Write($"  {i+1}  ");
                }
                Console.ResetColor();
                Console.WriteLine();
                var selected = Ask<int?>("Chose Color :", SelectColor);
                selectedColor = selected is int s ? s : selectedColor;
                Print(nonogram, validatedBackgroundColor, nonogram.PossibleColors[selectedColor]);
                var seal = Switch("Seal", "Don't seal");
                var x = Ask("X :", (string i, out int o) => int.TryParse(i, out o) && o >= 1 && o <= nonogram.Width) - 1;
                var y = Ask("Y :", (string i, out int o) => int.TryParse(i, out o) && o >= 1 && o <= nonogram.Height) - 1;
                nonogram.ValidateHints(x, y, selected is null ? nonogram.IgnoredColor : nonogram.PossibleColors[selectedColor], seal);
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
                if (o >= 1 && o <= nonogram.PossibleColors.Length)
                {
                    output = o - 1;
                    return true;
                }
                output = default;
                return false;
            }
        }

        private static void Print(Game<ConsoleColor> nonogram, ConsoleColor validatedBackgroundColor, ConsoleColor selectedColor)
        {
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
            Console.SetCursorPosition(0, Console.CursorTop + 2);
        }
    }
}
