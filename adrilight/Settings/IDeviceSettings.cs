﻿using adrilight.Spots;
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

        int DeviceID { get; set; }
        string DeviceName { get; set; }
        string DeviceSerial { get; set; }
        DeviceTypeEnum DeviceType { get; set; }
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
        List<IDeviceController> AvailableControllers { get; set; }
        IDeviceController CurrentActiveController { get; }
        ObservableCollection<IControlZone> CurrentLiveViewZones { get; }
        ObservableCollection<IControlZone> AvailableControlZones { get; }
        ObservableCollection<ControlZoneGroup> CurrentLiveViewGroup { get; }
        int CurrentActiveControlerIndex { get; set; }
        int SelectedOutput { get; set; }
        string Geometry { get; set; }
        string DeviceThumbnail { get; set; }
        string DeviceOutputMap { get; set; }
        string DeviceConnectionGeometry { get; set; }
        int Baudrate { get; set; }
        string DeviceUID { get; set; }
        string ActivatedProfileUID { get; set; }
        string DeviceConnectionType { get; set; }
        bool IsSelected { get; set; }
        bool IsSizeNeedUserDefine { get; set; }
        bool IsLoadingProfile { get; set; }
        void ActivateProfile(DeviceProfile profile);
        void SetOutput(IOutputSettings output, int outputID);
        bool IsLoadingSpeed { get; set; }
        string FwLocation { get; set; }
        int DashboardWidth { get; set; }
        int DashboardHeight { get; set; }
        ObservableCollection<ControlZoneGroup> ControlZoneGroups { get; set; }
        string RequiredFwVersion { get; set; }
        void RefreshFirmwareVersion();
        //void DeviceLocator(Color color);
        void RefreshDeviceActualSpeedAsync();
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
