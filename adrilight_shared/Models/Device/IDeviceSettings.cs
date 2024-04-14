﻿using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace adrilight_shared.Models.Device
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
        string DeviceOutputMap { get; }
        int Baudrate { get; set; }
        bool CustomBaudrateEnable { get; set; }
        string DeviceUID { get; set; }
        bool IsSelected { get; set; }
        bool IsSizeNeedUserDefine { get; set; }
        bool IsIndicatorLEDOn { get; set; }
        bool NoSignalLEDEnable { get; set; }
        bool AutoConnect { get; set; }
        int NoSignalFanSpeed { get; set; }
        int DashboardWidth { get; set; }
        int DashboardHeight { get; set; }
        int MaxBrightnessCap { get; set; }
        ObservableCollection<ControlZoneGroup> ControlZoneGroups { get; set; }
        string RequiredFwVersion { get; set; }
        DeviceStateEnum DeviceState { get; set; }
        Rect CurrentLivewItemsBound { get; }
        ISlaveDevice[] AvailableLightingDevices { get; }
        IOutputSettings[] AvailableLightingOutputs { get; }
        IOutputSettings[] AvailablePWMOutputs { get; }
        ISlaveDevice[] AvailablePWMDevices { get; }
        void DeviceEnableChanged();
        void UpdateChildSize();
        void UpdateUID();
        void HandleResolutionChange(double scaleX, double scaleY);
        void BrightnessUp(int value);
        void BrightnessDown(int value);
        void SpeedUp(int value);
        void SpeedDown(int value);
        void TurnOffLED();
        void TurnOnLED();
        void ToggleOnOffLED();
        void SetStaticColor(ColorCard colors);
        void SetModeByEnumValue(LightingModeEnum value);
        void ActivateControlMode(LightingMode lightingMode);
        string DeviceFirmwareExtension { get; }

        ///hardware lighting///
        bool HWL_enable { get; set; }
        bool StatusLEDEnable { get; set; }
        byte HWL_returnafter { get; set; }
        byte HWL_effectMode { get; set; }
        byte HWL_effectSpeed { get; set; }
        byte HWL_effectIntensity { get; set; }
        byte HWL_brightness { get; set; }
        Color HWL_singleColor { get; set; }
        Color[] HWL_palette { get; set; }
        int HWL_version { get; set; }
    }
}
