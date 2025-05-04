using System;
using System.Diagnostics;
using System.Windows.Input;

namespace XMeter.Util
{
    public class RelayCommand(Action<object> execute, Predicate<object> canExecute) : ICommand
    {
        #region Fields

        private readonly Action<object> execute = execute ?? throw new ArgumentNullException(nameof(execute));

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion // ICommand Members
    }
}