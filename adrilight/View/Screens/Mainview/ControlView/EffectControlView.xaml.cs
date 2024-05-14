using adrilight_shared.Models.ControlMode.ModeParameters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class EffectControlView
    {
        public EffectControlView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        private void GroupBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grBx = (Border)sender;
            var dataCntx = grBx.DataContext;
            var dataSource = (ListSelectionParameter)dataCntx;
            if (dataSource != null)
            {
                if (dataSource.ShowMore)
                    dataSource.ShowMore = false;
                else
                    dataSource.ShowMore = true;
            }
        }


        private void ButtonMode_OnClick(object sender, RoutedEventArgs e)
        {
            PopupMode.IsOpen = true;
        }


        private void NewModeSelected(object sender, SelectionChangedEventArgs e)
        {
            PopupMode.IsOpen = false;
        }

     

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            PopupExport.IsOpen = true;
        }
    }
}
