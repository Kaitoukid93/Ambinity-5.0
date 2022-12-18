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
    public partial class LEDSetupSelectionWindows
    {
        public LEDSetupSelectionWindows()
        {
            InitializeComponent();
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        IOutputSettings SelectedOutput { get; set; }

        private void Done_Button_Click(object sender, RoutedEventArgs e)
        {
            //SelectedOutput = (IOutputSettings)outputList.SelectedItem;
            DialogResult = true;

            // close this dialog
            this.Close();
          
        }
    }
}
