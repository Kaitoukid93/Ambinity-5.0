using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Spots
{
    public interface ILEDSetup
    {



        string Name { get; set; }
        string Owner { get; set; }
        IDeviceSpot[] Spots { get; set; }
        string TargetType { get; set; }
        string Description { get; set; }
        int MatrixWidth { get; set; }
        int MatrixHeight { get; set; }
        int PixelWidth { get; set; }
        int PixelHeight { get; set; }
        object Lock { get; }
        int SetupID { get; set; }    // to match with device ID
        void DimLED(float dimFactor);
    }

}

