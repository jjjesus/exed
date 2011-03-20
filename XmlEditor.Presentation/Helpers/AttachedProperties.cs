#region

using System.Windows;
using System.Windows.Input;

#endregion

namespace XmlEditor.Presentation.Helpers
{
    /// <summary>
    /// </summary>
    public class AttachedProperties
    {
        #region Register Command Bindings

        public static DependencyProperty registerCommandBindingsProperty =
            DependencyProperty.RegisterAttached("RegisterCommandBindings", typeof(CommandBindingCollection),
                                                typeof(AttachedProperties),
                                                new PropertyMetadata(null, OnRegisterCommandBindingChanged));

        public static void SetRegisterCommandBindings(UIElement element, CommandBindingCollection value) { if (element != null) element.SetValue(registerCommandBindingsProperty, value); }
        public static CommandBindingCollection GetRegisterCommandBindings(UIElement element) { return (element != null ? (CommandBindingCollection)element.GetValue(registerCommandBindingsProperty) : null); }

        private static void OnRegisterCommandBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;
            if (element == null) return;
            var bindings = e.NewValue as CommandBindingCollection;
            if (bindings != null) element.CommandBindings.AddRange(bindings);
        }

        #endregion Register Command Bindings

        #region Drag Drop Support

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(AttachedProperties), new PropertyMetadata(null,OnDropCommandChange));

        public static void SetDropCommand(DependencyObject source, ICommand value) { source.SetValue(DropCommandProperty, value); }

        public static ICommand GetDropCommand(DependencyObject source) { return (ICommand)source.GetValue(DropCommandProperty); }

        private static void OnDropCommandChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var command = e.NewValue as ICommand;
            var uiElement = d as UIElement;
            if (command == null || uiElement == null) return;
            uiElement.Drop += (sender, args) => command.Execute(args.Data); 
            uiElement.DragOver += (sender, args) => {
                                       args.Effects = command.CanExecute(args.Data)
                                                          ? DragDropEffects.Copy
                                                          : DragDropEffects.None;
                                       args.Handled = true;
                                   };

            // todo: if e.OldValue is not null, detatch the handler that references it
        }

        #endregion Drag Drop Support
    }
}