﻿using adrilight_shared.Models.ControlMode.ModeParameters;
using System.Collections.Generic;
using System.ComponentModel;

namespace adrilight_shared.Models.ControlMode.Mode
{
    public interface IControlMode : INotifyPropertyChanged
    {
        //bool Autostart { get; set; }




        /// <summary>
        /// Name of this mode
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Description to tell what is this mode all about
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Name of the creator, this tend to be personal
        /// </summary>
        string Creator { get; set; }
        /// <summary>
        /// Name of the company or individual that own this mode
        /// </summary>
        string Owner { get; set; }
        /// <summary>
        /// what is this lighting mode based on (rainbow, capturing screne, music capturing
        /// </summary>
        //LightingModeEnum BasedOn { get; set; }
        /// <summary>
        /// List of parameters that this mode have
        /// </summary>
        List<IModeParameter> Parameters { get; set; }
        string Geometry { get; set; }




    }
}