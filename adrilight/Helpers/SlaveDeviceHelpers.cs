using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Helpers
{
    public class SlaveDeviceHelpers
    {

        private LEDSetupHelpers LEDSetupHlprs { get; set; } = new LEDSetupHelpers();
        private  ControlModeHelpers CtrlHlprs { get; set; } = new ControlModeHelpers();
        public IDeviceSettings DefaultCreatedDevice(
            DeviceTypeEnum type,
            string deviceName,
            string outputPort,
            bool hasFancontrol,
            bool hasLightingControl,
            int outputCount)
        {
            var newDevice = new DeviceSettings();
            newDevice.DeviceName = "Ambino Basic";
            newDevice.DeviceUID = Guid.NewGuid().ToString();
            newDevice.TypeEnum = type;
            newDevice.DeviceConnectionType = "wired";
            newDevice.OutputPort = outputPort;
            newDevice.IsSizeNeedUserDefine = true;
            newDevice.AvailableControllers = new List<IDeviceController>();
            if (hasLightingControl)
            {
                var lightingController = DefaultCreatedDeviceController(ControllerTypeEnum.LightingController, "LED Controller", "brightness");
                
                for (int i = 0; i < outputCount; i++)
                {
                    lightingController.Outputs.Add(DefaultCreatedOutput(OutputTypeEnum.ARGBLEDOutput, i, "genericConnector", "Generic ARGB LED Output"));
                }
                newDevice.AvailableControllers.Add(lightingController);

            }
            if (hasFancontrol)
            {
                var pwmController = DefaultCreatedDeviceController(ControllerTypeEnum.PWMController, "PWM Controller", "fan");

                for (int i = 0; i < outputCount; i++)
                {
                    pwmController.Outputs.Add(DefaultCreatedOutput(OutputTypeEnum.PWMOutput, i, "genericConnector", "Generic PWM Fan Output"));
                }
                newDevice.AvailableControllers.Add(pwmController);
            }

            newDevice.UpdateChildSize();
            return newDevice;

        }
        public ISlaveDevice DefaultCreatedSlaveDevice(string name, SlaveDeviceTypeEnum type, int zoneCount)
        {
            ISlaveDevice newSlaveDevice = new ARGBLEDSlaveDevice();
            switch (type)
            {
                case SlaveDeviceTypeEnum.LEDStrip: // this type always has 4 frame, not implement for now
                    newSlaveDevice = new ARGBLEDSlaveDevice();
                    newSlaveDevice.Name = name;
                    newSlaveDevice.ControlableZones = new ObservableCollection<IControlZone>();
                    for (int i = 0; i < zoneCount; i++)
                    {
                        //build a default led setrip with 20 leds each
                        newSlaveDevice.ControlableZones.Add(LEDSetupHlprs.BuildLEDSetup(20, 1, "ARGB LEDStrip", 1200.0, 60.0));
                    }

                    break;
                case SlaveDeviceTypeEnum.FanMotor: // this type always has 4 frame, not implement for now
                    newSlaveDevice = new PWMMotorSlaveDevice();
                    newSlaveDevice.Name = "GenericFan";
                    newSlaveDevice.ControlableZones = new ObservableCollection<IControlZone>(); // support single zone speedcontrol per output
                    newSlaveDevice.ControlableZones.Add(new FanMotor() {
                        Name = "GenericFan",
                        FanSpiningAnimationPath = "I:\\Ambinity\\AmbinityWPF\\adrilight\\adrilight\\Animation\\splashLoading.json",
                        Width = 300,
                        Height = 300,
                        AvailableControlMode = new List<IControlMode>() {CtrlHlprs.FanSpeedAuto, CtrlHlprs.FanSpeedManual }
                    });
                    break;
            }
            return newSlaveDevice;

        }
        public IOutputSettings DefaultCreatedOutput(OutputTypeEnum type, int id, string geometry, string name)
        {
            var newOuput = new OutputSettings();
            newOuput.OutputName = name;
            newOuput.OutputType = type;
            newOuput.OutputID = id;
            newOuput.Geometry = geometry;
            switch (type)
            {
                case OutputTypeEnum.ARGBLEDOutput:
                    newOuput.SlaveDevice = DefaultCreatedSlaveDevice("Generic LED Strip", SlaveDeviceTypeEnum.LEDStrip, 1);
                    break;
                case OutputTypeEnum.PWMOutput:
                    newOuput.SlaveDevice = DefaultCreatedSlaveDevice("Generic PWM Fan", SlaveDeviceTypeEnum.FanMotor, 1);
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
    }
      
}
