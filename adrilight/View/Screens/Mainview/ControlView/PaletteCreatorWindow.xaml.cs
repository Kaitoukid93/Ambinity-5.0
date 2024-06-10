using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace adrilight.View.Screens.Mainview.ControlView
{
    /// <summary>
    /// Interaction logic for PaletteCreatorWindow.xaml
    /// </summary>
    public partial class PaletteCreatorWindow
    {
        public PaletteCreatorWindow()
        {
            InitializeComponent();
        }

        private void host_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            (border.Child as Popup).IsOpen = true;
            (border.Child as Popup).Focus();
        }

        private void host_LostMouseCapture(object sender, MouseEventArgs e)
        {
            
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }
    }
}
