using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Windows.Documents;

namespace adrilight.Util
{
    public class LightingModeParameter : ViewModelBase, ILightingModeParameter
    {
        private string _name;
        private string _description;
        private object _value;
        private LightingModeParameterTemplateEnum _template;
        private LightingModeParameterEnum _type;
        private int _minValue;
        private int _maxValue;
        private List<object> _availableValue;


        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or index in the list item
        /// </summary>
        public object Value { get => _value; set { Set(() => Value, ref _value, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        public List<object> AvailableValue  { get => _availableValue; set { Set(() => AvailableValue, ref _availableValue, value); } }
        /// <summary>
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        public LightingModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public LightingModeParameterEnum Type { get => _type; set { Set(() => Type, ref _type, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public object MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public object MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        #region default lightingmode parameter defined by ambino
        public LightingModeParameter GenericSpeedParameter {
            get
            {
                return new LightingModeParameter() {

                    Name = "Speed",
                    Description = "Speed of Motion",
                    Type = LightingModeParameterEnum.Speed,
                    Template = LightingModeParameterTemplateEnum.ValueSlider,
                    Value = 50

                };
            }
        }
        public LightingModeParameter GenericDirrectionParameter {
            get
            {
                return new LightingModeParameter() {

                    Name = "Dirrection",
                    Description = "Speed of Motion",
                    Type = LightingModeParameterEnum.Direction,
                    Template = LightingModeParameterTemplateEnum.ListSelection,
                    Value = 1,
                    AvailableValue = new List<object>() { "Foward", "Reverse" }

                };
            }
        }
        public LightingModeParameter GenericBrightnessParameter {
            get
            {
                return new LightingModeParameter() {

                    Name = "Brightness",
                    Description = "Brightness of LEDs",
                    Type = LightingModeParameterEnum.Brightness,
                    Template = LightingModeParameterTemplateEnum.ValueSlider,
                    Value = 50

                };
            }
        }
        /// <summary>
        /// Use this for rainbow engine to select chasing pattern from database
        /// </summary>
        public LightingModeParameter ChasingPatterns { 
            get
            {
                return new LightingModeParameter() {

                    Name = "Pattern",
                    Description = "The motion to be colored",
                    Type = LightingModeParameterEnum.ChasingPattern,
                    Template = LightingModeParameterTemplateEnum.ListSelection,
                    Value = 50

                };
            }
        }
        /// <summary>
        /// Use this for rainbow engine to select chasing pattern from database
        /// </summary>
        public LightingModeParameter ColorMode {
            get
            {
                return new LightingModeParameter() {

                    Name = "ColorMode",
                    Description = "How the color being used",
                    Type = LightingModeParameterEnum.ColorMode,
                    Template = LightingModeParameterTemplateEnum.ListSelection,
                    Value = 0,
                    AvailableValue = new List<object>() { "Solid", "Random", "Cyclic","Full"}


                };
            }
        }
        #endregion
    }
}