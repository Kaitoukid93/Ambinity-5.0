using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Settings;
using Newtonsoft.Json;
using NonInvasiveKeyboardHookLibrary;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight_shared.Models.Automation
{


    public class AutomationManager
    {
        public event Action<AutomationSettings> HotKeyPressed;
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
        #region Construct
        public AutomationManager(KeyboardHookManagerSingleton keyboardHookManager)
        {
            _keyboardHookManager = keyboardHookManager;
            _resourceHlprs = new ResourceHelpers();
        }
        #endregion
        #region Properties
        private KeyboardHookManagerSingleton _keyboardHookManager;
        private ResourceHelpers _resourceHlprs;
        #endregion
        #region Methods
        //this will raise an event when registered hotkey combo was pressed
        public void Start()
        {
            _keyboardHookManager.Instance.Start();
        }
        public void Register(AutomationSettings automation)
        {
            //_identifiers = new List<Guid?>();
            if (!automation.IsEnabled)
                return;
            if (automation.Condition is not HotkeyTriggerCondition)
                return;
            var condition = automation.Condition as HotkeyTriggerCondition;
            if (condition == null)
                return;
            var modifierkeys = new List<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
            if (condition.Modifiers != null)
            {
                foreach (var modifier in condition.Modifiers)
                {
                    modifierkeys.Add(modifier);
                }
            }

            try
            {
                switch (modifierkeys.Count)
                {
                    case 0:
                        _keyboardHookManager.Instance.RegisterHotkey(condition.StandardKey.KeyCode, () =>
                        {
                            HotKeyPressed?.Invoke(automation);
                            Log.Information(automation.Name + " excuted");
                        });
                        //_identifiers.Add(_identifier);
                        break;

                    case 1:
                        _keyboardHookManager.Instance.RegisterHotkey(modifierkeys.First(), condition.StandardKey.KeyCode, () =>
                        {
                            HotKeyPressed?.Invoke(automation);
                            Log.Information(automation.Name + " excuted");
                        });
                        //_identifiers.Add(_identifier);
                        break;

                    default:
                        _keyboardHookManager.Instance.RegisterHotkey(modifierkeys.ToArray(), condition.StandardKey.KeyCode, () =>
                        {
                            HotKeyPressed?.Invoke(automation);
                            Log.Information(automation.Name + " excuted");
                        });
                        //_identifiers.Add(_identifier);
                        break;
                }
            }
            catch (NonInvasiveKeyboardHookLibrary.HotkeyAlreadyRegisteredException ex)
            {
                HandyControl.Controls.MessageBox.Show(automation.Name + " Hotkey is being used by another automation!!!", "HotKey Already Registered", MessageBoxButton.OK, MessageBoxImage.Error);
                automation.Condition = null;
            }
            catch (Exception ex)
            {
                //empty catch
            }

        }
        public void Unregister()
        {
            _keyboardHookManager.Instance.UnregisterAll();
        }
        public void ExecuteAutomationActions(ObservableCollection<ActionSettings> actions, IDeviceSettings targetDevice)
        {
            if (actions == null)
                return;
            foreach (var action in actions)
            {

                targetDevice = AvailableDevices.Where(x => x.DeviceUID == action.TargetDeviceUID).FirstOrDefault();
                if (targetDevice == null)
                {
                    WriteAutomationCollectionJson();
                    continue;
                }
                if (action.ActionType.Type == "Activate") // this type of action require no target 
                    goto execute;

                if (!targetDevice.IsEnabled)
                    return;
                execute:
                switch (action.ActionType.Type)
                {

                    case "Activate":
                        var destinationProfile = LightingProfileManagerViewModel.AvailableLightingProfiles.Items.Where(x => (x as LightingProfile).ProfileUID == (string)action.ActionParameter.Value).FirstOrDefault() as LightingProfile;
                        if (destinationProfile != null)
                        {
                            targetDevice.TurnOnLED();
                            // LightingPlayer.Play(destinationProfile, targetDevice);
                            // ActivateCurrentLightingProfileForSpecificDevice(destinationProfile, targetDevice);
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
        private void LoadAvailableAutomations()
        {
            AvailableAutomations = new ObservableCollection<AutomationSettings>();
            ShutdownAutomations = new ObservableCollection<AutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
                if (automation.Condition != null && automation.Condition is SystemEventTriggerCondition)
                {
                    var condition = automation.Condition as SystemEventTriggerCondition;
                    if (condition.Event == SystemEventEnum.Shutdown)
                    {
                        ShutdownAutomations.Add(automation);
                    }
                }
            }
            WriteAutomationCollectionJson();
        }
        public List<AutomationSettings> LoadAutomationIfExist()
        {
            var loadedAutomations = new List<AutomationSettings>();
            if (!File.Exists(JsonAutomationFileNameAndPath))
            {
                _resourceHlprs.CopyResource("adrilight_shared.Resources.Automations.DefaultAutomations.json", JsonAutomationFileNameAndPath);
            }
            if (File.Exists(JsonAutomationFileNameAndPath))
            {
                var json = File.ReadAllText(JsonAutomationFileNameAndPath);

                var existedAutomation = JsonConvert.DeserializeObject<List<AutomationSettings>>(json);
                foreach (var automation in existedAutomation)
                {
                    loadedAutomations.Add(automation);
                }
            }

            return loadedAutomations;
        }
        public void CreateDeviceShutdownAction(IDeviceSettings device)
        {
            var shutdownAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.Shutdown).FirstOrDefault();
            if (shutdownAutomation == null)
                return;
            if (shutdownAutomation.Actions.Count == 0)
                return;
            if (shutdownAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceShutdownAction = ObjectHelpers.Clone<ActionSettings>(shutdownAutomation.Actions[0]);
            shutdownAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceShutdownAction.TargetDeviceName = device.DeviceName;
            newDeviceShutdownAction.TargetDeviceUID = device.DeviceUID;
            shutdownAutomation.Actions.Add(newDeviceShutdownAction);
            //lock the automation
            shutdownAutomation.IsLocked = true;
        }
        public void CreateDeviceMonitorSleepAction(IDeviceSettings device)
        {
            var monitorSleepAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorSleep).FirstOrDefault();
            if (monitorSleepAutomation == null)
                return;
            if (monitorSleepAutomation.Actions.Count == 0)
                return;
            if (monitorSleepAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceMonitorSleepAction = ObjectHelpers.Clone<ActionSettings>(monitorSleepAutomation.Actions[0]);
            monitorSleepAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceMonitorSleepAction.TargetDeviceName = device.DeviceName;
            newDeviceMonitorSleepAction.TargetDeviceUID = device.DeviceUID;
            monitorSleepAutomation.Actions.Add(newDeviceMonitorSleepAction);
            //lock the automation
            monitorSleepAutomation.IsLocked = true;
        }
        public void CreateDeviceMonitorWakeupAction(IDeviceSettings device)
        {
            var monitorWakeupAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorWakeup).FirstOrDefault();
            if (monitorWakeupAutomation == null)
                return;
            if (monitorWakeupAutomation.Actions.Count == 0)
                return;
            if (monitorWakeupAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceMonitorWakeupAction = ObjectHelpers.Clone<ActionSettings>(monitorWakeupAutomation.Actions[0]);
            monitorWakeupAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceMonitorWakeupAction.TargetDeviceName = device.DeviceName;
            newDeviceMonitorWakeupAction.TargetDeviceUID = device.DeviceUID;
            monitorWakeupAutomation.Actions.Add(newDeviceMonitorWakeupAction);
            //lock the automation
            monitorWakeupAutomation.IsLocked = true;
        }
        public List<ITriggerCondition> GetAvailableCondition()
        {
            var conditions = new List<ITriggerCondition>();
            var hotkeyCondition = new HotkeyTriggerCondition(adrilight_shared.Properties.Resources.TriggerCondition_Hotkey_name,
                adrilight_shared.Properties.Resources.TriggerCondition_Hotkey_info,
                null,
                null);
            var systemShutdownCondition = new SystemEventTriggerCondition(adrilight_shared.Properties.Resources.TriggerCondition_TurnOff_name,
                adrilight_shared.Properties.Resources.TriggerCondition_TurnOff_info,
                SystemEventEnum.Shutdown);
            var systemMonitorSleepCondition = new SystemEventTriggerCondition(adrilight_shared.Properties.Resources.TriggerCondition_ScreenOff_name,
                adrilight_shared.Properties.Resources.TriggerCondition_ScreenOff_info,
                SystemEventEnum.MonitorSleep);
            var systemMonitorWakeupCondition = new SystemEventTriggerCondition(adrilight_shared.Properties.Resources.TriggerCondition_ScreenOn_name,
                adrilight_shared.Properties.Resources.TriggerCondition_ScreenOn_info,
                SystemEventEnum.MonitorWakeup);
            conditions.Add(hotkeyCondition);
            conditions.Add(systemShutdownCondition);
            conditions.Add(systemMonitorSleepCondition);
            conditions.Add(systemMonitorWakeupCondition);
            return conditions;
        }
        public List<ActionType> GetAvailableActions()
        {
            var actions = new List<ActionType>();
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Activate_name,
                Description = "Kích hoạt một Profile có sẵn",
                Geometry = "apply",
                Type = "Activate",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Activate_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Increase_name,
                Description = "Tăng giá trị của một thuộc tính",
                Geometry = "apply",
                Type = "Increase",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Decrease_name,
                Description = "Giảm giá trị của một thuộc tính",
                Geometry = "apply",
                Type = "Decrease",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_TurnOn_name,
                Description = "Bật một tính năng",
                Geometry = "apply",
                Type = "On",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_TurnOff_name,
                Description = "Tắt một tính năng",
                Geometry = "apply",
                Type = "Off",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Turnonthenoff_name,
                Description = "Chuyển đổi trạng thái Bật Tắt",
                Geometry = "apply",
                Type = "On/Off",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            });
            actions.Add(new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Switchto_name,
                Description = "Chuyển đổi đồng thời kích hoạt một tính năng",
                Geometry = "apply",
                Type = "Change",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                ToResultText = "thành",
                IsValueDisplayed = true,
                IsTargetDeviceDisplayed = true
            });
            return actions;
        }
        #endregion


    }
}
