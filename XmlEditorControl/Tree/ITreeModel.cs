using System.Collections;
using System.Xml;

namespace TreeListControl.Tree
{
	public interface ITreeModel
	{
		#region Methods

		/// <summary>
		/// Get list of children of the specified parent
		/// </summary>
		IEnumerable GetChildren(object parent);

		/// <summary>
		/// Returns whether specified parent has any children or not.
		/// </summary>
		bool HasChildren(object parent);

	    bool CanUndo { get; }
	    bool CanRedo { get; }
	    void Undo();
	    void Redo();
	    void CopyNode(XmlNode node);
        void CutNode(XmlNode node, XmlNode parent);
        void PasteNode(XmlNode node);
        bool CanPaste(XmlElement node);

		#endregion Methods

		#region Events

		event XmlNodeChangedEventHandler NodeChanged;

		#endregion Events
	}
}