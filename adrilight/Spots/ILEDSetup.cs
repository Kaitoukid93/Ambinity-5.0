using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Spots
{
    public interface ILEDSetup : INotifyPropertyChanged
    {



        string Name { get; set; }
        string Owner { get; set; }
        ObservableCollection<IDeviceSpot> Spots { get; set; }
        string TargetType { get; set; }
        string Description { get; set; }
        int MatrixWidth { get; set; }
        int MatrixHeight { get; set; }
        double PixelWidth { get; set; }
        double PixelHeight { get; set; }
        object Lock { get; }
        int SetupID { get; set; }    // to match with device ID
        void DimLED(float dimFactor);
    }

}

