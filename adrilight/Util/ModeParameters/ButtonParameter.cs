using adrilight.Helpers;
using adrilight.Settings;
using adrilight_effect_analyzer.Model;
using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Defaults;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;

namespace adrilight.Util.ModeParameters
{
    public class ButtonParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {

        public ButtonParameter()
        {

        }
        public ButtonParameter(string commandParameter)
        {
           
            Template = ModeParameterTemplateEnum.PushButtonAction;
            CommandParameter = commandParameter;

        }
        private string _name;
        private string _description;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private bool _showMore;
        private string _commandParameter;
        private ObservableCollection<SubParameter> _subParams;
        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public string CommandParameter { get => _commandParameter; set { Set(() => CommandParameter, ref _commandParameter, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
        public string Geometry { get; set; }
    }
}

