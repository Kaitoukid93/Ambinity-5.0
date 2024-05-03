using adrilight.Helpers;
using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.SerialPortData;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using CSCore.CoreAudioAPI;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Application = System.Windows.Application;

namespace adrilight.ViewModel
{
    public class DeviceAdvanceSettingsViewModel : ViewModelBase
    {
        #region Construct
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");
        private string JsonFWToolsFWListFileNameAndPath => Path.Combine(JsonFWToolsFileNameAndPath, "adrilight-fwlist.json");
        private string BackupFolder => Path.Combine(JsonPath, "Backup");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        public DeviceAdvanceSettingsViewModel(DialogService service, IDeviceSettings device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                UpdateButtonContent = adrilight_shared.Properties.Resources.EnterDFU_ButtonContent;
                UpdateInstructionContent = adrilight_shared.Properties.Resources.HUBV_Checkforupdate_content;
            }
            Device.PropertyChanged += Device_PropertyChanged;
            _dialogService = service ?? throw new ArgumentNullException(nameof(service));
            ResourceHlprs = new ResourceHelpers();
            LocalFileHlprs = new LocalFileHelpers();
            DeviceHlprs = new DeviceHelpers();
            CommandSetup();
            HardwareLightingControlInit();
        }
        #endregion

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Device.HWL_effectMode):
                    HardwareLightingControlInit();
                    break;
            }
        }

        #region Properties
        public IDeviceSettings Device { get; set; }
        private DialogService _dialogService;
        private ResourceHelpers ResourceHlprs;
        private LocalFileHelpers LocalFileHlprs;
        private DeviceHelpers DeviceHlprs { get; set; }
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r', (byte)'\n' };
        private static byte[] sendCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private bool _hwl_HasIntensityControl = false;
        private bool _hwl_HasSpeedControl = false;
        public bool HWL_HasSpeedControl {
            get
            {
                return _hwl_HasSpeedControl;
            }
            set
            {
                _hwl_HasSpeedControl = value;
                RaisePropertyChanged();
            }
        }
        public bool HWL_HasIntensityControl {
            get
            {
                return _hwl_HasIntensityControl;
            }
            set
            {
                _hwl_HasIntensityControl = value;
                RaisePropertyChanged();
            }
        }
        private bool isApplyingDeviceHardwareSettings;
        public bool IsApplyingDeviceHardwareSettings {
            get
            {
                return isApplyingDeviceHardwareSettings;
            }
            set
            {
                isApplyingDeviceHardwareSettings = value;
                RaisePropertyChanged();
            }
        }

        private ListSelectionParameter _hardwareLightingColorSelection;

        public ListSelectionParameter HardwareLightingColorSelection {
            get { return _hardwareLightingColorSelection; }

            set
            {
                _hardwareLightingColorSelection = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Methods
        private void CommandSetup()
        {
            ShowDeviceBackupFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var backupPath = Path.Combine(BackupFolder, "Device");
                if (Directory.Exists(backupPath))
                    Process.Start("explorer.exe", backupPath);

            });
            BackupDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await BackupDevice();

            });
            RestoreDeviceCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, async (p) =>
                {
                    await RestoreDeviceFromFile();

                });
            SelecFirmwareForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                string openFileDialogDescription = "hex Files (.HEX)|*.hex";
                switch (Device.DeviceFirmwareExtension)
                {
                    case ".hex":
                        openFileDialogDescription = "hex Files (.HEX)|*.hex";
                        break;
                    case ".uf2":
                        openFileDialogDescription = "uf2 Files (.UF2)|*.uf2";
                        break;
                }
                var file = LocalFileHlprs.OpenImportFileDialog(Device.DeviceFirmwareExtension, openFileDialogDescription);
                if (file != null)
                {
                    CurrentSelectedFirmware = file;
                }

            });
            SelectBackupFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                var file = LocalFileHlprs.OpenImportFileDialog(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_ChoseBackupFile_header, "zip Files (.ZIP)|*.zip");
                if (file != null)
                {
                    CurrentSelectedDeviceBackupFile = file;
                }

            });
            ApplyDeviceHardwareSettingsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await ApplyDeviceHardwareSettings();
            });
            UpdateCurrentSelectedDeviceFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                switch (Device.DeviceType.Type)
                {
                    case DeviceTypeEnum.AmbinoHUBV2:
                        EnterDFU();
                        break;
                    default:
                        if (UpdateAvailable)
                        {
                            await UpdateNow();
                        }
                        else
                        {
                            await CheckForUpdate();
                        }

                        break;
                }

            });
            UpdateCustomFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await UpdateCustomFirmware();

            });
            WindowClosing = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //Player.WindowsStatusChanged(false);

            });
            WindowOpen = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //Player.WindowsStatusChanged(true);
            });
        }
        private Visibility _hardwareSettingsEnable;
        public Visibility HardwareSettingsEnable {
            get
            {
                return _hardwareSettingsEnable;
            }

            set
            {
                _hardwareSettingsEnable = value;
                RaisePropertyChanged();
            }
        }
        private void ColorSelectionInit(List<string> source)
        {
            HardwareLightingColorSelection = new ListSelectionParameter(ModeParameterEnum.Color);
            HardwareLightingColorSelection.DataSourceLocaFolderNames = source;
            HardwareLightingColorSelection.Name = adrilight_shared.Properties.Resources.LightingEngine_ColorControl_header;
            HardwareLightingColorSelection.Description = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_ColorSelectionInit_SelectColors;
            HardwareLightingColorSelection.LoadAvailableValues();
            UpdateColorSelectionSelectedValue();
            HardwareLightingColorSelection.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(HardwareLightingColorSelection.SelectedValue):
                        //update offline palette here
                        if (HardwareLightingColorSelection.SelectedValue is ColorPalette)
                        {
                            //resize color palette o 8 colors
                            var selectedpalette = HardwareLightingColorSelection.SelectedValue as ColorPalette;
                            if (selectedpalette != null)
                            {
                                var usablePalette = ResizePalette(selectedpalette.Colors, selectedpalette.Colors.Length, 8);
                                Device.HWL_palette = usablePalette;
                            }

                        }
                        else if (HardwareLightingColorSelection.SelectedValue is ColorCard)
                        {
                            var selectedColor = HardwareLightingColorSelection.SelectedValue as ColorCard;
                            if (selectedColor != null)
                            {
                                var usableColor = selectedColor.StartColor;
                                Device.HWL_singleColor = usableColor;
                            }
                        }
                        break;
                }
            };
        }
        private void HardwareLightingControlInit()
        {
            //if has color control
            List<string> source = new List<string>();
            if (Device.HWL_effectMode == 0 || Device.HWL_effectMode == 2)
                source = new List<string>() { "Colors" };
            else
            {
                source = new List<string>() { "ColorPalettes" };
            }
            ColorSelectionInit(source);
            if (Device.HWL_effectMode == 1 || Device.HWL_effectMode == 2)
                HWL_HasSpeedControl = true;
            else
                HWL_HasSpeedControl = false;
            if (Device.HWL_effectMode == 1)
                HWL_HasIntensityControl = true;
            else
                HWL_HasIntensityControl = false;

        }
        private Color[] ResizePalette(Color[] color, int w1, int w2)
        {
            Color[] temp = new Color[8];
            int x_ratio = (int)((w1 << 16) / w2) + 1;
            int y_ratio = 1;
            int x2, y2;
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < w2; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * w2) + j] = color[(y2 * w1) + x2];
                }
            }
            return temp;
        }
        public async Task RefreshDeviceHardwareInfo()
        {
            var rslt = await Task.Run(() => GetHardwareSettings());
            if (!rslt)
            {
                HardwareSettingsEnable = Visibility.Collapsed;
            }
            else
            {
                HardwareSettingsEnable = Visibility.Visible;
            }

        }
        #endregion


        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand BackupDeviceCommand { get; set; }
        public ICommand RestoreDeviceCommand { get; set; }
        public ICommand WindowOpen { get; private set; }
        public ICommand ShowDeviceBackupFolderCommand { get; set; }
        public ICommand SelecFirmwareForCurrentDeviceCommand { get; set; }
        public ICommand SelectBackupFileCommand { get; set; }
        public ICommand UpdateCurrentSelectedDeviceFirmwareCommand { get; set; }
        public ICommand ApplyDeviceHardwareSettingsCommand { get; set; }
        public ICommand UpdateCustomFirmwareCommand { get; set; }
        #endregion


        #region Hardware Related Properties and Method
        private byte[] GetSettingOutputStream()
        {
            var outputStream = new byte[48];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            int counter = sendCommand.Length;
            outputStream[counter++] = (byte)(Device.HWL_enable == true ? 1 : 0);
            outputStream[counter++] = (byte)(Device.StatusLEDEnable == true ? 1 : 0);
            outputStream[counter++] = (byte)Device.HWL_returnafter;
            outputStream[counter++] = (byte)Device.HWL_effectMode;
            outputStream[counter++] = (byte)Device.HWL_effectSpeed;
            outputStream[counter++] = (byte)Device.HWL_brightness;
            outputStream[counter++] = (byte)Device.HWL_singleColor.R;
            outputStream[counter++] = (byte)Device.HWL_singleColor.G;
            outputStream[counter++] = (byte)Device.HWL_singleColor.B;
            for (int i = 0; i < 8; i++)
            {
                outputStream[counter++] = (byte)Device.HWL_palette[i].R;
                outputStream[counter++] = (byte)Device.HWL_palette[i].G;
                outputStream[counter++] = (byte)Device.HWL_palette[i].B;
            }
            outputStream[counter++] = (byte)Device.HWL_effectIntensity;
            outputStream[counter++] = (byte)Device.NoSignalFanSpeed;
            outputStream[counter++] = (byte)Device.HWL_MaxLEDPerOutput;
            return outputStream;
        }
        private byte[] GetEEPRomDataOutputStream()
        {
            var outputStream = new byte[48];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            outputStream[3] = 255;
            return outputStream;
        }
        private bool IsFirmwareValid()
        {
            if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoBasic ||
                Device.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE ||
                Device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub ||
                Device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3)
            {
                string fwversion = Device.FirmwareVersion;
                if (fwversion == "unknown" || fwversion == string.Empty || fwversion == null)
                    fwversion = "1.0.0";
                var deviceFWVersion = new Version(fwversion);
                var requiredVersion = new Version();
                if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoBasic)
                {
                    requiredVersion = new Version("1.0.8");
                }
                else if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE)
                {
                    requiredVersion = new Version("1.0.5");
                }
                else if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub)
                {
                    requiredVersion = new Version("1.0.8");
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
        public async Task<bool> GetHardwareSettings()
        {
            Thread.Sleep(500);
            var result = RefreshFirmwareVersion();
            if (!result)
            {
                return false;
            }
            if (!IsFirmwareValid())
            {
                return false;
            }
            if (Device.HWL_version < 1)
            {
                //request firmware update and hide device settings
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_GetHardwareSettings_OldFirmware, adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_GetHardwareSettings_ProtocolOutdated, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            Device.IsTransferActive = false; // stop current serial stream attached to this device
            if (!SerialPort.GetPortNames().Contains(Device.OutputPort))
                return false;
            var _serialPort = new SerialPort(Device.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return await Task.FromResult(false);
            }
            catch (System.IO.IOException ex)
            {
                return await Task.FromResult(false);
            }

            var outputStream = GetEEPRomDataOutputStream();
            _serialPort.Write(outputStream, 0, outputStream.Length);
            _serialPort.WriteLine("\r\n");
            int retryCount = 0;
            int offset = 0;
            //searching for header
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
                        Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return await Task.FromResult(false);
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            //3 bytes header are valid continue to read next 45 byte of data
            if (offset == 3)
            {
                try
                {
                    ReadDeviceEEPROM(_serialPort);
                    //discard buffer
                    _serialPort.DiscardInBuffer();
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
        private void ReadDeviceEEPROM(SerialPort _serialPort)
        {
            /// Hardware Lighting Protocol version 1
            //+----------+---------------------+------------+---------------+
            //| Position | Name                | Size(byte) | Default Value |
            //+----------+---------------------+------------+---------------+
            //| 0        | HWL_enable          | 1          | 1             |
            //| 1        | StatusLEDEnable     | 1          | 0             |
            //| 2        | HWL_returnafter     | 1          | 3             |
            //| 3        | HWL_effectMode      | 1          | 0             |
            //| 4        | HWL_effectSpeed     | 1          | 20            |
            //| 5        | HWL_brightness      | 1          | 80            |
            //| 6 - 8    | HWL_singlecolor     | 3          | 255,0,0       |
            //| 9-32     | HWL_palette         | 24         |               |
            //| 33       | HWL_effectIntensity | 1          | 16            |
            //| 34       | HWF_FanSpeed        |            | 127           |
            //| 35-44    | HWF_PerFanSpeed     | 10         | 127           |
            //+----------+---------------------+------------+---------------+


            Device.HWL_enable = _serialPort.ReadByte() == 1 ? true : false;
            Log.Information("HWL_enable: " + Device.HWL_enable);
            Device.StatusLEDEnable = _serialPort.ReadByte() == 1 ? true : false;
            Log.Information("StatusLEDEnable: " + Device.StatusLEDEnable);
            Device.HWL_returnafter = (byte)_serialPort.ReadByte();
            Log.Information("HWL_returnafter: " + Device.HWL_returnafter);
            Device.HWL_effectMode = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectMode: " + Device.HWL_effectMode);
            Device.HWL_effectSpeed = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectSpeed: " + Device.HWL_effectSpeed);
            Device.HWL_brightness = (byte)_serialPort.ReadByte();
            Log.Information("HWL_brightness: " + Device.HWL_brightness);
            Device.HWL_singleColor = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte());
            Log.Information("HWL_singleColor: " + Device.HWL_singleColor);
            if (Device.HWL_palette == null)
            {
                Device.HWL_palette = new Color[8];
            }
            for (int i = 0; i < 8; i++)
            {
                Device.HWL_palette[i] = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte());
                Log.Information("HWL_palette " + i + ": " + Device.HWL_palette[i]);
            }

            Device.HWL_effectIntensity = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectIntensity: " + Device.HWL_effectIntensity);

            var noSignalFanSpeed = _serialPort.ReadByte();
            Device.NoSignalFanSpeed = noSignalFanSpeed < 20 ? 20 : noSignalFanSpeed;
            Log.Information("NoSignalFanSpeed: " + noSignalFanSpeed);
            Device.HWL_MaxLEDPerOutput = (byte)_serialPort.ReadByte();
            Log.Information("HWL_MaxLEDPerOutput: " + Device.HWL_MaxLEDPerOutput);

            UpdateColorSelectionSelectedValue();

        }
        private void UpdateColorSelectionSelectedValue()
        {
            if (Device.HWL_effectMode == 0 || Device.HWL_effectMode == 2)
            {
                HardwareLightingColorSelection.SelectedValue = new ColorCard() { StartColor = Device.HWL_singleColor, StopColor = Device.HWL_singleColor };
            }
            else if (Device.HWL_effectMode == 1)
                HardwareLightingColorSelection.SelectedValue = new ColorPalette() { Colors = Device.HWL_palette };
        }
        public async Task<bool> SendHardwareSettings()
        {
            Thread.Sleep(500);
            ///////////////////// Hardware settings data table, will be wirte to device EEPRom /////////
            /// [h,s,d,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,0,0,0,0,0,0,0] ///////

            Device.IsTransferActive = false; // stop current serial stream attached to this device
            var _serialPort = new SerialPort(Device.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
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
                ReadDeviceEEPROM(_serialPort);
                //discard buffer
                _serialPort.DiscardInBuffer();
            }


            _serialPort.Close();
            _serialPort.Dispose();
            return await Task.FromResult(true);
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            //IsTransferActive = true;
            //RaisePropertyChanged(nameof(IsTransferActive));
        }
        private bool _reloadDeviceLoadingVissible = false;
        public bool ReloadDeviceLoadingVissible {
            get { return _reloadDeviceLoadingVissible; }

            set
            {
                _reloadDeviceLoadingVissible = value;
                RaisePropertyChanged();
            }
        }

        private string _fwUploadOutputLog;
        public string FwUploadOutputLog {
            get { return _fwUploadOutputLog; }

            set
            {
                _fwUploadOutputLog = value;
                RaisePropertyChanged();
            }
        }
        private bool _isDownloadingFirmware;
        public bool IsDownloadingFirmware {
            get
            {
                return _isDownloadingFirmware;
            }
            set
            {
                _isDownloadingFirmware = value;
                RaisePropertyChanged();
            }
        }
        private int _fwUploadPercent;
        public int FwUploadPercent {
            get { return _fwUploadPercent; }

            set
            {
                _fwUploadPercent = value;
                RaisePropertyChanged();
            }
        }
        private async Task BackupDevice()
        {
            var vm = new ProgressDialogViewModel("Backing up device", "123", "usbIcon");
            Task.Run(() => SaveDeviceToFile(vm));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {

            }, vm);

        }
        private async Task RestoreDeviceFromFile()
        {
            if (CurrentSelectedDeviceBackupFile == null)
                return;
            var vm = new ProgressDialogViewModel("Restoring device", "123", "usbIcon");
             Task.Run(() => RestoreDevice(vm));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {

            }, vm);
        }
        private async Task RestoreDevice(ProgressDialogViewModel vm)
        {
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.CurrentProgressHeader = "Deserializing";
            vm.Value = 10;
            await Task.Delay(500);
            //launch save dialog
            Device.IsLoadingProfile = true;
            var dev = ImportDevice(CurrentSelectedDeviceBackupFile);
            if(dev.DeviceType.Type!= Device.DeviceType.Type)
            {
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RestoreDevice_WrongDeviceType, adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RestoreDevice_FileImport, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            dev.DeviceUID = Device.DeviceUID;
            dev.DeviceName = Device.DeviceName;
            vm.CurrentProgressHeader = "Rerstoring";
            vm.Value = 50;
            foreach (PropertyInfo property in typeof(DeviceSettings).GetProperties().Where(p => p.CanWrite && !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute))))
            {
                property.SetValue(Device, property.GetValue(dev, null), null);
            }
            vm.Value = 90;
            vm.CurrentProgressHeader = "Rebooting device";
            DeviceHlprs.WriteSingleDeviceInfoJson(Device);
            Device.RefreshLightingEngine();
            vm.Value = 100;
            vm.Header = "Done";
            vm.SecondaryActionButtonContent = "Close";
            vm.PrimaryActionButtonContent = "Show Log";
            await Task.Delay(500);
            vm.ProgressBarVisibility = Visibility.Hidden;
            vm.SuccessMesageVisibility = Visibility.Visible;
            vm.SuccessMessage = "Device restored successfully!";
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
        private IDeviceSettings ImportDevice(string path)
        {
            if (!File.Exists(path))
                return null;
            IDeviceSettings device;
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                //extract device
                //Create directory to extract
                var deviceTempFolderPath = Path.Combine(CacheFolderPath, fileName);
                Directory.CreateDirectory(deviceTempFolderPath);
                //then extract
                ZipFile.ExtractToDirectory(path, deviceTempFolderPath);
                var deviceJson = File.ReadAllText(Path.Combine(deviceTempFolderPath, "config.json"));
                device = JsonConvert.DeserializeObject<DeviceSettings>(deviceJson);
                if (device != null)
                {
                    //create device info
                    //device.UpdateChildSize();
                    device.UpdateUID();
                    //copy thumb
                    if (File.Exists(Path.Combine(deviceTempFolderPath, "thumbnail.png")) && !File.Exists(Path.Combine(ResourceFolderPath, device.DeviceName + "_thumb.png")))
                    {
                        File.Copy(Path.Combine(deviceTempFolderPath, "thumbnail.png"), Path.Combine(ResourceFolderPath, device.DeviceName + "_thumb.png"), true);
                    }
                    if (File.Exists(Path.Combine(deviceTempFolderPath, "outputmap.png")) && !File.Exists(Path.Combine(ResourceFolderPath, device.DeviceName + "_outputmap.png")))
                    {
                        File.Copy(Path.Combine(deviceTempFolderPath, "outputmap.png"), Path.Combine(ResourceFolderPath, device.DeviceName + "_outputmap.png"), true);
                    }
                    //copy required SlaveDevice
                    var dependenciesFiles = Path.Combine(deviceTempFolderPath, "dependencies");
                    if (Directory.Exists(dependenciesFiles))
                    {
                        foreach (var sub in Directory.GetDirectories(dependenciesFiles))
                        {
                            LocalFileHelpers.CopyDirectory(sub, SupportedDeviceCollectionFolderPath, true);
                        }
                    }
                    //remove cache
                    ClearCacheFolder();
                    return device;
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "File Import", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return null;
        }
        private void ClearCacheFolder()
        {
            System.IO.DirectoryInfo cache = new DirectoryInfo(CacheFolderPath);
            foreach (FileInfo file in cache.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in cache.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }
        private async Task SaveDeviceToFile(ProgressDialogViewModel vm)
        {
            if (Device == null)
                return;
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.CurrentProgressHeader = "Serializing";
            vm.CurrentProgressLog = "Creating output directory...";
            vm.Value = 10;
            await Task.Delay(500);
            //launch save dialog
            Device.IsLoadingProfile = true;
            var path = Path.Combine(BackupFolder, "Device");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var backupPath = Path.Combine(path, Device.DeviceName + Device.DeviceUID + DateTime.Now.ToString("yyyyMMddHHmmss"));
            //deserialize to config
            Directory.CreateDirectory(backupPath);
            var configjson = JsonConvert.SerializeObject(Device);
            vm.CurrentProgressLog = "Serializing device...";
            await Task.Delay(500);
            vm.Value = 20;
            File.WriteAllText(Path.Combine(backupPath, "config.json"), configjson);


            //copy thumbnail
            vm.CurrentProgressLog = "Saving images...";
            vm.Value = 40;
            await Task.Delay(500);
            if (File.Exists(Device.DeviceThumbnail))
                File.Copy(Device.DeviceThumbnail, Path.Combine(backupPath, "thumbnail.png"));
            //copy required slave device folder (config + thumbnail)
            var requiredSlaveDevice = new List<string>();
            vm.CurrentProgressLog = "Serializing dependencies...";
            vm.CurrentProgressHeader = "Almost done";
            vm.Value = 60;
            await Task.Delay(500);

            foreach (ARGBLEDSlaveDevice device in Device.AvailableLightingDevices)
            {
                if (requiredSlaveDevice.Any(p => p == device.Name))
                    continue;
                requiredSlaveDevice.Add(device.Name);
                var requiredSlaveDevicejson = JsonConvert.SerializeObject(device);
                Directory.CreateDirectory(Path.Combine(backupPath, "dependencies", "SlaveDevices", device.Name));
                File.WriteAllText(Path.Combine(backupPath, "dependencies", "SlaveDevices", device.Name, "config.json"), requiredSlaveDevicejson);
                File.Copy(device.Thumbnail, Path.Combine(backupPath, "dependencies", "SlaveDevices", device.Name, Path.GetFileName(device.Thumbnail)));
                File.Copy(device.ThumbnailWithColor, Path.Combine(backupPath, "dependencies", "SlaveDevices", device.Name, Path.GetFileName(device.ThumbnailWithColor)));
            }
            vm.Value = 80;
            vm.CurrentProgressLog = "zipping...";
            var zipPath = Path.Combine(path, Path.GetFileName(backupPath) + ".zip");
            ZipFile.CreateFromDirectory(backupPath, zipPath);
            Directory.Delete(backupPath, true);
            await Task.Delay(500);
            vm.Value = 100;
            vm.Header = "Done";
            vm.SecondaryActionButtonContent = "Close";
            vm.PrimaryActionButtonContent = "Show Log";
            await Task.Delay(500);
            vm.ProgressBarVisibility = Visibility.Hidden;
            vm.SuccessMesageVisibility = Visibility.Visible;
            vm.SuccessMessage = "Device backup files saved to " + path;
            //export
        }

        private void Copyuf2Fw(string fwPath, ProgressDialogViewModel vm)
        {
            vm.CurrentProgressHeader = "Flashing uf2...";
            var drive = DriveInfo.GetDrives().Where(drv => drv.VolumeLabel == "RPI-RP2").FirstOrDefault();
            if (drive == null)
            {
                vm.CurrentProgressHeader = "Can not find device in dfu mode..";
                HandyControl.Controls.MessageBox.Show("Update firmware không thành công, Không tìm thấy thiết bị ở trạng thái DFU", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string target = drive.RootDirectory.ToString();
            try
            {
                File.Copy(fwPath, Path.Combine(target, Path.GetFileName(fwPath)));
            }
            catch (Exception ex)
            {

            }
            //wait for device 
            vm.CurrentProgressHeader = "Waiting for device to reboot...";
            Thread.Sleep(2000);
            // note that since we're in different thread, we cannot interact with UI,
            // so you have to dispatch your operations to UI thread. That can be done just using 
            // Control's Invoke method
            RefreshFirmwareVersion();
            ShowUpdateSuccessMessage(vm);

        }
        private void StartCh55xFWTool(string fwPath, ProgressDialogViewModel vm)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo {
                WorkingDirectory = JsonFWToolsFileNameAndPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = "/C vnproch55x " + fwPath
            };
            var proc = new Process() {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            // see below for output handler
            proc.ErrorDataReceived += (sender, e) => proc_DataReceived(sender, e, vm);
            proc.OutputDataReceived += (sender, e) => proc_DataReceived(sender, e, vm);

            proc.Start();

            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            proc.Exited += (sender, e) => proc_FinishUploading(sender, e, vm); ;
        }

        private async Task UpgradeSelectedDeviceFirmware(IDeviceSettings device, string fwPath, ProgressDialogViewModel vm)
        {

            vm.CurrentProgressHeader = "Rebooting device";
            EnterDFU();
            // wait for device to enter dfu
            vm.CurrentProgressHeader = "Waiting for device";
            vm.ProgressBarVisibility = Visibility.Visible;
            // start fwtool
            if (device.DeviceFirmwareExtension == ".hex")
                StartCh55xFWTool(fwPath, vm);
            else if (device.DeviceFirmwareExtension == ".uf2")
            {
                Copyuf2Fw(fwPath, vm);
            }

        }
        private void EnterDFU()
        {
            var _serialPort = new SerialPort(Device.OutputPort, 1200);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {

                try
                {
                    _serialPort.Open();
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Open() was called twice
                }
                Thread.Sleep(500);
                try
                {
                    _serialPort.Write(new byte[3] { (byte)'f', (byte)'u', (byte)'g' }, 0, 3);
                }
                catch (Exception)
                {
                    // I don't know about this shit but we have to catch an empty exception because somehow SerialPort.Write() was called twice
                }


                Thread.Sleep(1000);
                _serialPort.Close();
            }
            else
            {

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
        private void ShowUpdateSuccessMessage(ProgressDialogViewModel vm)
        {
            percentCount = 0;
            vm.Value = 0;
            vm.ProgressBarVisibility = Visibility.Collapsed;
            vm.Header = "Done";
            vm.SuccessMessage = "Update thành công! Phiên bản" + " " + Device.FirmwareVersion;
            vm.SuccessMesageVisibility = Visibility.Visible;
            vm.SecondaryActionButtonContent = "Close";
            vm.PrimaryActionButtonContent = "Show Log";
            UpdateAvailable = false;
            UpdateButtonContent = "Check for update";
            NewFirmwareVersionContent = "";
            FwUploadOutputLog = string.Empty;
            ReloadDeviceLoadingVissible = false;
            FrimwareUpgradeIsInProgress = false;
        }
        private void proc_FinishUploading(object sender, System.EventArgs e, ProgressDialogViewModel vm)
        {
            //FwUploadPercent = 0;
            ////clear loading bar
            //FwUploadOutputLog = String.Empty;
            ////clear text box
            //percentCount = 0;
            ReloadDeviceLoadingVissible = true;

            Thread.Sleep(5000);
            Device.DeviceState = DeviceStateEnum.Normal;

            if (FwUploadOutputLog.Split('\n').Last() == "Found no CH55x USB")
            {
                //there is a chance of missing driver so first we install CH375 driver first
                //execute CH375 driver

                //try to restart uploading by resetting the state

                HandyControl.Controls.MessageBox.Show("Update firmware không thành công, Không tìm thấy thiết bị ở trạng thái DFU", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // check for current device actual firmware version
                RefreshFirmwareVersion();
                // show success
                ShowUpdateSuccessMessage(vm);

            }
        }

        private int percentCount = 0;

        private void proc_DataReceived(object sender, DataReceivedEventArgs e, ProgressDialogViewModel vm)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("[2K"))//clear current line
                {
                    percentCount++;
                    FwUploadPercent = percentCount * 100 / 308;
                    vm.Value = FwUploadPercent;
                    if (FwUploadPercent <= 80)
                        vm.CurrentProgressHeader = "Flashing";
                    else
                        vm.CurrentProgressHeader = "Almost Done";
                    //Dispatcher.BeginInvoke(new Action(() => Prog.Value = percent));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (Environment.NewLine + percentCount)));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (e.Data)));
                }
                else
                {
                    FwUploadOutputLog += Environment.NewLine + e.Data;
                    vm.CurrentProgressLog = e.Data;
                    Log.Information(e.Data);
                }
            }
        }
        private async Task ApplyDeviceHardwareSettings()
        {
            IsApplyingDeviceHardwareSettings = true;
            var result = await Task.Run(() => SendHardwareSettings());
            IsApplyingDeviceHardwareSettings = false;
        }
        public bool FrimwareUpgradeIsInProgress { get; set; }
        private string _currentSelectedFirmware;
        public string CurrentSelectedFirmware {
            get
            {
                return _currentSelectedFirmware;
            }

            set
            {
                _currentSelectedFirmware = value;
                RaisePropertyChanged();
            }
        }
        private string _currentSelectedDeviceBackupFile;
        public string CurrentSelectedDeviceBackupFile {
            get
            {
                return _currentSelectedDeviceBackupFile;
            }

            set
            {
                _currentSelectedDeviceBackupFile = value;
                RaisePropertyChanged();
            }
        }
        private string _updateButtonContent = "Check for update";
        public string UpdateButtonContent {
            get
            {
                return _updateButtonContent;
            }

            set
            {
                _updateButtonContent = value;
                RaisePropertyChanged();
            }
        }
        private string _updateInstructionContent = adrilight_shared.Properties.Resources.CheckForUpdate_content;
        public string UpdateInstructionContent {
            get
            {
                return _updateInstructionContent;
            }

            set
            {
                _updateInstructionContent = value;
                RaisePropertyChanged();
            }
        }
        private string _newFirmwareVersionContent;
        public string NewFirmwareVersionContent {
            get
            {
                return _newFirmwareVersionContent;
            }
            set
            {
                _newFirmwareVersionContent = value;
                RaisePropertyChanged();
            }
        }
        private bool _updateAvailable;
        public bool UpdateAvailable {
            get
            {
                return _updateAvailable;
            }
            set
            {
                _updateAvailable = value;
                RaisePropertyChanged();
            }
        }
        private DeviceFirmware FirmwareToUpdate;
        private async Task UpdateNow()
        {
            //download newest firmware
            if (FirmwareToUpdate == null) return;
            var fwOutputLocation = Path.Combine(JsonFWToolsFileNameAndPath, FirmwareToUpdate.Name);
            try
            {
                ResourceHlprs.CopyResource(FirmwareToUpdate.ResourceName, fwOutputLocation);
            }
            catch (ArgumentException)
            {
                //show messagebox no firmware found for this device
                return;
            }
            var vm = new ProgressDialogViewModel("Flashing Firmware", "123", "usbIcon");
            IsDownloadingFirmware = true;
            Task.Run(() => UpgradeSelectedDeviceFirmware(Device, fwOutputLocation, vm));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {

            }, vm);

            IsDownloadingFirmware = false;
        }
        private async Task UpdateCustomFirmware()
        {
            //download newest firmware
            if (CurrentSelectedFirmware == null)
                return;
            if (!File.Exists(CurrentSelectedFirmware))
                return;
            var vm = new ProgressDialogViewModel("Flashing Firmware", "123", "usbIcon");
            IsDownloadingFirmware = true;
            vm.ProgressBarVisibility = Visibility.Visible;
            await Task.Run(() => UpgradeSelectedDeviceFirmware(Device, CurrentSelectedFirmware, vm));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {
                //if (result == "True")
                //{
                //    var newCollection = CreateDataCollectionFromSelectedItem(vm.Content, p);
                //    _collectionItemStore.CreateCollection(newCollection);
                //}

            }, vm);

            IsDownloadingFirmware = false;
        }
        private async Task CheckForUpdate()
        {
            if (Device == null)
                return;
            FrimwareUpgradeIsInProgress = true;
            if (Device.HardwareVersion == "unknown") // old firmware or not supported
            {
                HandyControl.Controls.MessageBox.Show("Thiết bị đang ở firmware cũ hoặc phần cứng không hỗ trợ! Sử dụng trình sửa lỗi firmware để cập nhật", "Unknown hardware version", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // regconize this device, find the compatible firmware
                var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                var requiredFwVersion = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);

                var currentDeviceFirmwareInfo = requiredFwVersion.Where(p => p.TargetHardware == Device.HardwareVersion).FirstOrDefault();
                if (currentDeviceFirmwareInfo == null)
                {
                    var result = HandyControl.Controls.MessageBox.Show("Phần cứng không còn được hỗ trợ hoặc không nhận ra: " + Device.HardwareVersion + " Sử dụng trình sửa lỗi firmware để cập nhật", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var currentVersion = new Version(Device.FirmwareVersion);
                    var newVersion = new Version(currentDeviceFirmwareInfo.Version);
                    if (newVersion > currentVersion)
                    {
                        //coppy hex file to FWTools folder
                        NewFirmwareVersionContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_UpdateAvailable + newVersion;
                        UpdateAvailable = true;
                        UpdateButtonContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_UpdateNow;
                        FirmwareToUpdate = currentDeviceFirmwareInfo;
                    }
                    else
                    {
                        UpdateAvailable = false;
                        UpdateButtonContent = adrilight_shared.Properties.Resources.CheckForUpdate_content;
                        NewFirmwareVersionContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_NewestFirmware;
                    }
                }
            }

        }
        public bool RefreshFirmwareVersion()
        {
            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            byte[] hw = new byte[256];
            Device.IsTransferActive = false; // stop current serial stream attached to this device
            var _serialPort = new SerialPort(Device.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return false;

            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RefreshFirmwareVersion_Disconnect, adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RefreshFirmwareVersion_DeviceDisconnect_header, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            //write request info command
            _serialPort.Write(requestCommand, 0, 4);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            int hwLength = 0;
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
                    _serialPort.Write(requestCommand, 0, 4);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + "Không có thông tin về Firmware, vui lòng liên hệ Ambino trước khi cập nhật firmware thủ công", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    }
                    Debug.WriteLine("no respond, retrying...");
                    return false;
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


                Device.DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
                RaisePropertyChanged(nameof(Device.DeviceSerial));
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
                // DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
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
                Device.FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
                RaisePropertyChanged(nameof(Device.FirmwareVersion));
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
                    Device.HardwareVersion = Encoding.ASCII.GetString(hw, 0, hw.Length);
                }
                catch (TimeoutException)
                {
                    Log.Information(Device.DeviceName, "Unknown Firmware Version");
                    Device.HardwareVersion = "unknown";
                }

            }
            if (offset == 3 + idLength + nameLength + fwLength + hwLength) //3 bytes header are valid
            {
                Device.HWL_version = 0;
                try
                {
                    Device.HWL_version = _serialPort.ReadByte();
                }
                catch (TimeoutException)
                {
                    Log.Information(name.ToString(), "Unknown Hardware Lighting Version");
                }

            }
            _serialPort.Close();
            _serialPort.Dispose();
            return true;
        }
        #endregion
        #region Ambino Protocol
        /// Hardware Lighting Protocol version 0
        //+----------+---------------------+------------+---------------+
        //| Position | Name                | Size(byte) | Default Value |
        //+----------+---------------------+------------+---------------+
        //| 0        | HWL_enable          | 1          | 1             |
        //| 1        | StatusLEDEnable     | 1          | 0             |
        //| 2        | HWL_tbd             | 1          | 255           |
        //| 3        | HWL_tbd             | 1          | 255           |
        //| 4        | HWL_tbd             | 1          | 255           |
        //| 5        | HWL_tbd             | 1          | 255           |
        //| 6        | HWF_FanSpeed        | 1          | 127           |
        //| 7        | HWL_tbd             | 1          | 255           |
        //| 8        | HWL_tbd             | 1          | 255           |
        //| 9        | HWL_tbd             | 1          | 255           |
        //| 10       | HWL_tbd             | 1          | 255           |
        //| 11       | HWL_tbd             | 1          | 255           |
        //| 12       | HWL_tbd             | 1          | 255           |
        //+----------+---------------------+------------+---------------+
        #endregion

    }
}

