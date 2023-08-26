using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace adrilight.Manager
{
    class UserSettingsManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");


        private void SaveSettings(IGeneralSettings generalSettings)
        {
            var json = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonFileNameAndPath, json);
        }


        public IGeneralSettings LoadIfExists()
        {
            if (!File.Exists(JsonFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonFileNameAndPath);
            try
            {
                var generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                generalSettings.PropertyChanged += (_, __) => SaveSettings(generalSettings);

                HandleAutostart(generalSettings);
                return generalSettings;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public List<DeviceSettings> LoadDeviceIfExists()
        {
            var devices = new List<DeviceSettings>();
            if (!Directory.Exists(DevicesCollectionFolderPath)) return null; // no device has been added

            foreach (var folder in Directory.GetDirectories(DevicesCollectionFolderPath))
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(folder, "config.json"));
                    var device = JsonConvert.DeserializeObject<DeviceSettings>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
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
                    var output = JsonConvert.DeserializeObject<OutputSettings>(outputJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    var slaveDeviceJson = File.ReadAllText(Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "config.json"));
                    var slaveDevice = JsonConvert.DeserializeObject<T>(slaveDeviceJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

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

        private static void HandleAutostart(GeneralSettings settings)
        {
            if (settings.Autostart)
            {
                StartUpManager.AddApplicationToTaskScheduler(settings.StartupDelaySecond);
            }
            else
            {
                StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
            }

        }
        public IGeneralSettings MigrateOrDefault()
        {

            var generalSettings = new GeneralSettings();
            generalSettings.PropertyChanged += (_, __) => SaveSettings(generalSettings);
            var legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight");
            if (!Directory.Exists(legacyPath)) return generalSettings;

            var legacyFiles = Directory.GetFiles(legacyPath, "user.config", SearchOption.AllDirectories);

            var file = legacyFiles
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTimeUtc)
                        .FirstOrDefault();

            HandleAutostart(generalSettings);
            return generalSettings;
        }
    }
}
