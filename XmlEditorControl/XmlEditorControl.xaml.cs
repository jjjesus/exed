#region

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using TreeListControl.Tree;

#endregion

namespace TreeListControl
{
    /// <summary>
    ///   Interaction logic for XmlEditorControl.xaml
    /// </summary>
    public partial class XmlEditorControl
    {
        #region Static Fields

        /// <summary>
        ///   XmlModel Dependency Property
        /// </summary>
        public static readonly DependencyProperty XmlModelProperty = DependencyProperty.Register("XmlModel", typeof (ITreeModel), typeof (XmlEditorControl),
                                                                                                 new FrameworkPropertyMetadata(null,
                                                                                                                               new PropertyChangedCallback(
                                                                                                                                   OnXmlModelChanged)));

        #endregion Static Fields

        #region Constructors

        public XmlEditorControl() {
            InitializeComponent();

            //Loaded += XmlEditorControl_Loaded;
        }

        //void XmlEditorControl_Loaded(object sender, RoutedEventArgs e) {
        //    //InputBindings.Add(new KeyBinding(Presenter.UndoCommand, Key.Z, ModifierKeys.Control));
        //    //InputBindings.Add(new KeyBinding(Presenter.RedoCommand, Key.Y, ModifierKeys.Control));
        //}

        #endregion Constructors

        #region Private Properties

        ///// <summary>
        ///// Returns ViewModel
        ///// </summary>
        //XmlEditorControlViewModel Presenter
        //{
        //    get { return (DataContext as XmlEditorControlViewModel); }
        //}

        #endregion Private Properties

        #region Public Properties

        /// <summary>
        ///   Gets or sets the XmlModel property.  This dependency property 
        ///   indicates the XML document that should be displayed and loaded.
        /// </summary>
        public ITreeModel XmlModel { get { return (ITreeModel) GetValue(XmlModelProperty); } set { SetValue(XmlModelProperty, value); } }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        ///   Handles changes to the XmlModel property.
        /// </summary>
        private static void OnXmlModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { ((XmlEditorControl) d).OnXmlModelChanged(e); }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        ///   Provides derived classes an opportunity to handle changes to the XmlModel property.
        /// </summary>
        protected virtual void OnXmlModelChanged(DependencyPropertyChangedEventArgs e) { xmlTree.Model = (ITreeModel) e.NewValue; }

        #endregion Protected Methods

        #region Public Methods

        public void Save() { throw new NotImplementedException(); }

        #endregion Public Methods

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = XmlModel.CanUndo; }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e) { XmlModel.Undo(); }

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = XmlModel.CanRedo; }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e) { XmlModel.Redo(); }

        private void CutCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            e.CanExecute = node != null && node.Level > 0;
        }

        private void CutExecuted(object sender, ExecutedRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            if (node == null) return;
            XmlModel.CutNode(node.Tag as XmlNode, node.Parent.Tag as XmlNode);
        }

        private void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            e.CanExecute = node != null && node.Level > 0;
        }

        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            if (node == null) return;
            XmlModel.CopyNode(node.Tag as XmlNode);
        }

        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            e.CanExecute = node != null && node.Index != 0 && node.Tag is XmlElement && XmlModel.CanPaste((XmlElement) node.Tag);
        }

        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e) {
            var node = xmlTree.SelectedItem as TreeNode;
            if (node == null) return;
            XmlModel.PasteNode(node.Tag as XmlNode);
        }

        private void GridPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter && e.Key != Key.Tab) return;
            e.Handled = true;
            var uie = e.OriginalSource as UIElement;
            if (uie == null) return;
            uie.MoveFocus(e.KeyboardDevice.Modifiers == ModifierKeys.Shift
                              ? new TraversalRequest(FocusNavigationDirection.Up)
                              : new TraversalRequest(FocusNavigationDirection.Down));
        }

        private void EditableIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var uie = sender as UIElement;
            if (uie == null || uie.Visibility != Visibility.Visible) return;
            uie.Focus();
            var tb = sender as TextBox;
            if (tb == null) return;
            tb.SelectAll();
        }

        #region Depency Properties

        #region GridSplitterHeight

        /// <summary>
        ///   GridSplitterHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty GridSplitterHeightProperty = DependencyProperty.Register("GridSplitterHeight", typeof (decimal),
                                                                                                           typeof (XmlEditorControl),
                                                                                                           new FrameworkPropertyMetadata((decimal) 0,
                                                                                                                                         new PropertyChangedCallback(
                                                                                                                                             OnGridSplitterHeightChanged)));

        /// <summary>
        ///   Gets or sets the GridSplitterHeight property.  This dependency property 
        ///   indicates ....
        /// </summary>
        public decimal GridSplitterHeight { get { return (decimal) GetValue(GridSplitterHeightProperty); } set { SetValue(GridSplitterHeightProperty, value); } }

        /// <summary>
        ///   Handles changes to the GridSplitterHeight property.
        /// </summary>
        private static void OnGridSplitterHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as XmlEditorControl;
            if (control == null) return;
            var view = control.FindName("altViewHeight") as RowDefinition;
            if (view == null) return;
            try {
                view.Height = new GridLength(Convert.ToDouble(e.NewValue));
            }
            catch (Exception ex) {
                view.Height = new GridLength(200);
            }
            ((XmlEditorControl) d).OnGridSplitterHeightChanged(e);
        }

        /// <summary>
        ///   Provides derived classes an opportunity to handle changes to the GridSplitterHeight property.
        /// </summary>
        protected virtual void OnGridSplitterHeightChanged(DependencyPropertyChangedEventArgs e) { }

        #endregion

        #region XsltView

        /// <summary>
        ///   XsltView Dependency Property
        /// </summary>
        public static readonly DependencyProperty XsltViewProperty = DependencyProperty.Register("XsltView", typeof (UIElement), typeof (XmlEditorControl),
                                                                                                 new FrameworkPropertyMetadata(null,
                                                                                                                               new PropertyChangedCallback(
                                                                                                                                   OnXsltViewChanged)));

        /// <summary>
        ///   Gets or sets the XsltView property.  This dependency property 
        ///   indicates ....
        /// </summary>
        public UIElement XsltView { get { return (UIElement) GetValue(XsltViewProperty); } set { SetValue(XsltViewProperty, value); } }

        /// <summary>
        ///   Handles changes to the XsltView property.
        /// </summary>
        private static void OnXsltViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as XmlEditorControl;
            if (control == null) return;
            var view = control.FindName("view") as TabItem;
            if (view == null) return;
            view.Content = e.NewValue;
            view.Visibility = Visibility.Visible;

            ((XmlEditorControl) d).OnXsltViewChanged(e);
        }

        /// <summary>
        ///   Provides derived classes an opportunity to handle changes to the XsltView property.
        /// </summary>
        protected virtual void OnXsltViewChanged(DependencyPropertyChangedEventArgs e) { }

        #endregion

        #endregion
    }
}