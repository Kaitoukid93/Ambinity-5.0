//using adrilight.Helpers;
//using adrilight.Settings;
//using adrilight.Spots;
using adrilight_shared.Models.Device;
using Microsoft.Win32;

//using adrilight_effect_analyzer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Color = System.Windows.Media.Color;
using System.Buffers;


//using static adrilight.ViewModel.MainViewViewModel;
using Task = System.Threading.Tasks.Task;
using ExcelDataReader.Log;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Threading;

namespace adrilight_content_creator.ViewModel
{
    internal class DeviceUtilViewModel : BaseViewModel
    {

        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");


        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string GifsCollectionFolderPath => Path.Combine(JsonPath, "Gifs");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string ProfileCollectionFolderPath => Path.Combine(JsonPath, "Profiles");
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r', (byte)'\n' };
        private static byte[] sendCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };
        public DeviceUtilViewModel()
        {
            SetupCommand();
            AvailableTestColor = new ObservableCollection<System.Windows.Media.Color>()
            {
                Color.FromRgb(0, 255, 0),
                Color.FromRgb(255, 0, 0),
                Color.FromRgb(0, 0, 255),
                Color.FromRgb(0, 0, 0),
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(255, 255, 0),
                Color.FromRgb(0, 255, 255),
                Color.FromRgb(255, 0, 255),
            };
            MaxLED = 80;
        }


