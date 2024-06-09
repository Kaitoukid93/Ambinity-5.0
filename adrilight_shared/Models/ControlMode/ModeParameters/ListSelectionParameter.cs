using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ColorPalette = adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues.ColorPalette;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class ListSelectionParameter : ViewModelBase, IModeParameter
    {
        private static string appFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        public ListSelectionParameter()
        {

        }
        public ListSelectionParameter(ModeParameterEnum paramType)
        {
            Template = ModeParameterTemplateEnum.ListSelection;
            ParamType = paramType;
        }
        private string _name;
        private string _description;
        private string _warningMessage;
        private bool _warningMessageVisible;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private IParameterValue _selectedValue;
        private ObservableCollection<SubParameter> _subParams;
        private bool _isEnabled = true;
        private adrilight_shared.Models.ItemsCollection.ItemsCollection _values;
        public List<string> DataSourceLocaFolderNames { get; set; }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public IParameterValue SelectedValue { get => _selectedValue; set { Set(() => SelectedValue, ref _selectedValue, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }
        public bool WarningMessageVisible { get => _warningMessageVisible; set { Set(() => WarningMessageVisible, ref _warningMessageVisible, value); } }
        public string WarningMessage { get => _warningMessage; set { Set(() => WarningMessage, ref _warningMessage, value); } }
        [JsonIgnore]
        public object Lock { get; } = new object();
        public void Localize(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

