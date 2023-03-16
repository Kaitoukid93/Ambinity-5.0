using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class LightingMode: ViewModelBase, ILightingMode
    {
        //bool Autostart { get; set; }


        public LightingMode() {
            Parameters = new List<LightingModeParameter>();
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
        public LightingModeEnum BasedOn { get; set; }
        /// <summary>
        /// List of parameters that this mode have
        /// </summary>
        public List<LightingModeParameter> Parameters { get; set; }
        public void BrightnessUp(int upValue)
        {
            //find the brightness param and add value
            var brightnessParam = Parameters.Where(p=> p.Type == LightingModeParameterEnum.Brightness).FirstOrDefault();
            var currentValue = (int)brightnessParam.Value;
            var targetValue = currentValue + upValue;
            brightnessParam.Value = targetValue;

        }
    }
}
