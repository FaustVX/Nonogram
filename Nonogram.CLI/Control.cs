namespace Nonogram.CLI
{
    public interface IControl
    { }

    public class Ok : IControl
    {
        public bool IsSealed { get; }

        public Ok(bool isSealed)
            => IsSealed = isSealed;
    }

    public class Color : IControl
    {
        public int? Index { get; }

        public Color(int? index)
            => Index = index;
    }

    public class X : IControl
    {
        public int Pos { get; }

        public X(int pos)
            => Pos = pos;
    }

    public class Y : IControl
    {
        public int Pos { get; }

        public Y(int pos)
            => Pos = pos;
    }

    public class Undo : IControl
    { }

    public class Redo : IControl
    { }

    public class Restart : IControl
    { }
}
