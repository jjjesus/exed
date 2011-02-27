using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;

using TreeListControl.Tree;

namespace TreeListControl.Resources
{
	public class ConvertXmlNodeToListOfChildren : IValueConverter
	{
		#region Public Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TreeNode) || !((value as TreeNode).Tag is XmlElement)) return null;
			var el = (value as TreeNode).Tag as XmlElement;
		    return Utils.GetPossibleChildren(el, true).Select(child => new XmlMenuItem(child, value as TreeNode)).ToList();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Public Methods
	}

	public class XmlMenuItem
	{
		#region Constructors

		/// <summary>
		/// XML Data object for a MenuItem
		/// </summary>
		/// <param name="newNode">XmlSchemaElement of XmlSchemaAttribute</param>
		/// <param name="treeNode">Parent treenode</param>
		public XmlMenuItem(XmlSchemaObject newNode, TreeNode treeNode)
		{
			Node = newNode;
			TreeNode = treeNode;
		}

		#endregion Constructors

		#region Public Properties

		public string Annotation
		{
			get { return Utils.GetAnnotation(Node); }
		}

		public string Name
		{
			get {
			    if (Node is XmlSchemaElement)
			        return (Node as XmlSchemaElement).QualifiedName.Name;
			    if (Node is XmlSchemaAttribute)
			        return (Node as XmlSchemaAttribute).QualifiedName.Name;
			    return string.Empty;
			}
		}

		public XmlSchemaObject Node
		{
			get; set;
		}

		public XmlElement Parent
		{
			get { return TreeNode.Tag as XmlElement; }
		}

		public TreeNode TreeNode
		{
			get; set;
		}

		#endregion Public Properties
	}
}