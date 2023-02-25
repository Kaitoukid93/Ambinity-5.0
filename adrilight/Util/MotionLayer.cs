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
using adrilight_effect_analyzer.Model;

namespace adrilight.Util
{
    public class MotionLayer // for displaying motion at rainbow control panel
    {
        public MotionLayer()
        {
            Motions = new ObservableCollection<ITimeLineDataItem>();
        }
       public ObservableCollection<ITimeLineDataItem> Motions { get; set; }

    }
}
