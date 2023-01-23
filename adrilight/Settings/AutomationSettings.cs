using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class AutomationSettings : ViewModelBase, IAutomationSettings
    {
        private string _name;
        private ObservableCollection<IActionSettings> _actions;
        private int _conditionTypeIndex; // this is the index that point to type of the action, could be timmer or hot key

        private KeyModel _standardKey;// this is the condition such as key stroke code 
        private bool _isEnabled = true;
        
       
      
        private ObservableCollection<ModifierKeys> _modifiers;

        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public ObservableCollection<IActionSettings> Actions { get => _actions; set { Set(() => Actions, ref _actions, value); } }
        public ObservableCollection<ModifierKeys> Modifiers { get => _modifiers; set { Set(() => Modifiers, ref _modifiers, value); } }
        public int ConditionTypeIndex { get => _conditionTypeIndex; set { Set(() => ConditionTypeIndex, ref _conditionTypeIndex, value); } }
        public KeyModel StandardKey { get => _standardKey; set { Set(() => StandardKey, ref _standardKey, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
    }
}
