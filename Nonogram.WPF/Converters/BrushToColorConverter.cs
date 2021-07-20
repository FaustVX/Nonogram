﻿using System.Windows.Media;

namespace Nonogram.WPF.Converters
{
    public class BrushToColorConverter : IValueConverter<SolidColorBrush?, Color>
    {
        public Color NullColor { get; set; }

        public Color Convert(SolidColorBrush? value)
            => value?.Color ?? NullColor;
    }
}
