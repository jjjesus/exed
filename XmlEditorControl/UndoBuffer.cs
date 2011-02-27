using System;
using System.Collections;
using System.Xml;

namespace TreeListControl
{
	public interface IMemento
	{
		#region Properties

		XmlNodeChangedAction ActionType
		{
			get; set;
		}

		object State
		{
			get; set;
		}

		#endregion Properties
	}

	public class Memento : IMemento
	{
		#region Public Properties

		public XmlNodeChangedAction ActionType
		{
			get; set;
		}

		public object State
		{
			get; set;
		}

		#endregion Public Properties
	}

	// TODO add timestamp, so you can group multiple commands together (e.g. when inserting a set of nodes)
	/// <summary>
	/// A class that implements undo/redo capabilities
	/// </summary>
	/// <seealso cref="http://www.codeproject.com/KB/cs/undoredobuffer.aspx"/>
	public class UndoBuffer
	{
		#region Fields

		protected ArrayList buffer;
		protected int idx;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public UndoBuffer()
		{
			buffer = new ArrayList();
			idx = 0;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Returns true if at the top of the undo buffer.
		/// </summary>
		public bool AtTop
		{
			get { return idx == buffer.Count; }
		}

		/// <summary>
		/// Returns true if the current position in the undo buffer will
		/// allow for redo's.
		/// </summary>
		public bool CanRedo
		{
			// idx+1 because the topmost buffer item is the topmost state
			get { return buffer.Count > idx + 1; }
		}

		/// <summary>
		/// Returns true if there are items in the undo buffer.
		/// </summary>
		public bool CanUndo
		{
			get { return idx > 0; }
		}

		/// <summary>
		/// Returns the count of undo items in the buffer.
		/// </summary>
		public int Count
		{
			get { return buffer.Count; }
		}

		/// <summary>
		/// Sets the last Memento action text.
		/// </summary>
		public XmlNodeChangedAction LastAction
		{
			set {
			    if (idx == 0) throw new UndoRedoException("Invalid index.");
			    ((IMemento)buffer[idx - 1]).ActionType = value;
			}
		}

		/// <summary>
		/// Returns the action text associated with the Memento that holds
		/// the current state. 
		/// </summary>
		public XmlNodeChangedAction RedoAction
		{
			get {
			    return idx < buffer.Count ? ((IMemento)buffer[idx]).ActionType : XmlNodeChangedAction.Change;
			}
		}

		/// <summary>
		/// Returns the action text associated with the Memento that holds
		/// the last known state. 
		/// </summary>
		public XmlNodeChangedAction UndoAction
		{
			get {
			    return idx > 0 ? ((IMemento)buffer[idx - 1]).ActionType : XmlNodeChangedAction.Change;
			}
		}

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Save the current state at the index position. Anything past the
		/// index position is lost.
		/// This means that the "redo" action is no longer possible.
		/// Scenario--The user does 10 things. The user undo's 5 of them, then
		/// does something new.
		/// He can only undo now, he cannot "redo". If he does one 
		/// undo, then he can do one "redo".
		/// </summary>
		/// <param name="mem">The memento holding the current state.</param>
		public void Do(IMemento mem)
		{
			if (buffer.Count > idx) buffer.RemoveRange(idx, buffer.Count - idx);
			buffer.Add(mem);
			++idx;
		}

		/// <summary>
		/// Removes all state information.
		/// </summary>
		public void Flush()
		{
			buffer.Clear();
			idx = 0;
		}

		/// <summary>
		/// Saves the current state. This does not adjust the current undo indexer.
		/// Use this method only when performing an Undo and AtTop==true, so that 
		/// the current state, before the Undo, can be saved, allowing a Redo to 
		/// be applied.
		/// </summary>
		/// <param name="mem"></param>
		public void PushCurrentAction(IMemento mem)
		{
			buffer.Add(mem);
		}

		/// <summary>
		/// Returns the next memento.
		/// </summary>
		public IMemento Redo()
		{
			return (IMemento)buffer[idx++];
		}

		/// <summary>
		/// Returns the current memento.
		/// </summary>
		public IMemento Undo()
		{
			if (idx == 0) throw new UndoRedoException("Invalid index.");
			return (IMemento)buffer[--idx];
		}

		#endregion Public Methods
	}

	public class UndoRedoException : ApplicationException
	{
		#region Constructors

		public UndoRedoException(string msg)
			: base(msg)
		{
		}

		#endregion Constructors
	}
}