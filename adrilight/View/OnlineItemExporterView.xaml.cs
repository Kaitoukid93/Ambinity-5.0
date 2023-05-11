using adrilight.Settings;
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
            var mainViewModel = this.DataContext as MainViewViewModel;
            foreach (DeviceTypeDataEnum item in e.AddedItems)
            {
                mainViewModel.OnlineItemSelectedTargetTypes.Add(item);
            }

            foreach (DeviceTypeDataEnum item in e.RemovedItems)
            {
                mainViewModel.OnlineItemSelectedTargetTypes.Remove(item);
            }

        }
    }
}