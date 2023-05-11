using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace adrilight.Util.ModeParameters
{
    public class ListSelectectionParameter : ViewModelBase, IModeParameter
    {
        public ListSelectectionParameter()
        {
         
        }
        public ListSelectectionParameter(ModeParameterEnum paramType)
        {
            Template = ModeParameterTemplateEnum.ListSelection;
            DataSources = new ObservableCollection<DataSource>();
            ParamType = paramType;
        }
        private string _name;
        private string _description;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private int _minValue;
        private int _maxValue;
        private bool _showMore;
        private IParameterValue _selectedValue;
        private ObservableCollection<SubParameter> _subParams;
        private ObservableCollection<DataSource> _dataSources;
        private bool _isEnabled = true;
        private int _selectedValueIndex;
        private int _selectedDataSourceIndex;
        public int SelectedDataSourceIndex { get => _selectedDataSourceIndex; set { Set(() => SelectedDataSourceIndex, ref _selectedDataSourceIndex, value); } }
        public int SelectedValueIndex { get => _selectedValueIndex; set { Set(() => SelectedValueIndex, ref _selectedValueIndex, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public ObservableCollection<DataSource> DataSources { get => _dataSources; set { Set(() => DataSources, ref _dataSources, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public IParameterValue SelectedValue { get => _selectedValue; set { Set(() => SelectedValue, ref _selectedValue, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
    }
}

