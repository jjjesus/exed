#region

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using TreeListControl.Resources;
using TreeListControl.Tree;

#endregion

//todo Multi-line textbox 
//todo Search functionality
//todo Print (preview) - could use WebBrowser's Print (preview)
//todo Refactor MVVM setup
//todo Remove map
//todo How to add a CDATA section?
//todo Check undo functionality
//bug What to do with empty elements (that have a value).
//bug How can I delete a node that shouldn't be there, i.e. an element that should have no value?
//todo How can make navigation with the keyboard easier: arrows, enter, insert, delete?
//For later
//todo XsltView doesn't scale
//todo Can I use this editor to create XSDs, just like an XML tree, with types as value, and an optional column for annotations
//todo Optionally, add dotted lines to connect treelistitems
namespace TreeListControl
{

    #region Delegates

    public delegate void XmlNodeChangedEventHandler(object sender, XmlNodeChangedEventArgs e);

    #endregion Delegates

    /// <summary>
    ///   An XML tree model class
    /// </summary>
    public class XmlModel : ITreeModel
    {
        #region Fields

        private readonly UndoBuffer undoBuffer = new UndoBuffer();
        private XmlNode cutPasteNode;
        private XmlDocument document;
        private string fileName;
        private bool isDirty;

        /// <summary>
        ///   When inserting nodes recursively, I may generate a lot of new nodes. However, during undo I want to undo all these nodes at the same time.
        ///   In addition, I don't want to send XmlNodeChanges constantly - only at the end when I'm done.
        /// </summary>
        private bool isInsertingNodesRecursively;

        /// <summary>
        ///   During Undo or Redo, I don't want to add document changes to the XmlDocument again
        /// </summary>
        private bool isUndoRedoing;

        public Uri SchemaLocation { get; set; }

        #endregion Fields

        #region Constructors

        public XmlModel() {
            ErrorMessages = new ObservableCollection<ErrorMessage>();
            document = new XmlDocument();
        }

        public XmlDocument Document { get { return document; } }

