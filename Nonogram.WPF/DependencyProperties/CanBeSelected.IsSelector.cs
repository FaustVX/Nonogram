﻿using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Nonogram.WPF.DependencyProperties
{
    public static partial class CanBeSelected
    {
        public static IList GetIsSelector(DependencyObject obj)
            => (IList)obj.GetValue(IsSelectorProperty);

        public static void SetIsSelector(DependencyObject obj, IList value)
            => obj.SetValue(IsSelectorProperty, value);

        // Using a DependencyProperty as the backing store for IsSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectorProperty =
            DependencyProperty.RegisterAttached("IsSelector", typeof(IList), typeof(CanBeSelected), new PropertyMetadata(null, IsSelectorChanged));

        private static void IsSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _cache = new();
            _lastColorIndex = 0;
            switch (d, e.OldValue, e.NewValue)
            {
                case (ListBox list, null, IList colors):
                    _window = GetRoot(d)!;
                    _window.MouseWheel += MouseWheel;
                    _window.KeyUp += KeyUp;
                    _colors = SetValues(list, colors);
                    break;
                case (ListBox list, _, IList colors):
                    _colors = SetValues(list, colors);
                    break;
            }

            static IList SetValues(ListBox list, IList colors)
            {
                list.SetBinding(ListBox.SelectedIndexProperty, new Binding($"(dp:{nameof(CanBeSelected)}.{SelectedColorProperty.Name})")
                {
                    RelativeSource = new(RelativeSourceMode.FindAncestor, typeof(Window), 1),
                    Mode = BindingMode.TwoWay,
                });
                list.ItemsSource = colors;
                return colors;
            }

            static Window? GetRoot(DependencyObject obj)
            {
                var elem = obj as FrameworkElement;
                while ((obj, elem) is (not null, not null))
                {
                    obj = elem;
                    elem = elem.Parent as FrameworkElement;
                }
                return obj as Window;
            }
        }

        private static Window _window = default!;
        private static IList _colors = default!;
        private static int _lastColorIndex = 0;

        private static void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var offset = e.Delta switch
            {
                > 0 => GetOffset(_colors.Count - 1),
                0 => 0,
                < 0 => GetOffset(+1),
            };
            SetSelectedColor(_window, (GetSelectedColor(_window) + offset) % _colors.Count);
            e.Handled = true;

            static int GetOffset(int offset)
            {
                if (ValidateSelectedColor((GetSelectedColor(_window) + offset) % _colors.Count))
                    return offset;
                return GetOffset(offset + offset);
            }
        }

        private static void KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    var temp = _lastColorIndex;
                    _lastColorIndex = GetSelectedColor(_window);
                    SetSelectedColor(_window, temp);
                    e.Handled = true;
                    break;
                case >= Key.D1 and <= Key.D9 and var key when (key - Key.D1) is var pos && pos < _colors.Count && ValidateSelectedColor(pos):
                    if (GetSelectedColor(_window) != key - Key.D1)
                    {
                        _lastColorIndex = GetSelectedColor(_window);
                        SetSelectedColor(_window, key - Key.D1);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private static bool ValidateSelectedColor(int i)
            => i >= 0 && (_colors is { Count: > 0 } && _cache is not null && _cache.TryGetValue(_colors[i]!, out var elem)
                ? !GetIsLocked(elem)
                : true);
    }
}
