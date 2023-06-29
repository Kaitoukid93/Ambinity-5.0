using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TimeLineTool
{
	public interface ITimeLineDataItem :INotifyPropertyChanged
	{
		double StartFrame { get; set; }
		string Source { get; set; }
		double EndFrame { get; set; }
		string Name { get; set; }
		double TrimStart { get; set; }
		double TrimEnd { get; set; }
		double OriginalDuration { get; set; }
		Color Color { get; set; }
        Boolean TimelineViewExpanded { get; set; }
	}
}
