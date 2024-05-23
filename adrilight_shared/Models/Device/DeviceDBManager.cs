using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight_shared.Models.Device
{
    public class DeviceDBManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string BackupFolder => Path.Combine(JsonPath, "Backup");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        #region Construct
        public DeviceDBManager(DialogService dialogService)
        {
            _dialogService = dialogService;
            _deviceHlprs = new DeviceHelpers();

        }

        #endregion
        #region Properties

        private IDeviceSettings Device;
        private DialogService _dialogService;
        private DeviceHelpers _deviceHlprs;
        #endregion

        #region Methods
        public void Init(IDeviceSettings device)
        {
            Device = device;
        }
        public async Task BackupDevice()
        {
            var vm = new ProgressDialogViewModel("Backing up device", "123", "usbIcon");
            Task.Run(() => SaveDeviceToFile(vm));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {

            }, vm);

        }
        public async Task RestoreDeviceFromFile(string path)
        {
            if (path == null)
                return;
            var vm = new ProgressDialogViewModel("Restoring device", "123", "usbIcon");
            Task.Run(() => RestoreDevice(vm, path));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {

            }, vm);
        }
        private async Task RestoreDevice(ProgressDialogViewModel vm, string path)
        {
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.CurrentProgressHeader = "Deserializing";
            vm.Value = 10;
            await Task.Delay(500);
            //launch save dialog
            Device.IsLoadingProfile = true;
            var dev = ImportDevice(path);
            if (dev.DeviceType.Type != Device.DeviceType.Type)
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
            _deviceHlprs.WriteSingleDeviceInfoJson(Device);
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
        public IDeviceSettings ImportDevice(string path)
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
        public void OpenDevicesFolder()
        {
            if (Directory.Exists(DevicesCollectionFolderPath))
                Process.Start("explorer.exe", DevicesCollectionFolderPath);
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
        //this will load devices from default device folder
        public List<DeviceSettings> LoadDeviceFromFolder(string folderPath)
        {
            var devices = new List<DeviceSettings>();
            if (!Directory.Exists(folderPath)) return null; // no device has been added

            foreach (var folder in Directory.GetDirectories(folderPath))
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(folder, "config.json"));
                    var device = JsonConvert.DeserializeObject<DeviceSettings>(json);
                    device.AvailableControllers = new List<IDeviceController>();
                    //read slave device info
                    //check if this device contains lighting controller
                    var lightingoutputDir = Path.Combine(Path.Combine(folder, "LightingOutputs"));
                    var pwmoutputDir = Path.Combine(Path.Combine(folder, "PWMOutputs"));
                    DeserializeChild<ARGBLEDSlaveDevice>(lightingoutputDir, device, OutputTypeEnum.ARGBLEDOutput);
                    DeserializeChild<PWMMotorSlaveDevice>(pwmoutputDir, device, OutputTypeEnum.PWMOutput);
                    devices.Add(device);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, folder);
                    continue;
                }
            }
            return devices;
        }
        private void DeserializeChild<T>(string outputDir, IDeviceSettings device, OutputTypeEnum outputType)
        {
            if (Directory.Exists(outputDir))
            {
                //add controller to this device

                var controller = new DeviceController();
                switch (outputType)
                {
                    case OutputTypeEnum.PWMOutput:
                        controller.Geometry = "fanSpeedController";
                        controller.Name = "Fan";
                        controller.Type = ControllerTypeEnum.PWMController;
                        break;
                    case OutputTypeEnum.ARGBLEDOutput:
                        controller.Geometry = "brightness";
                        controller.Name = "Lighting";
                        controller.Type = ControllerTypeEnum.LightingController;
                        break;
                }


                foreach (var subfolder in Directory.GetDirectories(outputDir)) // each subfolder contains 1 slave device
                {
                    //read slave device info
                    var outputJson = File.ReadAllText(Path.Combine(subfolder, "config.json"));
                    var output = JsonConvert.DeserializeObject<OutputSettings>(outputJson);
                    var slaveDeviceJson = File.ReadAllText(Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "config.json"));
                    var slaveDevice = JsonConvert.DeserializeObject<T>(slaveDeviceJson);

                    if (slaveDevice == null)//somehow data corrupted
                        continue;
                    else
                    {
                        if (!File.Exists((slaveDevice as ISlaveDevice).Thumbnail))
                        {
                            //(slaveDevice as ISlaveDevice).Thumbnail = Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "thumbnail.png");
                        }
                    }


                    output.SlaveDevice = slaveDevice as ISlaveDevice;
                    controller.Outputs.Add(output);
                    //each slave device attach to one output so we need to create output
                    //lightin

                }
                device.AvailableControllers.Add(controller);
            }
        }
        #endregion
    }
}
