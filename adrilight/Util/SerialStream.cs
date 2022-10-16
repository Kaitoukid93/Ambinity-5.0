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
            deviceSettings.CurrentState = State.normal;
            RefreshTransferState();

            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }



        // private IDeviceSpotSet[] DeviceSpotSets { get; set; }
        private bool CheckSerialPort(string serialport)
        {
            Stop();//stop current serial stream first to avoid access denied
                   // BlockedComport.Clear();
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
                    HandyControl.Controls.MessageBox.Show("Serial Port " + serialport + " is in use or unavailable, Please chose another COM Port", "Serial Port", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public bool IsValid() => SerialPort.GetPortNames().Contains(DeviceSettings.OutputPort) || DeviceSettings.OutputPort == "Không có";
        // public bool IsAcess() => !BlockedComport.Contains(UserSettings.ComPort);
        // public IList<string> BlockedComport = new List<string>();

        private void RefreshTransferState()
        {

            if (DeviceSettings.IsTransferActive && DeviceSettings.CurrentState == State.normal) // normal scenario
            {
                if (IsRunning)
                {
                    Stop(); // stop current running if worker thread is alive.
                }
                if (IsValid() && CheckSerialPort(DeviceSettings.OutputPort))
                {

                    //start it
                    _log.Debug("starting the serial stream for device Name : " + DeviceSettings.DeviceName);
                    Start();
                }
                else
                {
                    DeviceSettings.IsTransferActive = false;
                    DeviceSettings.OutputPort = null;
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
                DFU();
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

        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };




        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;


        private int frameCounter;
        private int blackFrameCounter;



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
        public void SetSpeed()
        {
            if (DeviceSettings.OutputPort != null)
            {
                var serialPort = (ISerialPortWrapper)new WrappedSerialPort(new SerialPort(DeviceSettings.OutputPort, 1000000));
                try
                {
                    serialPort.Open();
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Open() was called twice
                }
                try
                {
                    serialPort.Write(new byte[19] { (byte)'s', (byte)'p', (byte)'d',
                        0,//Hi
                        0,//Lo
                        0,//Chk
                        (byte)DeviceSettings.SpeedMode,//HD1
                        0,//HD2
                        0,//HD3
                       (byte)DeviceSettings.DeviceSpeed,//Fan1 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan2 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan3 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan4 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan5 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan6 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan7 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan8 speed
                       (byte)DeviceSettings.DeviceSpeed,//Fan9 speed
                       (byte)DeviceSettings.DeviceSpeed//Fan10 speed

                    }, 0, 19);
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Write() was called twice
                }
                serialPort.Close();

            }
        }
        public void DFU()

        {
            //Open device at 1200 baudrate


            if (DeviceSettings.DeviceType == "ABHUBV2")
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





        private (byte[] Buffer, int OutputLength) GetOutputStream(IOutputSettings output, byte id)
        {
            byte[] outputStream;
            var currentOutput = output;
            var ledPerSpot = currentOutput.LEDPerSpot;
            int counter = _messagePreamble.Length;
            lock (currentOutput.OutputLEDSetup.Lock)
            {
                const int colorsPerLed = 3;
                int bufferLength = _messagePreamble.Length + 3 + 3
                    + (currentOutput.OutputLEDSetup.Spots.Length * colorsPerLed * ledPerSpot);


                outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);

                Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);







                byte lo = (byte)((currentOutput.OutputLEDSetup.Spots.Length * ledPerSpot) & 0xff);
                byte hi = (byte)(((currentOutput.OutputLEDSetup.Spots.Length * ledPerSpot) >> 8) & 0xff);
                byte chk = (byte)(hi ^ lo ^ 0x55);
                outputStream[counter++] = hi;
                outputStream[counter++] = lo;
                outputStream[counter++] = chk;
                outputStream[counter++] = id;
                outputStream[counter++] = (byte)DeviceSettings.DeviceSpeed;
                outputStream[counter++] = 0;
                var isEnabled = currentOutput.OutputIsEnabled;
                var parrentIsEnabled = DeviceSettings.IsEnabled;
                var allBlack = true;
                //}
                switch (DeviceSettings.CurrentState)
                {
                    case State.normal: // get data from ledsetup
                        if (!DeviceSettings.IsEnabled || !output.OutputIsEnabled)
                        {
                            if (DeviceSettings.IsUnionMode)
                                output.OutputLEDSetup.DimLED(0.99f);
                            else
                                output.OutputLEDSetup.DimLED(0.9f);

                        }


                        foreach (DeviceSpot spot in currentOutput.OutputLEDSetup.Spots)
                        {

                            var RGBOrder = currentOutput.OutputRGBLEDOrder;
                            var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.Red, spot.Green, spot.Blue);
                            for (int i = 0; i < ledPerSpot; i++)
                            {

                                outputStream[counter++] = reOrderedColor[0]; // blue
                                outputStream[counter++] = reOrderedColor[1]; // green
                                outputStream[counter++] = reOrderedColor[2]; // red
                            }

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

                                        for (int i = 0; i < ledPerSpot; i++)
                                        {
                                            outputStream[counter++] = 0; // blue
                                            outputStream[counter++] = 0; // green
                                            outputStream[counter++] = 0; // red
                                        }
                                    }
                                    break;
                                case 1:
                                    if (isEnabled && parrentIsEnabled)
                                    {
                                        var RGBOrder = currentOutput.OutputRGBLEDOrder;
                                        var reOrderedColor = ReOrderSpotColor(RGBOrder, spot.SentryRed, spot.SentryGreen, spot.SentryBlue);
                                        for (int i = 0; i < ledPerSpot; i++)
                                        {

                                            outputStream[counter++] = reOrderedColor[0]; // blue
                                            outputStream[counter++] = reOrderedColor[1]; // green
                                            outputStream[counter++] = reOrderedColor[2]; // red
                                        }
                                    }
                                    break;
                            }

                        }
                        break;

                }







                if (allBlack)
                {
                    blackFrameCounter++;
                }

                return (outputStream, bufferLength);
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
        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;
            ISerialPortWrapper serialPort = null;


            if (String.IsNullOrEmpty(DeviceSettings.OutputPort))
            {
                _log.Warn("Cannot start the serial sending because the comport is not selected.");
                return;
            }

            frameCounter = 0;
            blackFrameCounter = 0;
            bool isUnion = DeviceSettings.IsUnionMode;

            //retry after exceptions...
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
                            // serialPort.DisableDtr();
                            // serialPort.DisableRts();
                            serialPort.Open();
                            openedComPort = DeviceSettings.OutputPort;

                        }

                        //send frame data
                        if (isUnion)
                        {

                            for (int i = 0; i < DeviceSettings.AvailableOutputs.Length; i++)
                            {
                                var (outputBuffer, streamLength) = GetOutputStream(DeviceSettings.UnionOutput, (byte)i);
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
                                if (DeviceSettings.DeviceType == "ABHUBV2")
                                    fastLedTime = ((192) / 3.0 * 0.030d);
                                else
                                    fastLedTime = ((streamLength - _messagePreamble.Length) / 3.0 * 0.030d);
                                var serialTransferTime = outputBuffer.Length * 10 * 1000 / baudRate;
                                var fanSpeedSettingTime = 0;
                                if (DeviceSettings.DeviceType == "ABFANHUB")
                                    if (i == 2) // this is the output that share the same dataline with Tx to little core on fanhub
                                        fanSpeedSettingTime = 2; //so we need to increase sleep time to prevent package skipping

                                var minTimespan = (int)(fastLedTime + serialTransferTime) + 1 + fanSpeedSettingTime;

                                Thread.Sleep(minTimespan);
                            }
                        }

                        else
                        {
                            foreach (var output in DeviceSettings.AvailableOutputs)
                            {

                                var (outputBuffer, streamLength) = GetOutputStream(output, (byte)output.OutputID);
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
                                if (DeviceSettings.DeviceType == "ABHUBV2")
                                    fastLedTime = ((192) / 3.0 * 0.030d);
                                else
                                    fastLedTime = ((streamLength - _messagePreamble.Length) / 3.0 * 0.030d);
                                var serialTransferTime = outputBuffer.Length * 10 * 1000 / baudRate;
                                var fanSpeedSettingTime = 0;
                                if (DeviceSettings.DeviceType == "ABFANHUB")
                                    if (output.OutputID == 2) // this is the output that share the same dataline with Tx to little core on fanhub
                                        fanSpeedSettingTime = 2; //so we need to increase sleep time to prevent package skipping
                                var minTimespan = (int)(fastLedTime + serialTransferTime) + 1 + fanSpeedSettingTime;

                                Thread.Sleep(minTimespan);
                            }
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
                    var result = HandyControl.Controls.MessageBox.Show("USB của " + DeviceSettings.DeviceName + " Đã ngắt kết nối!!!. Kiểm tra lại kết nối sau đó nhấn [Confirm]", "Mất kết nối", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.OK)//restart app
                    {
                        System.Windows.Forms.Application.Restart();
                        Process.GetCurrentProcess().Kill();
                    }


                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    serialPort?.Dispose();

                    //allow the system some time to recover
                    Thread.Sleep(500);
                    Stop();
                    // Dispose();
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












