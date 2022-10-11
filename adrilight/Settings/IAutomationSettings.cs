﻿using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using NonInvasiveKeyboardHookLibrary;

namespace adrilight
{
    public interface IAutomationSettings : INotifyPropertyChanged
    {
     /// <summary>
     /// this present an automation that execute based on condition like hotkey pressed or timmer
     /// </summary>
     
        string Name { get; set; }
        List<IActionSettings> Actions { get; set; }
        int ConditionTypeIndex { get; set; }  // this is the index that point to type of the action, could be timmer or hot key

        int Condition { get; set; }// this is the condition such as key stroke code 
        List<IModifiersType> Modifiers { get; set; }
        bool IsEnabled { get; set; }
     
        
        
    }
}
