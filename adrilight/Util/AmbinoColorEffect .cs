using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public class AmbinoColorEffect : IAmbinoColorEffect
    {
        public string Name { get; set; }
        public string Creator { get; set; }
        public IColorPalette ColorPalette { get; set; }
        public Spots.ILEDSetup[] OutputLEDSetup { get; set; } // only take VID
        public string TargetType { get; set; } // target DeviceType
        public string Description { get; set; }
        public int EffectSpeed { get; set; }
        public string EffectVersion { get; set; }
       
        public string Geometry { get; set; }

    }
}
