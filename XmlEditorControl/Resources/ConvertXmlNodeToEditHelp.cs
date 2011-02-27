using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;

namespace TreeListControl.Resources
{
    public class ConvertXmlNodeToEditHelp : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || ((XmlNode)value).SchemaInfo == null || ((XmlNode)value).SchemaInfo.SchemaType == null) return string.Empty;
            var schemaType = ((XmlNode)value).SchemaInfo.SchemaType;
            var annotation = Utils.GetAnnotation((XmlNode)value);
            var inputRestrictions = Utils.GetInputRestrictions(schemaType);
            if (!string.IsNullOrEmpty(inputRestrictions)) {
                inputRestrictions = "Input restriction: " + inputRestrictions;
                return string.IsNullOrEmpty(annotation) ? inputRestrictions : annotation + Environment.NewLine + inputRestrictions;
            }
            return string.IsNullOrEmpty(annotation)? null : annotation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}
