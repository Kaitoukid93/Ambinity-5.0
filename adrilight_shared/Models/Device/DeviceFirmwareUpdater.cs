using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace adrilight_shared.Models.Device
{
    public class DeviceFirmwareUpdater
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");
        private string JsonFWToolsFWListFileNameAndPath => Path.Combine(JsonFWToolsFileNameAndPath, "adrilight-fwlist.json");

        private ResourceHelpers _resourceHlprs;

        public DeviceFirmwareUpdater(DialogService dialogService) {
            _dialogService = dialogService;
            _resourceHlprs = new ResourceHelpers();
        }
        private DialogService _dialogService;
        public void Init(IDeviceSettings device)
        {
            Device = device;
            _fwUpdateLog = string.Empty;
            _fwPath = null;
            percentCount = 0;

        }
        private IDeviceSettings Device;
        private string _fwUpdateLog;
        private int percentCount = 0;
        private string _fwPath;
       
        public async Task Update()
        {
            if (_fwPath == null)
                return;
            await DownloadToDevice(_fwPath);
        }
        public async Task Update(string path)
        {
            if (path == null)
                return;
            await DownloadToDevice(path);
        }
        private async Task DownloadToDevice(string fwPath)
        {
            //download newest firmware
            if (fwPath == null)
                return;
            if (!File.Exists(fwPath))
                return;
            var vm = new ProgressDialogViewModel("Flashing Firmware", "123", "usbIcon");
            vm.ProgressBarVisibility = Visibility.Visible;
            EnterDFU();
            if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                return;
            }
            vm.CurrentProgressHeader = "Rebooting device";
            // wait for device to enter dfu
            vm.CurrentProgressHeader = "Waiting for device";
            vm.ProgressBarVisibility = Visibility.Visible;
            // start fwtool
            if (Device.DeviceFirmwareExtension == ".hex")
                StartCh55xFWTool(fwPath, vm);
            else if (Device.DeviceFirmwareExtension == ".uf2")
            {
                Copyuf2Fw(fwPath, vm);
            }
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {
                //try update the view if needed

            }, vm);

        }
        public async Task<bool> CheckForUpdate()
        {
            //make this asycn so in the future, we will fetch firmware update from the server
            if (Device == null)
                return false;
            if (Device.HardwareVersion == "unknown") // old firmware or not supported
            {
                HandyControl.Controls.MessageBox.Show("Thiết bị đang ở firmware cũ hoặc phần cứng không hỗ trợ! Sử dụng trình sửa lỗi firmware để cập nhật", "Unknown hardware version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // regconize this device, find the compatible firmware
            var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
            var requiredFwVersion = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);

            var currentDeviceFirmwareInfo = requiredFwVersion.Where(p => p.TargetHardware == Device.HardwareVersion).FirstOrDefault();
            if (currentDeviceFirmwareInfo == null)
            {
                var result = HandyControl.Controls.MessageBox.Show("Phần cứng không còn được hỗ trợ hoặc không nhận ra: " + Device.HardwareVersion + " Sử dụng trình sửa lỗi firmware để cập nhật", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var currentVersion = new Version(Device.FirmwareVersion);
            var newVersion = new Version(currentDeviceFirmwareInfo.Version);
            if (newVersion > currentVersion)
            {
                //coppy hex file to FWTools folder
                //NewFirmwareVersionContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_UpdateAvailable + newVersion;
                //UpdateAvailable = true;
                //UpdateButtonContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_UpdateNow;
                var fwOutputLocation = Path.Combine(JsonFWToolsFileNameAndPath, currentDeviceFirmwareInfo.Name);
                try
                {
                    _resourceHlprs.CopyResource(currentDeviceFirmwareInfo.ResourceName, fwOutputLocation);
                    _fwPath = fwOutputLocation;
                }
                catch (ArgumentException)
                {
                    //show messagebox no firmware found for this device
                    return false;
                }
                return true;
            }
            else
            {
                //UpdateAvailable = false;
                //UpdateButtonContent = adrilight_shared.Properties.Resources.CheckForUpdate_content;
                //NewFirmwareVersionContent = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_CheckForUpdate_NewestFirmware;
                return false;
            }

        }
        private void StartCh55xFWTool(string fwPath, ProgressDialogViewModel vm)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = JsonFWToolsFileNameAndPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = "/C vnproch55x " + fwPath
            };
            var proc = new Process()
            {
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
           // RefreshFirmwareVersion();
            ShowUpdateSuccessMessage(vm);

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
            vm.Value = 0;
            vm.ProgressBarVisibility = Visibility.Collapsed;
            vm.Header = "Done";
            vm.SuccessMessage = "Update thành công! Phiên bản" + " " + Device.FirmwareVersion;
            vm.SuccessMesageVisibility = Visibility.Visible;
            vm.SecondaryActionButtonContent = "Close";
            vm.PrimaryActionButtonContent = "Show Log";
        }
        private void proc_FinishUploading(object sender, System.EventArgs e, ProgressDialogViewModel vm)
        {

            Thread.Sleep(5000);
            Device.DeviceState = DeviceStateEnum.Normal;

            if (_fwUpdateLog.Split('\n').Last() == "Found no CH55x USB")
            {
                //there is a chance of missing driver so first we install CH375 driver first
                //execute CH375 driver

                //try to restart uploading by resetting the state

                HandyControl.Controls.MessageBox.Show("Update firmware không thành công, Không tìm thấy thiết bị ở trạng thái DFU", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // show success
                ShowUpdateSuccessMessage(vm);

            }
        }
        private void proc_DataReceived(object sender, DataReceivedEventArgs e, ProgressDialogViewModel vm)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("[2K"))//clear current line
                {
                    percentCount++;
                    var percent = percentCount * 100 / 308;
                    vm.Value = percent;
                    if (percent <= 80)
                        vm.CurrentProgressHeader = "Flashing";
                    else
                        vm.CurrentProgressHeader = "Almost Done";

                }
                else
                {
                    _fwUpdateLog += Environment.NewLine + e.Data;
                    vm.CurrentProgressLog = e.Data;
                    Log.Information(e.Data);
                }
            }
        }
    }
}
