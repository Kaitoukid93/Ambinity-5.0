using adrilight_shared.Models.ControlMode.ModeParameters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class OutputControlView
    {
        public OutputControlView()
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

        private void Expand_ParamList(object sender, RoutedEventArgs e)
        {

            var grBx = (Button)sender;
            var dataCntx = grBx.DataContext;
            var dataSource = (IModeParameter)dataCntx;
            if (dataSource != null)
            {
                if (dataSource.ShowMore)
                    dataSource.ShowMore = false;
                else
                    dataSource.ShowMore = true;
            }
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            PopupExport.IsOpen = true;
        }
        //private void ToggleButton_MouseDown(object sender, MouseButtonEventArgs e)
        //{


        //}

        //private void betatoggle_Checked(object sender, RoutedEventArgs e)
        //{
        //    var vm = this.DataContext as MainViewViewModel;

        //        //open password dialog
        //        if (vm.OpenPasswordDialogCommand.CanExecute("pw"))
        //        {
        //            vm.OpenPasswordDialogCommand.Execute("pw");
        //        }


        //}

        //private void betatoggle_Unchecked(object sender, RoutedEventArgs e)
        //{

        //}

        //private void betatoggle_Click(object sender, RoutedEventArgs e)
        //{
        //    if ((sender as ToggleButton).IsChecked.Value)
        //    {
        //        betatoggle.IsChecked = false;
        //        var vm = this.DataContext as MainViewViewModel;

        //        //open password dialog
        //        if (vm.OpenPasswordDialogCommand.CanExecute("pw"))
        //        {
        //            vm.OpenPasswordDialogCommand.Execute("pw");
        //        }

        //    }
        //    else
        //    {
        //        // Code for Un-Checked state
        //    }
        //}
    }
}
