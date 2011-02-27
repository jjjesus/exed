#region

using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

#endregion

namespace TreeListControl.Resources {
    public class CustomDataTemplateSelector : DataTemplateSelector {
        public DataTemplate ColorTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }
        public DataTemplate ComboBoxReferenceTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }
        public DataTemplate TimeTemplate { get; set; }
        public DataTemplate DefaultAttributeTemplate { get; set; }
        public DataTemplate DefaultElementTemplate { get; set; }
        public DataTemplate TextBlockTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is XmlNode) {
                var element = (XmlNode) item;
                if (Utils.IsReferenceType(element)) return ComboBoxReferenceTemplate;
                if (element.SchemaInfo.SchemaType is XmlSchemaSimpleType) {
                    switch (element.SchemaInfo.SchemaType.Datatype.TypeCode) {
                        case XmlTypeCode.Time:
                            return TimeTemplate;
                        case XmlTypeCode.Date:
                            return DateTemplate;
                    }
                    var simple = (XmlSchemaSimpleType)element.SchemaInfo.SchemaType;
                    if (simple.Content is XmlSchemaSimpleTypeRestriction) //var restriction = (XmlSchemaSimpleTypeRestriction) simple.Content;
                        if (Utils.HasMultipleValues(simple)) return ComboBoxTemplate;
                }
                else if (element.SchemaInfo.SchemaType is XmlSchemaComplexType && 
                    element.SchemaInfo.SchemaType.BaseXmlSchemaType!=null && 
                    element.SchemaInfo.SchemaType.BaseXmlSchemaType.BaseXmlSchemaType == null) return TextBlockTemplate;
            }
            if (item is XmlAttribute || item is XmlDeclaration) return DefaultAttributeTemplate;
            return DefaultElementTemplate;
        }
    }
}