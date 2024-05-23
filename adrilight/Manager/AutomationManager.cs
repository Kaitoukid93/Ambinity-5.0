using adrilight.ViewModel.Automation;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Settings;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Windows.Devices.Input;

namespace adrilight.Manager
{
    public class AutomationManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
        public event Action<AutomationSettings> NewAutomationAdded;
        #region Construct
        public AutomationManager(KeyboardHookManagerSingleton keyboardHookManager,
            GeneralSettings generalSettings,
            DeviceManager deviceManager,
            AutomationDBManager dbManager,
            AutomationExecutor executor)
        {
            _generalSettings = generalSettings;
            _keyboardHookManager = keyboardHookManager;
            _dbManager = dbManager;
            _automationExecutor = executor;
            _deviceManager = deviceManager;
            _deviceManager.NewDeviceAdded += _deviceManager_NewDeviceAdded;
            LoadData();
        }


        #endregion
        #region Events
        private void _deviceManager_NewDeviceAdded(IDeviceSettings device)
        {
            ShutdownAutomation().Actions.Add(_dbManager.CreateDeviceShutdownAction(device));
            MonitorSleepAutomation().Actions.Add(_dbManager.CreateDeviceMonitorSleepAction(device));
            MonitorWakeUpAutomation().Actions.Add(_dbManager.CreateDeviceMonitorWakeupAction(device));


        }
        #endregion
        #region Properties
        private KeyboardHookManagerSingleton _keyboardHookManager;
        private List<AutomationSettings> _availableAutomations;
        private GeneralSettings _generalSettings;
        private AutomationDBManager _dbManager;
        private AutomationExecutor _automationExecutor;
        private DeviceManager _deviceManager;
        public List<AutomationSettings> AvailableAutomations {
            get
            {
                return _availableAutomations;
            }
        }
        #endregion
        #region Methods
        //this will raise an event when registered hotkey combo was pressed
        public void Start()
        {
            _keyboardHookManager.Instance.Start();
        }
        private void LoadData()
        {
            _availableAutomations = _dbManager.LoadAutomationIfExist();
            if (_generalSettings.HotkeyEnable)
            {
                Start();
                RegisterAllAutomation();
            }

        }
        public void AddNewAutomation(string name)
        {

            AutomationSettings newAutomation = new AutomationSettings { Name = name };
            _availableAutomations.Add(newAutomation);
            //update collectionview
            NewAutomationAdded?.Invoke(newAutomation);
        }
        private void RegisterAllAutomation()
        {
            foreach (var automation in _availableAutomations)
            {
                Register(automation);
            }
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
                            _automationExecutor.Execute(automation);
                            Log.Information(automation.Name + " excuted");
                        });
                        break;

                    case 1:
                        _keyboardHookManager.Instance.RegisterHotkey(modifierkeys.First(), condition.StandardKey.KeyCode, () =>
                        {
                            _automationExecutor.Execute(automation);
                            Log.Information(automation.Name + " excuted");
                        });
                        break;

                    default:
                        _keyboardHookManager.Instance.RegisterHotkey([.. modifierkeys], condition.StandardKey.KeyCode, () =>
                        {
                            _automationExecutor.Execute(automation);
                            Log.Information(automation.Name + " excuted");
                        });
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
        public void Refresh()
        {
            Unregister();
            RegisterAllAutomation();
        }
        public void Unregister()
        {
            _keyboardHookManager.Instance.UnregisterAll();
        }
        public AutomationSettings ShutdownAutomation() => _availableAutomations.Where(
               a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
           && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.Shutdown)
               .FirstOrDefault()
               as AutomationSettings;
        public AutomationSettings MonitorSleepAutomation() => _availableAutomations.Where(
               a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
           && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorSleep)
               .FirstOrDefault()
               as AutomationSettings;
        public AutomationSettings MonitorWakeUpAutomation() => _availableAutomations.Where(
               a => ((a as AutomationSettings).Condition is SystemEventTriggerCondition)
           && ((a as AutomationSettings).Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorWakeup)
               .FirstOrDefault()
               as AutomationSettings;
        #endregion


    }
}
