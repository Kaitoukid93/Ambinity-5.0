using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using Newtonsoft.Json;
using NLog;
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


        private ILogger _log = LogManager.GetCurrentClassLogger();

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

            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }



        // private IDeviceSpotSet[] DeviceSpotSets { get; set; }
        private bool CheckSerialPort(string serialport)
        {

            var available = true;
            int TestbaudRate = 1000000;

            if (serialport != null)
            {
                if (serialport == "Không có")
                {
                    // System.Windows.MessageBox.Show("Serial Port " + serialport + " is just for testing effects, not the real device, please note");
                    available = true;
                    return available;

                }
                var serialPorttest = (ISerialPortWrapper)new WrappedSerialPort(new SerialPort(serialport, TestbaudRate));

                //Open the serial port

                try
                {
                    serialPorttest.Open();

                }

                catch (Exception)
                {

                    // BlockedComport.Add(serialport);
                    _log.Debug("Serial Port " + serialport + " access denied, added to Blacklist");
                    // HandyControl.Controls.MessageBox.Show("Serial Port " + serialport + " is in use or unavailable, Please chose another COM Port", "Serial Port", MessageBoxButton.OK, MessageBoxImage.Error);
                    available = false;

                    //_log.Debug(ex, "Exception catched.");
                    //to be safe, we reset the serial port
                    //  MessageBox.Show("Serial Port " + UserSettings.ComPort + " is in use or unavailable, Please chose another COM Port");




                    //allow the system some time to recover

                    // Dispose();
                }
                serialPorttest.Close();

            }

            else
            {
                available = false;
            }

            return available;


        }
        #region PropertyChanged events
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
            if (DeviceSettings.IsTransferActive && DeviceSettings.DeviceState == DeviceStateEnum.Normal) // normal scenario
            {

                if (IsValid())
                {
                    //start it
                    _log.Debug("starting the serial stream for device Name : " + DeviceSettings.DeviceName);
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
                Thread.Sleep(500);
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
                DFU();
            }

        }


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
            var allBlack = true;
            int aliveSpotCounter = 0;
            var rgbOrder = currentLightingDevice.RGBLEDOrder;
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
                        switch (DeviceSettings.DeviceState)
                        {
                            case DeviceStateEnum.Normal: // get data from ledsetup
                                foreach (DeviceSpot spot in ledZone.Spots)
                                {
                                    if (spot.IsEnabled)
                                    {
                                        ApplyColorWhitebalance(spot.Red, spot.Green, spot.Blue,
                                         currentLightingDevice.WhiteBalanceRed, currentLightingDevice.WhiteBalanceGreen, currentLightingDevice.WhiteBalanceBlue,
                                         out byte FinalR, out byte FinalG, out byte FinalB);
                                        var reOrderedColor = ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB);
                                        //get data
                                        outputStream[counter + spot.Index * 3 + 0] = reOrderedColor[0]; // blue
                                        outputStream[counter + spot.Index * 3 + 1] = reOrderedColor[1]; // green
                                        outputStream[counter + spot.Index * 3 + 2] = reOrderedColor[2]; // red
                                        aliveSpotCounter++;
                                    }



                                    allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                                }
                                //fill the rest of outputStream zero

                                break;
                            case DeviceStateEnum.Sleep: // send black frame data
                                foreach (DeviceSpot spot in ledZone.Spots)
                                {

                                    //switch (currentOutput.SleepMode)
                                    //{
                                    //    case 0:
                                    //        if (isEnabled && parrentIsEnabled)
                                    //        {

                                    //            for (int i = 0; i < ledPerSpot; i++)
                                    //            {
                                    //                outputStream[counter++] = 0; // blue
                                    //                outputStream[counter++] = 0; // green
                                    //                outputStream[counter++] = 0; // red
                                    //            }
                                    //        }
                                    //        break;
                                    //    case 1:
                                    //        if (isEnabled && parrentIsEnabled)
                                    //        {
                                    //            var RGBOrder = currentOutput.OutputRGBLEDOrder;
                                    //            var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.SentryRed, spot.SentryGreen, spot.SentryBlue);
                                    //            for (int i = 0; i < ledPerSpot; i++)
                                    //            {

                                    //                outputStream[counter++] = reOrderedColor[0]; // blue
                                    //                outputStream[counter++] = reOrderedColor[1]; // green
                                    //                outputStream[counter++] = reOrderedColor[2]; // red
                                    //            }
                                    //        }
                                    //        break;
                                    //}

                                }

                                break;

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
                        switch (DeviceSettings.DeviceState)
                        {
                            case DeviceStateEnum.Normal: // get data from ledsetup
                                foreach (DeviceSpot spot in ledZone.Spots)
                                {
                                    if (spot.IsEnabled)
                                    {
                                        ApplyColorWhitebalance(spot.Red, spot.Green, spot.Blue,
                                         currentLightingDevice.WhiteBalanceRed, currentLightingDevice.WhiteBalanceGreen, currentLightingDevice.WhiteBalanceBlue,
                                         out byte FinalR, out byte FinalG, out byte FinalB);
                                        var reOrderedColor = ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB);
                                        //get data
                                        outputStream[counter + spot.Index * 3 + 0] = reOrderedColor[0]; // blue
                                        outputStream[counter + spot.Index * 3 + 1] = reOrderedColor[1]; // green
                                        outputStream[counter + spot.Index * 3 + 2] = reOrderedColor[2]; // red
                                        aliveSpotCounter++;
                                    }
                                    allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                                }
                                break;
                            case DeviceStateEnum.Sleep: // send black frame data
                                foreach (DeviceSpot spot in ledZone.Spots)
                                {

                                    //switch (currentOutput.SleepMode)
                                    //{
                                    //    case 0:
                                    //        if (isEnabled && parrentIsEnabled)
                                    //        {

                                    //            for (int i = 0; i < ledPerSpot; i++)
                                    //            {
                                    //                outputStream[counter++] = 0; // blue
                                    //                outputStream[counter++] = 0; // green
                                    //                outputStream[counter++] = 0; // red
                                    //            }
                                    //        }
                                    //        break;
                                    //    case 1:
                                    //        if (isEnabled && parrentIsEnabled)
                                    //        {
                                    //            var RGBOrder = currentOutput.OutputRGBLEDOrder;
                                    //            var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.SentryRed, spot.SentryGreen, spot.SentryBlue);
                                    //            for (int i = 0; i < ledPerSpot; i++)
                                    //            {

                                    //                outputStream[counter++] = reOrderedColor[0]; // blue
                                    //                outputStream[counter++] = reOrderedColor[1]; // green
                                    //                outputStream[counter++] = reOrderedColor[2]; // red
                                    //            }
                                    //        }
                                    //        break;
                                    //}

                                }

                                break;

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
                _log.Warn("Cannot start the serial sending because the comport is not selected.");
                return;
            }

            frameCounter = 0;
            blackFrameCounter = 0;

            //retry after exceptions...
            bool isDisconnectedMessage = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    const int baudRate = 1000000;
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
                        //after this we know the device is successfully open the first time or reconnected, we reset the  message show flag
                        isDisconnectedMessage = false;
                        //send frame data
                        for (int i = 0; i < _lightingDevices.Length; i++)
                        {
                            var (outputBuffer, streamLength) = _hasPWMCOntroller ? GetOutputStreamWithPWM(i) : GetOutputStream(i);
                            serialPort.Write(outputBuffer, 0, streamLength);
                            if (++frameCounter == 1024 && blackFrameCounter > 1000)
                            {
                                //there is maybe something wrong here because most frames where black. report it once per run only
                                var settingsJson = JsonConvert.SerializeObject(DeviceSettings, Formatting.None);
                                _log.Info($"Sent {frameCounter} frames already. {blackFrameCounter} were completely black. Settings= {settingsJson}");
                            }
                            ArrayPool<byte>.Shared.Return(outputBuffer);

                            //ws2812b LEDs need 30 µs = 0.030 ms for each led to set its color so there is a lower minimum to the allowed refresh rate
                            //receiving over serial takes it time as well and the arduino does both tasks in sequence
                            //+1 ms extra safe zone
                            double fastLedTime;
                            if (DeviceSettings.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
                                fastLedTime = ((192) / 3.0 * 0.030d);
                            else
                                fastLedTime = ((streamLength - _messagePreamble.Length) / 3.0 * 0.030d);
                            var serialTransferTime = outputBuffer.Length * 10 * 1000 / baudRate;
                            var minTimespan = (int)(fastLedTime + serialTransferTime) + 1;
                            Thread.Sleep(minTimespan);
                        }

                    }
                }
                catch (OperationCanceledException)
                {
                    _log.Debug("OperationCanceledException catched. returning.");

                    return;
                }
                catch (Exception ex)
                {



                    _log.Debug(ex, "Exception catched.");
                    //to be safe, we reset the serial port
                    //if (!isDisconnectedMessage)
                    //{
                    //    var result = HandyControl.Controls.MessageBox.Show("USB của " + DeviceSettings.DeviceName + " Đã ngắt kết nối!!!. Kiểm tra lại kết nối", "Mất kết nối", MessageBoxButton.OK, MessageBoxImage.Warning);

                    //    if (result == MessageBoxResult.OK)//stop showing message
                    //    {

                    //        isDisconnectedMessage = true;
                    //    }
                    //}



                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    serialPort?.Dispose();
                    //allow the system some time to recover
                    Thread.Sleep(500);
                }
                finally
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                        serialPort.Dispose();
                        _log.Debug("SerialPort Disposed!");
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












