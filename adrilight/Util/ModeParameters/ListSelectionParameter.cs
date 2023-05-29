using adrilight.Helpers;
using adrilight.Settings;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace adrilight.Util.ModeParameters
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
        private ModeParameterTemplateEnum _template;
        private ModeParameterEnum _paramType;
        private int _minValue;
        private int _maxValue;
        private bool _showMore;
        private IParameterValue _selectedValue;
        private ObservableCollection<IParameterValue> _availableValues;
        private ObservableCollection<SubParameter> _subParams;
        private bool _isEnabled = true;
        private int _selectedDataSourceIndex;
        public List<string> DataSourceLocaFolderNames { get; set; }
        public int SelectedDataSourceIndex { get => _selectedDataSourceIndex; set { Set(() => SelectedDataSourceIndex, ref _selectedDataSourceIndex, value); LoadAvailableValues(); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        [JsonIgnore]
        public ObservableCollection<IParameterValue> AvailableValues { get => _availableValues; set { Set(() => AvailableValues, ref _availableValues, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public IParameterValue SelectedValue { get => _selectedValue; set { Set(() => SelectedValue, ref _selectedValue, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }
        public object Lock { get; } = new object();
        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
        public void RefreshCollection()
        {
            RaisePropertyChanged(nameof(AvailableValues));
        }
        public void DisposeCollection()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            { AvailableValues.Clear(); });

        }
        public void LoadAvailableValues()
        {
            if (SelectedDataSourceIndex < 0 || SelectedDataSourceIndex > DataSourceLocaFolderNames.Count - 1)
            {
                SelectedDataSourceIndex = 0;
            }
            var dataSourceLocalFolderName = DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var path = Path.Combine(appFolder, dataSourceLocalFolderName);

            try
            {
                lock (Lock)
                {
                    AvailableValues = new ObservableCollection<IParameterValue>();
                    var configJson = File.ReadAllText(Path.Combine(path, "config.json"));
                    var config = JsonConvert.DeserializeObject<ResourceLoaderConfig>(configJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    string t = config.DataType;
                    var m = config.MethodEnum;
                    switch (m)
                    {
                        case DeserializeMethodEnum.SingleJson:
                            var valuesJson = File.ReadAllText(Path.Combine(path, "collection.json"));
                            switch (t)
                            {
                                case nameof(ColorCard):
                                    JsonConvert.DeserializeObject<List<ColorCard>>(valuesJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }).ForEach(c => AvailableValues.Add(c));
                                    break;
                            }
                            break;
                        case DeserializeMethodEnum.MultiJson:
                            string[] files = Directory.GetFiles(Path.Combine(path, "collection"));
                            switch (t)
                            {
                                case nameof(ColorPalette):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {

                                            AvailableValues.Add(DeserializeFromStream<ColorPalette>(stream));
                                        }

                                    }
                                    break;
                                case nameof(VIDDataModel):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {

                                            AvailableValues.Add(DeserializeFromStream<VIDDataModel>(stream));
                                        }
                                    }
                                    break;
                                case nameof(MIDDataModel):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {

                                            AvailableValues.Add(DeserializeFromStream<MIDDataModel>(stream));
                                        }
                                    }
                                    break;
                                case nameof(DancingModeParameterValue):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {

                                            AvailableValues.Add(DeserializeFromStream<DancingModeParameterValue>(stream));
                                        }
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
                                        AvailableValues.Add(data);
                                    }
                                    break;
                            }
                            break;


                    }

                }
            }
            catch (Exception ex)
            {
                //something wronf return null
                // return null;
            }

        }
        public void AddItemToCollection(IParameterValue item)
        {
            if (item.Name != null)
            {
                if (AvailableValues.Any(p => p.Name == item.Name))
                {
                    HandyControl.Controls.MessageBox.Show("File đã tồn tại, " + item.Name + "vui lòng chọn tên khác!", "file already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            AvailableValues.Insert(0, item);
            var dataSourceLocalFolderName = DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var path = Path.Combine(appFolder, dataSourceLocalFolderName);

            try
            {
                var configJson = File.ReadAllText(Path.Combine(path, "config.json"));
                var config = JsonConvert.DeserializeObject<ResourceLoaderConfig>(configJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                string t = config.DataType;
                var m = config.MethodEnum;
                switch (m)
                {
                    case DeserializeMethodEnum.SingleJson:
                        JsonHelpers.WriteSimpleJson(AvailableValues, Path.Combine(path, "collection.json"));
                        //SelectedValueIndex = 0;
                        break;
                    case DeserializeMethodEnum.MultiJson:

                        var collectionFolderPath = Path.Combine(path, "collection");
                        JsonHelpers.WriteSimpleJson(item, Path.Combine(collectionFolderPath, item.Name + ".col"));
                        break;
                }
            }
            catch { }
        }
        private static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}

