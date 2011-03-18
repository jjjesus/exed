#region

using System.Windows;
using System.Windows.Input;

#endregion

namespace XmlEditor.Presentation.Helpers
{
    public class AttachedProperties
    {
        public static DependencyProperty registerCommandBindingsProperty =
            DependencyProperty.RegisterAttached("RegisterCommandBindings", typeof (CommandBindingCollection),
                                                typeof (AttachedProperties), new PropertyMetadata(null, OnRegisterCommandBindingChanged));

        public static void SetRegisterCommandBindings(UIElement element, CommandBindingCollection value) { if (element != null) element.SetValue(registerCommandBindingsProperty, value); }
        public static CommandBindingCollection GetRegisterCommandBindings(UIElement element) { return (element != null ? (CommandBindingCollection) element.GetValue(registerCommandBindingsProperty) : null); }

        private static void OnRegisterCommandBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var element = sender as UIElement;
            if (element == null) return;
            var bindings = e.NewValue as CommandBindingCollection;
            if (bindings != null) element.CommandBindings.AddRange(bindings);
        }

    }
}