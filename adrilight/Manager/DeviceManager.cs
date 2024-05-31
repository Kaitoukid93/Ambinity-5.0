using adrilight.Services.DataStream;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.ViewModel;
using adrilight.ViewModel.Splash;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Services;
using adrilight_shared.Services.AdrilightStoreService;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using OpenRGB.NET.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Manager
{
    //this is the root of all service related to device activities (lighting,fan,..)
    //this will get injected to many viewmodels to interact like navigate to device, delete device, manual add device...
    //
    public class DeviceManager
    {
        public event Action<IDeviceSettings> NewDeviceAdded; // DeviceDashboardViewModel will listen to this event and add device to dashboard
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        #region Construct
        public DeviceManager(DeviceDBManager dbManager,
            AdrilightSFTPClient sFtpClient,
            DeviceDiscovery deviceDiscovery,
            DeviceLightingServiceManager lightingServicemanager,
            DeviceConnectionManager connectionManager,
            DeviceHardwareSettings deviceHardwareSettings,
            DialogService dialogService)
        {
            _dbManager = dbManager;
            _lightingServiceManager = lightingServicemanager;
            _deviceConnectionManager = connectionManager;
            _discoveryService = deviceDiscovery;
            _deviceConstructor = new DeviceConstructor();
            _sftpClient = sFtpClient;
            _discoveryService.OpenRGBDevicesScanComplete += OnOpenRGBDevicesScanComplete;
            _discoveryService.SerialDevicesScanComplete += OnSerialDevicesScanComplete;
            _deviceHardwareSettings = deviceHardwareSettings ?? throw new ArgumentNullException(nameof(deviceHardwareSettings));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }
        #endregion
        #region Events
        private async void OnOpenRGBDevicesScanComplete(List<Device> availableDevices)
        {
            //check if theres any old device existed
            //if true, turn on serial stream for each device
            //check if any new device
            //if true, show searching screen
            //run device construct and device downloader as part of the process, loading bar always exist
        }
        private async void OnSerialDevicesScanComplete(List<string> availableDevices)
        {
            //check if theres any old device existed
            var vm = new DeviceSearchingDialogViewModel("New Device", null, null);
            foreach (var device in availableDevices)
            {
                var matchDev = CheckDeviceForExistence(device);
                //there is an device existed with the exact comport
                if (matchDev != null)
                {
                    //do nothing for an activated device
                    //possible showing a connecting screen???...
                    RegisterDevice(matchDev);
                    matchDev.IsTransferActive = true;
                    continue;
                }
                //if there is no device match. this is a new device
                // call device discovery to get infomation about this new device
                //var dev = await TryCreatenewDevice(device, vm);


            }
        }
        #endregion

        #region Properties
        private DeviceDBManager _dbManager;
        private List<IDeviceSettings> _availableDevices;
        private DeviceLightingServiceManager _lightingServiceManager;
        private DeviceDiscovery _discoveryService;
        private DialogService _dialogService;
        private DeviceHardwareSettings _deviceHardwareSettings;
        private DeviceConstructor _deviceConstructor;
        private AdrilightSFTPClient _sftpClient;
        private DeviceConnectionManager _deviceConnectionManager;
        private List<IDataStream> _dataStreams;
        public List<IDeviceSettings> AvailableDevices {
            get
            {
                return _availableDevices;
            }
        }
        #endregion
        #region Methods
        #endregion
        public async Task LoadData(SplashScreenViewModel loadingViewModel)
        {
            //load device if exist
            _availableDevices = new List<IDeviceSettings>();
            var devices = await Task.Run(() => LoadDeviceFromFolder(DevicesCollectionFolderPath, loadingViewModel));
            if (devices != null)
            {
                foreach (var device in devices)
                {
                    _availableDevices.Add(device);
                }
            }
            await Task.Delay(500);
            loadingViewModel.Description = "Starting Device Discovery service";
            loadingViewModel.Progress = 90;
            _discoveryService.Start();
            await Task.Delay(500);
            loadingViewModel.Progress = 100;
            await Task.Delay(500);
        }
        //this will create neccesary service for device to run
        private void RegisterDevice(IDeviceSettings device)
        {
            if (_dataStreams == null)
                _dataStreams = new List<IDataStream>();
            var existedStream = _dataStreams.Where(d => (d as SerialStream).Port == device.OutputPort).ToList();
            if (existedStream.Count() > 0)
            {
                return;
            }
            var dataStream = _deviceConnectionManager.CreateDeviceStreamService(device);
            if (device.AutoConnect)
            {
                dataStream.Init(device);
                _dataStreams.Add(dataStream);
            }
            _lightingServiceManager.CreateLightingService(device);
        }
        public IDeviceSettings CheckDeviceForExistence(string comport)
        {
            return _availableDevices.Where(d => (d as DeviceSettings).OutputPort == comport).First();
        }
        //add new device manually and add to available devices by type

        public void StopDiscoveryService()
        {
            _discoveryService?.Stop();
        }
        public void StartDiscoveryService()
        {
            _discoveryService?.Start();
        }
        private void CreateNewDevice(DeviceType type)
        {

        }
        //create new device by hand-shake to serial port
        private async Task<List<DeviceSettings>> LoadDeviceFromFolder(string folderPath, SplashScreenViewModel loadingViewModel)
        {
            var devices = new List<DeviceSettings>();
            if (!Directory.Exists(folderPath)) return null; // no device has been added
            loadingViewModel.Progress = 25;
            int maxDevCount = Directory.GetDirectories(folderPath).Count();
            foreach (var folder in Directory.GetDirectories(folderPath))
            {
                try
                {
                    loadingViewModel.Description = "Loading " + new DirectoryInfo(folder).Name;
                    var json = File.ReadAllText(Path.Combine(folder, "config.json"));
                    var device = JsonConvert.DeserializeObject<DeviceSettings>(json);
                    device.AvailableControllers = new List<IDeviceController>();
                    //read slave device info
                    //check if this device contains lighting controller
                    var lightingoutputDir = Path.Combine(Path.Combine(folder, "LightingOutputs"));
                    var pwmoutputDir = Path.Combine(Path.Combine(folder, "PWMOutputs"));
                    _dbManager.DeserializeChild<ARGBLEDSlaveDevice>(lightingoutputDir, device, OutputTypeEnum.ARGBLEDOutput);
                    _dbManager.DeserializeChild<PWMMotorSlaveDevice>(pwmoutputDir, device, OutputTypeEnum.PWMOutput);
                    devices.Add(device);
                    loadingViewModel.Progress += (50/maxDevCount);
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, folder);
                    loadingViewModel.Description = ex.Message;
                    continue;
                }
                
            }
            return devices;
        }
        private async Task<IDeviceSettings> TryCreatenewDevice(string comPort, DeviceSearchingDialogViewModel vm)
        {

            //get device info
            _dialogService.ShowDialog<DeviceSearchingDialogViewModel>(result =>
            {
                //try update the view if needed

            }, vm);
            vm.CurrentProgressLog = "Requesting new device info...";
            string deviceName = null;
            string deviceID = null;
            string deviceFirmware = null;
            string deviceHardware = null;
            int deviceHWL = 0;
            var result = await Task.Run(() => _deviceHardwareSettings.RefreshDeviceInfo(comPort,
                out deviceName,
                out deviceID,
                out deviceFirmware,
                out deviceHardware,
                out deviceHWL));
            if (!result)
                return null;
            //construct new device
            vm.CurrentProgressLog = "Device is " + deviceName;
            await Task.Delay(1000);
            vm.CurrentProgressLog = "Constructing new device...";
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.Value = 10;
            //check for online available
            vm.CurrentProgressLog = "Downloading device modules: " + deviceName;
            var availableDevices = await _sftpClient.DownloadDeviceInfo(deviceName, deviceName.Replace(" ", string.Empty), DeviceConnectionTypeEnum.Wired);
            if (availableDevices.Count == 0)
            {
                vm.CurrentProgressLog = "Using Default: " + deviceName;
                return _deviceConstructor.ConstructNewSerialDevice(deviceName, deviceID, deviceFirmware, deviceHardware, deviceHWL, comPort);
            }
            else if (availableDevices.Count == 1)
            {
                vm.CurrentProgressLog = "Downloading modules : " + deviceName;
                var onlineDevice = availableDevices.First();
                var savePath = _sftpClient.DownloadFile(onlineDevice);
                var downloadedDevice = _dbManager.ImportDevice(savePath);
                if (downloadedDevice != null)
                {
                    downloadedDevice.OutputPort = comPort;
                    downloadedDevice.FirmwareVersion = deviceFirmware;
                    downloadedDevice.HardwareVersion = deviceHardware;
                    downloadedDevice.DeviceSerial = deviceID;
                    downloadedDevice.DeviceType.ConnectionTypeEnum = DeviceConnectionTypeEnum.Wired;
                    return downloadedDevice;
                }
                return null;
            }
            else if (availableDevices.Count > 1)
            {
                //show selection dialog
                //get selection by result
                //download module
                //
                return null;
            }
            return null;
        }

    }
}
