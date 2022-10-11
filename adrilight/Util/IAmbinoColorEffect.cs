using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IAmbinoColorEffect
    {
        string Name { get; set; }
        string Creator { get; set; }
        IColorPalette ColorPalette { get; set; }
        Spots.ILEDSetup[] OutputLEDSetup { get; set; } // only take VID
        string TargetType { get; set; } // target DeviceType
        
        string Description { get; set; }
        int EffectSpeed { get; set; }
        string EffectVersion { get; set; }
        string Geometry { get; set; }


    }
}
