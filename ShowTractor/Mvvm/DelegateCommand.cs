using System;
using System.Windows.Input;

namespace ShowTractor.Mvvm
{
    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action executeMethod)
            : base(o => executeMethod())
        {
        }
    }

    /// <summary>
    /// A command that calls the specified delegate when the command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _executeMethod;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public DelegateCommand(Action<T> executeMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod), "Execute Method cannot be null");
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !_isExecuting && CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            _isExecuting = true;
            try
            {
                Execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public bool CanExecute(T parameter) => true;

        public void Execute(T parameter)
        {
            _executeMethod(parameter);
        }
    }
}
