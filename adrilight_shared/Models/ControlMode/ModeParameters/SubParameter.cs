using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace adrilight.Models.ControlMode.ModeParameters
{
    public class SubParameter : ViewModelBase
    {
        public SubParameter() { }
        public SubParameter(string name, ModeParameterTemplateEnum template, string commandParameter, string command, int value, int maxValue, int minValue)
        {

            Name = name;
            Template = template;
            CommandParameter = commandParameter;
            Command = command;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;

        }
        private int _value;
        private int _minValue;
        private int _maxValue;
        private bool _isEnabled = true;
        private List<string> _availableValue;
        public string Name { get; set; }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public ModeParameterTemplateEnum Template { get; set; }
        public string Description { get; set; }
        public string CommandParameter { get; set; }
        public string Command { get; set; }
        public int Value { get => _value; set { Set(() => Value, ref _value, value); } }
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public List<string> AvailableValue { get => _availableValue; set { Set(() => AvailableValue, ref _availableValue, value); } }
    }
}