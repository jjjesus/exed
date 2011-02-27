using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;

namespace TreeListControl.Resources
{
	[ValueConversion(typeof(XmlElement), typeof(string))]
	class XmlNodeToAnnotationConverter : IValueConverter
	{
		#region Public Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(string) || !(value is XmlElement)) return string.Empty;

			var node = value as XmlElement;
			return Utils.GetAnnotation(node);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Public Methods
	}
}