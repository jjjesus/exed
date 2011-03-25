#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

#endregion

namespace TreeListControl.Resources
{
    public static class ExtensionMethod
    {
        #region Public Methods

        public static string CapitalizeAll(this string sString)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sString);
        }

        // apply this extension to any generic IEnumerable object.
        public static string ToString<T>(this IEnumerable<T> source, string separator) where T : class {
            if (source == null) throw new ArgumentException("source can not be null.");

            if (string.IsNullOrEmpty(separator)) throw new ArgumentException("separator can not be null or empty.");

            // A LINQ query to call ToString on each elements
            // and constructs a string array.
            var array = (from s in source where s != null select s.ToString()).ToArray();

            // utilise builtin string.Join to concate elements with
            // customizable separator.
            return string.Join(separator, array);
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Class with help functionality
    /// </summary>
    public static class Utils
    {
        private const int MaxNmbrOfItemsInCombobox = 100;

        /*
         * Original version, replaced with new version below.
                public static string GetAnnotation2(XmlNode node) {
                    if (node.SchemaInfo == null || node.SchemaInfo.SchemaElement == null ||
                        node.SchemaInfo.SchemaElement.Annotation == null ||
                        node.SchemaInfo.SchemaElement.Annotation.Items == null ||
                        node.SchemaInfo.SchemaElement.Annotation.Items.Count == 0 ||
                        node.SchemaInfo.SchemaElement.Annotation.Items[0] == null) return string.Empty;
                    var items = (XmlSchemaDocumentation)node.SchemaInfo.SchemaElement.Annotation.Items[0];
                    return ((items != null) && items.Markup.Length > 0) ? items.Markup[0].Value : string.Empty;
                }
        */

        /// <summary>
        /// Gets the annotation.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetAnnotation(XmlNode node) {
            XmlSchemaAnnotated xmlSchemaAnnotated;
            var schemaInfo = node.SchemaInfo;
            if (node is XmlAttribute) xmlSchemaAnnotated = schemaInfo.SchemaAttribute;
            else
                xmlSchemaAnnotated = node is XmlElement
                                         ? (XmlSchemaAnnotated) schemaInfo.SchemaElement
                                         : schemaInfo.SchemaType;
            if (xmlSchemaAnnotated != null && xmlSchemaAnnotated.Annotation != null &&
                xmlSchemaAnnotated.Annotation.Items != null && xmlSchemaAnnotated.Annotation.Items.Count > 0) {
                var sb = new StringBuilder();
                foreach (var item in xmlSchemaAnnotated.Annotation.Items) {
                    if (!(item is XmlSchemaDocumentation)) continue;
                    var xmlSchemaDoc = (XmlSchemaDocumentation) item;
                    if (xmlSchemaDoc.Markup.Length > 0) sb.Append(xmlSchemaDoc.Markup[0].Value.Replace("\t", string.Empty).Trim());
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the annotation.
        /// </summary>
        /// <param name="elem">The elem.</param>
        /// <returns></returns>
        public static string GetAnnotation(XmlSchemaObject elem) {
            try {
                var annot = elem is XmlSchemaElement ?
                    ((XmlSchemaElement) (elem)).ElementSchemaType :
                    ((XmlSchemaAttribute) (elem)).AttributeSchemaType as XmlSchemaAnnotated;
                if (annot != null && annot.Annotation != null && annot.Annotation.Items != null &&
                    annot.Annotation.Items.Count > 0) foreach (XmlSchemaDocumentation item in annot.Annotation.Items) if (item.Markup.Length > 0) return item.Markup[0].InnerText;
                return string.Empty;
            }
            catch(SystemException) {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get all the children that are XmlAttributes
        /// </summary>
        /// <param name="node">Parent</param>
        /// <returns>List of child XmlAttributes</returns>
        public static List<XmlSchemaObject> GetChildAttributes(XmlElement node)
        {
            var children = new List<XmlSchemaObject>();
            if (!(node.SchemaInfo.SchemaType is XmlSchemaComplexType)) return children;
            var complex = (XmlSchemaComplexType)node.SchemaInfo.SchemaType;
            foreach (var attribute in complex.AttributeUses)
            {
                if (!(attribute is DictionaryEntry)) continue;
                var attr = (XmlSchemaAttribute)((DictionaryEntry)attribute).Value;
                children.Add(attr);
            }
            return children;
        }

        /// <summary>
        /// Get all the children that are XmlElements
        /// </summary>
        /// <param name="node">Parent</param>
        /// <returns>List of child XmlElements</returns>
        public static List<XmlSchemaObject> GetChildElements(XmlElement node)
        {
            var children = new List<XmlSchemaObject>();
            if (!(node.SchemaInfo.SchemaType is XmlSchemaComplexType)) return children;
            var complex = (XmlSchemaComplexType)node.SchemaInfo.SchemaType;
            if (complex.ContentTypeParticle is XmlSchemaSequence)
                GetChildElementsInXmlSchemaSequence(children, complex.ContentTypeParticle as XmlSchemaSequence);
            if (complex.ContentTypeParticle is XmlSchemaAll) children.AddRange(((XmlSchemaAll) complex.ContentTypeParticle).Items.Cast<XmlSchemaElement>());
            //foreach (XmlSchemaElement element in ((XmlSchemaAll)complex.ContentTypeParticle).Items)
                //    children.Add(element);
            return children;
        }

        private static void GetChildElementsInXmlSchemaSequence(ICollection<XmlSchemaObject> children, XmlSchemaGroupBase xmlSchemaSequence)
        {
            foreach (var element in xmlSchemaSequence.Items)
            {
                if (element is XmlSchemaElement) children.Add(element);
                if (element is XmlSchemaSequence) GetChildElementsInXmlSchemaSequence(children, element as XmlSchemaSequence);
            }
        }

        /// <summary>
        /// Gets the default value.
        /// Note: this function may return null when no default value is found. However, 
        /// when assigning an XmlElement's InnerText with null, the #text node is created (and ChildNodes.Count changes from 0 to 1). 
        /// This often leads to invalid content!
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetDefaultValue(XmlSchemaObject node)
        {
            string defaultValue = null;
            var schemaType = new XmlSchemaType();
            if (node is XmlSchemaElement)
            {
                var elem = node as XmlSchemaElement;
                schemaType = elem.ElementSchemaType;
                if (!string.IsNullOrEmpty(elem.DefaultValue)) return elem.DefaultValue;
                if (schemaType is XmlSchemaComplexType) return defaultValue;
            }
            else if (node is XmlSchemaAttribute)
            {
                var attr = node as XmlSchemaAttribute;
                //if (attr.AttributeType == Dat)
                schemaType = attr.AttributeSchemaType;
                if (schemaType.Datatype != null && 
                    schemaType.Datatype.ValueType.FullName != null && 
                    schemaType.Datatype.ValueType.FullName.Equals("System.DateTime")) return DateTime.Now.ToString("yyyy-MM-dd");
                schemaType = attr.AttributeSchemaType.BaseXmlSchemaType;
            }
            if (schemaType is XmlSchemaSimpleType)
            {
                var possibleValues = GetPossibleValues((XmlSchemaSimpleType)schemaType);
                if (possibleValues.Count > 0) return possibleValues[0];
            }
            return defaultValue;
        }

        /// <summary>
        /// Get all the possible children that still can be added of a node, i.e. return all the possible elements and attributes
        /// </summary>
        /// <param name="node">Type to query</param>
        /// <param name="listSchemaChoices">If true, will list the XmlSchemaChoice elements too</param>
        /// <returns>return all the possible elements and attributes</returns>
        public static List<XmlSchemaObject> GetPossibleChildren(XmlElement node, bool listSchemaChoices)
        {
            var children = new List<XmlSchemaObject>();
            if (node.SchemaInfo.SchemaType is XmlSchemaComplexType)
            {
                var complex = (XmlSchemaComplexType)node.SchemaInfo.SchemaType;
                foreach (var attribute in complex.AttributeUses)
                {
                    if (!(attribute is DictionaryEntry)) continue;
                    var attr = (XmlSchemaAttribute)((DictionaryEntry)attribute).Value;
                    var attrFound = node.Attributes.Cast<XmlAttribute>().Any(child => child.Name.Equals(attr.QualifiedName.Name));
                    if (!attrFound) children.Add(attr);
                }
                if (complex.ContentTypeParticle is XmlSchemaSequence)
                    GetChildrenInXmlSchemaSequence(node, complex.ContentTypeParticle as XmlSchemaSequence, children);
                if (complex.ContentTypeParticle is XmlSchemaAll)
                    GetChildrenInXmlSchemaAll(node, (XmlSchemaAll)complex.ContentTypeParticle, children);
                if (listSchemaChoices && complex.ContentTypeParticle is XmlSchemaChoice && (node.ChildNodes.Count == 0 ||
                    (node.ChildNodes.Count == 1 && !(node.ChildNodes[0] is XmlElement)))) children.AddRange(((XmlSchemaChoice)complex.ContentTypeParticle).Items.Cast<XmlSchemaElement>());
            }
            return children;
        }

        private static void GetChildrenInXmlSchemaAll(XmlNode node, XmlSchemaGroupBase xmlSchemaAll, ICollection<XmlSchemaObject> children)
        {
            foreach (XmlSchemaElement element in xmlSchemaAll.Items)
            {
                var max = element.MaxOccurs;
                foreach (var child in node.ChildNodes)
                {
                    if (!(child is XmlElement)) continue;
                    var existingElement = child as XmlElement;
                    if (!existingElement.Name.Equals(element.QualifiedName.Name)) continue;
                    if (--max > 0) continue;
                    break;
                }
                if (max > 0) children.Add(element);
            }
        }

        private static void GetChildrenInXmlSchemaSequence(XmlNode node, XmlSchemaGroupBase xmlSchemaSequence, ICollection<XmlSchemaObject> children)
        {
            foreach (var element in xmlSchemaSequence.Items)
            {
                if (element is XmlSchemaElement) {
                    var xmlSchemaElement = (element as XmlSchemaElement);
                    var max = Math.Max(xmlSchemaElement.MaxOccurs, xmlSchemaSequence.MaxOccurs);
                    foreach (var child in node.ChildNodes)
                    {
                        if (!(child is XmlElement)) continue;
                        var existingElement = child as XmlElement;
                        if (!existingElement.Name.Equals(xmlSchemaElement.QualifiedName.Name)) continue;
                        if (--max > 0) continue;
                        break;
                    }
                    if (max > 0) children.Add(element);
                }
                if (element is XmlSchemaSequence) GetChildrenInXmlSchemaSequence(node, element as XmlSchemaSequence, children);
            }
        }

        /// <summary>
        /// Determines whether we are dealing with a type that refers to another value somewhere in the schema, e.g. a UUIDRef or IDRef refering to a UUID or ID.
        /// </summary>
        /// <param name="node">current node</param>
        /// <returns>
        /// 	<c>true</c> if is reference type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReferenceType(XmlNode node)
        {
            if (!(node.SchemaInfo.SchemaType is XmlSchemaSimpleType)) return false;
            var simple = (XmlSchemaSimpleType)node.SchemaInfo.SchemaType;
            return !string.IsNullOrEmpty(simple.Name) && simple.Name.ToLower().Contains("ref");
        }

        public static List<XmlNode> GetReferences(XmlNode node)
        {
            var simple = (XmlSchemaSimpleType)node.SchemaInfo.SchemaType;
            // we are dealing with a special case, i.e. a reference to something else
            if (string.IsNullOrEmpty(simple.Name) || !simple.Name.ToLower().Contains("ref")) return null;
            if (node.OwnerDocument == null) return null;
            var references = GetXmlNodesOfType(node.OwnerDocument.DocumentElement, GetReferencedName(simple));
            var referredNode = GetReferredNode(node);
            references.Remove(referredNode);
            references.Insert(0, referredNode);
            return references;
        }

        private static string GetReferencedName(XmlSchemaType simple)
        {
            if (simple == null) throw new ArgumentNullException("simple");
            return simple.Name.ToLower().Contains("reference")
                       ? simple.Name.ToLower().Replace("reference", "")
                       : simple.Name.ToLower().Replace("ref", "");
        }

        private static List<XmlNode> GetXmlNodesOfType(XmlNode node, string typeName)
        {
            var nodes = new List<XmlNode>();
            if (node != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.SchemaInfo.SchemaType != null && child.SchemaInfo.SchemaType.Name != null &&
                        child.SchemaInfo.SchemaType.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) nodes.Add(child);
                    if (child.HasChildNodes) nodes.AddRange(GetXmlNodesOfType(child, typeName));
                }
            }
            return nodes;
        }

        /// <summary>
        /// Get the XmlNode, of a certain type, with a certain value
        /// </summary>
        /// <param name="node"></param>
        /// <param name="typeName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static XmlNode GetXmlNodeOfTypeWithValue(XmlNode node, string typeName, string value)
        {
            if (node != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.SchemaInfo.SchemaType != null && child.SchemaInfo.SchemaType.Name != null &&
                        child.SchemaInfo.SchemaType.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                        if (child.FirstChild != null && child.FirstChild.Value != null && child.FirstChild.Value.Equals(value))
                            return child;
                    if (!child.HasChildNodes) continue;
                    var found = GetXmlNodeOfTypeWithValue(child, typeName, value);
                    if (found != null) return found;
                }
            }
            return null;
        }

        ///// <summary>
        ///// Find the XmlNode with a certain value
        ///// </summary>
        ///// <param name="node"></param>
        ///// <param name="value"></param>
        ///// <returns>Returns the first node that contains the value. Should probably build a list using yield</returns>
        //private static XmlNode FindXmlNodesWithValue(XmlNode node, string value)
        //{
        //    if (node != null)
        //    {
        //        value = value.ToLower();
        //        if (node.Attributes != null) if (node.Attributes.Cast<XmlAttribute>().Any(attribute => attribute.Value.ToLower().Contains(value))) return node;
        //        foreach (XmlNode child in node.ChildNodes)
        //        {
        //            if (child.FirstChild != null && child.FirstChild.Value != null && child.FirstChild.Value.ToLower().Contains(value)) return child;
        //            if (!child.HasChildNodes) continue;
        //            var found = FindXmlNodesWithValue(child, value);
        //            if (found != null) return found;
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Returns true when the node contains a name
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if the specified node has name; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasName(XmlNode node)
        {
            return node.Name.ToLower().Contains("name");
        }

        /// <summary>
        /// Gets the name of the XML node. In this case, we are not refering to the XmlNode name, but any child element that contains 'name'
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Name of the node</returns>
        public static string GetXmlNodeName(XmlNode node)
        {
            if (node.Attributes != null) foreach (XmlAttribute attribute in node.Attributes) if (attribute.Name.ToLower().Contains("name")) return attribute.Value.Replace(Environment.NewLine, " - ");
            foreach (XmlNode child in node.ChildNodes) if (child.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && child.FirstChild != null) return child.FirstChild.Value.Replace(Environment.NewLine, " - ");
            foreach (XmlNode child in node.ChildNodes) if (child.Name.ToLower().Contains("name") && child.FirstChild != null) return child.FirstChild.Value.Replace(Environment.NewLine, " - ");
            foreach (XmlNode child in node.ChildNodes) if (child.Name.Equals("title", StringComparison.OrdinalIgnoreCase) && child.FirstChild != null) return child.FirstChild.Value.Replace(Environment.NewLine, " - ");
            //foreach (XmlAttribute child in node.Attributes)
            //    if (child.Name.ToLower().Contains("name"))
            //        return child.Value;
            return string.Empty;
        }

        /// <summary>
        /// When an XmlNode has restrictions specified in its XSD, convert these restrictions to a list of allowable values.
        /// </summary>
        /// <remarks>Currently, only support for enums, min/max, GUID pattern and booleans are supported</remarks>
        /// <param name="node">XmlNode that needs to be analysed</param>
        /// <returns>List of allowable values (may be empty)</returns>
        public static List<string> GetPossibleValues(XmlNode node)
        {
            var simple = (XmlSchemaSimpleType)node.SchemaInfo.SchemaType;

            return GetPossibleValues(simple);
        }

        /// <summary>
        /// Instead of computing all possible values, only check whether there is more than one, and less than MaxNmbrOfItemsInCombobox.
        /// </summary>
        /// <param name="simple">XmlSchemaSimpleType that needs to be analysed</param>
        /// <returns>True if there is more than one possible value</returns>
        public static bool HasMultipleValues(XmlSchemaSimpleType simple)
        {
            var restriction = (XmlSchemaSimpleTypeRestriction)simple.Content;
            if (restriction == null) return false;
            if (simple.Datatype.ValueType.Name.Equals("Boolean") || restriction.BaseTypeName.Name.Equals("boolean")) return true;
            double min = 0, max = 0;
            bool minSet = false, maxSet = false;
            foreach (var facet in restriction.Facets)
            {
                if (facet is XmlSchemaEnumerationFacet) return true;
                if (facet is XmlSchemaMinInclusiveFacet)
                {
                    // Allowable values are restricted between min and max: if there are too many, spit it.
                    min = double.Parse(((XmlSchemaMinInclusiveFacet)facet).Value);
                    minSet = true;
                    continue;
                }
                if (facet is XmlSchemaMaxInclusiveFacet)
                {
                    max = double.Parse(((XmlSchemaMaxInclusiveFacet)facet).Value);
                    maxSet = true;
                    continue;
                }
            }
            return minSet && maxSet && max - min <= MaxNmbrOfItemsInCombobox;
        }

        /// <summary>
        /// When an XmlNode has restrictions specified in its XSD, convert these restrictions to a list of allowable values.
        /// </summary>
        /// <remarks>Currently, only support for enums, min/max, GUID pattern and booleans are supported</remarks>
        /// <param name="simple">XmlSchemaSimpleType that needs to be analysed</param>
        /// <returns>List of allowable values (may be empty)</returns>
        public static List<string> GetPossibleValues(XmlSchemaSimpleType simple)
        {
            var output = new List<string>();
            var restriction = (XmlSchemaSimpleTypeRestriction)simple.Content;
            if (restriction == null) return output;
            if (simple.Datatype.ValueType.Name.Equals("Boolean") || restriction.BaseTypeName.Name.Equals("boolean"))
            {
                output.Add("false");
                output.Add("true");
                return output;
            }
            double min = 0, max = 0;
            bool minSet = false, maxSet = false;
            foreach (var facet in restriction.Facets)
            {
                if (facet is XmlSchemaEnumerationFacet)
                {
                    output.Add(((XmlSchemaEnumerationFacet)facet).Value);
                    continue;
                }
                if (facet is XmlSchemaPatternFacet)
                {
                    var pattern = ((XmlSchemaPatternFacet)facet).Value;
                    if (pattern.Equals("[0-9a-z]{8}\\-[0-9a-z]{4}\\-[0-9a-z]{4}\\-[0-9a-z]{4}\\-[0-9a-z]{12}", StringComparison.OrdinalIgnoreCase))
                        output.Add(Guid.NewGuid().ToString());
                    // Add GUID references?
                    continue;
                }
                if (facet is XmlSchemaMinInclusiveFacet)
                {
                    // Allowable values are restricted between min and max: if there are too many, spit it.
                    min = double.Parse(((XmlSchemaMinInclusiveFacet)facet).Value);
                    minSet = true;
                    continue;
                }
                if (facet is XmlSchemaMaxInclusiveFacet)
                {
                    max = double.Parse(((XmlSchemaMaxInclusiveFacet)facet).Value);
                    maxSet = true;
                    continue;
                }
            }
            if (minSet && maxSet)
            {
                if (max - min <= MaxNmbrOfItemsInCombobox) for (var i = min; i <= max; i++) output.Add(i.ToString());
                else output.Add(min.ToString());
            }

            return output;
        }

        /// <summary>
        /// Schema contains root element, i.e., we can use it to generate a new XML document.
        /// </summary>
        /// <param name="schemaFile">The schema file.</param>
        /// <returns>True if we can use the schema to generate a new XML document</returns>
        public static bool SchemaSetContainsRootElement(string schemaFile)
        {
            var schemaSet = new XmlSchemaSet();
            try
            {
                schemaSet.Add(null, schemaFile);
                schemaSet.Compile();
                if (!schemaSet.IsCompiled) return false;
                var rootElement = XmlQualifiedName.Empty;
                var schemaElem = schemaSet.GlobalElements[rootElement] as XmlSchemaElement;
                if (schemaElem == null) //If element by name is not found, Get first non-abstract root element
                    foreach (XmlSchemaElement elem1 in schemaSet.GlobalElements.Values)
                    {
                        if (elem1.IsAbstract) continue;
                        schemaElem = elem1;
                        break;
                    }
                return (schemaElem != null);
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        /// <summary>
        /// Gets help information to aid editing.
        /// </summary>
        /// <param name="schemaType">Type of the schema.</param>
        /// <returns>Help info</returns>
        public static string GetInputRestrictions(XmlSchemaType schemaType)
        {
            var minInclusive = string.Empty;
            var maxInclusive = string.Empty;
            var minExclusive = string.Empty;
            var maxExclusive = string.Empty;
            var lengthRestriction = string.Empty;
            var minLengthRestriction = string.Empty;
            var maxLengthRestriction = string.Empty;
            var fractionDigits = string.Empty;
            var totalDigits = string.Empty;
            var pattern = string.Empty;
            XmlSchemaObjectCollection facets = null;
            if (schemaType is XmlSchemaSimpleType)
            {
                var content = (XmlSchemaSimpleTypeRestriction)((XmlSchemaSimpleType)schemaType).Content;
                if (content != null) facets = content.Facets;
            }
            else if (schemaType.BaseXmlSchemaType is XmlSchemaSimpleType)
                facets = ((XmlSchemaSimpleTypeRestriction)((XmlSchemaSimpleType)schemaType.BaseXmlSchemaType).Content).Facets;
            else return string.Empty;
            if (facets != null)
                foreach (var facet in facets) {
                    if (facet is XmlSchemaEnumerationFacet) return string.Empty;
                    if (facet is XmlSchemaPatternFacet) { pattern = ((XmlSchemaPatternFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMinInclusiveFacet) { minInclusive = ((XmlSchemaMinInclusiveFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMaxInclusiveFacet) { maxInclusive = ((XmlSchemaMaxInclusiveFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMinExclusiveFacet) { minExclusive = ((XmlSchemaMinExclusiveFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMaxExclusiveFacet) { maxExclusive = ((XmlSchemaMaxExclusiveFacet)facet).Value; continue; }
                    if (facet is XmlSchemaLengthFacet) { lengthRestriction = ((XmlSchemaLengthFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMinLengthFacet) { minLengthRestriction = ((XmlSchemaMinLengthFacet)facet).Value; continue; }
                    if (facet is XmlSchemaMaxLengthFacet) { maxLengthRestriction = ((XmlSchemaMaxLengthFacet)facet).Value; continue; }
                    if (facet is XmlSchemaFractionDigitsFacet) { fractionDigits = ((XmlSchemaFractionDigitsFacet)facet).Value; continue; }
                    if (facet is XmlSchemaTotalDigitsFacet) { totalDigits = ((XmlSchemaTotalDigitsFacet)facet).Value; continue; }
                }
            switch (schemaType.Datatype.TypeCode)
            {
                case XmlTypeCode.String:
                    var lengthRestrictionText = GetLengthRestrictionText(lengthRestriction, minLengthRestriction, maxLengthRestriction);
                    var patternText = string.Format("Regex pattern {0}.", pattern.Replace(@"\", ""));
                    return string.IsNullOrEmpty(pattern) ?
                        string.IsNullOrEmpty(lengthRestrictionText) ? string.Empty : lengthRestrictionText :
                        string.IsNullOrEmpty(lengthRestrictionText) ? patternText : patternText + Environment.NewLine + lengthRestrictionText;
                case XmlTypeCode.Duration:
                    return "Duration: PnYnMnDTnHnMnS, and nY represents years, nM months, nD days, " + Environment.NewLine +
                        "'T' is the time separator, nH hours, nM minutes and nS seconds (optionally including decimal digits to arbitrary precision).";
                case XmlTypeCode.Decimal:
                    var numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, Decimal.MinValue.ToString("00e+0", CultureInfo.InvariantCulture), Decimal.MaxValue.ToString("00e+0", CultureInfo.InvariantCulture));
                    var fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    var text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Decimal value " + text;
                case XmlTypeCode.Float:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, float.MinValue.ToString("00e+0", CultureInfo.InvariantCulture), float.MaxValue.ToString("00e+0", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "32-bit floating point " + text;
                case XmlTypeCode.Double:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, double.MinValue.ToString("00e+0", CultureInfo.InvariantCulture), double.MaxValue.ToString("00e+0", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "64-bit double value " + text;
                case XmlTypeCode.Integer:
                case XmlTypeCode.Int:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, int.MinValue.ToString("#,#", CultureInfo.InvariantCulture), int.MaxValue.ToString("#,#", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Integer value " + text;
                case XmlTypeCode.PositiveInteger:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, "0", int.MaxValue.ToString("#,#", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Positive integer value " + text;
                case XmlTypeCode.NegativeInteger:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, int.MinValue.ToString("#,#", CultureInfo.InvariantCulture), "0");
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Negative integer value " + text;
                case XmlTypeCode.Short:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, short.MinValue.ToString("#,#", CultureInfo.InvariantCulture), short.MaxValue.ToString("#,#", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Short value " + text;
                case XmlTypeCode.Long:
                    numericRangeRestrictionText = GetNumericRangeRestriction(pattern, minInclusive, minExclusive, maxInclusive, maxExclusive, long.MinValue.ToString("00e+0", CultureInfo.InvariantCulture), long.MaxValue.ToString("00e+0", CultureInfo.InvariantCulture));
                    fractionRestrictionText = GetFractionRestrictionText(fractionDigits, totalDigits);
                    text = string.IsNullOrEmpty(numericRangeRestrictionText) ?
                        string.IsNullOrEmpty(fractionRestrictionText) ? string.Empty : ": " + fractionRestrictionText :
                        string.IsNullOrEmpty(fractionRestrictionText) ? numericRangeRestrictionText : numericRangeRestrictionText + Environment.NewLine + fractionRestrictionText;
                    return "Long value " + text;
                case XmlTypeCode.DateTime:
                    return "Date time, starting with an optional '-' and yyyy-mm-ddThh:mm:ss" + Environment.NewLine +
                        "and ending with an optional fraction of seconds and time zone indicator" + Environment.NewLine +
                        "(e.g. -05:00 is five hours behind UTC)";
                case XmlTypeCode.Date:
                    return "Date, starting with an optional '-' and yyyy-mm-dd";
                case XmlTypeCode.Time:
                    return "Time, hh:mm:ss and ending with an optional fraction of seconds and time zone indicator (e.g. -05:00 is five hours behind UTC)";
                default:
                    return "Data type code is " + schemaType.Datatype.TypeCode;
            }
        }

        private static string GetFractionRestrictionText(string fractionDigits, string totalDigits)
        {
            if (!string.IsNullOrEmpty(fractionDigits) && !string.IsNullOrEmpty(totalDigits)) return string.Format("The entered number cannot have more than {0} digits, of which {1} fraction digits.", totalDigits, fractionDigits);
            if (!string.IsNullOrEmpty(fractionDigits)) return string.Format("The entered number cannot have more than {0} fraction digits.", fractionDigits);
            if (!string.IsNullOrEmpty(totalDigits)) return string.Format("The entered number cannot have more than {0} digits", totalDigits);
            return string.Empty;
        }

        /// <summary>
        /// Gets the length restriction text.
        /// </summary>
        /// <param name="lengthRestriction">The length restriction.</param>
        /// <param name="minLengthRestriction">The min length restriction.</param>
        /// <param name="maxLengthRestriction">The max length restriction.</param>
        /// <returns>The length restricted text</returns>
        private static string GetLengthRestrictionText(string lengthRestriction, string minLengthRestriction, string maxLengthRestriction)
        {
            if (!string.IsNullOrEmpty(lengthRestriction)) return string.Format("The entered text should be {0} characters long.", lengthRestriction);
            if (!string.IsNullOrEmpty(minLengthRestriction) && !string.IsNullOrEmpty(maxLengthRestriction)) return string.Format("The entered text should be between {0} and {1} characters long.", minLengthRestriction, maxLengthRestriction);
            if (!string.IsNullOrEmpty(minLengthRestriction)) return string.Format("The entered text should be minimally {0} characters long.", minLengthRestriction);
            if (!string.IsNullOrEmpty(maxLengthRestriction)) return string.Format("The entered text should be maximally {0} characters long.", maxLengthRestriction);
            return string.Empty;
        }

        /// <summary>
        /// Gets the allowable numeric range.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <param name="minInclusive">The min inclusive, i.e. the minimum value including the value itself.</param>
        /// <param name="minExclusive">The min exclusive, i.e. the minimum value excluding the value itself.</param>
        /// <param name="maxInclusive">The max inclusive, i.e. the maximum value including the value itself.</param>
        /// <param name="maxExclusive">The max exclusive, i.e. the maximum value excluding the value itself.</param>
        /// <param name="min">The minimum value of this datatype</param>
        /// <param name="max">The maximum value of this datatype</param>
        /// <returns>The allowable range, e.g. [1,5], i.e. 1,2,3,4,5 or (1,5], i.e. 2,3,4,5</returns>
        private static string GetNumericRangeRestriction(string pattern, string minInclusive, string minExclusive, string maxInclusive, string maxExclusive, string min, string max)
        {
            var openingBracket = "[";
            var closingBracket = "]";
            var minimum = min;
            if (!string.IsNullOrEmpty(minInclusive)) minimum = minInclusive;
            if (!string.IsNullOrEmpty(minExclusive))
            {
                minimum = minExclusive;
                openingBracket = "(";
            }
            var maximum = max;
            if (!string.IsNullOrEmpty(maxInclusive)) maximum = maxInclusive;
            if (!string.IsNullOrEmpty(maxExclusive))
            {
                maximum = maxExclusive;
                closingBracket = ")";
            }
            var range = string.Format("{0}{1}, {2}{3}", openingBracket, minimum, maximum, closingBracket);
            return (string.IsNullOrEmpty(pattern) ? string.Empty : ", using regex pattern " + pattern + ", ") + "in the range " + range;
        }



        /// <summary>
        /// Extract the root element from an XSD
        /// </summary>
        /// <param name="schemaSet">Initial schemaset containing (all) XSD files</param>
        /// <returns>Assumed root XmlSchemaElement</returns>
        public static XmlSchemaElement GetRootElement(XmlSchemaSet schemaSet)
        {
            var schemaElem = schemaSet.GlobalElements[XmlQualifiedName.Empty] as XmlSchemaElement;
            if (schemaElem == null)
            {
                //If element by name is not found, Get first non-abstract root element 
                var sourceUri = string.Empty;
                foreach (XmlSchema schema in schemaSet.Schemas())
                {
                    sourceUri = schema.SourceUri;
                    break;
                }
                // First process only the original XSD, and try to find a global element in there
                foreach (XmlSchemaElement elem1 in schemaSet.GlobalElements.Values)
                {
                    if (elem1.IsAbstract || !elem1.SourceUri.Equals(sourceUri)) continue;
                    schemaElem = elem1;
                    break;
                }
                // Otherwise, take the first global element that's available, and consider that as the root
                if (schemaElem == null)
                    foreach (XmlSchemaElement elem1 in schemaSet.GlobalElements.Values)
                    {
                        if (elem1.IsAbstract) continue;
                        schemaElem = elem1;
                        break;
                    }
            }
            return schemaElem;
        }

        /*
                /// <summary>
                /// Extract the root elements from an XSD
                /// </summary>
                /// <param name="schemaSet">Initial schemaset containing (all) XSD files</param>
                /// <returns>Set of root XmlSchemaElement elements</returns>
                public static List<XmlSchemaElement> GetRootElements(XmlSchemaSet schemaSet) {
                    var schemaElements = new List<XmlSchemaElement>();
                    var schemaElem = schemaSet.GlobalElements[XmlQualifiedName.Empty] as XmlSchemaElement;
                    if (schemaElem == null)
                        foreach (XmlSchemaElement elem1 in schemaSet.GlobalElements.Values) {
                            if (elem1.IsAbstract) continue;
                            schemaElements.Add(elem1);
                        }
                    else schemaElements.Add(schemaElem);
                    return schemaElements;
                }
        */

        public static XmlNode GetElementFromValue(XmlNode node)
        {
            if (node.OwnerDocument == null) return null;
            return node.OwnerDocument.DocumentElement == null ? null : GetElementFromValue(node.OwnerDocument.DocumentElement, node.FirstChild.Value);
        }

        private static XmlNode GetElementFromValue(XmlNode node, string value)
        {
            if (node == null) return null;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.FirstChild != null && !string.IsNullOrEmpty(child.FirstChild.Value) && child.FirstChild.Value.Equals(value)) return child;
                if (!child.HasChildNodes) continue;
                var foundNode = GetElementFromValue(child, value);
                if (foundNode != null) return foundNode;
            }
            return null;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetNodeValue(XmlNode node)
        {
            if (node == null) return string.Empty;
            if (!string.IsNullOrEmpty(node.Value)) return node.Value;
            if (node.FirstChild != null && !string.IsNullOrEmpty(node.FirstChild.Value)) return node.FirstChild.Value;
            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public static string GetNodeName(XmlAttribute attribute)
        {
            if (attribute.OwnerElement == null) return attribute.Name;
            var friendlyName = GetXmlNodeName(attribute.OwnerElement);
            return (string.IsNullOrEmpty(friendlyName) ?
                string.Format("{0}\\{1}", attribute.OwnerElement.Name, attribute.Name) :
                string.Format("{0}: {1}\\{2}", attribute.OwnerElement.Name, friendlyName, attribute.Name));
        }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string GetNodeName(XmlNode node)
        {
            if ((node is XmlText || node is XmlComment || node is XmlCDataSection) && node.ParentNode != null) return string.Format("{0}\\{1}", node.ParentNode.Name, node.Name);
            if (node is XmlAttribute) return GetNodeName(node as XmlAttribute);
            if (!(node is XmlElement)) return node.Name;
            var friendlyName = GetXmlNodeName(node);
            return (string.IsNullOrEmpty(friendlyName) ?
                node.Name :
                string.Format("{0} {1}", node.Name, friendlyName));
        }

        /// <summary>
        /// Based on a unique user ID, find the node that owns this ID
        /// </summary>
        /// <param name="node">Current node</param>
        /// <returns>node that contains this ID</returns>
        public static XmlNode GetReferredNode(XmlNode node)
        {
            if (node == null || node.SchemaInfo.SchemaType == null || node.OwnerDocument == null) return null;
            return GetXmlNodeOfTypeWithValue(node.OwnerDocument.DocumentElement, GetReferencedName(node.SchemaInfo.SchemaType), node.FirstChild.Value);
        }

        /// <summary>
        /// Gets the namespaces, returned as a key value pair, where the key is the namespace prefix, and the value the namespace value.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetNamespaces(XmlDocument document) {
            var namespaces = new Dictionary<string, string>();
            var root = document.DocumentElement;
            if (root == null || !root.HasAttributes) return namespaces;
            var defaultNamespace = string.Empty;
            foreach (XmlAttribute attribute in root.Attributes) {
                if (attribute.Name.Equals("xmlns")) {
                    defaultNamespace = attribute.Value;
                    continue;
                }
                if (!attribute.Name.StartsWith("xmlns")) continue;
                var key = attribute.Name.Remove(0, 6).Trim(); // remove the xmlns: bit of the namespace prefix
                if (!namespaces.ContainsKey(key)) namespaces.Add(key, attribute.Value);
            }
            if (string.IsNullOrEmpty(defaultNamespace)) return namespaces;
            if (!namespaces.ContainsKey("d")) namespaces.Add("d", defaultNamespace);
            else if (!namespaces.ContainsKey("p")) namespaces.Add("p", defaultNamespace);
            else if (!namespaces.ContainsKey("e")) namespaces.Add("e", defaultNamespace);
            else if (!namespaces.ContainsKey("z")) namespaces.Add("z", defaultNamespace);
            return namespaces;
        }
    }
}