        #region Public Properties
        private ObservableCollection<System.Windows.Media.Color> _availableTestColor;
        public ObservableCollection<System.Windows.Media.Color> AvailableTestColor
        {
            get { return _availableTestColor; }
            set
            {
                _availableTestColor = value;
                RaisePropertyChanged();
            }
        }
        private int _maxOutputPorts;
        public int MaxOutputPorts
        {
            get { return _maxOutputPorts; }
            set
            {
                _maxOutputPorts = value;
                RaisePropertyChanged();
            }
        }
        private int _selectedOutput;
        public int SelectedOutput
        {
            get { return _selectedOutput; }
            set
            {
                _selectedOutput = value;
                RaisePropertyChanged();
            }
        }
        private int _maxLED;
        public int MaxLED
        {
            get { return _maxLED; }
            set
            {
                _maxLED = value;
                RaisePropertyChanged();
            }
        }
        private System.Windows.Media.Color _selectedTestColor;
        public System.Windows.Media.Color SelectedTestColor
        {
            get { return _selectedTestColor; }
            set
            {
                _selectedTestColor = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<DeviceEEPROMDataModel> _currentDeviceEEPROMData;
        public ObservableCollection<DeviceEEPROMDataModel> CurrentDeviceEEPROMData
        {
            get { return _currentDeviceEEPROMData; }
            set
            {
                _currentDeviceEEPROMData = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ComPortObject> _availableComPorts;
        public ObservableCollection<ComPortObject> AvailableComPorts
        {
            get { return _availableComPorts; }
            set
            {
                _availableComPorts = value;
                RaisePropertyChanged();
            }
        }
        private bool _isLoadingData;
        public bool IsLoadingData
        {
            get { return _isLoadingData; }
            set
            {
                _isLoadingData = value;
                RaisePropertyChanged();
            }
        }
        private bool _isMakingHUB;
        public bool IsMakingHUB
        {
            get { return _isMakingHUB; }
            set
            {
                _isMakingHUB = value;
                RaisePropertyChanged();
            }
        }
        private string _makingHUBLog;
        public string MakingHUBLog
        {
            get { return _makingHUBLog; }
            set
            {
                _makingHUBLog = value;
                RaisePropertyChanged();
            }
        }
        private bool _isLoadingEEPROM;
        public bool IsLoadingEEPROM
        {
            get { return _isLoadingEEPROM; }
            set
            {
                _isLoadingEEPROM = value;
                RaisePropertyChanged();
            }
        }
        private ComPortObject _selectedComPort;
        public ComPortObject SelectedComPort
        {
            get { return _selectedComPort; }
            set
            {
                _selectedComPort = value;
                CurrentDevice = null;
                // AvailableTestColor = null;
                RaisePropertyChanged();
            }
        }
        private IDeviceSettings _currentDevice;
        public IDeviceSettings CurrentDevice
        {
            get { return _currentDevice; }
            set
            {
                _currentDevice = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        public ICommand TestSelectedOutputCommand { get; set; }
        public ICommand GetSelectedDeviceDataCommand { get; set; }
        public ICommand RefreshAvailableComPortsCommand { get; set; }
        public ICommand GetSelectedDeviceEEPROMDataCommand { get; set; }
        public ICommand SetSelectedDeviceEEPROMDataCommand { get; set; }
        public ICommand MakeSelectedComPortasHUBCommand { get; set; }
        private void SetupCommand()
        {
            RefreshAvailableComPortsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                RefreshAvailableComPorts();
                CurrentDevice = null;

            }
          );
            MakeSelectedComPortasHUBCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsMakingHUB = true;
                await Task.Run(()=> MakeSelectedComPortasHUB());
                IsMakingHUB = false;

            }
       );
            GetSelectedDeviceDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingData = true;
                await Task.Run(() => GetSelectedDeviceData(SelectedComPort));
                IsLoadingData = false;
            }
         );
            GetSelectedDeviceEEPROMDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingEEPROM = true;
                var result = await Task.Run(() => GetHardwareSettings());
                if (!result)
                {

                }
                IsLoadingEEPROM = false;


            }

    );
            SetSelectedDeviceEEPROMDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingEEPROM = true;
                var result = await Task.Run(() => SendHardwareSettings());
                if (!result)
                {

                }
                IsLoadingEEPROM = false;


            }

);

            TestSelectedOutputCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                TestSelectedOutput();

            }

);
        }

        public class ComPortObject
        {
            public ComPortObject(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
            public string Port { get; set; }
            public bool IsConnected { get; set; }
            public bool IsChecked { get; set; }

        }
        public class DeviceEEPROMDataModel
        {
            public DeviceEEPROMDataModel(int address, int value)
            {
                Address = address;
                Value = value;
            }
            public int Address { get; set; }
            public int Value { get; set; }

        }


        #region Private Methods
        private async Task GetSelectedDeviceData(ComPortObject selectedComPort)
        {
            if (selectedComPort == null)
            {
                return;
            }

            CurrentDevice = new DeviceSettings()
            {
                OutputPort = selectedComPort.Port
            };
            CurrentDevice.DeviceName = "waiting...";
            CurrentDevice.DeviceSerial = "waiting...";
            CurrentDevice.FirmwareVersion = "waiting...";
            CurrentDevice.HardwareVersion = "waiting...";
            await GetDeviceFactoryData(CurrentDevice);

        }
        private async Task<bool> MakeSelectedComPortasHUB()
        {
            /// [15,12,93,PortID,Number of outputs,0,0,0,0,0,last 6 bytes ID set] ///////
            /// 
            MakingHUBLog = "init...";
            var selectedComPort = AvailableComPorts?.Where(c => c.IsChecked).ToList();
            if (selectedComPort == null || selectedComPort.Count < 2)
                return false;
            var master = selectedComPort.First();
            var (id, name, fw, hw) = await GetFactoryData(master);
            MakingHUBLog = "master serial is :" + BitConverter.ToString(id).Replace('-', ' ');
            int portAddress = 0;
            try
            {
                foreach (var port in selectedComPort)
                {
                    var outputStream = new byte[17];
                    Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
                    int counter = sendCommand.Length;
                    outputStream[counter++] = (byte)(portAddress);
                    if (portAddress == 3)
                    {
                        outputStream[counter++] = 2;
                    }
                    else
                    {
                        outputStream[counter++] = 1;
                    }
                    outputStream[counter++] = 0;
                    outputStream[counter++] = 0;
                    outputStream[counter++] = 0;
                    outputStream[counter++] = 0;
                    outputStream[counter++] = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        outputStream[counter++] = id[i];
                    }
                    outputStream[counter++] = (byte)'\n';
                    var _serialPort = new SerialPort(port.Port, 1000000);
                    _serialPort.DtrEnable = true;
                    _serialPort.ReadTimeout = 5000;
                    _serialPort.WriteTimeout = 1000;
                    try
                    {
                        _serialPort.Open();
                        Thread.Sleep(500);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return await Task.FromResult(false);
                    }
                    _serialPort.Write(outputStream, 0, outputStream.Length);
                    //Thread.Sleep(1000);
                    portAddress++;
                    MakingHUBLog = "Sending port " + portAddress + "data...";
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    _serialPort.Dispose();
                    
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            MakingHUBLog = "done!";
            return true;
            // get ID of first comport on the list
            //assign port order 
            //we need simple method and reliable
        }
        private void RefreshAvailableComPorts()
        {
            AvailableComPorts = new ObservableCollection<ComPortObject>();
            List<string> CH55X = GetComPortByID("1209", "c550");
            List<string> CH340 = GetComPortByID("1A86", "7522");
            var devices = new List<string>();
            var subDevices = new List<string>();
            List<string> sd = GetComPortByID("1209", "c55c");
            if (CH55X.Count > 0 || CH340.Count > 0 || sd.Count > 0)
            {
                foreach (var port in CH55X)
                {
                    var p = new ComPortObject(port + " - CH55x");
                    p.Port = port;
                    AvailableComPorts.Add(p);
                }
                foreach (var port in CH340)
                {
                    var p = new ComPortObject(port + " - Ch34x");
                    p.Port = port;
                    AvailableComPorts.Add(p);
                }
                foreach (var port in sd)
                {
                    var p = new ComPortObject(port + " - HyperPort");
                    p.Port = port;
                    AvailableComPorts.Add(p);
                }
            }
            else
            {
                Debug.WriteLine("No Compatible Device Detected");
            }

        }
        static List<string> GetComPortByID(String VID, String PID)
        {
            String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            List<string> comports = new List<string>();
            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (String s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (String s in rk3.GetSubKeyNames())
                {
                    if (_rx.Match(s).Success)
                    {
                        RegistryKey rk4 = rk3.OpenSubKey(s);
                        foreach (String s2 in rk4.GetSubKeyNames())
                        {
                            RegistryKey rk5 = rk4.OpenSubKey(s2);
                            RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                            string portName = (string)rk6.GetValue("PortName");
                            if (!String.IsNullOrEmpty(portName) && SerialPort.GetPortNames().Contains(portName))
                            {
                                comports.Add((string)rk6.GetValue("PortName"));
                            }
                        }
                    }
                }

            }
            return comports;

        }
        private Task<(byte[], byte[], byte[], byte[])> GetFactoryData(ComPortObject comPort)
        {
            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            byte[] hw = new byte[256];
            var _serialPort = new SerialPort(comPort.Port, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
                Thread.Sleep(500);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            //write request info command

            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            int hwLength = 0;
            // IDeviceSettings newDevice = new DeviceSettings();
            while (offset < 3)
            {


                try
                {
                    _serialPort.Write(requestCommand, 0, 4);
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    //_serialPort.Write(requestCommand, 0, 3);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + "Không có thông tin về Firmware, vui lòng liên hệ Ambino trước khi cập nhật firmware thủ công", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                // RaisePropertyChanged(nameof(DeviceName));


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
            }
            if (offset == 3 + idLength + nameLength + fwLength) //3 bytes header are valid
            {
                try
                {
                    hwLength = (byte)_serialPort.ReadByte();
                    int count = hwLength;
                    hw = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(hw, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                }
                catch (TimeoutException)
                {
                    // Log.Information(newDevice.DeviceName, "Unknown Firmware Version");
                    // newDevice.HardwareVersion = "unknown";
                }

            }
            _serialPort.DiscardInBuffer();
            _serialPort.Close();
            _serialPort.Dispose();
            return Task.FromResult((id, name, fw, hw));
        }
        public async Task GetDeviceFactoryData(IDeviceSettings device)
        {

            var (id, name, fw, hw) = await GetFactoryData(SelectedComPort);
            device.DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
            device.HardwareVersion = Encoding.ASCII.GetString(hw, 0, hw.Length);
            device.FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
            device.DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
            switch (device.DeviceName)
            {
                case "Ambino Basic":
                    MaxLED = 80;
                    MaxOutputPorts = 1;
                    break;
                case "Ambino EDGE":
                    MaxLED = 80;
                    MaxOutputPorts = 1;
                    break;
                case "Ambino HubV3":
                    MaxLED = 80;
                    MaxOutputPorts = 4;
                    break;
                case "Ambino FanHub":
                    MaxLED = 80;
                    MaxOutputPorts = 10;
                    break;
                case "Ambino HyperPort":
                    MaxLED = 160;
                    MaxOutputPorts = 2;
                    break;
                default:
                    MaxLED = 80;
                    MaxOutputPorts = 1;
                    break;
            }

        }
        private bool IsFirmwareValid(IDeviceSettings device)
        {
            if (device.DeviceName == "Ambino Basic" ||
                device.DeviceName == "Ambino EDGE" ||
                device.DeviceName == "Ambino FanHub" ||
                device.DeviceName == "Ambino HubV3" || device.DeviceName == "Ambino HyperPort")
            {
                string fwversion = device.FirmwareVersion;
                if (fwversion == "unknown" || fwversion == string.Empty || fwversion == null)
                    fwversion = "1.0.0";
                var deviceFWVersion = new Version(fwversion);
                var requiredVersion = new Version();
                if (device.DeviceName == "Ambino Basic")
                {
                    requiredVersion = new Version("1.0.8");
                }
                else if (device.DeviceName == "Ambino EDGE")
                {
                    requiredVersion = new Version("1.0.5");
                }
                else if (device.DeviceName == "Ambino FanHub")
                {
                    requiredVersion = new Version("1.0.8");
                }
                else if (device.DeviceName == "Ambino HyperPort")
                {
                    requiredVersion = new Version("1.0.0");
                }
                if (deviceFWVersion >= requiredVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            { return false; }
        }
        private void TestSelectedOutput()
        {

            var _serialPort = new SerialPort(SelectedComPort.Port, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
                Thread.Sleep(500);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            var outputstream = GetColorOutputStream();
            _serialPort.Write(outputstream, 0, outputstream.Length);
            _serialPort.Close();
            _serialPort.Dispose();
            SelectedTestColor = AvailableTestColor[AvailableTestColor.IndexOf(SelectedTestColor) + 1 == AvailableTestColor.Count ? 0 : AvailableTestColor.IndexOf(SelectedTestColor) + 1];
        }
        private byte[] GetEEPRomDataOutputStream()
        {
            var outputStream = new byte[16];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            outputStream[3] = 255;
            return outputStream;
        }
        private byte[] GetColorOutputStream()
        {
            byte[] outputStream;

            int counter = _messagePreamble.Length;
            const int colorsPerLed = 3;
            const int hilocheckLenght = 3;
            const int extraHeader = 3;
            int bufferLength = _messagePreamble.Length + hilocheckLenght + extraHeader + (MaxLED * colorsPerLed);

            outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);
            Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);

            byte lo = (byte)((MaxLED == 0 ? 1 : MaxLED) & 0xff);
            byte hi = (byte)(((MaxLED == 0 ? 1 : MaxLED) >> 8) & 0xff);
            byte chk = (byte)(hi ^ lo ^ 0x55);


            outputStream[counter++] = hi;
            outputStream[counter++] = lo;
            outputStream[counter++] = chk;
            outputStream[counter++] = (byte)(SelectedOutput > 0 ? SelectedOutput - 1 : 0);
            outputStream[counter++] = 0;
            outputStream[counter++] = 0;
            for (int i = 0; i < MaxLED; i++)
            {
                outputStream[counter++] = SelectedTestColor.G; // blue
                outputStream[counter++] = SelectedTestColor.R; // green
                outputStream[counter++] = SelectedTestColor.B; // red
            }
            return outputStream;
        }
        private byte[] GetSettingOutputStream()
        {
            var outputStream = new byte[16];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            int counter = sendCommand.Length;
            for (int i = 0; i < CurrentDeviceEEPROMData.Count; i++)
            {
                outputStream[counter++] = (byte)CurrentDeviceEEPROMData[i].Value;
            }
            return outputStream;
        }
        public async Task<bool> SendHardwareSettings()
        {

            if (CurrentDevice == null)
            {
                return false;
            }
            if (CurrentDevice.FirmwareVersion == null || CurrentDevice.FirmwareVersion == string.Empty)
                return false;
            if (!IsFirmwareValid(CurrentDevice))
            {
                return false;
            }
            CurrentDevice.IsTransferActive = false; // stop current serial stream attached to this device
            if (!SerialPort.GetPortNames().Contains(CurrentDevice.OutputPort))
                return false;
            var _serialPort = new SerialPort(CurrentDevice.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
                Thread.Sleep(500);
            }
            catch (UnauthorizedAccessException)
            {
                return await Task.FromResult(false);
            }

            var outputStream = GetSettingOutputStream();
            _serialPort.Write(outputStream, 0, outputStream.Length);
            _serialPort.WriteLine("\r\n");
            int retryCount = 0;
            int offset = 0;
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
                    _serialPort.Write(outputStream, 0, outputStream.Length);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return await Task.FromResult(false);
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid continue to read next 13 byte of data
            {

                CurrentDeviceEEPROMData = new ObservableCollection<DeviceEEPROMDataModel>();
                try
                {
                    //led on off
                    int address = 0;
                    while (_serialPort.BytesToRead > 0)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {

                            CurrentDeviceEEPROMData.Add(new DeviceEEPROMDataModel(address++, _serialPort.ReadByte()));
                        });
                    }
                }
                catch (TimeoutException ex)
                {
                    //discard buffer
                    _serialPort.DiscardInBuffer();
                    _serialPort.Close();
                    _serialPort.Dispose();
                    return await Task.FromResult(true);
                }

                //discard buffer

            }

            _serialPort.DiscardInBuffer();
            _serialPort.Close();
            _serialPort.Dispose();
            return await Task.FromResult(true);
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            //IsTransferActive = true;
            //RaisePropertyChanged(nameof(IsTransferActive));
        }
        public async Task<bool> GetHardwareSettings()
        {

            ///////////////////// Hardware settings data table, will be wirte to device EEPRom /////////
            /// [h,s,d,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,0,0,0,0,0,0,0] ///////
            /// 
            if (CurrentDevice == null)
            {
                return false;
            }
            if (CurrentDevice.FirmwareVersion == null || CurrentDevice.FirmwareVersion == string.Empty)
                return false;
            if (!IsFirmwareValid(CurrentDevice))
            {
                return false;
            }
            CurrentDevice.IsTransferActive = false; // stop current serial stream attached to this device
            if (!SerialPort.GetPortNames().Contains(CurrentDevice.OutputPort))
                return false;
            var _serialPort = new SerialPort(CurrentDevice.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
                Thread.Sleep(500);
            }
            catch (UnauthorizedAccessException)
            {
                return await Task.FromResult(false);
            }
            _serialPort.DiscardInBuffer();
            var outputStream = GetEEPRomDataOutputStream();
            _serialPort.Write(outputStream, 0, outputStream.Length);
            _serialPort.WriteLine("\r\n");
            int retryCount = 0;
            int offset = 0;
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
                    _serialPort.Write(outputStream, 0, outputStream.Length);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        // Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return await Task.FromResult(false);
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid continue to read next 13 byte of data
            {

                /// [15,12,93,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,No Signal Fan Speed,0,0,0,0,0,0] ///////
                CurrentDeviceEEPROMData = new ObservableCollection<DeviceEEPROMDataModel>();
                try
                {
                    //led on off
                    int address = 0;
                    while (_serialPort.BytesToRead > 0)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {

                            CurrentDeviceEEPROMData.Add(new DeviceEEPROMDataModel(address++, _serialPort.ReadByte()));
                        });
                    }
                }
                catch (TimeoutException ex)
                {
                    //discard buffer
                    _serialPort.DiscardInBuffer();
                    _serialPort.Close();
                    _serialPort.Dispose();
                    return await Task.FromResult(true);
                }


            }
            //discard buffer
            _serialPort.DiscardInBuffer();
            _serialPort.Close();
            _serialPort.Dispose();
            return await Task.FromResult(true);
        }
        #endregion
    }
}
