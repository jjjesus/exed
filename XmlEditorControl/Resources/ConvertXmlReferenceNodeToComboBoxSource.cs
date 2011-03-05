using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace TreeListControl.Resources
{
    /// <summary>
    /// When we are dealing with a reference node, extract the allowable reference from the schema and export them as a List<string>
    /// </summary>
    public class ConvertXmlReferenceNodeToComboBoxSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is XmlNode)) return null;
            return Utils.GetReferences((XmlNode)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

    }
}