#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Waf.Foundation;

#endregion

namespace XmlEditor.Applications.Documents
{
    public abstract class DocumentType : Model, IDocumentType
    {
        protected DocumentType(string description, string fileExtension) {
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("description must not be null or empty.");
            if (string.IsNullOrEmpty(fileExtension)) throw new ArgumentException("fileExtension must not be null or empty");
            if (fileExtension[0] != '.') throw new ArgumentException("The argument fileExtension must start with the '.' character.");

            Description = description; 
            FileExtension = fileExtension;
        }

        #region IDocumentType Members

        public string Description { get; private set; }

        public string FileExtension { get; private set; }

        public List<DocumentSubType> SubTypes { get; set; }

        public virtual bool CanNew() {
            return false;
        }

        public IDocument New() {
            if (!CanNew()) throw new NotSupportedException("The New operation is not supported. CanNew returned false.");

            return NewCore();
        }

        public IDocument New(string subType)
        {
            if (!CanNew()) throw new NotSupportedException("The New operation is not supported. CanNew returned false.");

            return NewCore(subType);
        }

        public virtual bool CanOpen()
        {
            return false;
        }

        public IDocument Open(string fileName) {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("fileName must not be null or empty.");
            if (!CanOpen()) throw new NotSupportedException("The Open operation is not supported. CanOpen returned false.");

            var document = OpenCore(fileName);
            if (document != null) document.FileName = fileName;
            return document;
        }

        public virtual bool CanSave(IDocument document) {
            return false;
        }

        public void Save(IDocument document, string fileName) {
            if (document == null) throw new ArgumentNullException("document");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("fileName must not be null or empty.");
            if (!CanSave(document)) throw new NotSupportedException("The Save operation is not supported. CanSave returned false.");

            SaveCore(document, fileName);

            if (!CanOpen()) return;
            document.FileName = fileName;
            document.Modified = false;
            document.ModifiedOn = File.GetLastWriteTime(fileName);
        }

        public void Print(IDocument document) { PrintCore(document); }
        public void PrintPreview(IDocument document) { PrintPreviewCore(document); }

        #endregion

        protected virtual void PrintCore(IDocument document) {
            throw new NotImplementedException();
        }

        protected virtual void PrintPreviewCore(IDocument document) {
            throw new NotImplementedException();
        }

        protected virtual IDocument NewCore() {
            throw new NotImplementedException();
        }

        protected virtual IDocument NewCore(string xsd) {
            throw new NotImplementedException();
        }

        protected virtual IDocument OpenCore(string fileName) {
            throw new NotImplementedException();
        }

        protected virtual void SaveCore(IDocument document, string fileName) {
            throw new NotImplementedException();
        }

    }
}