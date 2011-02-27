using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace TreeListControl.Resources
{
	/// <summary>
	/// When a schema is specified, extract the allowable values from the schema and export them as a List<string>
	/// </summary>
	class ConvertXmlNodeToComboBoxSource : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is XmlNode)) return null;
			return Utils.GetPossibleValues((XmlNode) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/*
		/// <summary>
		/// Generic Enum to List<T> converter 
		/// </summary>
		/// <typeparam name="T">Enumeration type</typeparam>
		/// <returns>List of type T</returns>
		/// <example>return EnumToList<UNITS>().ConvertAll(x => x.ToString());</example>
		/// <seealso cref="http://devlicio.us/blogs/joe_niland/archive/2006/10/10/Generic-Enum-to-List_3C00_T_3E00_-converter.aspx"/>
		public static List<T> EnumToList<T>() {
		    var enumType = typeof(T);

		    // Can't use type constraints on value types, so have to do check like this
		    if (enumType.BaseType != typeof(Enum))
		        throw new ArgumentException("T must be of type System.Enum");

		    var enumValArray = Enum.GetValues(enumType);
		    var enumValList = new List<T>(enumValArray.Length);
		    foreach (int val in enumValArray) enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
		    return enumValList;
		}
		*/
	}
}