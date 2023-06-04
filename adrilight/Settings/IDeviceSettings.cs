﻿using adrilight.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

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
        string DeviceThumbnail { get; }
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
        void HandleResolutionChange(double scaleX, double scaleY);
        void BrightnessUp(int value);
        void BrightnessDown(int value);
        void SpeedUp(int value);
        void SpeedDown(int value);
        void ActivateProfile(IDeviceSettings profile);
    }
}
