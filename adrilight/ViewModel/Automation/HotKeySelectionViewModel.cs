using adrilight_shared.Models.Automation;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.RelayCommand;
using GalaSoft.MvvmLight;
using HidSharp.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel.Automation
{
    public class HotKeySelectionViewModel : ViewModelBase
    {
        public HotKeySelectionViewModel()
        {
            CommandSetup();
            CurrentSelectedModifiers = new ObservableCollection<string>();
            CurrentSelectedShortKeys = new ObservableCollection<KeyModel>();
        }
        #region Methods
        private Key _lastKey;
        private bool _newPress = false;
        private int keyCount = 0;
        private bool _showHint = true;
        private bool _saveEnabled;
        private bool _isKeyValid = false;
        private ObservableCollection<KeyModel> _currentSelectedShortKeys;
        private ObservableCollection<string> _currentSelectedModifiers;
        public bool IsKeyValid { get { return _isKeyValid; } set { _isKeyValid = value; RaisePropertyChanged(); } }
        public ObservableCollection<KeyModel> CurrentSelectedShortKeys { get { return _currentSelectedShortKeys; } set { _currentSelectedShortKeys = value; RaisePropertyChanged(); } }
        public ObservableCollection<string> CurrentSelectedModifiers { get { return _currentSelectedModifiers; } set { _currentSelectedModifiers = value; RaisePropertyChanged(); } }
        public bool SaveEnabled { get { return _saveEnabled; } set { _saveEnabled = value; RaisePropertyChanged(); } }
        public bool ShowHint { get { return _showHint; } set { _showHint = value; RaisePropertyChanged(); } }

        private void CommandSetup()
        {
            KeyDownCommand = new RelayCommand<KeyEventArgs>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ShowHint = false;
                OnKeyDownHandler(p);

            });
        }
        private void OnKeyDownHandler(KeyEventArgs e)
        {
            string currentKey = "None";
            //Add pressed key to listbox
            if (e.Key == _lastKey)
                return;
            _lastKey = e.Key;
            //check if key pressed is standardkey or modifiers
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                if (e.Key == Key.LeftShift)
                {
                    currentKey = "Shift";
                }
                else if (e.Key == Key.RightShift)
                {
                    currentKey = "Shift";
                }
                else if (e.Key == Key.LeftAlt)
                {
                    currentKey = "Alt";
                }
                else if (e.Key == Key.RightAlt)
                {
                    currentKey = "Alt";
                }
                else if (e.Key == Key.LeftCtrl)
                {
                    currentKey = "Ctrl";
                }
                else if (e.Key == Key.RightCtrl)
                {
                    currentKey = "Ctrl";
                }
                //if holding this modifiers, increment display
                if (_newPress)
                {
                    CurrentSelectedModifiers.Clear();
                }
                if (!CurrentSelectedModifiers.Contains(currentKey))
                    CurrentSelectedModifiers.Add(currentKey);
                IsKeyValid = false;

            }
            else
            {

                CurrentSelectedShortKeys.Clear();
                IoCmd_t keyChar = new IoCmd_t();
                KeyToChar(e.Key, ref keyChar);
                var key = new KeyModel {
                    Name = keyChar.s,
                    KeyCode = KeyInterop.VirtualKeyFromKey(e.Key)

                };
                CurrentSelectedShortKeys.Add(key);
                if (CurrentSelectedModifiers.Count == 0)
                {
                    IsKeyValid = false;

                }
                else
                {
                    IsKeyValid = true;
                }


            }
            _newPress = false;
            keyCount++;
            if (CurrentSelectedShortKeys.Count == 0 || CurrentSelectedModifiers.Count == 0)
            {
                SaveEnabled = false;
            }
            else
            {
                SaveEnabled = true;
            }

        }
        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            //if key up is standard key, replace with new key
            _newPress = true;

        }
        public struct IoCmd_t
        {
            public Key key;
            public bool printable;
            public char character;
            public bool shift;
            public bool ctrl;
            public bool alt;
            public int type; //sideband
            public string s;    //sideband
        };

        public void KeyToChar(Key key, ref IoCmd_t KeyDecode)
        {
            bool iscap;
            bool caplock;
            bool shift;

            KeyDecode.key = key;

            KeyDecode.alt = Keyboard.IsKeyDown(Key.LeftAlt) ||
                              Keyboard.IsKeyDown(Key.RightAlt);

            KeyDecode.ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) ||
                              Keyboard.IsKeyDown(Key.RightCtrl);

            KeyDecode.shift = Keyboard.IsKeyDown(Key.LeftShift) ||
                              Keyboard.IsKeyDown(Key.RightShift);

            if (KeyDecode.alt || KeyDecode.ctrl)
            {
                KeyDecode.printable = false;
                KeyDecode.type = 1;
            }
            else
            {
                KeyDecode.printable = true;
                KeyDecode.type = 0;
            }

            shift = KeyDecode.shift;
            caplock = Console.CapsLock; //Keyboard.IsKeyToggled(Key.CapsLock);
            iscap = (caplock && !shift) || (!caplock && shift);

            switch (key)
            {
                case Key.Enter: KeyDecode.character = '\n'; return;
                case Key.A: KeyDecode.character = (iscap ? 'A' : 'a'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.B: KeyDecode.character = (iscap ? 'B' : 'b'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.C: KeyDecode.character = (iscap ? 'C' : 'c'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D: KeyDecode.character = (iscap ? 'D' : 'd'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.E: KeyDecode.character = (iscap ? 'E' : 'e'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.F: KeyDecode.character = (iscap ? 'F' : 'f'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.G: KeyDecode.character = (iscap ? 'G' : 'g'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.H: KeyDecode.character = (iscap ? 'H' : 'h'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.I: KeyDecode.character = (iscap ? 'I' : 'i'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.J: KeyDecode.character = (iscap ? 'J' : 'j'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.K: KeyDecode.character = (iscap ? 'K' : 'k'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.L: KeyDecode.character = (iscap ? 'L' : 'l'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.M: KeyDecode.character = (iscap ? 'M' : 'm'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.N: KeyDecode.character = (iscap ? 'N' : 'n'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.O: KeyDecode.character = (iscap ? 'O' : 'o'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.P: KeyDecode.character = (iscap ? 'P' : 'p'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Q: KeyDecode.character = (iscap ? 'Q' : 'q'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.R: KeyDecode.character = (iscap ? 'R' : 'r'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.S: KeyDecode.character = (iscap ? 'S' : 's'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.T: KeyDecode.character = (iscap ? 'T' : 't'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.U: KeyDecode.character = (iscap ? 'U' : 'u'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.V: KeyDecode.character = (iscap ? 'V' : 'v'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.W: KeyDecode.character = (iscap ? 'W' : 'w'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.X: KeyDecode.character = (iscap ? 'X' : 'x'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Y: KeyDecode.character = (iscap ? 'Y' : 'y'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Z: KeyDecode.character = (iscap ? 'Z' : 'z'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D0: KeyDecode.character = (shift ? '0' : '0'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D1: KeyDecode.character = (shift ? '1' : '1'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D2: KeyDecode.character = (shift ? '2' : '2'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D3: KeyDecode.character = (shift ? '3' : '3'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D4: KeyDecode.character = (shift ? '4' : '4'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D5: KeyDecode.character = (shift ? '5' : '5'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D6: KeyDecode.character = (shift ? '6' : '6'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D7: KeyDecode.character = (shift ? '7' : '7'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D8: KeyDecode.character = (shift ? '8' : '8'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.D9: KeyDecode.character = (shift ? '9' : '9'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemPlus: KeyDecode.character = (shift ? '=' : '='); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemMinus: KeyDecode.character = (shift ? '-' : '-'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemQuestion: KeyDecode.character = (shift ? '/' : '/'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemComma: KeyDecode.character = (shift ? ',' : ','); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemPeriod: KeyDecode.character = (shift ? '.' : '.'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemOpenBrackets: KeyDecode.character = (shift ? '[' : '['); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.OemQuotes: KeyDecode.character = (shift ? '.' : '.'); KeyDecode.s = "\""; return;
                case Key.Oem1: KeyDecode.character = (shift ? ';' : ';'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Oem3: KeyDecode.character = (shift ? '`' : '`'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Oem5: KeyDecode.character = (shift ? '\\' : '\\'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Oem6: KeyDecode.character = (shift ? ']' : ']'); KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Tab: KeyDecode.character = '\t'; KeyDecode.s = "Tab"; return;
                case Key.Space: KeyDecode.character = ' '; KeyDecode.s = "Space"; return;

                // Number Pad
                case Key.NumPad0: KeyDecode.character = '0'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad1: KeyDecode.character = '1'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad2: KeyDecode.character = '2'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad3: KeyDecode.character = '3'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad4: KeyDecode.character = '4'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad5: KeyDecode.character = '5'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad6: KeyDecode.character = '6'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad7: KeyDecode.character = '7'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad8: KeyDecode.character = '8'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.NumPad9: KeyDecode.character = '9'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Subtract: KeyDecode.character = '-'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Add: KeyDecode.character = '+'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Decimal: KeyDecode.character = '.'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Divide: KeyDecode.character = '/'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.Multiply: KeyDecode.character = '*'; KeyDecode.s = KeyDecode.character.ToString(); return;
                case Key.CapsLock: KeyDecode.character = '*'; KeyDecode.s = "Capslock"; return;
                case Key.F1: KeyDecode.character = 'F'; KeyDecode.s = "F1"; return;
                case Key.F2: KeyDecode.character = 'F'; KeyDecode.s = "F2"; return;
                case Key.F3: KeyDecode.character = 'F'; KeyDecode.s = "F3"; return;
                case Key.F4: KeyDecode.character = 'F'; KeyDecode.s = "F4"; return;
                case Key.F5: KeyDecode.character = 'F'; KeyDecode.s = "F5"; return;
                case Key.F6: KeyDecode.character = 'F'; KeyDecode.s = "F6"; return;
                case Key.F7: KeyDecode.character = 'F'; KeyDecode.s = "F7"; return;
                case Key.F8: KeyDecode.character = 'F'; KeyDecode.s = "F8"; return;
                case Key.F9: KeyDecode.character = 'F'; KeyDecode.s = "F9"; return;
                case Key.F10: KeyDecode.character = 'F'; KeyDecode.s = "F10"; return;
                case Key.F11: KeyDecode.character = 'F'; KeyDecode.s = "F11"; return;
                case Key.F12: KeyDecode.character = 'F'; KeyDecode.s = "F12"; return;
                default:
                    KeyDecode.type = 1;
                    KeyDecode.printable = false;
                    KeyDecode.character = '\x00';
                    return;
            } //switch          
        } // function
        #endregion
        #region Icommand
        public ICommand KeyDownCommand { get; set; }
        #endregion
    }
}
