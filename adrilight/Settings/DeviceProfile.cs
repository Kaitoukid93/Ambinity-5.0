using adrilight.Helpers;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class DeviceProfile : ViewModelBase
    {
        public DeviceProfile() { }
        public string Name { get; set; }       
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string ProfileUID { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }
        [JsonIgnore]
        public DeviceType DeviceType => DeviceSettings.DeviceType;
        public void SaveProfile(IDeviceSettings device)
        {
            device.IsLoadingProfile = true;
            DeviceSettings = ObjectHelpers.Clone<DeviceSettings>(device as DeviceSettings);
            device.IsLoadingProfile = false;

        }
    }
}
