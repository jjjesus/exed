#region

using System;
using System.ComponentModel;

#endregion

namespace XmlEditor.Applications.Documents
{
    public interface IDocument : INotifyPropertyChanged
    {
        IDocumentType DocumentType { get; }

        string FileName { get; set; }

        bool Modified { get; set; }
        
        DateTime ModifiedOn { get; set; }
    }
}