using adrilight.ViewModel;
using adrilight_shared.Models.Device;
using System.Windows;
using System.Windows.Controls;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class OnlineItemExporterView
    {
        public OnlineItemExporterView()
        {
            InitializeComponent();

        }

        private void ImageSelector_ImageSelected(object sender, RoutedEventArgs e)
        {

        }
        private void CheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var mainViewModel = this.DataContext as MainViewViewModel;
            //foreach (DeviceType item in e.AddedItems)
            //{
            //    mainViewModel.OnlineItemSelectedTargetTypes.Add(item);
            //}

            //foreach (DeviceType item in e.RemovedItems)
            //{
            //    mainViewModel.OnlineItemSelectedTargetTypes.Remove(item);
            //}

        }
    }
}