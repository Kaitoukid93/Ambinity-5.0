using adrilight.Models.ControlMode.ModeParameters;
using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            var dataSource = (ListSelectionParameter)dataCntx;
            if (dataSource != null)
            {
                if (dataSource.ShowMore)
                    dataSource.ShowMore = false;
                else
                    dataSource.ShowMore = true;
            }
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
