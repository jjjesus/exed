using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace TreeListControl.Resources
{
    /// <summary>
    /// Try to extract a sensible name for the XML element, e.g. the Name child XmlElement, or alternatively, the first XmlElement that contains name
    /// </summary>
    public class ConvertXmlNodeToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value is XmlNode) ? Utils.GetXmlNodeName((XmlNode)value) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}
