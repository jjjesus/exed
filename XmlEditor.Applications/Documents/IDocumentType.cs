using System.Collections.Generic;

namespace XmlEditor.Applications.Documents
{
    public interface IDocumentType
    {
        string Description { get; }

        string FileExtension { get; }

        /// <summary>
        /// In some cases, a document type may have several subtypes that need to be initialized in different ways, e.g. an XML editor using different XSD
        /// </summary>
        List<DocumentSubType> SubTypes { get; }

        bool CanNew();

        IDocument New();
        
        IDocument New(string subType);

        bool CanOpen();

        IDocument Open(string fileName);

        bool CanSave(IDocument document);

        void Save(IDocument document, string fileName);

        void Print(IDocument document);

        void PrintPreview(IDocument document);
    }
}