using adrilight.Services.SerialStream;
using adrilight.Util;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Settings;
using NLog;
using System;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Services.NetworkStream
{
    internal sealed class
        NetworkStream : IDisposable, ISerialStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public NetworkStream(IDeviceSettings deviceSettings, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            // DeviceSpotSets = deviceSpotSets ?? throw new ArgumentNullException(nameof(deviceSpotSets));
            DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            RefreshTransferState();

            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        public DeviceStateEnum CurrentState { get; set; }// to be implemented
        // private IDeviceSpotSet[] DeviceSpotSets { get; set; }
        private async Task<bool> Refresh() //fetches updated values from WLED device
        {

            bool available;
            available = await SendAPICall("");
            if (!available)
            {
                _log.Debug("current device network address is not available, the device could be offline");
                HandyControl.Controls.MessageBox.Show("Current device is not available", "Network Address", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            return available;


        }
        public async Task<bool> SendAPICall(string call)
        {
            var networkAddress = DeviceSettings.OutputPort;
            string url = "http://" + networkAddress;
            if (networkAddress.StartsWith("https://"))
            {
                url = networkAddress;
            }

            var response = await DeviceHttpConnection.GetInstance().Send_WLED_API_Call(url, call);
            if (response == null)
            {
                //CurrentStatus = DeviceStatus.Unreachable;
                return false;
            }

            if (response.Equals("err")) //404 or other non-success http status codes, indicates that target is not a WLED device
            {
                //CurrentStatus = DeviceStatus.Error;
                return false;
            }

            XmlApiResponse deviceResponse = XmlApiResponseParser.ParseApiResponse(response);
            if (deviceResponse == null) //could not parse XML API response
            {
                //CurrentStatus = DeviceStatus.Error;
                return false;
            }

            // CurrentStatus = DeviceStatus.Default; //the received response was valid

            //if (!NameIsCustom) Name = deviceResponse.Name;

            ////only consider brightness if light is on and if it wasn't modified in the same call (prevents brightness slider "jumps")
            //if (deviceResponse.Brightness > 0 && !call.Contains("A="))
            //{
            //    brightnessReceived = deviceResponse.Brightness;
            //    BrightnessCurrent = brightnessReceived;
            //    OnPropertyChanged("BrightnessCurrent"); //update slider binding
            //}

            //ColorCurrent = deviceResponse.LightColor;
            //OnPropertyChanged("ColorCurrent");

            //StateCurrent = deviceResponse.IsOn;
            return true;
        }

        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeviceSettings.IsTransferActive):
                case nameof(DeviceSettings.OutputPort):
                    RefreshTransferState();
                    break;
            }
        }

        public bool IsValid() => SerialPort.GetPortNames().Contains(DeviceSettings.OutputPort) || DeviceSettings.OutputPort == "Không có";
        // public bool IsAcess() => !BlockedComport.Contains(UserSettings.ComPort);
        // public IList<string> BlockedComport = new List<string>();

        private async void RefreshTransferState()
        {

            if (DeviceSettings.IsTransferActive)
            {
                var available = await Refresh();
                if (available)
                {

                    //start it
                    _log.Debug("starting the network stream for device Name : " + DeviceSettings.DeviceName);
                    Start();
                }
                else
                {
                    DeviceSettings.IsTransferActive = false;

                }
            }

            else if (!DeviceSettings.IsTransferActive && IsRunning)
            {
                //stop it
                _log.Debug("stopping the network stream");
                Stop();
            }
        }

        private readonly byte[] _messageWARLSPreamble = { 1, 5 };// first byte is 1 means this network stream using WARLS protocol



        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;


        private int frameCounter;
        private int blackFrameCounter;
        private int _iD;
        //public int ID {
        //    get { return DeviceSettings.DeviceID; }
        //    set
        //    {
        //        _iD = value;
        //    }
        //}


        public void Start()
        {
            _log.Debug("Start called.");
            if (_workerThread != null) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(DoWork) {
                Name = "UDP sending",
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
            //not apply for wireless device

        }





        /*  private (byte[] Buffer, int OutputLength) GetOutputStream( int output)
          {
              byte[] outputStream;
              var currentOutput = DeviceSettings.AvailableOutputs[output];

              int counter = _messageWARLSPreamble.Length;

              lock (currentOutput.OutputLEDSetup.Lock)
              {
                  const int colorsPerLed = 4;
                  int bufferLength = _messageWARLSPreamble.Length 
                      + (currentOutput.OutputLEDSetup.Spots.Count * colorsPerLed);


                  outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);

                  Buffer.BlockCopy(_messageWARLSPreamble, 0, outputStream, 0, _messageWARLSPreamble.Length);

                  var isEnabled = currentOutput.OutputIsEnabled;
                  var allBlack = true;
                  //}


                      foreach (DeviceSpot spot in currentOutput.OutputLEDSetup.Spots)
                      {
                      if (isEnabled)
                      {
                          var RGBOrder = currentOutput.OutputRGBLEDOrder;
                          switch (RGBOrder)
                          {
                              case "RGB": //RGB
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Red; // blue
                                  outputStream[counter++] = spot.Green; // green
                                  outputStream[counter++] = spot.Blue; // red
                                  break;
                              case "GRB": //GRB
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Green; // blue
                                  outputStream[counter++] = spot.Red; // green
                                  outputStream[counter++] = spot.Blue; // red
                                  break;
                              case "BRG": //BRG
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Blue; // blue
                                  outputStream[counter++] = spot.Red; // green
                                  outputStream[counter++] = spot.Green; // red
                                  break;
                              case "BGR": //BGR
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Blue; // blue
                                  outputStream[counter++] = spot.Green; // green
                                  outputStream[counter++] = spot.Red; // red
                                  break;
                              case "GBR"://GBR
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Green; // blue
                                  outputStream[counter++] = spot.Blue; // green
                                  outputStream[counter++] = spot.Red; // red
                                  break;
                              case "RBG": //GBR
                                  outputStream[counter++] = (byte)spot.Index; //LED index
                                  outputStream[counter++] = spot.Red; // blue
                                  outputStream[counter++] = spot.Blue; // green
                                  outputStream[counter++] = spot.Green; // red
                                  break;



                          }


                          allBlack = allBlack && spot.Red == 0 && spot.Green == 0 && spot.Blue == 0;

                      }
                      else
                      {
                          outputStream[counter++] = (byte)spot.Index; //LED index
                          outputStream[counter++] = 0; // blue
                          outputStream[counter++] = 0; // green
                          outputStream[counter++] = 0; // red
                      }


                      }





                  if (allBlack)
                  {
                      blackFrameCounter++;
                  }

              return (outputStream, bufferLength);
              }





          }
      */

        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;



            if (string.IsNullOrEmpty(DeviceSettings.OutputPort))
            {
                _log.Warn("Cannot start the UDP Broadcasting because the address is not valid.");
                return;
            }

            frameCounter = 0;
            blackFrameCounter = 0;

            //retry after exceptions...
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    const int baudRate = 1000000;
                    string openedComPort = null;

                    var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    IPAddress serverAddr = IPAddress.Parse(DeviceSettings.OutputPort);

                    var endPoint = new IPEndPoint(serverAddr, 21324);





                    while (!cancellationToken.IsCancellationRequested)
                    {

                        /*
                        //send frame data
                        for(int i = 0; i < DeviceSettings.AvailableOutputs.Length; i++)
                        {
                            var (outputBuffer, streamLength) = GetOutputStream(i);
                            sock.SendTo(outputBuffer, endPoint);
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
                            var fastLedTime = ((streamLength - _messageWARLSPreamble.Length) / 3.0 * 0.030d);
                            var serialTransferTime = outputBuffer.Length * 10 * 1000 / baudRate;
                            var minTimespan = (int)(fastLedTime + serialTransferTime) + 1;

                            Thread.Sleep(10);
                        }
                        








                        */



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

                    Thread.Sleep(500);
                    Stop();

                }
                finally
                {

                    _log.Debug("SerialPort Disposed!");


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












