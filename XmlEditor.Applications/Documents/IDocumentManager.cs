#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

#endregion

namespace XmlEditor.Applications.Documents
{
    public interface IDocumentManager : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IDocumentType> DocumentTypes { get; }

        ReadOnlyObservableCollection<IDocument> Documents { get; }

        IDocument ActiveDocument { get; set; }

        event EventHandler<DocumentsClosingEventArgs> DocumentsClosing;

        void Register(IDocumentType documentType);

        void Deregister(IDocumentType documentType);

        IDocument New(IDocumentType documentType);

        IDocument New(IDocumentType documentType, string subType);

        IDocument Open(string fileName);

        ObservableCollection<string> MRU { get; }

        bool Close(IDocument document);

        bool CloseAll();

        void Save(IDocument document);

        void SaveAs(IDocument document);

        void Print(IDocument activeDocument);

        void PrintPreview(IDocument activeDocument);
    }
}