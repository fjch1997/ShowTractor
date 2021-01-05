using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShowTractor.Mvvm
{
    public interface IAsyncCommand : ICommand
    {
        ValueTask ExecuteAsync(object? parameter);
    }
    public class AwaitableDelegateCommand : AwaitableDelegateCommand<object>
    {
        public AwaitableDelegateCommand(Func<ValueTask> executeMethod)
            : base(o => executeMethod()) { }
        public AwaitableDelegateCommand(Func<ValueTask> executeMethod, Func<Exception, ValueTask> exceptionAction)
            : base(o => executeMethod(), exceptionAction) { }
    }

    public class AwaitableDelegateCommand<T> : IAsyncCommand
    {
        private readonly Func<T?, ValueTask> executeMethod;
        private readonly Func<Exception, ValueTask>? exceptionAction;
        private bool isExecuting;

        public AwaitableDelegateCommand(Func<T?, ValueTask> executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        public AwaitableDelegateCommand(Func<T?, ValueTask> executeMethod, Func<Exception, ValueTask> exceptionAction)
        {
            this.executeMethod = executeMethod;
            this.exceptionAction = exceptionAction;
        }

        public async ValueTask ExecuteAsync(object? obj)
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
            try
            {
                isExecuting = true;
                await executeMethod((T?)obj);
            }
            catch (Exception ex)
            {
                if (exceptionAction != null)
                    await exceptionAction(ex);
                else
                    throw;
            }
            finally
            {
                isExecuting = false;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public ICommand Command { get { return this; } }

        public bool CanExecute(object parameter)
        {
            return !isExecuting;
        }

        public event EventHandler? CanExecuteChanged;

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }
    }
}
