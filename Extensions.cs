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
        public static bool Switch(string @true, string @false)
        {
            //←→
            Console.WriteLine($"{@true} < Press arrow > {@false}");
            while (Console.ReadKey(intercept: true) is { Key: var key })
                if (key is ConsoleKey.LeftArrow)
                    return true;
                else if (key is ConsoleKey.RightArrow)
                    return false;
            throw null;
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

        public static void WriteAt(char c, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(c);
        }

        public static TOut[,] ReduceArray<TIn, TOut>(this TIn[,] arrayIn, int width, int height, Func<ROSpan2D<TIn>, TOut> converter)
        where TOut : notnull
        {
            var (inWidth, inHeight) = (arrayIn.GetLength(1), arrayIn.GetLength(0));
            var arrayOut = new TOut[height, width];
            var stepSizeX = inWidth / width;
            var stepSizeY = inHeight / height;
            var lengthX = inWidth - (stepSizeX * (width - 1));
            var lengthY = inHeight - (stepSizeY * (height - 1));

            var temp = new ROSpan2D<TIn>(arrayIn, 0, 0, lengthX, lengthY);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    arrayOut[y, x] = converter(temp.Offset(stepSizeX * x, stepSizeY * y));
            return arrayOut;
        }
    }
}
