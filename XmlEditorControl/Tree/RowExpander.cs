using System.Windows;
using System.Windows.Controls;

namespace TreeListControl.Tree
{
	public class RowExpander : Control
	{
		#region Constructors

		static RowExpander()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
		}

		#endregion Constructors
	}
}