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
            deviceSettings.CurrentState = State.normal;
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
                case nameof(DeviceSettings.CurrentState):
                    RefreshTransferState();
                    break;
                case nameof(DeviceSettings.IsUnionMode):

                    RefreshTransferState();
                    break;
            }
        }



        private void RefreshTransferState()
        {

            if (DeviceSettings.IsTransferActive && DeviceSettings.CurrentState == State.normal) // normal scenario
            {
                if (IsRunning)
                {
                    Stop(); // stop current running if worker thread is alive.
                }
                if (AmbinityClient.Client != null && AmbinityClient.Client.Connected)
                {

                    //start it
                    //find out which position 
                    lock(AmbinityClient.Lock)
                    {
                        for (var i = 0; i < AmbinityClient.Client.GetControllerCount(); i++)
                        {
                            var device = AmbinityClient.Client.GetControllerData(i);
                            var uid = device.Name + device.Version + device.Location;
                            if (DeviceSettings.DeviceUID == uid)
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
            else if (DeviceSettings.IsTransferActive && DeviceSettings.CurrentState == State.sleep) // computer susped or app exit, this could be an event from sleep button ( not available at the moment)
            {
                // this is handled by GetOutputStream at the moment.
                // change output stream to black or sentry.
                // stop the serial stream.
            }
            else if (DeviceSettings.IsTransferActive && DeviceSettings.CurrentState == State.dfu) // this is only requested by dfu or fwupgrade button.
            {
                Stop();
                Thread.Sleep(1000);
            }
            else if (DeviceSettings.IsTransferActive && DeviceSettings.CurrentState == State.speed) // this is only requested by dfu or fwupgrade button.
            {
                DeviceSettings.IsLoadingSpeed = true;
                Stop();
                Thread.Sleep(500);
                DeviceSettings.CurrentState = State.normal;
                DeviceSettings.RefreshDeviceActualSpeedAsync();
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




        private OpenRGB.NET.Models.Color[] GetOutputStream(IOutputSettings output)
        {
            var currentOutput = output;
            OpenRGB.NET.Models.Color[] outputColor = new OpenRGB.NET.Models.Color[currentOutput.OutputLEDSetup.Spots.Length];
            int counter = 0;
            lock (currentOutput.OutputLEDSetup.Lock)
            {
                var isEnabled = currentOutput.OutputIsEnabled;
                var parrentIsEnabled = DeviceSettings.IsEnabled;
                var allBlack = true;
                //}
                switch (DeviceSettings.CurrentState)
                {
                    case State.normal: // get data from ledsetup
                        if (!DeviceSettings.IsEnabled || !output.OutputIsEnabled)
                        {

                            output.OutputLEDSetup.IndicateMissingValues();

                        }


                        foreach (DeviceSpot spot in currentOutput.OutputLEDSetup.Spots)
                        {

                            var RGBOrder = currentOutput.OutputRGBLEDOrder;
                            var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.Red, spot.Green, spot.Blue);


                            outputColor[counter++] = new OpenRGB.NET.Models.Color(reOrderedColor[0], reOrderedColor[1], reOrderedColor[2]);

                            allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                        }
                        break;
                    case State.sleep: // send black frame data
                        foreach (DeviceSpot spot in currentOutput.OutputLEDSetup.Spots)
                        {

                            switch (currentOutput.SleepMode)
                            {
                                case 0:
                                    if (isEnabled && parrentIsEnabled)
                                    {


                                        outputColor[counter++] = new OpenRGB.NET.Models.Color(0, 0, 0);

                                    }
                                    break;
                                case 1:
                                    if (isEnabled && parrentIsEnabled)
                                    {
                                        var RGBOrder = currentOutput.OutputRGBLEDOrder;
                                        var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.SentryRed, spot.SentryGreen, spot.SentryBlue);

                                        outputColor[counter++] = new OpenRGB.NET.Models.Color(reOrderedColor[0], reOrderedColor[1], reOrderedColor[2]);

                                    }
                                    break;
                            }

                        }
                        break;

                }



                return outputColor;
            }





        }

        private byte[] ReOrderSpotColor(string order, byte r, byte g, byte b)
        {
            byte[] reOrderedColor = new byte[3];
            switch (order)
            {
                case "RGB"://do nothing
                    reOrderedColor[0] = r;
                    reOrderedColor[1] = g;
                    reOrderedColor[2] = b;
                    break;
                case "RBG"://do nothing
                    reOrderedColor[0] = r;
                    reOrderedColor[1] = b;
                    reOrderedColor[2] = g;
                    break;
                case "BGR"://do nothing
                    reOrderedColor[0] = b;
                    reOrderedColor[1] = g;
                    reOrderedColor[2] = r;
                    break;
                case "BRG"://do nothing
                    reOrderedColor[0] = b;
                    reOrderedColor[1] = r;
                    reOrderedColor[2] = g;
                    break;
                case "GRB"://do nothing
                    reOrderedColor[0] = g;
                    reOrderedColor[1] = r;
                    reOrderedColor[2] = b;
                    break;
                case "GBR"://do nothing
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

            bool isUnion = DeviceSettings.IsUnionMode;

            //retry after exceptions...
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {




                    while (!cancellationToken.IsCancellationRequested)
                    {


                        //send frame data
                        if (isUnion)
                        {
                            var deviceColors = new List<OpenRGB.NET.Models.Color>();
                            for (int i = 0; i < DeviceSettings.AvailableOutputs.Length; i++)
                            {

                                var outputColor = GetOutputStream(DeviceSettings.UnionOutput);
                                if (outputColor != null)
                                {
                                    foreach (var color in outputColor)
                                        deviceColors.Add(color);


                                }

                            }
                            lock (AmbinityClient.Lock)
                            {
                                AmbinityClient.Client.UpdateLeds(index, deviceColors.ToArray());
                            }

                        }

                        else
                        {
                            foreach (var output in DeviceSettings.AvailableOutputs)
                            {

                                var deviceColors = new List<OpenRGB.NET.Models.Color>();
                                for (int i = 0; i < DeviceSettings.AvailableOutputs.Length; i++)
                                {

                                    var outputColor = GetOutputStream(DeviceSettings.AvailableOutputs[i]);
                                    if (outputColor != null)
                                    {
                                        foreach (var color in outputColor)
                                            deviceColors.Add(color);


                                    }

                                }
                                lock(AmbinityClient.Lock)
                                {
                                    AmbinityClient.Client.UpdateLeds(index, deviceColors.ToArray());
                                }
                                

                            }
                        }
                        Thread.Sleep(15);
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












