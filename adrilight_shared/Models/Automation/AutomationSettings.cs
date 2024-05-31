using adrilight_shared.Models.Device;
using adrilight_shared.Models.ItemsCollection;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace adrilight_shared.Models.Automation
{
    public class AutomationSettings : ViewModelBase, IGenericCollectionItem
    {
        public AutomationSettings()
        {
            Actions = new ObservableCollection<ActionSettings>();

        }
        private string _name;
        private string _geometry = "Shortcut";
        private ObservableCollection<ActionSettings> _actions;
        private bool _isEnabled = true;
        private ITriggerCondition _condition;
        private bool _isLocked = false;
        private bool _isPinned = false;
        private bool _isChecked = false;
        private bool _isSelected = false;
        public bool IsPinned { get => _isPinned; set { Set(() => IsPinned, ref _isPinned, value); } }
        public string Name { get; set; }
        [JsonIgnore]
        public bool IsEditing { get; set; }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string LocalPath { get; set; }
        public string InfoPath { get; set; }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
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
