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

namespace adrilight.Util
{
    public class ModeParameter : ViewModelBase, IModeParameter // parameter specific for lighting control
    {
        private string _name;
        private string _description;
        private int _value;
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private int _minValue;
        private int _maxValue;
        private bool _showMore;
        private ObservableCollection<object> _availableValue;
        private ObservableCollection<SubParameter> _subParams;
        private List<SelectableLocalPath> _availableValueLocalPaths;
        private int _selectedValueLocalPathIndex;
        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        /// <summary>
        /// provide description of this parameter(turn on/off something,set something)
        /// </summary>
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or index in the list item
        /// </summary>
        public int Value { get => _value; set { Set(() => Value, ref _value, value); RaisePropertyChanged(nameof(SelectedValue)); } }
        /// <summary>
        /// this value could be bool(on/off) or nummeric(int) or even listof item
        /// </summary>
        /// 
        [JsonIgnore]
        public ObservableCollection<IParameterValue> AvailableValue => LoadAvailableValue(SelectedValueLocalPath.Path);
        /// <summary>
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        /// 
        public List<SelectableLocalPath> AvailableValueLocalPaths { get => _availableValueLocalPaths; set { Set(() => AvailableValueLocalPaths, ref _availableValueLocalPaths, value); } }
        [JsonIgnore]
        public SelectableLocalPath SelectedValueLocalPath => SelectedValueLocalPathIndex > AvailableValueLocalPaths.Count - 1 || SelectedValueLocalPathIndex < 0 ? AvailableValueLocalPaths[0] : AvailableValueLocalPaths[SelectedValueLocalPathIndex];
        public int SelectedValueLocalPathIndex { get => _selectedValueLocalPathIndex; set { Set(() => SelectedValueLocalPathIndex, ref _selectedValueLocalPathIndex, value); RaisePropertyChanged(nameof(SelectedValueLocalPath)); RaisePropertyChanged(nameof(AvailableValue)); RaisePropertyChanged(nameof(SelectedValue)); } }
        [JsonIgnore]
        public IParameterValue SelectedValue => Value > AvailableValue.Count - 1 || Value < 0 ? AvailableValue[0] : AvailableValue[Value];
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
        public void RefreshCollection()
        {
            RaisePropertyChanged(nameof(AvailableValue));
        }
        private ObservableCollection<IParameterValue> LoadAvailableValue(string availableValueLocalPath)
        {
            var availableValue = new ObservableCollection<IParameterValue>();
            try
            {
                var configJson = File.ReadAllText(Path.Combine(availableValueLocalPath, "config.json"));
                var config = JsonConvert.DeserializeObject<ResourceLoaderConfig>(configJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                string t = config.DataType;
                var m = config.MethodEnum;
                switch (m)
                {
                    case DeserializeMethodEnum.SingleJson:
                        var valuesJson = File.ReadAllText(Path.Combine(availableValueLocalPath, "collection.json"));
                        switch (t)
                        {
                            case nameof(ColorCard):
                                JsonConvert.DeserializeObject<List<ColorCard>>(valuesJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }).ForEach(c => availableValue.Add(c));
                                break;
                        }
                        break;
                    case DeserializeMethodEnum.MultiJson:
                        string[] files = Directory.GetFiles(Path.Combine(availableValueLocalPath, "collection"));
                        switch (t)
                        {
                            case nameof(ColorPalette):
                                foreach (var file in files)
                                {
                                    var json = File.ReadAllText(file);

                                    var data = JsonConvert.DeserializeObject<ColorPalette>(json);

                                    availableValue.Add(data);
                                }
                                break;
                            case nameof(VIDDataModel):
                                foreach (var file in files)
                                {
                                    var json = File.ReadAllText(file);

                                    var data = JsonConvert.DeserializeObject<VIDDataModel>(json);

                                    availableValue.Add(data);
                                }
                                break;
                            case nameof(MIDDataModel):
                                foreach (var file in files)
                                {
                                    var json = File.ReadAllText(file);

                                    var data = JsonConvert.DeserializeObject<MIDDataModel>(json);

                                    availableValue.Add(data);
                                }
                                break;
                            case nameof(DancingModeParameterValue):
                                foreach (var file in files)
                                {
                                    var json = File.ReadAllText(file);

                                    var data = JsonConvert.DeserializeObject<DancingModeParameterValue>(json);

                                    availableValue.Add(data);
                                }
                                break;
                            case nameof(ChasingPattern):
                                foreach (var file in files)
                                {
                                    var data = new ChasingPattern() {
                                        Name = Path.GetFileName(file),
                                        Description = "xxx",
                                        Type = ChasingPatternTypeEnum.BlacknWhite,
                                        Path = file

                                    };
                                    availableValue.Add(data);
                                }
                                break;
                        }
                        break;


                }
            }
            catch (Exception ex)
            {
                //something wronf return null
                return null;
            }
            return availableValue;
        }

    }
}

