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
            });
            nonogram.ValidateHints(2, 0, 1);
            nonogram.ValidateHints(2, 1, 1);
            nonogram.ValidateHints(2, 2, 1);
            nonogram.ValidateHints(2, 3, 2);
            nonogram.ValidateHints(2, 4, 2);
            foreach (var group in Nonogram<char>.CalculateHints(GetCharFromConsole()))
                System.Console.WriteLine($"\b\n'{group.color}': {group.qty:00}");
        }

        private static IEnumerable<char> GetCharFromConsole()
        {
            while (Console.ReadKey() is { KeyChar: var key, Key: not ConsoleKey.Escape })
                yield return key;
        }
    }
}
