using adrilight.Resources;
using adrilight.Services.OpenRGBService;
using adrilight.Util;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Extensions;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Settings;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace adrilight.Services.DeviceDiscoveryServices
{
    internal class DeviceDiscovery
    {

        public DeviceDiscovery(IGeneralSettings settings, IContext context, IAmbinityClient ambinityClient, MainViewViewModel mainViewViewModel)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            if (Settings.UsingOpenRGB)
            {
                AmbinityClient = ambinityClient as AmbinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
                AmbinityClient.PropertyChanged += (_, __) =>
                {
                    if (__.PropertyName == nameof(AmbinityClient.IsInitialized))
                    {
                        AmbinityClientStateChanged();
                    }
                };
            }

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            MainViewViewModel.AvailableDevices.CollectionChanged += (_, __) => DeviceCollectionChanged();
            MainViewViewModel.RequestingDeviceRescanEvent += (_, __) => RescanOpenRGBDevices();
            StartThread();

        }
        private bool _openRGBIsInit = false;
        private void AmbinityClientStateChanged()
        {
            _openRGBIsInit = false;
        }
        private void RescanOpenRGBDevices()
        {
            _openRGBIsInit = false;
        }
        private void DeviceCollectionChanged()
        {
            SerialDeviceDetector = new SerialDeviceDetection(MainViewViewModel.AvailableDevices.Where(p => p.DeviceType.ConnectionTypeEnum == DeviceConnectionTypeEnum.Wired).ToList());
        }
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        public void StartThread()
        {
            //if (App.IsPrivateBuild) return;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => StartDiscovery(_cancellationTokenSource.Token)) {
                Name = "Device Discovery",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();

        }

        public IGeneralSettings Settings { get; }
        public IContext Context { get; }
        private bool _isSerialScanCompelete;
        public ObservableCollection<IDeviceSettings> AvailableOpenRGBDevices { get; set; }
        public ObservableCollection<IDeviceSettings> AvailableWLEDDevices { get; set; }
        public ObservableCollection<IDeviceSettings> AvailableSerialDevices { get; set; }
        public MainViewViewModel MainViewViewModel { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private bool _isAllDeviceConnected => !MainViewViewModel.AvailableDevices.Any(d => d.IsTransferActive == false);
        private async void StartDiscovery(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    //get the list of existed device
                    // if device is existed and device is connected ( device.IstransferActive ) , remove from the list
                    // if device is existed and device is connected, leave it available to reconnect
                    var existedSerialDevices = MainViewViewModel.AvailableDevices.Where(d => d.DeviceType.ConnectionTypeEnum == DeviceConnectionTypeEnum.Wired).ToList();
                    var existedOpenRGBDevices = MainViewViewModel.AvailableDevices.Where(d => d.DeviceType.ConnectionTypeEnum == DeviceConnectionTypeEnum.OpenRGB).ToList();
                    SerialDeviceDetector = new SerialDeviceDetection(existedSerialDevices);
                    var shouldBeRunning = !MainViewViewModel.FrimwareUpgradeIsInProgress && !MainViewViewModel.IsApplyingDeviceHardwareSettings;
                    if (Settings.DeviceDiscoveryMode == 0 && shouldBeRunning)
                    {
                        // openRGB device scan keep running until all existed device get connected
                        // if openrgb devices is not transfer active, check if this device is in the list of openRGB software and reconnect
                        var openRGBDevices = (new List<IDeviceSettings>(), new List<string>());
                        if (Settings.IsOpenRGBEnabled && !_openRGBIsInit && Settings.UsingOpenRGB)
                        {
                            MainViewViewModel.IsRescanningDevices = true;
                            openRGBDevices = await ScanOpenRGBDevices(existedOpenRGBDevices);
                            MainViewViewModel.IsRescanningDevices = false;
                        }
                        var serialDevices = await ScanSerialDevice();
                        var newDevices = new List<IDeviceSettings>();
                        var oldDevicesReconnected = new List<string>();
                        openRGBDevices.Item1.ForEach(d => newDevices.Add(d));
                        serialDevices.Item1.ForEach(d => newDevices.Add(d));
                        openRGBDevices.Item2.ForEach(d => oldDevicesReconnected.Add(d));
                        serialDevices.Item2.ForEach(d => oldDevicesReconnected.Add(d));
                        if (newDevices.Count > 0)
                        {
                            MainViewViewModel.ShowSearchingScreen();
                        }
                        await Task.Run(() => MainViewViewModel.FoundNewDevice(newDevices));
                        MainViewViewModel.OldDeviceReconnected(oldDevicesReconnected);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a second for updates
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task<(List<IDeviceSettings>, List<string>)> ScanOpenRGBDevices(List<IDeviceSettings> existedDevice)
        {
            //stop every existed device openrgb stream
            existedDevice.ForEach(d => d.IsTransferActive = false);
            //wait for all process to finish
            Thread.Sleep(500);
            var newDevicesDetected = new List<IDeviceSettings>();
            var oldDeviceReconnected = new List<string>();
            if (!AmbinityClient.IsInitialized && !AmbinityClient.IsInitializing)
            {
                await AmbinityClient.RefreshTransferState();
            }
            var detectedDevices = AmbinityClient.ScanNewDevice();
            if (detectedDevices != null)
            {
                foreach (var openRGBDevice in detectedDevices)
                {
                    try
                    {
                        var deviceName = openRGBDevice.Name.ToValidFileName();
                        var deviceUID = Guid.NewGuid().ToString();
                        if (existedDevice.Any(d => d.OutputPort == deviceName + openRGBDevice.Location && d.IsTransferActive == false))
                        {
                            oldDeviceReconnected.Add(deviceName + openRGBDevice.Location);
                            continue;
                        }
                        var convertedDevice = new SlaveDeviceHelpers().DefaultCreateOpenRGBDevice(openRGBDevice.Type, deviceName, openRGBDevice.Location, openRGBDevice.Serial, deviceUID);
                        var zonesData = new ZoneData[openRGBDevice.Zones.Length];
                        for (var i = 0; i < openRGBDevice.Zones.Length; i++)
                        {
                            var currentZone = openRGBDevice.Zones[i];
                            ZoneData currentZoneData = null;
                            switch (currentZone.Type)
                            {
                                case OpenRGB.NET.Enums.ZoneType.Single:
                                    currentZoneData = new ZoneData(currentZone.Name, 1, 1, 50, 50);
                                    break;

                                case OpenRGB.NET.Enums.ZoneType.Linear:
                                    currentZoneData = new ZoneData(currentZone.Name, (int)currentZone.LedCount, 1, (int)currentZone.LedCount * 50, 50);
                                    break;

                                case OpenRGB.NET.Enums.ZoneType.Matrix:
                                    currentZoneData = new ZoneData(currentZone.Name, (int)currentZone.MatrixMap.Width, (int)currentZone.MatrixMap.Height, (int)currentZone.MatrixMap.Width * 50, (int)currentZone.MatrixMap.Height * 50);
                                    break;
                            }
                            zonesData[i] = currentZoneData;
                        }
                        convertedDevice.DashboardWidth = 230;
                        convertedDevice.DashboardHeight = 270;
                        convertedDevice.AvailableControllers[0].Outputs[0].SlaveDevice = new SlaveDeviceHelpers().DefaultCreatedSlaveDevice("Generic LED Strip", SlaveDeviceTypeEnum.LEDStrip, zonesData);
                        convertedDevice.UpdateChildSize();
                        newDevicesDetected.Add(convertedDevice);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Parameter is Incorrect for OpenRGB Device");
                        //something error with one or more parametters
                        continue;
                    }

                }

            }
            _openRGBIsInit = true;
            return await Task.FromResult((newDevicesDetected, oldDeviceReconnected));
        }
        private static object _syncRoot = new object();
        public void Stop()
        {
            Log.Information("Stop called for Device Discovery");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            tokenSource?.Cancel();
            tokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }
        SerialDeviceDetection SerialDeviceDetector { get; set; }
        private async Task<(List<IDeviceSettings>, List<string>)> ScanSerialDevice()
        {
            var newDevicesDetected = new List<IDeviceSettings>();
            var oldDeviceReconnected = new List<string>();
            _isSerialScanCompelete = false;
            var jobTask = Task.Run(() =>
            {
                // Organize critical sections around logical serial port operations somehow.
                lock (_syncRoot)
                {
                    return SerialDeviceDetector.DetectedDevices;
                }
            });
            if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, token)))
            {
                // Timeout;
                _isSerialScanCompelete = true;

            }
            var devices = await jobTask;
            if (devices.Item1.Count == 0 && devices.Item2.Count == 0)
            {
                // HandyControl.Controls.MessageBox.Show("Unable to detect any supported device, try adding manually", "No Compatible Device Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                _isSerialScanCompelete = true;

            }
            else
            {
                foreach (var device in devices.Item1)
                {
                    Log.Information("SerialDeviceDetection Found New Device");
                    Log.Information("Name: " + device.DeviceName);
                    Log.Information("ID: " + device.DeviceSerial);
                    Log.Information("Firmware Version: " + device.FirmwareVersion);
                    Log.Information("---------------");
                    // MainViewViewModel.SetSearchingScreenProgressText("Found new device: " + device.DeviceName + ". Address: " + device.OutputPort);
                    Log.Information("Device: " + device.DeviceName + " is a new device at: " + device.OutputPort);
                    newDevicesDetected.Add(device);
                }
                foreach (var device in devices.Item2)
                {
                    // MainViewViewModel.SetSearchingScreenProgressText("Device reconnected: " + device.DeviceName + ". Address: " + device.OutputPort);
                    Log.Information(device + " is connected");
                    oldDeviceReconnected.Add(device);
                }

                tokenSource.Cancel();
                _isSerialScanCompelete = true;

            }
            return await Task.FromResult((newDevicesDetected, oldDeviceReconnected));
        }
    }
}