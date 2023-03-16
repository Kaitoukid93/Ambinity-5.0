using adrilight.Resources;
using adrilight.Settings;
using adrilight.Util;
using adrilight.ViewModel;
using Newtonsoft.Json;
using NLog;
using Semver;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight
{
    internal class DeviceDiscovery : IDeviceDiscovery
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private bool _openRGBIsInit = false;
        public DeviceDiscovery(IGeneralSettings settings, IContext context, IAmbinityClient ambinityClient, MainViewViewModel mainViewViewModel)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            AmbinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            StartThread();

        }
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        public void StartThread()
        {
            //if (App.IsPrivateBuild) return;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(StartDiscovery) {
                Name = "Device Discovery",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start(_cancellationTokenSource.Token);

        }

        public IGeneralSettings Settings { get; }
        public IContext Context { get; }
        private bool _isSerialScanCompelete;
        public ObservableCollection<IDeviceSettings> AvailableOpenRGBDevices { get; set; }
        public ObservableCollection<IDeviceSettings> AvailableWLEDDevices { get; set; }
        public ObservableCollection<IDeviceSettings> AvailableSerialDevices { get; set; }
        public MainViewViewModel MainViewViewModel { get; set; }
        private bool _enable = true;
        public bool enable {
            get { return _enable; }
            set { _enable = value; }

        }

        private IAmbinityClient AmbinityClient { get; }
        private async void StartDiscovery(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (Settings.DeviceDiscoveryMode == 0 && !Settings.FrimwareUpgradeIsInProgress && enable)
                    {
                        var newOpenRGBDevices = new List<IDeviceSettings>(); // openRGB device scan only run once at startup
                        if (!_openRGBIsInit)
                            newOpenRGBDevices = ScanOpenRGBDevices();
                        var connectedSerialDevices = await ScanSerialDevice();

                        MainViewViewModel.FoundNewDevice(connectedSerialDevices[0], newOpenRGBDevices);
                        MainViewViewModel.OldDeviceReconnected(connectedSerialDevices[1], newOpenRGBDevices);
                    }

                    //else if (Settings.DeviceDiscoveryMode==1)
                    //{

                    //    var newDevicesString = new StringBuilder().AppendLine($"Tìm thấy các thiết bị mới :").ToString() +"\n";
                    //    if (newOpenRGBDevices.Count>0)
                    //    {
                    //        foreach(var device in newOpenRGBDevices)
                    //        {
                    //            newDevicesString += device.DeviceName + "\n";
                    //        }
                    //    }
                    //    if (newSerialDevices.Count > 0)
                    //    {
                    //        foreach (var device in newSerialDevices)
                    //        {
                    //            newDevicesString += device.DeviceName + "\n";
                    //        }
                    //    }
                    //    newDevicesString += "Bạn có muốn thêm các thiết bị này vào Dashboard không?" + "\n";
                    //    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(newDevicesString, "New Devices Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    //    if (result == MessageBoxResult.Yes)
                    //    {
                    //        // Enable OpenRGB
                    //        MainViewViewModel.FoundNewDevice(newSerialDevices, newOpenRGBDevices);

                    //    }
                    //}

                    //else ask before add

                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a second for updates
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private List<IDeviceSettings> ScanOpenRGBDevices()
        {

            var newDevices = new List<IDeviceSettings>();
            var detectedDevices = AmbinityClient.ScanNewDevice();
            if (detectedDevices != null)
            {

                foreach (var openRGBDevice in detectedDevices)
                {
                    try
                    {
                        IDeviceSettings convertedDevice = new DeviceSettings();
                        convertedDevice.DeviceName = openRGBDevice.Name;
                        convertedDevice.DeviceType = openRGBDevice.Type.ToString();
                        convertedDevice.FirmwareVersion = openRGBDevice.Version;
                        convertedDevice.OutputPort = AmbinityClient.Client.ToString();
                        convertedDevice.DeviceDescription = "Device Supported Throught Open RGB Client";
                        convertedDevice.DeviceConnectionType = "OpenRGB";
                        //convertedDevice.DeviceID = AvailableDevices.Count + 1;// this is removed due to unneccessary of device ID, replaced with deviceUID
                        convertedDevice.DeviceSerial = openRGBDevice.Serial;
                        convertedDevice.DeviceUID = openRGBDevice.Name + openRGBDevice.Version + openRGBDevice.Location;
                        convertedDevice.Geometry = "orgb";
                        convertedDevice.DeviceConnectionGeometry = "orgb";
                        convertedDevice.AvailableOutputs = new OutputSettings[openRGBDevice.Zones.Length];
                        int zoneCount = 0;
                        foreach (var zone in openRGBDevice.Zones)
                        {
                            switch (zone.Type)
                            {
                                //case OpenRGB.NET.Enums.ZoneType.Single:
                                //    convertedDevice.AvailableOutputs[zoneCount] = DefaulOutputCollection.GenericLEDStrip(zoneCount, 1, zone.Name, 1, true, "ledstrip");
                                //    break;

                                //case OpenRGB.NET.Enums.ZoneType.Linear:
                                //    convertedDevice.AvailableOutputs[zoneCount] = DefaulOutputCollection.GenericLEDStrip(zoneCount, (int)zone.LedCount, zone.Name, 1, true, "ledstrip");
                                //    break;

                                //case OpenRGB.NET.Enums.ZoneType.Matrix:
                                //    convertedDevice.AvailableOutputs[zoneCount] = DefaulOutputCollection.GenericLEDMatrix(zoneCount, (int)zone.MatrixMap.Width, (int)zone.MatrixMap.Height, zone.Name, 1, true, "matrix");
                                //    break;
                            }
                            zoneCount++;
                        }
                        convertedDevice.IsTransferActive = true;
                        convertedDevice.IsEnabled = true;
                        newDevices.Add(convertedDevice);
                    }
                    catch (Exception ex)
                    {
                        //something error with one or more parametters
                        continue;
                    }

                }

            }
            _openRGBIsInit = true;
            return newDevices;
            //else
            //{
            //}
        }
        private static object _syncRoot = new object();
        public void Stop()
        {
            _log.Debug("Stop called.");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            tokenSource?.Cancel();
            tokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }

        private async Task<List<List<IDeviceSettings>>> ScanSerialDevice()
        {
            var newDevicesDetected = new List<IDeviceSettings>();
            var oldDeviceReconnected = new List<IDeviceSettings>();
            _isSerialScanCompelete = false;
            ISerialDeviceDetection detector = new SerialDeviceDetection(MainViewViewModel.AvailableDevices.Where(p => p.DeviceConnectionType == "wired").ToList());
           

            var jobTask = Task.Run(() =>
            {
                // Organize critical sections around logical serial port operations somehow.
                lock (_syncRoot)
                {
                    return detector.DetectedDevices;
                }
            });
            if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, token)))
            {
                // Timeout;
                _isSerialScanCompelete = true;

            }
            var newDevices = await jobTask;
            if (newDevices.Count == 0)
            {
                // HandyControl.Controls.MessageBox.Show("Unable to detect any supported device, try adding manually", "No Compatible Device Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                _isSerialScanCompelete = true;

            }
            else
            {
                foreach (var device in newDevices)
                {
                    Debug.WriteLine("Name: " + device.DeviceName);
                    Debug.WriteLine("ID: " + device.DeviceSerial);
                    Debug.WriteLine("Firmware Version: " + device.FirmwareVersion);
                    Debug.WriteLine("---------------");
                    if (MainViewViewModel.AvailableDevices.Any(p => p.OutputPort == device.OutputPort)) // this device match an old device that existed 
                    {
                        oldDeviceReconnected.Add(device);

                    }
                    else
                    {
                        newDevicesDetected.Add(device);
                    }
                }
                //AvailableSerialDevices = new ObservableCollection<IDeviceSettings>();
                //foreach (var device in newDevices)
                //{
                //    newDevicesDetected.Add(device);
                //}
                tokenSource.Cancel();
                _isSerialScanCompelete = true;

            }
            return await Task.FromResult(new List<List<IDeviceSettings>> { newDevicesDetected, oldDeviceReconnected });
        }
    }
}