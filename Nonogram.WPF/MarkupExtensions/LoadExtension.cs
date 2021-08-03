using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using static Nonogram.Extensions;

namespace Nonogram.WPF.MarkupExtensions
{
    public class LoadExtension : MarkupExtension
    {
        public LoadExtension(string property)
            => Properties = property.Split('/');

        public string[] Properties { get; }
        public string? Default { get; init; }
        public bool AddRoot { get; init; } = true;
        public string? NameSpace { get; init; } = "XAML";

        public override object? ProvideValue(IServiceProvider serviceProvider)
            => (serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget) switch
            {
                { TargetProperty: DependencyProperty { PropertyType: var type, DefaultMetadata: { DefaultValue: var def } } }
                    => GetValue(type, def, (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider))!),
                { TargetObject: Setter { Property: { PropertyType: var type, DefaultMetadata: { DefaultValue: var def } } } }
                    => GetValue(type, def, (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider))!),
                { TargetProperty: PropertyInfo { PropertyType: var type } }
                    => GetValue(type, null, (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider))!),
                _ => null,
            };

        private object? GetValue(Type type, object? def, IRootObjectProvider root)
        {
            if (!AddRoot && NameSpace is null)
                throw new Exception("Can't find a root property. Consider Adding a Namespace or set AddRoot to True");
            var rootName = root.RootObject.GetType().Name;
            var settings = Load<JObject>(NameSpace ?? rootName) ?? new();
            if (AddRoot && NameSpace is not null)
                settings = (JObject)(settings[rootName] ??= new JObject());
            var result = Convert(GetToken(settings), type);
            if (result is null)
            {
                GetToken(settings).Parent!.Parent![Properties[^1]] = JToken.FromObject(result = JToken.Parse('"' + (Default ?? def?.ToString()!) + '"').ToObject(type)!);
                Save(NameSpace ?? rootName, settings);
            }
            return result;
        }

        private JToken GetToken(JObject obj)
        {
            JToken? token = obj;
            foreach (var property in Properties)
            {
                token = (token[property] ??= new JObject());
            }
            return token;
        }

        private static object? Convert(JToken? settings, Type type)
        {
            try
            {
                if (settings is JValue { Type: JTokenType.String, Value: string reference } && reference.StartsWith("@`") && reference.EndsWith('`'))
                    return Convert(new LoadExtension(reference[2..^1]).GetToken((JObject)settings.Root), type);
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
