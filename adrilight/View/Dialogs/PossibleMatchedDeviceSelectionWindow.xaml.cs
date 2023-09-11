using adrilight.ViewModel;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for ActionmanagerWindow.xaml
    /// </summary>
    public partial class PossibleMatchedDeviceSelectionWindow
    {
        public PossibleMatchedDeviceSelectionWindow()
        {
            InitializeComponent();
        }
        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
