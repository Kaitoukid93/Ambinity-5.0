using adrilight.Helpers;
using adrilight.ViewModel.Profile;
using adrilight_shared.Enums;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.ViewModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using adrilight_shared.Settings;
using adrilight_shared.Models.ItemsCollection;
using adrilight.Manager;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ControlMode.Mode;
using Microsoft.Win32.TaskScheduler;
using System.Web.Management;

namespace adrilight.ViewModel.Automation
{
    public class AutomationEditorViewModel : ViewModelBase
    {
        public event Action<bool> OnEditorViewClosing;
        #region Construct
        public AutomationEditorViewModel(DialogService service, AutomationDBManager automationDBManager, AutomationDialogViewModel dialogViewModel)
        {
            _dialogService = service;
            _dbManager = automationDBManager;
            _dialogViewModel = dialogViewModel;
            AvailableActions = new ObservableCollection<ActionType>();
            AvailableTriggerConditions = new ObservableCollection<ITriggerCondition>();
            CommandSetup();
            LoadSelectableIcons();


        }
        #endregion


        #region Properties

        //private
        private DialogService _dialogService;
        private AutomationSettings _automation;
        public AutomationSettings Automation {
            get
            {
                return _automation;
            }
            set
            {
                _automation = value;
                RaisePropertyChanged();
            }
        }
        private AutomationDBManager _dbManager;
        private ActionSettings _selectedAction;
        private AutomationDialogViewModel _dialogViewModel;
        public ObservableCollection<ActionType> AvailableActions { get; set; }
        public ObservableCollection<ITriggerCondition> AvailableTriggerConditions { get; set; }
        public ObservableCollection<string> SelectableIcons { get; set; }


        #endregion





