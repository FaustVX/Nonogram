using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nonogram
{
    public static class Extensions
    {
        static Extensions()
        {
            _save = new FileInfo("Nonogram.json");
            if (!_save.Exists)
                File.WriteAllText(_save.FullName, "{}");
        }

        private static readonly FileInfo _save;

        public static T? Load<T>(string group)
            where T : JToken
            => (T?)JObject.Parse(File.ReadAllText(_save.FullName))[group];

        public static T Load<T>(string group, bool autosave)
            where T : JToken, new()
        {
            var save = Load<T>(group) ?? new();
            if (autosave && save is JContainer container)
                container.CollectionChanged += (s, e) => Save(group, (JToken)s!);
            return save;
        }

        public static void Save(string group, JToken data)
        {
            var save = JObject.Parse(File.ReadAllText(_save.FullName));
            save[group] = data;
            File.WriteAllText(_save.FullName, save.ToString(Newtonsoft.Json.Formatting.Indented));
        }

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

        public static TOut[,] ReduceArray<TIn, TOut>(this TIn[,] arrayIn, int width, int height, Func<ROSpan2D<TIn>, TOut> converter)
        where TOut : notnull
        {
            var (inWidth, inHeight) = (arrayIn.GetLength(1), arrayIn.GetLength(0));
            if (width <= 0)
                width = inWidth;
            if (height <= 0)
                height = inHeight;
            var arrayOut = new TOut[height, width];
            var stepSizeX = inWidth / width;
            var stepSizeY = inHeight / height;
            var lengthX = inWidth - (stepSizeX * (width - 1));
            var lengthY = inHeight - (stepSizeY * (height - 1));

            var temp = new ROSpan2D<TIn>(arrayIn, 0, 0, lengthX, lengthY);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    arrayOut[y, x] = converter(temp.Offset(stepSizeX * x, stepSizeY * y));
            return arrayOut;
        }

        public static TOut[,] To2DArray<TIn, TOut>(this TIn[][] array, Func<TIn, TOut> converter)
        {
            var height = array.Length;
            var width = array[0].Length;
            var result = new TOut[height, width];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    result[y, x] = converter(array[y][x]);
            return result;
        }

        public static IEnumerable<(int x, int y)> GenerateCoord<T>(this Game<T> @this)
            where T : notnull
            => Enumerable.Range(0, @this.Width)
                .SelectMany(x => Enumerable.Range(0, @this.Height)
                    .Select(y => (x, y)));

        public static IEnumerable<(int x, int y)> GenerateCoord<T>(this IList<T> @this, Func<T, ICollection> selectMany)
            where T : notnull
            => Enumerable.Range(0, @this.Count)
                .SelectMany(x => Enumerable.Range(0, selectMany(@this[x]).Count)
                    .Select(y => (x, y)));

        public static IEnumerable<(int x, int y)> GenerateCoord<T>(this T[,] @this)
            => Enumerable.Range(0, @this.GetLength(1))
                .SelectMany(x => Enumerable.Range(0, @this.GetLength(0))
                    .Select(y => (x, y)));

        public static bool TryGetNext<T>(this IEnumerator<T> enumerator, [MaybeNullWhen(false)] out T value)
        {
            if (enumerator.MoveNext())
            {
                value = enumerator.Current;
                return true;
            }
            value = default;
            return false;
        }

        public static T? GetNext<T>(this IEnumerator<T> enumerator)
        {
            if (TryGetNext(enumerator, out var value))
                return value;
            return default;
        }

        public static bool OnPropertyChanged<T>(this INotifyPropertyChanged @this, ref T storage, in T value, PropertyChangedEventHandler? @event, [CallerMemberName] string propertyName = default!)
        {
            if ((storage is IEquatable<T> comp && !comp.Equals(value)) || (!storage?.Equals(value) ?? (value is not null)))
            {
                storage = value;
                @event?.Invoke(@this, new(propertyName));
                return true;
            }
            return false;
        }

        public static void NotifyProperty(this INotifyPropertyChanged @this, PropertyChangedEventHandler? @event, string otherPropertyName)
            => @event?.Invoke(@this, new(otherPropertyName));
    }
}
