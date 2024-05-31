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
using adrilight_shared.ViewModel;

namespace adrilight_content_creator.ViewModel
{
    public class DeviceUtilViewModel : BaseViewModel
    {

        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };
        public DeviceUtilViewModel(DeviceHardwareSettings deviceHardware)
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
            _deviceHardwareSettings = deviceHardware;
        }
        private DeviceHardwareSettings _deviceHardwareSettings;

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
                await Task.Run(() => _deviceHardwareSettings.GetHardwareSettings(true,CurrentDevice));
                IsLoadingEEPROM = false;


            }

    );
            SetSelectedDeviceEEPROMDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingEEPROM = true;
                var vm = new ProgressDialogViewModel("Applying settings", "123", "usbIcon");
                vm.ProgressBarVisibility = Visibility.Visible;
                vm.CurrentProgressHeader = "Sending to device";
                vm.ProgressBarVisibility = Visibility.Visible;
                await Task.Run(() => _deviceHardwareSettings.SendHardwareSettings(vm,CurrentDevice));
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
        public async Task GetDeviceFactoryData(IDeviceSettings device)
        {
            string deviceName = null;
            string deviceID = null;
            string deviceFirmware = null;
            string deviceHardware = null;
            int deviceHWL = 0;
            var result = await Task.Run(() => _deviceHardwareSettings.RefreshDeviceInfo(device.OutputPort,
                out deviceName,
                out deviceID,
                out deviceFirmware,
                out deviceHardware,
                out deviceHWL));
            if (!result)
                return;

            device.DeviceName = deviceName;
            device.HardwareVersion = deviceHardware;
            device.FirmwareVersion = deviceFirmware;
            device.DeviceSerial = deviceID;
            device.HWL_version = deviceHWL;
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
        #endregion
    }
}
