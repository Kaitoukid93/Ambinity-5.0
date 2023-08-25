using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Data;
using LibreHardwareMonitor.Hardware;
using System.ComponentModel;

namespace adrilight.Util
{
    internal class Computer : ViewModelBase , IComputer
    {
        //private ObservableCollection<LibreHardwareMonitor.Hardware.IHardware> _processor;
        //public ObservableCollection<LibreHardwareMonitor.Hardware.IHardware> Processor { get => _processor; set { Set(() => Processor, ref _processor, value); } }
        public string Name { get; set; }
        public string Geometry { get; set; }
        public Color Color { get; set; }
        public List<IHardware> Processor { get; set; }
        public List<IHardware> GraphicCard { get; set; }
        public List<IHardware> Ram { get; set; }
        public List<IHardware> MotherBoard { get; set; }
        public string Description { get; set; }
        public  void Refresh()
        {
            
            RaisePropertyChanged();
          
        }

    }
}
