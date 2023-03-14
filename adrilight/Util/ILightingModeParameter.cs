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
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        LightingModeParameterEnum Type { get; set; }
    }
}