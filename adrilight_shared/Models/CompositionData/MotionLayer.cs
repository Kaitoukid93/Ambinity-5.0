using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TimeLineTool;

namespace adrilight_shared.Models.CompositionData
{
    public class MotionLayer : INotifyPropertyChanged // for displaying motion at rainbow control panel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<ITimeLineDataItem> _motions;
        public MotionLayer()
        {
            Motions = new ObservableCollection<ITimeLineDataItem>();
        }
        public ObservableCollection<ITimeLineDataItem> Motions
        {
            get { return _motions; }
            set { _motions = value; OnPropertyChanged(); }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
