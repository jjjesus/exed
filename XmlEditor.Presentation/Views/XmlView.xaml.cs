#region

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IXmlView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class XmlView : IXmlView
    {
        public XmlView() {
            InitializeComponent();
        }

        private void GridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter && e.KeyboardDevice.Modifiers != ModifierKeys.None) || e.Key == Key.Tab) {
                e.Handled = true;
                var uie = e.OriginalSource as UIElement;
                if (uie == null) return;
                uie.MoveFocus(e.KeyboardDevice.Modifiers == ModifierKeys.Shift
                                  ? new TraversalRequest(FocusNavigationDirection.Up)
                                  : new TraversalRequest(FocusNavigationDirection.Down));
            }
        }

        private void EditableIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var uie = sender as UIElement;
            if (uie == null || uie.Visibility != Visibility.Visible) return;
            uie.Focus();
            var tb = sender as TextBox;
            if (tb == null) return;
            tb.SelectAll();
        }
    }
}