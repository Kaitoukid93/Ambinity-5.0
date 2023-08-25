using GalaSoft.MvvmLight;
using NonInvasiveKeyboardHookLibrary;

namespace adrilight_shared.Models.KeyboardHook
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
