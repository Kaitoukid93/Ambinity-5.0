using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Settings
{
    public interface IDeviceFirmware : INotifyPropertyChanged
    {
        string Name { get; set; }
        string TargetHardware { get; set; }
        string TargetDeviceType { get; set; }
        string Version { get; set; }
        string ResourceName { get; set; }
        string Geometry { get; set; } 
    }
}
