using System;
using System.Windows;
using System.Windows.Input;
using static Nonogram.WPF.Extensions;

namespace Nonogram.WPF.DependencyProperties
{
    public static class Measure
    {
        public static bool GetStarted(DependencyObject obj)
            => (bool)obj.GetValue(StartedProperty);

        public static void SetStarted(DependencyObject obj, bool value)
            => obj.SetValue(StartedProperty, value);

        // Using a DependencyProperty as the backing store for Started.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartedProperty =
            DependencyProperty.RegisterAttached("Started", typeof(bool), typeof(Measure), new PropertyMetadata(false));




        public static FrameworkElement? GetStartPoint(DependencyObject obj)
            => (FrameworkElement?)obj.GetValue(StartPointProperty);

        public static void SetStartPoint(DependencyObject obj, FrameworkElement? value)
            => obj.SetValue(StartPointProperty, value);

        // Using a DependencyProperty as the backing store for StartPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartPointProperty =
            DependencyProperty.RegisterAttached("StartPoint", typeof(FrameworkElement), typeof(Measure), new PropertyMetadata(null, StartPointChanged));

        private static void StartPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d, e.OldValue, e.NewValue)
            {
                case (Window w, null, not null):
                    SetStarted(w, true);
                    break;
                case (Window w, not null, null):
                    SetStarted(w, false);
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
                    elem.MouseEnter += MouseEnter;
                    elem.MouseUp += MouseUp;
                    break;
            }
        }

        private static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            var window = GetTool(elem);
            if (!GetStarted(window))
                return;
            SetStartPoint(window, elem);
            e.Handled = true;
        }

        private static void MouseEnter(object sender, MouseEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            var window = GetTool(elem);
            if (!GetStarted(window))
                return;
            e.Handled = true;
        }

        private static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            var window = GetTool(elem);
            if (!GetStarted(window) || GetStartPoint(window) is not FrameworkElement start)
                return;
            var startPos = GetXYFromTag(start);
            var endPos = GetXYFromTag(elem);
            MessageBox.Show(window, $"The size of this selection is (W: {Math.Abs(endPos.x - startPos.x) + 1} - H: {Math.Abs(endPos.y - startPos.y) + 1})", "Measure", MessageBoxButton.OK, MessageBoxImage.Information);
            SetStartPoint(window, null);
            e.Handled = true;
        }
    }
}
