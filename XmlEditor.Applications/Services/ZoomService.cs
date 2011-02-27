#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Waf.Foundation;

#endregion

namespace XmlEditor.Applications.Services
{
    [Export(typeof (IZoomService))]
    public class ZoomService : Model, IZoomService
    {
        private readonly List<double> defaultZooms;
        private readonly ReadOnlyCollection<double> readOnlyDefaultZooms;
        private double activeZoom;

        [ImportingConstructor]
        public ZoomService() {
            defaultZooms = new List<double> {3, 2.5, 2, 1.75, 1.5, 1.25, 1, 0.75, 0.5, 0.25};
            readOnlyDefaultZooms = new ReadOnlyCollection<double>(defaultZooms);
            activeZoom = 1;
        }

        #region IZoomService Members

        public IEnumerable<double> DefaultZooms {
            get { return readOnlyDefaultZooms; }
        }

        public double ActiveZoom {
            get { return activeZoom; }
            set {
                if (activeZoom != value) {
                    activeZoom = value;
                    RaisePropertyChanged("ActiveZoom");
                }
            }
        }

        #endregion
    }
}