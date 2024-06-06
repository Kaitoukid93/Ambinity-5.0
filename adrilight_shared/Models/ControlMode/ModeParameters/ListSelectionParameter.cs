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
        private int _minValue;
        private int _maxValue;
        private bool _showMore;
        private bool _showDeleteButton;
        private IParameterValue _selectedValue;
        private ObservableCollection<IParameterValue> _availableValues;
        private ObservableCollection<SubParameter> _subParams;
        private bool _isEnabled = true;
        private int _selectedDataSourceIndex;
        private adrilight_shared.Models.ItemsCollection.ItemsCollection _values;
        public List<string> DataSourceLocaFolderNames { get; set; }
        public int SelectedDataSourceIndex { get => _selectedDataSourceIndex; set { Set(() => SelectedDataSourceIndex, ref _selectedDataSourceIndex, value); LoadAvailableValues(); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        [JsonIgnore]
        public ObservableCollection<IParameterValue> AvailableValues { get => _availableValues; set { Set(() => AvailableValues, ref _availableValues, value); } }
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        [JsonIgnore]
        public string WarningMessage { get => _warningMessage; set { Set(() => WarningMessage, ref _warningMessage, value); } }
        [JsonIgnore]
        public bool WarningMessageVisible { get => _warningMessageVisible; set { Set(() => WarningMessageVisible, ref _warningMessageVisible, value); } }
        public IParameterValue SelectedValue { get => _selectedValue; set { Set(() => SelectedValue, ref _selectedValue, value); } }
        public ModeParameterTemplateEnum Template { get => _template; set { Set(() => Template, ref _template, value); } }
        public ModeParameterEnum ParamType { get => _paramType; set { Set(() => ParamType, ref _paramType, value); } }
        public ObservableCollection<SubParameter> SubParams { get => _subParams; set { Set(() => SubParams, ref _subParams, value); } }
        [JsonIgnore]
        public object Lock { get; } = new object();
        /// <summary>
        /// this is the min and max value of this parameter , use to set min or max value of the template (slider, nummeric updown
        /// </summary>
        public int MinValue { get => _minValue; set { Set(() => MinValue, ref _minValue, value); } }
        public int MaxValue { get => _maxValue; set { Set(() => MaxValue, ref _maxValue, value); } }
        [JsonIgnore]
        public bool ShowDeleteButton { get => _showDeleteButton; set { Set(() => ShowDeleteButton, ref _showDeleteButton, value); } }
        [JsonIgnore]
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value);OnShowMoreChanged(); } }
        [JsonIgnore]
        public adrilight_shared.Models.ItemsCollection.ItemsCollection Values { get => _values; set { Set(() => Values, ref _values, value); } }
        [JsonIgnore]
        public string OnlineCatergory { get; set; }
        private DeserializeMethodEnum _deserializeMethod;
        private void OnShowMoreChanged()
        {
            if(ShowMore)
            {
                Values = new adrilight_shared.Models.ItemsCollection.ItemsCollection();
                foreach(var item in AvailableValues)
                {
                    Values.AddItem(item);
                }
            }
            else
            {
                if(Values != null)
                Values.Items.Clear();
            }
        }
        public void DeletedSelectedItem(IParameterValue item)
        {
            ShowDeleteButton = false;
            AvailableValues.Remove(item);
            if (_deserializeMethod == DeserializeMethodEnum.SingleJson)
            {
                var path = Path.Combine(appFolder, dataSourceLocalFolderName);
                JsonHelpers.WriteSimpleJson(AvailableValues, System.IO.Path.Combine(path, "collection.json"));
            }
        }
        private string dataSourceLocalFolderName;
        public void LoadAvailableValues()
        {
            ShowDeleteButton = false;
            if (SelectedDataSourceIndex < 0 || SelectedDataSourceIndex > DataSourceLocaFolderNames.Count - 1)
            {
                SelectedDataSourceIndex = 0;
            }
            dataSourceLocalFolderName = DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var path = Path.Combine(appFolder, dataSourceLocalFolderName);

            try
            {
                lock (Lock)
                {
                    AvailableValues = new ObservableCollection<IParameterValue>();
                    var configJson = File.ReadAllText(Path.Combine(path, "config.json"));
                    var config = JsonConvert.DeserializeObject<ResourceLoaderConfig>(configJson);
                    var t = config.DataType;
                    var m = config.MethodEnum;
                    var files = new string[1] { string.Empty };
                    if (Directory.Exists(Path.Combine(path, "collection")))
                        files = Directory.GetFiles(Path.Combine(path, "collection"));
                    var infoPath = Path.Combine(path, "info");

                    switch (m)
                    {
                        case DeserializeMethodEnum.SingleJson:
                            _deserializeMethod = DeserializeMethodEnum.SingleJson;
                            var valuesJson = File.ReadAllText(Path.Combine(path, "collection.json"));
                            switch (t)
                            {
                                case nameof(ColorCard):
                                    JsonConvert.DeserializeObject<List<ColorCard>>(valuesJson).ForEach(c =>
                                    {
                                        AvailableValues.Add(c);
                                        c.PropertyChanged += (_, __) =>
                                        {
                                            if (__.PropertyName == nameof(c.IsChecked))
                                            {
                                                //show deletebutton
                                                ShowDeleteButton = AvailableValues.Where(c => (c as ColorCard).IsChecked).Count() > 0 ? true : false;
                                            }
                                        };

                                    });

                                    break;
                            }
                            break;
                        case DeserializeMethodEnum.MultiJson:
                            _deserializeMethod = DeserializeMethodEnum.MultiJson;
                            switch (t)
                            {
                                case nameof(ColorPalette):
                                    OnlineCatergory = "Colors";
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            var colorPalette = DeserializeFromStream<ColorPalette>(stream);
                                            colorPalette.LocalPath = file;
                                            colorPalette.InfoPath = Path.Combine(infoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                                            colorPalette.PropertyChanged += (_, __) =>
                                            {
                                                if (__.PropertyName == nameof(colorPalette.IsChecked))
                                                {
                                                    //show deletebutton
                                                    ShowDeleteButton = AvailableValues.Where(c => (c as ColorPalette).IsChecked).Count() > 0 ? true : false;
                                                }
                                            };
                                            AvailableValues.Add(colorPalette);
                                        }

                                    }
                                    break;
                                case nameof(VIDDataModel):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            var vidData = DeserializeFromStream<VIDDataModel>(stream);
                                            vidData.LocalPath = file;
                                            vidData.IsDeleteable = vidData.ExecutionType == VIDType.PositonGeneratedID ? false : true;
                                            vidData.InfoPath = Path.Combine(infoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                                            vidData.PropertyChanged += (_, __) =>
                                            {
                                                if (__.PropertyName == nameof(vidData.IsChecked))
                                                {
                                                    //show deletebutton
                                                    ShowDeleteButton = AvailableValues.Where(c => c.IsChecked).Count() > 0 ? true : false;
                                                }
                                            };
                                            AvailableValues.Add(vidData);
                                        }
                                    }
                                    break;
                                case nameof(MIDDataModel):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            var midData = DeserializeFromStream<MIDDataModel>(stream);
                                            midData.LocalPath = file;
                                            midData.InfoPath = Path.Combine(infoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                                            midData.PropertyChanged += (_, __) =>
                                            {
                                                if (__.PropertyName == nameof(midData.IsChecked))
                                                {
                                                    //show deletebutton
                                                    ShowDeleteButton = AvailableValues.Where(c => c.IsChecked).Count() > 0 ? true : false;
                                                }
                                            };
                                            AvailableValues.Add(midData);
                                        }
                                    }
                                    break;
                                case nameof(DancingModeParameterValue):
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            var dcData = DeserializeFromStream<DancingModeParameterValue>(stream);
                                            dcData.LocalPath = file;
                                            dcData.PropertyChanged += (_, __) =>
                                            {
                                                if (__.PropertyName == nameof(dcData.IsChecked))
                                                {
                                                    //show deletebutton
                                                    ShowDeleteButton = AvailableValues.Where(c => c.IsChecked).Count() > 0 ? true : false;
                                                }
                                            };
                                            AvailableValues.Add(dcData);
                                        }
                                    }
                                    break;

                            }
                            break;
                        case DeserializeMethodEnum.Files:
                            _deserializeMethod = DeserializeMethodEnum.Files;
                            switch (t)
                            {
                                case nameof(Gif):
                                    OnlineCatergory = "Gifs";
                                    foreach (var file in files)
                                    {
                                        var data = new Gif()
                                        {
                                            Name = Path.GetFileNameWithoutExtension(file),
                                            Description = "Ambino Default Gif Collection",
                                            LocalPath = file,
                                            InfoPath = Path.Combine(infoPath, Path.GetFileNameWithoutExtension(file) + ".info")
                                        };
                                        data.PropertyChanged += (_, __) =>
                                        {
                                            if (__.PropertyName == nameof(data.IsChecked))
                                            {
                                                //show deletebutton
                                                ShowDeleteButton = AvailableValues.Where(c => c.IsChecked).Count() > 0 ? true : false;
                                            }
                                        };
                                        //data.InitGif();
                                        AvailableValues.Add(data);
                                    }
                                    break;
                                case nameof(ChasingPattern):
                                    OnlineCatergory = "Animations";
                                    foreach (var file in files)
                                    {
                                        var data = new ChasingPattern()
                                        {
                                            Name = Path.GetFileNameWithoutExtension(file),
                                            Description = "xxx",
                                            Type = ChasingPatternTypeEnum.BlacknWhite,
                                            LocalPath = file,
                                            InfoPath = Path.Combine(infoPath, Path.GetFileNameWithoutExtension(file) + ".info")

                                        };
                                        data.PropertyChanged += (_, __) =>
                                        {
                                            if (__.PropertyName == nameof(data.IsChecked))
                                            {
                                                //show deletebutton
                                                ShowDeleteButton = AvailableValues.Where(c => c.IsChecked).Count() > 0 ? true : false;
                                            }
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
        public bool AddItemToCollection(IParameterValue item)
        {
            if (item.Name != null)
            {
                if (AvailableValues.Any(p => p.Name == item.Name))
                {
                    HandyControl.Controls.MessageBox.Show("File đã tồn tại, " + item.Name + " vui lòng chọn tên khác!", "file already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            AvailableValues.Insert(0, item);
            var dataSourceLocalFolderName = DataSourceLocaFolderNames[SelectedDataSourceIndex];
            var path = Path.Combine(appFolder, dataSourceLocalFolderName);

            try
            {
                var configJson = File.ReadAllText(Path.Combine(path, "config.json"));
                var config = JsonConvert.DeserializeObject<ResourceLoaderConfig>(configJson);
                var t = config.DataType;
                var m = config.MethodEnum;
                var collectionFolderPath = Path.Combine(path, "collection");
                switch (m)
                {
                    case DeserializeMethodEnum.SingleJson:
                        JsonHelpers.WriteSimpleJson(AvailableValues, Path.Combine(path, "collection.json"));
                        //SelectedValueIndex = 0;
                        break;
                    case DeserializeMethodEnum.MultiJson:
                        if (item is ColorPalette)
                        {
                            JsonHelpers.WriteSimpleJson(item, Path.Combine(collectionFolderPath, item.Name + ".col"));
                        }
                        if (item is VIDDataModel)
                        {
                            JsonHelpers.WriteSimpleJson(item, Path.Combine(collectionFolderPath, item.Name + ".json"));
                        }
                        break;
                    case DeserializeMethodEnum.Files:
                        if (item is Gif)
                        {
                            var gif = item as Gif;
                            File.Copy(gif.LocalPath, Path.Combine(collectionFolderPath, item.Name));
                        }
                        else if (item is ChasingPattern)
                        {
                            var pattern = item as ChasingPattern;
                            File.Copy(pattern.LocalPath, Path.Combine(collectionFolderPath, item.Name));
                        }
                        break;
                }
            }
            catch { return false; }
            return true;
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
        public void Localize(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

