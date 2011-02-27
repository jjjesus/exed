using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;

namespace TreeListControl.Tree
{
	/// <summary>
	/// This articles explores the problems of standard WPF TreeView controls and describes a better way to display hierarchical data using a custom TreeListView control.
	/// <seealso cref="http://www.codeproject.com/KB/WPF/wpf_treelistview_control.aspx"/>
	/// </summary>
	public class TreeList : ListView
	{
		/// <summary>
		/// Model Dependency Property
		/// </summary>
		public static readonly DependencyProperty ModelProperty = 
		    DependencyProperty.Register("Model", typeof(ITreeModel), typeof(TreeList),
		        new FrameworkPropertyMetadata(null, OnModelChanged));

		private readonly TreeNode root;

		public TreeList()
		{
			Rows = new ObservableCollectionAdv<TreeNode>();
			root = new TreeNode(this, null) { IsExpanded = true };
			ItemsSource = Rows;
			ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;

		    SelectionMode = SelectionMode.Single;
            //SelectionChanged += TreeListSelectionChanged;
		}

        //void TreeListSelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    SelectedNode = SelectedItem;
        //}

		internal TreeNode PendingFocusNode
		{
			get; set;
		}

		internal TreeNode Root
		{
			get { return root; }
		}

		/// <summary>
		/// Internal collection of rows representing visible nodes, actually displayed in the ListView
		/// </summary>
		internal ObservableCollectionAdv<TreeNode> Rows
		{
			get; private set;
		}


	    /// <summary>
		/// Gets or sets the Model property.  This dependency property 
		/// indicates ....
		/// </summary>
		public ITreeModel Model
		{
			get { return (ITreeModel)GetValue(ModelProperty); }
			set { SetValue(ModelProperty, value); }
		}

		public ReadOnlyCollection<TreeNode> Nodes
		{
			get { return Root.Nodes; }
		}

        #region SelectedNode

        /// <summary>
        /// SelectedNode Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register("SelectedNode", typeof(object), typeof(TreeList), new PropertyMetadata(null, OnSelectedNodeChanged));

        /// <summary>
        /// Gets or sets the SelectedNode property.  This dependency property 
        /// indicates which tree node has been set.
        /// </summary>
        public object SelectedNode {
            get { return GetValue(SelectedNodeProperty); }
            set { SetValue(SelectedNodeProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SelectedNode property.
        /// </summary>
        private static void OnSelectedNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(e.NewValue is XmlNode)) return;
            ((TreeList)d).Select((XmlNode)e.NewValue);
            ((TreeList)d).OnSelectedNodeChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SelectedNode property.
        /// </summary>
        protected virtual void OnSelectedNodeChanged(DependencyPropertyChangedEventArgs e) { }

        /// <summary>
        /// Select an xml node which may not be currently visible.
        /// </summary>
        /// <remarks>First, we put all parent nodes (or the owner element in case of attributes) on the stack, 
        /// and subsequently we traverse the tree backwards starting from the root</remarks>
        /// <param name="node">The XmlNode to show in the tree</param>
        private void Select(XmlNode node) {
            if (node == null || ((TreeNode) SelectedItem).Tag == node) return;
            var stack = new Stack();
            var parent = (node is XmlAttribute) ? ((XmlAttribute) node).OwnerElement : node.ParentNode;
            while (parent != null && !(parent is XmlDocument)) {
                stack.Push(parent);
                parent = parent.ParentNode;
            }
            root.IsExpanded = true;
            var treeNode = root;
            while (stack.Count > 0) {
                var current = stack.Pop();
                foreach (var child in treeNode.AllVisibleChildren.Where(child => child.Tag.Equals(current))) {
                    child.IsExpanded = true;
                    treeNode = child;
                    break;
                }
            }
            if (node is XmlAttribute || node is XmlElement) {
                foreach (var child in treeNode.AllVisibleChildren.Where(child => child.Tag.Equals(node)))
                {
                    child.IsExpanded = true;
                    treeNode = child;
                    break;
                }
            }
            SelectedItem = treeNode;
            ScrollToTop(treeNode);
        }

        internal static void ExpandAllNodes(TreeNode node) {
            foreach (var visibleChild in node.AllVisibleChildren.Where(child => child.Tag is XmlElement)) {
                visibleChild.IsExpanded = true;
                if (visibleChild.VisibleChildrenCount > 0) ExpandAllNodes(visibleChild);
            }
        }

	    private void ScrollToTop(TreeNode child) {
	        child.IsExpanded = true;
            ScrollIntoView(Items[Items.Count - 1]);
            //UpdateLayout();
            ScrollIntoView(child);
	    }

		public ICollection<TreeNode> SelectedNodes
		{
			get { return SelectedItems.Cast<TreeNode>().ToArray(); }
		}

		private void CreateChildrenRows(TreeNode node)
		{
			var index = Rows.IndexOf(node);
			if (index < 0 && node != root) return;
			var nodes = node.AllVisibleChildren.ToArray();
			Rows.InsertRange(index + 1, nodes);
		}

		private IEnumerable GetChildren(TreeNode parent)
		{
			return Model != null ? Model.GetChildren(parent.Tag) : null;
		}

		private bool HasChildren(TreeNode parent)
		{
			if (parent == Root) return true;
			return Model != null && Model.HasChildren(parent.Tag);
		}

		private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
		{
			if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated || PendingFocusNode == null) return;
			var item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListItem;
			if (item != null) item.Focus();
			PendingFocusNode = null;
		}

