using adrilight.Services.OpenRGBService;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.AppProfile;
using adrilight_shared.Models.Audio;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Language;
using adrilight_shared.Models.Stores;
using adrilight_shared.Models.User;
using adrilight_shared.Settings;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using System.Linq;

namespace adrilight.Manager
{
    public class DBmanager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");

        private string JsonFWToolsFWListFileNameAndPath => Path.Combine(JsonFWToolsFileNameAndPath, "adrilight-fwlist.json");
        private string JsonGifsCollectionFileNameAndPath => Path.Combine(JsonPath, "adrilight-gifCollection.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");

        private string JsonGradientFileNameAndPath => Path.Combine(JsonPath, "adrilight-GradientCollection.json");

        #region database local folder paths

        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string LightingProfilesCollectionFolderPath => Path.Combine(JsonPath, "LightingProfiles");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string GifsCollectionFolderPath => Path.Combine(JsonPath, "Gifs");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string ProfileCollectionFolderPath => Path.Combine(JsonPath, "Profiles");

        #endregion database local folder paths
        public DBmanager()
        {
            ResourceHlprs = new ResourceHelpers();
            LocalFileHlprs = new LocalFileHelpers();
            DeviceHlprs = new DeviceHelpers();
            FilesQToRemove = new ObservableCollection<string>();
            StartThread();

        }

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private ResourceHelpers ResourceHlprs;
        private DeviceHelpers DeviceHlprs;
        private LocalFileHelpers LocalFileHlprs;
        public void StartThread()
        {
            //if (App.IsPrivateBuild) return;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                Name = "DBManager",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();
        }

