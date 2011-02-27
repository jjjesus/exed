#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Waf.Applications.Services;
using System.Waf.Foundation;
using XmlEditor.Applications.Services;

#endregion

namespace XmlEditor.Applications.Documents
{
    [Export(typeof (IDocumentManager))] public class DocumentManager : Model, IDocumentManager
    {
        private readonly ObservableCollection<IDocumentType> documentTypes;
        private readonly ObservableCollection<IDocument> documents;
        private readonly IFileDialogService fileDialogService;
        private readonly IMostRecentlyUsedFilesService mruService;
        private readonly ReadOnlyObservableCollection<IDocumentType> readOnlyDocumentTypes;
        private readonly ReadOnlyObservableCollection<IDocument> readOnlyDocuments;
        private IDocument activeDocument;

        [ImportingConstructor] public DocumentManager(IFileDialogService fileDialogService,
                                                      IMostRecentlyUsedFilesService mruService) {
            if (fileDialogService == null) throw new ArgumentNullException("fileDialogService");
            if (mruService == null) throw new ArgumentNullException("mruService");
            this.mruService = mruService;

            this.fileDialogService = fileDialogService;
            documentTypes = new ObservableCollection<IDocumentType>();
            readOnlyDocumentTypes = new ReadOnlyObservableCollection<IDocumentType>(documentTypes);
            documents = new ObservableCollection<IDocument>();
            readOnlyDocuments = new ReadOnlyObservableCollection<IDocument>(documents);
        }

        #region IDocumentManager Members

        public ReadOnlyObservableCollection<IDocumentType> DocumentTypes {
            get { return readOnlyDocumentTypes; }
        }

        public ObservableCollection<string> MRU {
            get { return mruService.MRU; }
        }

        public ReadOnlyObservableCollection<IDocument> Documents {
            get { return readOnlyDocuments; }
        }

        public IDocument ActiveDocument {
            get { return activeDocument; }
            set {
                if (activeDocument == value) return;
                if (value != null && !documents.Contains(value)) throw new ArgumentException("value is not an item of the Documents collection.");
                activeDocument = value;
                RaisePropertyChanged("ActiveDocument");
            }
        }

        public event EventHandler<DocumentsClosingEventArgs> DocumentsClosing;

        public void Register(IDocumentType documentType) {
            if (documentType == null) throw new ArgumentNullException("documentType");
            documentTypes.Add(documentType);
        }

        public void Deregister(IDocumentType documentType) {
            if (documentType == null) throw new ArgumentNullException("documentType");
            if (documents.Any(d => d.DocumentType == documentType))
                throw new InvalidOperationException(
                    "It's not possible to deregister a document type which is still used by some documents.");
            documentTypes.Remove(documentType);
        }

        public IDocument New(IDocumentType documentType) {
            if (documentType == null) throw new ArgumentNullException("documentType");
            if (!documentTypes.Contains(documentType)) throw new ArgumentException("documentType is not an item of the DocumentTypes collection.");
            var document = documentType.New();
            documents.Add(document);
            ActiveDocument = document;
            return document;
        }

        public IDocument New(IDocumentType documentType, string subType) {
            if (documentType == null) throw new ArgumentNullException("documentType");
            if (string.IsNullOrEmpty(subType)) throw new ArgumentNullException("subType");
            if (!documentTypes.Contains(documentType)) throw new ArgumentException("documentType is not an item of the DocumentTypes collection.");
            var document = documentType.New(subType);
            documents.Add(document);
            ActiveDocument = document;
            return document;
        }

        public IDocument Open(string fileName) {
            FileType fileType;
            if (string.IsNullOrEmpty(fileName)) {
                var fileTypes = from d in documentTypes
                                where d.CanOpen()
                                select new FileType(d.Description, d.FileExtension);
                if (!fileTypes.Any())
                    throw new InvalidOperationException(
                        "No DocumentType is registered that supports the Open operation.");

                var result = fileDialogService.ShowOpenFileDialog(fileTypes);
                if (!result.IsValid) return null;
                fileName = result.FileName;
                fileType = result.SelectedFileType;
            }
            else fileType = new FileType("MRU file", Path.GetExtension(fileName));
            // Check if document is already opened
            var document = documents.SingleOrDefault(d => d.FileName == fileName);
            if (document == null) {
                var documentType = GetDocumentType(fileType);
                document = documentType.Open(fileName);
                documents.Add(document);
                mruService.Opened(fileName);
            }
            ActiveDocument = document;
            return document;
        }

        public void Save(IDocument document) {
            if (document == null) throw new ArgumentNullException("document");
            if (!documents.Contains(document)) throw new ArgumentException("document is not an item of the Documents collection.");

            if (Path.IsPathRooted(document.FileName)) {
                var saveTypes = documentTypes.Where(d => d.CanSave(document));
                var documentType = saveTypes.First(d => d.FileExtension == Path.GetExtension(document.FileName));
                documentType.Save(document, document.FileName);
            }
            else SaveAs(document);
        }

        public void SaveAs(IDocument document) {
            if (document == null) throw new ArgumentNullException("document");
            if (!documents.Contains(document)) throw new ArgumentException("document is not an item of the Documents collection.");

            var fileTypes = from d in documentTypes
                            where d.CanSave(document)
                            select new FileType(d.Description, d.FileExtension);
            if (!fileTypes.Any()) throw new InvalidOperationException("No DocumentType is registered that supports the Save operation.");

            FileType selectedFileType;
            if (File.Exists(document.FileName)) {
                var saveTypes = documentTypes.Where(d => d.CanSave(document));
                var documentType = saveTypes.First(d => d.FileExtension == Path.GetExtension(document.FileName));
                selectedFileType =
                    fileTypes.Where(
                        f => f.Description == documentType.Description && f.FileExtension == documentType.FileExtension)
                        .First();
            }
            else selectedFileType = fileTypes.First();
            var fileName = Path.GetFileNameWithoutExtension(document.FileName);

            var result = fileDialogService.ShowSaveFileDialog(fileTypes, selectedFileType, fileName);
            if (result.IsValid) {
                var documentType = GetDocumentType(result.SelectedFileType);
                documentType.Save(document, result.FileName);
                mruService.Opened(result.FileName);
            }
        }

        public void Print(IDocument document) {
            if (document == null) throw new ArgumentNullException("document");
            if (!documents.Contains(document)) throw new ArgumentException("document is not an item of the Documents collection.");
         
            document.DocumentType.Print(document);
        }

        public void PrintPreview(IDocument document) {
            if (document == null) throw new ArgumentNullException("document");
            if (!documents.Contains(document)) throw new ArgumentException("document is not an item of the Documents collection.");

            document.DocumentType.PrintPreview(document);            
        }

        public bool Close(IDocument document) {
            if (document == null) throw new ArgumentNullException("document");
            if (!documents.Contains(document)) throw new ArgumentException("document is not an item of the Documents collection.");

            var eventArgs = new DocumentsClosingEventArgs(new[] {document});
            OnDocumentsClosing(eventArgs);
            if (eventArgs.Cancel) return false;

            if (ActiveDocument == document) ActiveDocument = null;
            documents.Remove(document);
            return true;
        }

        public bool CloseAll() {
            var eventArgs = new DocumentsClosingEventArgs(Documents);
            OnDocumentsClosing(eventArgs);
            if (eventArgs.Cancel) return false;

            ActiveDocument = null;
            while (documents.Any()) documents.Remove(documents.First());
            return true;
        }

        #endregion

        protected virtual void OnDocumentsClosing(DocumentsClosingEventArgs e) { if (DocumentsClosing != null) DocumentsClosing(this, e); }

        private IDocumentType GetDocumentType(FileType fileType) {
            try {
                var documentType = (from d in documentTypes
                                    where
                                        d.Description == fileType.Description &&
                                        d.FileExtension == fileType.FileExtension
                                    select d).First();
                return documentType;
            }
            catch {
                return (from d in documentTypes where d.FileExtension == fileType.FileExtension select d).First();
            }
        }
    }
}