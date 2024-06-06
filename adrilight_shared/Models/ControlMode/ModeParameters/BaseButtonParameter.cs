using adrilight_shared.Enums;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class BaseButtonParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {

        private string _name;
        private string _description;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
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
        public virtual PreviewableContent PreviewContent { get; set; }
        public string Geometry { get; set; }
    }
}

