using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace Nonogram.WPF
{
    public static class Extensions
    {
        static Extensions()
        {
            _save = new FileInfo("Nonogram.json");
            if (!_save.Exists)
                File.WriteAllText(_save.FullName, "{}");
        }

        public static T? CheckType<T>(this object? value, bool allowNull)
            => (value, allowNull) switch
            {
                (null, true) => default,
                (T t, _) => t,
                _ => throw new NotImplementedException(),
            };
        public static object? CheckType<T1, T2>(this object? value, bool allowNull)
            => (value, allowNull) switch
            {
                (null, true) => default,
                (T1 t, _) => t,
                (T2 t, _) => t,
                _ => throw new NotImplementedException(),
            };

        public static (int x, int y) GetXYFromTag(FrameworkElement element)
            => ((int, int))element.Tag;

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
    }
}
