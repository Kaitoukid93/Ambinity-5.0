using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace adrilight.Settings.Automation
{
    public class AutomationSettings : ViewModelBase
    {
        private string _name;
        private ObservableCollection<ActionSettings> _actions;
        private bool _isEnabled = true;
        private ITriggerCondition _condition;
        private bool _isLocked = false;


        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public ObservableCollection<ActionSettings> Actions { get => _actions; set { Set(() => Actions, ref _actions, value); } }
        public ITriggerCondition Condition { get => _condition; set { Set(() => Condition, ref _condition, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public bool IsLocked { get => _isLocked; set { Set(() => IsLocked, ref _isLocked, value); } }
        //this method check if any device removed but action still exist in automation
        public void UpdateActions(List<IDeviceSettings> devices)
        {
            var actionsToRemove = new List<ActionSettings>();
            foreach (var action in Actions)
            {
                if (!devices.Any(d => d.DeviceUID == action.TargetDeviceUID))
                {
                    actionsToRemove.Add(action);
                }
            }
            actionsToRemove.ForEach(a => Actions.Remove(a));
        }
    }
}
