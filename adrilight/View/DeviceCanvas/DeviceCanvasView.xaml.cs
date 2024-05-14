
using adrilight.View.DeviceCanvas.Adorners;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adrilight.View.DeviceCanvas
{
    /// <summary>
    /// Interaction logic for RichCavnas.xaml
    /// </summary>
    public partial class DeviceCanvasView : UserControl
    {
        public DeviceCanvasView()
        {
            InitializeComponent();
        }
        private void OnScrolling(object sender, RoutedEventArgs e)
        {
            AttachedAdorner.OnScrolling();

        }
    }
}
