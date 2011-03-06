#region

using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using TreeListControl;
using TreeListControl.Resources;
using TreeListControl.Tree;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Helpers;
using XmlEditor.Applications.Interfaces;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    public class XmlViewModel : ViewModel<IXmlView>, ISearch
    {
        private readonly MyXmlDocument document;
        private readonly XsltViewModel xsltViewModel;
        private readonly SearchViewModel searchViewModel;

        private RelayCommand<TreeNode> copyNodeCommand;
        private RelayCommand<TreeNode> cutNodeCommand;
        private RelayCommand<TreeNode> deleteNodeCommand;
        private RelayCommand<TreeNode> insertCommentCommand;
        private RelayCommand<TreeNode> insertNodeCommand;
        private RelayCommand<XmlMenuItem> insertNodeCommand2;
        private RelayCommand<TreeNode> pasteNodeCommand;
        private RelayCommand undoCommand;
        private RelayCommand redoCommand;

        private TreeNode cutPasteNode;
        private ErrorMessage selectedError;
        private object selectedNode;
        private object selectedTab;

        public XmlViewModel(CompositionContainer container, IXmlView view, MyXmlDocument document) : base(view) {
            this.document = document;

            xsltViewModel = container.GetExportedValue<XsltViewModel>();
            xsltViewModel.Document = document;

            searchViewModel = container.GetExportedValue<SearchViewModel>();
            searchViewModel.Document = document;
            searchViewModel.FoundNodeSelected += SearchViewModelFoundNodeSelected;

            document.Content.NodeChanged += DocumentChanged;
        }

        ~XmlViewModel() {
            XmlModel.Document.NodeChanged -= DocumentChanged;
            searchViewModel.FoundNodeSelected -= SearchViewModelFoundNodeSelected;
        }

        private void SearchViewModelFoundNodeSelected(object sender, FoundNodeEventArgs e)
        {
            if (e.FoundNode != null) SelectedNode = e.FoundNode;
        }

        private void DocumentChanged(object sender, XmlNodeChangedEventArgs e)
        {
            document.Modified = true;
            // When the XSLTView is loaded, transform when the document is changed.
            if (((TabItem)selectedTab).Content == xsltViewModel.View) xsltViewModel.TransformDocument();           
        }

        public XmlModel XmlModel { get { return document.Content; } }

        public object XsltView { get { return xsltViewModel.View; } }
        
        public object SearchView { get { return searchViewModel.View; } }

        public MyXmlDocument Document { get { return document; } }

        public ObservableCollection<ErrorMessage> ErrorMessages { get { return XmlModel.ErrorMessages; } }
        
        #region Commands
        /// <summary>
        ///   Can the document be saved
        /// </summary>
        public bool CanSave { get { return XmlModel != null && XmlModel.CanSave; } }

        public ICommand CopyNodeCommand {
            get {
                return copyNodeCommand ?? (copyNodeCommand = new RelayCommand<TreeNode>(node => {
                                                                                            XmlModel.CopyNode(
                                                                                                node.Tag as XmlNode);
                                                                                            cutPasteNode = node;
                                                                                            // do not allow the xml version or the root element to be used as a copy source
                                                                                        },
                                                                                        node =>
                                                                                        node != null && node.Level > 0));
            }
        }

        public ICommand CutNodeCommand {
            get {
                return cutNodeCommand ?? (cutNodeCommand = new RelayCommand<TreeNode>(node => {
                                                                                          cutPasteNode = node;
                                                                                          XmlModel.CutNode(
                                                                                              node.Tag as XmlNode,
                                                                                              node.Parent.Tag as XmlNode);
                                                                                          //node.Remove();
                                                                                      },
                                                                                      node => node != null && node.Level > 0));
                // do not allow to cut the xml version or the root element
            }
        }

        public ICommand DeleteNodeCommand {
            get {
                return deleteNodeCommand ??
                       (deleteNodeCommand =
                        new RelayCommand<TreeNode>(
                            node => XmlModel.DeleteXmlNode(node.Tag as XmlNode, node.Parent.Tag as XmlNode),
                            node => node != null && node.Level > 0));
                // prevent the xml version and root element from being deleted));
            }
        }

        /// <summary>
        ///   Actually, doesn't insert or do anything: purely a placeholder for the canExecute
        /// </summary>
        public ICommand InsertNodeCommand {
            get {
                return insertNodeCommand ??
                       (insertNodeCommand =
                        new RelayCommand<TreeNode>(node => { },
                                                   node =>
                                                   node != null && node.Tag is XmlElement &&
                                                   Utils.GetPossibleChildren(node.Tag as XmlElement, true).Count > 0));
            }
        }

        public ICommand InsertNodeCommand2 {
            get {
                return insertNodeCommand2 ??
                       (insertNodeCommand2 =
                        new RelayCommand<XmlMenuItem>(item => XmlModel.InsertXmlNode(item.Node, item.Parent)));
            }
        }

        public ICommand InsertCommentCommand {
            get {
                return insertCommentCommand ??
                       (insertCommentCommand =
                        new RelayCommand<TreeNode>(
                            node => XmlModel.InsertComment((XmlNode) node.Tag, (XmlNode) node.Parent.Tag),
                            node => node != null && (node.Tag is XmlElement || node.Tag is XmlAttribute)));
            }
        }

        public RelayCommand UndoCommand { get { return undoCommand ?? (undoCommand = new RelayCommand(() => XmlModel.Undo(), () => XmlModel.CanUndo)); } }

        public ICommand PasteNodeCommand
        {
            get {
                return pasteNodeCommand ??
                       (pasteNodeCommand =
                        new RelayCommand<TreeNode>(node => XmlModel.PasteNode(node.Tag as XmlNode),
                                                   node =>
                                                   cutPasteNode != null && node != null && // node.Index != 0 &&
                                                   node.Tag is XmlElement && XmlModel.CanPaste((XmlElement) node.Tag)));
            }
        }

        public ICommand RedoCommand { get { return redoCommand ?? (redoCommand = new RelayCommand(() => XmlModel.Redo(), () => XmlModel.CanRedo)); } }

        #endregion Commands

        public ErrorMessage SelectedError {
            get { return selectedError; }
            set {
                selectedError = value;
                RaisePropertyChanged("SelectedError");
                if (selectedError == null) return;
                SelectedNode = selectedError.Tag;
            }
        }

        public object SelectedNode {
            get { return selectedNode; }
            set {
                selectedNode = value;
                RaisePropertyChanged("SelectedNode");
            }
        }

        public object SelectedTab {
            get { return selectedTab; } 
            set {
                if (selectedTab == value) return;
                selectedTab = value;
                RaisePropertyChanged("SelectedTab");
            }
        }

        public void Search(string searchTerm, bool nextTerm)
        {
            if (Document == null || Document.Content == null || Document.Content.Document == null || string.IsNullOrEmpty(searchTerm)) return;
            SelectedTab = SearchView;
            searchViewModel.Search(searchTerm.ToLower(), selectedNode as XmlNode, nextTerm);
        }
    }
}