		private void ModelNodeChanged(object sender, XmlNodeChangedEventArgs e) {
		    var parent = e.NewParent ?? e.OldParent;
		    foreach (var node in Rows.Where(node => node.Tag.Equals(parent))) {
		        node.Update();
		        break;
		    }
            switch (e.Action)
            {
                case XmlNodeChangedAction.Insert:
                    Select(e.Node);
                    break;
                case XmlNodeChangedAction.Remove:
                    Select(e.NewParent);
                    break;
            }

		}

	    /// <summary>
		/// Handles changes to the Model property.
		/// </summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TreeList)d).OnModelChanged(e);
		}

		#endregion Private Methods

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new TreeListItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is TreeListItem;
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the Model property.
		/// </summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (Model != null) Model.NodeChanged -= ModelNodeChanged;
			Model = (ITreeModel)e.NewValue;
			if (Model != null) Model.NodeChanged += ModelNodeChanged;
			root.Children.Clear();
			Rows.Clear();
			CreateChildrenNodes(root);
			// EV: Expand the first node
			foreach (var child in root.Children) child.IsExpanded = true;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			var ti = element as TreeListItem;
			var node = item as TreeNode;
			if (ti == null || node == null) return;
			ti.Node = node;
			base.PrepareContainerForItemOverride(element, node.Tag);
		}

		internal void CreateChildrenNodes(TreeNode node)
		{
			var children = GetChildren(node);
			if (children == null) return;
			var rowIndex = Rows.IndexOf(node);
			//node.ChildrenSource = children as INotifyCollectionChanged;
			foreach (var obj in children) {
			    var child = new TreeNode(this, obj);
			    child.HasChildren = HasChildren(child);
			    node.Children.Add(child);
			}
			Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
		}

		internal void DropChildrenRows(TreeNode node, bool removeParent)
		{
			var start = Rows.IndexOf(node);
			if (start < 0 && node != root) return;
			var count = node.VisibleChildrenCount;
			if (removeParent) count++;
			else start++;
			Rows.RemoveRange(start, count);
		}

		internal void InsertNewNode(TreeNode parent, object tag, int rowIndex, int index)
		{
			var node = new TreeNode(this, tag);
			if (index >= 0 && index < parent.Children.Count) parent.Children.Insert(index, node);
			else {
			    index = parent.Children.Count;
			    parent.Children.Add(node);
			}
			Rows.Insert(rowIndex + index + 1, node);
            ExpandAllNodes(node);
		}

		internal void SetIsExpanded(TreeNode node, bool value)
		{
			if (value)
			    if (!node.IsExpandedOnce) {
			        node.IsExpandedOnce = true;
			        node.AssignIsExpanded(true);
			        CreateChildrenNodes(node);
			    }
			    else {
			        node.AssignIsExpanded(true);
			        CreateChildrenRows(node);
			    }
			else {
			    DropChildrenRows(node, false);
			    node.AssignIsExpanded(false);
			}
		}

	}
}