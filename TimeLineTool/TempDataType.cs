
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace TimeLineTool
{
	public class TempDataType : ITimeLineDataItem
	{
        public event PropertyChangedEventHandler PropertyChanged;
        double _startFrame;
        double _endFrame;
        double _trimStart;
        double _trimEnd;
        string _name;
        double _originalDuration;
        string _source;


        public double StartFrame
        {
            get { return _startFrame; }
            set { _startFrame = value; OnPropertyChanged(); }
        }
        public double EndFrame
        {
            get { return _endFrame; }
            set { _endFrame = value; OnPropertyChanged(); }
        }
        public double TrimStart
        {
            get { return _trimStart; }
            set { _trimStart = value; OnPropertyChanged(); }
        }
        public double TrimEnd
        {
            get { return _trimEnd; }
            set { _trimEnd = value; OnPropertyChanged(); }
        }
        public string Source
        {
            get { return _source; }
            set { _source = value; OnPropertyChanged(); }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
        public double OriginalDuration
        {
            get { return _originalDuration; }
            set { _originalDuration = value; OnPropertyChanged(); }
        }
   
        public Boolean TimelineViewExpanded { get; set; }
	
        private Color _color;
        public Color Color
        {
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
