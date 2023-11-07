using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ProfileCollectionView
    {
        public ProfileCollectionView()
        {
            InitializeComponent();

        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            PopUpAddTo.IsOpen = true;
        }



    }
}
