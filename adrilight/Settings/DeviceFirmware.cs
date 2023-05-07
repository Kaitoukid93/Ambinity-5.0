using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
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