        /// <summary>
        ///   Initializes a new instance of the
        ///   <see cref="XmlModel" />
        ///   class.
        /// </summary>
        /// <param name="filename">
        ///   The filename of an XML or XSD document.
        /// </param>
        public XmlModel(string filename) {
            ErrorMessages = new ObservableCollection<ErrorMessage>();
            if (!File.Exists((filename))) throw new ArgumentException("File does not exist!");
            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension)) throw new ArgumentException("Wrong file extension: only XML or XSD extension is supported!", filename);
            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)) LoadXML(filename);
            else if (extension.Equals(".xsd", StringComparison.OrdinalIgnoreCase)) CreateXmlDocument(filename); //LoadXML(InitXmlDocument(filename), filename);
            else throw new ArgumentException("Wrong file extension: only XML or XSD extension is supported!", filename);
        }

        #endregion Constructors

        private string clipboard;

        private XmlNode CutPasteNode {
            get {
                if (!Clipboard.ContainsText(TextDataFormat.Html)) return null;
                if (!string.IsNullOrEmpty(clipboard) && clipboard.Equals(Clipboard.GetText(TextDataFormat.Html)) &&
                    cutPasteNode != null) return cutPasteNode;
                var fragment = document.CreateDocumentFragment();
                try {
                    fragment.InnerXml = clipboard = Clipboard.GetText(TextDataFormat.Html);
                }
                catch (XmlException e) {
                    return null;
                }
                AddValueToNodes(fragment);
                RemoveNamespaces(fragment);
                return cutPasteNode = fragment.FirstChild;
            }
            set { Clipboard.SetText(value.OuterXml, TextDataFormat.Html); }
        }

        #region ITreeModel Members

        public bool CanPaste(XmlElement node) {
            if (node == null || CutPasteNode == null) return false;
            if (cutPasteNode is XmlComment) return true;
            var name = cutPasteNode.Name;
            if (string.IsNullOrEmpty(name)) return false;
            var isAttribute = cutPasteNode is XmlAttribute;
            var isElement = cutPasteNode is XmlElement;
            var possibleChildren = Utils.GetPossibleChildren(node, isElement);
            if (isAttribute) {
                if (possibleChildren.OfType<XmlSchemaAttribute>().Any(child => name.Equals((child).Name))) return true;
            }
            else if (isElement) if (possibleChildren.OfType<XmlSchemaElement>().Any(child => name.Equals((child).Name))) return true;
            return false;
        }

        #endregion

        /// <summary>
        /// Adds the string.Empty value to nodes. This is necessary in order to be able to bind them.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        private static void AddValueToNodes(XmlNode fragment) {
            foreach (XmlNode child in fragment.ChildNodes)
                if (child is XmlElement)
                    if (!child.HasChildNodes && string.IsNullOrEmpty(child.InnerText)) child.InnerText = string.Empty;
                    else AddValueToNodes(child);
                else if (child is XmlAttribute && string.IsNullOrEmpty(child.Value)) child.Value = string.Empty;
        }

        /// <summary>
        /// Removes the namespace attributes xmlns.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        private static void RemoveNamespaces(XmlNode fragment) {
            foreach (XmlNode child in fragment.ChildNodes)
                if (child is XmlElement) {
                    if ((child as XmlElement).HasAttributes)
                        for (var i = child.Attributes.Count - 1; i >= 0; --i) {
                            var att = child.Attributes[i];
                            if (!string.IsNullOrEmpty(att.Value) && att.Name.Equals("xmlns")) child.Attributes.Remove(att);
                        }
                    RemoveNamespaces(child);
                }
        }

        private void CreateXmlDocument(string xsdFile) {
            ErrorMessages.Clear();
            if (!File.Exists(xsdFile)) {
                ErrorMessages.Add(new ErrorMessage{Message = string.Format("No schema file found: {0} is missing", xsdFile)});
                return;
            }
            isDirty = true;
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, xsdFile);
            schemaSet.Compile();
            if (!schemaSet.IsCompiled) return;
            var schemaElem = Utils.GetRootElement(schemaSet);
            //var schemaElements = Utils.GetRootElements(schemaSet);
            if (schemaElem != null) InitXmlDocument(xsdFile, schemaSet, schemaElem);
            else ErrorMessages.Add(new ErrorMessage {Message = "No root element found"});
        }

        /// <summary>
        /// Initializes the XML document with the namespaces.
        /// </summary>
        /// <param name="xsdFile">The XSD file.</param>
        /// <param name="schemaSet">The schema set.</param>
        /// <param name="schemaElem">The schema elem.</param>
        private void InitXmlDocument(string xsdFile, XmlSchemaSet schemaSet, XmlSchemaElement schemaElem) {
            if (document != null) {
                document.NodeChanged -= DocumentNodeChanged;
                document.NodeInserted -= DocumentNodeChanged;
                document.NodeRemoved -= DocumentNodeChanged;
            }
            document = new XmlDocument();
            document.Schemas.Add(schemaSet);
            SchemaLocation = new Uri(xsdFile);
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));
            var root = document.CreateElement(schemaElem.QualifiedName.Name, schemaElem.QualifiedName.Namespace);
            document.AppendChild(root);
            // Set namespace
            var xmlnsXsi = document.CreateAttribute("xmlns", "xsi", "http://www.w3.org/2000/xmlns/");
            xmlnsXsi.Value = XmlSchema.InstanceNamespace;
            root.SetAttributeNode(xmlnsXsi);
            // Set XSD file
            var xmlnsXsd = document.CreateAttribute("xsi", "noNamespaceSchemaLocation",
                                                    "http://www.w3.org/2001/XMLSchema-instance");
            xmlnsXsd.Value = Path.GetFileName(xsdFile);
            root.SetAttributeNode(xmlnsXsd);
            RecursivelyInsertNodes(root);

            document.NodeChanged += DocumentNodeChanged;
            document.NodeInserted += DocumentNodeChanged;
            document.NodeRemoved += DocumentNodeChanged;

            var e = new XmlNodeChangedEventArgs(root, null, root.ParentNode, string.Empty, string.Empty,
                                                XmlNodeChangedAction.Insert);
            OnNodeChanged(e);
        }

        #region Private Properties

        private bool CanValidate {
            get { return (File.Exists(SchemaLocation.LocalPath) && document.DocumentElement != null); }
        }

        #endregion Private Properties

        #region Public Properties

        public bool CanSave {
            get { return isDirty; }
        }

        public ObservableCollection<ErrorMessage> ErrorMessages { get; set; }

        public bool IsValid { get; set; }

        /// <summary>
        ///   Gets a value indicating whether this instance can redo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can redo; otherwise <c>false</c>.
        /// </value>
        public bool CanRedo {
            get { return undoBuffer.CanRedo; }
        }

        /// <summary>
        ///   Gets a value indicating whether this instance can undo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can undo; otherwise <c>false</c>.
        /// </value>
        public bool CanUndo {
            get { return undoBuffer.CanUndo; }
        }

        #endregion Public Properties

        #region Private Methods

        private XmlNode AddAttribute(XmlSchemaObject node, XmlNode parent) {
            var attr = (XmlSchemaAttribute) node;
            var newNode = document.CreateAttribute(attr.QualifiedName.Name, attr.QualifiedName.Namespace);
            newNode.Value = Utils.GetDefaultValue(node);
            return InsertAttribute(newNode, parent);
        }

        /// <summary>
        ///   Add an XmlElement to the document tree, in the same position as in the original XSD sequence
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private XmlNode AddElement(XmlSchemaObject node, XmlNode parent) {
            var el = (XmlSchemaElement) node;
            var newNode = document.CreateElement(el.QualifiedName.Name, el.QualifiedName.Namespace);
            newNode.InnerText = Utils.GetDefaultValue(node);
            return InsertElement(newNode, parent);
        }

        private void DocumentNodeChanged(object sender, XmlNodeChangedEventArgs e) {
            //SaveXml(e); // just for testing purposes, saves the undo/redo changes
            if (!(isUndoRedoing || isInsertingNodesRecursively)) undoBuffer.Do(new Memento {ActionType = e.Action, State = e});
            switch (e.Action) {
                case XmlNodeChangedAction.Change:
                    if (!CanValidate || e.OldValue == e.NewValue) return;
                    if (!isUndoRedoing)
                        document.Validate(
                            (send, ex) => { throw new ArgumentException(ex.Severity + ": " + ex.Message); }, e.NewParent);
                    // When updating a reference type (i.e. a node that has a type ***ref) or a name (i.e. a node that has a name containing "name"), the GUI update needs to be
                    // explicitly triggered
                    //if ((isUndoRedoing || Utils.IsReferenceType(e.NewParent) || Utils.HasName(e.NewParent)) &&
                    if (!isInsertingNodesRecursively) 
                        OnNodeChanged(e);
                    Validate();
                    break;
                default:
                    Validate();
                    if (!isInsertingNodesRecursively) OnNodeChanged(e);
                    break;
            }
            isDirty = true;
        }

        private Uri GetLocalSchemaLocation(XmlNode ctx, string filename) {
            var baseUri = new Uri(new Uri(fileName), new Uri(".", UriKind.Relative));
            if (!string.IsNullOrEmpty(ctx.BaseURI)) baseUri = new Uri(ctx.BaseURI);
            return new Uri(baseUri, filename);
        }

        /// <summary>
        ///   To keep things simple, I only resolve locally stored schemas, i.e. not from the Internet
        /// </summary>
        /// <param name="doc">
        ///   XmlDocument that contains an XSD specification or location
        /// </param>
        /// <returns>Uri of the XSD</returns>
        private Uri GetSchemaLocation(XmlDocument doc) {
            if (doc.DocumentElement != null) {
                foreach (XmlAttribute a in doc.DocumentElement.Attributes)
                    if (a.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance")
                        switch (a.LocalName) {
                            case "noNamespaceSchemaLocation":
                                //var path = a.Value;
                                //if (!string.IsNullOrEmpty(path)) {}
                                return GetLocalSchemaLocation(a, a.Value);
                            case "schemaLocation":
                                var whitespace = a.Value.LastIndexOf(' ');
                                return GetLocalSchemaLocation(a, Path.GetFileName(a.Value.Substring(whitespace + 1, a.Value.Length - whitespace - 1)));
                        }
                var localXsdPath = Path.Combine(Path.GetDirectoryName(new Uri(doc.BaseURI).LocalPath), doc.DocumentElement.LocalName) + ".xsd";
                return File.Exists(localXsdPath) ? new Uri(localXsdPath, UriKind.RelativeOrAbsolute) : new Uri(InferXSD(doc, localXsdPath));
            }
            return new Uri(AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        ///   Insert the attribute in the right position
        /// </summary>
        /// <param name="newNode">New node to insert</param>
        /// <param name="parent">Parent node</param>
        /// <returns></returns>
        private static XmlNode InsertAttribute(XmlAttribute newNode, XmlNode parent) {
            if (parent.Attributes == null) return null;
            if (parent.Attributes.Count == 0) {
                // is only child
                parent.Attributes.Append(newNode);
                return newNode;
            }
            // Insert the new node in the same position as in the XSD: 1) because they may need to be in sequence (xsd:sequence), and 2) because it keeps the tree consistent
            var children = Utils.GetChildAttributes((XmlElement) parent);
            var myPosition = children.FindIndex(x => ((XmlSchemaAttribute) x).Name.Equals(newNode.Name));
            if (myPosition == 0) {
                // is first
                parent.Attributes.Prepend(newNode);
                return newNode;
            }
            if (myPosition == children.Count - 1) {
                // is last
                parent.Attributes.Append(newNode);
                return newNode;
            }
            for (var i = 0; i < parent.Attributes.Count; i++) {
                var child = parent.Attributes[i];
                var childPosition = children.FindIndex(x => ((XmlSchemaAttribute) x).Name.Equals(child.Name));
                if (childPosition <= myPosition) continue;
                parent.Attributes.InsertBefore(newNode, child);
                return newNode;
            }
            parent.Attributes.Append(newNode);
            return newNode;
        }

        /// <summary>
        ///   Insert the element in the right position
        /// </summary>
        /// <param name="newNode">New node to insert</param>
        /// <param name="parent">Parent node</param>
        /// <returns></returns>
        private XmlNode InsertElement(XmlNode newNode, XmlNode parent) {
            if (parent == null) {
                document.AppendChild(newNode);
                return newNode;
            }
            if (!parent.HasChildNodes) {
                // is only child
                parent.AppendChild(newNode);
                return newNode;
            }
            // Insert the new node in the same position as in the XSD: 1) because they may need to be in sequence (xsd:sequence), and 2) because it keeps the tree consistent
            var children = Utils.GetChildElements((XmlElement) parent);
            var myPosition =
                children.FindIndex(
                    x =>
                    newNode.Name.Equals(((XmlSchemaElement) x).Name) ||
                    newNode.Name.Equals(((XmlSchemaElement) x).QualifiedName.Name));
            if (myPosition == children.Count - 1) {
                // is last
                parent.AppendChild(newNode);
                return newNode;
            }
            for (var i = 0; i < parent.ChildNodes.Count; i++) {
                if (!(parent.ChildNodes[i] is XmlElement)) continue;
                var child = (XmlElement) parent.ChildNodes[i];
                var childPosition =
                    children.FindIndex(
                        x =>
                        child.Name.Equals(((XmlSchemaElement) x).Name) ||
                        child.Name.Equals(((XmlSchemaElement) x).QualifiedName.Name));
                if (childPosition <= myPosition) continue;
                parent.InsertBefore(newNode, child);
                return newNode;
            }
            parent.AppendChild(newNode);
            return newNode;
        }

        /// <summary>
        ///   Insert a node recursively.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>New node</returns>
        private XmlNode InsertNodeRecursively(XmlSchemaObject node, XmlNode parent) {
            var newNode = node is XmlSchemaAttribute ? AddAttribute(node, parent) : AddElement(node, parent);
            if (newNode is XmlElement) RecursivelyInsertNodes(newNode);
            return newNode;
        }

        /// <summary>
        ///   Looks at the newNode, and adds any nodes that are required recursively
        /// </summary>
        /// <param name="newNode"></param>
        private void RecursivelyInsertNodes(XmlNode newNode) {
            Validate();
            foreach (var child in Utils.GetPossibleChildren(newNode as XmlElement, false)) {
                if (child is XmlSchemaAttribute) {
                    var attr = child as XmlSchemaAttribute;
                    if (attr.Use.Equals(XmlSchemaUse.Required)) InsertNodeRecursively(attr, newNode as XmlElement);
                }
                if (!(child is XmlSchemaElement)) continue;
                var elem = child as XmlSchemaElement;
                if (elem.MinOccurs > 0) InsertNodeRecursively(elem, newNode as XmlElement);
            }
        }

        /*
		   XmlCachingResolver

		Another common operation when dealing with XML data that contains HTTP DTD or Schema references is to want to cache those DTD's and/or schemas locally to avoid expensive HTTP requests upon every document read operation. This is what the XmlCachingResolver does.

		The XmlCachingResolver caches the entities in memory, but could be easily modified to cache the entities on the local hard drive for a more scalable solution. 
		 * XmlCachingResolver cache = new XmlCachingResolver();
		    XmlDocument doc = new XmlDocument();
		    doc.XmlResolver = cache;
		    doc.Load(url);

		 */

        /*
                /// <summary>
                /// Loads the XML.
                /// </summary>
                /// <param name="stream">The stream.</param>
                /// <param name="xmlSchemaFile">The XML schema file.</param>
                private void LoadXML(Stream stream, string xmlSchemaFile) {
                    stream.Seek(0, SeekOrigin.Begin); // rewind
                    try {
                        if (document != null) {
                            document.NodeChanged -= DocumentNodeChanged;
                            document.NodeInserted -= DocumentNodeChanged;
                            document.NodeRemoved -= DocumentNodeChanged;
                        }
                        schemaLocation = new Uri(xmlSchemaFile);
                        ErrorMessages.Clear();
                        if (File.Exists(schemaLocation.LocalPath)) {
                            // Create the XmlSchemaSet class.
                            var sc = new XmlSchemaSet();
                            // Add the schema to the collection.
                            sc.Add(string.Empty, schemaLocation.LocalPath);
                            var settings = new XmlReaderSettings {ValidationType = ValidationType.Schema, Schemas = sc};
                            settings.ValidationEventHandler += ValidateDocumentHandler;
                            var reader = XmlReader.Create(stream, settings);
                            document = new XmlDocument();
                            document.Load(reader);
                            Validate();
                            document.NodeChanged += DocumentNodeChanged;
                            document.NodeInserted += DocumentNodeChanged;
                            document.NodeRemoved += DocumentNodeChanged;
                            // set up namespace manager to enable XML searching
                            //var nsmgr = new XmlNamespaceManager(document.NameTable);
                            //chose df to represent default
                            //nsmgr.AddNamespace("df", "http://tempuri.org/LoggingMessagesDefinition.xsd");
                        }
                        else ErrorMessages.Add(new ErrorMessage {Message = "No schema found"});
                    }
                    catch (XmlException e) {
                        ErrorMessages.Add(new ErrorMessage {Message = string.Format("Xml is invalid: Line {0}, Column {1}: {2}", e.LineNumber, e.LinePosition, e.Message)});
                    }
                }
        */

        /// <summary>
        ///   Loads the XML file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        private void LoadXML(string filename) {
            fileName = Path.GetFullPath(filename);
            var reader = XmlReader.Create(filename, null
                //new XmlReaderSettings {
                //ValidationFlags = XmlSchemaValidationFlags.AllowXmlAttributes | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints,
                //IgnoreWhitespace = true,
                //ValidationType = ValidationType.Schema
                //                                                          }
                );
            try {
                if (document != null) {
                    document.NodeChanged -= DocumentNodeChanged;
                    document.NodeInserted -= DocumentNodeChanged;
                    document.NodeRemoved -= DocumentNodeChanged;
                }
                document = new XmlDocument();
                document.Load(reader);
                SchemaLocation = GetSchemaLocation(document);
                ErrorMessages.Clear();
                if (SchemaLocation != null && !File.Exists(SchemaLocation.LocalPath))
                    SchemaLocation =
                        new Uri(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XsdCache"),
                                             Path.GetFileName(SchemaLocation.AbsolutePath)));
                if (SchemaLocation != null && File.Exists(SchemaLocation.LocalPath)) {
                    // Create the XmlSchemaSet class.
                    var sc = new XmlSchemaSet();
                    // Add the schema to the collection.
                    var namespaceUri = document.DocumentElement != null
                                           ? document.DocumentElement.NamespaceURI
                                           : string.Empty;
                    sc.Add(namespaceUri, SchemaLocation.LocalPath);
                    var settings = new XmlReaderSettings {ValidationType = ValidationType.Schema, Schemas = sc};
                    settings.ValidationEventHandler += ValidateDocumentHandler;
                    reader = XmlReader.Create(filename, settings);
                    document = new XmlDocument();
                    document.Load(reader);
                    AssignValuesToEmptyElements(document.DocumentElement);
                    Validate();
                    document.NodeChanged += DocumentNodeChanged;
                    document.NodeInserted += DocumentNodeChanged;
                    document.NodeRemoved += DocumentNodeChanged;
                    // set up namespace manager to enable XML searching
                    //var nsmgr = new XmlNamespaceManager(document.NameTable);
                    //chose df to represent default
                    //nsmgr.AddNamespace("df", "http://tempuri.org/LoggingMessagesDefinition.xsd");
                }
                else ErrorMessages.Add(new ErrorMessage {Message = "No schema found"});
            }
            catch (XmlException e) {
                ErrorMessages.Add(new ErrorMessage {
                                                       Message =
                                                           string.Format("Xml is invalid: Line {0}, Column {1}: {2}", e.LineNumber,
                                                                         e.LinePosition, e.Message)
                                                   });
            }
        }

        /// <summary>
        /// Assigns a value to empty elements, so we can bind to them in the editor (e.g. if we don't do this, the FirstChild is null, 
        /// so binding to FirstChild.Value raises an exception in XAML (so the application doesn't crash, but we still cannot set a value).
        /// </summary>
        /// <param name="element">The element.</param>
        private static void AssignValuesToEmptyElements(XmlElement element) {
            return;
            foreach (XmlAttribute child in element.Attributes) if (string.IsNullOrEmpty(child.Value)) child.Value = string.Empty;
            foreach (var child in element.ChildNodes) {
                if (child is XmlElement) {
                    var el = child as XmlElement;
                    if (el.HasChildNodes && el.ChildNodes.Count > 0) AssignValuesToEmptyElements(el);
                    else if (el.FirstChild == null) el.AppendChild(el.OwnerDocument.CreateTextNode(string.Empty));
                }
            }
        }


        /// <summary>
        /// Infers the XSD and writes it to disk (root.xsd, where root is the document element).
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="outputFileName">Name of the output file.</param>
        /// <returns>Path to XSD on disk</returns>
        /// <seealso cref="http://www.java2s.com/Code/CSharp/XML-LINQ/CreatinganXSDSchemabyInferringItfromanXMLDocument.htm"/>
        private static string InferXSD(XmlDocument doc, string outputFileName) {
            if (File.Exists(outputFileName)) return outputFileName;
            var inference = new XmlSchemaInference();
            var schemaSet = inference.InferSchema(new XmlTextReader(doc.BaseURI));
            var w = XmlWriter.Create(outputFileName, new XmlWriterSettings {Indent = true, NewLineOnAttributes = false});
            foreach (XmlSchema schema in schemaSet.Schemas()) schema.Write(w);
            w.Close();
            return outputFileName;
        }

        private static void RemoveNode(XmlNode node, XmlNode parent) {
            if (node is XmlAttribute) parent.Attributes.Remove(node as XmlAttribute);
            else node.ParentNode.RemoveChild(node);
        }

        /*
                /// <summary>
                /// Save the XML document for testing purposes
                /// </summary>
                private void SaveXml(XmlNodeChangedEventArgs e) { document.Save(string.Format("{0:0000} {1} - {2}.xml", ++testFilenameCounter, TestFilename, XmlNodeChange(e))); }
        */

        /// <summary>
        ///   Validates this instance.
        /// </summary>
        private void Validate() {
            if (!CanValidate) return;
            ErrorMessages.Clear();
            document.Validate(ValidateXmlNodeHandler);
            //SaveXml("tmp.xml"); // only for testing purposes
        }

        /// <summary>
        ///   Validate the XmlDocument when reading it using the XmlReader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateDocumentHandler(object sender, ValidationEventArgs e) {
            IsValid = false;
            var xmlReader = (XmlReader) sender;
            var error = new ErrorMessage();
            if (xmlReader != null) {
                error.IsXmlElement = (xmlReader.NodeType == XmlNodeType.Element);
                error.Name = xmlReader.Name;
                error.Value = xmlReader.Value;
                error.Tag = e.Exception.Data;
            }
            switch (e.Severity) {
                case XmlSeverityType.Error:
                    error.Message = String.Format("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    error.Message = String.Format("Warning {0}", e.Message);
                    break;
            }
            ErrorMessages.Add(error);
        }

        /// <summary>
        ///   Validate the XmlDocument when nodes get inserted or added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateXmlNodeHandler(object sender, ValidationEventArgs e) {
            IsValid = false;
            var error = new ErrorMessage();
            var source = (XmlNode) (((XmlSchemaValidationException) e.Exception).SourceObject);
            if (source != null) {
                error.IsXmlElement = source is XmlElement;
                error.Name = source.Name.Equals("#text") && source.ParentNode != null ? source.ParentNode.Name + "\\" + source.Name : source.Name;
                error.Value = source.Value;
                error.Tag = source;
            }
            switch (e.Severity) {
                case XmlSeverityType.Error:
                    error.Message = String.Format("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    error.Message = String.Format("Warning {0}", e.Message);
                    break;
            }
            ErrorMessages.Add(error);
        }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        ///   Invoke the NodeChanged event; called whenever list changes
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNodeChanged(XmlNodeChangedEventArgs e) { if (NodeChanged != null) NodeChanged(this, e); }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        ///   Get list of children of the specified parent
        /// </summary>
        public IEnumerable GetChildren(object parent) {
            var key = parent as XmlNode;
            if (key != null) {
                if (key is XmlElement) {
                    var node = key as XmlElement;
                    if (node.HasAttributes) foreach (var att in node.Attributes) yield return att;
                }
                foreach (XmlNode child in key.ChildNodes) if (!(child is XmlText || child.LocalName.StartsWith("#")) || child is XmlComment) yield return child;
            }
            else if (document != null && document.FirstChild != null) {
                yield return document.FirstChild; // xml version, utf-8
                if (!document.FirstChild.Equals(document.DocumentElement)) yield return document.DocumentElement;
            }
            // First element: show all Children (i.e. the xml attribute, any other nodes as well as the document.Element.
            //foreach (var child in document.ChildNodes) yield return child;
        }

        /// <summary>
        ///   Returns whether specified parent has any children or not.
        /// </summary>
        public bool HasChildren(object parent) {
            if (parent is XmlAttribute || !(parent is XmlElement)) return false;
            var el = parent as XmlElement;
            if (el.HasAttributes) return true;
            if (el.HasChildNodes) return el.ChildNodes.Cast<XmlNode>().Any(child => !child.LocalName.StartsWith("#"));
            return false;
        }

        /// <summary>
        ///   Redoes this instance.
        /// </summary>
        public void Redo() {
            isUndoRedoing = true;
            var action = undoBuffer.Redo();
            var nce = (XmlNodeChangedEventArgs) action.State;
            switch (action.ActionType) {
                case XmlNodeChangedAction.Change:
                    if (nce.OldParent is XmlElement) nce.OldParent.FirstChild.Value = nce.NewValue;
                    else {
                        if (string.IsNullOrEmpty(nce.NewValue)) {
                            // I do this because when I set an attribute to an empty string, it causes a Validation error that will reset it back again. 
                            Redo();
                            return;
                        }
                        nce.OldParent.Value = nce.NewValue;
                    }
                    break;
                case XmlNodeChangedAction.Insert:
                    if (nce.Node is XmlAttribute) InsertAttribute((XmlAttribute) nce.Node, nce.NewParent);
                    else InsertElement(nce.Node, nce.NewParent);
                    break;
                case XmlNodeChangedAction.Remove:
                    if (nce.Node is XmlAttribute) nce.OldParent.Attributes.Remove((XmlAttribute) nce.Node);
                    else nce.OldParent.RemoveChild(nce.Node);
                    break;
            }
            isUndoRedoing = false;
        }

        /// <summary>
        ///   Undoes this instance.
        /// </summary>
        public void Undo() {
            isUndoRedoing = true;
            var isAtTop = false;
            if (undoBuffer.AtTop) isAtTop = true;
            var action = undoBuffer.Undo();
            var nce = (XmlNodeChangedEventArgs) action.State;
            switch (action.ActionType) {
                case XmlNodeChangedAction.Change:
                    if (nce.OldParent is XmlElement) nce.OldParent.FirstChild.Value = nce.OldValue;
                    else {
                        if (string.IsNullOrEmpty(nce.OldValue)) {
                            // I do this because when I set an attribute to an empty string, it causes a Validation error that will reset it back again. 
                            Undo();
                            return;
                        }
                        nce.OldParent.Value = nce.OldValue;
                    }
                    break;
                case XmlNodeChangedAction.Insert:
                    if (nce.Node is XmlAttribute) nce.NewParent.Attributes.Remove((XmlAttribute) nce.Node);
                    else nce.NewParent.RemoveChild(nce.Node);
                    break;
                case XmlNodeChangedAction.Remove:
                    if (nce.Node is XmlAttribute) InsertAttribute((XmlAttribute) nce.Node, nce.OldParent);
                    else InsertElement(nce.Node, nce.OldParent);
                    break;
            }
            if (isAtTop) undoBuffer.PushCurrentAction(action);
            isUndoRedoing = false;
        }

        public void CopyNode(XmlNode node) {
            if (node == null || (node is XmlElement && node.ParentNode == null)) return;
            CutPasteNode = node.Clone();
        }

        public void CutNode(XmlNode node, XmlNode parent) {
            if (node == null || (node is XmlElement && node.ParentNode == null)) return;
            CutPasteNode = node;
            RemoveNode(node, parent);
        }

        public void PasteNode(XmlNode node) {
            if (node == null || (node is XmlElement && node.ParentNode == null)) return;
            if (cutPasteNode is XmlAttribute) InsertAttribute((XmlAttribute) cutPasteNode.Clone(), node);
            else InsertElement(cutPasteNode.Clone(), node);
        }

        /// <summary>
        ///   Inserts a new comment.
        /// </summary>
        /// <param name="node">Select node.</param>
        /// <param name="xmlNode">Parent node.</param>
        public void InsertComment(XmlNode node, XmlNode xmlNode) {
            if (node == null) return;
            if (node is XmlElement) node.PrependChild(document.CreateComment(string.Empty));
            else if (node is XmlAttribute) {
                var attribute = (XmlAttribute) node;
                if (attribute.OwnerElement != null) attribute.OwnerElement.PrependChild(document.CreateComment(string.Empty));
            }
        }

        public void DeleteXmlNode(XmlNode node, XmlNode parent) {
            if (node == null || (node is XmlElement && node.ParentNode == null)) return;
            RemoveNode(node, parent);
        }

        /// <summary>
        ///   Insert a new node into the DOM
        /// </summary>
        /// <param name="node">
        ///   New node type to insert
        /// </param>
        /// <param name="parent">Parent</param>
        /// <returns>Newly created node</returns>
        public void InsertXmlNode(XmlSchemaObject node, XmlElement parent) {
            isInsertingNodesRecursively = true;
            var newNode = InsertNodeRecursively(node, parent);
            isInsertingNodesRecursively = false;
            var e = new XmlNodeChangedEventArgs(newNode, null,
                                                newNode.ParentNode ?? ((XmlAttribute) newNode).OwnerElement,
                                                string.Empty, string.Empty, XmlNodeChangedAction.Insert);
            if (!isUndoRedoing) undoBuffer.Do(new Memento {ActionType = e.Action, State = e});
            OnNodeChanged(e);
        }

        public void SaveXml(string filename) {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(filename, "Filename cannot be empty!");
            isDirty = false;
            undoBuffer.Flush();
            document.Save(filename);
        }

        #endregion Public Methods

        #region Events

        public event XmlNodeChangedEventHandler NodeChanged;

        #endregion Events

        /*
        private MemoryStream CreateXmlSampleFile(string xsdFile) {
	        isDirty = true;
            var schemas = new XmlSchemaSet();
	        schemas.Add(null, xsdFile);
	        var memStream = new MemoryStream();
	        var textWriter = new XmlTextWriter(memStream, null) { Formatting = Formatting.Indented };
	        //var textWriter2 = new XmlTextWriter("Sample.xml", null) { Formatting = Formatting.Indented };
	        var genr = new XmlSampleGenerator.XmlSampleGenerator(schemas, XmlQualifiedName.Empty) {
	                                                                                                  HideOptionalElements = true,
	                                                                                                  MaxThreshold = 1,
	                                                                                                  ListLength = 1
	                                                                                              };
	        //genr.WriteXml(textWriter2);
	        genr.WriteXml(textWriter);
	        return memStream;
	    }

*/
    }
}