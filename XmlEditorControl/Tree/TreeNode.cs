#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#endregion

namespace TreeListControl.Tree {
    public sealed class TreeNode : INotifyPropertyChanged {
        #region Fields

        private readonly Collection<TreeNode> children;

        //private INotifyCollectionChanged childrenSource;
        private readonly ReadOnlyCollection<TreeNode> nodes;
        private readonly object tag;
        private readonly TreeList tree;
        private int index = -1;
        private bool isExpanded;
        private bool isSelected;

        #endregion Fields

        #region Constructors

        internal TreeNode(TreeList tree, object tag) {
            if (tree == null) throw new ArgumentNullException("tree");

            this.tree = tree;
            children = new NodeCollection(this);

            nodes = new ReadOnlyCollection<TreeNode>(children);
            this.tag = tag;
        }

        #endregion Constructors

        #region Internal Properties

        internal TreeNode BottomNode {
            get {
                var parent = Parent;
                if (parent != null) return parent.NextNode ?? parent.BottomNode;
                return null;
            }
        }

        internal Collection<TreeNode> Children { get { return children; } }

        /// <summary>
        /// Returns true if all parent nodes of this node are expanded.
        /// </summary>
        internal bool IsVisible {
            get {
                var node = Parent;
                while (node != null) {
                    if (!node.IsExpanded) return false;
                    node = node.Parent;
                }
                return true;
            }
        }

        internal TreeNode NextVisibleNode {
            get {
                if (IsExpanded && Nodes.Count > 0) return Nodes[0];
                var nn = NextNode;
                return nn ?? BottomNode;
            }
        }

        internal TreeList Tree { get { return tree; } }

        #endregion Internal Properties

        #region Public Properties

        public IEnumerable<TreeNode> AllVisibleChildren {
            get {
                var level = Level;
                var node = this;
                while (true) {
                    node = node.NextVisibleNode;
                    if (node != null && node.Level > level) yield return node;
                    else break;
                }
            }
        }

        public bool HasChildren { get; internal set; }

        /*
		        internal INotifyCollectionChanged ChildrenSource {
		            get { return childrenSource; }
		            set {
		                if (childrenSource != null) childrenSource.CollectionChanged -= ChildrenChanged;

		                childrenSource = value;

		                if (childrenSource != null) childrenSource.CollectionChanged += ChildrenChanged;
		            }
		        }
		*/

        /// <summary>
        /// Row index in current INDENT LEVEL
        /// </summary>
        public int Index { get { return index; } }

        public bool IsExpandable { get { return (HasChildren && !IsExpandedOnce) || Nodes.Count > 0; } }

        public bool IsExpanded {
            get { return isExpanded; }
            set {
                if (value == IsExpanded) return;
                Tree.SetIsExpanded(this, value);
                OnPropertyChanged("IsExpanded");
                OnPropertyChanged("IsExpandable");
            }
        }

        public bool IsExpandedOnce { get; internal set; }

        public bool IsSelected {
            get { return isSelected; }
            set {
                if (value == isSelected) return;
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public int Level {
            get {
                if (Parent == null) return -1;
                return Parent.Level + 1;
            }
        }

        public TreeNode NextNode {
            get {
                if (Parent != null) {
                    var i = Index;
                    if (i < Parent.Nodes.Count - 1) return Parent.Nodes[i + 1];
                }
                return null;
            }
        }

        public ReadOnlyCollection<TreeNode> Nodes { get { return nodes; } }

        public TreeNode Parent { get; private set; }

        public TreeNode PreviousNode {
            get {
                if (Parent != null) {
                    var i = Index;
                    if (i > 0) return Parent.Nodes[i - 1];
                }
                return null;
            }
        }

        public object Tag { get { return tag; } }

        public int VisibleChildrenCount { get { return AllVisibleChildren.Count(); } }

        #endregion Public Properties

        #region Private Methods

        private void OnPropertyChanged(string name) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name)); }

        #endregion Private Methods

        #region Internal Methods

        internal void AssignIsExpanded(bool value) { isExpanded = value; }

        #endregion Internal Methods

        #region Public Methods

        //private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e) {
        //    switch (e.Action) {
        //        case NotifyCollectionChangedAction.Add:
        //            if (e.NewItems != null) {
        //                var newStartingIndex = e.NewStartingIndex;
        //                var rowIndex = Tree.Rows.IndexOf(this);
        //                foreach (var obj in e.NewItems) {
        //                    Tree.InsertNewNode(this, obj, rowIndex, newStartingIndex);
        //                    newStartingIndex++;
        //                }
        //            }
        //            break;
        //        case NotifyCollectionChangedAction.Remove:
        //            if (Children.Count > e.OldStartingIndex) RemoveChildAt(e.OldStartingIndex);
        //            break;
        //        case NotifyCollectionChangedAction.Move:
        //        case NotifyCollectionChangedAction.Replace:
        //        case NotifyCollectionChangedAction.Reset:
        //            while (Children.Count > 0) RemoveChildAt(0);
        //            Tree.CreateChildrenNodes(this);
        //            break;
        //    }
        //    HasChildren = Children.Count > 0;
        //    OnPropertyChanged("IsExpandable");
        //}
        //private void RemoveChildAt(int atIndex) {
        //    var child = Children[atIndex];
        //    Tree.DropChildrenRows(child, true);
        //    //ClearChildrenSource(child);
        //    Children.RemoveAt(atIndex);
        //}
        /// <summary>
        /// Remove the current node (including all of its children)
        /// </summary>
        //public void Remove() {
        //    Parent.RemoveChildAt(Parent.Children.IndexOf(this));
        //}
        /// <summary>
        /// Insert the node as the last child
        /// </summary>
        /// <param name="node"></param>
        public void Insert(TreeNode node) {
            Tree.DropChildrenRows(this, false);
            children.Clear();
            Tree.CreateChildrenNodes(this);
        }

        public override string ToString() { return Tag != null ? Tag.ToString() : base.ToString(); }

        public void Update() {
            if (IsExpandedOnce) {
                if (isExpanded) Tree.DropChildrenRows(this, false);
                else isExpanded = true;
                children.Clear();
            }
            else {
                IsExpandedOnce = true;
                AssignIsExpanded(true);
            }
            Tree.CreateChildrenNodes(this);
        }

        #endregion Public Methods

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Other

        //private static void ClearChildrenSource(TreeNode node) {
        //    node.ChildrenSource = null;
        //    foreach (var n in node.Children) ClearChildrenSource(n);
        //}

        private class NodeCollection : Collection<TreeNode> {
            #region Fields

            private readonly TreeNode owner;

            #endregion Fields

            #region Constructors

            public NodeCollection(TreeNode owner) { this.owner = owner; }

            #endregion Constructors

            #region Protected Methods

            protected override void ClearItems() { while (Count != 0) RemoveAt(Count - 1); }

            protected override void InsertItem(int index, TreeNode item) {
                if (item == null) throw new ArgumentNullException("item");

                if (item.Parent == owner) return;
                if (item.Parent != null) item.Parent.Children.Remove(item);
                item.Parent = owner;
                item.index = index;
                for (var i = index; i < Count; i++) this[i].index++;
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index) {
                var item = this[index];
                item.Parent = null;
                item.index = -1;
                for (var i = index + 1; i < Count; i++) this[i].index--;
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, TreeNode item) {
                if (item == null) throw new ArgumentNullException("item");
                RemoveAt(index);
                InsertItem(index, item);
            }

            #endregion Protected Methods
        }

        #endregion Other
    }
}