#region

using System;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IXmlView)), PartCreationPolicy(CreationPolicy.NonShared)] public partial class XmlView : IXmlView
    {
        private readonly Lazy<XmlViewModel> viewModel;

        public XmlView() {
            InitializeComponent();
            viewModel = new Lazy<XmlViewModel>(() => this.GetViewModel<XmlViewModel>());
        }

        private void GridPreviewKeyDown(object sender, KeyEventArgs e) {
            if ((e.Key == Key.Enter && e.KeyboardDevice.Modifiers != ModifierKeys.None) || e.Key == Key.Tab) {
                e.Handled = true;
                var uie = e.OriginalSource as UIElement;
                if (uie == null) return;
                uie.MoveFocus(e.KeyboardDevice.Modifiers == ModifierKeys.Shift
                                  ? new TraversalRequest(FocusNavigationDirection.Up)
                                  : new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void EditableIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var uie = sender as UIElement;
            if (uie == null || uie.Visibility != Visibility.Visible) return;
            uie.Focus();
            var tb = sender as TextBox;
            if (tb == null) return;
            tb.SelectAll();
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.UndoCommand.Execute(null); }

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.UndoCommand.CanExecute(null); }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.RedoCommand.Execute(null); }

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.RedoCommand.CanExecute(null); }

        private void CutExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.CutNodeCommand.Execute(xmlTree.SelectedItem); }

        private void CutCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.CutNodeCommand.CanExecute(xmlTree.SelectedItem); }

        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.CopyNodeCommand.Execute(xmlTree.SelectedItem); }

        private void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.CopyNodeCommand.CanExecute(xmlTree.SelectedItem); }

        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.PasteNodeCommand.Execute(xmlTree.SelectedItem); }

        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.PasteNodeCommand.CanExecute(xmlTree.SelectedItem); }

        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e) { viewModel.Value.DeleteNodeCommand.Execute(xmlTree.SelectedItem); }

        private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = viewModel.Value.DeleteNodeCommand.CanExecute(xmlTree.SelectedItem); }

    }
}