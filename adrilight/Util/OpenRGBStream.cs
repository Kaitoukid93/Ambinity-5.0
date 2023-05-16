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
using System.Collections.Generic;
using adrilight.Settings;
using Renci.SshNet.Messages;

namespace adrilight
{

    internal sealed class OpenRGBStream : IDisposable, ISerialStream
    {


        private ILogger _log = LogManager.GetCurrentClassLogger();

        public OpenRGBStream(IDeviceSettings deviceSettings, IGeneralSettings generalSettings, IAmbinityClient ambinityClient)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            AmbinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));

            // DeviceSpotSets = deviceSpotSets ?? throw new ArgumentNullException(nameof(deviceSpotSets));
            DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            DeviceSettings.DeviceState = DeviceStateEnum.Normal;
            RefreshTransferState();

            _log.Info($"OpenRGBStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IAmbinityClient AmbinityClient { get; }

        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeviceSettings.IsTransferActive):
                case nameof(DeviceSettings.OutputPort):
                case nameof(DeviceSettings.DeviceState):
                    RefreshTransferState();
                    break;
            }
        }



        private void RefreshTransferState()
        {

            if (DeviceSettings.IsTransferActive && DeviceSettings.DeviceState == DeviceStateEnum.Normal) // normal scenario
            {
                if (IsRunning)
                {
                    Stop(); // stop current running if worker thread is alive.
                }
                if (AmbinityClient.Client != null && AmbinityClient.Client.Connected)
                {

                    //start it
                    //find out which position 
                    lock (AmbinityClient.Lock)
                    {
                        for (var i = 0; i < AmbinityClient.Client.GetControllerCount(); i++)
                        {
                            var device = AmbinityClient.Client.GetControllerData(i);
                            var deviceName = device.Name.ToValidFileName();
                            if (DeviceSettings.DeviceName + DeviceSettings.OutputPort == deviceName + device.Location)
                                //so we're at i
                                index = i;

                        }
                    }

                    _log.Debug("starting the OpenRGB stream for device Name : " + DeviceSettings.DeviceName);
                    Start();
                }
                else
                {
                    DeviceSettings.IsTransferActive = false;

                }
            }

            else if (!DeviceSettings.IsTransferActive && IsRunning) // user requesting for stop transfer state, this is due to changing comport or user intend to stop the serial stream.
            {
                //stop it
                _log.Debug("stopping the serial stream");
                Stop();
            }
            else if (DeviceSettings.IsTransferActive && DeviceSettings.DeviceState == DeviceStateEnum.Sleep) // computer susped or app exit, this could be an event from sleep button ( not available at the moment)
            {
                // this is handled by GetOutputStream at the moment.
                // change output stream to black or sentry.
                // stop the serial stream.
            }
            else if (DeviceSettings.IsTransferActive && DeviceSettings.DeviceState == DeviceStateEnum.DFU) // this is only requested by dfu or fwupgrade button.
            {
                Stop();
                Thread.Sleep(1000);
            }

        }


        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;


        private int index;

        public void Start()
        {
            _log.Debug("Start called.");
            if (_workerThread != null) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(DoWork) {
                Name = "Serial sending",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            WinApi.TimeBeginPeriod(1);

            // The call has failed

            _workerThread.Start(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _log.Debug("Stop called.");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;




        private OpenRGB.NET.Models.Color[] GetOutputStream(ISlaveDevice device)
        {
            var currentDevice = device as ARGBLEDSlaveDevice;
            int ledCount = currentDevice.LEDCount;
            OpenRGB.NET.Models.Color[] outputColor = new OpenRGB.NET.Models.Color[ledCount];
            foreach (var zone in currentDevice.ControlableZones)
            {
                var currentZone = zone as LEDSetup;
                lock (currentZone.Lock)
                {
                    var isEnabled = currentZone.IsEnabled;
                    var parrentIsEnabled = DeviceSettings.IsEnabled;
                    var allBlack = true;
                    //}
                    switch (DeviceSettings.DeviceState)
                    {
                        case DeviceStateEnum.Normal: // get data from ledsetup
                            if (!DeviceSettings.IsEnabled || !currentZone.IsEnabled)
                            {

                                currentZone.DimLED(0.9f);

                            }

                            foreach (DeviceSpot spot in currentZone.Spots)
                            {

                                var RGBOrder = currentZone.RGBLEDOrder;
                                var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.Red, spot.Green, spot.Blue);
                                outputColor[spot.Index] = new OpenRGB.NET.Models.Color(reOrderedColor[0], reOrderedColor[1], reOrderedColor[2]);
                                allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                            }
                            break;
                        case DeviceStateEnum.Sleep: // send black frame data
                            foreach (DeviceSpot spot in currentZone.Spots)
                            {

                                //switch (currentZone.SleepMode)
                                //{
                                //    case 0:
                                //        if (isEnabled && parrentIsEnabled)
                                //        {


                                //            outputColor[counter++] = new OpenRGB.NET.Models.Color(0, 0, 0);

                                //        }
                                //        break;
                                //    case 1:
                                //        if (isEnabled && parrentIsEnabled)
                                //        {
                                //            var RGBOrder = currentOutput.OutputRGBLEDOrder;
                                //            var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.SentryRed, spot.SentryGreen, spot.SentryBlue);

                                //            outputColor[counter++] = new OpenRGB.NET.Models.Color(reOrderedColor[0], reOrderedColor[1], reOrderedColor[2]);

                                //        }
                                //        break;
                                //}

                            }
                            break;

                    }




                }
            }

            return outputColor;




        }

        private byte[] ReOrderSpotColor(RGBLEDOrderEnum order, byte r, byte g, byte b)
        {
            byte[] reOrderedColor = new byte[3];
            switch (order)
            {
                case RGBLEDOrderEnum.RGB:
                    reOrderedColor[0] = r;
                    reOrderedColor[1] = g;
                    reOrderedColor[2] = b;
                    break;
                case RGBLEDOrderEnum.RBG:
                    reOrderedColor[0] = r;
                    reOrderedColor[1] = b;
                    reOrderedColor[2] = g;
                    break;
                case RGBLEDOrderEnum.BGR:
                    reOrderedColor[0] = b;
                    reOrderedColor[1] = g;
                    reOrderedColor[2] = r;
                    break;
                case RGBLEDOrderEnum.BRG:
                    reOrderedColor[0] = b;
                    reOrderedColor[1] = r;
                    reOrderedColor[2] = g;
                    break;
                case RGBLEDOrderEnum.GRB:
                    reOrderedColor[0] = g;
                    reOrderedColor[1] = r;
                    reOrderedColor[2] = b;
                    break;
                case RGBLEDOrderEnum.GBR:
                    reOrderedColor[0] = g;
                    reOrderedColor[1] = b;
                    reOrderedColor[2] = r;
                    break;
            }
            return reOrderedColor;
        }
        public void DFU()

        {
            //nothing to do here
        }

        public bool IsValid() => AmbinityClient.Client != null && AmbinityClient.Client.Connected;
        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;

            if (AmbinityClient.Client == null)
            {
                _log.Warn("Cannot start the socket sending because the Ambinity Client is null.");
                return;
            }


            //retry after exceptions...
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {




                    while (!cancellationToken.IsCancellationRequested)
                    {

                        var deviceColors = new List<OpenRGB.NET.Models.Color>();
                        //send frame data

                        var outputColor = GetOutputStream(DeviceSettings.AvailableLightingOutputs[0].SlaveDevice);
                        if (outputColor != null)
                        {
                            foreach (var color in outputColor)
                            {
                                    deviceColors.Add(color == null ? new OpenRGB.NET.Models.Color(0, 0, 0) : color);
                            }
                               
                        }


                        lock (AmbinityClient.Lock)
                        {
                            AmbinityClient.Client.UpdateLeds(index, deviceColors.ToArray());
                        }
                        Thread.Sleep(30);
                    }

                }

                catch (OperationCanceledException)
                {
                    _log.Debug("OperationCanceledException catched. returning.");

                    return;
                }
                catch (Exception ex)
                {



                    _log.Debug(ex, "Exception catched. OpenRGB proccess could be deleted");

                    //allow the system some time to recover
                    Thread.Sleep(500);
                    Stop();
                    // Dispose();
                }
                finally
                {
                    //do nothing at the moment because AmbinityCilent is holding the Client


                }

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












