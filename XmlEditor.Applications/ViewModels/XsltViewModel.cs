#region

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class XsltViewModel : ViewModel<IXsltView>
    {
        private readonly IFileDialogService fileDialogService;
        private readonly IMessageService messageService;
        private readonly DelegateCommand openFileCommand;
        private readonly IXsltView view;
        private MyXmlDocument document;
        private XslCompiledTransform xslt;

        private string xsltPath;

        [ImportingConstructor]
        public XsltViewModel(IFileDialogService fileDialogService, IMessageService messageService, IXsltView view)
            : base(view)
        {
            if (fileDialogService == null) throw new ArgumentNullException("fileDialogService");
            if (messageService == null) throw new ArgumentNullException("messageService");

            this.view = view;
            this.view.Loaded += delegate { TransformDocument(); };
            this.fileDialogService = fileDialogService;
            this.messageService = messageService;
            openFileCommand = new DelegateCommand(OpenXsltFile);
        }

        /// <summary>
        ///   Gets or sets the XSLT path to the *.xsl file.
        /// </summary>
        /// <value>The XSLT path.</value>
        public string XsltPath
        {
            get { return xsltPath; }
            set
            {
                if (xsltPath == value || !File.Exists(value)) return;
                xsltPath = value;
                LoadXsl();
                //xslt = new XslCompiledTransform();
                //xslt.Load(xsltPath, XsltSettings.TrustedXslt, new XmlUrlResolver());
                TransformDocument();
                RaisePropertyChanged("XsltPath");
            }
        }

        public DelegateCommand OpenFileCommand
        {
            get { return openFileCommand; }
        }

        public MyXmlDocument Document
        {
            get { return document; }
            set
            {
                if (document == value) return;
                document = value;
                DiscoverXslt();
                TransformDocument();
            }
        }

        /// <summary>
        ///   Opens the XSLT file.
        /// </summary>
        private void OpenXsltFile()
        {
            var result = fileDialogService.ShowOpenFileDialog(new FileType("XSLT transformation", ".xsl;*.xslt"));
            if (!result.IsValid) return;
            XsltPath = result.FileName;
        }

        /// <summary>
        ///   Discovers the XSLT file (*.xls) automatically, either in the XsdCache folder or in the current folder.
        /// </summary>
        private void DiscoverXslt()
        {
            var schemas = Document.Content.Document.Schemas;
            if (schemas.Count == 0) return;
            var path = string.Empty;
            foreach (XmlSchema schema in schemas.Schemas())
            {
                if (string.IsNullOrEmpty(schema.SourceUri) || !schema.SourceUri.Contains(".xsd")) continue;
                path = schema.SourceUri;
            }
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace(".xsd", ".xsl"); // path points to XSD directory
            if (!File.Exists(path))
                path = Path.Combine(Path.GetDirectoryName(Document.FileName), Path.GetFileName(path));
            // path points to current directory
            XsltPath = path;
        }

        /// <summary>
        /// Loads the XSL.
        /// </summary>
        private void LoadXsl() {
            xslt = new XslCompiledTransform();
            try {
                if (string.IsNullOrEmpty(xsltPath)) xslt.Load("PrettyPrint.xslt", XsltSettings.TrustedXslt, new EmbeddedResourceResolver());
                else xslt.Load(xsltPath, XsltSettings.TrustedXslt, new XmlUrlResolver());
            }
            catch (SystemException e) {
                messageService.ShowError(e.Message);
                xsltPath = string.Empty;
                RaisePropertyChanged("XsltPath");
                LoadXsl();
            }
        }

        /// <summary>
        ///   Transforms the XML document based on the currently supplied XSLT.
        /// </summary>
        /// <seealso cref="http://www.tkachenko.com/blog/archives/000653.html"/>
        public void TransformDocument()
        {
            if (Document == null) return;
            var stream = new MemoryStream();
            if (xslt == null) LoadXsl();
            if (xslt == null) {
                messageService.ShowError("Cannot resolve XSLT file");
                return;
            }
            try {
                xslt.Transform(Document.Content.Document, null, stream);
                view.NavigateToStream(stream);
            }
            catch (SystemException e) {
                messageService.ShowError(e.Message);
                xslt = null;
                xsltPath = string.Empty;
                RaisePropertyChanged("XsltPath");
            }
        }
    }

    public class EmbeddedResourceResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(GetType(), Path.GetFileName(absoluteUri.AbsolutePath));
        }
    }
}