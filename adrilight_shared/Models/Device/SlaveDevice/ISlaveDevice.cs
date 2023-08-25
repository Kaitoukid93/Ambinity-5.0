using adrilight.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace adrilight
{
    public interface ISlaveDevice : INotifyPropertyChanged
    {
        string Name { get; set; }
        int ParrentID { get; set; }
        string Owner { get; set; }
        string Thumbnail { get; }
        DeviceTypeEnum DesiredParrent { get; set; } // which hub or controller this slave device can be attached to
        SlaveDeviceTypeEnum DeviceType { get; set; }
        string Description { get; set; }
        DeviceType TargetDeviceType { get; set; }
        ObservableCollection<IControlZone> ControlableZones { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        void UpdateSizeByChild(bool withPoint);
        void RotateLEDSetup(double angleInDegrees);
        void ReflectLEDSetupVertical();
    }
}