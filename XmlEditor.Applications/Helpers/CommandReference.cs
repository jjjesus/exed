﻿#region

using System;
using System.Windows;
using System.Windows.Input;

#endregion

namespace XmlEditor.Applications.Helpers
{
    /// <summary>
    ///   This class facilitates associating a key binding in XAML markup to a command
    ///   defined in a View Model by exposing a Command dependency property.
    ///   The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
    /// </summary>
    public class CommandReference : Freezable, ICommand
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof (ICommand),
                                                                                                typeof (CommandReference),
                                                                                                new PropertyMetadata(
                                                                                                    new PropertyChangedCallback
                                                                                                        (OnCommandChanged)));

        public ICommand Command { get { return (ICommand) GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        #region ICommand Members

        public bool CanExecute(object parameter) {
            return Command != null && Command.CanExecute(parameter);
        }

        public void Execute(object parameter) { Command.Execute(parameter); }

        public event EventHandler CanExecuteChanged;

        #endregion

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var commandReference = d as CommandReference;
            if (commandReference == null) return;
            var oldCommand = e.OldValue as ICommand;
            var newCommand = e.NewValue as ICommand;

            if (oldCommand != null) oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            if (newCommand != null) newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
        }

        #region Freezable

        protected override Freezable CreateInstanceCore() { throw new NotImplementedException(); }

        #endregion
    }
}