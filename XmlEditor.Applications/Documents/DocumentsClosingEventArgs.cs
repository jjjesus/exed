#region

using System.Collections.Generic;
using System.ComponentModel;

#endregion

namespace XmlEditor.Applications.Documents
{
    public class DocumentsClosingEventArgs : CancelEventArgs
    {
        private readonly IEnumerable<IDocument> documents;

        public DocumentsClosingEventArgs(IEnumerable<IDocument> documents) {
            this.documents = documents;
        }

        public IEnumerable<IDocument> Documents {
            get { return documents; }
        }
    }
}