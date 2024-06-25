using adrilight_shared.Models.Device.Zone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Services.LightingEngine.GlobalLighting
{
    public class GlobalLightingPreset
    {
        public GlobalLightingPreset() { }
        public ObservableCollection<ControlZone> Zones { get; set; }

    }
}
