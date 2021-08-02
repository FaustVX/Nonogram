using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
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
            var result = Convert(settings[Property], type);
            if (result is null)
            {
                settings[Property] = JToken.FromObject(result = JToken.Parse('"' + (Default ?? def?.ToString()!) + '"').ToObject(type)!);
                Save("XAML", settings);
            }
            return result;
        }

        private object? Convert(JToken? settings, Type type)
        {
            try
            {
                if (settings is JValue { Type: JTokenType.String, Value: string value }
                    && type.GetCustomAttribute<TypeConverterAttribute>()?.ConverterTypeName is string converterName
                    && Type.GetType(converterName) is Type converter
                    && Activator.CreateInstance(converter) is TypeConverter typeConverter)
                    return typeConverter.ConvertFromString(value);
                return settings?.ToObject(type);
            }
            catch
            {
                return null;
            }
        }
    }
}
