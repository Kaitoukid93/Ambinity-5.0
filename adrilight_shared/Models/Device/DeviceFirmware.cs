using adrilight_shared.Enums;
using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Device
{
    public class DeviceFirmware : ViewModelBase
    {
        public string Name { get; set; }
        public string TargetHardware { get; set; }
        public DeviceTypeEnum TargetDeviceType { get; set; }
        public string Version { get; set; }
        public string ResourceName { get; set; }
        public string Geometry { get; set; }

    }

}
