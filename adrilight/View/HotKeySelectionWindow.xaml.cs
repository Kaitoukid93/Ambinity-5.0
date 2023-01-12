using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class HotKeySelectionWindow
    {
        public HotKeySelectionWindow()
        {
            InitializeComponent();
        }
        private Key _lastKey;
        private bool _newPress = false;
        private int keyCount = 0;
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            //Add pressed key to listbox
            if (e.Key == _lastKey)
                return;
            _lastKey = e.Key;
            
            var view = DataContext as MainViewViewModel;
            //check if key pressed is standardkey or modifiers
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                if(e.Key == Key.LeftShift)
                {

                }
                else if(e.Key == Key.RightShift)
                {

                }
                else if (e.Key == Key.LeftAlt)
                {

                }
                else if (e.Key == Key.RightAlt)
                {

                }
                else if (e.Key == Key.LeftCtrl)
                {

                }
                else if (e.Key == Key.RightCtrl)
                {

                }
                //if holding this modifiers, increment display
                if (_newPress)
                {
                    view.CurrentSelectedModifiers.Clear();
                }
                if(!view.CurrentSelectedModifiers.Contains(e.Key.ToString()))
                view.CurrentSelectedModifiers.Add(e.Key.ToString());
            }
            else
            {
                view.CurrentSelectedShortKeys.Clear();
                view.CurrentSelectedShortKeys.Add(e.Key.ToString());
            }
            _newPress = false;
            keyCount++;
        }
        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            //if key up is standard key, replace with new key
            _newPress = true;
            
        }



    }
}
