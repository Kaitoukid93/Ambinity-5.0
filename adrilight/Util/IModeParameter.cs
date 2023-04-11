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
        int Value { get; set; }
        /// <summary>
        /// this is the template of lighting mode, use to get the data template
        /// </summary>
        ModeParameterTemplateEnum Template { get; set; }
        /// <summary>
        /// this is the type of lighting mode, used for the background process to find this param
        /// </summary>
        ModeParameterEnum ParamType { get; set; }
        /// <summary>
        /// this is the list of available value, used for list selection type
        /// </summary>
        ObservableCollection<IPrameterValue> AvailableValue { get;}
        /// <summary>
        /// this is the min value, used for prevent the value to undershoot
        /// </summary>
        string AvailableValueLocalPath { get; set; }
        bool IsEnabled { get; set; }
        IPrameterValue SelectedValue { get; }
        ObservableCollection<SubParameter> SubParams { get; set; }
        void RefreshCollection();
        int MinValue { get; set; }
        /// <summary>
        /// this is the max value, used for prevent the value to overshoot
        /// </summary>
        int MaxValue { get; set; }
        /// <summary>
        /// this store line chart value for data visualization
        /// </summary>
       
    }
}