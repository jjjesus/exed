using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TreeListControl
{
	public class ObservableCollectionAdv<T> : ObservableCollection<T>
	{
		#region Private Methods

		private void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		private void OnReset()
		{
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion Private Methods

		#region Public Methods

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			CheckReentrancy();
			var items = Items as List<T>;
			if (items != null) items.InsertRange(index, collection);
			OnReset();
		}

		public void RemoveRange(int index, int count)
		{
			CheckReentrancy();
			var items = Items as List<T>;
			if (items != null) items.RemoveRange(index, count);
			OnReset();
		}

		#endregion Public Methods
	}
}