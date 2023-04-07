﻿using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Defaults;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace adrilight.Util
{
    public class ModeParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {
        private string _name;
        private string _description;
        private int _value;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _type;
        private int _minValue;
        private int _maxValue;
        private ObservableCollection<object> _availableValue;
       


        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or index in the list item
        /// </summary>
        public int Value { get => _value; set { Set(() => Value, ref _value, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        public ObservableCollection<object> AvailableValue  { get => _availableValue; set { Set(() => AvailableValue, ref _availableValue, value); } }
        /// <summary>
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        /// 

        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum Type { get => _type; set { Set(() => Type, ref _type, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        
    }
}