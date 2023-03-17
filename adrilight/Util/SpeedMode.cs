﻿using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class SpeedMode : ViewModelBase, IControlMode
    {
        //bool Autostart { get; set; }


        public SpeedMode()
        {
            Parameters = new List<IModeParameter>();
        }

        /// <summary>
        /// Name of this mode
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description to tell what is this mode all about
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name of the creator, this tend to be personal
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// Name of the company or individual that own this mode
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// what is this lighting mode based on (rainbow, capturing screne, music capturing
        /// </summary>
        public SpeedModeEnum SpeedType { get; set; }
        /// <summary>
        /// List of parameters that this mode have
        /// </summary>
        public List<IModeParameter> Parameters { get; set; }


        internal void SetSpeed(int speedValue)
        {
            var speedParam = Parameters.Where(p => p.Type == ModeParameterEnum.Speed).FirstOrDefault();
            if (speedParam != null)
                speedParam.Value = speedValue;
        }
        internal int GetSpeed()
        {
            int speed = 0;
            var speedParam = Parameters.Where(p => p.Type == ModeParameterEnum.Speed).FirstOrDefault();
            if (speedParam != null)
                speed = (int)speedParam.Value;
            return speed;
        }
    }
}
