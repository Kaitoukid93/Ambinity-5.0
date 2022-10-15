using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using NLog;
using System.Buffers;
using adrilight.Util;
using System.Linq;
using Newtonsoft.Json;
using System.Windows;
using adrilight.Spots;
using OpenRGB.NET;
using System.Collections.Generic;
using Polly;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using OpenRGB.NET.Models;
using System.Threading.Tasks;

namespace adrilight
{
    internal sealed class
        AmbinityClient : IDisposable, IAmbinityClient
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ORGBJsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenRGB\\");
        private string ORGBPath => Path.Combine(JsonPath, "ORGB\\");
        private string ORGBExeFolderNameAndPath => Path.Combine(ORGBPath, "OpenRGB\\");
        private string ORGBExeFileNameAndPath => Path.Combine(ORGBExeFolderNameAndPath, "OpenRGB Windows 64-bit\\OpenRGB.exe");

        public AmbinityClient(IDeviceSettings[] deviceSettings, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));//rescan device may took longer and user manualy start server also

            GeneralSettings.PropertyChanged += UserSettings_PropertyChanged;
            // IsInitialized = false;


            //if (AvailableDevices.Count > 0) // add more condition about 1st time installing when no device found
            //the logic is, scan for serial device first, if hubV3 found, only then start openRGB 
            RefreshTransferState();
            _log.Info($"SerialStream created.");
        }
        //Dependency Injection//
        private IDeviceSettings[] DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSettings[] ReorderedDevices { get; set; }
        public List<IDeviceSettings> AvailableDevices { get; set; }
        private System.Diagnostics.Process _oRGBProcess;
        public System.Diagnostics.Process ORGBProcess {
            get { return _oRGBProcess; }
            set { _oRGBProcess = value; }
        }
        public bool IsInitialized { get; set; }
        private OpenRGBClient _client;
        public OpenRGBClient Client {
            get { return _client; }
            set { _client = value; }
        }


        private readonly Policy _retryPolicy;

        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case nameof(GeneralSettings.IsOpenRGBEnabled):

            //        RefreshTransferState();
            //        break;
            //}
        }

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;

        public void RefreshTransferState()
        {
            AvailableDevices = new List<IDeviceSettings>();
            foreach (var device in DeviceSettings)
            {
                if (device.DeviceConnectionType == "OpenRGB")
                    AvailableDevices.Add(device);
            }
            foreach (var device in AvailableDevices)
            {
                device.IsTransferActive = false;
            }

            if (!IsInitialized && GeneralSettings.IsOpenRGBEnabled) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
            {
                //check if OpenRGB is existed in adrilight folder

                if (File.Exists(ORGBExeFileNameAndPath))
                {
                    // now start open rgb
                    ORGBProcess = System.Diagnostics.Process.Start(ORGBExeFileNameAndPath, "--server --startminimized --gui");

                }
                else
                {
                    //coppy ORGB from resource // this action using ZipStorer
                    try
                    {
                        Directory.CreateDirectory(ORGBPath);

                        CopyResource("adrilight.Tools.OpenRGB.OpenRGB.zip", Path.Combine(ORGBPath, "OpenRGB.zip"));
                        //Create directory to extract
                        Directory.CreateDirectory(ORGBExeFolderNameAndPath);
                        //then extract
                        ZipFile.ExtractToDirectory(Path.Combine(ORGBPath, "OpenRGB.zip"), ORGBExeFolderNameAndPath);
                        //then delete the zip to prevent further conflict
                        File.Delete(Path.Combine(ORGBPath, "OpenRGB.zip"));
                    }
                    catch (ArgumentException)
                    {
                        //show messagebox no firmware found for this device
                        return;
                    }

                    ORGBProcess = System.Diagnostics.Process.Start(ORGBExeFileNameAndPath, "--server --startminimized --gui");
                }


                try
                {
                    if (Client != null)
                        Client.Dispose();
                    var attempt = 0;
                    _retryPolicy.Execute(() => RefreshOpenRGBDeviceState(true)); _log.Info($"Attempt {++attempt}");
                    IsInitialized = true;
                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy Server OpenRGB, Hãy thử thoát ứng dụng và mở lại");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
                catch (System.Net.Sockets.SocketException)
                {
                    HandyControl.Controls.MessageBox.Show("Mất kết nối ứng dụng OpenRGB, vui lòng không thoát OpenRGB khi đang sử dụng");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
                catch (Exception ex)
                {
                    if (ex.Message == "ORGB busy")
                    {
                        // no device available
                        HandyControl.Controls.MessageBox.Show("Không có thiết bị bên thứ ba nào được tìm thấy");
                    }
                }
            }

            else if (!GeneralSettings.IsOpenRGBEnabled) // show message require user to turn on Using OpenRGB
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("Bạn phải bật Enable OpenRGB để có thể tìm thấy và điều khiển các thiết bị OpenRGB! bạn có muốn bật không?", "OpenRGB is disabled", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Enable OpenRGB
                    GeneralSettings.IsOpenRGBEnabled = true;
                    RefreshTransferState();

                }
            }

        }

        private void CopyResource(string resourceName, string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.OpenWrite(file))
                {
                    resource.CopyTo(output);
                }
            }
        }
        public object Lock { get; } = new object();
        public List<OpenRGB.NET.Models.Device> ScanNewDevice()
        {
            var AvailableOpenRGBDevices = new List<Device>();
            try
            {
                if (Client != null)
                    Client.Dispose();
                Client = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);

                if (Client != null && Client.Connected == true)
                {

                    var newOpenRGBDevices = Client.GetAllControllerData();

                    foreach (var device in newOpenRGBDevices)
                    {
                        AvailableOpenRGBDevices.Add(device);
                    }
                    //check if any devices is already in the dashboard
                    foreach (var device in newOpenRGBDevices)
                    {
                        var deviceUID = device.Name + device.Version + device.Location;
                        foreach (var existedDevice in AvailableDevices.Where(p => p.DeviceConnectionType == "OpenRGB"))
                        {
                            if (deviceUID == existedDevice.DeviceUID)
                                AvailableOpenRGBDevices.Remove(device);
                        }
                    }
 
                }

            }
            catch(Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("sum ting wong");
            }

            //WriteOpenRGBDeviceInfoJson();

            return AvailableOpenRGBDevices;
        }


        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        public void RefreshOpenRGBDeviceState(bool init)//init
        {
            if (init)
            {
                if (Client != null)
                    Client.Dispose();
                Client = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);
            }

            if (Client != null)
            {
                //check if we get any device from Openrgb
                if (Client.GetControllerCount() > 0)
                {

                    var devices = Client.GetAllControllerData();
                    int index = 0;
                    ReorderedDevices = new DeviceSettings[devices.Length];
                    foreach (var device in devices)
                    {

                        for (var i = 0; i < device.Modes.Length; i++)
                        {

                            Debug.WriteLine(device.Modes[i].Name.ToString());
                            if (device.Modes[i].Name == "Direct")
                            {
                                Client.SetMode(index, i);
                            }
                        }


                        _log.Info($"Device found : " + device.Name.ToString() + "At index: " + index);

                        var deviceUID = device.Name + device.Version + device.Location;
                        foreach (var convertedDevice in AvailableDevices)
                        {
                            if (deviceUID == convertedDevice.DeviceUID) // this is known device
                            {
                                convertedDevice.IsTransferActive = true;
                                //convertedDevice.IsEnabled = true;
                                ReorderedDevices[index] = convertedDevice;



                            }
                            else
                            {
                                // this is new device from ORGB, prompt asking user to add or not?

                            }

                        }
                        index++;
                    }


                }
                else // this could happen due to device scanning is in progress
                {
                    //dispose the client
                    Client.Dispose();
                    throw (new Exception("ORGB busy"));
                    // throw some type of exception , now retry policy will catch the exception and retry
                    // the thing is, how many retrying is enough before throwing message box "no device detected"??


                }


            }

        }

        public void Dispose()
        {
            Client.Dispose();
            if (ORGBProcess != null)
                ORGBProcess.Kill();
            GC.SuppressFinalize(this);
        }

    
    }
}
