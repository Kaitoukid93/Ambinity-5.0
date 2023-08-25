using adrilight_shared.Models.ControlMode.Enum;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class SliderParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {

        public SliderParameter()
        {

        }
        public SliderParameter(int minValue, int maxValue, int defaultValue, ModeParameterEnum type)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Value = defaultValue;
            Template = ModeParameterTemplateEnum.ValueSlider;
            ParamType = type;

        }
        private string _name;
        private string _description;
        private int _value;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private int _minValue;
        private int _maxValue;
        private bool _showMore;
        private ObservableCollection<SubParameter> _subParams;
        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public int Value { get => _value; set { Set(() => Value, ref _value, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
        public string Geometry { get; set; }
    }
}

