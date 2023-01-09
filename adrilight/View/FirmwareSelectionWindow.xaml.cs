using adrilight.ViewModel;
using HandyControl.Data;
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
using System.Windows.Shapes;

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
            ViewModel.GeneralSettings.FrimwareUpgradeIsInProgress = false;
            this.Close();

        }
        

    }
}
