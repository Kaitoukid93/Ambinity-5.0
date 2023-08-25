using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
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
    public partial class AddNewProfileWindow 
    {
        public AddNewProfileWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainViewModel = this.DataContext as MainViewViewModel;
            foreach (DeviceSettings item in e.AddedItems)
            {
                mainViewModel.SelectedDevicesForCurrentProfile.Add(item);
            }

            foreach (DeviceSettings item in e.RemovedItems)
            {
                mainViewModel.SelectedDevicesForCurrentProfile.Remove(item);
            }
        }
    }
}
