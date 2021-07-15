using System;
using System.Windows;
using System.Windows.Input;

namespace Nonogram.WPF.DependencyProperties
{
    public static class UndoRedo
    {
        public static IUndoRedo? GetUndoRedo(DependencyObject obj)
            => (IUndoRedo?)obj.GetValue(UndoRedoProperty);

        public static void SetUndoRedo(DependencyObject obj, IUndoRedo? value)
            => obj.SetValue(UndoRedoProperty, value);

        // Using a DependencyProperty as the backing store for UndoRedo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UndoRedoProperty =
            DependencyProperty.RegisterAttached("UndoRedo", typeof(IUndoRedo), typeof(UndoRedo), new PropertyMetadata(null, UndoRedoChanged));

        private static void UndoRedoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d, e.OldValue, e.NewValue)
            {
                case (FrameworkElement elem, null, IUndoRedo):
                    elem.KeyUp += This_KeyUp;
                    elem.MouseUp += This_MouseUp;
                    break;
                case (FrameworkElement elem, _, null):
                    elem.KeyUp -= This_KeyUp;
                    elem.MouseUp -= This_MouseUp;
                    break;
            }
        }

        private static void This_KeyUp(object sender, KeyEventArgs e)
        {
            var game = GetUndoRedo((DependencyObject)sender)!;
            switch ((e.KeyboardDevice.Modifiers, e.Key))
            {
                case (ModifierKeys.Control, Key.Z) when !game.IsCorrect:
                    game.Undo();
                    e.Handled = true;
                    break;
                case (ModifierKeys.Control, Key.Y):
                    game.Redo();
                    e.Handled = true;
                    break;
            }
        }

        private static void This_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var game = GetUndoRedo((DependencyObject)sender)!;
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1 when !game.IsCorrect:
                    e.Handled = true;
                    game.Undo();
                    break;
                case MouseButton.XButton2:
                    e.Handled = true;
                    game.Redo();
                    break;
            }
        }
    }
}
