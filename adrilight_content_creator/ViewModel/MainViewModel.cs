//using adrilight.Helpers;
//using adrilight.Settings;
//using adrilight.Spots;
using adrilight_content_creator.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.Store;
using FTPServer;
//using adrilight_effect_analyzer.Model;
using Newtonsoft.Json;
using Renci.SshNet;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
//using static adrilight.ViewModel.MainViewViewModel;
using File = System.IO.File;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace adrilight_content_creator.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private const double UPDATE_FRAME_RATE = 60.0;
        private readonly DispatcherTimer _timer;
        private int _frameCounter = 0;
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");


        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string GifsCollectionFolderPath => Path.Combine(JsonPath, "Gifs");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string ProfileCollectionFolderPath => Path.Combine(JsonPath, "Profiles");
        public MainViewModel(IList<ISelectableViewPart> selectableViewParts)
        {
            SetupCommand();
            if (selectableViewParts == null)
            {
                throw new ArgumentNullException(nameof(selectableViewParts));
            }
            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order)
                .ToList();
            SelectedViewPart = SelectableViewParts.First();
            CanvasItems = new ObservableCollection<IDrawable>();
            CanvasSelectedItems = new ObservableCollection<IDrawable>();
            CanvasSelectedItems.CollectionChanged += SelectedItemsChanged;
            AvailableDeviceTypes = new ObservableCollection<SlaveDeviceTypeEnum>();
            foreach (SlaveDeviceTypeEnum type in Enum.GetValues(typeof(SlaveDeviceTypeEnum)))
            {
                AvailableDeviceTypes.Add(type);
            }
            AvailableMasterDeviceTypes = new ObservableCollection<DeviceTypeEnum>();
            foreach (DeviceTypeEnum type in Enum.GetValues(typeof(DeviceTypeEnum)))
            {
                AvailableMasterDeviceTypes.Add(type);
            }
            AvailableConnectionTypes = new ObservableCollection<DeviceConnectionTypeEnum>();
            foreach (DeviceConnectionTypeEnum type in Enum.GetValues(typeof(DeviceConnectionTypeEnum)))
            {
                AvailableConnectionTypes.Add(type);
            }
            _timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(1000.0 / UPDATE_FRAME_RATE) };
        }
        public ICommand SetSelectedOutputSlaveDeviceFromFileCommand { get; set; }
        public ICommand SaveCarouselToFileCommand { get; set; }
        public ICommand OpenURLCommand { get; set; }
        public ICommand RefreshExistedDeviceCollectionCommand { get; set; }
        public ICommand SaveCurrentDeviceToFilesCommand { get; set; }
        public ICommand ChageCurrentDeviceSourceCommand { get; set; }
        public ICommand OpenImageSelectionForCurrentDeviceCommand { get; set; }
        public ICommand AddNewDeviceCommand { get; set; }
        public ICommand OpenAddNewDeviceWindowCommand { get; set; }
        public ICommand OpenAddNewDrawableItemCommand { get; set; }
        public ICommand SaveDeviceDataCommand { get; set; }
        public ICommand ApplyDeviceActualDimensionCommand { get; set; }
        public ICommand AddImageToPIDCanvasCommand { get; set; }
        public ICommand AddItemsToPIDCanvasCommand { get; set; }
        public ICommand ChangeSelectedSpotSizeCommand { get; set; }
        public ICommand OpenChangeSpotSizeWindowCommand { get; set; }
        public ICommand AddSpotLayoutCommand { get; set; }
        public ICommand AddSpotGeometryCommand { get; set; }
        public ICommand ImportSVGCommand { get; set; }
        public ICommand ClearPIDCanvasCommand { get; set; }
        public ICommand DeleteSelectedItemsCommand { get; set; }
        public ICommand CombineSelectedSpotsCommand { get; set; }
        public ICommand AddNewZoneCommand { get; set; }
        public ICommand SelectPIDCanvasItemCommand { get; set; }
        public ICommand ApplyCarouselItemListCommand { get; set; }
        public ICommand AddURLToListCommand { get; set; }
        public ICommand OpenAddURLToListWindowCommand { get; set; }
        public ICommand ClearURLListCommand { get; set; }
        public ICommand DeleteSelectedURLCommand { get; set; }

        public ICommand SelectFrameDataFolderCommand { get; set; }
        public ICommand PlayPauseCurrentMotion { get; set; }
        public ICommand ExportCurrentLayerCommand { get; set; }

        public ICommand SaveFilterToFileCommand { get; set; }

        private ObservableCollection<IDeviceSettings> _availableDevices;
        public ObservableCollection<IDeviceSettings> AvailableDevices
        {
            get { return _availableDevices; }
            set
            {
                _availableDevices = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IDeviceSettings> _availableAddedDevices;
        public ObservableCollection<IDeviceSettings> AvailableAddedDevices
        {
            get { return _availableAddedDevices; }
            set
            {
                _availableAddedDevices = value;
                RaisePropertyChanged();
            }
        }
        private string _selectedDeviceThumbnail;
        public string SelectedDeviceThumbnail
        {
            get { return _selectedDeviceThumbnail; }
            set
            {
                _selectedDeviceThumbnail = value;
                RaisePropertyChanged();
            }
        }
        private IDeviceSettings _selectedDevice;
        public IDeviceSettings SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                RaisePropertyChanged();
            }
        }
        private bool _canSelectMultipleItems;
        public bool CanSelectMultipleItems
        {
            get { return _canSelectMultipleItems; }
            set
            {
                _canSelectMultipleItems = value;
            }
        }
        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CanvasSelectedItem = CanvasSelectedItems.Count == 1 ? CanvasSelectedItems[0] : null;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (CanvasSelectedItems.Count == 0 || CanvasSelectedItems.Count > 1)
                {
                    CanvasSelectedItem = null;
                }
            }

        }
        public void SetupCommand()
        {
            RefreshExistedDeviceCollectionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingDevice = true;
                var devices = await Task.Run(() => LoadDeviceIfExists());
                AvailableDevices = new ObservableCollection<IDeviceSettings>();

                foreach (var device in devices)
                {
                    AvailableDevices.Add(device);
                }
                IsLoadingDevice = false;


            }
          );
            SaveCurrentDeviceToFilesCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SaveDeviceToFile();

            }
          );
            SaveFilterToFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SaveFilterToFile();

            }
       );
            SaveCarouselToFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SaveCarouselToFile();

            }
        );
            SetSelectedOutputSlaveDeviceFromFileCommand = new RelayCommand<int>((p) =>
            {
                return true;
            }, (p) =>
            {

                SetSelectedOutputSlaveDeviceFromFile(p);

            }
          );
            AddURLToListCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddURLToList(p);

            }
          );
            ApplyCarouselItemListCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ApplycarouselItemList();

            }
          );
            ClearURLListCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                CarouselURLs?.Clear();

            }
          );
            DeleteSelectedURLCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                CarouselURLs?.Remove(p);

            }
         );
            OpenAddURLToListWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                var addStringWindow = new AddNewStringWindow();
                addStringWindow.Owner = System.Windows.Application.Current.MainWindow;
                addStringWindow.ShowDialog();

            }
          );
            OpenURLCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                System.Diagnostics.Process.Start(p);

            }
          );
            ApplyDeviceActualDimensionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ApplyDeviceActualDimension();

            }
           );
            AddNewDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddNewDefaultDevice(NewDeviceOutputCount);

            }
           );
            OpenAddNewDeviceWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var newDeviceWindow = new NewDeviceParametersWindow();
                newDeviceWindow.Owner = System.Windows.Application.Current.MainWindow;
                newDeviceWindow.ShowDialog();
            }
           );
            SaveDeviceDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SaveDeviceData();

            }
           );
            OpenImageSelectionForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SelectDeviceImage();

            }
         );

            ChageCurrentDeviceSourceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SelectedDevice = null;

            }
         );
            ImportSVGCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, (p) =>
                {

                    ImportSVG();

                }
              );

            SelectPIDCanvasItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p is LEDSetup)
                {
                    var zone = p as LEDSetup;
                    foreach (var spot in zone.Spots)
                    {
                        spot.SetColor(0, 0, 255, true);
                    }
                }
            });
            AddImageToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddImage();

            }
           );
            ClearPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ClearPIDCanvas();

            }
          );
            AddSpotLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddSpotLayout();

            }
           );
            AddNewZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddNewZone();
            });
            CombineSelectedSpotsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CombineSelectedSpots();
            });
            DeleteSelectedItemsCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {

                DeleteSelectedItems(p);

            }
            );

            ChangeSelectedSpotSizeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                SetSpotSize();

            }
         );
            OpenChangeSpotSizeWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenChangeSpotSizeWindow();

            }
       );
            SelectFrameDataFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SelectFrameDataFolder();
            }
       );
            PlayPauseCurrentMotion = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                _timer.Start();
                _timer.Tick += TimerOnTick;
            }

       );
            ExportCurrentLayerCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentLayer();
            }

       );
        }
        private double _selectionRectangleStrokeThickness = 2.0;
        public double SelectionRectangleStrokeThickness
        {
            get { return _selectionRectangleStrokeThickness; }
            set
            {
                _selectionRectangleStrokeThickness = value;
                RaisePropertyChanged();
            }
        }
        private Thickness _canvasItemBorder = new Thickness(2.0);
        public Thickness CanvasItemBorder
        {
            get { return _canvasItemBorder; }
            set
            {
                _canvasItemBorder = value;
                RaisePropertyChanged();
            }
        }
        private double _canvasScale = 1.0;
        public double CanvasScale
        {
            get { return _canvasScale; }
            set
            {
                _canvasScale = value;
                CalculateItemsBorderThickness(value);
                RaisePropertyChanged();
            }
        }
        private void CalculateItemsBorderThickness(double scaleValue) // this keeps items border remain the same 2px anytime
        {
            SelectionRectangleStrokeThickness = 2 / scaleValue;
            CanvasItemBorder = new Thickness(SelectionRectangleStrokeThickness);
        }
        public IList<ISelectableViewPart> SelectableViewParts { get; }
        public ISelectableViewPart _selectedViewPart;
        public ISelectableViewPart SelectedViewPart
        {
            get => _selectedViewPart;
            set
            {
                Set(ref _selectedViewPart, value);
                if (value.ViewPartName == "Device+")
                {
                    //reload existed device
                    RefreshExistedDeviceCollectionCommand.Execute("refresh");

                }

            }
        }
        #region HomePage+ Viewmodel Region
        private string _carouselDescription;
        public string CarouselDescription
        {
            get { return _carouselDescription; }
            set
            {
                _carouselDescription = value;
                RaisePropertyChanged();
            }
        }
        private string _carouselName;
        public string CarouselName
        {
            get { return _carouselName; }
            set
            {
                _carouselName = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _carouselURLs;
        public ObservableCollection<string> CarouselURLs
        {
            get
            {
                return _carouselURLs;
            }
            set
            {
                _carouselURLs = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<OnlineItemModel> _carouselOnlineItems;
        public ObservableCollection<OnlineItemModel> CarouselOnlineItems
        {
            get
            {
                return _carouselOnlineItems;
            }
            set
            {
                _carouselOnlineItems = value;
                RaisePropertyChanged();
            }
        }
        private void AddURLToList(string url)
        {
            if (CarouselURLs == null)
            {
                CarouselURLs = new ObservableCollection<string>();
            }
            CarouselURLs.Add(url);
        }
        private FTPServerHelpers FTPHlprs { get; set; }
        private void SFTPConnect()
        {
            if (FTPHlprs == null)
                return;
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    FTPHlprs.sFTP.Connect();
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show("Adrilight Server không khả dụng ở thời điểm hiện tại", "Server notfound", MessageBoxButton.OK, MessageBoxImage.Error);
                    //return;
                }
            }
        }
        private void SFTPInit()
        {
            if (FTPHlprs != null && FTPHlprs.sFTP.IsConnected)
            {
                FTPHlprs.sFTP.Disconnect();
            }
            string host = @"103.148.57.184";
            string public_User_LoginName = "adrilight_publicuser";
            string public_User_PassWord = "@drilightPublic";
            FTPHlprs = new FTPServerHelpers();
            FTPHlprs.sFTP = new SftpClient(host, 1512, public_User_LoginName, public_User_PassWord);
        }

        private string GetInfoPath(OnlineItemModel item)
        {
            string localPath = string.Empty;
            switch (item.Type)
            {
                case "ARGBLEDSlaveDevice":
                    localPath = SupportedDeviceCollectionFolderPath;
                    break;
                case "Gif":
                    localPath = GifsCollectionFolderPath;
                    break;
                case "ColorPalette":
                    localPath = PalettesCollectionFolderPath;
                    break;
                case "ChasingPattern":
                    localPath = ChasingPatternsCollectionFolderPath;
                    break;
            }
            var itemLocalInfo = Path.Combine(localPath, "info", item.Name + ".info");
            return itemLocalInfo;
        }
        private int _templateSelector;
        public int TemplateSelector
        {
            get { return _templateSelector; }
            set
            {
                _templateSelector = value;
                RaisePropertyChanged();
            }
        }
        private async void ApplycarouselItemList()
        {
            //download items
            CarouselOnlineItems = new ObservableCollection<OnlineItemModel>();
            if (CarouselURLs == null)
                return;
            if (FTPHlprs == null)
            {
                SFTPInit();
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    return;
                }

            }
            foreach (var url in CarouselURLs)
            {

                //get name
                var itemName = FTPHlprs.GetFileOrFoldername(url).Name;
                //get description content
                var descriptionPath = url + "/description.md";
                var description = await FTPHlprs.GetStringContent(descriptionPath);
                var itemList = new List<OnlineItemModel>();
                var infoPath = url + "/info.json";
                var info = FTPHlprs.GetFiles<OnlineItemModel>(infoPath).Result;
                info.Path = url;
                var itemLocalInfo = GetInfoPath(info);
                if (File.Exists(itemLocalInfo))
                {
                    info.IsLocalExisted = true;
                    var json = File.ReadAllText(itemLocalInfo);
                    var localInfo = JsonConvert.DeserializeObject<OnlineItemModel>(json);
                    var v1 = new Version(localInfo.Version);
                    var v2 = new Version(info.Version);
                    if (v2 > v1)
                        info.IsUpgradeAvailable = true;
                }
                lock (CarouselOnlineItems)
                    CarouselOnlineItems.Add(info);
            }
            lock (CarouselOnlineItems)
            {
                foreach (var item in CarouselOnlineItems)
                {
                    var thumbPath = item.Path + "/thumb.png";
                    item.Thumb = FTPHlprs.GetThumb(thumbPath).Result;
                }
            }
        }
        public int CarouselOrder { get; set; }
        private void SaveCarouselToFile()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.FileName = CarouselName;
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            if (Export.ShowDialog() == DialogResult.OK)
            {
                //deserialize to config
                Directory.CreateDirectory(Export.FileName);
                var carousel = new HomePageCarouselItem()
                {
                    Name = CarouselName,
                    Description = CarouselDescription,
                    Order = CarouselOrder,
                    EmbeddedURL = CarouselURLs?.ToList()
                };
                var configjson = JsonConvert.SerializeObject(carousel);
                File.WriteAllText(Path.Combine(Export.FileName, "config.json"), configjson);
            }
            //export

        }
        #endregion
        #region Device+ Viewmodel region
        public int NewDeviceOutputCount { get; set; }
        private bool _isLoadingDevice;

        public bool IsLoadingDevice
        {
            get
            {
                return _isLoadingDevice;
            }
            set
            {
                _isLoadingDevice = value;
                RaisePropertyChanged();
            }
        }
        private void SetSelectedOutputSlaveDeviceFromFile(int output)
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "json";
            Import.Filter = "All files (*.*)|*.*";
            Import.FilterIndex = 1;
            Import.ShowDialog();
            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                try
                {
                    var dev = JsonConvert.DeserializeObject<ARGBLEDSlaveDevice>(json);
                    SelectedDevice.AvailableLightingOutputs[output].SlaveDevice = dev;
                }
                catch (Exception)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "File Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }
        private void SaveFilterToFile()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.FileName = "filters.json";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            if (Export.ShowDialog() == DialogResult.OK)
            {
                //deserialize to config
                var filters = new List<StoreFilterModel>();
                foreach (var txt in CarouselURLs)
                {
                    var filter = new StoreFilterModel()
                    {
                        Name = txt,
                        CatergoryFilter = null,
                        NameFilter = txt
                    };
                    filters.Add(filter);
                }
                var configjson = JsonConvert.SerializeObject(filters);
                File.WriteAllText(Export.FileName, configjson);

            }

        }
        private void SaveDeviceToFile()
        {
            SelectedDevice.IsLoadingProfile = true;
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.FileName = SelectedDevice.DeviceName;
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            if (Export.ShowDialog() == DialogResult.OK)
            {
                //deserialize to config
                Directory.CreateDirectory(Export.FileName);
                var configjson = JsonConvert.SerializeObject(SelectedDevice);
                File.WriteAllText(Path.Combine(Export.FileName, "config.json"), configjson);

            }
            //copy thumbnail
            if (File.Exists(SelectedDeviceThumbnail))
                File.Copy(SelectedDeviceThumbnail, Path.Combine(Export.FileName, "thumbnail.png"));
            //copy required slave device folder (config + thumbnail)
            var requiredSlaveDevice = new List<string>();
            foreach (ARGBLEDSlaveDevice device in SelectedDevice.AvailableLightingDevices)
            {
                if (requiredSlaveDevice.Any(p => p == device.Name))
                    continue;
                requiredSlaveDevice.Add(device.Name);
                var requiredSlaveDevicejson = JsonConvert.SerializeObject(device);
                Directory.CreateDirectory(Path.Combine(Export.FileName, "dependencies", "SlaveDevices", device.Name));
                File.WriteAllText(Path.Combine(Export.FileName, "dependencies", "SlaveDevices", device.Name, "config.json"), requiredSlaveDevicejson);
                File.Copy(device.Thumbnail, Path.Combine(Export.FileName, "dependencies", "SlaveDevices", device.Name, Path.GetFileName(device.Thumbnail)));
                File.Copy(device.ThumbnailWithColor, Path.Combine(Export.FileName, "dependencies", "SlaveDevices", device.Name, Path.GetFileName(device.ThumbnailWithColor)));
            }

            //export
        }
        private void AddNewDefaultDevice(int outputCount)
        {
            if (AvailableAddedDevices == null)
                AvailableAddedDevices = new ObservableCollection<IDeviceSettings>();
            if (outputCount <= 0 || outputCount > 20)
                return;
            var newDevice = new SlaveDeviceHelpers().DefaultCreatedGenericDevice(
                                new DeviceType(DeviceTypeEnum.Unknown),
                                "Change name",
                                "Không có",
                                false,
                                true,
                                outputCount);
            newDevice.DashboardWidth = 230;
            newDevice.DashboardHeight = 270;
            newDevice.UpdateChildSize();
            AvailableAddedDevices.Add(newDevice);
        }
        public Task<List<DeviceSettings>> LoadDeviceIfExists()
        {
            var devices = new List<DeviceSettings>();
            if (!Directory.Exists(DevicesCollectionFolderPath)) return Task.FromResult(devices); ; // no device has been added

            foreach (var folder in Directory.GetDirectories(DevicesCollectionFolderPath))
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(folder, "config.json"));
                    var device = JsonConvert.DeserializeObject<DeviceSettings>(json);
                    device.AvailableControllers = new List<IDeviceController>();
                    //read slave device info
                    //check if this device contains lighting controller
                    var lightingoutputDir = Path.Combine(Path.Combine(folder, "LightingOutputs"));
                    var pwmoutputDir = Path.Combine(Path.Combine(folder, "PWMOutputs"));
                    DeserializeChild<ARGBLEDSlaveDevice>(lightingoutputDir, device, OutputTypeEnum.ARGBLEDOutput);
                    DeserializeChild<PWMMotorSlaveDevice>(pwmoutputDir, device, OutputTypeEnum.PWMOutput);
                    devices.Add(device);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return Task.FromResult(devices);
        }
        private void DeserializeChild<T>(string outputDir, IDeviceSettings device, OutputTypeEnum outputType)
        {
            if (Directory.Exists(outputDir))
            {
                //add controller to this device

                var controller = new DeviceController();
                switch (outputType)
                {
                    case (OutputTypeEnum.PWMOutput):
                        controller.Geometry = "fanSpeedController";
                        controller.Name = "Fan";
                        controller.Type = ControllerTypeEnum.PWMController;
                        break;
                    case (OutputTypeEnum.ARGBLEDOutput):
                        controller.Geometry = "brightness";
                        controller.Name = "Lighting";
                        controller.Type = ControllerTypeEnum.LightingController;
                        break;
                }


                foreach (var subfolder in Directory.GetDirectories(outputDir)) // each subfolder contains 1 slave device
                {
                    //read slave device info
                    var outputJson = File.ReadAllText(Path.Combine(subfolder, "config.json"));
                    var output = JsonConvert.DeserializeObject<OutputSettings>(outputJson);
                    var slaveDeviceJson = File.ReadAllText(Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "config.json"));
                    var slaveDevice = JsonConvert.DeserializeObject<T>(slaveDeviceJson);

                    if (slaveDevice == null)//somehow data corrupted
                        continue;
                    else
                    {
                        if (!File.Exists((slaveDevice as ISlaveDevice).Thumbnail))
                        {
                            //(slaveDevice as ISlaveDevice).Thumbnail = Path.Combine(Directory.GetDirectories(subfolder).FirstOrDefault(), "thumbnail.png");
                        }
                    }


                    output.SlaveDevice = slaveDevice as ISlaveDevice;
                    controller.Outputs.Add(output);
                    //each slave device attach to one output so we need to create output
                    //lightin

                }
                device.AvailableControllers.Add(controller);
            }
        }
        private void SelectDeviceImage()
        {
            var lclfhlprs = new LocalFileHelpers();
            SelectedDeviceThumbnail = lclfhlprs.OpenImportFileDialog("png", "Image files (*.png)|*.Png|Image files (*.jpg)|*.jpeg");
        }

        #endregion

        #region Layout Creator Viewmodel region
        public double NewItemWidth { get; set; }
        public double NewItemHeight { get; set; }
        public int ItemNumber { get; set; }
        private double _itemsScale;
        public void UpdateZoneSize(bool withPoint, LEDSetup zone)
        {
            //get all child and set size
            var boundRct = GetDeviceRectBound(zone.Spots.ToList());
            zone.Width = boundRct.Width;
            zone.Height = boundRct.Height;
            if (withPoint)
            {
                zone.Left = boundRct.Left;
                zone.Top = boundRct.Top;
            }


        }
        private DrawableHelpers DrawableHlprs = new DrawableHelpers();
        private ControlModeHelpers CtrlHlprs = new ControlModeHelpers();
        public Rect GetDeviceRectBound(List<IDrawable> spots)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var listDrawable = new List<IDrawable>();
            foreach (var spot in spots)
            {
                listDrawable.Add(spot as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);


        }
        public Rect GetDeviceRectBound(List<IDeviceSpot> spots)
        {


            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var listDrawable = new List<IDrawable>();
            foreach (var spot in spots)
            {
                listDrawable.Add(spot as IDrawable);
            }

            return DrawableHlprs.GetBound(listDrawable);


        }
        private void CombineSelectedSpots()
        {

            var spotList = CanvasItems.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            if (spotList.Count < 2)
                return;

            var bound = GetDeviceRectBound(spotList);

            var stringValue = "";
            foreach (var spot in spotList)
            {
                var scaled = Geometry.Combine(
                    (spot as DeviceSpot).Geometry,
                    (spot as DeviceSpot).Geometry, GeometryCombineMode.Intersect,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new TranslateTransform(spot.Left, spot.Top),
                            new ScaleTransform(1.0 , 1.0 )
                        }
                    }
                );

                var scaledString = scaled.ToString(CultureInfo.InvariantCulture)
                    .Replace("F1", "")
                    .Replace(";", ",")
                    .Replace("L", " L")
                    .Replace("C", " C");

                stringValue = stringValue + " " + scaledString;
            }
            var shape = Geometry.Parse(stringValue.Trim());
            var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
            var newItem = new DeviceSpot(
                      bound.Top,
                      bound.Left,
                      shape.Bounds.Width,
                      shape.Bounds.Height,
                      1,
                      1,
                      1,
                      1,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      lastSpotID,
                      false,
                      shape);
            newItem.IsSelected = false;
            newItem.IsDeleteable = true;
            CanvasSelectedItems.Clear();
            CanvasItems.Add(newItem);
            spotList.ForEach(s => CanvasItems.Remove(s));
        }
        private void AddNewZone() // create new zone from selected spot fomr wellknown itemsource
        {

            var newZone = new LEDSetup();
            var spotList = CanvasItems.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            if (spotList.Count < 1)
                return;
            foreach (var spot in spotList)
            {
                var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                newZone.Spots.Add(clonedSpot);

            }
            UpdateZoneSize(true, newZone);

            foreach (var spot in newZone.Spots)
            {

                (spot as DeviceSpot).Left -= newZone.Left;
                (spot as DeviceSpot).Top -= newZone.Top;
                (spot as DeviceSpot).IsSelected = false;

            }
            CanvasSelectedItems.Clear();
            CanvasItems.Add(newZone);
            spotList.ForEach(spot => CanvasItems.Remove(spot));

        }
        private double _newSpotWidth;
        public double NewSpotWidth
        {
            get { return _newSpotWidth; }
            set
            {
                _newSpotWidth = value;
                RaisePropertyChanged();
            }
        }
        private double _newSpotHeight;
        public double NewSpotHeight
        {
            get { return _newSpotHeight; }
            set
            {
                _newSpotHeight = value;
                RaisePropertyChanged();
            }
        }

        private void OpenChangeSpotSizeWindow()
        {
            var changeSpotSizeWindow = new ChangeSpotSizeWindow();
            changeSpotSizeWindow.ShowDialog();
        }
        private void SetSpotSize()
        {
            //get total bound
            var usableItems = CanvasSelectedItems.Where(i => i is DeviceSpot).ToList();
            var bound = DrawableHlprs.GetBound(usableItems);
            var scaleX = NewSpotWidth / bound.Width;
            var scaleY = NewSpotHeight / bound.Height;
            foreach (var item in CanvasSelectedItems)
            {
                var led = item as DeviceSpot;
                if (led == null)
                    return;
                var geometry = led.Geometry.Clone();

                geometry.Transform = new TransformGroup
                {
                    Children = new TransformCollection
                {
                    new ScaleTransform(scaleX, scaleY),
                   // new TranslateTransform(0-boundsLeft*scaleX, 0-boundsTop*scaleY),
                    // new RotateTransform(angleInDegrees)
        }
                };
                var result = geometry.GetFlattenedPathGeometry();
                result.Freeze();
                led.Geometry = result;
                led.Width = result.Bounds.Width;
                led.Height = result.Bounds.Height;
                led.Left *= scaleX; led.Top *= scaleY;
            }

        }

        private double _deviceActualWidth;
        private double _deviceActualHeight;
        public double DeviceActualWidth
        {
            get { return _deviceActualWidth; }
            set
            {
                _deviceActualWidth = value;
                RaisePropertyChanged();
            }
        }
        public double DeviceActualHeight
        {
            get { return _deviceActualHeight; }
            set
            {
                _deviceActualHeight = value;
                RaisePropertyChanged();
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Vendor { get; set; }
        private void SaveDeviceData()
        {


            var usableZone = CanvasItems.Where(z => z is LEDSetup).ToList();
            if (usableZone.Count() == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải thêm ít nhất 1 Zone", "Invalid LED number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var rectBound = GetDeviceRectBound(CanvasItems.ToList());
            var image = CanvasItems.Where(i => i is ImageVisual).FirstOrDefault();
            System.Windows.Point scale = new System.Windows.Point(DeviceActualWidth / rectBound.Width, DeviceActualHeight / rectBound.Height);
            foreach (var zone in usableZone)
            {
                //reset origin for each zone
                var ledSetup = zone as LEDSetup;
                ledSetup.Left -= rectBound.Left;
                ledSetup.Top -= rectBound.Top;
                ledSetup.SetScale(scale.X, scale.Y, false);
                ledSetup.ZoneUID = Guid.NewGuid().ToString();
                //make this zone controlable
                CtrlHlprs.MakeZoneControlable(ledSetup);

            }
            Device = new ARGBLEDSlaveDevice();
            Device.ActualWidth = DeviceActualWidth;
            Device.ActualHeight = DeviceActualHeight;
            Device.Width = DeviceActualWidth;
            Device.Height = DeviceActualHeight;
            Device.Name = Name;
            Device.Description = Description;
            Device.Vendor = Vendor;
            Device.DeviceType = DeviceType;

            if (image != null)
            {
                image.Left -= rectBound.Left;
                image.Top -= rectBound.Top;
                image.SetScale(scale.X, scale.Y, false);
                Device.Image = image as ImageVisual;

            }
            usableZone.ForEach(zone => Device.ControlableZones.Add(zone as LEDSetup));
            CanvasSelectedItems.Clear();
            usableZone.ForEach(zone => CanvasItems.Remove(zone));
            CanvasItems.Remove(image);
            CanvasItems.Add(Device);
            CanSelectMultipleItems = false;
            ExportCurrentDeviceToFile();
        }
        private void ExportCurrentDeviceToFile()
        {
            Microsoft.Win32.SaveFileDialog Export = new Microsoft.Win32.SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = "Layer";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "json";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var layerJson = JsonConvert.SerializeObject(Device);

            if (Export.ShowDialog() == true)
            {

                File.WriteAllText(Export.FileName, layerJson);

            }
        }

        private void ApplyDeviceActualDimension()
        {



            var rectBound = GetDeviceRectBound(CanvasItems.ToList());
            System.Windows.Point scale = new System.Windows.Point(DeviceActualWidth / rectBound.Width, DeviceActualHeight / rectBound.Height);
            foreach (var item in CanvasItems)
            {
                item.Left -= rectBound.Left;
                item.Top -= rectBound.Top;
                item.SetScale(scale.X, scale.Y, false);
            }
        }
        public SlaveDeviceTypeEnum DeviceType { get; set; }
        public ObservableCollection<SlaveDeviceTypeEnum> AvailableDeviceTypes { get; set; }
        public ObservableCollection<DeviceTypeEnum> AvailableMasterDeviceTypes { get; set; }
        public ObservableCollection<DeviceConnectionTypeEnum> AvailableConnectionTypes { get; set; }
        public ARGBLEDSlaveDevice Device { get; set; }
        private IDrawable _canvasSelectedItem;
        public IDrawable CanvasSelectedItem
        {
            get { return _canvasSelectedItem; }
            set { _canvasSelectedItem = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _canvasSelectedItems;
        public ObservableCollection<IDrawable> CanvasSelectedItems
        {
            get { return _canvasSelectedItems; }
            set { _canvasSelectedItems = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _canvasItems;
        public ObservableCollection<IDrawable> CanvasItems
        {
            get { return _canvasItems; }
            set { _canvasItems = value; RaisePropertyChanged(); }
        }
        private System.Windows.Point _mousePosition;
        public System.Windows.Point MousePosition
        {
            get { return _mousePosition; }
            set
            {
                _mousePosition = value; RaisePropertyChanged();
            }
        }
        private void AddImage()
        {
            OpenFileDialog addImage = new OpenFileDialog();
            addImage.Title = "Chọn Ảnh";
            addImage.CheckFileExists = true;
            addImage.CheckPathExists = true;
            addImage.DefaultExt = "Png";
            addImage.Filter = "Image files (*.png;*.jpeg;*jpg)|*.png;*.jpeg;*jpg|All files (*.*)|*.*";
            addImage.FilterIndex = 2;

            addImage.ShowDialog();

            if (!string.IsNullOrEmpty(addImage.FileName) && File.Exists(addImage.FileName))
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(addImage.FileName, true);
                var image = new ImageVisual
                {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    ImagePath = addImage.FileName,
                    Width = DeviceActualWidth,
                    Height = DeviceActualHeight,
                    IsResizeable = true,
                    IsDeleteable = true
                };
                CanvasItems.Insert(0, image);
            }
        }

        private void DeleteSelectedItems(ObservableCollection<IDrawable> itemSource)
        {
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            for (var i = 0; i < selectedItems.Count(); i++)
            {
                selectedItems[i].IsSelected = false;
                if (selectedItems[i].IsDeleteable)
                    itemSource.Remove(selectedItems[i]);
            }
        }

        private void AglignSelectedItemstoLeft(ObservableCollection<IDrawable> itemSource)
        {
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Left = minLeft;
            }
        }
        private void SpreadItemHorizontal(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min X
            double spacing = 10.0;
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = selectedItems[i - 1].Width + previousLeft + spacing;
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = previousLeft - (selectedItems[i].Width + spacing);
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;

            }
        }
        private void SpreadItemVertical(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min Y
            double spacing = 10.0;
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = selectedItems[i - 1].Height + previousTop + spacing;
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = previousTop - (selectedItems[i].Height + spacing);
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;

            }

        }
        private void AglignSelectedItemstoTop(ObservableCollection<IDrawable> itemSource)
        {
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Top = minTop;
            }
        }
        private void ClearPIDCanvas()
        {

            CanvasSelectedItems.Clear();
            CanvasItems.Clear();

        }
        private void LockSelectedItem(ObservableCollection<IDrawable> itemSource)
        {

            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = false;
            }
        }
        private void UnlockSelectedItem(ObservableCollection<IDrawable> items)
        {

            foreach (var item in items.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = true;
                item.IsSelectable = true;

            }
        }
        private void AddSpotLayout()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.txt)|*.TXT";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();

            var text = System.IO.File.ReadAllText(Import.FileName);
            try
            {
                StringReader sr = new StringReader(text);

                XmlReader reader = XmlReader.Create(sr);

                GeometryGroup gr = (GeometryGroup)XamlReader.Load(reader);


                var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
                var newItems = new ObservableCollection<IDrawable>();
                for (int i = 0; i < gr.Children.Count; i++)
                {
                    var newItem = new DeviceSpot(
                        gr.Children[i].Bounds.Top,
                        gr.Children[i].Bounds.Left,
                        gr.Children[i].Bounds.Width,
                        gr.Children[i].Bounds.Height,
                        1,
                        1,
                        1,
                        1,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        lastSpotID + i,
                        false,
                        gr.Children[i]);
                    newItem.IsSelected = true;
                    newItem.IsDeleteable = true;
                    newItems.Add(newItem);

                }
                foreach (var item in newItems)
                {
                    CanvasItems.Add(item);
                    //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
                }
            }
            catch (Exception ex)
            {

            }

        }
        public List<GeometryDrawing> GatherGeometry(DrawingGroup drawingGroup)
        {
            var result = new List<GeometryDrawing>();
            result.AddRange(drawingGroup.Children.Where(c => c is GeometryDrawing).Cast<GeometryDrawing>());
            foreach (var childGroup in drawingGroup.Children.Where(c => c is DrawingGroup).Cast<DrawingGroup>())
                result.AddRange(GatherGeometry(childGroup));

            return result;
        }
        private void ImportSVG()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "SVG files (*.svg)|*.SVG";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();
            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var fileName = Import.FileName;
                var xamlFileName = fileName.Replace(".svg", ".xaml");
                var settings = new WpfDrawingSettings { IncludeRuntime = true, TextAsGeometry = true };
                var converter = new FileSvgConverter(settings);

                using var fileStream = File.OpenRead(fileName);
                converter.Convert(fileStream, xamlFileName);
                var xaml = File.ReadAllText(xamlFileName);
                File.Delete(xamlFileName);

                var parsed = (DrawingGroup)XamlReader.Parse(xaml, new ParserContext { BaseUri = new Uri(System.IO.Path.GetDirectoryName(fileName)) });
                var geometry = GatherGeometry(parsed);

                var group = new DrawingGroup { Children = new DrawingCollection(geometry) };
                // var stringValue = "";
                var newItems = new List<IDrawable>();
                foreach (var geometryDrawing in geometry)
                {
                    var offsetLeft = geometryDrawing.Bounds.Left;
                    var offsetTop = geometryDrawing.Bounds.Top;
                    var scaled = Geometry.Combine(
                        geometryDrawing.Geometry,
                        geometryDrawing.Geometry, GeometryCombineMode.Intersect,
                        new TransformGroup
                        {
                            Children = new TransformCollection
                            {
                            new TranslateTransform(offsetLeft * -1, offsetTop * -1),
                            new ScaleTransform(1.0 , 1.0 )
                            }
                        }
                    );

                    var scaledString = scaled.ToString(CultureInfo.InvariantCulture)
                        .Replace("F1", "")
                        .Replace(";", ",")
                        .Replace("L", " L")
                        .Replace("C", " C");

                    //stringValue = stringValue + " " + scaledString;
                    var lastSpotID = CanvasItems.Where(s => s is DeviceSpot).Count();
                    var shape = Geometry.Parse(scaledString.Trim());
                    var newItem = new DeviceSpot(
                           offsetTop,
                           offsetLeft,
                           shape.Bounds.Width,
                           shape.Bounds.Height,
                           1,
                           1,
                           1,
                           1,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           lastSpotID,
                           false,
                           shape);
                    newItem.IsSelected = true;
                    newItem.IsDeleteable = true;
                    CanvasItems.Add(newItem);
                }
                //create geometry



            }
        }
        #endregion
        #region Effect Analyzer Viewmodel region
        private ObservableCollection<string> _listImage;

        public ObservableCollection<string> ListImage
        {
            get { return _listImage; }
            set
            {
                _listImage = value;
                RaisePropertyChanged();
            }
        }
        private Frame _currentFrame;
        public Frame CurrentFrame
        {
            get
            {
                return _currentFrame;
            }
            set
            {
                _currentFrame = value;
                RaisePropertyChanged(nameof(CurrentFrame));
            }
        }
        private Motion _layer;
        public Motion Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
                RaisePropertyChanged(nameof(Layer));
            }
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            _frameCounter++;
            if (_frameCounter >= Layer.Frames.Count())
                _frameCounter -= Layer.Frames.Count();
            CurrentFrame = Layer.Frames[_frameCounter];
        }
        private void SelectFrameDataFolder()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn Frame";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Image files (*.png)|*.Png";
            Import.FilterIndex = 2;
            Import.Multiselect = true;

            Import.ShowDialog();

            ListImage = new ObservableCollection<string>();
            Layer = new Motion(Import.FileNames.Length);
            for (int i = 0; i < Import.FileNames.Length; i++)
            {
                BitmapData bitmapData = new BitmapData();
                Bitmap curentFrame = new Bitmap(Import.FileNames[i]);
                ListImage.Add(Import.FileNames[i]);
                var frameWidth = curentFrame.Width;
                var frameHeight = curentFrame.Height;

                var rectSet = BuildMatrix(frameWidth, frameHeight, 256, 1);
                //lock frame
                try
                {
                    curentFrame.LockBits(new Rectangle(0, 0, frameWidth, frameHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb, bitmapData);
                }

                catch (System.ArgumentException)
                {
                    //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                    // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                    curentFrame = null;
                    // continue;
                }
                byte[] brightnessMap = new byte[rectSet.Length];
                int pixelCount = 0;
                CurrentFrame = new Frame(256);
                foreach (var rect in rectSet)
                {
                    const int numberOfSteps = 15;
                    int stepx = Math.Max(1, rect.Width / numberOfSteps);
                    int stepy = Math.Max(1, rect.Height / numberOfSteps);
                    GetAverageColorOfRectangularRegion(rect, stepy, stepx, bitmapData, out int sumR, out int sumG, out int sumB, out int count);
                    var countInverse = 1f / count;
                    brightnessMap[pixelCount++] = (byte)sumR;
                    System.Windows.Media.Color pixelColor = new System.Windows.Media.Color();
                    pixelColor = System.Windows.Media.Color.FromRgb((byte)(sumR * countInverse), (byte)(sumG * countInverse), (byte)(sumB * countInverse));
                    //add displaypixel to current frame

                }
                var newFrame = new Frame(256);
                for (int j = 0; j < brightnessMap.Count(); j++)
                {
                    newFrame.BrightnessData[j] = brightnessMap[j];
                }
                Layer.Frames[i] = newFrame;

                //display frame at preview
                //store current frame to json file
            }
            //splice bitmap into 256 led width and 1 led height
            // so the rectangle is width : 4 and height:4 with top is 0 and left is i*4



        }

        private unsafe void GetAverageColorOfRectangularRegion(Rectangle spotRectangle, int stepy, int stepx, BitmapData bitmapData, out int sumR, out int sumG, out int sumB, out int count)
        {
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            var stepCount = spotRectangle.Width / stepx;
            var stepxTimes4 = stepx * 4;
            for (var y = spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
            {
                byte* pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (int i = 0; i < stepCount; i++)
                {
                    sumB += pointer[0];
                    sumG += pointer[1];
                    sumR += pointer[2];

                    pointer += stepxTimes4;
                }
                count += stepCount;
            }
        }



        private Rectangle[] BuildMatrix(int rectwidth, int rectheight, int spotsX, int spotsY)
        {
            int spacing = 0;
            if (spotsX == 0)
                spotsX = 1;
            if (spotsY == 0)
                spotsY = 1;
            Rectangle[] rectangleSet = new Rectangle[spotsX * spotsY];
            var rectWidth = (rectwidth - (spacing * (spotsX + 1))) / spotsX;
            var rectHeight = (rectheight - (spacing * (spotsY + 1))) / spotsY;



            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;




            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing * i + (rectwidth - (spotsX * rectWidth) - spacing * (spotsX - 1)) / 2 + i * rectWidth;
                    var y = spacing * j + (rectheight - (spotsY * rectHeight) - spacing * (spotsY - 1)) / 2 + j * rectHeight;
                    var index = counter;

                    rectangleSet[index] = new Rectangle(x, y, rectWidth, rectHeight);
                    counter++;

                }
            }

            return rectangleSet;

        }
        private void ExportCurrentLayer()
        {
            if (Layer == null)
                return;
            Microsoft.Win32.SaveFileDialog Export = new Microsoft.Win32.SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = "Layer";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "AML";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var layerJson = JsonConvert.SerializeObject(Layer);

            if (Export.ShowDialog() == true)
            {

                File.WriteAllText(Export.FileName, layerJson);

            }
        }
        #endregion
    }
}