        #region Methods
        public void Init(AutomationSettings automation)
        {
            if (automation == null)
            {
                return;
            }
            Automation = automation;
            AvailableActions.Clear();
            foreach (var action in _dbManager.GetAvailableActions())
            {
                AvailableActions.Add(action);
            }
            AvailableTriggerConditions.Clear();
            foreach (var condition in _dbManager.GetAvailableCondition())
            {
                AvailableTriggerConditions.Add(condition);
            }
        }
        public void CommandSetup()
        {
            AddNewBlankActionCommand = new RelayCommand<ActionType>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                AddSelectedActionTypeToList(p);

            });
            OpenTargetParamSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                _selectedAction = p;
                var result =_dialogViewModel.Init("Parameters", "Select parameter", p);
                if (!result)
                    return;
                _dialogService.ShowDialog<AutomationDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        SetCurrentActionParamForSelectedAction(_dialogViewModel.Value as ActionParameter);
                    }

                }, _dialogViewModel);

            });
            OpenTargetDeviceSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                _selectedAction = p;
              var result = _dialogViewModel.Init("Devices", "Select target device", p);
                if (!result)
                    return;
                _dialogService.ShowDialog<AutomationDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        SetCurrentActionTargetDeviceForSelectedAction(_dialogViewModel.Value as DeviceSettings);
                    }

                }, _dialogViewModel);

            });
            OpenAutomationValuePickerWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                _selectedAction = p;
               var result = _dialogViewModel.Init("Values", "Select value", p);
                if (!result)
                    return;
                _dialogService.ShowDialog<AutomationDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        switch(_selectedAction.ActionParameter.Type)
                        {
                            case "color":
                                SetCurrentSelectedActionTypeColorValue(_dialogViewModel.Value as ColorCard);
                                break;
                            case "mode":
                                SetCurrentSelectedActionTypeModeValue(_dialogViewModel.Value as LightingMode);
                                break;
                        }
                        
                    }

                }, _dialogViewModel);

            });
            OpenHotkeySelectorCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //init hotkeyselector viewmodel
                var vm = new HotKeySelectionViewModel();
                _dialogService.ShowDialog<HotKeySelectionViewModel>(result =>
                {
                    if (result == "True")
                    {
                        SaveCurrentSelectedAutomationShortkey(vm.CurrentSelectedModifiers, vm.CurrentSelectedShortKeys.ToArray());
                    }

                }, vm);
            });
            SaveButtonCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //back to collection view
                OnEditorViewClosing?.Invoke(true);

            });
            DeleteSelectedActionFromListCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //back to collection view
                DeleteSelectedActionFromList(p);

            });

        }
        private void AddSelectedActionTypeToList(ActionType actionType)
        {
            actionType.Init();
            ActionSettings newBlankAction = new ActionSettings();
            newBlankAction.ActionType = actionType;
            newBlankAction.ActionParameter = new ActionParameter { Name = adrilight_shared.Properties.Resources.ActionParameter_Properties_name, Type = "unknown", Value = "none" };
            Automation.Actions.Add(newBlankAction);
        }
        private void DeleteSelectedActionFromList(ActionSettings action)
        {
            Automation.Actions.Remove(action);
        }

        private void SetCurrentActionTargetDeviceForSelectedAction(IDeviceSettings targetDevice)
        {
            if (targetDevice == null)
                return;
            _selectedAction.TargetDeviceUID = targetDevice.DeviceUID;
            _selectedAction.TargetDeviceName = targetDevice.DeviceName;
            //after this step, the parameter has to be reset because the profile UID will return invalid profile for new device
        }

        private void SetCurrentActionParamForSelectedAction(ActionParameter param)
        {
            if (param != null)
                return;
            _selectedAction.ActionParameter = param;
        }
        private void SetCurrentSelectedActionTypeModeValue(LightingMode mode)
        {
            if (mode == null)
                return;
            _selectedAction.ActionParameter.Value = mode.BasedOn;
        }

        private void SetCurrentSelectedActionTypeColorValue(ColorCard color)
        {
            if (color == null)
                return;
            _selectedAction.ActionParameter.Value = color;
        }
        private void SaveCurrentSelectedAutomationShortkey(ObservableCollection<string> modifiers, KeyModel[] keys)
        {
            var modifiersKey = new ObservableCollection<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
            foreach (var modifier in modifiers)
            {
                modifiersKey.Add(ConvertStringtoModifier(modifier));
            }
            var condition = new HotkeyTriggerCondition("HotKey", "Hotkey", modifiersKey, keys[0]);
            Automation.Condition = condition;
        }
        private NonInvasiveKeyboardHookLibrary.ModifierKeys ConvertStringtoModifier(string key)
        {
            NonInvasiveKeyboardHookLibrary.ModifierKeys returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.WindowsKey;
            switch (key)
            {
                case "Shift":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift;
                    break;

                case "Ctrl":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control;
                    break;

                case "Alt":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt;
                    break;
            }
            return returnKey;
        }
        private void LoadSelectableIcons() => SelectableIcons = new ObservableCollection<string>() {
                 "Shortcut",
                 "Youtube",
                 "Gaming",
                 "Reading",
                 "Still Image",
                 "Boost",
                 "Study",
                 "Fire",
                 "Spotlight",
                 "Eye Open",
                 "Eye Close",
                 "Meeting",
                 "Coding",
                 "Lightbulb",
                 "Switch On",
                 "Switch Off",
                 "Increase",
                 "Decrease",
            };
        #endregion

        #region Icommand
        public ICommand AddNewBlankActionCommand { get; set; }
        public ICommand OpenTargetParamSelectionWindowCommand { get; set; }
        public ICommand OpenTargetDeviceSelectionWindowCommand { get; set; }
        public ICommand OpenAutomationValuePickerWindowCommand { get; set; }
        public ICommand DeleteSelectedActionFromListCommand { get; set; }
        public ICommand OpenHotkeySelectorCommand { get; set; }
        public ICommand SaveButtonCommand { get; set; }
        #endregion

    }
}
