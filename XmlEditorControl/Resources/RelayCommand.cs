#region

using System;
using System.Diagnostics;
using System.Windows.Input;

#endregion

namespace TreeListControl.Resources
{
    ///// <summary>
    /////   A command whose sole purpose is to 
    /////   relay its functionality to other
    /////   objects by invoking delegates. The
    /////   default return value for the CanExecute
    /////   method is 'true'.
    ///// </summary>
    //public class RelayCommand<T> : ICommand
    //{
    //    #region Fields

    //    private readonly Predicate<T> _canExecute;
    //    private readonly Action<T> _execute;

    //    #endregion Fields

    //    #region Constructors

    //    public RelayCommand(Action<T> execute)
    //        : this(execute, null) {}

    //    /// <summary>
    //    ///   Creates a new command.
    //    /// </summary>
    //    /// <param name="execute">The execution logic.</param>
    //    /// <param name="canExecute">
    //    ///   The execution status logic.
    //    /// </param>
    //    public RelayCommand(Action<T> execute, Predicate<T> canExecute) {
    //        if (execute == null)
    //            throw new ArgumentNullException("execute");

    //        _execute = execute;
    //        _canExecute = canExecute;
    //    }

    //    #endregion Constructors

    //    #region Public Methods

    //    [DebuggerStepThrough]
    //    public bool CanExecute(object parameter) {
    //        return _canExecute == null ? true : _canExecute((T) parameter);
    //    }

    //    public void Execute(object parameter) {
    //        _execute((T) parameter);
    //    }

    //    #endregion Public Methods

    //    #region Events

    //    public event EventHandler CanExecuteChanged {
    //        add {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested += value;
    //        }
    //        remove {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested -= value;
    //        }
    //    }

    //    #endregion Events
    //}

    ///// <summary>
    /////   A command whose sole purpose is to 
    /////   relay its functionality to other
    /////   objects by invoking delegates. The
    /////   default return value for the CanExecute
    /////   method is 'true'.
    ///// </summary>
    //public class RelayCommand : ICommand
    //{
    //    #region Fields

    //    private readonly Func<bool> _canExecute;
    //    private readonly Action _execute;

    //    #endregion Fields

    //    #region Constructors

    //    /// <summary>
    //    ///   Creates a new command that can always execute.
    //    /// </summary>
    //    /// <param name="execute">The execution logic.</param>
    //    public RelayCommand(Action execute)
    //        : this(execute, null) {}

    //    /// <summary>
    //    ///   Creates a new command.
    //    /// </summary>
    //    /// <param name="execute">The execution logic.</param>
    //    /// <param name="canExecute">
    //    ///   The execution status logic.
    //    /// </param>
    //    public RelayCommand(Action execute, Func<bool> canExecute) {
    //        if (execute == null)
    //            throw new ArgumentNullException("execute");

    //        _execute = execute;
    //        _canExecute = canExecute;
    //    }

    //    #endregion Constructors

    //    #region Public Methods

    //    [DebuggerStepThrough]
    //    public bool CanExecute(object parameter) {
    //        return _canExecute == null ? true : _canExecute();
    //    }

    //    public void Execute(object parameter) {
    //        _execute();
    //    }

    //    #endregion Public Methods

    //    #region Events

    //    public event EventHandler CanExecuteChanged {
    //        add {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested += value;
    //        }
    //        remove {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested -= value;
    //        }
    //    }

    //    #endregion Events
    //}
}