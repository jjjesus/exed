#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

#endregion

namespace XmlEditor.Applications.Helpers
{
    /// <summary>
    ///   A command whose sole purpose is to 
    ///   relay its functionality to other
    ///   objects by invoking delegates. The
    ///   default return value for the CanExecute
    ///   method is 'true'.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;

        #endregion Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
            : this(execute, null) {}

        /// <summary>
        ///   Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">
        ///   The execution status logic.
        /// </param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute) {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion Constructors

        #region Public Methods

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            return canExecute == null ? true : canExecute((T) parameter);
        }

        public void Execute(object parameter) {
            execute((T) parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion Public Methods

        #region Events

        public event EventHandler CanExecuteChanged {
            add {
                if (canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove {
                if (canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        #endregion Events
    }

    /// <summary>
    ///   A command whose sole purpose is to 
    ///   relay its functionality to other
    ///   objects by invoking delegates. The
    ///   default return value for the CanExecute
    ///   method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Func<bool> canExecute;
        private readonly Action execute;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null) {}

        /// <summary>
        ///   Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">
        ///   The execution status logic.
        /// </param>
        public RelayCommand(Action execute, Func<bool> canExecute) {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion Constructors

        #region Public Methods

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            return canExecute == null ? true : canExecute();
        }

        public void Execute(object parameter) {
            execute();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion Public Methods

        #region Events

        public event EventHandler CanExecuteChanged {
            add {
                if (canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove {
                if (canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        #endregion Events
    }
}