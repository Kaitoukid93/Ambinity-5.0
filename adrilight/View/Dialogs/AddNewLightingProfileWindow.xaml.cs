using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AddNewLightingProfileWindow
    {
        public AddNewLightingProfileWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
