using System.Windows.Input;

namespace Nonogram.WPF
{
    public class MouseExtraButtonGesture : MouseGesture
    {
        public MouseExtraButtonGesture(MouseButton mouseButton)
            => MouseButton = mouseButton;

        public MouseButton MouseButton { get; }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
            => inputEventArgs is MouseButtonEventArgs { ChangedButton: var button } && MouseButton == button;
    }
}