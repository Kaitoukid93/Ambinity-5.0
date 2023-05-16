using LiveCharts;
using LiveCharts.Defaults;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace adrilight.Util
{
    public interface IModeParameter : INotifyPropertyChanged
    {
        string Name { get; set; }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        ModeParameterTemplateEnum Template { get; set; }
        /// <summary>
        /// this is the type of lighting mode, used for the background process to find this param
        /// </summary>
        ModeParameterEnum ParamType { get; set; }
        /// <summary>
        /// this is the min value, used for prevent the value to undershoot
        /// </summary>
        bool IsEnabled { get; set; }
        ObservableCollection<SubParameter> SubParams { get; set; }
        bool ShowMore{ get; set; }
 
    }
}