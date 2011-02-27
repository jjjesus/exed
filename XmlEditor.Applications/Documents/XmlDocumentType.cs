#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TreeListControl;
using TreeListControl.Resources;
using XmlEditor.Applications.Helpers;

#endregion

namespace XmlEditor.Applications.Documents
{
    public class XmlDocumentType : DocumentType
    {
        private const string XsdBaseDirectory = "XsdCache";

        private int documentCount;

        public XmlDocumentType() : base("Scenario document", ".xml") { SubTypes = XsdTypes; }

        private List<DocumentSubType> XsdTypes {
            get {
                var xsdTypes = new List<DocumentSubType>();
                var xsdCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, XsdBaseDirectory);
                if (Directory.Exists(xsdCacheDirectory))
                    xsdTypes.AddRange(
                        Directory.GetFiles(xsdCacheDirectory, "*.xsd").Where(Utils.SchemaSetContainsRootElement).Select(
                            xsd => new DocumentSubType {
                                                    Description = CreateDescription(xsd),
                                                    Tag = xsd,
                                                    Type = this
                                                }));
                return xsdTypes;
            }
        }

        private static string CreateDescription(string xsd) { return Path.GetFileNameWithoutExtension(xsd).ToUpperFirstLetter() + " xml document"; }

        public override bool CanNew() { return true; }

        public override bool CanOpen() { return true; }

        public override bool CanSave(IDocument document) { return document is MyXmlDocument; }

        protected override IDocument NewCore() {
            var document = new MyXmlDocument(this) { FileName = NewFileName(string.Empty) };
            return document;
        }

        protected override IDocument NewCore(string xsd) {
            var document = new MyXmlDocument(this, xsd) { FileName = NewFileName(xsd) };
            return document;
        }

        protected override IDocument OpenCore(string fileName) {
            var document = new MyXmlDocument(this, new XmlModel(fileName));
            documentCount++;
            return document;
        }

        protected override void SaveCore(IDocument document, string fileName) {
            var xmlModel = ((MyXmlDocument) document).Content;
            xmlModel.SaveXml(fileName);
        }

        protected override void PrintCore(IDocument document)
        {
            var xmlModel = ((MyXmlDocument)document).Content;           
        }

        protected override void PrintPreviewCore(IDocument document)
        {
            var xmlModel = ((MyXmlDocument)document).Content;                       
        }

        protected string NewFileName(string xsd) {
            if (!string.IsNullOrEmpty(xsd)) xsd = CreateDescription(xsd) + " ";
            return string.Format(CultureInfo.CurrentCulture, "{0:yyyyMMdd} {1}{2}{3}", DateTime.Now, xsd, ++documentCount, FileExtension);
        }
    }
}