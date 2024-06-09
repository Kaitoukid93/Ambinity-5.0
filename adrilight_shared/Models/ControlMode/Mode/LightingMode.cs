using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.ModeParameters;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace adrilight_shared.Models.ControlMode.Mode
{
    public class LightingMode : ViewModelBase, IControlMode
    {
        //bool Autostart { get; set; }


        public LightingMode()
        {
            Parameters = new List<IModeParameter>();
        }

        /// <summary>
        /// Name of this mode
        /// </summary>
        public string Name => GetName();
        /// <summary>
        /// Description to tell what is this mode all about
        /// </summary>
        public string Description => GetDescription();
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
        private string GetName()
        {
            string name;
            switch (BasedOn)
            {
                case LightingModeEnum.ScreenCapturing:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_ScreenCapturing;
                    break;
                case LightingModeEnum.MusicCapturing:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_MusicCapturing;
                    break;
                case LightingModeEnum.StaticColor:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_StaticColor;
                    break;
                case LightingModeEnum.Rainbow:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_Rainbow;
                    break;
                case LightingModeEnum.Animation:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_Animation;
                    break;
                case LightingModeEnum.Gifxelation:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_Gifxelation;
                    break;
                case LightingModeEnum.Custom:
                    name = adrilight_shared.Properties.Resources.LightingMode_GetName_Custom;
                    break;
                default:
                    name = string.Empty;
                    break;
            }
            return name;

        }
        private string GetDescription()
        {
            string description;
            switch(BasedOn)
            {
                case LightingModeEnum.ScreenCapturing:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_ScreenCapturing;
                    break;
                case LightingModeEnum.MusicCapturing:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_Df;
                    break;
                case LightingModeEnum.StaticColor:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_StaticColor;
                    break;
                case LightingModeEnum.Rainbow:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_Rainbow;
                    break;
                case LightingModeEnum.Animation:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_Animation;
                    break;
                case LightingModeEnum.Gifxelation:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_Gifxelation;
                    break;
                case LightingModeEnum.Custom:
                    description = adrilight_shared.Properties.Resources.LightingMode_GetDescription_Sdf;
                    break;
                default:
                    description = string.Empty;
                    break;
            }
            return description;

        }
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
        [JsonIgnore]
        public object Lock { get; } = new object();
    }
}
