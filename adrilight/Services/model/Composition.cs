using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TimeLineTool;
using System.Collections.ObjectModel;

namespace adrilight.Util
{
    public class Composition // for displaying motion at rainbow control panel
    {


        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Source { get; set; } // to load motion


        //timeline data item inheritance
        public ObservableCollection<MotionLayer> Layers {get;set;}
    }
}
