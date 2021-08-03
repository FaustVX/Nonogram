using System.ComponentModel;

namespace Nonogram
{
    public partial class Game<T> where T : notnull
    {
        public class Hint : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public T Value { get; }

            public int Total { get; }

            private bool _validated;
            public bool Validated
            {
                get => _validated;
                set => this.OnPropertyChanged(ref _validated, in value, PropertyChanged);
            }

            public Hint(T value, int total)
                => (Value, Total) = (value, total);

            public override int GetHashCode()
                => (Value, Total).GetHashCode();

            public override bool Equals(object? obj)
                => obj is Hint c && c.Total == Total && ColorEqualizer(c.Value, Value);
        }
    }
}
