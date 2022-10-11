using adrilight.Spots;
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
    public interface IModifiersType : INotifyPropertyChanged
    {
      
        
        string Name { get; set; }
       
        ModifierKeys ModifierKey { get; set; } 
        bool IsChecked { get; set; }
        
        
    }
}
