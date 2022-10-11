using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using adrilight.ViewModel;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView 
    {
       
        public MainView()
        {
            InitializeComponent();
            // ViewModel = new MainViewViewModel();
            // this.DataContext = ViewModel;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.9;
            this.Width = SystemParameters.PrimaryScreenWidth * 0.8;
            noticon.Init();
       
            var view = DataContext as MainViewViewModel;
            if (view != null)
            {
                view.FanControlView = new SeriesCollection
           {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80)

                }
                }
            };
                view.IsCanvasLightingWindowOpen = true;
                view.IsSplitLightingWindowOpen = true;
                //view.IsSplitLightingWindowOpen = true;
            }


        }
     
        //protected override void OnClosed(EventArgs e)
        //{
        //    _source.RemoveHook(HwndHook);
        //    UnregisterHotKey(_windowHandle, HOTKEY_ID);
        //    base.OnClosed(e);
        //}
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
           
            
            NonClientAreaContent = new NonClientAreaContent();
            

            

           
          
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {           
           // this.DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewViewModel;
            if (vm != null)
            {
                vm.IsSplitLightingWindowOpen = false;
                vm.IsCanvasLightingWindowOpen = false;
                if (vm.CurrentDevice != null)
                    vm.SaveCurrentProfile(vm.CurrentDevice.ActivatedProfileUID);
            }
            e.Cancel = true;
            // Hide Window instead
            this.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           

        }

        private void OpenDashboard(object sender, RoutedEventArgs e)
        {
            this.Visibility=Visibility.Visible;
            var vm = DataContext as MainViewViewModel;
            vm.IsSplitLightingWindowOpen = true;
            vm.IsCanvasLightingWindowOpen = true;
        }
    }
}
