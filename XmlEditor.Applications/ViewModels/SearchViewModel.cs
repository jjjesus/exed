using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using System.Xml;
using TreeListControl.Resources;
using TreeListControl.Tree;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Views;

namespace XmlEditor.Applications.ViewModels
{
    public class FoundNodeEventArgs : EventArgs
    {
        public XmlNode FoundNode { get; set; }
        public FoundNodeEventArgs(XmlNode foundNode) { FoundNode = foundNode; }
    }

    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, FoundNodeEventArgs e);
    
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SearchViewModel : ViewModel<ISearchView>
    {
        public event ChangedEventHandler FoundNodeSelected;

        /// <summary>
        /// Invokes the found node selected.
        /// </summary>
        /// <param name="e">The <see cref="XmlEditor.Applications.ViewModels.FoundNodeEventArgs"/> instance containing the event data.</param>
        public void InvokeFoundNodeSelected(FoundNodeEventArgs e) {
            var handler = FoundNodeSelected;
            if (handler != null) handler(this, e);
        }

        private readonly BackgroundWorker bw = new BackgroundWorker();
        private readonly ObservableCollection<FoundNode> foundNodes = new ObservableCollection<FoundNode>();
        private int indexOfSelectedNodeInFoundNodes;
        private string mySearchTerm;
        private XmlNode mySelectedNode;
        private FoundNode selectedFoundNode;
        private bool selectNextFoundNode;

        [ImportingConstructor]
        public SearchViewModel(CompositionContainer container, ISearchView view) : base(view) {
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += (s, e) =>
            {
                // Recursively add found nodes to the result
                var worker = (BackgroundWorker)s;
                e.Result = SearchNodes((XmlNode)e.Argument, worker, e);
            };
            bw.RunWorkerCompleted += (s, e) => {
                                         var found = (List<FoundNode>) e.Result;
                                         if (found == null || found.Count == 0) {
                                             foundNodes.Add(new FoundNode{Name = "No results found."});
                                             return;
                                         }
                                         foreach (var foundNode in found) foundNodes.Add(foundNode);
                                         if (foundNodes.Count <= 0) return;
                                         if (selectNextFoundNode)
                                             SelectedFoundNode = indexOfSelectedNodeInFoundNodes + 1 < foundNodes.Count
                                                                     ? foundNodes[++indexOfSelectedNodeInFoundNodes]
                                                                     : foundNodes[0];
                                         else
                                             SelectedFoundNode = indexOfSelectedNodeInFoundNodes >= 0
                                                                     ? foundNodes[indexOfSelectedNodeInFoundNodes]
                                                                     : foundNodes.Last();
                                     };

        }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>The document.</value>
        public MyXmlDocument Document { get; set; }

        /// <summary>
        /// Gets the found nodes.
        /// </summary>
        /// <value>The found nodes.</value>
        public ObservableCollection<FoundNode> FoundNodes { get { return foundNodes; } }

        /// <summary>
        /// Searches the specified search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="selectedNode">The selected node.</param>
        /// <param name="nextTerm">if set to <c>true</c> [next term].</param>
        public void Search(string searchTerm, XmlNode selectedNode, bool nextTerm)
        {
            if (Document == null || Document.Content == null || Document.Content.Document == null) return;
            mySelectedNode = selectedNode;
            selectNextFoundNode = nextTerm;
            if (bw.IsBusy) bw.CancelAsync();
            mySearchTerm = searchTerm;
            indexOfSelectedNodeInFoundNodes = -1;
            FoundNodes.Clear();
            while (bw.IsBusy) {
                Thread.Sleep(50);
                continue;
            }
            bw.RunWorkerAsync(Document.Content.Document.DocumentElement);
        }

        /// <summary>
        /// Searches the nodes recursively.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="worker">The worker.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private List<FoundNode> SearchNodes(XmlNode parent, BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return null;
            }
            var found = new List<FoundNode>();
            foreach (XmlNode node in parent.ChildNodes)
            {
                // Check attributes
                if (node is XmlElement && ((XmlElement)node).HasAttributes)
                {
                    var node1 = node;
                    found.AddRange(from XmlAttribute attribute in node.Attributes
                                   where attribute.Name.ToLower().Contains(mySearchTerm) || (node1.Value != null && node1.Value.ToLower().Contains(mySearchTerm))
                                   select new FoundNode { Name = GetNodeName(attribute), Value = attribute.Value, Tag = attribute });
                }
                // Check element or comment
                if (node.Name.ToLower().Contains(mySearchTerm) || (node.Value != null && node.Value.ToLower().Contains(mySearchTerm))) found.Add(new FoundNode { Name = GetNodeName(node), Value = node.Value, Tag = node });
                // Check children
                if (node.HasChildNodes) found.AddRange(SearchNodes(node, worker, e));
                // Check if we've already passed the selected node (used when finding the next or previous search hit)
                if (node == mySelectedNode) indexOfSelectedNodeInFoundNodes = found.Count;
            }
            return found;
        }

        private static string GetNodeName(XmlNode node) {
            if (!(node is XmlElement)) return node.Name;
            var friendlyName = Utils.GetXmlNodeName(node);
            return (string.IsNullOrEmpty(friendlyName) ?
                node.Name :
                string.Format("{0} {1}", node.Name, friendlyName));
        }

        private static string GetNodeName(XmlAttribute attribute) {
            if (attribute.OwnerElement == null) return attribute.Name;
            var friendlyName = Utils.GetXmlNodeName(attribute.OwnerElement);
            return (string.IsNullOrEmpty(friendlyName) ?  
                string.Format("{0}\\{1}", attribute.OwnerElement.Name, attribute.Name) : 
                string.Format("{0} {1}\\{2}", attribute.OwnerElement.Name, friendlyName, attribute.Name));
        }

        /// <summary>
        /// Gets or sets the selected found node.
        /// </summary>
        /// <value>The selected found node.</value>
        public FoundNode SelectedFoundNode
        {
            get { return selectedFoundNode; }
            set
            {
                selectedFoundNode = value;
                RaisePropertyChanged("SelectedFoundNode");
                if (selectedFoundNode == null) return;
                InvokeFoundNodeSelected(new FoundNodeEventArgs(selectedFoundNode.Tag as XmlNode));
            }
        }
    }
}
