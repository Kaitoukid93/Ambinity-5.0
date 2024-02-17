using adrilight.ViewModel;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for ActionmanagerWindow.xaml
    /// </summary>
    public partial class FirmwareSelectionWindow
    {
        public FirmwareSelectionWindow()
        {
            InitializeComponent();
        }
        private MainViewViewModel ViewModel {
            get
            {
                return (MainViewViewModel)this.DataContext;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.FrimwareUpgradeIsInProgress = false;
            this.Close();

        }


    }
}
