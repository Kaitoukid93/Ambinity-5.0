
using adrilight.Services.OpenRGBService;
using adrilight.View;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Services;
using adrilight_shared.Services.AdrilightStoreService;
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
using System.Xml.Linq;

namespace adrilight.Services.DeviceDiscoveryServices
{
    public class DeviceDiscovery
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        public event Action<List<String>> SerialDevicesScanComplete;
        public event Action<List<Device>> OpenRGBDevicesScanComplete;
        public DeviceDiscovery(IGeneralSettings settings,
            AmbinityClient ambinityClient
)
        {
            _generaSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ambinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
           
           
            if (_generaSettings.UsingOpenRGB)
            {

            }

        }
        private bool _openRGBIsInit = false;
        private IGeneralSettings _generaSettings;
        private AmbinityClient _ambinityClient;
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        
        
        private AdrilightSFTPClient _sftpClient;
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
    }
}