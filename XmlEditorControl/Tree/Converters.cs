using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TreeListControl.Tree
{
	internal class CanExpandConverter : IValueConverter
	{
		#region Public Methods

		public object Convert(object o, Type type, object parameter, CultureInfo culture)
		{
			if ((bool)o)
			    return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion Public Methods
	}

	/// <summary>
	/// Convert Level to left margin
	/// </summary>
	internal class LevelToIndentConverter : IValueConverter
	{
		#region Constants

		private const double IndentSize = 19.0;

		#endregion Constants

		#region Public Methods

		public object Convert(object o, Type type, object parameter, CultureInfo culture)
		{
			return new Thickness((int)o * IndentSize, 0, 0, 0);
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion Public Methods
	}
}