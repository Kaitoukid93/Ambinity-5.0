using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Automation
{
    public class AutomationDBManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
        public AutomationDBManager()
        {
            _resourceHlprs = new ResourceHelpers();
        }

        private ResourceHelpers _resourceHlprs;

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
        public ActionSettings CreateDeviceShutdownAction(IDeviceSettings device)
        {
            var newShutdownAction = new ActionSettings();
            newShutdownAction.ActionType = TurnOffActionType();
            newShutdownAction.TargetDeviceName = device.DeviceName;
            newShutdownAction.TargetDeviceUID = device.DeviceUID;
            return newShutdownAction;
        }
        public ActionSettings CreateDeviceMonitorSleepAction(IDeviceSettings device)
        {
            var newMonitorSleepAction = new ActionSettings();
            newMonitorSleepAction.ActionType = TurnOffActionType();
            newMonitorSleepAction.TargetDeviceName = device.DeviceName;
            newMonitorSleepAction.TargetDeviceUID = device.DeviceUID;
            return newMonitorSleepAction;
        }
        public ActionSettings CreateDeviceMonitorWakeupAction(IDeviceSettings device)
        {
            var newMonitorWakeupAction = new ActionSettings();
            newMonitorWakeupAction.ActionType = TurnOnActionType();
            newMonitorWakeupAction.TargetDeviceName = device.DeviceName;
            newMonitorWakeupAction.TargetDeviceUID = device.DeviceUID;
            return newMonitorWakeupAction;
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
        private ActionType ActivateActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Activate_name,
                Description = "Kích hoạt một Profile có sẵn",
                Geometry = "apply",
                Type = "Activate",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Activate_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType IncreaseActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Increase_name,
                Description = "Tăng giá trị của một thuộc tính",
                Geometry = "Increase",
                Type = "Increase",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType DecreaseActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Decrease_name,
                Description = "Giảm giá trị của một thuộc tính",
                Geometry = "Decrease",
                Type = "Decrease",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType TurnOffActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_TurnOff_name,
                Description = "Tắt một tính năng",
                Geometry = "Switch Off",
                Type = "Off",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType TurnOnActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_TurnOn_name,
                Description = "Bật một tính năng",
                Geometry = "Switch On",
                Type = "On",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType ToggleActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Turnonthenoff_name,
                Description = "Chuyển đổi trạng thái Bật Tắt",
                Geometry = "Toggle On-Off",
                Type = "On/Off",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                IsValueDisplayed = false,
                IsTargetDeviceDisplayed = true
            };
        }
        private ActionType SwitchToActionType()
        {
            return new ActionType
            {
                Name = adrilight_shared.Properties.Resources.ActionType_Switchto_name,
                Description = "Chuyển đổi đồng thời kích hoạt một tính năng",
                Geometry = "Switch Mode",
                Type = "Change",
                LinkText = adrilight_shared.Properties.Resources.ActionType_Increase_linktext,
                ToResultText = "thành",
                IsValueDisplayed = true,
                IsTargetDeviceDisplayed = true
            };
        }
        public List<ActionType> GetAvailableActions()
        {
            var actions = new List<ActionType>
            {
                IncreaseActionType(),
                DecreaseActionType(),
                TurnOnActionType(),
                TurnOffActionType(),
                ToggleActionType(),
                SwitchToActionType(),
                ActivateActionType(),
            };
            return actions;
        }
    }
}
