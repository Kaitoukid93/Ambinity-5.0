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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace adrilight.Util
{
    public class MotionLayer:INotifyPropertyChanged // for displaying motion at rainbow control panel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<ITimeLineDataItem> _motions;
        public MotionLayer()
        {
            Motions = new ObservableCollection<ITimeLineDataItem>();
        }
       public ObservableCollection<ITimeLineDataItem> Motions {
            get { return _motions; }
            set { _motions = value; OnPropertyChanged(); }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
