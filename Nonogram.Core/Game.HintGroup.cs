using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nonogram
{
    public partial class Game<T> where T : notnull
    {
        public class HintGroup : INotifyPropertyChanged, IEnumerable<Hint>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public Hint[] Hints { get; init; }

            public int Length => Hints.Length;

            public Hint this[Index index]
                => Hints[index];

            public HintGroup(Game<T>.Hint[] hints)
                => Hints = hints;

            private bool _isGroupInvalid;
            public bool IsGroupInvalid
            {
                get => _isGroupInvalid;
                set => this.OnPropertyChanged(ref _isGroupInvalid, in value, PropertyChanged);
            }

            public IEnumerator<Game<T>.Hint> GetEnumerator()
                => ((IEnumerable<Game<T>.Hint>)Hints).GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
