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
                case (FrameworkElement elem, null, IUndoRedo g):
                    elem.InputBindings.Add(new(new RelayCommand(g.Undo), new KeyGesture(Key.Z, ModifierKeys.Control)));
                    elem.InputBindings.Add(new(new RelayCommand(g.Undo), new MouseExtraButtonGesture(MouseButton.XButton1)));
                    elem.InputBindings.Add(new(new RelayCommand(g.Redo), new KeyGesture(Key.Y, ModifierKeys.Control)));
                    elem.InputBindings.Add(new(new RelayCommand(g.Redo), new MouseExtraButtonGesture(MouseButton.XButton2)));
                    break;
            }
        }
    }
}
