using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.AppProfile
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
