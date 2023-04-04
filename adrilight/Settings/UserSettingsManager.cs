
using adrilight.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Xml.Linq;
using System.Xml.XPath;

namespace adrilight
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
           catch(JsonReaderException)
            {
                return null;
            }
        }

        public List<DeviceSettings> LoadDeviceIfExists()
        {
            var devices  = new List<DeviceSettings>();
            if (!Directory.Exists(DevicesCollectionFolderPath)) return null; // no device has been added

            foreach(var folder in Directory.GetDirectories(DevicesCollectionFolderPath))
            {
                var json = File.ReadAllText(Path.Combine(folder,"config.json"));
                var device = JsonConvert.DeserializeObject<DeviceSettings>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                device.AvailableControllers = new List<Settings.IDeviceController>();
                //read slave device info
                //check if this device contains lighting controller
                var lightingoutputDir = Path.Combine(Path.Combine(folder, "LightingOutputs"));
                var pwmoutputDir = Path.Combine(Path.Combine(folder, "PWMOutputs"));
                DeserializeChild<ARGBLEDSlaveDevice>(lightingoutputDir, device,OutputTypeEnum.ARGBLEDOutput);
                DeserializeChild<PWMMotorSlaveDevice>(pwmoutputDir, device, OutputTypeEnum.PWMOutput);


                devices.Add(device);
            }
            return devices;
        }
        private void DeserializeChild<T>(string outputDir, IDeviceSettings device , OutputTypeEnum outputType)
        {
            if (Directory.Exists(outputDir))
            {
                //add controller to this device

                var controller = new DeviceController();
                switch(outputType)
                {
                    case (OutputTypeEnum.PWMOutput):
                        controller.Geometry = "fanspeed";
                        controller.Type = ControllerTypeEnum.PWMController;
                        break;
                    case (OutputTypeEnum.ARGBLEDOutput):
                        controller.Geometry = "brightness";
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


                    if (!File.Exists((slaveDevice as ISlaveDevice).Thumbnail))
                    {
                        (slaveDevice as ISlaveDevice).Thumbnail = Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "thumbnail.png");
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

    

        //private void ReadAndApply<T>(XDocument xdoc, IUserSettings settings, string settingName, Expression<Func<IUserSettings, T>> targetProperty)
        //{
        //    var content = xdoc.XPathSelectElement($"//setting[@name='{settingName}']/value");
        //    if (content == null) return;


        //    var text = content.Value;
        //    var propertyExpression = (MemberExpression)targetProperty.Body;
        //    var member = (PropertyInfo)propertyExpression.Member;

        //    object targetValue;

        //    if (typeof(T) == typeof(int))
        //    {
        //        //int
        //        targetValue = Convert.ToInt32(text);
        //    }
        //    else if (typeof(T) == typeof(byte))
        //    {
        //        //byte
        //        targetValue = Convert.ToByte(text);
        //    }
        //    else if (typeof(T) == typeof(bool))
        //    {
        //        //bool
        //        targetValue = Convert.ToBoolean(text);
        //    }
        //    else if (typeof(T) == typeof(string))
        //    {
        //        //string
        //        targetValue = text;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException($"converting to {typeof(T).FullName} is not implemented");
        //    }

        //    member.SetValue(settings, targetValue);
        //}
    }
}
