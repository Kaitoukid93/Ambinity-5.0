using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace adrilight.View.Screens.Mainview.ControlView
{
    /// <summary>
    /// Interaction logic for PaletteCreatorWindow.xaml
    /// </summary>
    public partial class ColorGradientCreatorWindow
    {
        public ColorGradientCreatorWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
