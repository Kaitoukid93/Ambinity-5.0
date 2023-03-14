using GalaSoft.MvvmLight;

namespace adrilight.Util
{
    public class LightingModeParameter : ViewModelBase, ILightingModeParameter
    {
        private string _name;
        private string _description;
        private object _value;
        private LightingModeParameterEnum _type;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        public object Value { get => _value; set { Set(() => Value, ref _value, value); } }
        /// <summary>
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        public LightingModeParameterEnum Type { get => _type; set { Set(() => Type, ref _type, value); } }
    }
}