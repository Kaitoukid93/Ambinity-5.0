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
        OpenRGBStream : IDisposable, IOpenRGBStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ORGBJsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenRGB\\");
        private string ORGBPath => Path.Combine(JsonPath, "ORGB\\");
        private string ORGBExeFolderNameAndPath => Path.Combine(ORGBPath, "OpenRGB\\");
        private string ORGBExeFileNameAndPath => Path.Combine(ORGBExeFolderNameAndPath, "OpenRGB Windows 64-bit\\OpenRGB.exe");

        public OpenRGBStream(IDeviceSettings[] deviceSettings, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));//rescan device may took longer and user manualy start server also

            GeneralSettings.PropertyChanged += UserSettings_PropertyChanged;
            // IsInitialized = false;
            AvailableDevices = new List<IDeviceSettings>();
            foreach (var device in DeviceSettings)
            {
                if (device.DeviceConnectionType == "OpenRGB")
                    AvailableDevices.Add(device);
            }
            //if (AvailableDevices.Count > 0) // add more condition about 1st time installing when no device found
            //the logic is, scan for serial device first, if hubV3 found, only then start openRGB 

            RefreshTransferState();

            // Do things here.
            // NOTE: You may need to invoke this to your main thread depending on what you're doing




            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings[] DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSpotSet[] DeviceSpotSet { get; set; }
        private IDeviceSettings[] ReorderedDevices { get; set; }
        private List<IDeviceSettings> AvailableDevices { get; set; }
        private System.Diagnostics.Process _oRGBProcess;
        public System.Diagnostics.Process ORGBProcess {
            get { return _oRGBProcess; }
            set { _oRGBProcess = value; }
        }
        public bool IsInitialized { get; set; }
        private OpenRGBClient _ambinityClient;
        public OpenRGBClient AmbinityClient {
            get { return _ambinityClient; }
            set { _ambinityClient = value; }
        }

        //private int deviceID = 0;

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

            foreach (var device in AvailableDevices)
            {
                device.IsTransferActive = false;
            }
            if (!IsInitialized && GeneralSettings.IsOpenRGBEnabled) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
            {
                // check if openRGB folder is created


                //if (Directory.Exists(ORGBJsonPath))
                //{
                //    //if openRGB is created, check if the config file is properly configurated
                //    // or just overwrite it for the first time
                //    // check if this is first time the app run after install
                //    if (GeneralSettings.OpenRGBConfigRequested)
                //    {

                //        try
                //        {
                //            CopyResource("adrilight.OpenRGB.OpenRGB.json", Path.Combine(ORGBJsonPath, "OpenRGB.json"));

                //            //after this there is no need to config openRGB anymore, we turn this property off
                //            GeneralSettings.OpenRGBConfigRequested = false;
                //            //until user request to reconfig OpenRGB
                //        }
                //        catch (ArgumentException)
                //        {
                //            //show messagebox no firmware found for this device
                //            return;
                //        }
                //    }
                //    // if any error occur, require user to exit open rgb
                //}
                //else
                //{
                //    //create OpenRGB Config directory
                //    Directory.CreateDirectory(ORGBJsonPath);
                //    //coppy config file

                //}

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
                        //// Open an existing zip file for reading
                        //  ZipStorer zip = ZipStorer.Open(Path.Combine(ORGBExeFileNameAndPath, "OpenRGB.zip"), FileAccess.Read);

                        ////  // Read the central directory collection
                        // List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();


                        //  foreach (ZipStorer.ZipFileEntry entry in dir)
                        // {
                        ////     //extract every single file
                        //       zip.ExtractFile(entry, Path.Combine(ORGBExeFileNameAndPath,entry.FilenameInZip));
                        //         break;

                        // }
                        // zip.Close();










                    }
                    catch (ArgumentException)
                    {
                        //show messagebox no firmware found for this device
                        return;
                    }
                }


                try
                {
                    if (AmbinityClient != null)
                        AmbinityClient.Dispose();
                    var attempt = 0;
                    _retryPolicy.Execute(() => RefreshOpenRGBDeviceState()); _log.Info($"Attempt {++attempt}");




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
                    // Enable OpenRGB
                    GeneralSettings.IsOpenRGBEnabled = true;
                //else if (result == MessageBoxResult.No)
                //{
                //    _log.Debug("stopping the serial stream");
                //    Stop();
                //}
                //do nothing
                //stop it

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

        public List<OpenRGB.NET.Models.Device> ScanNewDevice()
        {

            // kill current openRGB process first
            // need to stop the stream first
            //Stop();
            //ORGBProcess.Kill();
            //IsInitialized = false;
            ////wait a bit
            //Thread.Sleep(1000);
            Stop();
            var AvailableOpenRGBDevices = new List<Device>();
            if (AmbinityClient != null)
                AmbinityClient.Dispose();
            AmbinityClient = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);
            //RefreshTransferState();
            Start();
            if (AmbinityClient != null && AmbinityClient.Connected == true)
            {

                var newOpenRGBDevices = AmbinityClient.GetAllControllerData();



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

                return AvailableOpenRGBDevices;
            }
            else
            {
                return null;
            }
            //WriteOpenRGBDeviceInfoJson();


        }


        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        public void RefreshOpenRGBDeviceState()//init
        {
            if (AmbinityClient != null)
                AmbinityClient.Dispose();
            AmbinityClient = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);
            if (AmbinityClient != null)
            {
                //check if we get any device from Openrgb
                if (AmbinityClient.GetControllerCount() > 0)
                {

                    var devices = AmbinityClient.GetAllControllerData();
                    int index = 0;
                    ReorderedDevices = new DeviceSettings[devices.Length];
                    foreach (var device in devices)
                    {

                        for (var i = 0; i < device.Modes.Length; i++)
                        {

                            Debug.WriteLine(device.Modes[i].Name.ToString());
                            if (device.Modes[i].Name == "Direct")
                            {
                                AmbinityClient.SetMode(index, i);
                            }
                        }


                        _log.Info($"Device found : " + device.Name.ToString() + "At index: " + index);

                        var deviceUID = device.Name + device.Version + device.Location;
                        foreach (var convertedDevice in AvailableDevices)
                        {
                            if (deviceUID == convertedDevice.DeviceUID) // this is known device
                            {
                                convertedDevice.IsTransferActive = true;
                                ReorderedDevices[index] = convertedDevice;



                            }
                            else
                            {
                                // this is new device from ORGB, prompt asking user to add or not?

                            }

                        }
                        index++;
                    }
                    Start();

                }
                else // this could happen due to device scanning is in progress
                {
                    //dispose the client
                    AmbinityClient.Dispose();
                    throw (new Exception("ORGB busy"));
                    // throw some type of exception , now retry policy will catch the exception and retry
                    // the thing is, how many retrying is enough before throwing message box "no device detected"??


                }


            }

        }
        public void DFU()
        {
            //nothing to do here with OpenRGB Devices
        }

        public void Start()
        {
            _log.Debug("Start called.");


            _cancellationTokenSource = new CancellationTokenSource();
            WinApi.TimeBeginPeriod(1);
            if (_workerThread != null)
            {
                _workerThread = null;
            }

            _workerThread = new Thread(DoWork) {
                Name = "Serial sending",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start(_cancellationTokenSource.Token);




            // The call has failed


        }

        public void Stop()
        {
            _log.Debug("Stop called.");
            IsInitialized = false;
            if (AmbinityClient != null)
                AmbinityClient.Dispose();
            if (_workerThread == null) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }









        private OpenRGB.NET.Models.Color[] GetOutputStream(IOutputSettings currentOutput)//output is zone
        {

            OpenRGB.NET.Models.Color[] outputColor = new OpenRGB.NET.Models.Color[currentOutput.OutputLEDSetup.Spots.Length];
            var client = AmbinityClient;
            if (client != null)
            {


                lock (currentOutput.OutputLEDSetup.Lock)
                {
                    int counter = 0;
                    foreach (DeviceSpot spot in currentOutput.OutputLEDSetup.Spots)
                    {
                        var color = new OpenRGB.NET.Models.Color(spot.Red, spot.Green, spot.Blue);
                        outputColor[counter++] = color;

                    }

                }

            }

            return outputColor;

        }


        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;


            try
            {
                var client = AmbinityClient;

                if (client.Connected)
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //int index = 0;
                        //foreach (var device in AvailableDevices)
                        //{
                        //    foreach (var output in device.AvailableOutputs)
                        //    {
                        //        var outputColor = GetOutputStream(output);
                        //        if (outputColor != null)
                        //        {

                        //            client.UpdateZone(index, output.OutputID, outputColor);
                        //        }

                        //        Thread.Sleep(10);
                        //    }
                        //    index++;
                        //}
                        for (int i = 0; i < ReorderedDevices.Length; i++)
                        {
                            if (ReorderedDevices[i] != null && ReorderedDevices[i].IsEnabled)
                            {
                                var deviceColors = new List<OpenRGB.NET.Models.Color>();
                                foreach (var output in ReorderedDevices[i].AvailableOutputs)
                                {
                                    var outputColor = GetOutputStream(output);
                                    if (outputColor != null)
                                    {
                                        foreach (var color in outputColor)
                                            deviceColors.Add(color);


                                    }


                                }

                                client.UpdateLeds(i, deviceColors.ToArray());
                            }

                        }
                        Thread.Sleep(17);

                    }
                }

            }
            catch (TimeoutException)
            {
                HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                //foreach (var device in DeviceSettings)
                //{
                //    if (device.DeviceSerial != "151293")
                //        device.IsConnected = false;
                //}
                Thread.Sleep(500);
                Stop();
            }
            catch (Exception ex)
            {
                //HandyControl.Controls.MessageBox.Show(ex.ToString());
                //foreach (var device in DeviceSettings)
                //{
                //    if (device.DeviceSerial != "151293")
                //        device.IsConnected = false;
                //}   

                Thread.Sleep(500);
                IsInitialized = false;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
                Thread.Sleep(500);
                ORGBProcess.Kill();
                //wait a bit
                Thread.Sleep(1000);
                RefreshTransferState();
            }








        }





        public void Dispose()
        {
            Dispose(true);
            ORGBProcess.Kill();
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

            }
        }
    }
}
