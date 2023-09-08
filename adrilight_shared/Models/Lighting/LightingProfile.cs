using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.Lighting
{
    public class LightingProfile
    {
        public LightingProfile()
        {

        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ownder { get; set; }
        IControlMode ControlMode { get; set; }
        public List<string> TargetDevicesUID { get; set; }
        [JsonIgnore]
        public ObservableCollection<IDeviceSettings> TargetDevices { get; set; }
    }
}
