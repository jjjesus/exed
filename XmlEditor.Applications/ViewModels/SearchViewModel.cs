using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using System.Windows.Input;
using System.Xml;
using TreeListControl.Resources;
using TreeListControl.Tree;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Helpers;
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

        private readonly CommandBindingCollection commandBindings = new CommandBindingCollection();
        public CommandBindingCollection CommandBindings { get { return commandBindings; } }

        /// <summary>
        /// Invokes the found node selected.
        /// </summary>
        /// <param name="e">The <see cref="XmlEditor.Applications.ViewModels.FoundNodeEventArgs"/> instance containing the event data.</param>
        public void InvokeFoundNodeSelected(FoundNodeEventArgs e)
        {
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
        public SearchViewModel(CompositionContainer container, ISearchView view) : base(view)
        {
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += (s, e) =>
            {
                // Recursively add found nodes to the result
                var worker = (BackgroundWorker)s;
                e.Result = SearchNodes((XmlNode)e.Argument, worker, e);
            };
            bw.RunWorkerCompleted += (s, e) =>
            {
                var found = (List<FoundNode>)e.Result;
                if (found == null || found.Count == 0)
                {
                    foundNodes.Add(new FoundNode { Name = "No results found." });
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


            var searchCommandBinding = new CommandBinding(NavigationCommands.Search, SearchExecuted, SearchCanExecuted);
            CommandManager.RegisterClassCommandBinding(typeof(SearchViewModel), searchCommandBinding);
            CommandBindings.Add(searchCommandBinding);

            var searchPreviousCommandBinding = new CommandBinding(NavigationCommands.BrowseBack, SearchPreviousExecuted, SearchCanExecuted);
            CommandManager.RegisterClassCommandBinding(typeof(SearchViewModel), searchPreviousCommandBinding);
            CommandBindings.Add(searchPreviousCommandBinding);

            AddWeakEventListener(foundNodes, FoundNodesCollectionChanged);
        }

        private void FoundNodesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (foundNodes.Count > 0) PublishStatusMessage(string.Format("{0} results found.", foundNodes.Count));            
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
            FoundNodes.Clear();
            var xmlDocument = Document.Content.Document;
            if (searchTerm.Contains("/"))
            {
                XPathQuery(searchTerm, xmlDocument);
                return;
            }
            // Use regular, plain text search
            mySelectedNode = selectedNode;
            selectNextFoundNode = nextTerm;
            if (bw.IsBusy) bw.CancelAsync();
            mySearchTerm = searchTerm.ToLower();
            indexOfSelectedNodeInFoundNodes = -1;
            while (bw.IsBusy)
            {
                Thread.Sleep(50);
                continue;
            }
            bw.RunWorkerAsync(xmlDocument.DocumentElement);
        }

        /// <summary>
        /// Search using an XPath query.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="xmlDocument">The XML document.</param>
        private void XPathQuery(string searchTerm, XmlDocument xmlDocument)
        {
            var mgr = new XmlNamespaceManager(xmlDocument.NameTable);
            var namespaces = Utils.GetNamespaces(xmlDocument);
            foreach (var ns in namespaces) mgr.AddNamespace(ns.Key, ns.Value);
            //var searchUsingNamespace = searchTerm.Replace("/", "/p:");
            var nodes = Document.Content.Document.SelectNodes(searchTerm, mgr);
            if (nodes == null || nodes.Count == 0)
            {
                var noResults = new FoundNode { Name = "No results found." };
                foundNodes.Add(noResults);
                if (namespaces.Count > 0)
                {
                    noResults.Value = "When performing an XPath query, please use a namespace prefix.";
                    if (xmlDocument.DocumentElement != null)
                        foundNodes.Add(new FoundNode { Name = "For example", Value = string.Format("/{0}:{1}", namespaces.Last().Key, xmlDocument.DocumentElement.Name) });
                    foundNodes.Add(new FoundNode());
                    foundNodes.Add(new FoundNode { Name = "Prefix", Value = "Namespace" });
                    foreach (var ns in namespaces) foundNodes.Add(new FoundNode { Name = ns.Key, Value = ns.Value });
                    foundNodes.Last().Name += " (default)";
                }
                return;
            }
            foreach (XmlNode node in nodes)
                FoundNodes.Add(new FoundNode { Name = Utils.GetNodeName(node), Value = Utils.GetNodeValue(node), Tag = node });
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
                                   select new FoundNode { Name = Utils.GetNodeName(attribute), Value = attribute.Value, Tag = attribute });
                }
                // Check element or comment
                if (node.Name.ToLower().Contains(mySearchTerm) || (node.Value != null && node.Value.ToLower().Contains(mySearchTerm)))
                    found.Add(new FoundNode { Name = Utils.GetNodeName(node), Value = Utils.GetNodeValue(node), Tag = node });
                // Check children
                if (node.HasChildNodes) found.AddRange(SearchNodes(node, worker, e));
                // Check if we've already passed the selected node (used when finding the next or previous search hit)
                if (node == mySelectedNode) indexOfSelectedNodeInFoundNodes = found.Count;
            }
            return found;
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

        /// <summary>
        /// Go to next instance.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        public void SearchExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var index = foundNodes.IndexOf(selectedFoundNode) + 1;
            if (index >= foundNodes.Count) index = 0;
            SelectedFoundNode = foundNodes[index];
            e.Handled = true;
        }

        public void SearchCanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (foundNodes != null && foundNodes.Count > 1);
            e.Handled = true;
        }

        /// <summary>
        /// Go to previouses instance.
        /// </summary>
        public void SearchPreviousExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var index = foundNodes.IndexOf(selectedFoundNode) - 1;
            if (index < 0) index = foundNodes.Count - 1;
            SelectedFoundNode = foundNodes[index];
        }

        private static void PublishStatusMessage(string message)
        {
            EventAggregationProvider.Instance.Publish(new StatusMessage(message));
        }
    }
}