        public ObservableCollection<string> FilesQToRemove { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private void DBInit()
        {
                CreateColorCollectionFolder();
                CreatePaletteCollectionFolder();
                CreateResourceCollectionFolder();
                CreateChasingPatternCollectionFolder();
                CreateGifCollectionFolder();
                CreateVIDCollectionFolder();
                CreateMIDCollectionFolder();
                CreateProfileCollectionFolder();
                CreateSupportedDevicesCollectionFolder();
            
        }
        private void CreateFWToolsFolderAndFiles()
        {
            if (!Directory.Exists(JsonFWToolsFileNameAndPath))
            {
                Directory.CreateDirectory(JsonFWToolsFileNameAndPath);
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.busybox.exe", Path.Combine(JsonFWToolsFileNameAndPath, "busybox.exe"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.CH375DLL.dll", Path.Combine(JsonFWToolsFileNameAndPath, "CH375DLL.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libgcc_s_sjlj-1.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libgcc_s_sjlj-1.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libusb-1.0.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusb-1.0.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libusbK.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusbK.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.vnproch55x.exe", Path.Combine(JsonFWToolsFileNameAndPath, "vnproch55x.exe"));
                //required fw version
            }
            CreateRequiredFwVersionJson();
        }

        private void CreateColorCollectionFolder()
        {
            if (!Directory.Exists(ColorsCollectionFolderPath))
            {
                Directory.CreateDirectory(ColorsCollectionFolderPath);
                //get data from resource file and copy to local folder
                var colorCollectionResourcePath = "adrilight_shared.Resources.Colors.ColorCollection.json";
                ResourceHlprs.CopyResource(colorCollectionResourcePath, Path.Combine(ColorsCollectionFolderPath, "collection.json"));
                //Create deserialize config
                var config = new ResourceLoaderConfig(nameof(ColorCard), DeserializeMethodEnum.SingleJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(ColorsCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }
        private void CreateGifCollectionFolder()
        {
            if (!Directory.Exists(GifsCollectionFolderPath))
            {
                Directory.CreateDirectory(GifsCollectionFolderPath);
                var collectionFolder = Path.Combine(GifsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".gif")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(Gif), DeserializeMethodEnum.Files);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(GifsCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }
        private void CreateVIDCollectionFolder()
        {
            if (!Directory.Exists(VIDCollectionFolderPath))
            {
                Directory.CreateDirectory(VIDCollectionFolderPath);
                var collectionFolder = Path.Combine(VIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var vidCollection = new List<VIDDataModel>();
                var lef2Right = new VIDDataModel() {
                    Name = "Trái sang phải",
                    IsDeleteable = false,
                    Geometry = "left2right",
                    Description = "Màu chạy từ trái sang phải",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.left2right
                };
                var right2Left = new VIDDataModel() {
                    Name = "Phải sang trái",
                    IsDeleteable = false,
                    Geometry = "right2left",
                    Description = "Màu chạy từ phải sang trái",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.right2left
                };
                var up2Down = new VIDDataModel() {
                    Name = "Trên xuống dưới",
                    IsDeleteable = false,
                    Geometry = "topdown",
                    Description = "Màu chạy từ trên xuống dưới",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.top2bot
                };
                var down2Up = new VIDDataModel() {
                    Name = "Dưới lên trên",
                    IsDeleteable = false,
                    Geometry = "bottomup",
                    Description = "Màu chạy từ dưới lên trên",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.bot2top
                };
                var linear = new VIDDataModel() {
                    Name = "Tuyến tính",
                    Geometry = "back",
                    Description = "Màu chạy theo thứ tự LED",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.linear
                };
                vidCollection.Add(lef2Right);
                vidCollection.Add(right2Left);
                vidCollection.Add(up2Down);
                vidCollection.Add(down2Up);
                vidCollection.Add(linear);
                foreach (var vid in vidCollection)
                {
                    var json = JsonConvert.SerializeObject(vid);
                    File.WriteAllText(Path.Combine(collectionFolder, vid.Name + ".json"), json);
                }
                //coppy all internal palettes to local
                var config = new ResourceLoaderConfig(nameof(VIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(VIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }

        private void CreateMIDCollectionFolder()
        {
            if (!Directory.Exists(MIDCollectionFolderPath))
            {
                Directory.CreateDirectory(MIDCollectionFolderPath);
                var collectionFolder = Path.Combine(MIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var midCollection = new List<MIDDataModel>();
                var low = new MIDDataModel() {
                    Name = "Low",
                    Description = "Trung bình của các tần số thấp (Bass)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Low
                };
                var middle = new MIDDataModel() {
                    Name = "Mid",
                    Description = "Trung bình của các tần số trung (Mid)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Middle
                };
                var high = new MIDDataModel() {
                    Name = "High",
                    Description = "Trung bình của các tần số cao (Treble)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.High
                };

                var custom = new MIDDataModel() {
                    Name = "Custom",
                    Description = "Chọn Tần số bằng tay",
                    ExecutionType = VIDType.PredefinedID
                };
                midCollection.Add(low);
                midCollection.Add(middle);
                midCollection.Add(high);
                midCollection.Add(custom);
                foreach (var mid in midCollection)
                {
                    var json = JsonConvert.SerializeObject(mid);
                    File.WriteAllText(Path.Combine(collectionFolder, mid.Name + ".json"), json);
                }
                //coppy all internal palettes to local
                var config = new ResourceLoaderConfig(nameof(MIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(MIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }

        private void CreateRequiredFwVersionJson()
        {
            var ABR1p = new DeviceFirmware() {
                Name = "ABR1p.hex",
                Version = "1.0.6",
                TargetHardware = "ABR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ABR1p.hex"
            };
            var ABR2e = new DeviceFirmware() {
                Name = "ABR2e.hex",
                Version = "1.1.0",
                TargetHardware = "ABR2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ABR2e.hex"
            };
            var AER1e = new DeviceFirmware() {
                Name = "AER1e.hex",
                Version = "1.0.4",
                TargetHardware = "AER1e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AER1e.hex"
            };
            var AER2e = new DeviceFirmware() {
                Name = "AER2e.hex",
                Version = "1.0.8",
                TargetHardware = "AER2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AER2e.hex"
            };
            var AFR1g = new DeviceFirmware() {
                Name = "AFR1g.hex",
                Version = "1.0.3",
                TargetHardware = "AFR1g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR1g.hex"
            };
            var AFR2g = new DeviceFirmware() {
                Name = "AFR2g.hex",
                Version = "1.0.5",
                TargetHardware = "AFR2g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR2g.hex"
            };
            var AFR3g = new DeviceFirmware() {
                Name = "AFR3g.hex",
                Version = "1.1.1",
                TargetHardware = "AFR3g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR3g.hex"
            };
            var AHR2g = new DeviceFirmware() {
                Name = "AHR2g.hex",
                Version = "1.0.5",
                TargetHardware = "AHR2g",
                TargetDeviceType = DeviceTypeEnum.AmbinoHUBV3,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AHR2g.hex"
            };
            var ARR1p = new DeviceFirmware() {
                Name = "ARR1p.hex",
                Version = "1.0.2",
                TargetHardware = "ARR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoRainPowPro,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ARR1p.hex"
            };
            var AHR4p = new DeviceFirmware() {
                Name = "AHR4p.uf2",
                Version = "1.0.2",
                TargetHardware = "AHR4p",
                TargetDeviceType = DeviceTypeEnum.AmbinoHUBV3,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AHR4p.uf2"
            };
            var firmwareList = new List<DeviceFirmware>();
            firmwareList.Add(ABR1p);
            firmwareList.Add(ABR2e);
            firmwareList.Add(AER1e);
            firmwareList.Add(AER2e);
            firmwareList.Add(AFR1g);
            firmwareList.Add(AHR2g);
            firmwareList.Add(ARR1p);
            firmwareList.Add(AFR2g);
            firmwareList.Add(AFR3g);
            firmwareList.Add(AHR4p);
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P without PowerLED Support    | CH552P | 32 | 14 | ABR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E Without PowerLED Support    | CH552E | 15 | 17 | ABR1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E(rev2) With PowerLED Support | CH552e | 14 | 15 | ARR2e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552E Without PowerLED Support     | CH552E | 15 | 17 | AER1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P(rev2) With PowerLED Support | CH552P |    |    | ABR2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552P (rev2) With PowerLED Support | CH552P |    |    | AER2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev1                       | CH552G |    |    | AFR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev2                       | CH552G |    |    | AFR2g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev1                        | CH552G |    |    | AHR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino RainPow CH552P rev1                      | CH552P |    |    | ARR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev2                        | CH552G |    |    | AHR2g |
            //+-------------------------------------------------+--------+----+----+-------+
            var requiredFwVersionjson = JsonConvert.SerializeObject(firmwareList, Formatting.Indented);
            File.WriteAllText(JsonFWToolsFWListFileNameAndPath, requiredFwVersionjson);
        }
        private void CreateResourceCollectionFolder()
        {
            if (!Directory.Exists(ResourceFolderPath))
            {
                Directory.CreateDirectory(ResourceFolderPath);
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".png")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(ResourceFolderPath, name));
                }
            }
        }
        private void CreateProfileCollectionFolder()
        {
            if (!Directory.Exists(ProfileCollectionFolderPath))
                Directory.CreateDirectory(ProfileCollectionFolderPath);
            var collectionFolder = Path.Combine(ProfileCollectionFolderPath, "collection");
            Directory.CreateDirectory(collectionFolder);
            var config = new ResourceLoaderConfig(nameof(AppProfile), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(ProfileCollectionFolderPath, "config.json"), configJson);
        }

        public void CreateChasingPatternCollectionFolder()
        {
            if (!Directory.Exists(ChasingPatternsCollectionFolderPath))
            {
                Directory.CreateDirectory(ChasingPatternsCollectionFolderPath);
                var collectionFolder = Path.Combine(ChasingPatternsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".AML")))
                {

                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(ChasingPattern), DeserializeMethodEnum.Files);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(ChasingPatternsCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }

        public void CreateSupportedDevicesCollectionFolder()
        {
            if (!Directory.Exists(SupportedDeviceCollectionFolderPath))
            {
                Directory.CreateDirectory(SupportedDeviceCollectionFolderPath);
            }
        }

        public void CreatePaletteCollectionFolder()
        {
            if (!Directory.Exists(PalettesCollectionFolderPath))
            {
                Directory.CreateDirectory(PalettesCollectionFolderPath);
                var collectionFolder = Path.Combine(PalettesCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".col")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(ColorPalette), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, "config.json"), configJson);
            }
        }
        private async void Run(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await SaveFile();
                        Log.Information("Periodically App Data Saved!");

                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"error when saving devices data : {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a second for updates
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
        public Task SaveFile()
        {
            //if (MainViewViewModel.DeviceHlprs == null)
            //    return Task.FromResult(false);
            //lock (MainViewViewModel.AvailableDevices)
            //{
            //    foreach (var device in MainViewViewModel.DeviceManagerViewModel.AvailableDevices.Items)
            //    {
            //        lock (device)
            //            MainViewViewModel.DeviceHlprs.WriteSingleDeviceInfoJson(device as DeviceSettings);
            //    }
            //}
            //MainViewViewModel.LightingProfileManagerViewModel.SaveData();
            return Task.FromResult(true);
        }
        private static object _syncRoot = new object();
        public void Stop()
        {
            Log.Information("Stop called for DBManager");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }

    }
}