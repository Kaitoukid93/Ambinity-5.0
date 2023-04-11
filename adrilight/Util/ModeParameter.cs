using adrilight.Settings;
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
        private ObservableCollection<object> _availableValue;
        private ObservableCollection<SubParameter> _subParams;
        private string _availableValueLocalPath;
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
        public ObservableCollection<IPrameterValue> AvailableValue => LoadAvailableValue(AvailableValueLocalPath);
        /// <summary>
        /// this is the type of lighting mode, use to get the data template
        /// </summary>
        /// 
        public string AvailableValueLocalPath { get => _availableValueLocalPath; set { Set(() => AvailableValueLocalPath, ref _availableValueLocalPath, value); } }
        [JsonIgnore]
        public IPrameterValue SelectedValue => Value > AvailableValue.Count - 1 || Value < 0 ? AvailableValue[0] : AvailableValue[Value];
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }

        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public void RefreshCollection()
        {
            RaisePropertyChanged(nameof(AvailableValue));
        }
        private ObservableCollection<IPrameterValue> LoadAvailableValue(string availableValueLocalPath)
        {
            var availableValue = new ObservableCollection<IPrameterValue>();
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