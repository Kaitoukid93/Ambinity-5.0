using adrilight.Manager;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Lighting;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace adrilight.ViewModel.Automation
{
    public class AutomationDialogViewModel : ViewModelBase
    {
        public AutomationDialogViewModel(DeviceManager deviceManager, LightingProfileManager profileManager)
        {
            _devicemanager = deviceManager;
            _profilemanager = profileManager;
        }
        // call every dialog open to get available params
        private DeviceManager _devicemanager;
        private LightingProfileManager _profilemanager;
        private ActionSettings _action;
        public bool Init(string header, string content, ActionSettings action)
        {
            Header = header;
            Content = content;
            Values = new ObservableCollection<object>();
            _action = action;
            // add brightness and(or) speed
            //prevent dialog open when no device selected
            if (action.TargetDeviceUID == null)
                return false;
            var targetDevice = _devicemanager.AvailableDevices.Where(d => (d as DeviceSettings).DeviceUID == action.TargetDeviceUID).FirstOrDefault() as DeviceSettings;
            if (targetDevice == null)
                return false;
            if (Header == "Parameters")
            {
                switch (action.ActionType.Type)
                {
                    case "Activate":
                        //get available profile
                        foreach (var profile in _profilemanager.AvailableProfiles)
                        {
                            if (profile != null)
                                Values.Add(new ActionParameter {
                                    Geometry = "brightness",
                                    Name = profile.Name,
                                    Type = "profile",
                                    Value = (profile as LightingProfile).ProfileUID
                                });
                        }
                        break;
                    case "Increase":
                        Values.Add(GetAutoMationParam("brightness", "up"));
                        Values.Add(GetAutoMationParam("speed", "up"));
                        break;
                    case "Decrease":
                        // add brightness and(or) speed
                        Values.Add(GetAutoMationParam("brightness", "down"));
                        Values.Add(GetAutoMationParam("speed", "down"));
                        break;
                    case "On":
                        // add all leds or part of
                        Values.Add(GetAutoMationParam("state", "on"));
                        break;
                    case "Off":
                        // add all leds or part of
                        Values.Add(GetAutoMationParam("state", "off"));
                        break;
                    case "On/Off":
                        // add all leds or part of
                        Values.Add(GetAutoMationParam("state", "off"));
                        break;
                    case "Change":
                        // add colors and modes
                        Values.Add(GetAutoMationParam("color", "#ffff53c9"));

                        Values.Add(GetAutoMationParam("mode", targetDevice.AvailableLightingDevices[0].ControlableZones[0].CurrentActiveControlMode.Name));
                        break;

                }
            }
            else if (Header == "Devices")
            {
                //add available devices
                foreach (var device in _devicemanager.AvailableDevices)
                    Values.Add(device);
            }
            else if (Header == "Values")
            {
                //add available values
                switch (action.ActionParameter.Type)
                {
                    case "color":
                        (targetDevice.AvailableLightingDevices[0].ControlableZones[0] as LEDSetup).GetStaticColorDataSource().ForEach(c => Values.Add(c));
                        break;

                    case "mode":
                        targetDevice.AvailableLightingDevices[0].ControlableZones[0].AvailableControlMode.ForEach(m => Values.Add((m as LightingMode).BasedOn));
                        break;
                }
            }
            return true;
        }
        private ActionParameter GetAutoMationParam(string paramType, string value)
        {
            var returnParam = new ActionParameter();
            switch (paramType)
            {
                case "brightness":
                    returnParam.Name = adrilight_shared.Properties.Resources.GetAutoMationParam_Brightness;
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "brightness";
                    returnParam.Value = value;
                    break;

                case "state":
                    returnParam.Name = adrilight_shared.Properties.Resources.GetAutoMationParam_AllLEDs;
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "state";
                    returnParam.Value = value;
                    break;

                case "color":
                    returnParam.Name = adrilight_shared.Properties.Resources.GetAutoMationParam_Color;
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "color";
                    returnParam.Value = Color.FromRgb(255, 255, 0);
                    break;

                case "mode":
                    returnParam.Name = adrilight_shared.Properties.Resources.GetAutoMationParam_Mode;
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "mode";
                    returnParam.Value = value;
                    break;

                case "speed":
                    returnParam.Name = adrilight_shared.Properties.Resources.GetAutoMationParam_FanSpeed;
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "speed";
                    returnParam.Value = value;
                    break;
            }
            return returnParam;
        }
        public string Geometry { get; set; }
        public string Content { get; set; }
        public string Header { get; set; }
        public ObservableCollection<object> Values { get; set; }
        public object Value { get; set; }
    }
}

