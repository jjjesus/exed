#region

using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;
using TreeListControl.Tree;

#endregion

namespace TreeListControl.Resources
{
    public class ImageConverter : IValueConverter
    {
        public string AttributeImage { get; set; }
        public string ElementImage { get; set; }
        public string CommentImage { get; set; }
        public string ClosedElementImage { get; set; }
        public string DefaultImage { get; set; }
        public string OpenElementImage { get; set; }
        public string TextImage { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var item = value as TreeListItem;
            if (item == null) return DefaultImage;
            var xmlNode = item.Node.Tag as XmlNode;
            if (xmlNode is XmlElement) {
                if (item.Node.HasChildren) return OpenElementImage;
                var el = (XmlElement) xmlNode;
                if (el.HasAttributes) return ClosedElementImage;
                if (el.HasChildNodes) foreach (XmlNode child in el.ChildNodes) if (!child.LocalName.StartsWith("#")) return ClosedElementImage;
                return ElementImage;
            }
            if (xmlNode is XmlAttribute) return AttributeImage;
            if (xmlNode is XmlText) return TextImage;
            if (xmlNode is XmlComment) return CommentImage;
            return DefaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }

        #endregion
    }
}