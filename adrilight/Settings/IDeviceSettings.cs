using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using adrilight.Settings;
using System.Windows;
using System.Collections.ObjectModel;
using System.Drawing;

namespace adrilight
{
    public interface IDeviceSettings : INotifyPropertyChanged
    {

        string DeviceName { get; set; }
        string DeviceSerial { get; set; }
        DeviceType DeviceType { get; set; }
        string Manufacturer { get; set; }
        string DeviceDescription { get; set; }
        string FirmwareVersion { get; set; }
        string HardwareVersion { get; set; }
        string ProductionDate { get; set; }
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        string OutputPort { get; set; }
        bool IsTransferActive { get; set; }
        bool IsLoadingProfile { get; set; }
        List<IDeviceController> AvailableControllers { get; set; }
        IDeviceController CurrentActiveController { get; }
        int CurrentActiveControlerIndex { get; set; }
        ObservableCollection<IControlZone> CurrentLiveViewZones { get; }
        ObservableCollection<IControlZone> AvailableControlZones { get; }
        ObservableCollection<ControlZoneGroup> CurrentLiveViewGroup { get; }
        string DeviceThumbnail { get;}
        string DeviceOutputMap { get;}
        int Baudrate { get; set; }
        string DeviceUID { get; set; }
        bool IsSelected { get; set; }
        bool IsSizeNeedUserDefine { get; set; }
        int DashboardWidth { get; set; }
        int DashboardHeight { get; set; }
        ObservableCollection<ControlZoneGroup> ControlZoneGroups { get; set; }
        string RequiredFwVersion { get; set; }
        void RefreshFirmwareVersion();
        DeviceStateEnum DeviceState { get; set; }
        Rect CurrentLivewItemsBound { get; }
        ISlaveDevice[] AvailableLightingDevices { get; }
        IOutputSettings[] AvailableLightingOutputs { get; }
        IOutputSettings[] AvailablePWMOutputs { get; }
        ISlaveDevice[] AvailablePWMDevices { get; }
        void UpdateChildSize();
        void HandleResolutionChange(double scaleX,double scaleY);
        void BrightnessUp(int value);
        void BrightnessDown(int value);
        void SpeedUp(int value);
        void SpeedDown(int value);
    }
}
