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

namespace adrilight
{
    public enum State { sleep, dfu, normal, surfaceEditor };
    public interface IDeviceSettings : INotifyPropertyChanged
    {

        int DeviceID { get; set; }
        string DeviceName { get; set; }
        string DeviceSerial { get; set; }
        string DeviceType { get; set; }
        string Manufacturer { get; set; }
        string DeviceDescription { get; set; }
        bool IsLoading { get; set; } // just a loading indicator to tell user to fking wait a bit
        string FirmwareVersion { get; set; }
        string HardwareVersion { get; set; }
        string ProductionDate { get; set; }
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        bool IsDummy { get; set; }
        string OutputPort { get; set; }
        bool IsTransferActive { get; set; }
        int ActivatedProfileIndex { get; set; }
        IDeviceController[] AvailableControllers { get; set; }
        IDeviceController CurrentActiveController { get; }
        ObservableCollection<IControlZone> CurrentLiveViewZones { get; }
        int CurrentActiveControlerIndex { get; set; }
        int SelectedOutput { get; set; }
        string Geometry { get; set; }
        string DeviceThumbnail { get; set; }
        string DeviceConnectionGeometry { get; set; }
        int Baudrate { get; set; }
        string DeviceUID { get; set; }
        string ActivatedProfileUID { get; set; }
        string DeviceConnectionType { get; set; }
        bool IsSelected { get; set; }
        bool IsSizeNeedUserDefine { get; set; }
        bool IsLoadingProfile { get; set; }
        void ActivateProfile(IDeviceProfile profile);
        void SetOutput(IOutputSettings output, int outputID);
        bool IsLoadingSpeed { get; set; }
        string FwLocation { get; set; }
        string RequiredFwVersion { get; set; }
        void RefreshFirmwareVersion();
        //void DeviceLocator(Color color);
        void RefreshDeviceActualSpeedAsync();
        State CurrentState { get; set; }
        Rect CurrentLivewItemsBound { get; }
        ISlaveDevice[] AvailableLightingDevices { get; }
        void UpdateChildSize();
        void BrightnessUp(int value);
        void BrightnessDown(int value);
        void SpeedUp(int value);
        void SpeedDown(int value);
    }
}
