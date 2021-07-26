using System.Windows.Input;

namespace Nonogram.WPF
{
    public class MouseWheelGesture : MouseGesture
    {
        public static MouseWheelGesture Down => new()
            {
                Direction = WheelDirection.Down
            };

        public static MouseWheelGesture Up => new()
            {
                Direction = WheelDirection.Up
            };

        public MouseWheelGesture()
            : base(MouseAction.WheelClick)
        { }

        public MouseWheelGesture(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers)
        { }

        public WheelDirection Direction { get; init; }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs))
                return false;
            return inputEventArgs is MouseWheelEventArgs { Delta: var delta } && Direction switch
            {
                WheelDirection.None => delta == 0,
                WheelDirection.Up => delta > 0,
                WheelDirection.Down => delta < 0,
                _ => false,
            };
        }

        public enum WheelDirection
        {
            None,
            Up,
            Down,
        }
    }
}