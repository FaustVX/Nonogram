using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using static Nonogram.Extensions;

namespace Nonogram.WPF.MarkupExtensions
{
    public class LoadExtension : MarkupExtension
    {
        public LoadExtension(string property)
            => Property = property;

        public string Property { get; }
        public string? Default { get; init; }

        public override object? ProvideValue(IServiceProvider serviceProvider)
            => (serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget) switch
            {
                { TargetProperty: DependencyProperty { PropertyType: var type, DefaultMetadata: { DefaultValue: var def } } }
                    => GetValue(type, def),
                { TargetProperty: PropertyInfo { PropertyType: var type } }
                    => GetValue(type, null),
                _ => null,
            };

        private object? GetValue(Type type, object? def)
        {
            var settings = Load<JObject>("XAML") ?? new();
            var result = settings[Property]?.ToObject(type);
            if (result is null)
            {
                settings[Property] = JToken.FromObject(result = JToken.Parse('"' + (Default ?? def?.ToString()!) + '"').ToObject(type)!);
                Save("XAML", settings);
            }
            return result;
        }
    }
}
