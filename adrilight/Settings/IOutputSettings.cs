using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using System.Windows.Input;
using adrilight.ViewModel;
using System.Windows;
using adrilight_effect_analyzer.Model;
using Newtonsoft.Json;
using adrilight.Settings;

namespace adrilight
{
    public interface IOutputSettings : INotifyPropertyChanged 
    {
        //bool Autostart { get; set; }

       
       
       
     
        string OutputName { get; set; }
        string OutputDescription { get; set; }
        bool IsVissible { get; set; }
      
        int OutputID { get; set; } // position off current output
        //int VUOrientation { get; set; }
        //int VUMode { get; set; }
        OutputTypeEnum OutputType { get; set; } 
        string OutputInterface { get; set; }
        string TargetDevice { get; set; }
        string OutputUniqueID { get; set; }
        //int OutputBrightness { get; set; }
        int OutputPowerVoltage { get; set; }
        int OutputPowerMiliamps { get; set; }
        bool IsEnabled { get; set; }
        string Geometry { get; set; } // store drawing data of output interface
        //each output can only be attached 1 single device but can contains multiple zones
        //slave device can be LED or Motor or Sensor
        ISlaveDevice SlaveDevice { get; set; }
        //void SetRectangle(System.Drawing.Rectangle rectangle);
        //void SetPreviewRectangle(System.Drawing.Rectangle rectangle);

       
        


    }
}
