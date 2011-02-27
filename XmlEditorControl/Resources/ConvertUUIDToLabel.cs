using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace TreeListControl.Resources
{
    public class ConvertUUIDToLabel : IValueConverter
    {
        /// <summary>
        /// Converts a value from a Unique User ID to a sensible label. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.
        ///                 </param><param name="targetType">The type of the binding target property.
        ///                 </param><param name="parameter">The converter parameter to use.
        ///                 </param><param name="culture">The culture to use in the converter.
        ///                 </param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is XmlNode)) return null;
            var node = (XmlNode) value;
            var referredNode = Utils.GetReferredNode(node);
            return string.Format("{0} {1}: {2}", Utils.GetXmlNodeName(referredNode.ParentNode), referredNode.ParentNode.Name, node.FirstChild.Value);
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.
        ///                 </param><param name="targetType">The type to convert to.
        ///                 </param><param name="parameter">The converter parameter to use.
        ///                 </param><param name="culture">The culture to use in the converter.
        ///                 </param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
