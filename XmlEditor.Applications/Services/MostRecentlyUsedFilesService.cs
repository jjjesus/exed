#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Waf.Foundation;
using XmlEditor.Applications.Properties;

#endregion

namespace XmlEditor.Applications.Services
{
    /// <summary>
    /// A simple class that maintains a list of most recently used files.
    /// Requires access to the Properties.Settings.Default, specically the MRU StringCollection and the NoOfFilesInMRU int.
    /// </summary>
    [Export(typeof (IMostRecentlyUsedFilesService))]
    public class MostRecentlyUsedFilesService : Model, IMostRecentlyUsedFilesService
    {
        [ImportingConstructor]
        public MostRecentlyUsedFilesService() {
            try {
                if (Settings.Default.MRU == null) Settings.Default.MRU = new StringCollection();
                MRU = new ObservableCollection<string>(Settings.Default.MRU.Cast<string>().ToList());

                for (var i = MRU.Count() - 1; i >= 0; i--) {
                    if (File.Exists(MRU[i])) continue;
                    MRU.RemoveAt(i);
                }
            }
            catch (SystemException e) {
                Console.WriteLine(e.Message);
            }
        }

        #region IMostRecentlyUsedFilesService Members

        public ObservableCollection<string> MRU { get; private set; }

        public void Opened(string fileName) {
            if (MRU.Contains(fileName)) MRU.Remove(fileName);
            MRU.Insert(0, fileName);
            while (MRU.Count > Settings.Default.NoOfFilesInMRU) MRU.RemoveAt(MRU.Count - 1);
        }

        #endregion

        ~MostRecentlyUsedFilesService() {
            Settings.Default.MRU.Clear();
            foreach (var file in MRU)
                Settings.Default.MRU.Add(file);
            Settings.Default.Save();
        }
    }
}