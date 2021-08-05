using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Input;

namespace Nonogram.WPF.DependencyProperties
{
    public static class Size
    {
        public static readonly string PropertyName = "CellSize";
        public static double GetCellSize(DependencyObject obj)
            => (double)obj.GetValue(CellSizeProperty);

        public static void SetCellSize(DependencyObject obj, double value)
            => obj.SetValue(CellSizeProperty, value);

        // Using a DependencyProperty as the backing store for CellSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.RegisterAttached(PropertyName, typeof(double), typeof(Size), new PropertyMetadata(0d, CellSizeChanged, CoerceCellSize));

        private static object CoerceCellSize(DependencyObject d, object baseValue)
            => Math.Round((double)baseValue, 1, MidpointRounding.AwayFromZero);

        private static void CellSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d, e.OldValue, e.NewValue)
            {
                case (FrameworkElement o, 0d, double):
                    o.InputBindings.Add(new(new RelayCommand(() => SetCellSize(o, GetCellSize(o) + 0.1)), new KeyGesture(Key.Add, ModifierKeys.Control)));
                    o.InputBindings.Add(new(new RelayCommand(() => SetCellSize(o, GetCellSize(o) + 0.1)), new KeyGesture(Key.OemPlus, ModifierKeys.Control)));
                    o.InputBindings.Add(new(new RelayCommand(() => SetCellSize(o, GetCellSize(o) - 0.1)), new KeyGesture(Key.Subtract, ModifierKeys.Control)));
                    o.InputBindings.Add(new(new RelayCommand(() => SetCellSize(o, GetCellSize(o) - 0.1)), new KeyGesture(Key.OemMinus, ModifierKeys.Control)));
                    break;
                case (_, _, double value):
                    var settings = Nonogram.Extensions.Load<JObject>("MainWindow") ?? new();
                    settings[PropertyName] = value;
                    Nonogram.Extensions.Save(settings.Path, settings);
                    break;
            }
        }
    }
}
