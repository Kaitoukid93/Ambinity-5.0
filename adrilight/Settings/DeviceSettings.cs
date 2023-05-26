using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.ViewModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows;
using System.Buffers;
using HandyControl.Themes;
using System.Threading;
using Newtonsoft.Json;
using adrilight.Helpers;
using System.IO;
using System.Web.UI.WebControls.WebParts;

namespace adrilight
{
    internal class DeviceSettings : ViewModelBase, IDeviceSettings
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
        private string _deviceUID;
        private bool _isSelected = false;
        private bool _isLoadingProfile = false;
        private DeviceStateEnum _deviceState = DeviceStateEnum.Normal;
        private string _requiredFwVersion;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private bool _isSizeNeedUserDefine = false;
        private DeviceType _deviceType;
        private int _currentActiveControllerIndex;
        private IDeviceController _currentActiveController;
        public int DashboardWidth { get => _dashboardWidth; set { Set(() => DashboardWidth, ref _dashboardWidth, value); } }
        public int DashboardHeight { get => _dashboardHeight; set { Set(() => DashboardHeight, ref _dashboardHeight, value); } }
        [JsonIgnore]
        public string DeviceThumbnail => File.Exists(Path.Combine(ResourceCollectionFolderPath, DeviceName + "_thumb.png")) ? Path.Combine(ResourceCollectionFolderPath, DeviceName + "_thumb.png") : Path.Combine(ResourceCollectionFolderPath, DeviceType.Name + "_thumb.png");
        [JsonIgnore]
        public string DeviceOutputMap => Path.Combine(deviceDirectory, "outputmap.png");
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
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); DeviceEnableChanged(); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string OutputPort { get => _outputPort; set { Set(() => OutputPort, ref _outputPort, value); } }
        [JsonIgnore]
        public bool IsTransferActive { get => _isTransferActive; set { Set(() => IsTransferActive, ref _isTransferActive, value); } }
        public int Baudrate { get => _baudrate; set { Set(() => Baudrate, ref _baudrate, value); } }
        public string DeviceDescription { get => _deviceDescription; set { Set(() => DeviceDescription, ref _deviceDescription, value); } }
        public string DeviceUID { get => _deviceUID; set { Set(() => DeviceUID, ref _deviceUID, value); } }
        public bool IsSizeNeedUserDefine { get => _isSizeNeedUserDefine; set { Set(() => IsSizeNeedUserDefine, ref _isSizeNeedUserDefine, value); } }
        public List<IDeviceController> AvailableControllers { get => _availableControllers; set { Set(() => AvailableControllers, ref _availableControllers, value); } }
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
        private ISlaveDevice[] GetSlaveDevices(ControllerTypeEnum type)
        {
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
            foreach (var output in controller.Outputs)
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
        private void DeviceEnableChanged()
        {


            //if (AvailableOutputs != null)
            //{

            //    if (AvailableOutputs.Length == 1)
            //    {
            //        AvailableOutputs[0].OutputIsEnabled = IsEnabled;
            //    }
            //    else
            //    {
            //        foreach (var output in AvailableOutputs)
            //        {
            //            output.OutputParrentIsEnable = IsEnabled;

            //        }
            //    }
            //}


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

            //foreach (var output  in AvailableOutputs)//possible replace with method from IOutputSettings
            //  {
            //  var currentBrightness = (output as OutputSettings).GetBrightness();
            //  var nextBrightness = currentBrightness + value;
            //  if(nextBrightness<100)
            //      (output as OutputSettings).SetBrightness(nextBrightness);
            //  else
            //  {
            //      (output as OutputSettings).SetBrightness(100);
            //  }
            //  }

        }
        public void SpeedUp(int value)
        {

            //if (DeviceSpeed < 255)
            //    DeviceSpeed += value;
            //if (DeviceSpeed > 255)
            //    DeviceSpeed = 255;

        }
        public void SpeedDown(int value)
        {

            //if (DeviceSpeed >20)
            //    DeviceSpeed -= value;
            //if (DeviceSpeed < 20)
            //    DeviceSpeed = 20;

        }
        public void BrightnessDown(int value)
        {


            //foreach (var output in AvailableOutputs)//possible replace with method from IOutputSettings
            //{
            //    var currentBrightness = (output as OutputSettings).GetBrightness();
            //    var nextBrightness = currentBrightness - value;
            //    if (nextBrightness > 0)
            //        (output as OutputSettings).SetBrightness(nextBrightness);
            //    else
            //    {
            //        (output as OutputSettings).SetBrightness(0);
            //    }
            //}

        }

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
        public void ActivateProfile(IDeviceSettings device)
        {
            int slaveDeviceCounter = 0;
            foreach (var slaveDevice in AvailableLightingDevices)
            {
                int ledZoneCounter = 0;
                var slaveDeviceProfileData = device.AvailableLightingDevices[slaveDeviceCounter] as ARGBLEDSlaveDevice;
                foreach (PropertyInfo property in slaveDevice.GetType().GetProperties())
                {

                    if (property.CanWrite && !Attribute.IsDefined(property, typeof(ProfileIgnoreAttribute)))
                            property.SetValue(slaveDevice, property.GetValue(slaveDeviceProfileData, null), null);
                }
                foreach (var zone in slaveDevice.ControlableZones)
                {
                    var ledZone = zone as LEDSetup;
                    var zoneProfileData = slaveDeviceProfileData.ControlableZones[ledZoneCounter] as LEDSetup;
                    foreach (PropertyInfo property in ledZone.GetType().GetProperties())
                    {

                        // if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
                        if (property.CanWrite)
                            property.SetValue(ledZone, property.GetValue(zoneProfileData, null), null);
                    }
                    ledZoneCounter++;
                }
                slaveDeviceCounter++;
            }

        }
        public void RefreshFirmwareVersion()
        {

            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];

            bool isValid = false;


            IsTransferActive = false; // stop current serial stream attached to this device

            var _serialPort = new SerialPort(OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            //write request info command
            _serialPort.Write(requestCommand, 0, 3);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            IDeviceSettings newDevice = new DeviceSettings();
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + "Không có thông tin về Firmware, vui lòng liên hệ Ambino trước khi cập nhật firmware thủ công", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        isValid = false;
                        break;
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                id = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(id, 0, count);
                    offset += readCount;
                    count -= readCount;
                }


                DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
                RaisePropertyChanged(nameof(DeviceSerial));
            }
            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                name = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(name, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
                RaisePropertyChanged(nameof(DeviceName));


            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                fw = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(fw, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
                RaisePropertyChanged(nameof(FirmwareVersion));
            }
            _serialPort.Close();
            _serialPort.Dispose();
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            IsTransferActive = true;
            RaisePropertyChanged(nameof(IsTransferActive));
        }

    }
}
