using adrilight.Settings;
using adrilight.Spots;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight
{
    public interface ISlaveDevice
    {
        string Name { get; set; }
        string Owner { get; set; }
        string Thumbnail { get; set; }
        DeviceTypeEnum DesiredParrent { get; set; } // which hub or controller this slave device can be attached to
        SlaveDeviceTypeEnum DeviceType { get; set; }
        string Description { get; set; }
        ObservableCollection<IControlZone> ControlableZones { get; set; }
        
        void UpdateSizeByChild(bool withPoint);
    }
}