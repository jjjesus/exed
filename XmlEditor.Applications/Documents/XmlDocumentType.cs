#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;
using System.Xml.Xsl;
using TreeListControl;
using TreeListControl.Resources;
using XmlEditor.Applications.Helpers;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.Documents
{
    public class XmlDocumentType : DocumentType
    {
        private const string XsdBaseDirectory = "XsdCache";
        private XslCompiledTransform xslt;
        private string xsltPath;
        private int documentCount;

        public XmlDocumentType() : base("XML document", ".xml") { SubTypes = XsdTypes; }

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

        private static string CreateDescription(string xsd) { return Path.GetFileNameWithoutExtension(xsd).ToUpperFirstLetter(); }

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
            PrintOrPreviewXmlDocument((MyXmlDocument)document, true);
        }

        protected override void PrintPreviewCore(IDocument document)
        {
            PrintOrPreviewXmlDocument((MyXmlDocument)document, false);
        }

        /// <summary>
        /// Prints or previews the XML document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="printDocument">if set to <c>true</c> [print document] else preview.</param>
        public void PrintOrPreviewXmlDocument(MyXmlDocument document, bool printDocument) {
            if (document == null || document.Content == null || document.Content.Document == null || string.IsNullOrEmpty(document.Content.Document.OuterXml)) return;
            var printDialog = new PrintDialog();
            
            var settings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
            var tempFile = Path.GetTempFileName();
            using (var writer = XmlWriter.Create(tempFile, settings))
            {
                document.Content.Document.WriteTo(writer);
                if (writer != null) writer.Close();
            }

            var doc = new FlowDocument{
                                          FontFamily = new FontFamily("Courier New"), FontSize = 10,
                                          PageHeight = printDialog.PrintableAreaHeight, PageWidth = printDialog.PrintableAreaWidth,
                                          PagePadding = new Thickness(25), ColumnGap = 0
                                      };
            
            doc.ColumnWidth = (doc.PageWidth - doc.ColumnGap - doc.PagePadding.Left - doc.PagePadding.Right);
            using (var reader = new StreamReader(tempFile))
            {
                doc.Blocks.Add(new Paragraph(new Run(reader.ReadToEnd())));
            }
            var paginator = ((IDocumentPaginatorSource) doc).DocumentPaginator;
            File.Delete(tempFile);

            if (printDocument) {
                if (printDialog.ShowDialog() == true) printDialog.PrintDocument(paginator, document.FileName);
                return;
            }

            using (var xpsDocument = new XpsDocument(tempFile, FileAccess.ReadWrite))
            {
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                writer.Write(paginator);

                var previewWindow = new PrintPreview {Document = xpsDocument.GetFixedDocumentSequence()};
                previewWindow.ShowDialog();
            }

        }

        /// <summary>
        /// Create a new name of the file.
        /// </summary>
        /// <param name="xsd">The XSD.</param>
        /// <returns></returns>
        protected string NewFileName(string xsd) {
            if (!string.IsNullOrEmpty(xsd)) xsd = CreateDescription(xsd) + " ";
            return string.Format(CultureInfo.CurrentCulture, "{0:yyyyMMdd} {1}{2}{3}", DateTime.Now, xsd, ++documentCount, FileExtension);
        }

        /// <summary>
        /// Loads the XSL.
        /// </summary>
        protected void LoadXsl()
        {
            xslt = new XslCompiledTransform();
            try
            {
                if (string.IsNullOrEmpty(xsltPath)) xslt.Load("PrettyPrint.xslt", XsltSettings.TrustedXslt, new EmbeddedResourceResolver());
                else xslt.Load(xsltPath, XsltSettings.TrustedXslt, new XmlUrlResolver());
            }
            catch (SystemException e)
            {
                xsltPath = string.Empty;
                LoadXsl();
            }
        }

        /// <summary>
        ///   Transforms the XML document based on the currently supplied XSLT.
        /// </summary>
        /// <seealso cref="http://www.tkachenko.com/blog/archives/000653.html"/>
        protected WebBrowser TransformDocument(MyXmlDocument document)
        {
            if (document == null) return null;
            var stream = new MemoryStream();
            if (xslt == null) LoadXsl();
            if (xslt == null) return null;
            var webBrowser = new WebBrowser();
            try
            {
                xslt.Transform(document.Content.Document, null, stream);
                webBrowser.NavigateToStream(stream);
            }
            catch (SystemException e)
            {
                xslt = null;
                xsltPath = string.Empty;
            }
            return webBrowser;
        }

    }
}