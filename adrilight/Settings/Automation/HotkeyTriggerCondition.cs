using GalaSoft.MvvmLight;
using NonInvasiveKeyboardHookLibrary;
using System.Collections.ObjectModel;

namespace adrilight.Settings.Automation
{
    public class HotkeyTriggerCondition : ViewModelBase, ITriggerCondition
    {
        public HotkeyTriggerCondition()
        {

        }
        public HotkeyTriggerCondition(string name, string description, ObservableCollection<ModifierKeys> modifiers, KeyModel standardKey)
        {
            Name = name;
            Description = description;
            Modifiers = modifiers;
            StandardKey = standardKey;

        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        private KeyModel _standardKey;// this is the condition such as key stroke code 

        private ObservableCollection<ModifierKeys> _modifiers;
        public ObservableCollection<ModifierKeys> Modifiers { get => _modifiers; set { Set(() => Modifiers, ref _modifiers, value); } }
        public KeyModel StandardKey { get => _standardKey; set { Set(() => StandardKey, ref _standardKey, value); } }
    }
}