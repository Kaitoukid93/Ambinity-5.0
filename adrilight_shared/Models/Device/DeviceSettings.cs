﻿using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.DashboardItem;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace adrilight_shared.Models.Device
{
    public class DeviceSettings : ViewModelBase, IDeviceSettings, IDashboardItem, IGenericCollectionItem
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string ResourceCollectionFolderPath => Path.Combine(JsonPath, "Resource");
        private string deviceDirectory => Path.Combine(DevicesCollectionFolderPath, DeviceName + "-" + DeviceUID);
        private string _deviceName;
        private string _deviceSerial;
        private string _manufacturer;
        private string _deviceDescription;
        private string _firmwareVersion;
        private string _hardwareVersion = "unknown";
        private List<IDeviceController> _availableControllers;
        private string _productionDate;
        private bool _isVisible;
        private bool _isEnabled = true;
        private string _outputPort;
        private bool _isTransferActive;
        private ObservableCollection<ControlZoneGroup> _controlZoneGroups;
        private int _dashboardWidth;
        private int _dashboardHeight;
        private int _baudrate = 1000000;
        private bool _customBaudrateEnable = false;
        private string _deviceUID;
        private bool _isSelected = false;
        private bool _isLoadingProfile = false;
        private DeviceStateEnum _deviceState = DeviceStateEnum.Normal;
        private string _requiredFwVersion;

        private bool _isSizeNeedUserDefine = false;
        private DeviceType _deviceType;
        private int _currentActiveControllerIndex;
        private IDeviceController _currentActiveController;

        private bool _hwl_enable;
        private bool _statusLEDEnable;
        private byte _hwl_returnafter;
        private byte _hwl_effectMode;
        private byte _hwl_effectSpeed;
        private byte _hwl_brightness;
        private Color _hwl_singleColor;
        private Color[] _hwl_palette;
        private byte _hwl_effectIntensity;
        private int _hwl_version;
        private int _hwl_MaxLEDPerOutput;

        private int _maxBrightnessCap = 80;
        public int DashboardWidth { get => _dashboardWidth; set { Set(() => DashboardWidth, ref _dashboardWidth, value); } }
        public int MaxBrightnessCap { get => _maxBrightnessCap; set { Set(() => MaxBrightnessCap, ref _maxBrightnessCap, value); } }
        public int DashboardHeight { get => _dashboardHeight; set { Set(() => DashboardHeight, ref _dashboardHeight, value); } }
        public bool CustomBaudrateEnable { get => _customBaudrateEnable; set { Set(() => CustomBaudrateEnable, ref _customBaudrateEnable, value); } }
        //dashboard display param
        private bool _isPinned = false;
        private bool _isChecked = false;
        public bool IsPinned { get => _isPinned; set { Set(() => IsPinned, ref _isPinned, value); } }
        public string Name { get; set; }
        [JsonIgnore]
        public bool IsEditing { get; set; }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
        public string InfoPath { get; set; }
        private bool _autoConnect = true;
        public bool AutoConnect { get => _autoConnect; set { Set(() => AutoConnect, ref _autoConnect, value); } }

        [JsonIgnore]
        public string DeviceThumbnail => File.Exists(Path.Combine(ResourceCollectionFolderPath, DeviceName + "_thumb.png")) ? Path.Combine(ResourceCollectionFolderPath, DeviceName + "_thumb.png") : Path.Combine(ResourceCollectionFolderPath, DeviceType.Name + "_thumb.png");
        [JsonIgnore]
        public string DeviceOutputMap => File.Exists(Path.Combine(ResourceCollectionFolderPath, DeviceName + "_outputmap.png")) ? Path.Combine(ResourceCollectionFolderPath, DeviceName + "_outputmap.png") : DeviceThumbnail;
        public DeviceType DeviceType { get => _deviceType; set { Set(() => DeviceType, ref _deviceType, value); } }
        public ObservableCollection<ControlZoneGroup> ControlZoneGroups { get => _controlZoneGroups; set { Set(() => ControlZoneGroups, ref _controlZoneGroups, value); } }
        public DeviceStateEnum DeviceState { get => _deviceState; set { Set(() => DeviceState, ref _deviceState, value); } }
        public string RequiredFwVersion { get => _requiredFwVersion; set { Set(() => RequiredFwVersion, ref _requiredFwVersion, value); } }
        public string DeviceName { get => _deviceName; set { Set(() => DeviceName, ref _deviceName, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => DeviceSerial, ref _deviceSerial, value); } }
        public string Manufacturer { get => _manufacturer; set { Set(() => Manufacturer, ref _manufacturer, value); } }
        public string FirmwareVersion { get => _firmwareVersion; set { Set(() => FirmwareVersion, ref _firmwareVersion, value); } }
        public string HardwareVersion { get => _hardwareVersion; set { Set(() => HardwareVersion, ref _hardwareVersion, value); } }
        public string ProductionDate { get => _productionDate; set { Set(() => ProductionDate, ref _productionDate, value); } }
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string OutputPort { get => _outputPort; set { Set(() => OutputPort, ref _outputPort, value); } }
        [JsonIgnore]
        public bool IsTransferActive { get => _isTransferActive; set { Set(() => IsTransferActive, ref _isTransferActive, value); } }
        public int Baudrate { get => _baudrate; set { Set(() => Baudrate, ref _baudrate, value); } }
        public string DeviceDescription { get => _deviceDescription; set { Set(() => DeviceDescription, ref _deviceDescription, value); } }
        public string DeviceUID { get => _deviceUID; set { Set(() => DeviceUID, ref _deviceUID, value); } }
        public bool IsSizeNeedUserDefine { get => _isSizeNeedUserDefine; set { Set(() => IsSizeNeedUserDefine, ref _isSizeNeedUserDefine, value); } }
        public List<IDeviceController> AvailableControllers { get => _availableControllers; set { Set(() => AvailableControllers, ref _availableControllers, value); } }
        [JsonIgnore]
        public bool IsLoadingProfile { get => _isLoadingProfile; set { Set(() => IsLoadingProfile, ref _isLoadingProfile, value); IsLoadingProfilePropertyChanged(); } }
        [JsonIgnore]
        public IDeviceController CurrentActiveController { get => AvailableControllers[CurrentActiveControlerIndex]; set { Set(() => CurrentActiveController, ref _currentActiveController, value); } }

        [JsonIgnore]
        public Rect CurrentLivewItemsBound => GetDeviceRectBound(CurrentLiveViewZones.ToArray());

        [JsonIgnore]
        public ObservableCollection<IControlZone> CurrentLiveViewZones => GetControlZones(CurrentActiveController);
        [JsonIgnore]
        public ObservableCollection<IControlZone> AvailableControlZones => GetControlZones();
        [JsonIgnore]
        public ObservableCollection<ControlZoneGroup> CurrentLiveViewGroup => GetControlZoneGroups(CurrentActiveController);
        [JsonIgnore]
        public ISlaveDevice[] AvailableLightingDevices => GetSlaveDevices(ControllerTypeEnum.LightingController);
        [JsonIgnore]
        public IOutputSettings[] AvailableLightingOutputs => GetOutput(ControllerTypeEnum.LightingController);
        [JsonIgnore]
        public IOutputSettings[] AvailablePWMOutputs => GetOutput(ControllerTypeEnum.PWMController);
        [JsonIgnore]
        public ISlaveDevice[] AvailablePWMDevices => GetSlaveDevices(ControllerTypeEnum.PWMController);
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); DeviceEnableChanged(); } }
        [JsonIgnore]
        public bool DeviceHardwareControlEnable => (DeviceType.Type == DeviceTypeEnum.AmbinoBasic || DeviceType.Type == DeviceTypeEnum.AmbinoEDGE || DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3);
        [JsonIgnore]
        public bool DeviceFanSpeedControlEnable => (DeviceType.Type == DeviceTypeEnum.AmbinoFanHub);
        [JsonIgnore]
        public bool IsIndicatorLEDOn { get; set; }
        [JsonIgnore]
        public bool NoSignalLEDEnable { get; set; }
        [JsonIgnore] 
        public int NoSignalFanSpeed { get; set; }
        [JsonIgnore]
        public string DeviceFirmwareExtension => GetDeviceFirmwareExtensionString();
        [JsonIgnore]
        public bool HWL_enable { get => _hwl_enable; set { Set(() => HWL_enable, ref _hwl_enable, value); } }
        [JsonIgnore]
        public bool StatusLEDEnable { get => _statusLEDEnable; set { Set(() => StatusLEDEnable, ref _statusLEDEnable, value); } }
        [JsonIgnore]
        public byte HWL_returnafter { get => _hwl_returnafter; set { Set(() => HWL_returnafter, ref _hwl_returnafter, value); } }
        [JsonIgnore]
        public byte HWL_effectMode { get => _hwl_effectMode; set { Set(() => HWL_effectMode, ref _hwl_effectMode, value); } }
        [JsonIgnore]
        public byte HWL_effectSpeed { get => _hwl_effectSpeed; set { Set(() => HWL_effectSpeed, ref _hwl_effectSpeed, value); } }
        [JsonIgnore]
        public byte HWL_brightness { get => _hwl_brightness; set { Set(() => HWL_brightness, ref _hwl_brightness, value); } }
        [JsonIgnore]
        public Color HWL_singleColor { get => _hwl_singleColor; set { Set(() => HWL_singleColor, ref _hwl_singleColor, value); } }
        [JsonIgnore]
        public Color[] HWL_palette { get => _hwl_palette; set { Set(() => HWL_palette, ref _hwl_palette, value); } }
        [JsonIgnore]
        public byte HWL_effectIntensity { get => _hwl_effectIntensity; set { Set(() => HWL_effectIntensity, ref _hwl_effectIntensity, value); } }
        [JsonIgnore]
        public int HWL_version { get => _hwl_version; set { Set(() => HWL_version, ref _hwl_version, value); } }
        [JsonIgnore]
        public int HWL_MaxLEDPerOutput { get => _hwl_MaxLEDPerOutput; set { Set(() => HWL_MaxLEDPerOutput, ref _hwl_MaxLEDPerOutput, value); } }
        private string GetDeviceFirmwareExtensionString()
        {
            string ex = ".hex";
            switch (HardwareVersion)
            {
                case "ARR1p":
                case "AHR2g":
                case "AFR3g":
                case "AFR2g":
                case "AFR1g":
                case "AER2e":
                case "AER1e":
                case "ABR2e":
                case "ABR1p":
                    ex = ".hex";
                    break;
                case "AHR4p":
                    ex = ".uf2";
                    break;
            }
            return ex;
        }
        private ISlaveDevice[] GetSlaveDevices(ControllerTypeEnum type)
        {
            if (AvailableControllers == null)
                return null;
            var slaveDevices = new List<ISlaveDevice>();
            try
            {
                foreach (var controller in AvailableControllers.Where(x => x.Type == type))
                {
                    foreach (var output in controller.Outputs)
                    {
                        slaveDevices.Add(output.SlaveDevice);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return slaveDevices.ToArray();
        }
        private IOutputSettings[] GetOutput(ControllerTypeEnum type)
        {
            if (AvailableControllers == null) return null;
            var outputs = new List<IOutputSettings>();
            foreach (var controller in AvailableControllers.Where(x => x.Type == type))
            {
                foreach (var output in controller.Outputs)
                {
                    outputs.Add(output);
                }
            }
            return outputs.ToArray();
        }
        private ObservableCollection<IControlZone> GetControlZones(IDeviceController controller)
        {
            ObservableCollection<IControlZone> zones = new ObservableCollection<IControlZone>();
            foreach (var output in controller.Outputs.Where(o => o.IsEnabled))
            {
                foreach (var zone in output.SlaveDevice.ControlableZones)
                {
                    zones.Add(zone);
                }
            }
            return zones;
        }
        private ObservableCollection<IControlZone> GetControlZones()
        {
            if (AvailableControllers == null) return null;
            ObservableCollection<IControlZone> zones = new ObservableCollection<IControlZone>();
            foreach (var controller in AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    foreach (var zone in output.SlaveDevice.ControlableZones)
                    {
                        zones.Add(zone);
                    }
                }
            }

            return zones;
        }
        private ObservableCollection<ControlZoneGroup> GetControlZoneGroups(IDeviceController controller)
        {
            ObservableCollection<ControlZoneGroup> groups = new ObservableCollection<ControlZoneGroup>();
            foreach (var group in ControlZoneGroups.Where(g => g.Type == controller.Type))
            {
                groups.Add(group);
            }
            return groups;
        }
        public int CurrentActiveControlerIndex { get => _currentActiveControllerIndex; set { if (value >= 0) Set(() => CurrentActiveControlerIndex, ref _currentActiveControllerIndex, value); OnActiveControllerChanged(); } }
        private void OnActiveControllerChanged()
        {
            if (AvailableControllers != null)
            {
                if (CurrentActiveControlerIndex >= 0)
                {
                    CurrentActiveController = AvailableControllers[CurrentActiveControlerIndex];
                    //reset selected liveview zone because the collection changed
                    RaisePropertyChanged(nameof(CurrentLiveViewZones));
                    RaisePropertyChanged(nameof(CurrentActiveController));
                }
            }

        }
        bool _isDeserializing = false;
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            _isDeserializing = true;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            _isDeserializing = false;
        }
        public bool ShouldSerializeAvailableControllers()
        {
            return IsLoadingProfile;
        }
        private void IsLoadingProfilePropertyChanged()
        {
            if (AvailableControllers != null)
            {
                foreach (var controller in AvailableControllers)
                {
                    foreach (var output in controller.Outputs)
                    {
                        output.IsLoadingProfile = IsLoadingProfile;
                    }
                }
            }
        }


        private DrawableHelpers DrawableHlprs;

        public Rect GetDeviceRectBound(IControlZone[] zones)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();


            return DrawableHlprs.GetRealBound(zones);

        }

        public void BrightnessUp(int value)
        {

            foreach (var device in AvailableLightingDevices)//possible replace with method from IOutputSettings
            {
                var lightingDevice = device as ARGBLEDSlaveDevice;
                lightingDevice.BrightnessUp(value);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var lightingZone = group.MaskedControlZone as LEDSetup;
                    if (lightingZone != null)
                    {
                        lightingZone.BrightnessUp(value);
                    }
                }
            }


        }
        public void BrightnessDown(int value)
        {

            foreach (var device in AvailableLightingDevices)//possible replace with method from IOutputSettings
            {
                var lightingDevice = device as ARGBLEDSlaveDevice;
                lightingDevice.BrightnessDown(value);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var lightingZone = group.MaskedControlZone as LEDSetup;
                    if (lightingZone != null)
                    {
                        lightingZone.BrightnessDown(value);
                    }
                }
            }


        }
        public void SpeedUp(int value)
        {

            foreach (var device in AvailablePWMDevices)//possible replace with method from IOutputSettings
            {
                var pwmDevice = device as PWMMotorSlaveDevice;
                pwmDevice.SpeedUp(value);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var pwmZone = group.MaskedControlZone as FanMotor;
                    if (pwmZone != null)
                    {
                        pwmZone.SpeedUp(value);
                    }
                }
            }

        }
        public void SpeedDown(int value)
        {

            foreach (var device in AvailablePWMDevices)//possible replace with method from IOutputSettings
            {
                var pwmDevice = device as PWMMotorSlaveDevice;
                pwmDevice.SpeedDown(value);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var pwmZone = group.MaskedControlZone as FanMotor;
                    if (pwmZone != null)
                    {
                        pwmZone.SpeedDown(value);
                    }
                }
            }


        }
        public void RefreshLightingEngine()
        {
            foreach(var zone in AvailableControlZones.Where(z=>z is LEDSetup))
            {
                RaisePropertyChanged(nameof(zone.CurrentActiveControlMode));
            }
        }
        public void TurnOffLED()
        {
            DeviceState = DeviceStateEnum.Off;

        }
        public void TurnOnLED()
        {
            DeviceState = DeviceStateEnum.Normal;
        }
        public void DeviceEnableChanged()
        {
            if (IsEnabled)
                TurnOnLED();
            else
            {
                TurnOffLED();
            }
            if (_isDeserializing) return;
            var dvcHlprs = new DeviceHelpers();
            dvcHlprs.WriteSingleDeviceInfoJson(this);
        }
        public void ToggleOnOffLED()
        {
            if (DeviceState == DeviceStateEnum.Off)
            {
                DeviceState = DeviceStateEnum.Normal;
            }
            else if (DeviceState == DeviceStateEnum.Normal)
            {
                DeviceState = DeviceStateEnum.Off;
            }
        }
        public void SetStaticColor(ColorCard colors)
        {
            foreach (var device in AvailableLightingDevices)//possible replace with method from IOutputSettings
            {
                var lightingDevice = device as ARGBLEDSlaveDevice;
                lightingDevice.SetStaticColor(colors);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var lightingZone = group.MaskedControlZone as LEDSetup;
                    if (lightingZone != null)
                    {
                        var staticColorMode = lightingZone.AvailableControlMode.Where(m => (m as LightingMode).BasedOn == LightingModeEnum.StaticColor).FirstOrDefault() as LightingMode;
                        if (staticColorMode == null)
                            continue;
                        staticColorMode.Enable();
                        (staticColorMode.ColorParameter as ListSelectionParameter).SelectedValue = colors;
                        lightingZone.CurrentActiveControlMode = staticColorMode;
                        var zones = GetGroupChildItems(group);
                        if (zones != null)
                        {
                            foreach (var zone in zones)
                            {
                                var ledZone = zone as LEDSetup;
                                ledZone.CurrentActiveControlMode = staticColorMode;
                            }
                        }
                    }
                }
            }
        }
        private List<IControlZone> GetGroupChildItems(ControlZoneGroup group)
        {
            List<IControlZone> childItems = new List<IControlZone>();
            foreach (var zone in AvailableControlZones)
            {
                if (zone.GroupID == group.GroupUID)
                {
                    childItems.Add(zone);
                }
            }
            return childItems;
        }
        public void ActivateControlMode(LightingMode lightingMode)
        {
            if (lightingMode == null)
                return;
            foreach (var device in AvailableLightingDevices)//possible replace with method from IOutputSettings
            {
                var lightingDevice = device as ARGBLEDSlaveDevice;
                lightingDevice.ActivateControlMode(lightingMode);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var lightingZone = group.MaskedControlZone as LEDSetup;
                    if (lightingZone != null)
                    {
                        lightingZone.CurrentActiveControlMode = lightingMode;
                        var zones = GetGroupChildItems(group);
                        if (zones != null)
                        {
                            foreach (var zone in zones)
                            {
                                var ledZone = zone as LEDSetup;
                                ledZone.CurrentActiveControlMode = lightingMode;
                            }
                        }
                    }
                }
            }
        }
        public void SetModeByEnumValue(LightingModeEnum value)
        {
            foreach (var device in AvailableLightingDevices)//possible replace with method from IOutputSettings
            {
                var lightingDevice = device as ARGBLEDSlaveDevice;
                lightingDevice.SetModeByEnumValue(value);
            }
            if (ControlZoneGroups != null)
            {
                foreach (var group in ControlZoneGroups)
                {
                    var lightingZone = group.MaskedControlZone as LEDSetup;
                    if (lightingZone != null)
                    {
                        var targetMode = lightingZone.AvailableControlMode.Where(m => (m as LightingMode).BasedOn == value).FirstOrDefault() as LightingMode;
                        if (targetMode == null)
                            continue;
                        lightingZone.CurrentActiveControlMode = targetMode;
                        var zones = GetGroupChildItems(group);
                        if (zones != null)
                        {
                            foreach (var zone in zones)
                            {
                                var ledZone = zone as LEDSetup;
                                ledZone.CurrentActiveControlMode = targetMode;
                            }
                        }
                    }
                }
            }
        }
        #region Graphic Related Method
        public void UpdateChildSize()
        {
            foreach (var controller in AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    output.SlaveDevice.UpdateSizeByChild(false);
                }
            }
            RaisePropertyChanged(nameof(CurrentLiveViewZones));
        }
        public void HandleResolutionChange(double scaleX, double scaleY)
        {
            foreach (var controller in AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    (output.SlaveDevice as IDrawable).SetScale(scaleX, scaleY, false);
                }
            }
        }
        #endregion

        public void UpdateUID()
        {
            foreach (var zone in AvailableControlZones)
            {
                zone.ZoneUID = Guid.NewGuid().ToString();
            }
            DeviceUID = Guid.NewGuid().ToString();
        }


    }
}
