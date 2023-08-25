using NonInvasiveKeyboardHookLibrary;
using System.ComponentModel;

namespace adrilight_shared.Models.KeyboardHook
{
    public interface IModifiersType : INotifyPropertyChanged
    {


        string Name { get; set; }

        ModifierKeys ModifierKey { get; set; }
        bool IsChecked { get; set; }


    }
}
