using adrilight.ViewModel;
using adrilight_shared.Models.Device;
using System.Windows;
using System.Windows.Controls;

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
