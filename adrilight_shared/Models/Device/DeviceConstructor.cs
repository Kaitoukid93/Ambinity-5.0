using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Settings;
using LiveCharts.Defaults;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LEDSetupHelpers = adrilight_shared.Helpers.LEDSetupHelpers;

namespace adrilight_shared.Models.Device
{
    //this construct new device out of thin air or download existed from server
    public class DeviceConstructor
    {
        public DeviceConstructor()
        {
            _ledSetupHlprs = new LEDSetupHelpers();
            _controlModeHlprs = new ControlModeHelpers();
        }
        //construct new device from infomation 
        private LEDSetupHelpers _ledSetupHlprs;
        private ControlModeHelpers _controlModeHlprs;

       
        public DeviceSettings ConstructNewDevice(string name, string deviceSerial, string fwVersion, string hwVersion, int hwLightingVersion, string outputPort)
        {
            var newDevice = new DeviceSettings();
            newDevice.DeviceName = name;
            newDevice.HWL_version = hwLightingVersion;
            switch (newDevice.DeviceName)
            {
                case "Ambino Basic":// General Ambino Basic USB Device
                                    //newDevice = availableDefaultDevice.AmbinoBasic24Inch;

                    newDevice = DefaultCreatedAmbinoDevice(
                        new DeviceType(DeviceTypeEnum.AmbinoBasic),
                        newDevice.DeviceName,
                        outputPort,
                        false,
                        true,
                        1);
                    newDevice.DashboardWidth = 230;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino EDGE":// General Ambino Edge USB Device
                    newDevice = DefaultCreatedAmbinoDevice(
                     new DeviceType(DeviceTypeEnum.AmbinoEDGE),
                     newDevice.DeviceName,
                     outputPort,
                     false,
                     true,
                     1);
                    newDevice.DashboardWidth = 230;
                    newDevice.DashboardHeight = 270;

                    break;
                case "Ambino FanHub":
                    newDevice = DefaultCreatedAmbinoDevice(
                       new DeviceType(DeviceTypeEnum.AmbinoFanHub),
                    newDevice.DeviceName,
                       outputPort,
                       true,
                       true,
                       10);
                    newDevice.DashboardWidth = 472;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino HubV2":
                    newDevice = DefaultCreatedAmbinoDevice(
                     new DeviceType(DeviceTypeEnum.AmbinoHUBV2),
                  newDevice.DeviceName,
                     outputPort,
                     false,
                     true,
                     7);
                    newDevice.DashboardWidth = 320;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino HubV3":
                    newDevice = DefaultCreatedAmbinoDevice(
                      new DeviceType(DeviceTypeEnum.AmbinoHUBV3),
                   newDevice.DeviceName,
                      outputPort,
                      false,
                      true,
                      4);
                    newDevice.DashboardWidth = 320;
                    newDevice.DashboardHeight = 270;
                    break;
            }

            newDevice.DeviceSerial = deviceSerial;
            newDevice.FirmwareVersion = fwVersion;
            newDevice.HardwareVersion = hwVersion;
            newDevice.UpdateChildSize();
            return newDevice;
        }

       
        public DeviceSettings DefaultCreateOpenRGBDevice(OpenRGB.NET.Enums.DeviceType type, string deviceName, string outputPort, string serial, string uid)
        {
            var newDevice = new DeviceSettings();
            newDevice.DeviceType = GetTypeFromOpenRGB(type);
            newDevice.DeviceName = deviceName;
            newDevice.OutputPort = deviceName + outputPort;
            newDevice.DeviceDescription = "Device Supported Throught Open RGB Client";
            newDevice.DeviceSerial = serial;
            newDevice.DeviceUID = uid;
            newDevice.IsSizeNeedUserDefine = true;
            var lightingController = DefaultCreatedDeviceController(ControllerTypeEnum.LightingController, "Lighting", "brightness");
            var output = DefaultCreatedOutput(OutputTypeEnum.ARGBLEDOutput, 0, "genericConnector", "Generic ARGB LED Output") as OutputSettings;
            output.Left = 0;
            output.Top = 0;
            output.Width = 500;
            output.Height = 500;
            lightingController.Outputs.Add(output);
            newDevice.AvailableControllers = new List<IDeviceController>();
            newDevice.AvailableControllers.Add(lightingController);
            return newDevice;
        }
        private DeviceType GetTypeFromOpenRGB(OpenRGB.NET.Enums.DeviceType type)
        {
            var returnType = new DeviceType();

            switch (type)
            {
                case OpenRGB.NET.Enums.DeviceType.Motherboard:
                    returnType.Type = DeviceTypeEnum.Motherboard;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Dram:
                    returnType.Type = DeviceTypeEnum.Dram;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Cooler:
                    returnType.Type = DeviceTypeEnum.Cooler;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Gpu:
                    returnType.Type = DeviceTypeEnum.Gpu;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Mouse:
                    returnType.Type = DeviceTypeEnum.Mouse;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Mousemat:
                    returnType.Type = DeviceTypeEnum.Mousemat;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Keyboard:
                    returnType.Type = DeviceTypeEnum.Keyboard;
                    break;
                case OpenRGB.NET.Enums.DeviceType.HeadsetStand:
                    returnType.Type = DeviceTypeEnum.HeadsetStand;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Headset:
                    returnType.Type = DeviceTypeEnum.Headset;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Light:
                    returnType.Type = DeviceTypeEnum.Light;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Unknown:
                    returnType.Type = DeviceTypeEnum.Unknown;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Ledstrip:
                    returnType.Type = DeviceTypeEnum.Ledstrip;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Gamepad:
                    returnType.Type = DeviceTypeEnum.Gamepad;
                    break;
                case OpenRGB.NET.Enums.DeviceType.Virtual:
                    returnType.Type = DeviceTypeEnum.Virtual;
                    break;
            }
            returnType.ConnectionTypeEnum = DeviceConnectionTypeEnum.OpenRGB;
            return returnType;
        }
        public DeviceSettings DefaultCreatedAmbinoDevice(
            DeviceType type,
            string deviceName,
            string outputPort,
            bool hasFancontrol,
            bool hasLightingControl,
            int outputCount)
        {
            var newDevice = new DeviceSettings();
            newDevice.DeviceName = deviceName;
            newDevice.DeviceUID = Guid.NewGuid().ToString();
            newDevice.DeviceType = type;
            newDevice.OutputPort = outputPort;
            newDevice.IsSizeNeedUserDefine = true;
            newDevice.AvailableControllers = new List<IDeviceController>();
            if (hasLightingControl)
            {
                var lightingController = DefaultCreatedDeviceController(ControllerTypeEnum.LightingController, "Lighting", "brightness");

                foreach (var output in GetOutputMap(outputCount))
                {
                    lightingController.Outputs.Add(output);

                }
                newDevice.AvailableControllers.Add(lightingController);

            }
            if (hasFancontrol)
            {
                var pwmController = DefaultCreatedDeviceController(ControllerTypeEnum.PWMController, "Fan", "fanSpeedController");

                for (int i = 0; i < outputCount; i++)
                {
                    pwmController.Outputs.Add(DefaultCreatedOutput(OutputTypeEnum.PWMOutput, i, "genericConnector", "Generic PWM Fan Output"));
                }
                newDevice.AvailableControllers.Add(pwmController);
            }

            // newDevice.UpdateChildSize();
            return newDevice;

        }
        public DeviceSettings DefaultCreatedGenericDevice(
            DeviceType type,
            string deviceName,
            string outputPort,
            bool hasFancontrol,
            bool hasLightingControl,
            int outputCount)
        {
            var newDevice = new DeviceSettings();
            newDevice.DeviceName = deviceName;
            newDevice.DeviceUID = Guid.NewGuid().ToString();
            newDevice.DeviceType = type;
            newDevice.OutputPort = outputPort;
            newDevice.IsSizeNeedUserDefine = true;
            newDevice.AvailableControllers = new List<IDeviceController>();
            if (hasLightingControl)
            {
                var lightingController = DefaultCreatedDeviceController(ControllerTypeEnum.LightingController, "Lighting", "brightness");

                foreach (var output in GetOutputMap(outputCount))
                {
                    lightingController.Outputs.Add(output);

                }
                newDevice.AvailableControllers.Add(lightingController);

            }
            if (hasFancontrol)
            {
                var pwmController = DefaultCreatedDeviceController(ControllerTypeEnum.PWMController, "Fan", "fanSpeedController");

                for (int i = 0; i < outputCount; i++)
                {
                    pwmController.Outputs.Add(DefaultCreatedOutput(OutputTypeEnum.PWMOutput, i, "genericConnector", "Generic PWM Fan Output"));
                }
                newDevice.AvailableControllers.Add(pwmController);
            }

            // newDevice.UpdateChildSize();
            return newDevice;

        }
        public ISlaveDevice DefaultCreatedSlaveDevice(string name, SlaveDeviceTypeEnum type, ZoneData[] zoneData)
        {
            ISlaveDevice newSlaveDevice = new ARGBLEDSlaveDevice();
            switch (type)
            {
                case SlaveDeviceTypeEnum.LEDStrip: // this type always has 4 frame, not implement for now
                    newSlaveDevice = new ARGBLEDSlaveDevice();
                    newSlaveDevice.Name = name;
                    newSlaveDevice.ControlableZones = new ObservableCollection<IControlZone>();
                    for (int i = 0; i < zoneData.Length; i++)
                    {
                        //build a default led setrip with 20 leds each
                        var top = 0;
                        var indexOffset = 0;
                        if (i >= 1)
                        {
                            top = i * zoneData[i - 1].Height;
                            var totalLEDCount = 0;
                            for (int j = 0; j < i; j++)
                            {
                                totalLEDCount += zoneData[j].NumLEDX * zoneData[j].NumLEDY;
                            }
                            indexOffset = totalLEDCount;
                        }

                        newSlaveDevice.ControlableZones.Add(_ledSetupHlprs.BuildLEDSetup(zoneData[i].Name, 0, top, zoneData[i].NumLEDX, zoneData[i].NumLEDY, zoneData[i].Width, zoneData[i].Height, indexOffset));
                    }

                    break;
                case SlaveDeviceTypeEnum.FanMotor: // this type always has 4 frame, not implement for now
                    newSlaveDevice = new PWMMotorSlaveDevice();
                    newSlaveDevice.Name = "GenericFan";
                    newSlaveDevice.ControlableZones = new ObservableCollection<IControlZone>(); // support single zone speedcontrol per output
                    newSlaveDevice.ControlableZones.Add(new FanMotor()
                    {
                        Name = "GenericFan",
                        FanSpiningAnimationPath = "I:\\Ambinity\\AmbinityWPF\\adrilight\\adrilight\\Animation\\splashLoading.json",
                        Width = 300,
                        Height = 300,
                        ZoneUID = Guid.NewGuid().ToString(),
                        LineValues = new ChartValues<ObservableValue>
                        {
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80)},
                        AvailableControlMode = new List<IControlMode>() { _controlModeHlprs.FanSpeedAuto, _controlModeHlprs.FanSpeedManual }
                    });
                    break;
            }
            return newSlaveDevice;

        }
        public IOutputSettings DefaultCreatedOutput(OutputTypeEnum type, int id, string geometry, string name)
        {
            var newOuput = new OutputSettings();
            //property for outputmaping
            newOuput.OutputName = name;
            //newOuput.Rectangle = new System.Drawing.Rectangle(25, 36, 78, 22);
            newOuput.OutputType = type;
            newOuput.OutputID = id;
            newOuput.Geometry = geometry;
            newOuput.Width = 80;
            newOuput.Height = 80;
            switch (type)
            {
                case OutputTypeEnum.ARGBLEDOutput:
                    newOuput.SlaveDevice = DefaultCreatedSlaveDevice("Generic LED Strip", SlaveDeviceTypeEnum.LEDStrip, new ZoneData[1] { new ZoneData("Zone", 20, 1, 1200, 60) });
                    newOuput.SlaveDevice.ParrentID = id;
                    break;
                case OutputTypeEnum.PWMOutput:
                    newOuput.SlaveDevice = DefaultCreatedSlaveDevice("Generic PWM Fan", SlaveDeviceTypeEnum.FanMotor, null);
                    newOuput.SlaveDevice.ParrentID = id;
                    break;
            }

            return newOuput;
        }
        public IDeviceController DefaultCreatedDeviceController(ControllerTypeEnum type, string name, string geometry)
        {
            var newController = new DeviceController();
            newController.Name = name;
            newController.Type = type;
            newController.Geometry = geometry;
            return newController;

        }
        //this section return output with exact position to use in output maping view
        public List<IOutputSettings> GetOutputMap(int count)
        {
            var outputList = new List<IOutputSettings>();
            for (int i = 0; i < count; i++)
            {
                var output = DefaultCreatedOutput(OutputTypeEnum.ARGBLEDOutput, i, "genericConnector", "Generic ARGB LED Output") as OutputSettings;
                outputList.Add(output);
            }
            return outputList;
        }

    }
}
