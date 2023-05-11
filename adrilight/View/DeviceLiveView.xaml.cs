using adrilight.Helpers;
using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceLiveView
    {
        public DeviceLiveView()
        {

            InitializeComponent();
            //MainViewModel = this.DataContext as MainViewViewModel;
        }
        //MainViewViewModel MainViewModel { get; set; }
        private void OnScrolling(object sender, RoutedEventArgs e)
        {
            AttachedAdorner.OnScrolling();

        }

        //private void Canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.OemOpenBrackets)
        //    {
        //        if (MainViewModel.IsInIDEditStage)
        //            MainViewModel.DecreaseBrushSize();
        //    }
        //    if (e.Key == Key.OemCloseBrackets)
        //    {
        //        if (MainViewModel.IsInIDEditStage)
        //            MainViewModel.IncreaseBrushSize();
        //    }
        //}
       

    }
}
