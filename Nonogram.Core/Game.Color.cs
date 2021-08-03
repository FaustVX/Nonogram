using System.ComponentModel;

namespace Nonogram
{
    public partial class Game<T> where T : notnull
    {
        public class Color : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; }

            private int _current;
            public int Current
            {
                get => _current;
                set
                {
                    this.OnPropertyChanged(ref _current, in value, PropertyChanged);
                    Validated = Current == Total;
                }
            }

            public int Total { get; }

            private bool _validated;
            public bool Validated
            {
                get => _validated;
                set => this.OnPropertyChanged(ref _validated, in value, PropertyChanged);
            }

            public Color(T value, int total)
                => (Value, Total) = (value, total);

            public override int GetHashCode()
                => Value.GetHashCode();

            public override bool Equals(object? obj)
                => obj is Color c && ColorEqualizer(c.Value, Value);
        }
    }
}
