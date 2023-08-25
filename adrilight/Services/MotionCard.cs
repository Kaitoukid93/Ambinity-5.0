using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TimeLineTool;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace adrilight.Util
{
    public class MotionCard : ITimeLineDataItem // for displaying motion at rainbow control panel
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
  

       
        //timeline data item inheritance

        public Boolean TimelineViewExpanded { get; set; }
    
        double _startFrame;
        double _endFrame;
        double _trimStart;
        double _trimEnd;
        string _name;
        double _originalDuration;
        string _source;


        public double StartFrame {
            get { return _startFrame; }
            set { _startFrame = value; OnPropertyChanged(); }
        }
        public double EndFrame {
            get { return _endFrame; }
            set { _endFrame = value; OnPropertyChanged(); }
        }
        public double TrimStart {
            get { return _trimStart; }
            set { _trimStart = value; OnPropertyChanged(); }
        }
        public double TrimEnd {
            get { return _trimEnd; }
            set { _trimEnd = value; OnPropertyChanged(); }
        }
        public string Source {
            get { return _source; }
            set { _source = value; OnPropertyChanged(); }
        }
        public string Name {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
        public double OriginalDuration {
            get { return _originalDuration; }
            set { _originalDuration = value; OnPropertyChanged(); }
        }
        private Color _color;
        public Color Color {
            get { return _color; }
            set
            {
                _color = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
