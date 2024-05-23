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
        }
        #endregion


        #region Properties

        //private
        private DialogService _dialogService;
        public AutomationSettings Automation { get; set; }
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
            if (Automation == null)
            {
                return;
            }
            Automation = automation;

            AvailableActions = new ObservableCollection<ActionType>();
            foreach (var action in _dbManager.GetAvailableActions())
            {
                AvailableActions.Add(action);
            }
            AvailableTriggerConditions = new ObservableCollection<ITriggerCondition>();
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

                _dialogViewModel.Init("Parameters", "Select parameter", p);
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
                _dialogViewModel.Init("Devices", "Select target device", p);
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
                _dialogViewModel.Init("Values", "Select value", p);
                _dialogService.ShowDialog<AutomationDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        SetCurrentSelectedActionTypeColorValue((Color)_dialogViewModel.Value);
                    }

                }, _dialogViewModel);

            });
            OpenHotkeySelectorCommand = new RelayCommand<ActionSettings>((p) =>
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
                        SaveCurrentSelectedAutomationShortkey(vm.CurrentSelectedModifiers,vm.CurrentSelectedShortKeys.ToArray());
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
            _selectedAction.TargetDeviceUID = targetDevice.DeviceUID;
            _selectedAction.TargetDeviceName = targetDevice.DeviceName;
            //after this step, the parameter has to be reset because the profile UID will return invalid profile for new device
        }

        private void SetCurrentActionParamForSelectedAction(ActionParameter param)
        {
            _selectedAction.ActionParameter = param;
        }

        private void SetCurrentSelectedActionTypeColorValue(Color color)
        {
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
        #endregion

        #region Icommand
        public ICommand AddNewBlankActionCommand { get; set; }
        public ICommand OpenTargetParamSelectionWindowCommand { get; set; }
        public ICommand OpenTargetDeviceSelectionWindowCommand { get; set; }
        public ICommand OpenAutomationValuePickerWindowCommand { get; set; }
        public ICommand OpenHotkeySelectorCommand { get; set; }
        public ICommand SaveButtonCommand { get; set; }
        #endregion

    }
}
