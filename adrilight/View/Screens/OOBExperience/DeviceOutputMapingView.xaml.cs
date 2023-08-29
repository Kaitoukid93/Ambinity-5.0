using adrilight.View.Adorners;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceOutputMapingView
    {
        public DeviceOutputMapingView()
        {

            InitializeComponent();
        }

        private void OnScrolling(object sender, RoutedEventArgs e)
        {
            AttachedAdorner.OnScrolling();
        }


    }
}
