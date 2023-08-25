using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NonInvasiveKeyboardHookLibrary;

namespace adrilight
{
    public class KeyModel : ViewModelBase
    {
        public string Name { get; set; }
        public int KeyCode { get; set; }

     
    }
}
