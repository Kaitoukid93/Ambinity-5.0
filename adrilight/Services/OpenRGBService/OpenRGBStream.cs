using adrilight.Services.SerialStream;
using adrilight.Util;
using adrilight_shared.Enums;
using adrilight_shared.Extensions;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Settings;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adrilight.Services.OpenRGBService
{

    internal sealed class OpenRGBStream : IDisposable, ISerialStream
    {

        public OpenRGBStream(IDeviceSettings deviceSettings, IGeneralSettings generalSettings, IAmbinityClient ambinityClient)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            AmbinityClient = ambinityClient as AmbinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));

            // DeviceSpotSets = deviceSpotSets ?? throw new ArgumentNullException(nameof(deviceSpotSets));
            DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            DeviceSettings.DeviceState = DeviceStateEnum.Normal;
            switch (deviceSettings.DeviceType.Type)
            {
                case DeviceTypeEnum.Ram:
                case DeviceTypeEnum.Dram:
                    _frameRate = 20;
                    break;
                default:
                    _frameRate = 50;
                    break;
            }
            //RefreshTransferState();
        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private int _frameRate = 60;
        private async void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeviceSettings.IsTransferActive):
                case nameof(DeviceSettings.OutputPort):
                    await RefreshTransferState();
                    break;
                case nameof(DeviceSettings.DeviceState):
                    DeviceStateChanged();
                    break;
            }
        }

        private double _dimFactor;
        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.01)
                    _dimFactor -= 0.01;
                // if (_dimFactor < 0.1)
                //  _dimMode = DimMode.Up;
            }
            else if (_dimMode == DimMode.Up)
            {
                if (_dimFactor <= 0.99)
                    _dimFactor += 0.01;
                //_dimMode = DimMode.Down;
            }
        }
        private void DeviceStateChanged()
        {

            if (DeviceSettings.DeviceState == DeviceStateEnum.Normal)
            {
                _dimMode = DimMode.Up;
                _dimFactor = 0.00;

            }
            else
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
            }
        }
        private async Task RefreshTransferState()
        {
            if (DeviceSettings.IsTransferActive) // normal scenario
            {

                if (!IsValid())
                {
                    await AmbinityClient.RefreshTransferState();
                }
                if (IsValid())
                {
                //find which position this device is in the OpenRGB App
                try
                    {
                        lock (AmbinityClient.Lock)
                        {
                            for (var i = 0; i < AmbinityClient.Client.GetControllerCount(); i++)
                            {
                                var device = AmbinityClient.Client.GetControllerData(i);
                                var deviceName = device.Name.ToValidFileName();
                                if (DeviceSettings.OutputPort == deviceName + device.Location)
                                {
                                    //so we're at i
                                    index = i;
                                    break;
                                }
                            }
                        }
                        Log.Information("starting the OpenRGB stream for device Name : " + DeviceSettings.DeviceName);
                        Start();
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex);
                    }

                }
                else
                {
                    DeviceSettings.IsTransferActive = false;
                }
            }

            else if (!DeviceSettings.IsTransferActive && IsRunning) // user requesting for stop transfer state, this is due to changing comport or user intend to stop the serial stream.
            {
                //stop it
                Log.Information("stopping the OpenRGB Stream");
                Stop();
                Thread.Sleep(500);
            }

        }

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;


        private int index;

        public void Start()
        {
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
            var ledCount = currentDevice.LEDCount;
            var outputColor = new OpenRGB.NET.Models.Color[ledCount];
            var RGBOrder = currentDevice.RGBLEDOrder;
            DimLED();
            foreach (var zone in currentDevice.ControlableZones)
            {
                var currentZone = zone as LEDSetup;
                lock (currentZone.Lock)
                {
                    var allBlack = true;

                    foreach (DeviceSpot spot in currentZone.Spots)
                    {
                        if (spot.IsEnabled)
                        {
                            ApplyColorWhitebalance(spot.Red, spot.Green, spot.Blue,
                              currentDevice.WhiteBalanceRed, currentDevice.WhiteBalanceGreen, currentDevice.WhiteBalanceBlue,
                              out var FinalR, out var FinalG, out var FinalB);
                            var reOrderedColor = ReOrderSpotColor(RGBOrder, FinalR, FinalG, FinalB);
                            //get data
                            outputColor[spot.Index] = new OpenRGB.NET.Models.Color((byte)(reOrderedColor[0] * _dimFactor), (byte)(reOrderedColor[1] * _dimFactor), (byte)(reOrderedColor[2] * _dimFactor));
                            // aliveSpotCounter++;
                        }
                        allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                    }
                }
            }
            return outputColor;
        }
        private void ApplyColorWhitebalance(byte r, byte g, byte b, int whiteBalanceRed, int whiteBalanceGreen, int whiteBalanceBlue, out byte finalR, out byte finalG, out byte finalB)
        {

            finalR = (byte)(r * whiteBalanceRed / 100);
            finalG = (byte)(g * whiteBalanceGreen / 100);
            finalB = (byte)(b * whiteBalanceBlue / 100);
        }
        private byte[] ReOrderSpotColor(RGBLEDOrderEnum order, byte r, byte g, byte b)
        {
            var reOrderedColor = new byte[3];
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
        public bool IsValid() => AmbinityClient.Client != null && AmbinityClient.Client.Connected;
        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;

            if (AmbinityClient.Client == null)
            {
                Log.Warning("Cannot start the socket sending because the Ambinity Client is null.");
                return;
            }

            //retry after exceptions...
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var ledCount = AmbinityClient.Client.GetControllerData(index).Leds.Count();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var deviceColors = new List<OpenRGB.NET.Models.Color>();
                        //send frame data
                        var outputColor = new List<OpenRGB.NET.Models.Color>();
                        foreach (var output in DeviceSettings.AvailableLightingOutputs)
                        {
                            var currentSegmentColor = GetOutputStream(output.SlaveDevice);
                            outputColor.AddRange(currentSegmentColor.ToList());
                        }

                        if (outputColor.Count > 0)
                        {
                            foreach (var color in outputColor)
                            {
                                deviceColors.Add(color == null ? new OpenRGB.NET.Models.Color(0, 0, 0) : color);
                            }

                        }


                        lock (AmbinityClient.Lock)
                        {
                            if (AmbinityClient.IsInitialized)
                                AmbinityClient.Client.UpdateLeds(index, deviceColors.Take(ledCount).ToArray());
                        }
                        Thread.Sleep(1000 / _frameRate);
                    }

                }

                catch (OperationCanceledException)
                {
                    Log.Error("OperationCanceledException catched. returning.");

                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception catched. OpenRGB proccess could be deleted");

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

        public void DFU() => throw new NotImplementedException();
    }
}












