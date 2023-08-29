﻿using adrilight_shared.Enums;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class ToggleParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {

        public ToggleParameter()
        {

        }
        public ToggleParameter(int defaultValue, ModeParameterEnum type)
        {
            Value = defaultValue;
            Template = ModeParameterTemplateEnum.ToggleOnOff;
            ParamType = type;

        }
        private string _name;
        private string _description;
        private int _value;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
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
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
    }
}
