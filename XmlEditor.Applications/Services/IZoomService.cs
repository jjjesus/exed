#region

using System.Collections.Generic;
using System.ComponentModel;

#endregion

namespace XmlEditor.Applications.Services
{
    public interface IZoomService : INotifyPropertyChanged
    {
        IEnumerable<double> DefaultZooms { get; }

        double ActiveZoom { get; set; }
    }
}