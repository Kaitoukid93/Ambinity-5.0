using adrilight.Util;
using System.Collections.Generic;
using System.ComponentModel;

namespace adrilight
{
    public interface IOutputControlableProperty : INotifyPropertyChanged
    {

        string Name { get; set; }
        string Description { get; set; }
        OutputControlablePropertyEnum Type { get; set; }
        /// <summary>
        /// this list contains all available control mode ex: auto-manual,music-rainbow-capturing...
        /// </summary>
        List<IControlMode> AvailableControlMode { get; set; }
        IControlMode CurrentActiveControlMode { get; set; }

        int CurrentActiveControlModeIndex { get; set; }

        string Icon { get; set; }
    }
}