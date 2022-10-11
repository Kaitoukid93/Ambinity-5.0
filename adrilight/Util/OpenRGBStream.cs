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

namespace adrilight
{
    internal sealed class
        OpenRGBStream : IDisposable, IOpenRGBStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public OpenRGBStream(IDeviceSettings[] deviceSettings, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 30, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));//rescan device may took longer and user manualy start server also

            GeneralSettings.PropertyChanged += UserSettings_PropertyChanged;
            // IsInitialized = false;
            AvailableDevices = new List<IDeviceSettings>();
            foreach (var device in DeviceSettings)
            {
                if (device.DeviceConnectionType == "OpenRGB")
                    AvailableDevices.Add(device);
            }
            if(AvailableDevices.Count > 0)
            RefreshTransferState();


            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings[] DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSpotSet[] DeviceSpotSet { get; set; }
        private IDeviceSettings[] ReorderedDevices { get; set; }
        private List<IDeviceSettings> AvailableDevices { get; set; }

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

                try
                {
                    if (AmbinityClient != null)
                        AmbinityClient.Dispose();
                    var attempt = 0;
                    _retryPolicy.Execute(() => RefreshOpenRGBDeviceState()); _log.Info($"Attempt {++attempt}");

                    if (AmbinityClient != null)
                    {
                        
                        var devices = AmbinityClient.GetAllControllerData();
                        int index = 0;
                        ReorderedDevices = new DeviceSettings[devices.Length];
                        foreach (var device in devices)
                        {

                            for(var i = 0;i<device.Modes.Length;i++)
                            {

                                Debug.WriteLine(device.Modes[i].Name.ToString());
                                if(device.Modes[i].Name=="Direct")
                                {
                                    AmbinityClient.SetMode(index,i);
                                }
                            }
                                
                            
                            _log.Info($"Device found : " + device.Name.ToString() + "At index: " + index);
                             
                            var deviceUID = device.Name + device.Version + device.Location;
                            foreach(var convertedDevice in AvailableDevices)
                            {
                                if (convertedDevice.DeviceUID == deviceUID)
                                {
                                    convertedDevice.IsTransferActive = true;
                                    ReorderedDevices[index] = convertedDevice;
                                   
                                    
                                    
                                }
                            }
                            index++;
                        } 
                        Start();


                    }


                    IsInitialized = true;
                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
                catch (System.Net.Sockets.SocketException)
                {
                    HandyControl.Controls.MessageBox.Show("Khởi động lại ứng dụng OpenRGB và Start Server");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
            }

            else if (!GeneralSettings.IsOpenRGBEnabled) // show message require user to turn on Using OpenRGB
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("Bạn phải bật Enable OpenRGB để có thể tìm thấy các thiết bị OpenRGB! bạn có muốn bật không?", "OpenRGB is disabled",MessageBoxButton.YesNo,MessageBoxImage.Question);
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


        public OpenRGB.NET.Models.Device[] GetDevices {
            get
            {
                return AmbinityClient.GetAllControllerData();
            }
            
        }


        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        public OpenRGBClient RefreshOpenRGBDeviceState()//init
        {
            if (AmbinityClient != null)
                AmbinityClient.Dispose();
           AmbinityClient = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);
            return AmbinityClient;
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
                            for(int i=0;i<ReorderedDevices.Length;i++)
                            {
                                if (ReorderedDevices[i]!=null && ReorderedDevices[i].IsEnabled)
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
                    RefreshTransferState();
                }







            
        }





        public void Dispose()
        {
            Dispose(true);
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
