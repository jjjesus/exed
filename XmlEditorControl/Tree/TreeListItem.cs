using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace TreeListControl.Tree
{
	public class TreeListItem : ListViewItem, INotifyPropertyChanged
	{
		#region Fields

		private TreeNode node;

		#endregion Fields

		#region Public Properties

		public TreeNode Node
		{
			get { return node; }
			internal set {
			    node = value;
			    OnPropertyChanged("Node");
			}
		}

		#endregion Public Properties

		#region Private Methods

		private static void ChangeFocus(TreeNode node)
		{
			var tree = node.Tree;
			if (tree == null) return;
			var item = tree.ItemContainerGenerator.ContainerFromItem(node) as TreeListItem;
			if (item != null) item.Focus();
			else tree.PendingFocusNode = node;
		}

		private static void ExpandAll(TreeNode node)
		{
			node.IsExpanded = true;
			foreach (var child in node.Children) ExpandAll(child);
		}

		private void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion Private Methods

		#region Protected Methods

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Node != null)
			    switch (e.Key) {
			        case Key.Right:
			            e.Handled = true;
			            if (!Node.IsExpanded) {
			                Node.IsExpanded = true;
			                ChangeFocus(Node);
			            }
			            else if (Node.Children.Count > 0) ChangeFocus(Node.Children[0]);
			            break;
			        case Key.Left:
			            e.Handled = true;
			            if (Node.IsExpanded && Node.IsExpandable) {
			                Node.IsExpanded = false;
			                ChangeFocus(Node);
			            }
			            else ChangeFocus(Node.Parent);
			            break;
			        case Key.Subtract:
			            e.Handled = true;
			            Node.IsExpanded = false;
			            ChangeFocus(Node);
			            break;
			        case Key.Add:
			            e.Handled = true;
			            Node.IsExpanded = true;
			            ChangeFocus(Node);
			            break;
			        case Key.Multiply:
			            e.Handled = true;
			            ExpandAll(Node);
			            ChangeFocus(Node);
			            break;
			    }

			if (!e.Handled) base.OnKeyDown(e);
		}

		#endregion Protected Methods

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Events
	}
}