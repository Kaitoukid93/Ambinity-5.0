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
    public partial class OOTBWindow
    {
        public OOTBWindow()
        {
            InitializeComponent();
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        IOutputSettings SelectedOutput { get; set; }
        private void NewModeSelected(object sender, SelectionChangedEventArgs e) => PopupMode.IsOpen = false;
        private void ButtonMode_OnClick(object sender, RoutedEventArgs e) => PopupMode.IsOpen = true;
        private void Done_Button_Click(object sender, RoutedEventArgs e)
        {
            //SelectedOutput = (IOutputSettings)outputList.SelectedItem;
            DialogResult = true;

            // close this dialog
            this.Close();
          
        }
    }
}
