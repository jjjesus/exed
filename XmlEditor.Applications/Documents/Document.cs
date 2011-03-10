#region

using System;
using System.Waf.Foundation;

#endregion

namespace XmlEditor.Applications.Documents
{
    public abstract class Document : Model, IDocument
    {
        private readonly IDocumentType documentType;
        private string fileName;
        private bool modified;
        private DateTime modifiedOn;

        protected Document(IDocumentType documentType) {
            if (documentType == null) throw new ArgumentNullException("documentType");
            this.documentType = documentType;
        }

        #region IDocument Members

        public IDocumentType DocumentType {
            get { return documentType; }
        }

        public string FileName {
            get { return fileName; }
            set {
                if (fileName == value) return;
                fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public bool Modified {
            get { return modified; }
            set {
                if (modified == value) return;
                modified = value;
                RaisePropertyChanged("Modified");
            }
        }

        public DateTime ModifiedOn {
            get { return modifiedOn; }
            set {
                if (modifiedOn == value) return;
                modifiedOn = value;
                RaisePropertyChanged("ModifiedOn");
            }
        }

        #endregion
    }
}