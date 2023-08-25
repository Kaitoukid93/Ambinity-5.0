using adrilight_shared.Enum;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ChasingPatternData;
using adrilight_shared.Models.ColorData;
using adrilight_shared.Models.ControlMode.Enum;
using adrilight_shared.Models.GifData;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

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
        public bool ShowDeleteButton { get => _showDeleteButton; set { Set(() => ShowDeleteButton, ref _showDeleteButton, value); } }
        public bool ShowMore { get => _showMore; set { Set(() => ShowMore, ref _showMore, value); } }
        [JsonIgnore]
        public string OnlineCatergory { get; set; }
        public void RefreshCollection()
        {
            RaisePropertyChanged(nameof(AvailableValues));
        }
        public void DisposeCollection()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            { AvailableValues.Clear(); });

        }
        public void DeletedSelectedItem(IParameterValue item)
        {
            ShowDeleteButton = false;
            AvailableValues.Remove(item);
            //LoadAvailableValues();
        }
        public void LoadAvailableValues()
        {
            ShowDeleteButton = false;
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
                    var t = config.DataType;
                    var m = config.MethodEnum;
                    var files = new string[1] { string.Empty };
                    if (Directory.Exists(Path.Combine(path, "collection")))
                        files = Directory.GetFiles(Path.Combine(path, "collection"));
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

                            switch (t)
                            {
                                case nameof(ColorPalette):
                                    OnlineCatergory = "Palette";
                                    foreach (var file in files)
                                    {
                                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            var colorPalette = DeserializeFromStream<ColorPalette>(stream);
                                            colorPalette.LocalPath = file;
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
                            switch (t)
                            {
                                case nameof(Gif):
                                    OnlineCatergory = "Gif";
                                    foreach (var file in files)
                                    {
                                        var data = new Gif()
                                        {
                                            Name = Path.GetFileName(file),
                                            Description = "Ambino Default Gif Collection",
                                            LocalPath = file
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
                                case nameof(ChasingPattern):
                                    OnlineCatergory = "Pattern";
                                    foreach (var file in files)
                                    {
                                        var data = new ChasingPattern()
                                        {
                                            Name = Path.GetFileName(file),
                                            Description = "xxx",
                                            Type = ChasingPatternTypeEnum.BlacknWhite,
                                            LocalPath = file

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

