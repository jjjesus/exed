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
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    public class XmlViewModel : ViewModel<IXmlView>
    {
        private readonly MyXmlDocument document;
        private readonly XsltViewModel xsltViewModel;
        private RelayCommand<TreeNode> copyNodeCommand;
        private RelayCommand<TreeNode> cutNodeCommand;
        private TreeNode cutPasteNode;

        private RelayCommand<TreeNode> deleteNodeCommand;
        private RelayCommand<TreeNode> insertCommentCommand;
        private RelayCommand<TreeNode> insertNodeCommand;
        private RelayCommand<XmlMenuItem> insertNodeCommand2;
        private RelayCommand<TreeNode> pasteNodeCommand;
        private RelayCommand redoCommand;
        private ErrorMessage selectedError;
        private object selectedNode;
        private RelayCommand undoCommand;

        public XmlViewModel(CompositionContainer container, IXmlView view, MyXmlDocument document) : base(view) {
            this.document = document;

            xsltViewModel = container.GetExportedValue<XsltViewModel>();
            xsltViewModel.Document = document;

            document.Content.NodeChanged += DocumentChanged;
        }

        ~XmlViewModel() {
            XmlDocument.Document.NodeChanged -= DocumentChanged;
        }

        private void DocumentChanged(object sender, XmlNodeChangedEventArgs e)
        {
            document.Modified = true;
            // When the XSLTView is loaded, transform when the document is changed.
            if (((TabItem)selectedTab).Content == xsltViewModel.View) xsltViewModel.TransformDocument();           
        }

        public XmlModel XmlDocument { get { return document.Content; } }

        public object XsltView { get { return xsltViewModel.View; } }

        public MyXmlDocument Document { get { return document; } }

        public ObservableCollection<ErrorMessage> ErrorMessages { get { return XmlDocument.ErrorMessages; } }

        #region Commands
        /// <summary>
        ///   Can the document be saved
        /// </summary>
        public bool CanSave { get { return XmlDocument != null && XmlDocument.CanSave; } }

        public ICommand CopyNodeCommand {
            get {
                return copyNodeCommand ?? (copyNodeCommand = new RelayCommand<TreeNode>(node => {
                                                                                            XmlDocument.CopyNode(
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
                                                                                          XmlDocument.CutNode(
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
                            node => XmlDocument.DeleteXmlNode(node.Tag as XmlNode, node.Parent.Tag as XmlNode),
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
                        new RelayCommand<XmlMenuItem>(item => XmlDocument.InsertXmlNode(item.Node, item.Parent)));
            }
        }

        public ICommand InsertCommentCommand {
            get {
                return insertCommentCommand ??
                       (insertCommentCommand =
                        new RelayCommand<TreeNode>(
                            node => XmlDocument.InsertComment((XmlNode) node.Tag, (XmlNode) node.Parent.Tag),
                            node => node != null && (node.Tag is XmlElement || node.Tag is XmlAttribute)));
            }
        }

        public ICommand UndoCommand { get { return undoCommand ?? (undoCommand = new RelayCommand(() => XmlDocument.Undo(), () => XmlDocument.CanUndo)); } }

        public ICommand PasteNodeCommand
        {
            get {
                return pasteNodeCommand ??
                       (pasteNodeCommand =
                        new RelayCommand<TreeNode>(node => XmlDocument.PasteNode(node.Tag as XmlNode),
                                                   node =>
                                                   cutPasteNode != null && node != null && // node.Index != 0 &&
                                                   node.Tag is XmlElement && XmlDocument.CanPaste((XmlElement) node.Tag)));
            }
        }

        public ICommand RedoCommand { get { return redoCommand ?? (redoCommand = new RelayCommand(() => XmlDocument.Redo(), () => XmlDocument.CanRedo)); } }

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

        private object selectedTab;
        public object SelectedTab {
            get { return selectedTab; } 
            set {
                if (selectedTab == value) return;
                selectedTab = value;
                RaisePropertyChanged("SelectedTab");
            }
        }

    }
}