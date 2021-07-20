using System;
using System.Windows;
using System.Windows.Input;
using static Nonogram.WPF.Extensions;

namespace Nonogram.WPF.DependencyProperties
{
    public static class Measure
    {
        public static bool GetIsStarted(DependencyObject obj)
            => (bool)obj.GetValue(IsStartedProperty);

        public static void SetIsStarted(DependencyObject obj, bool value)
            => obj.SetValue(IsStartedProperty, value);

        // Using a DependencyProperty as the backing store for IsStarted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStartedProperty =
            DependencyProperty.RegisterAttached("IsStarted", typeof(bool), typeof(Measure), new PropertyMetadata(false, IsStartedChanged));

        private static void IsStartedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => SetPoint(d, null);



        public static FrameworkElement? GetPoint(DependencyObject obj)
            => (FrameworkElement?)obj.GetValue(PointProperty);

        public static void SetPoint(DependencyObject obj, FrameworkElement? value)
            => obj.SetValue(PointProperty, value);

        // Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointProperty =
            DependencyProperty.RegisterAttached("Point", typeof(FrameworkElement), typeof(Measure), new PropertyMetadata(null, PointChanged));

        private static void PointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d, e.OldValue, e.NewValue)
            {
                case (Window w, null, not null):
                    SetIsStarted(w, true);
                    break;
                case (Window w, FrameworkElement old, FrameworkElement @new):
                    var startPos = GetXYFromTag(old);
                    var endPos = GetXYFromTag(@new);
                    MessageBox.Show(w, $"The size of this selection is (W: {Math.Abs(endPos.x - startPos.x) + 1} - H: {Math.Abs(endPos.y - startPos.y) + 1})", "Measure", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetPoint(w, null);
                    break;
                case (Window w, not null, null):
                    SetIsStarted(w, false);
                    break;
            }
        }



        public static Window GetTool(DependencyObject obj)
            => (Window)obj.GetValue(ToolProperty);

        public static void SetTool(DependencyObject obj, Window value)
            => obj.SetValue(ToolProperty, value);

        // Using a DependencyProperty as the backing store for Tool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolProperty =
            DependencyProperty.RegisterAttached("Tool", typeof(Window), typeof(Measure), new PropertyMetadata(null, ToolChanged));

        private static void ToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d, e.OldValue, e.NewValue)
            {
                case (FrameworkElement elem, null, Window w):
                    elem.MouseDown += MouseDown;
                    elem.MouseUp += MouseUp;
                    break;
            }
        }

        private static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            var window = GetTool(elem);
            if (!GetIsStarted(window))
                return;
            SetPoint(window, elem);
            e.Handled = true;
        }

        private static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            var window = GetTool(elem);
            if (!GetIsStarted(window) || GetPoint(window) is null)
                return;
            SetPoint(window, elem);
            e.Handled = true;
        }
    }
}
