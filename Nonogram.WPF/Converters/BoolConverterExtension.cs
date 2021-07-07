using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nonogram.WPF.Converters
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class BoolConverterExtension : MarkupExtension
    {
        public BoolConverterExtension()
        {
            Type = typeof(bool);
        }

        public BoolConverterExtension(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
        public object True { get; set; } = default!;
        public object False { get; set; } = default!;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var type = typeof(BoolToTConverter<>).MakeGenericType(Type);
            return type.GetConstructor(new[] { True.GetType(), False.GetType() })!.Invoke(new[] { True, False })!;
        }
    }
}
