﻿using adrilight.Services.SerialStream;
using adrilight.Util;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.SerialPortData;
using adrilight_shared.Settings;
using Serilog;
using System;
using System.Buffers;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace adrilight
{

    internal sealed class

        SerialStream : IDisposable, ISerialStream
    {

        public SerialStream(IDeviceSettings deviceSettings, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));

            // DeviceSpotSets = deviceSpotSets ?? throw new ArgumentNullException(nameof(deviceSpotSets));
            DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            foreach (var output in deviceSettings.AvailableLightingOutputs)
            {
                output.PropertyChanged += (_, __) =>
                {
                    switch (__.PropertyName)
                    {
                        case nameof(output.SlaveDevice):
                            LightingDevicesChanged();
                            break;
                    }
                };


            }
            DeviceSettings.DeviceState = DeviceStateEnum.Normal;
            _hasPWMCOntroller = DeviceSettings.AvailablePWMOutputs != null && DeviceSettings.AvailablePWMOutputs.Count() > 0;
            RefreshTransferState();
        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        #region PropertyChanged events
        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeviceSettings.IsTransferActive):
                case nameof(DeviceSettings.OutputPort):
                    RefreshTransferState();
                    break;
                case nameof(DeviceSettings.DeviceState):
                    DeviceStateChanged();
                    break;
            }
        }
        private void LightingDevicesChanged()
        {
            #region Get All Needed Params from current device
            _lightingDevices = new ARGBLEDSlaveDevice[DeviceSettings.AvailableLightingDevices.Length];
            for (int i = 0; i < DeviceSettings.AvailableLightingDevices.Length; i++)
            {
                _lightingDevices[i] = DeviceSettings.AvailableLightingDevices[i] as ARGBLEDSlaveDevice;
            }
            if (DeviceSettings.AvailablePWMDevices != null)
            {
                _pwmDevices = new PWMMotorSlaveDevice[DeviceSettings.AvailablePWMDevices.Length];
                for (int i = 0; i < DeviceSettings.AvailablePWMDevices.Length; i++)
                {
                    _pwmDevices[i] = DeviceSettings.AvailablePWMDevices[i] as PWMMotorSlaveDevice;
                }
            }

            #endregion
        }
        private void DeviceStateChanged()
        {

            if (DeviceSettings.DeviceState == DeviceStateEnum.Normal)
            {
                _dimMode = DimMode.Up;
                _dimFactor = 0.00;

            }
            else if (DeviceSettings.DeviceState == DeviceStateEnum.Off)
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
            }
            else if (DeviceSettings.DeviceState == DeviceStateEnum.DFU)
            {
                if (DeviceSettings.IsTransferActive)
                {
                    Stop();
                    Thread.Sleep(1000);
                }
                DFU();
            }
        }
        #endregion

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private bool _hasPWMCOntroller;
        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };
        private int frameCounter;
        private int blackFrameCounter;
        private ISerialPortWrapper serialPort = null;
        private ARGBLEDSlaveDevice[] _lightingDevices { get; set; }
        private PWMMotorSlaveDevice[] _pwmDevices { get; set; }

        public bool IsValid() => SerialPort.GetPortNames().Contains(DeviceSettings.OutputPort) || DeviceSettings.OutputPort == "Không có";
        private void RefreshTransferState()
        {
            if (DeviceSettings.IsTransferActive) // normal scenario
            {

                if (IsValid())
                {
                    //start it
                    Log.Information("starting the serial stream for device Name : " + DeviceSettings.DeviceName);
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
                Log.Information("stopping the serial stream");
                Stop();
                Thread.Sleep(500);
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
                if (_dimFactor < 0.01)
                    _dimFactor = 0;
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

        public void Start()
        {
            Log.Information("Start called for SerialStream");
            _workerThread = new Thread(DoWork) {
                Name = "Serial sending",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _cancellationTokenSource = new CancellationTokenSource();
            WinApi.TimeBeginPeriod(1);
            // The call has failed
            _workerThread.Start(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            Log.Information("Stop called for Serial Stream");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;

        public void DFU()

        {
            //Open device at 1200 baudrate
            if (DeviceSettings.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                DeviceSettings.IsTransferActive = false;
                var serialPort = (ISerialPortWrapper)new WrappedSerialPort(new SerialPort(DeviceSettings.OutputPort, 1000000));
                try
                {
                    serialPort.Open();
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Open() was called twice
                }
                Thread.Sleep(500);
                try
                {
                    serialPort.Write(new byte[3] { (byte)'f', (byte)'u', (byte)'g' }, 0, 3);
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Write() was called twice
                }


                Thread.Sleep(1000);
                serialPort.Close();
            }
            else
            {
                if (DeviceSettings.OutputPort != null)
                {

                    var _serialPort = new SerialPort(DeviceSettings.OutputPort, 1200);
                    _serialPort.DtrEnable = true;
                    _serialPort.ReadTimeout = 5000;
                    _serialPort.WriteTimeout = 1000;
                    try
                    {
                        if (!_serialPort.IsOpen)
                            _serialPort.Open();
                    }
                    catch (Exception)
                    {
                        //
                    }

                    Thread.Sleep(1000);
                    if (_serialPort.IsOpen)
                        _serialPort.Close();

                }
            }
        }

        private (byte[] Buffer, int OutputLength) GetOutputStreamWithPWM(int id)
        {
            byte[] outputStream;
            var currentLightingDevice = _lightingDevices[id]; // need to implement device number mismatch
            var currentPWMDevice = new PWMMotorSlaveDevice();
            FanMotor fan = null;
            if (id < _pwmDevices.Length)
            {
                currentPWMDevice = _pwmDevices[id];
                fan = currentPWMDevice.ControlableZones[0] as FanMotor;
            }

            int counter = _messagePreamble.Length;

            const int colorsPerLed = 3;
            const int hilocheckLength = 3;
            const int extraHeader = 3;


            int ledCount = currentLightingDevice.LEDCount;
            int bufferLength = _messagePreamble.Length + hilocheckLength + extraHeader + (ledCount * colorsPerLed);
            outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);
            Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);


            byte lo = (byte)((ledCount == 0 ? 1 : ledCount) & 0xff);
            byte hi = (byte)(((ledCount == 0 ? 1 : ledCount) >> 8) & 0xff);
            byte chk = (byte)(hi ^ lo ^ 0x55);


            outputStream[counter++] = hi;
            outputStream[counter++] = lo;
            outputStream[counter++] = chk;
            outputStream[counter++] = (byte)id;
            outputStream[counter++] = fan != null ? (byte)(fan.CurrentPWMValue * 255 / 100) : (byte)200;
            outputStream[counter++] = 0;

            double currentFramePowerRequired = 0;
            const int miliampsPerLED = 20;

            var allBlack = true;
            int aliveSpotCounter = 0;
            var rgbOrder = currentLightingDevice.RGBLEDOrder;
            DimLED();
            foreach (var zone in currentLightingDevice.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                lock (ledZone.Lock)
                {
                    if (ledZone.Spots.Count == 0)//this could be PID has removed all items add 1 dummy
                    {
                        outputStream[counter++] = 0; // blue
                        outputStream[counter++] = 0; // green
                        outputStream[counter++] = 0; // red
                    }
                    else
                    {
                        foreach (DeviceSpot spot in ledZone.Spots)
                        {
                            if (spot.IsEnabled)
                            {
                                currentFramePowerRequired += (spot.Red + spot.Green + spot.Blue) * miliampsPerLED / 255;
                            }
                        }
                        var powerLimitFactor = 1.0;
                        if (GeneralSettings.IsPowerLimitEnabled)
                            powerLimitFactor = PowerLimitationFactor(currentFramePowerRequired, 500);
                        foreach (DeviceSpot spot in ledZone.Spots)
                        {
                            if (spot.IsEnabled)
                            {
                                ApplyColorWhitebalance(spot.Red, spot.Green, spot.Blue,
                                 currentLightingDevice.WhiteBalanceRed, currentLightingDevice.WhiteBalanceGreen, currentLightingDevice.WhiteBalanceBlue,
                                 out byte FinalR, out byte FinalG, out byte FinalB);
                                var reOrderedColor = ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB);
                                //get data
                                outputStream[counter + spot.Index * 3 + 0] = (byte)(reOrderedColor[0] * powerLimitFactor * _dimFactor); // blue
                                outputStream[counter + spot.Index * 3 + 1] = (byte)(reOrderedColor[1] * powerLimitFactor * _dimFactor); // green
                                outputStream[counter + spot.Index * 3 + 2] = (byte)(reOrderedColor[2] * powerLimitFactor * _dimFactor); // red
                                aliveSpotCounter++;
                            }
                            allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                        }

                    }
                }
            }
            if (allBlack)
            {
                blackFrameCounter++;
            }
            for (int i = counter + aliveSpotCounter * 3; i < bufferLength; i++)
            {
                outputStream[i] = 0;
            }
            return (outputStream, bufferLength);
        }

        private (byte[] Buffer, int OutputLength) GetOutputStream(int id)
        {
            byte[] outputStream;
            var currentLightingDevice = _lightingDevices[id];
            int counter = _messagePreamble.Length;
            const int colorsPerLed = 3;
            const int hilocheckLenght = 3;
            const int extraHeader = 3;
            int ledCount = currentLightingDevice.LEDCount;
            int bufferLength = _messagePreamble.Length + hilocheckLenght + extraHeader + (ledCount * colorsPerLed);

            outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);
            Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);

            byte lo = (byte)((ledCount == 0 ? 1 : ledCount) & 0xff);
            byte hi = (byte)(((ledCount == 0 ? 1 : ledCount) >> 8) & 0xff);
            byte chk = (byte)(hi ^ lo ^ 0x55);


            outputStream[counter++] = hi;
            outputStream[counter++] = lo;
            outputStream[counter++] = chk;
            outputStream[counter++] = (byte)id;
            outputStream[counter++] = 0;
            outputStream[counter++] = 0;


            var allBlack = true;
            int aliveSpotCounter = 0;
            var rgbOrder = currentLightingDevice.RGBLEDOrder;

            double currentFramePowerRequired = 0;
            const int miliampsPerLED = 20;
            DimLED();
            foreach (var zone in currentLightingDevice.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                lock (ledZone.Lock)
                {
                    if (ledZone.Spots.Count == 0)//this could be PID has removed all items add 1 dummy
                    {
                        outputStream[counter++] = 0; // blue
                        outputStream[counter++] = 0; // green
                        outputStream[counter++] = 0; // red
                    }
                    else
                    {
                        foreach (DeviceSpot spot in ledZone.Spots)
                        {
                            if (spot.IsEnabled)
                            {
                                currentFramePowerRequired += (spot.Red + spot.Green + spot.Blue) * miliampsPerLED / 765;
                            }
                        }
                        var powerLimitFactor = 1.0;
                        if (GeneralSettings.IsPowerLimitEnabled)
                            powerLimitFactor = PowerLimitationFactor(currentFramePowerRequired, 500);
                        foreach (DeviceSpot spot in ledZone.Spots)
                        {
                            if (spot.IsEnabled)
                            {
                                ApplyColorWhitebalance(spot.Red, spot.Green, spot.Blue,
                                 currentLightingDevice.WhiteBalanceRed, currentLightingDevice.WhiteBalanceGreen, currentLightingDevice.WhiteBalanceBlue,
                                 out byte FinalR, out byte FinalG, out byte FinalB);
                                var reOrderedColor = ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB);
                                //get data
                                outputStream[counter + spot.Index * 3 + 0] = (byte)(reOrderedColor[0] * powerLimitFactor * _dimFactor); // blue
                                outputStream[counter + spot.Index * 3 + 1] = (byte)(reOrderedColor[1] * powerLimitFactor * _dimFactor); // green
                                outputStream[counter + spot.Index * 3 + 2] = (byte)(reOrderedColor[2] * powerLimitFactor * _dimFactor); // red
                                aliveSpotCounter++;
                            }
                            allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                        }

                    }
                }
            }
            if (allBlack)
            {
                blackFrameCounter++;
            }
            for (int i = counter + aliveSpotCounter * 3; i < bufferLength; i++)
            {
                outputStream[i] = 0;
            }
            return (outputStream, bufferLength);
        }

        private void ApplyColorWhitebalance(byte r, byte g, byte b, int whiteBalanceRed, int whiteBalanceGreen, int whiteBalanceBlue, out byte finalR, out byte finalG, out byte finalB)
        {

            finalR = (byte)(r * whiteBalanceRed / 100);
            finalG = (byte)(g * whiteBalanceGreen / 100);
            finalB = (byte)(b * whiteBalanceBlue / 100);
        }

        private double PowerLimitationFactor(double currentRequiredPower, int powerSuplyMiliamps)
        {

            if (currentRequiredPower < powerSuplyMiliamps)
                return 1.0;
            else
                return powerSuplyMiliamps / currentRequiredPower;

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
        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;

            LightingDevicesChanged();

            if (String.IsNullOrEmpty(DeviceSettings.OutputPort))
            {
                Log.Warning("Cannot start the serial sending because the comport is not selected.");
                return;
            }

            frameCounter = 0;
            blackFrameCounter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int baudRate = 1000000;
                    if (DeviceSettings.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub)
                        baudRate = 2000000;
                    string openedComPort = null;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //open or change the serial port
                        if (openedComPort != DeviceSettings.OutputPort)
                        {
                            serialPort?.Close();
                            serialPort = DeviceSettings.OutputPort != "Không có" ? (ISerialPortWrapper)new WrappedSerialPort(new SerialPort(DeviceSettings.OutputPort, baudRate)) : new FakeSerialPort();
                            serialPort.Open();
                            openedComPort = DeviceSettings.OutputPort;

                        }
                        //send frame data
                        for (int i = 0; i < _lightingDevices.Length; i++)
                        {
                            var (outputBuffer, streamLength) = _hasPWMCOntroller ? GetOutputStreamWithPWM(i) : GetOutputStream(i);
                            serialPort.Write(outputBuffer, 0, streamLength);
                            ArrayPool<byte>.Shared.Return(outputBuffer);

                            //ws2812b LEDs need 30 µs = 0.030 ms for each led to set its color so there is a lower minimum to the allowed refresh rate
                            //receiving over serial takes it time as well and the arduino does both tasks in sequence
                            //+1 ms extra safe zone
                            double fastLedTime;
                            if (DeviceSettings.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
                                fastLedTime = ((192) / 3.0 * 0.030d);
                            else
                                fastLedTime = ((streamLength - _messagePreamble.Length - 6) / 3.0 * 0.030d);
                            var serialTransferTime = outputBuffer.Length * 10 * 1000 / baudRate;
                            var minTimespan = (int)(fastLedTime + serialTransferTime) + 1;
                            Thread.Sleep(minTimespan);
                        }

                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Error("OperationCanceledException catched. returning.");
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Device is removed or malfunction: " + serialPort.SerialPort.PortName);

                    if (serialPort != null && serialPort.IsOpen)
                    {
                        try
                        {
                            serialPort.Close();
                        }
                        catch (Exception ex2)
                        {
                            ///
                            Thread.Sleep(1000);
                        }

                    }
                    serialPort?.Dispose();
                    //allow the system some time to recover
                    Thread.Sleep(1000);
                    DeviceSettings.IsTransferActive = false;
                }
                finally
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                        serialPort.Dispose();
                        Log.Information("SerialPort Disposed!");
                    }


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












