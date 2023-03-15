using System.Collections.Generic;
using System.ComponentModel;

namespace adrilight.Util
{
    public interface ILightingModeParameter : INotifyPropertyChanged
    {
        string Name { get; set; }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// this is the template of lighting mode, use to get the data template
        /// </summary>
        LightingModeParameterTemplateEnum Template { get; set; }
        /// <summary>
        /// this is the type of lighting mode, used for the background process to find this param
        /// </summary>
        LightingModeParameterEnum Type { get; set; }
        /// <summary>
        /// this is the list of available value, used for list selection type
        /// </summary>
        List<object> AvailableValue { get; set; }
    }
}