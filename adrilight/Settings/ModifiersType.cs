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
    internal class ModifiersType : ViewModelBase, IModifiersType
    {
        private string _name;

        private ModifierKeys _modifierKey;
        private bool _isChecked;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public ModifierKeys ModifierKey { get => _modifierKey; set { Set(() => ModifierKey, ref _modifierKey, value); } }
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }

     
    }
}
