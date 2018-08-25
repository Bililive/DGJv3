using System;
using System.Windows.Input;

namespace DGJv3
{
    internal class UniversalCommand : ICommand
    {
        private Func<object, bool> _canExecute = null;

        private Action<object> _execute = null;

        public UniversalCommand(Action<object> execute) : this(null, execute) { }

        public UniversalCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

#pragma warning disable CS0067 // 从不使用事件
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter)
        {
            return _canExecute != null ? _canExecute(parameter) : true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
