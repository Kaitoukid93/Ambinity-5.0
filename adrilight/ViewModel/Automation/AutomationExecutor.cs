using adrilight.Manager;
using adrilight_shared.Enums;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using System;
using System.Linq;

namespace adrilight.ViewModel.Automation
{
    public class AutomationExecutor
    {
        public AutomationExecutor(DeviceCollectionViewModel devicecollection, LightingProfileManager lightingProfileManager)
        {
            _deviceCollection = devicecollection;
            _lightingProfileManager = lightingProfileManager;
        }
        private DeviceCollectionViewModel _deviceCollection;
        private LightingProfileManager _lightingProfileManager;
        public void Execute(AutomationSettings automation)
        {
            if (automation.Actions == null)
                return;
            foreach (var action in automation.Actions)
            {

                var targetDevice = _deviceCollection.AvailableDevices.Items.Where(x => (x as DeviceSettings).DeviceUID == action.TargetDeviceUID).FirstOrDefault() as DeviceSettings;
                if (targetDevice == null)
                {
                    continue;
                }
                if (!targetDevice.IsEnabled)
                    continue;
                switch (action.ActionType.Type)
                {

                    case "Activate":
                        var profileUID = (string)action.ActionParameter.Value;
                        if (profileUID != null)
                        {
                           _lightingProfileManager.ActivateProfile(profileUID, targetDevice);
                        }
                        break;

                    case "Increase":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessUp(10);
                                break;

                            case "speed":
                                targetDevice.SpeedUp(10);
                                break;
                        }
                        break;

                    case "Decrease":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessDown(10);
                                break;

                            case "speed":
                                targetDevice.SpeedDown(10);
                                break;
                        }
                        break;

                    case "Off":
                        // just turn off all leds for now
                        targetDevice.TurnOffLED();
                        break;

                    case "On":
                        //targetDevice.IsEnabled = true;
                        targetDevice.TurnOnLED();
                        break;
                    case "On/Off":
                        targetDevice.ToggleOnOffLED();
                        break;
                    case "Change":
                        //just change solid color and activate static mode
                        targetDevice.TurnOnLED();
                        switch (action.ActionParameter.Type)
                        {
                            case "color":
                                targetDevice.SetStaticColor(action.ActionParameter.Value as ColorCard);
                                break;
                            case "mode":
                                LightingModeEnum value = (LightingModeEnum)Enum.Parse(typeof(LightingModeEnum), action.ActionParameter.Value.ToString());
                                targetDevice.SetModeByEnumValue(value);
                                break;
                        }

                        break;
                }
            }
        }
    }
}
