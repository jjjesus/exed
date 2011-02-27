using System.Collections.ObjectModel;

namespace XmlEditor.Applications.Services
{
    public interface IMostRecentlyUsedFilesService
    {
        ObservableCollection<string> MRU { get; }

        void Opened(string fileName);

    }
}
