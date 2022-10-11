using LibreHardwareMonitor.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for ScreenCapturingControl.xaml
    /// </summary>
    public partial class HardwareCard : UserControl
    {

        public HardwareCard()
        {

            InitializeComponent();
            


        }

        public static readonly DependencyProperty CardNameProperty = DependencyProperty.Register("CardName", typeof(string), typeof(HardwareCard));
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof(GeometryGroup), typeof(HardwareCard));
        public static readonly DependencyProperty AvailableSensorsProperty = DependencyProperty.Register("AvailableSensors", typeof(IEnumerable), typeof(HardwareCard));

        public string CardName {
            get
            {
                return (string)GetValue(CardNameProperty);
            }
            set
            {
                SetValue(CardNameProperty, value);

            }
        }
        private List<ISensor> _temperatureSensorList;
        public List<ISensor> TemperatureSensorList {
            get
            {
                var temperatureSensorList = new List<ISensor>();
                if(AvailableSensors!=null)
                {
                    foreach (ISensor sensor in AvailableSensors)
                    {
                        if (sensor.SensorType == SensorType.Load)
                        {
                            temperatureSensorList.Add(sensor);
                        }
                    }
                }
               
                return temperatureSensorList;
            }
            set
            {
                _temperatureSensorList = value;

            }
        }
        public IEnumerable AvailableSensors {
            get
            {
                return (IEnumerable)GetValue(AvailableSensorsProperty);
            }
            set
            {
                SetValue(AvailableSensorsProperty, value);

            }
        }




        public GeometryGroup Geometry {
            get
            {
                return (GeometryGroup)GetValue(GeometryProperty);
            }
            set
            {
                SetValue(GeometryProperty, value);

            }
        }


        private bool TemperatureSensorsFilter(object item)
        {
            ISensor sensor = item as ISensor;
            return sensor.SensorType == SensorType.Load;
        }

        //private void ButtonAdd_OnClick(object sender, RoutedEventArgs e) => PopupAdd.IsOpen = true;

    }
}
