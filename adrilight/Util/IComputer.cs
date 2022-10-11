using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LibreHardwareMonitor.Hardware;



namespace adrilight.Util
{
    public interface IComputer : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Geometry { get; set; }
        Color Color { get; set; }
        List<IHardware> Processor { get; set; }
        List<IHardware> GraphicCard { get; set; }
        List<IHardware> Ram { get; set; }
        List<IHardware> MotherBoard { get; set; }
        string Description { get; set; }
        void Refresh();
        
    }
}
