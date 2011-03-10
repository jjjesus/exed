#region

using System;
using TreeListControl;

#endregion

namespace XmlEditor.Applications.Documents
{
    public class MyXmlDocument : Document
    {
        private readonly XmlModel content;

        public MyXmlDocument(XmlDocumentType documentType) : this(documentType, new XmlModel()) { }

        public MyXmlDocument(XmlDocumentType documentType, string subType) : this(documentType, new XmlModel(subType)) {
            Modified = true;
            SubType = subType;
        }

        public MyXmlDocument(XmlDocumentType documentType, XmlModel content)
            : base(documentType) {
            this.content = content;
            if (content.SchemaLocation != null) SubType = content.SchemaLocation.AbsolutePath;
        }

        public string SubType { get; set; }

        public XmlModel Content
        {
            get { return content; }
        }

        public XmlModel CloneContent() {
            throw new NotImplementedException("Cloning of XmlModels hasn't been implemented yet"); 
            //var clone = new XmlModel();

            //using (var stream = new MemoryStream()) {
            //    var source = new TextRange(Content.ContentStart, Content.ContentEnd);
            //    source.Save(stream, DataFormats.Xaml);
            //    var target = new TextRange(clone.ContentStart, clone.ContentEnd);
            //    target.Load(stream, DataFormats.Xaml);
            //}

            //return clone;
        }

    }
}