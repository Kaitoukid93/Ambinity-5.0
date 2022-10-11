﻿using adrilight.Spots;
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
        private List<IActionSettings> _actions;
        private int _conditionTypeIndex; // this is the index that point to type of the action, could be timmer or hot key

        private int _condition;// this is the condition such as key stroke code 
        private bool _isEnabled = true;
        
       
      
        private List<IModifiersType> _modifiers;

        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public List<IActionSettings> Actions { get => _actions; set { Set(() => Actions, ref _actions, value); } }
        public List<IModifiersType> Modifiers { get => _modifiers; set { Set(() => Modifiers, ref _modifiers, value); } }
        public int ConditionTypeIndex { get => _conditionTypeIndex; set { Set(() => ConditionTypeIndex, ref _conditionTypeIndex, value); } }
        public int Condition { get => _condition; set { Set(() => Condition, ref _condition, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
    }
}
