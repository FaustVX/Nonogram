using System;
using System.Windows.Input;

namespace Nonogram.WPF
{
    internal class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action _action;

        public RelayCommand(Action action)
            => _action = action;

        public bool CanExecute(object? parameter)
            => true;
        public void Execute(object? parameter)
            => _action();
    }
}