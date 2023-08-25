using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Spots
{
    public interface IDeviceSpotSet
    {
        
        ILEDSetup LEDSetup { get; set; }  
        object Lock { get; }
    }
}
