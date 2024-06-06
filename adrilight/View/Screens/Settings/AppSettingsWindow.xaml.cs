using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AppSettingsWindow
    {
        public AppSettingsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GroupBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grBx = (Border)sender;
            var dataCntx = grBx.DataContext;
            var dataSource = (adrilight_shared.Models.ControlMode.ModeParameters.ListSelectionParameter)dataCntx;
            if (dataSource != null)
            {
                //if (dataSource.ShowMore)
                //    dataSource.ShowMore = false;
                //else
                //    dataSource.ShowMore = true;
            }
        }

    }
}
