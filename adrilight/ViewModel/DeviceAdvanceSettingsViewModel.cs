using adrilight.Helpers;
using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

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
        public DeviceAdvanceSettingsViewModel(DialogService service, IDeviceSettings device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                UpdateButtonContent = "Enter DFU";
                UpdateInstructionContent = "HUBV2 cần sử dụng FlyMCU để nạp firmware";
            }

            _dialogService = service ?? throw new ArgumentNullException(nameof(service));
            ResourceHlprs = new ResourceHelpers();
            LocalFileHlprs = new LocalFileHelpers();
            CommandSetup();
        }
        #endregion

        #region Properties
        public IDeviceSettings Device { get; set; }
        private DialogService _dialogService;
        private ResourceHelpers ResourceHlprs;
        private LocalFileHelpers LocalFileHlprs;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r', (byte)'\n' };
        private static byte[] sendCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
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

        #endregion
       

        #region Methods
        private void CommandSetup()
        {
            ShowDeviceBackupFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            },  (p) =>
            {
                var backupPath = Path.Combine(BackupFolder, "Device");
                if(Directory.Exists(backupPath))
                Process.Start("explorer.exe", backupPath);

            });
            BackupDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await BackupDevice();

            });
            SelecFirmwareForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var file = LocalFileHlprs.OpenImportFileDialog(".hex", "hex Files (.HEX)|*.hex");
                if (file != null)
                {
                    CurrentSelectedFirmware = file;
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
                        await EnterDFU();
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
        public async Task RefreshDeviceHardwareInfo()
        {
            //get device settings info
            //if (!Device.IsTransferActive)
            //{
            //    return;
            //}
            ////IsApplyingDeviceHardwareSettings = true;
            var rslt = await Task.Run(() => GetHardwareSettings());
            if (!rslt)
            {
                HardwareSettingsEnable = Visibility.Collapsed;
            }
            else
            {
                HardwareSettingsEnable = Visibility.Visible;
            }

            //if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceFirmwareSettingsWindow"}") is System.Windows.Window window)
            //{
            //    //reset progress and log display
            //    FwUploadPercentVissible = false;
            //    percentCount = 0;
            //    FwUploadPercent = 0;
            //    FwUploadOutputLog = String.Empty;
            //    window.Owner = System.Windows.Application.Current.MainWindow;
            //    window.ShowDialog();
            //}

        }
        #endregion


        #region Commands
        public ICommand WindowClosing { get; private set; }
        public ICommand BackupDeviceCommand { get; set; }
        public ICommand WindowOpen { get; private set; }
        public ICommand ShowDeviceBackupFolderCommand { get; set; }
        public ICommand SelecFirmwareForCurrentDeviceCommand { get; set; }
        public ICommand UpdateCurrentSelectedDeviceFirmwareCommand { get; set; }
        public ICommand ApplyDeviceHardwareSettingsCommand { get; set; }
        public ICommand UpdateCustomFirmwareCommand { get; set; }
        #endregion


        #region Hardware Related Properties and Method
        private byte[] GetSettingOutputStream()
        {
            var outputStream = new byte[16];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            int counter = sendCommand.Length;
            outputStream[counter++] = Device.NoSignalLEDEnable == true ? (byte)15 : (byte)12;
            outputStream[counter++] = Device.IsIndicatorLEDOn == true ? (byte)15 : (byte)12;
            outputStream[counter++] = 0;
            outputStream[counter++] = 0;
            outputStream[counter++] = 0;
            outputStream[counter++] = 0;
            outputStream[counter++] = (byte)Device.NoSignalFanSpeed;
            // 6 bytes CustomID
            outputStream[counter++] = 11;
            outputStream[counter++] = 22;
            outputStream[counter++] = 33;
            outputStream[counter++] = 44;
            outputStream[counter++] = 55;
            outputStream[counter++] = 66;
            return outputStream;
        }
        private byte[] GetEEPRomDataOutputStream()
        {
            var outputStream = new byte[16];
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
            ///////////////////// Hardware settings data table, will be wirte to device EEPRom /////////
            /// [h,s,d,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,0,0,0,0,0,0,0] ///////
            await Task.Run(() => RefreshFirmwareVersion());
            if (!IsFirmwareValid())
            {
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
                        Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
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
                try
                {
                    //led on off
                    var noDataLEDEnable = _serialPort.ReadByte();
                    Device.NoSignalLEDEnable = noDataLEDEnable == 15 ? true : false;
                    Log.Information("Device EEPRom Data: " + noDataLEDEnable);
                    //signal led on off
                    var signalLEDEnable = _serialPort.ReadByte();
                    Device.IsIndicatorLEDOn = signalLEDEnable == 15 ? true : false;
                    Log.Information("Device EEPRom Data: " + signalLEDEnable);
                    if (_serialPort.BytesToRead > 0)
                    {
                        var connectionType = _serialPort.ReadByte();
                        var maxBrightness = _serialPort.ReadByte();
                        var showWelcomeLED = _serialPort.ReadByte();
                        var serialTimeout = _serialPort.ReadByte();
                        var noSignalFanSpeed = _serialPort.ReadByte();
                        Device.NoSignalFanSpeed = noSignalFanSpeed < 20 ? 20 : noSignalFanSpeed;
                        Log.Information("Device EEPRom Data: " + noSignalFanSpeed);
                        string[] id = new string[6];

                        for (int i = 0; i < 6; i++)
                        {
                            id[i] = _serialPort.ReadByte().ToString();
                        }
                        Log.Information("Device Custom ID: " + id[0].ToString() + id[1].ToString() + id[2].ToString() + id[3].ToString() + id[4].ToString() + id[5].ToString());

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

                /// [15,12,93,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,0,0,0,0,0,0,0] ///////
                //led on off
                var noDataLEDEnable = _serialPort.ReadByte();
                Log.Information("Device EEPRom Data: " + noDataLEDEnable);
                //signal led on off
                var signalLEDEnable = _serialPort.ReadByte();
                Log.Information("Device EEPRom Data: " + signalLEDEnable);
                if (_serialPort.BytesToRead > 0)
                {
                    var connectionType = _serialPort.ReadByte();
                    var maxBrightness = _serialPort.ReadByte();
                    var showWelcomeLED = _serialPort.ReadByte();
                    var serialTimeout = _serialPort.ReadByte();
                    var noSignalFanSpeed = _serialPort.ReadByte();
                    Device.NoSignalFanSpeed = noSignalFanSpeed < 20 ? 20 : noSignalFanSpeed;
                    Log.Information("Device EEPRom Data: " + noSignalFanSpeed);
                    string[] id = new string[6];

                    for (int i = 0; i < 6; i++)
                    {
                        id[i] = _serialPort.ReadByte().ToString();
                    }
                    Log.Information("Device Custom ID: " + id[0].ToString() + id[1].ToString() + id[2].ToString() + id[3].ToString() + id[4].ToString() + id[5].ToString());

                }
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
        private ObservableCollection<DeviceFirmware> _availableFirmware;
        public ObservableCollection<DeviceFirmware> AvailableFirmware {
            get { return _availableFirmware; }

            set
            {
                if (_availableFirmware == value) return;
                _availableFirmware = value;

                RaisePropertyChanged();
            }
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

        private bool _fwUploadPercentVissible = false;
        public bool FwUploadPercentVissible {
            get { return _fwUploadPercentVissible; }

            set
            {
                _fwUploadPercentVissible = value;
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
            var zipPath = Path.Combine(path, Path.GetFileName(backupPath) + ".zip") ;
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
        private async Task UpgradeSelectedDeviceFirmware(IDeviceSettings device, string fwPath, ProgressDialogViewModel vm)
        {
            //if (GeneralSettings.DriverRequested)
            //{
            //    await Task.Run(() => PromptDriverInstaller());
            //    return;
            //}
            //put device in dfu state
            vm.CurrentProgressHeader = "Rebooting device";
            device.DeviceState = DeviceStateEnum.DFU;
            // wait for device to enter dfu
            Thread.Sleep(1000);
            vm.CurrentProgressHeader = "Waiting for device";
            vm.ProgressBarVisibility = Visibility.Visible;
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
                // reset loading bar
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
        private string _updateInstructionContent = "Kiểm tra bản cập nhật nếu có";
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
        private async Task EnterDFU()
        {
            Device.DeviceState = DeviceStateEnum.DFU;
            Thread.Sleep(5000);
            Device.DeviceState = DeviceStateEnum.Normal;
            HandyControl.Controls.MessageBox.Show("Đã gửi thông tin đến Device, mở FlyMCU để tiếp tục nạp firmware sau đó bật lại kết nối", "DFU", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async Task CheckForUpdate()
        {
            if (Device == null)
                return;
            FrimwareUpgradeIsInProgress = true;
            if (Device.HardwareVersion == "unknown") // old firmware or not supported
            {
                // show message box : unknown hardware version, please update firmware manually by chosing one of these firmware file in the list below
                HandyControl.Controls.MessageBox.Show("Thiết bị đang ở firmware cũ hoặc phần cứng không hỗ trợ! Sử dụng trình sửa lỗi firmware để cập nhật", "Unknown hardware version", MessageBoxButton.OK, MessageBoxImage.Warning);
                //if (result == MessageBoxResult.Yes)
                //{
                //    //grab available firmware for current device type
                //    var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                //    var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);
                //    AvailableFirmware = new ObservableCollection<DeviceFirmware>();
                //    foreach (var firmware in availableFirmware)
                //    {
                //        if (firmware.TargetDeviceType == device.DeviceType.Type)
                //            AvailableFirmware.Add(firmware);
                //    }

                //    // show list selected firmware
                //    //OpenFirmwareSelectionWindow();
                //}
            }
            else
            {
                // regconize this device, find the compatible firmware
                var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                var requiredFwVersion = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);

                var currentDeviceFirmwareInfo = requiredFwVersion.Where(p => p.TargetHardware == Device.HardwareVersion).FirstOrDefault();
                if (currentDeviceFirmwareInfo == null)
                {
                    //not supported hardware

                    var result = HandyControl.Controls.MessageBox.Show("Phần cứng không còn được hỗ trợ hoặc không nhận ra: " + Device.HardwareVersion + " Sử dụng trình sửa lỗi firmware để cập nhật", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
                    //if (result == MessageBoxResult.Yes)
                    //{
                    //    var fwjson = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                    //    var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(fwjson);
                    //    AvailableFirmware = new ObservableCollection<DeviceFirmware>();
                    //    foreach (var firmware in availableFirmware)
                    //    {
                    //        if (firmware.TargetDeviceType == device.DeviceType.Type)
                    //            AvailableFirmware.Add(firmware);
                    //    }

                    //    // show list selected firmware
                    //    //OpenFirmwareSelectionWindow();
                    //}
                }
                else
                {
                    var currentVersion = new Version(Device.FirmwareVersion);
                    var newVersion = new Version(currentDeviceFirmwareInfo.Version);
                    if (newVersion > currentVersion)
                    {
                        //coppy hex file to FWTools folder
                        NewFirmwareVersionContent = "--> Update available: " + newVersion;
                        UpdateAvailable = true;
                        UpdateButtonContent = "Update now";
                        FirmwareToUpdate = currentDeviceFirmwareInfo;
                    }
                    else
                    {
                        UpdateAvailable = false;
                        UpdateButtonContent = "Check for update";
                        NewFirmwareVersionContent = "Thiết bị đang chạy firmware mới nhất!";
                        //if (result == MessageBoxResult.Yes)
                        //{
                        //    IsDownloadingFirmware = true;
                        //    await Task.Run(() => UpgradeSelectedDeviceFirmware(Device, fwOutputLocation));
                        //    IsDownloadingFirmware = false;
                        //}
                        //FrimwareUpgradeIsInProgress = false;
                    }
                }
            }

        }
        public void RefreshFirmwareVersion()
        {

            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            byte[] hw = new byte[256];
            bool isValid = false;


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
                return;
            }
            catch (Exception ex)
            {
                return;
            }
            //write request info command
            _serialPort.Write(requestCommand, 0, 4);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            int hwLength = 0;
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
                    _serialPort.Write(requestCommand, 0, 4);
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
                    Log.Information(newDevice.DeviceName, "Unknown Firmware Version");
                    newDevice.HardwareVersion = "unknown";
                }

            }
            _serialPort.Close();
            _serialPort.Dispose();
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            // IsTransferActive = true;
            //RaisePropertyChanged(nameof(IsTransferActive));
        }
        #endregion


    }
}
