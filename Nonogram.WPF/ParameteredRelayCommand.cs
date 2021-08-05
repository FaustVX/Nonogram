using System;
using System.Windows.Input;

namespace Nonogram.WPF
{
    internal class ParameteredRelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action<object?> _action;

        public ParameteredRelayCommand(Action<object?> action)
            => _action = action;

        public bool CanExecute(object? parameter)
            => true;
        public void Execute(object? parameter)
            => _action(parameter);
    }
}