using adrilight.Models.ControlMode.Enum;
using adrilight.Util.ModeParameters;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace adrilight.Models.ControlMode.Mode
{
    internal class LightingMode : ViewModelBase, IControlMode
    {
        //bool Autostart { get; set; }


        public LightingMode()
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
        public LightingModeEnum BasedOn { get; set; }
        /// <summary>
        /// List of parameters that this mode have
        /// </summary>
        public List<IModeParameter> Parameters { get; set; }
        public string Geometry { get; set; }
        [JsonIgnore]
        public IModeParameter ColorParameter => Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault();
        public void Disable()
        {
            var disableEnableParam = Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            disableEnableParam.Value = 0;
        }
        public void Enable()
        {
            var disableEnableParam = Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            disableEnableParam.Value = 1;
        }
        public int GetBrightness()
        {
            var brightnessParam = Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            int brightness = 0;
            if (brightnessParam != null) { }
            brightness = brightnessParam.Value;
            return brightness;

        }
        public void SetBrightness(int value)
        {
            var brightnessParam = Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            brightnessParam.Value = value;
        }
        [JsonIgnore]
        public int MaxBrightness => (Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter).MaxValue;
        [JsonIgnore]
        public int MinBrightness => (Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter).MinValue;
    }
}
