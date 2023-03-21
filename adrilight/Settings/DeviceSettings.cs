using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.ViewModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows;
using System.Buffers;
using HandyControl.Themes;
using System.Threading;
using Newtonsoft.Json;

namespace adrilight
{
    internal class DeviceSettings : ViewModelBase, IDeviceSettings
    {
        private int _deviceID;
        private string _deviceName;
        private string _deviceSerial;
        private string _deviceType;
        private string _manufacturer;
        private string _deviceDescription;
        private string _firmwareVersion;
        private string _hardwareVersion = "unknown";
        private IDeviceController[] _availableControllers;
        private string _productionDate;
        private bool _isVisible;
        private bool _isEnabled;
        private string _outputPort;
        private bool _isTransferActive;
        private bool _isDummy = false;
        private bool _isLoading = false;

        private int _selectedOutput = 0;
        private string _geometry = "generaldevice";
        private string _deviceConnectionGeometry = "connection";
        private int _baudrate = 1000000;
        private int _activatedProfileIndex = 0;
        private string _deviceUID;
        private string _deviceConnectionType = "";
        private bool _isSelected = false;
        private string _deviceThumbnail;
        private bool _isLoadingProfile = false;
        private string _activatedProfileUID;
        private string _fwLocation;
        private State _currentState = State.normal;
        private string _requiredFwVersion;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] requestSpeedCommand = { (byte)'1', (byte)'5', (byte)'4' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private bool _isSizeNeedUserDefine = false;
        private bool _isLoadingSpeed = false;

        private int _currentActiveControllerIndex;
        private IDeviceController _currentActiveController;
        public string DeviceThumbnail { get => _deviceThumbnail; set { Set(() => DeviceThumbnail, ref _deviceThumbnail, value); } }

  
        public State CurrentState { get => _currentState; set { Set(() => CurrentState, ref _currentState, value); } }
        public string RequiredFwVersion { get => _requiredFwVersion; set { Set(() => RequiredFwVersion, ref _requiredFwVersion, value); } }
        public int DeviceID { get => _deviceID; set { Set(() => DeviceID, ref _deviceID, value); } }
        public string DeviceName { get => _deviceName; set { Set(() => DeviceName, ref _deviceName, value); } }
        public string FwLocation { get => _fwLocation; set { Set(() => FwLocation, ref _fwLocation, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => DeviceSerial, ref _deviceSerial, value); } }
        public string DeviceType { get => _deviceType; set { Set(() => DeviceType, ref _deviceType, value); } }
        public string Manufacturer { get => _manufacturer; set { Set(() => Manufacturer, ref _manufacturer, value); } }
        public string FirmwareVersion { get => _firmwareVersion; set { Set(() => FirmwareVersion, ref _firmwareVersion, value); } }
        public string HardwareVersion { get => _hardwareVersion; set { Set(() => HardwareVersion, ref _hardwareVersion, value); } }
        public string ProductionDate { get => _productionDate; set { Set(() => ProductionDate, ref _productionDate, value); } }
        public string ActivatedProfileUID { get => _activatedProfileUID; set { Set(() => ActivatedProfileUID, ref _activatedProfileUID, value); } }
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); DeviceEnableChanged(); } }
        public bool IsLoading { get => _isLoading; set { Set(() => IsLoading, ref _isLoading, value); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string OutputPort { get => _outputPort; set { Set(() => OutputPort, ref _outputPort, value); } }
        public bool IsTransferActive { get => _isTransferActive; set { Set(() => IsTransferActive, ref _isTransferActive, value); } }
        public bool IsDummy { get => _isDummy; set { Set(() => IsDummy, ref _isDummy, value); } }


        public int Baudrate { get => _baudrate; set { Set(() => Baudrate, ref _baudrate, value); } }
        public int ActivatedProfileIndex { get => _activatedProfileIndex; set { Set(() => ActivatedProfileIndex, ref _activatedProfileIndex, value); } }

        public int SelectedOutput { get => _selectedOutput; set { Set(() => SelectedOutput, ref _selectedOutput, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public string DeviceDescription { get => _deviceDescription; set { Set(() => DeviceDescription, ref _deviceDescription, value); } }
        public string DeviceUID { get => _deviceUID; set { Set(() => DeviceUID, ref _deviceUID, value); } }

        public bool IsLoadingSpeed { get => _isLoadingSpeed; set { Set(() => IsLoadingSpeed, ref _isLoadingSpeed, value); } }

        public string DeviceConnectionGeometry { get => _deviceConnectionGeometry; set { Set(() => DeviceConnectionGeometry, ref _deviceConnectionGeometry, value); } }
        public string DeviceConnectionType { get => _deviceConnectionType; set { Set(() => DeviceConnectionType, ref _deviceConnectionType, value); } }
        public bool IsLoadingProfile { get => _isLoadingProfile; set { Set(() => IsLoadingProfile, ref _isLoadingProfile, value); } }
        public bool IsSizeNeedUserDefine { get => _isSizeNeedUserDefine; set { Set(() => IsSizeNeedUserDefine, ref _isSizeNeedUserDefine, value); } }

        public IDeviceController[] AvailableControllers { get => _availableControllers; set { Set(() => AvailableControllers, ref _availableControllers, value); } }

        [JsonIgnore]
        public IDeviceController CurrentActiveController { get => AvailableControllers[CurrentActiveControlerIndex]; set { Set(() => CurrentActiveController, ref _currentActiveController, value); } }

        [JsonIgnore]
        public Rect CurrentLivewItemsBound => GetDeviceRectBound(CurrentLiveViewZones);


        public IControlZone[] CurrentLiveViewZones => GetControlZones(CurrentActiveController);
        [JsonIgnore]
        public ISlaveDevice[] AvailableLightingDevices => GetSlaveDevices(ControllerTypeEnum.LightingController);
        private ISlaveDevice[] GetSlaveDevices(ControllerTypeEnum type)
        {
            var slaveDevices = new List<ISlaveDevice>();
            foreach (var controller in AvailableControllers.Where(x => x.Type == type))
            {
                foreach(var output in controller.Outputs)
                {
                    slaveDevices.Add(output.SlaveDevice);
                }
            }
            return slaveDevices.ToArray();
        }
        public IControlZone[] GetControlZones(IDeviceController controller)
        {
            List<IControlZone> zones = new List<IControlZone>();
            foreach (var output in controller.Outputs)
            {
                foreach (var zone in output.SlaveDevice.ControlableZones)
                {
                    zones.Add(zone);
                }
            }
            return zones.ToArray();
        }
        public int CurrentActiveControlerIndex { get => _currentActiveControllerIndex; set { if (value >= 0) Set(() => CurrentActiveControlerIndex, ref _currentActiveControllerIndex, value); OnActiveControllerChanged(); } }

        private void OnActiveControllerChanged()
        {
            if (CurrentActiveControlerIndex >= 0)
            {
                CurrentActiveController = AvailableControllers[CurrentActiveControlerIndex];
                //reset selected liveview zone because the collection changed
                RaisePropertyChanged(nameof(CurrentLiveViewZones));
                RaisePropertyChanged(nameof(CurrentActiveController));
            }
        }
        private void DeviceEnableChanged()
        {


            //if (AvailableOutputs != null)
            //{

            //    if (AvailableOutputs.Length == 1)
            //    {
            //        AvailableOutputs[0].OutputIsEnabled = IsEnabled;
            //    }
            //    else
            //    {
            //        foreach (var output in AvailableOutputs)
            //        {
            //            output.OutputParrentIsEnable = IsEnabled;

            //        }
            //    }
            //}


        }
        public void SetOutput(IOutputSettings output, int outputID)
        {
            //AvailableOutputs[outputID].OutputIsLoadingProfile = true;

            //foreach (PropertyInfo property in AvailableOutputs[outputID].GetType().GetProperties())
            //{

            //    if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
            //        property.SetValue(AvailableOutputs[outputID], property.GetValue(output, null), null);
            //    AvailableOutputs[outputID].LEDPerLED = output.LEDPerLED;
            //    AvailableOutputs[outputID].LEDPerSpot = output.LEDPerSpot;
            //}

            //AvailableOutputs[outputID].OutputIsLoadingProfile = false;

        }
        private DrawableHelpers DrawableHlprs;
       
        public Rect GetDeviceRectBound(IControlZone[] zones)
        {
            
            
            if(DrawableHlprs==null)
                DrawableHlprs = new DrawableHelpers();


            return DrawableHlprs.GetBound(zones);
           
        }
        public void ActivateProfile(IDeviceProfile profile)
        {
            //ActivatedProfileUID = profile.ProfileUID;
            //for (var i = 0; i < AvailableOutputs.Length; i++)
            //{
            //    AvailableOutputs[i].OutputIsLoadingProfile = true;

            //    foreach (PropertyInfo property in AvailableOutputs[i].GetType().GetProperties())
            //    {

            //        if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
            //            property.SetValue(AvailableOutputs[i], property.GetValue(profile.OutputSettings[i], null), null);
            //    }

            //    AvailableOutputs[i].OutputIsLoadingProfile = false;
            //}



        }




        public void BrightnessUp(int value)
        {

            //foreach (var output  in AvailableOutputs)//possible replace with method from IOutputSettings
            //  {
            //  var currentBrightness = (output as OutputSettings).GetBrightness();
            //  var nextBrightness = currentBrightness + value;
            //  if(nextBrightness<100)
            //      (output as OutputSettings).SetBrightness(nextBrightness);
            //  else
            //  {
            //      (output as OutputSettings).SetBrightness(100);
            //  }
            //  }

        }
        public void SpeedUp(int value)
        {

            //if (DeviceSpeed < 255)
            //    DeviceSpeed += value;
            //if (DeviceSpeed > 255)
            //    DeviceSpeed = 255;

        }
        public void SpeedDown(int value)
        {

            //if (DeviceSpeed >20)
            //    DeviceSpeed -= value;
            //if (DeviceSpeed < 20)
            //    DeviceSpeed = 20;

        }
        public void BrightnessDown(int value)
        {


            //foreach (var output in AvailableOutputs)//possible replace with method from IOutputSettings
            //{
            //    var currentBrightness = (output as OutputSettings).GetBrightness();
            //    var nextBrightness = currentBrightness - value;
            //    if (nextBrightness > 0)
            //        (output as OutputSettings).SetBrightness(nextBrightness);
            //    else
            //    {
            //        (output as OutputSettings).SetBrightness(0);
            //    }
            //}

        }


        //public void DeviceLocator(Color color) // this function send color signal to device to locate new device added
        //{
        //    if (OutputPort != null && DeviceConnectionType == "wired")
        //    {
        //        var _serialPort = new SerialPort(OutputPort, 1000000);
        //        _serialPort.DtrEnable = true;
        //        _serialPort.ReadTimeout = 5000;
        //        _serialPort.WriteTimeout = 1000;
        //        try
        //        {
        //            _serialPort.Open();
        //        }
        //        catch (UnauthorizedAccessException)
        //        {
        //            return;
        //        }
        //        //just get data of first output only and send signal of 64 LED
        //        var (outputBuffer, streamLength) = GetLocatorOutputStream(color);
        //        //write 60 frame of data to ensure device received it
        //        for (int i = 0; i < 10; i++)
        //        {
        //            _serialPort.Write(outputBuffer, 0, streamLength);
        //            Thread.Sleep(1);
        //        }
        //        try
        //        {
        //            _serialPort.Close();
        //        }
        //        catch (Exception)
        //        {
        //            return;
        //        }


        //    }

        //}
        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };
        //private (byte[] Buffer, int OutputLength) GetLocatorOutputStream(Color color)
        //{
        //    byte[] outputStream;
        //    int counter = _messagePreamble.Length;
        //    int locatorNumLED = 64;

        //    const int colorsPerLed = 3;
        //    int bufferLength = _messagePreamble.Length + 3 + 3
        //        + (locatorNumLED * colorsPerLed);


        //    outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);

        //    Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);







        //    byte lo = (byte)((locatorNumLED) & 0xff);
        //    byte hi = (byte)(((locatorNumLED) >> 8) & 0xff);
        //    byte chk = (byte)(hi ^ lo ^ 0x55);
        //    outputStream[counter++] = hi;
        //    outputStream[counter++] = lo;
        //    outputStream[counter++] = chk;
        //    outputStream[counter++] = 0;
        //    outputStream[counter++] = 0;
        //    outputStream[counter++] = 0;


        //    for (int i = 0; i < locatorNumLED; i++)
        //    {

        //        var RGBOrder = "RGB";
        //        var reOrderedColor = ReOrderSpotColor(RGBOrder, color.R, color.G, color.B);

        //        outputStream[counter++] = reOrderedColor[0]; // blue
        //        outputStream[counter++] = reOrderedColor[1]; // green
        //        outputStream[counter++] = reOrderedColor[2]; // red


        //    }


        //    return (outputStream, bufferLength);



        //}
        //private byte[] ReOrderSpotColor(string order, byte r, byte g, byte b)
        //{
        //    byte[] reOrderedColor = new byte[3];
        //    switch (order)
        //    {
        //        case "RGB"://do nothing
        //            reOrderedColor[0] = r;
        //            reOrderedColor[1] = g;
        //            reOrderedColor[2] = b;
        //            break;
        //        case "RBG"://do nothing
        //            reOrderedColor[0] = r;
        //            reOrderedColor[1] = b;
        //            reOrderedColor[2] = g;
        //            break;
        //        case "BGR"://do nothing
        //            reOrderedColor[0] = b;
        //            reOrderedColor[1] = g;
        //            reOrderedColor[2] = r;
        //            break;
        //        case "BRG"://do nothing
        //            reOrderedColor[0] = b;
        //            reOrderedColor[1] = r;
        //            reOrderedColor[2] = g;
        //            break;
        //        case "GRB"://do nothing
        //            reOrderedColor[0] = g;
        //            reOrderedColor[1] = r;
        //            reOrderedColor[2] = b;
        //            break;
        //        case "GBR"://do nothing
        //            reOrderedColor[0] = g;
        //            reOrderedColor[1] = b;
        //            reOrderedColor[2] = r;
        //            break;
        //    }
        //    return reOrderedColor;
        //}
        public void UpdateChildSize()
        {
            foreach (var controller in AvailableControllers)
            {
                foreach(var output in controller.Outputs)
                {
                    output.SlaveDevice.UpdateSizeByChild();
                }
            }
        }
        public void RefreshFirmwareVersion()
        {

            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];

            bool isValid = false;


            IsTransferActive = false; // stop current serial stream attached to this device

            var _serialPort = new SerialPort(OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            //write request info command
            _serialPort.Write(requestCommand, 0, 3);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            IDeviceSettings newDevice = new DeviceSettings();
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + "Không có thông tin về Firmware, vui lòng liên hệ Ambino trước khi cập nhật firmware thủ công", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        isValid = false;
                        break;
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                id = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(id, 0, count);
                    offset += readCount;
                    count -= readCount;
                }


                DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
                RaisePropertyChanged(nameof(DeviceSerial));
            }
            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                name = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(name, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
                RaisePropertyChanged(nameof(DeviceName));


            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                fw = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(fw, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
                RaisePropertyChanged(nameof(FirmwareVersion));
            }
            _serialPort.Close();
            _serialPort.Dispose();
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            IsTransferActive = true;
            RaisePropertyChanged(nameof(IsTransferActive));
        }
        public async void RefreshDeviceActualSpeedAsync()
        {
            await Task.Run(() => GetActualSpeed());
            IsLoadingSpeed = false;
            RaisePropertyChanged(nameof(IsLoadingSpeed));
            HandyControl.Controls.MessageBox.Show("Speed information sent to device!", "Speed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void GetActualSpeed()
        {

            //byte[] speed = new byte[256];

            //bool isValid = false;


            //IsTransferActive = false; // stop current serial stream attached to this device

            //var _serialPort = new SerialPort(OutputPort, 1000000);
            //_serialPort.DtrEnable = true;
            //_serialPort.ReadTimeout = 5000;
            //_serialPort.WriteTimeout = 1000;
            //try
            //{
            //    _serialPort.Open();
            //}
            //catch (UnauthorizedAccessException)
            //{
            //    DeviceActualSpeed = "unknown";
            //    RaisePropertyChanged(nameof(DeviceActualSpeed));
            //    return;
            //}

            ////write request info command
            //_serialPort.Write(requestSpeedCommand, 0, 3);
            //int retryCount = 0;
            //int offset = 0;
            //int spdInfoLength = 0; // Expected response length of valid deviceID 


            //while (offset < 3)
            //{


            //    try
            //    {
            //        byte header = (byte)_serialPort.ReadByte();
            //        if (header == expectedValidHeader[offset])
            //        {
            //            offset++;
            //        }
            //    }
            //    catch (TimeoutException)// retry until received valid header
            //    {
            //        _serialPort.Write(requestSpeedCommand, 0, 3);
            //        retryCount++;
            //        if (retryCount == 3)
            //        {
            //            _serialPort.Close();
            //            _serialPort.Dispose();
            //            IsTransferActive = true;
            //            RaisePropertyChanged(nameof(IsTransferActive));
            //            DeviceActualSpeed = "unknown";
            //            RaisePropertyChanged(nameof(DeviceActualSpeed));
            //            return;

            //        }
            //        Debug.WriteLine("no respond, retrying...");
            //    }


            //}
            //if (offset == 3) //3 bytes header are valid
            //{
            //    spdInfoLength = (byte)_serialPort.ReadByte();
            //    int count = spdInfoLength;
            //    speed = new byte[count];
            //    while (count > 0)
            //    {
            //        var readCount = _serialPort.Read(speed, 0, count);
            //        offset += readCount;
            //        count -= readCount;
            //    }

            //}

            //_serialPort.Close();
            //_serialPort.Dispose();
            ////if (isValid)
            ////    newDevices.Add(newDevice);
            ////reboot serialStream
            //IsTransferActive = true;
            //RaisePropertyChanged(nameof(IsTransferActive));
            //DeviceActualSpeed = speed[0].ToString();
            //RaisePropertyChanged(nameof(DeviceActualSpeed));
        }
       
    }
}
