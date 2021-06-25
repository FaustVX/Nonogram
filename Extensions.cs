using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public static class Extensions
    {
        public static int[] CalculateGroups(params bool[] row)
            => CalculateGroups(row.AsEnumerable());

        public static int[] CalculateGroups(IEnumerable<bool> row)
            => Game<bool>.CalculateHints(row).Where(g => g.color).Select(g => g.qty).ToArray();

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, params T[] row)
            where T : notnull
            => CalculateGroups(ignored, row.AsEnumerable());

        public static (T color, int qty)[] CalculateGroups<T>(T ignored, IEnumerable<T> row)
            where T : notnull
            => Game<T>.CalculateHints(row).Where(g => !g.color.Equals(ignored)).ToArray();

        public static TOut ConvertTo<TIn, TOut>(this TIn color, TIn[] oldPossibleColors, TOut[] newPossibleColors, TOut ignoredColor)
        where TIn : notnull
        where TOut : notnull
        {
            for (var i = 0; i < oldPossibleColors.Length; i++)
                if (color.Equals(oldPossibleColors[i]))
                    return newPossibleColors[i];
            return ignoredColor;
        }

        public delegate bool TryConvert<T>(string input, out T output);
        public static T Ask<T>(string prompt, TryConvert<T> converter)
        {
            T output;
            do
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(prompt);
                Console.ResetColor();
            } while (!(Console.ReadLine() is string read && converter(read, out output)));
            return output;
        }

        public static IEnumerable<T> GetCol<T>(this T[,] array, int col)
            => GetRowCol(array, 0, i => array[i, col]);

        public static IEnumerable<T> GetRow<T>(this T[,] array, int row)
            => GetRowCol(array, 1, i => array[row, i]);

        private static IEnumerable<T> GetRowCol<T>(T[,] array, int dimension, Func<int, T> get)
        {
            return array.GetLength(dimension) is not 0 and var length
                ? Execute(length, get)
                : Enumerable.Empty<T>();

            static IEnumerable<T> Execute(int length, Func<int, T> get)
            {
                for (var i = 0; i < length; i++)
                    yield return get(i);
            }
        }
    }
}
