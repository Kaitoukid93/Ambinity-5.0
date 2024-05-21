using adrilight.Services.OpenRGBService;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Settings;
using adrilight_shared.ViewModel;
using Microsoft.Win32;
using OpenRGB.NET.Models;
using Renci.SshNet.Sftp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Services.DeviceDiscoveryServices
{
    public class DeviceDiscovery
    {
        public event Action<List<String>> SerialDevicesScanComplete;
        public event Action<List<Device>> OpenRGBDevicesScanComplete;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static byte[] settingCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] unexpectedValidHeader = { (byte)'A', (byte)'b', (byte)'n' };
        public DeviceDiscovery(IGeneralSettings settings, AmbinityClient ambinityClient, DeviceConstructor deviceConstructor)
        {
            _generaSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ambinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
            _deviceConstructor = deviceConstructor;
            if (_generaSettings.UsingOpenRGB)
            {

            }
            Start();

        }
        private bool _openRGBIsInit = false;
        private IGeneralSettings _generaSettings;
        private AmbinityClient _ambinityClient;
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private DeviceConstructor _deviceConstructor;
        public void Start()
        {
            //if (App.IsPrivateBuild) return;
            _cancellationTokenSource = new CancellationTokenSource();
            _openRGBIsInit = false;
            _workerThread = new Thread(() => StartDiscovery(_cancellationTokenSource.Token)) {
                Name = "Device Discovery",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();

        }
        private async void StartDiscovery(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    //get the list of new devices for every second
                    // new device contains serial and openrgb devices ( Wled devices in the future)
                    ScanSerialDevice();
                    if (!_openRGBIsInit)
                        await ScanOpenRGBDevices();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
                }
                //check once a second for updates
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        public async Task<DeviceSettings> GetSerialDeviceInformation(string comPort, ProgressDialogViewModel vm)
        {
            var newDevice = new DeviceSettings();
            string id = "000000";
            string name = "unknown";
            string fw = "unknown";
            string hw = "unknown";
            int hwLightingVersion = 0;
            var _serialPort = new SerialPort(comPort, 1000000);
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.DtrEnable = true;
            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                // Log.Error(ex,"AcessDenied " + _serialPort.PortName);
                Log.Error(_serialPort.PortName + " is removed");
                Thread.Sleep(2000);
                return null;
            }

        //write request info command
        retry:
            try
            {
                _serialPort.Write(requestCommand, 0, 3);
                _serialPort.WriteLine("\r\n");
            }

            catch (System.IO.IOException ex)// retry until received valid header
            {
                Log.Warning("This Device seems to have Ambino PID/VID but not an USB device " + _serialPort.PortName);
                vm.CurrentProgressLog = "This Device seems to have Ambino PID/VID but not an USB device " + _serialPort.PortName;
                return null;
            }
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            int hwLength = 0;

            while (offset < 3)
            {
                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                    else if (header == unexpectedValidHeader[offset])
                    {
                        offset++;
                        if (offset == 3)
                        {
                            Log.Information("Old Ambino Device at" + _serialPort.PortName + ". Restarting Firmware Request");
                            vm.CurrentProgressLog = "Old Ambino Device at" + _serialPort.PortName + ". Restarting Firmware Request";
                            goto retry;
                        }
                    }
                }
                catch (TimeoutException ex)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3);
                    _serialPort.WriteLine("\r\n");
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                        vm.CurrentProgressLog = "timeout waiting for respond on serialport " + _serialPort.PortName;
                        //if (!isNoRespondingMessageShowed)
                        //{
                        //    isNoRespondingMessageShowed = true;
                        //    HandyControl.Controls.MessageBox.Show("Device at " + _serialPort.PortName + " is not responding, try adding it manually", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //}
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return null;
                    }
                }
                catch (System.IO.IOException ex)// retry until received valid header
                {
                    return null;
                }

            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                var d = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(d, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                id = BitConverter.ToString(d);
                vm.CurrentProgressLog = "ID: " + id;
            }

            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                var n = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(n, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                name = Encoding.ASCII.GetString(n, 0, n.Length);
                vm.CurrentProgressLog = "Name: " + name;

            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                var f = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(f, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                fw = Encoding.ASCII.GetString(f, 0, f.Length);
                vm.CurrentProgressLog = "Firmware: " + fw;
            }
            if (offset == 3 + idLength + nameLength + fwLength) //3 bytes header are valid
            {
                try
                {
                    hwLength = (byte)_serialPort.ReadByte();
                    int count = hwLength;
                    var h = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(h, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                    hw = Encoding.ASCII.GetString(h, 0, h.Length);
                    vm.CurrentProgressLog = "Hardware: " + hw;
                }
                catch (TimeoutException)
                {
                    Log.Information(name.ToString(), "Unknown Hardware Version");
                    vm.CurrentProgressLog = "Unknown Hardware Version";
                }

            }
            if (offset == 3 + idLength + nameLength + fwLength + hwLength) //3 bytes header are valid
            {
                try
                {
                    hwLightingVersion = _serialPort.ReadByte();
                }
                catch (TimeoutException)
                {
                    Log.Information(name.ToString(), "Unknown Hardware Lighting Version");
                    vm.CurrentProgressLog = "Unknown Hardware Lighting Version";
                }

            }
            //construct new device
            vm.CurrentProgressLog = "Constructing new device...";
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.Value = 10;
            //check for online available
            vm.CurrentProgressLog ="Downloading device modules: " + name;
            var result = await DownloadDeviceInfo(device);
            if (!result)
            {
                SetSearchingScreenHeaderText("Using Default: " + device.DeviceName, true);
            }
            else
            {
                SetSearchingScreenHeaderText("Device modules downloaded: " + device.DeviceName, true);
                var downloadedDevice = ImportDevice(Path.Combine(CacheFolderPath, device.DeviceName + ".zip"));
                if (downloadedDevice != null)
                {
                    //transplant
                    downloadedDevice.OutputPort = device.OutputPort;
                    downloadedDevice.FirmwareVersion = device.FirmwareVersion;
                    downloadedDevice.HardwareVersion = device.HardwareVersion;
                    downloadedDevice.DeviceSerial = device.DeviceSerial;
                    downloadedDevice.DeviceType.ConnectionTypeEnum = device.DeviceType.ConnectionTypeEnum;
                    device = downloadedDevice;
                }
            }
            //if not, construct new device
            var dev = _deviceConstructor.ConstructNewDevice(name, id, fw, hw, hwLightingVersion, comPort);
            _serialPort.Close();
            _serialPort.Dispose();
            return dev;
        }
        private async Task ScanOpenRGBDevices()
        {
            if (!_ambinityClient.IsInitialized && !_ambinityClient.IsInitializing)
            {
                await _ambinityClient.Init();
            }
            _openRGBIsInit = true;
            var detectedDevices = _ambinityClient.ScanNewDevice();
            OpenRGBDevicesScanComplete?.Invoke(detectedDevices);
        }
        private static object _syncRoot = new object();
        public void Stop()
        {
            Log.Information("Stop called for Device Discovery");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }
        static List<string> GetComPortByID(String VID, String PID)
        {
            String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            List<string> comports = new List<string>();
            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (String s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (String s in rk3.GetSubKeyNames())
                {
                    if (_rx.Match(s).Success)
                    {
                        RegistryKey rk4 = rk3.OpenSubKey(s);
                        foreach (String s2 in rk4.GetSubKeyNames())
                        {
                            RegistryKey rk5 = rk4.OpenSubKey(s2);
                            RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                            string portName = (string)rk6.GetValue("PortName");
                            if (!String.IsNullOrEmpty(portName) && SerialPort.GetPortNames().Contains(portName))
                            {
                                comports.Add((string)rk6.GetValue("PortName"));
                            }
                        }
                    }
                }

            }
            return comports;
        }
        /// <summary>
        /// this return any availabe port that is not in use by the app
        /// </summary>
        /// <returns></returns>
        public void ScanSerialDevice()
        {
            //these are valid PID VID used by Ambino devices
            List<string> CH55X = GetComPortByID("1209", "c550");
            List<string> CH340 = GetComPortByID("1A86", "7522");
            List<string> ada = GetComPortByID("239A", "CAFE");
            var devices = new List<string>();
            if (CH55X.Count > 0 || CH340.Count > 0 || ada.Count > 0)
            {
                foreach (var port in CH55X)
                {
                    devices.Add(port);
                }
                foreach (var port in CH340)
                {
                    devices.Add(port);
                }
                foreach (var port in ada)
                {
                    devices.Add(port);
                }
            }
            else
            {
                Log.Warning("No Compatible Device Detected");
            }
            SerialDevicesScanComplete?.Invoke(devices);
        }
        private async Task<bool> DownloadDeviceInfo(IDeviceSettings device)
        {
            bool result;
            PossibleMatchedDevices = new ObservableCollection<SftpFile>();
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                SFTPConnect();
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    result = false;
                    return await Task.FromResult(result);
                }
            }
            SftpFile matchedFile = null;
            string deviceFolderPath = string.Empty;
            switch (device.DeviceType.ConnectionTypeEnum)
            {
                case DeviceConnectionTypeEnum.OpenRGB:
                    matchedFile = await FTPHlprs.GetFileByNameMatching(device.DeviceName + ".zip", openRGBDevicesFolderPath + "/" + device.DeviceType.Type.ToString());
                    deviceFolderPath = openRGBDevicesFolderPath + "/" + device.DeviceType.Type.ToString();
                    break;
                case DeviceConnectionTypeEnum.Wired:
                    matchedFile = await FTPHlprs.GetFileByNameMatching(device.DeviceName + ".zip", ambinoDevicesFolderPath + "/" + device.DeviceType.Type.ToString());
                    deviceFolderPath = ambinoDevicesFolderPath + "/" + device.DeviceType.Type.ToString();
                    break;
            }
            if (matchedFile == null)
            //return available device instead
            {
                var availableFiles = await FTPHlprs.GetAllFilesInFolder(deviceFolderPath);
                if (availableFiles == null)
                {
                    result = false;
                    return await Task.FromResult(result);
                }
                foreach (var file in availableFiles)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PossibleMatchedDevices.Add(file);
                    });
                }
                if (PossibleMatchedDevices != null && PossibleMatchedDevices.Count > 0)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        possibleMatchedDeviceSelection = new PossibleMatchedDeviceSelectionWindow();
                        possibleMatchedDeviceSelection.Owner = _searchingForDeviceScreen;
                        bool? dialogResult = possibleMatchedDeviceSelection.ShowDialog();
                        if (dialogResult == true)
                        {
                            matchedFile = SelectedMatchedDevice;
                        }
                    });
                }
            }
            //if user decide to cancel or nothing exist
            if (matchedFile == null)
            {
                result = false;
                return await Task.FromResult(result);
            }
            //if nothing wrong, download to cache
            ClearCacheFolder();
            FTPHlprs.DownloadFile(matchedFile.FullName, Path.Combine(CacheFolderPath, matchedFile.Name), DownloadProgresBar);
            device.DeviceName = Path.GetFileNameWithoutExtension(Path.Combine(CacheFolderPath, matchedFile.Name));
            result = true;
            return await Task.FromResult(result);
        }
    }
}