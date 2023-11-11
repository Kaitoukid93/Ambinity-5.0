using adrilight.Helpers;
using adrilight.Manager;
using adrilight.Services.NetworkStream;
using adrilight.Services.SerialStream;
using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.AppProfile;
using adrilight_shared.Models.AppUser;
using adrilight_shared.Models.Audio;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.CompositionData;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Preview;
using adrilight_shared.Models.Store;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using FTPServer;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using MoreLinq;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Serilog;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using TimeLineTool;
using Un4seen.BassWasapi;
using Application = System.Windows.Application;
using Bitmap = System.Drawing.Bitmap;
using Border = adrilight_shared.Models.Drawable.Border;
using Color = System.Windows.Media.Color;
using File = System.IO.File;
using Formatting = Newtonsoft.Json.Formatting;
using IComputer = adrilight.Util.IComputer;
using NotifyIcon = HandyControl.Controls.NotifyIcon;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Point = System.Windows.Point;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using SelectionMode = System.Windows.Controls.SelectionMode;
using SplashScreen = adrilight.View.SplashScreen;
using Task = System.Threading.Tasks.Task;
namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonGeneralFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string JsonSolidColorFileNameAndPath => Path.Combine(JsonPath, "adrilight-solidColor.json");
        private string JsonDeviceProfileFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceProfiles.json");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
        private string JsonOpenRGBDevicesFileNameAndPath => Path.Combine(JsonPath, "adrilight-openrgbdevices.json");
        private string JsonGifsFileNameAndPath => Path.Combine(JsonPath, "Gif");
        private string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");

        private string JsonFWToolsFWListFileNameAndPath => Path.Combine(JsonFWToolsFileNameAndPath, "adrilight-fwlist.json");
        private string JsonGifsCollectionFileNameAndPath => Path.Combine(JsonPath, "adrilight-gifCollection.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");

        private string JsonGradientFileNameAndPath => Path.Combine(JsonPath, "adrilight-GradientCollection.json");

        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Developer_Release";

        #region database local folder paths

        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string LightingProfilesCollectionFolderPath => Path.Combine(JsonPath, "LightingProfiles");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string GifsCollectionFolderPath => Path.Combine(JsonPath, "Gifs");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        private string CacheFolderPath => Path.Combine(JsonPath, "Cache");
        private string ProfileCollectionFolderPath => Path.Combine(JsonPath, "Profiles");

        #endregion database local folder paths

        #region constant string

        public const string ImagePathFormat = "pack://application:,,,/adrilight;component/View/Images/{0}";
        public const string dashboard = "Dashboard";
        public const string deviceSetting = "Device Settings";
        public const string appSetting = "App Settings";
        public const string faq = "FAQ";
        public const string general = "General";
        public const string lighting = "Lighting";
        public const string canvasLighting = "Canvas Lighting";
        public const string groupLighting = "Group Lighting";

        #endregion constant string

        #region property

        private ObservableCollection<int> _availableBaudrates;

        public ObservableCollection<int> AvailableBaudrates {
            get
            {
                return _availableBaudrates;
            }

            set
            {
                _availableBaudrates = value;
                RaisePropertyChanged(nameof(AvailableBaudrates));
            }
        }

        private ObservableCollection<SystemTrayContextMenu> _availableContextMenus;

        public ObservableCollection<SystemTrayContextMenu> AvailableContextMenus {
            get
            {
                return _availableContextMenus;
            }

            set
            {
                _availableContextMenus = value;
                RaisePropertyChanged(nameof(AvailableContextMenus));
            }
        }

        private ObservableCollection<ActionType> _availableActionsforCurrentDevice;

        public ObservableCollection<ActionType> AvailableActionsforCurrentDevice {
            get
            {
                return _availableActionsforCurrentDevice;
            }

            set
            {
                _availableActionsforCurrentDevice = value;

                RaisePropertyChanged();
            }
        }

        private ActionSettings _currentSelectedAction;

        public ActionSettings CurrentSelectedAction {
            get
            {
                return _currentSelectedAction;
            }

            set
            {
                _currentSelectedAction = value;
            }
        }

        private ObservableCollection<KeyModel> _currentSelectedShortKeys;

        public ObservableCollection<KeyModel> CurrentSelectedShortKeys {
            get { return _currentSelectedShortKeys; }

            set
            {
                _currentSelectedShortKeys = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _currentSelectedModifiers;

        public ObservableCollection<string> CurrentSelectedModifiers {
            get { return _currentSelectedModifiers; }

            set
            {
                _currentSelectedModifiers = value;
                RaisePropertyChanged();
            }
        }

        private DeviceFirmware _currentSelectedFirmware;

        public DeviceFirmware CurrentSelectedFirmware {
            get
            {
                return _currentSelectedFirmware;
            }

            set
            {
                _currentSelectedFirmware = value;
                RaisePropertyChanged();
            }
        }
        private string _selectedActionType;
        private IDeviceSettings _currentDevice;

        public IDeviceSettings CurrentDevice {
            get { return _currentDevice; }

            set
            {
                if (_currentDevice == value) return;

                _currentDevice = value;

                RaisePropertyChanged();
            }
        }
        public ICommand OpenLightingProfileManagerWindowCommand { get; set; }
        public ICommand CreateNewLightingProfileCommand { get; set; }
        public ICommand RequestingRescanDevicesCommand { get; set; }
        public ICommand CreateNewVIDCommand { get; set; }
        public ICommand OpenCreateNewLightingProfileWindowCommand { get; set; }
        public ICommand OpenCreateNewVIDWindowCommand { get; set; }
        public ICommand ActivateCurrentLightingProfileCommand { get; set; }
        public ICommand SaveDeviceInformationToDiskCommand { get; set; }
        public ICommand ImportDeviceFromFileCommand { get; set; }
        public ICommand BackToHomePageCommand { get; set; }
        public ICommand OpenOOTBCommand { get; set; }
        public ICommand RefreshAudioDeviceCommand { get; set; }
        public ICommand RefreshMonitorCollectionCommand { get; set; }
        public ICommand CloseSearchingScreenCommand { get; set; }
        public ICommand ManuallyAddSelectedDeviceToDashboard { get; set; }
        public ICommand FilterStoreItemByTargetDeviceTypeCommand { get; set; }
        public ICommand ExportItemForOnlineStoreCommand { get; set; }
        public ICommand GroupSelectedZoneForMaskedControlCommand { get; set; }
        public ICommand UnselectAllLiveiewItemCommand { get; set; }
        public ICommand LiveViewMouseButtonUpCommand { get; set; }
        public ICommand UnselectAllSurfaceEditorItemCommand { get; set; }
        public ICommand IsolateSelectedItemsCommand { get; set; }
        public ICommand OpenSpotPIDQuickEDitWindowCommand { get; set; }
        public ICommand RotateSelectedSurfaceEditorItemCommand { get; set; }
        public ICommand ReflectSelectedSurfaceEditorItemCommand { get; set; }
        public ICommand SaveCurrentPIDCommand { get; set; }
        public ICommand CancelCurrentPIDCommand { get; set; }
        public ICommand ExitIsolateModeCommand { get; set; }
        public ICommand UnGroupZoneCommand { get; set; }
        public ICommand ExportCurrentOnlineItemToFilesCommand { get; set; }
        public ICommand OpenImageSelectorCommand { get; set; }
        public ICommand DownloadCurrentItemCommand { get; set; }
        public ICommand CompositionNextFrameCommand { get; set; }
        public ICommand SelectSlaveDeviceForCurrentOutputCommand { get; set; }
        public ICommand SelectSlaveDeviceForAllOutputCommand { get; set; }
        public ICommand OpenSlaveDeviceSelectorCommand { get; set; }
        public ICommand SelectPIDCanvasItemCommand { get; set; }
        public ICommand SelectLiveViewItemCommand { get; set; }
        public ICommand ChangeSelectedControlZoneActiveControlModeCommand { get; set; }
        public ICommand SelectSurfaceEditorItemCommand { get; set; }
        public ICommand SetSelectedSurfaceEditorDevicePreviewColorCommand { get; set; }
        public ICommand ApplySelectedItemsScaleCommand { get; set; }
        public ICommand ApplySelectedItemsRotationCommand { get; set; }
        public ICommand RestoreSelectedSurfaceDeviceSizeAndRotationCommand { get; set; }
        public ICommand CutSelectedMotionCommand { get; set; }
        public ICommand ToggleCompositionPlayingStateCommand { get; set; }
        public ICommand CompositionFrameStartOverCommand { get; set; }
        public ICommand CompositionPreviousFrameCommand { get; set; }
        public ICommand SetSelectedMotionCommand { get; set; }
        public ICommand NextOOTBCommand { get; set; }
        public ICommand OpenTutorialCommand { get; set; }
        public ICommand FinishOOTBCommand { get; set; }
        public ICommand PrevioustOOTBCommand { get; set; }
        public ICommand SkipOOTBCommand { get; set; }
        public ICommand PIDWindowClosingCommand { get; set; }
        public ICommand SurfaceEditorWindowClosingCommand { get; set; }
        public ICommand ClearPIDCanvasCommand { get; set; }
        public ICommand DeleteSelectedItemsCommand { get; set; }
        public ICommand AddItemsToPIDCanvasCommand { get; set; }
        public ICommand AddImageToPIDCanvasCommand { get; set; }
        public ICommand AddSpotGeometryCommand { get; set; }
        public ICommand AddSpotLayoutCommand { get; set; }
        public ICommand SaveCurretSurfaceLayoutCommand { get; set; }
        public ICommand SetRandomOutputColorCommand { get; set; }
        public ICommand LockSelectedItemCommand { get; set; }
        public ICommand UnlockSelectedItemCommand { get; set; }
        public ICommand ResetToDefaultRectangleScaleCommand { get; set; }
        public ICommand AglignSelectedItemstoLeftCommand { get; set; }
        public ICommand SpreadItemLeftHorizontalCommand { get; set; }
        public ICommand SpreadItemRightHorizontalCommand { get; set; }
        public ICommand SpreadItemUpVerticalCommand { get; set; }
        public ICommand SpreadItemDownVerticalCommand { get; set; }
        public ICommand AglignSelectedItemstoTopCommand { get; set; }
        public ICommand ExitCurrentRunningAppCommand { get; set; }
        public ICommand ExecuteAutomationFromManagerCommand { get; set; }
        public ICommand UnpinAutomationFromDashboardCommand { get; set; }
        public ICommand UpdateAppCommand { get; set; }
        public ICommand OpenLEDSteupSelectionWindowsCommand { get; set; }
        public ICommand OpenAvailableUpdateListWindowCommand { get; set; }
        public ICommand ResetAppCommand { get; set; }
        public ICommand SaveCurrentSelectedRegionCommand { get; set; }
        public ICommand ApplySpotImportDataCommand { get; set; }
        public ICommand ApplyOutputImportDataCommand { get; set; }
        public ICommand OpenSpotMapWindowCommand { get; set; }
        public ICommand OpenAboutWindowCommand { get; set; }
        public ICommand OpenSurfaceEditorWindowCommand { get; set; }
        public ICommand OpenAppSettingsWindowCommand { get; set; }
        public ICommand OpenDebugWindowCommand { get; set; }
        public ICommand OpenLogFolderCommand { get; set; }
        public ICommand SelecFirmwareForCurrentDeviceCommand { get; set; }
        public ICommand ApplyDeviceHardwareSettingsCommand { get; set; }
        public ICommand SetCurrentLEDSetupSentryColorCommand { get; set; }
        public ICommand SetAllOutputSelectedGradientColorCommand { get; set; }
        public ICommand UpdateCurrentSelectedDeviceFirmwareCommand { get; set; }
        public ICommand SetAllOutputSelectedSolidColorCommand { get; set; }
        public ICommand LaunchDeleteSelectedDeviceWindowCommand { get; set; }
        public ICommand DeleteSelectedDeviceCommand { get; set; }
        public ICommand SaveAllAutomationCommand { get; set; }
        public ICommand SaveCurrentSelectedAutomationShortkeyCommand { get; set; }
        public ICommand CloseIDSetupCommand { get; set; }
        public ICommand ResetAllItemsIDCommand { get; set; }
        public ICommand AddSelectedActionTypeToListCommand { get; set; }
        public ICommand DeleteSelectedActionFromListCommand { get; set; }
        public ICommand OpenAddNewAutomationCommand { get; set; }
        public ICommand OpenActionsManagerWindowCommand { get; set; }
        public ICommand OpenHotKeySelectionWindowCommand { get; set; }
        public ICommand OpenTargetDeviceSelectionWindowCommand { get; set; }
        public ICommand OpenTargetParamSelectionWindowCommand { get; set; }
        public ICommand OpenTargetActionSelectionWindowCommand { get; set; }
        public ICommand OpenAutomationValuePickerWindowCommand { get; set; }
        public ICommand OpenAutomationManagerWindowCommand { get; set; }
        public ICommand OpenAmbinoStoreWindowCommand { get; set; }
        public ICommand OpenAmbinoStoreWindowForCurrentDeviceCommand { get; set; }
        public ICommand SetCurrentActionTypeForSelectedActionCommand { get; set; }
        public ICommand SetCurrentActionTargetDeviceForSelectedActionCommand { get; set; }
        public ICommand SetCurrentActionParamForSelectedActionCommand { get; set; }
        public ICommand SetCurrentSelectedActionTypeColorValueCommand { get; set; }
        public ICommand OpenHardwareMonitorWindowCommand { get; set; }
        public ICommand RefreshCurrentCollectionCommand { get; set; }
        public ICommand ListSelectionItemMouseEnterCommand { get; set; }
        public ICommand ListSelectionItemMouseLeaveCommand { get; set; }
        public ICommand RefreshLocalSlaveDeviceCollectionCommand { get; set; }
        public ICommand DeleteSelectedItemFromCurrentCollectionCommand { get; set; }
        public ICommand CoppyColorCodeCommand { get; set; }
        public ICommand DeleteSelectedAutomationCommand { get; set; }
        public ICommand LaunchWBAdjustWindowCommand { get; set; }
        public ICommand ImportProfileCommand { get; set; }
        public ICommand ActivateSelectedProfileCommmand { get; set; }
        public ICommand DeleteAttachedProfileCommand { get; set; }
        public ICommand CreateNewProfileCommand { get; set; }
        public ICommand OpenProfileCreateCommand { get; set; }
        public ICommand ImportLEDSetupCommand { get; set; }
        public ICommand UnZoneItemsCommand { get; set; }
        public ICommand RenameZoneCommand { get; set; }
        public ICommand AddNewZoneCommand { get; set; }
        public ICommand OpenDeviceConnectionSettingsWindowCommand { get; set; }
        public ICommand OpenDeviceFirmwareSettingsWindowCommand { get; set; }
        public ICommand RenameSelectedItemCommand { get; set; }

        public ICommand SetSelectedItemScaleFactorCommand { get; set; }
        public ICommand SetSelectedItemRotateFactorCommand { get; set; }
        public ICommand OpenAdvanceSettingWindowCommand { get; set; }
        public ICommand ShowNameEditWindow { get; set; }
        public ICommand SaveNewUserEditLEDSetup { get; set; }
        public ICommand SetSpotPIDCommand { get; set; }
        public ICommand SetSpotVIDCommand { get; set; }
        public ICommand SetZoneMIDCommand { get; set; }
        public ICommand ResetSpotIDCommand { get; set; }
        public ICommand BackToPreviousAddDeviceWizardStateCommand { get; set; }
        public ICommand LaunchCompositionEditWindowCommand { get; set; }
        public ICommand SubParamActionCommand { get; set; }
        public ICommand AddPickedSolidColorCommand { get; set; }
        public ICommand OpenExternalWindowsFromModeParameterCommand { get; set; }
        public ICommand DeleteSelectedPaletteCommand { get; set; }
        public ICommand CreateNewPaletteCommand { get; set; }
        public ICommand CreateNewAutomationCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand BackToCollectionViewCommand { get; set; }
        public ICommand SnapshotCommand { get; set; }

        #endregion property

        private ObservableCollection<IDeviceSettings> _availableDevices;

        public ObservableCollection<IDeviceSettings> AvailableDevices {
            get { return _availableDevices; }
            set => Set(ref _availableDevices, value);
        }

        private ObservableCollection<object> _automationParamList;

        public ObservableCollection<object> AutomationParamList {
            get { return _automationParamList; }
            set => Set(ref _automationParamList, value);
        }

        private string _paramType;

        public string ParamType {
            get { return _paramType; }
            set => Set(ref _paramType, value);
        }

        private ObservableCollection<WLEDDevice> _availableWLEDDevices;

        public ObservableCollection<WLEDDevice> AvailableWLEDDevices {
            get { return _availableWLEDDevices; }

            set
            {
                if (_availableWLEDDevices == value) return;
                _availableWLEDDevices = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSettings> _availableOpenRGBDevices;

        public ObservableCollection<IDeviceSettings> AvailableOpenRGBDevices {
            get { return _availableOpenRGBDevices; }

            set
            {
                if (_availableOpenRGBDevices == value) return;
                _availableOpenRGBDevices = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSettings> _availableSerialDevices;

        public ObservableCollection<IDeviceSettings> AvailableSerialDevices {
            get { return _availableSerialDevices; }

            set
            {
                if (_availableSerialDevices == value) return;
                _availableSerialDevices = value;
                RaisePropertyChanged();
            }
        }

        private List<IOutputSettings> _availableOutputForSelectedDevice;

        public List<IOutputSettings> AvailableOutputForSelectedDevice {
            get { return _availableOutputForSelectedDevice; }

            set
            {
                if (_availableOutputForSelectedDevice == value) return;
                _availableOutputForSelectedDevice = value;
                RaisePropertyChanged();
            }
        }

        private IOutputSettings _selectedOutputForCurrentDevice;

        public IOutputSettings SelectedOutputForCurrentDevice {
            get { return _selectedOutputForCurrentDevice; }

            set
            {
                if (_selectedOutputForCurrentDevice == value) return;
                _selectedOutputForCurrentDevice = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<WLEDDevice> _selectedWLEDDevice;

        public ObservableCollection<WLEDDevice> SelectedWLEDDevice {
            get { return _selectedWLEDDevice; }
            set { _selectedWLEDDevice = value; }
        }

        private ObservableCollection<AppProfile> _availableProfiles;

        public ObservableCollection<AppProfile> AvailableProfiles {
            get { return _availableProfiles; }

            set
            {
                if (_availableProfiles == value) return;
                _availableProfiles = value;

                RaisePropertyChanged();
            }
        }
        private ObservableCollection<DeviceFirmware> _availableFirmwareForCurrentDevice;

        public ObservableCollection<DeviceFirmware> AvailableFirmwareForCurrentDevice {
            get { return _availableFirmwareForCurrentDevice; }

            set
            {
                if (_availableFirmwareForCurrentDevice == value) return;
                _availableFirmwareForCurrentDevice = value;

                RaisePropertyChanged();
            }
        }
        private ObservableCollection<AutomationSettings> _shutdownAutomations;

        public ObservableCollection<AutomationSettings> ShutdownAutomations {
            get { return _shutdownAutomations; }
            set
            {
                if (_shutdownAutomations == value) return;
                _shutdownAutomations = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _selectableIcons;
        public ObservableCollection<string> SelectableIcons {
            get { return _selectableIcons; }
            set
            {
                _selectableIcons = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<AutomationSettings> _availableAutomations;

        public ObservableCollection<AutomationSettings> AvailableAutomations {
            get { return _availableAutomations; }

            set
            {
                if (_availableAutomations == value) return;
                _availableAutomations = value;

                RaisePropertyChanged();
            }
        }
        public List<AutomationSettings> DashboardPinnedAutomation => AvailableAutomations?.Where(a => a.IsPinned == true).ToList();
        private ObservableCollection<AppUser> _availableAppUser;

        public ObservableCollection<AppUser> AvailableAppUser {
            get { return _availableAppUser; }

            set
            {
                _availableAppUser = value;
                RaisePropertyChanged();
            }
        }
        private IComputer _thisComputer;

        public IComputer ThisComputer {
            get { return _thisComputer; }

            set
            {
                _thisComputer = value;
                RaisePropertyChanged();
            }
        }

        public void RefreshHardwareStatus()
        {
            RaisePropertyChanged(nameof(ThisComputer));
        }

        private ObservableCollection<DeviceProfile> _availableProfilesForCurrentDevice;

        public ObservableCollection<DeviceProfile> AvailableProfilesForCurrentDevice {
            get { return _availableProfilesForCurrentDevice; }

            set
            {
                _availableProfilesForCurrentDevice = value;
                RaisePropertyChanged();
            }
        }
        public ICommand SelectCardCommand { get; set; }
        public ICommand SelectOnlineItemCommand { get; set; }
        public ICommand SeAllCarouselItemCommand { get; set; }
        public ICommand LightingModeSelection { get; set; }
        public ICommand ShowAddNewCommand { get; set; }
        public ICommand RefreshDeviceCommand { get; set; }

        private string JsonDeviceNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        public IList<String> _AvailableComPorts;

        public IList<String> AvailableComPorts {
            get
            {
                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");

                return _AvailableComPorts;
            }
        }

        public IList<double> _availableMotionSpeed;

        public IList<double> AvailableMotionSpeed {
            get
            {
                return _availableMotionSpeed;
            }

            set
            {
                _availableMotionSpeed = value;
                RaisePropertyChanged();
            }
        }

        public IList<string> _availableMotionDirrection;

        public IList<string> AvailableMotionDirrection {
            get
            {
                return _availableMotionDirrection;
            }

            set
            {
                _availableMotionDirrection = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<VisualizerDataModel> _audioVisualizers;

        public ObservableCollection<VisualizerDataModel> AudioVisualizers {
            get { return _audioVisualizers; }
            set { _audioVisualizers = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<DesktopFrameCard> _availableBitmaps;

        public ObservableCollection<DesktopFrameCard> AvailableBitmaps {
            get { return _availableBitmaps; }
            set { _availableBitmaps = value; RaisePropertyChanged(); }
        }
        private DesktopFrameCard _selectedBitmap;

        public DesktopFrameCard SelectedBitmap {
            get { return _selectedBitmap; }
            set { _selectedBitmap = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<GifCard> _availableGifs;

        public ObservableCollection<GifCard> AvailableGifs {
            get { return _availableGifs; }
            set { _availableGifs = value; RaisePropertyChanged(); }
        }
        public WriteableBitmap _greyBitmap;

        public WriteableBitmap GreyBitmap {
            get => _greyBitmap;

            set
            {
                _greyBitmap = value;
                RaisePropertyChanged();
            }
        }

        public WriteableBitmap _gifxelationBitmap;

        public WriteableBitmap GifxelationBitmap {
            get => _gifxelationBitmap;

            set
            {
                _gifxelationBitmap = value;
                RaisePropertyChanged(nameof(GifxelationBitmap));
            }
        }



        private int _selectedDeviceCount;

        public int SelectedDeviceCount {
            get { return _selectedDeviceCount; }

            set
            {
                _selectedDeviceCount = value;
                RaisePropertyChanged(nameof(_selectedDeviceCount));
            }
        }

        private bool _toolbarVisible;

        public bool ToolbarVisible {
            get { return _toolbarVisible; }

            set
            {
                _toolbarVisible = value;
                RaisePropertyChanged(nameof(ToolbarVisible));
            }
        }

        public ResourceHelpers ResourceHlprs { get; private set; }
        public DeviceHelpers DeviceHlprs { get; private set; }
        public LocalFileHelpers LocalFileHlprs { get; private set; }
        public IGeneralSettings GeneralSettings { get; }
        public LightingProfileManagerViewModel LightingProfileManagerViewModel { get; set; }
        public PlaylistDecoder LightingPlayer { get; set; }
        public CollectionItemStore ItemStore { get; set; }
        public IDialogService DialogService { get; }
        public ISerialStream[] SerialStreams { get; }

        public object this[string propertyName] {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName]
                // instead of the following
                Type myType = typeof(MainViewViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }

            set
            {
                Type myType = typeof(MainViewViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }

        public MainViewViewModel(IGeneralSettings generalSettings,
            IList<ISelectableViewPart> selectableViewParts,
            IDialogService dialogService,
            LightingProfileManagerViewModel lightingProfileManagerViewModel,
            CollectionItemStore collectionItemStore,
            PlaylistDecoder playlistDecoder
            )
        {
            #region load Params
            DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            LightingProfileManagerViewModel = lightingProfileManagerViewModel ?? throw new ArgumentNullException(nameof(lightingProfileManagerViewModel));
            LightingPlayer = playlistDecoder ?? throw new ArgumentNullException(nameof(playlistDecoder));
            ItemStore = collectionItemStore ?? throw new ArgumentNullException(nameof(collectionItemStore));
            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order)
                .ToList();
            SelectedViewPart = SelectableViewParts.First();
            ReadData();
            #endregion load Params

            #region Registering devices

            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            AvailableDevices.CollectionChanged += async (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        var newDevices = e.NewItems;

                        foreach (IDeviceSettings device in newDevices)
                        {
                            Log.Information("Adding device to Dashboard...", device.DeviceName);
                            SetSearchingScreenProgressText("Adding device to Dashboard...");
                            SetSearchingScreenHeaderText("Adding Device", true);

                            await Task.Run(() => RegisterDevice(device));
                            string finalInfo = "";
                            finalInfo += "Device: ";
                            finalInfo += Environment.NewLine;
                            finalInfo += "Name: " + device.DeviceName;
                            finalInfo += Environment.NewLine;
                            finalInfo += "ID: " + device.DeviceSerial;
                            finalInfo += Environment.NewLine;
                            finalInfo += "Firmware: " + device.FirmwareVersion;
                            finalInfo += Environment.NewLine;
                            finalInfo += "Hardware: " + device.HardwareVersion;
                            finalInfo += Environment.NewLine;
                            finalInfo += "Enjoy!";
                            SetSearchingScreenProgressText(finalInfo);
                            SetSearchingScreenHeaderText("Done!", false);

                        }
                        break;
                }
            };

            #endregion Registering devices
            #region registering General settings

            RegisterGeneralSettings(GeneralSettings);

            #endregion registering General settings

            #region Create Resource and Collections Folder

            CreateFWToolsFolderAndFiles();

            #endregion Create Resource and Collections Folder
        }

        public void SetGifxelationPreviewImage(ByteFrame frame)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (GifxelationBitmap == null)
                {
                    GifxelationBitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else
                {
                    if (frame != null)
                    {
                        GifxelationBitmap.Lock();
                        IntPtr pixelAddress = GifxelationBitmap.BackBuffer;
                        Marshal.Copy(frame.Frame, 0, pixelAddress, frame.Frame.Length);

                        GifxelationBitmap.AddDirtyRect(new Int32Rect(0, 0, frame.FrameWidth, frame.FrameHeight));

                        GifxelationBitmap.Unlock();
                        //ShaderBitmap = MatrixBitmap;
                        //RaisePropertyChanged(() => DeviceRectWidthMax);
                        //RaisePropertyChanged(() => DeviceRectHeightMax);
                    }
                    else
                    {
                        //notify the UI show error message
                    }
                }
            });
        }

        private void WriteDeviceInfo(IDeviceSettings device)
        {
            // create directory based on device name and UID
            //check if device collection folder path is exist
            if (!Directory.Exists(DevicesCollectionFolderPath))
            {
                Directory.CreateDirectory(DevicesCollectionFolderPath);
            }
            var directory = Path.Combine(DevicesCollectionFolderPath, device.DeviceName + "-" + device.DeviceUID);
            Directory.CreateDirectory(directory);

            var lightingOutputDirectory = Path.Combine(directory, "LightingOutputs"); //contains lighting controller info
            foreach (var output in device.AvailableLightingOutputs)
            {
                var outputDirectory = Path.Combine(lightingOutputDirectory, output.OutputName + "_" + output.OutputID.ToString());
                Directory.CreateDirectory(outputDirectory);
                //write output infojson
                DeviceHlprs.WriteSingleOutputInfoJson(output, device);
                var slaveDeviceDirectory = Path.Combine(outputDirectory, "AttachedDevice");
                Directory.CreateDirectory(slaveDeviceDirectory);
                DeviceHlprs.WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            }
            var pwmController = device.AvailableControllers.Where(c => c.Type == ControllerTypeEnum.PWMController).FirstOrDefault();
            if (pwmController != null)
            {
                var pwmOutputDirectory = Path.Combine(directory, "PWMOutputs"); //contains pwm controller info
                foreach (var output in device.AvailablePWMOutputs)
                {
                    var outputDirectory = Path.Combine(pwmOutputDirectory, output.OutputName + "_" + output.OutputID.ToString());
                    Directory.CreateDirectory(outputDirectory);
                    //write output infojson
                    DeviceHlprs.WriteSingleOutputInfoJson(output, device);

                    //write slave device config
                    var slaveDeviceDirectory = Path.Combine(outputDirectory, "AttachedDevice");
                    Directory.CreateDirectory(slaveDeviceDirectory);
                    DeviceHlprs.WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                }
            }

            // finally write infojson
            DeviceHlprs.WriteSingleDeviceInfoJson(device);
            Log.Information("Device information saved!", device.DeviceName);
        }

        #region component register

        /// <summary>
        /// this region contains all function need to call when a new component is added
        /// </summary>
        ///
        private void RegisterGeneralSettings(IGeneralSettings generalSettings)

        {
            generalSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(generalSettings.ThemeIndex):
                        if (generalSettings.ThemeIndex == 0)
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        else
                        {
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }
                        break;

                    case nameof(generalSettings.Autostart):
                        if (generalSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(generalSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;
                    case nameof(generalSettings.CurrentAppUser):
                        if (FTPHlprs != null)
                        {
                            if (FTPHlprs.sFTP.IsConnected)
                                FTPHlprs.sFTP.Disconnect();
                            FTPHlprs = null;

                        }
                        break;
                    case nameof(generalSettings.StartupDelaySecond):
                        if (generalSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(generalSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;

                    case nameof(generalSettings.HotkeyEnable):
                        if (generalSettings.HotkeyEnable)
                        {
                            KeyboardHookManagerSingleton.Instance.Start();
                            Register();
                        }
                        else
                        {
                            KeyboardHookManagerSingleton.Instance.Stop();
                            Unregister();
                        }
                        break;

                    default:
                        break;
                }
            };
        }
        private void CreateDeviceShutdownAction(IDeviceSettings device)
        {
            var shutdownAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.Shutdown).FirstOrDefault();
            if (shutdownAutomation == null)
                return;
            if (shutdownAutomation.Actions.Count == 0)
                return;
            if (shutdownAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceShutdownAction = ObjectHelpers.Clone<ActionSettings>(shutdownAutomation.Actions[0]);
            shutdownAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceShutdownAction.TargetDeviceName = device.DeviceName;
            newDeviceShutdownAction.TargetDeviceUID = device.DeviceUID;
            shutdownAutomation.Actions.Add(newDeviceShutdownAction);
            //lock the automation
            shutdownAutomation.IsLocked = true;
        }
        private void CreateDeviceMonitorSleepAction(IDeviceSettings device)
        {
            var monitorSleepAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorSleep).FirstOrDefault();
            if (monitorSleepAutomation == null)
                return;
            if (monitorSleepAutomation.Actions.Count == 0)
                return;
            if (monitorSleepAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceMonitorSleepAction = ObjectHelpers.Clone<ActionSettings>(monitorSleepAutomation.Actions[0]);
            monitorSleepAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceMonitorSleepAction.TargetDeviceName = device.DeviceName;
            newDeviceMonitorSleepAction.TargetDeviceUID = device.DeviceUID;
            monitorSleepAutomation.Actions.Add(newDeviceMonitorSleepAction);
            //lock the automation
            monitorSleepAutomation.IsLocked = true;
        }
        private void CreateDeviceMonitorWakeupAction(IDeviceSettings device)
        {
            var monitorWakeupAutomation = AvailableAutomations.Where(a => (a.Condition is SystemEventTriggerCondition) && (a.Condition as SystemEventTriggerCondition).Event == SystemEventEnum.MonitorWakeup).FirstOrDefault();
            if (monitorWakeupAutomation == null)
                return;
            if (monitorWakeupAutomation.Actions.Count == 0)
                return;
            if (monitorWakeupAutomation.Actions.Any(a => a.TargetDeviceUID == device.DeviceUID))
                return;
            var newDeviceMonitorWakeupAction = ObjectHelpers.Clone<ActionSettings>(monitorWakeupAutomation.Actions[0]);
            monitorWakeupAutomation.UpdateActions(AvailableDevices.ToList());
            newDeviceMonitorWakeupAction.TargetDeviceName = device.DeviceName;
            newDeviceMonitorWakeupAction.TargetDeviceUID = device.DeviceUID;
            monitorWakeupAutomation.Actions.Add(newDeviceMonitorWakeupAction);
            //lock the automation
            monitorWakeupAutomation.IsLocked = true;
        }

        private async Task RegisterDevice(IDeviceSettings device)
        {
            //add device to default shutdown automation
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CreateDeviceShutdownAction(device);
                CreateDeviceMonitorSleepAction(device);
                CreateDeviceMonitorWakeupAction(device);
            });
            device.PropertyChanged += (_, __) =>
            {
                //WriteSingleDeviceInfoJson(device);
                switch (__.PropertyName)
                {
                    case nameof(device.CurrentActiveControlerIndex):
                        {
                            //update liveview
                            GetItemsForLiveView();
                            UpdateLiveView();
                        }
                        break;
                }
            };

            foreach (var controller in device.AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    RegisterOutput(output, device);
                }
            }
            if (device.ControlZoneGroups != null)
            {
                foreach (var group in device.ControlZoneGroups)
                {
                    //get group Items
                    group.Init(device);
                    await Task.Run(() => group.RegisterGroupItem());
                }
            }
        }

        private void RegisterOutput(IOutputSettings output, IDeviceSettings device)
        {
            output.PropertyChanged += (_, __) =>
            {
                // WriteSingleOutputInfoJson(output, device);

                switch (__.PropertyName)
                {
                    case nameof(output.SlaveDevice): // in case slave device changed to completely new one
                                                     // WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                        RegisterSlaveDevice(output.SlaveDevice, output, device);
                        break;
                }
            };
            RegisterSlaveDevice(output.SlaveDevice, output, device);
        }

        private void RegisterSlaveDevice(ISlaveDevice slaveDevice, IOutputSettings output, IDeviceSettings device)
        {
            slaveDevice.PropertyChanged += (_, __) =>
            {
                //this master change in position reflect other output type of the device
                if (output.OutputType == OutputTypeEnum.ARGBLEDOutput)
                {
                    var ledSlaveDevice = output.SlaveDevice as IDrawable;
                    switch (__.PropertyName)
                    {
                        case nameof(ledSlaveDevice.Left):
                        case nameof(ledSlaveDevice.Top):
                        case nameof(ledSlaveDevice.Width):
                        case nameof(ledSlaveDevice.Height):
                            if (device.AvailablePWMOutputs.Count() > 0)
                            {
                                var pwmSlaveDevice = device.AvailablePWMOutputs.Where(o => o.OutputID == output.OutputID).FirstOrDefault().SlaveDevice;
                                if (pwmSlaveDevice != null)
                                {
                                    (pwmSlaveDevice as IDrawable).Left = ledSlaveDevice.Left;
                                    (pwmSlaveDevice as IDrawable).Top = ledSlaveDevice.Top;
                                    (pwmSlaveDevice as IDrawable).Width = ledSlaveDevice.Width;
                                    (pwmSlaveDevice as IDrawable).Height = ledSlaveDevice.Height;
                                }

                            }
                            break;
                    }
                }

            };
            foreach (var zone in output.SlaveDevice.ControlableZones)
            {
                RegisterZone(zone, slaveDevice, output, device);
            }
        }

        private void RegisterZone(IControlZone zone, ISlaveDevice slaveDevice, IOutputSettings output, IDeviceSettings device)
        {
            //foreach (var mode in zone.AvailableControlMode)
            //{
            //    mode.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            //    foreach (var param in mode.Parameters)
            //    {
            //        param.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            //        if (param.SubParams != null)
            //        {
            //            foreach (var subParam in param.SubParams)
            //            {
            //                subParam.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            //            }
            //        }
            //    }
            //}
        }

        #endregion component register

        #region writing database

        public void WriteSettingJson(IGeneralSettings generalSettings)
        {
            var json = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGeneralFileNameAndPath, json);
        }

        #endregion writing database

        private bool _searchingForDevices;

        public bool SearchingForDevices {
            get { return _searchingForDevices; }

            set
            {
                _searchingForDevices = value;
                RaisePropertyChanged();
            }
        }
        private DeviceSearchingScreen _searchingForDeviceScreen { get; set; }
        public void ShowSearchingScreen()
        {
            SearchingForDevices = false;
            if (_searchingForDeviceScreen == null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _searchingForDeviceScreen = new DeviceSearchingScreen();
                    _searchingForDeviceScreen.DataContext = this;
                    _searchingForDeviceScreen.Owner = System.Windows.Application.Current.MainWindow;
                    _searchingForDeviceScreen.Show();
                });
            }
        }
        public void CloseSearchingScreen()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (_searchingForDeviceScreen != null)
                {
                    _searchingForDeviceScreen.Close();
                    _searchingForDeviceScreen = null;
                }

            });

        }
        private ObservableCollection<IDeviceSettings> _availableDefaultDevices;
        public ObservableCollection<IDeviceSettings> AvailableDefaultDevices {
            get
            {
                return _availableDefaultDevices;
            }
            set
            {
                _availableDefaultDevices = value;
                RaisePropertyChanged();
            }
        }
        private void GetAvailableDefaultDevice()
        {
            var ambinoBasic = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                new DeviceType(DeviceTypeEnum.AmbinoBasic),
                "Ambino Basic",
                "Không có",
                false,
                true,
                1);
            ambinoBasic.DashboardWidth = 230;
            ambinoBasic.DashboardHeight = 270;
            ambinoBasic.HardwareVersion = "unknown";
            ambinoBasic.FirmwareVersion = "unknown";
            ambinoBasic.DeviceSerial = "unknown";
            var ambinoEdge = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
             new DeviceType(DeviceTypeEnum.AmbinoEDGE),
             "Ambino EDGE",
             "Không có",
             false,
             true,
             1);
            ambinoEdge.DashboardWidth = 230;
            ambinoEdge.DashboardHeight = 270;
            ambinoEdge.HardwareVersion = "unknown";
            ambinoEdge.FirmwareVersion = "unknown";
            ambinoEdge.DeviceSerial = "unknown";
            var ambinoFanhub = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
               new DeviceType(DeviceTypeEnum.AmbinoFanHub),
            "Ambino FanHub",
               "Không có",
               true,
               true,
               10);
            ambinoFanhub.DashboardWidth = 472;
            ambinoFanhub.DashboardHeight = 270;
            ambinoFanhub.HardwareVersion = "unknown";
            ambinoFanhub.FirmwareVersion = "unknown";
            ambinoFanhub.DeviceSerial = "unknown";

            var ambinoHUBV2 = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
             new DeviceType(DeviceTypeEnum.AmbinoHUBV2),
          "Ambino HubV2",
             "Không có",
             false,
             true,
             7);
            ambinoHUBV2.DashboardWidth = 320;
            ambinoHUBV2.DashboardHeight = 270;
            ambinoHUBV2.HardwareVersion = "unknown";
            ambinoHUBV2.FirmwareVersion = "unknown";
            ambinoHUBV2.DeviceSerial = "unknown";
            var ambinoHUBV3 = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
              new DeviceType(DeviceTypeEnum.AmbinoHUBV3),
           "Ambino HubV3",
              "Không có",
              false,
              true,
              4);
            ambinoHUBV3.DashboardWidth = 320;
            ambinoHUBV3.DashboardHeight = 270;
            ambinoHUBV3.HardwareVersion = "unknown";
            ambinoHUBV3.FirmwareVersion = "unknown";
            ambinoHUBV3.DeviceSerial = "unknown";
            AvailableDefaultDevices.Add(ambinoBasic);
            AvailableDefaultDevices.Add(ambinoEdge);
            AvailableDefaultDevices.Add(ambinoFanhub);
            AvailableDefaultDevices.Add(ambinoHUBV2);
            AvailableDefaultDevices.Add(ambinoHUBV3);
        }
        public void LoadAvailableDefaultDevices()
        {
            AvailableDefaultDevices = new ObservableCollection<IDeviceSettings>();
            GetAvailableDefaultDevice();

        }
        public void SetSearchingScreenHeaderText(string text, bool isLoading)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                DeviceSearchingHeaderText = text;
                SearchingForDevices = isLoading;
            });
        }
        public void SetSearchingScreenProgressText(string text)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                DeviceSearchingProgressText = text;

            });
        }
        private string _deviceSearchingHeaderText = "";

        public string DeviceSearchingHeaderText {
            get { return _deviceSearchingHeaderText; }

            set
            {
                _deviceSearchingHeaderText = value;
                RaisePropertyChanged();
            }
        }
        private string _deviceSearchingProgressText = "";

        public string DeviceSearchingProgressText {
            get { return _deviceSearchingProgressText; }

            set
            {
                _deviceSearchingProgressText = value;
                RaisePropertyChanged();
            }
        }
        private int _noDeviceDetectedCounter;
        private bool _isDeviceDiscoveryInit;
        public bool IsDeviceDiscoveryInit {
            get
            {
                return _isDeviceDiscoveryInit;
            }
            set
            {
                _isDeviceDiscoveryInit = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<SftpFile> _possibleMatchedDevices;
        public ObservableCollection<SftpFile> PossibleMatchedDevices {
            get
            {
                return _possibleMatchedDevices;
            }
            set
            {
                _possibleMatchedDevices = value;
                RaisePropertyChanged();
            }
        }
        private SftpFile _selectedmatchedDevice;
        public SftpFile SelectedMatchedDevice {
            get
            {
                return _selectedmatchedDevice;
            }
            set
            {
                _selectedmatchedDevice = value;
                RaisePropertyChanged();
            }
        }
        private async Task<bool> DownloadDeviceInfo(IDeviceSettings device)
        {
            bool result;
            PossibleMatchedDevices = new ObservableCollection<SftpFile>();
            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                SFTPConnect();
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    result = false;
                    return await Task.FromResult(result);
                }
            }
            SftpFile matchedFile = null;
            string deviceFolderPath = string.Empty;
            switch (device.DeviceType.ConnectionTypeEnum)
            {
                case DeviceConnectionTypeEnum.OpenRGB:
                    matchedFile = await FTPHlprs.GetFileByNameMatching(device.DeviceName + ".zip", openRGBDevicesFolderPath + "/" + device.DeviceType.Type.ToString());
                    deviceFolderPath = openRGBDevicesFolderPath + "/" + device.DeviceType.Type.ToString();
                    break;
                case DeviceConnectionTypeEnum.Wired:
                    matchedFile = await FTPHlprs.GetFileByNameMatching(device.DeviceName + ".zip", ambinoDevicesFolderPath + "/" + device.DeviceType.Type.ToString());
                    deviceFolderPath = ambinoDevicesFolderPath + "/" + device.DeviceType.Type.ToString();
                    break;
            }
            if (matchedFile == null)
            //return available device instead
            {
                var availableFiles = await FTPHlprs.GetAllFilesInFolder(deviceFolderPath);
                if (availableFiles == null)
                {
                    result = false;
                    return await Task.FromResult(result);
                }
                foreach (var file in availableFiles)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PossibleMatchedDevices.Add(file);
                    });
                }
                if (PossibleMatchedDevices != null && PossibleMatchedDevices.Count > 0)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        possibleMatchedDeviceSelection = new PossibleMatchedDeviceSelectionWindow();
                        possibleMatchedDeviceSelection.Owner = _searchingForDeviceScreen;
                        bool? dialogResult = possibleMatchedDeviceSelection.ShowDialog();
                        if (dialogResult == true)
                        {
                            matchedFile = SelectedMatchedDevice;
                        }
                    });
                }
            }
            //if user decide to cancel or nothing exist
            if (matchedFile == null)
            {
                result = false;
                return await Task.FromResult(result);
            }
            //if nothing wrong, download to cache
            ClearCacheFolder();
            FTPHlprs.DownloadFile(matchedFile.FullName, Path.Combine(CacheFolderPath, matchedFile.Name), DownloadProgresBar);
            device.DeviceName = Path.GetFileNameWithoutExtension(Path.Combine(CacheFolderPath, matchedFile.Name));
            result = true;
            return await Task.FromResult(result);
        }
        private PossibleMatchedDeviceSelectionWindow possibleMatchedDeviceSelection { get; set; }
        private bool _isRescanningDevices;
        public bool IsRescanningDevices {
            get
            {
                return _isRescanningDevices;
            }
            set
            {
                _isRescanningDevices = value;
                RaisePropertyChanged();
            }
        }
        public event EventHandler RequestingDeviceRescanEvent;
        public void OnRequestingDeviceRescanEvent()
        {
            EventHandler handler = RequestingDeviceRescanEvent;
            if (null != handler) handler(this, EventArgs.Empty);
        }
        public async Task FoundNewDevice(List<IDeviceSettings> newDevices)
        {
            if (IsLoadingProfile)
                return;
            if (newDevices != null && newDevices.Count > 0)
            {
                IsDeviceDiscoveryInit = false;
                foreach (var newDevice in newDevices)
                {
                    var device = newDevice as IDeviceSettings;
                    SetSearchingScreenHeaderText("New device found: " + device.DeviceName, true);
                    //download device info
                    SetSearchingScreenHeaderText("Downloading device modules: " + device.DeviceName, true);
                    var result = await DownloadDeviceInfo(device);
                    if (!result)
                    {
                        SetSearchingScreenHeaderText("Using Default: " + device.DeviceName, true);
                    }
                    else
                    {
                        SetSearchingScreenHeaderText("Device modules downloaded: " + device.DeviceName, true);
                        var downloadedDevice = ImportDevice(Path.Combine(CacheFolderPath, device.DeviceName + ".zip"));
                        if (downloadedDevice != null)
                        {
                            //transplant
                            downloadedDevice.OutputPort = device.OutputPort;
                            downloadedDevice.FirmwareVersion = device.FirmwareVersion;
                            downloadedDevice.HardwareVersion = device.HardwareVersion;
                            downloadedDevice.DeviceSerial = device.DeviceSerial;
                            downloadedDevice.DeviceType.ConnectionTypeEnum = device.DeviceType.ConnectionTypeEnum;
                            device = downloadedDevice;
                        }
                    }
                    device.IsTransferActive = true;

                    if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3)
                    {
                        GeneralSettings.IsOpenRGBEnabled = true;
                    }
                    SetSearchingScreenProgressText("Writing device information...");

                    //await Task.Delay(TimeSpan.FromSeconds(2));
                    lock (device)
                    {
                        WriteDeviceInfo(device);
                    }


                    lock (AvailableDeviceLock)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            AvailableDevices.Insert(0, device);
                        });
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                SearchingForDevices = false;
                IsDeviceDiscoveryInit = true;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            else
            {
                if (_noDeviceDetectedCounter < 3)
                    _noDeviceDetectedCounter++;
                if (_noDeviceDetectedCounter >= 3)
                {
                    SearchingForDevices = false;
                    IsDeviceDiscoveryInit = true;
                }
            }
        }

        public void OldDeviceReconnected(List<string> oldDevices)
        {
            if (IsLoadingProfile)
                return;

            if (oldDevices != null && oldDevices.Count > 0)
            {
                foreach (var port in oldDevices)
                {
                    //set first device found active again since it's recconected

                    var oldDevice = AvailableDevices.Where(p => p.OutputPort == port).FirstOrDefault();
                    // oldDevice.IsTransferActive = false;
                    //Thread.Sleep(500);
                    if (!oldDevice.IsTransferActive)
                    {
                        oldDevice.IsTransferActive = true;
                        if (oldDevice.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3)
                        {
                            GeneralSettings.IsOpenRGBEnabled = true;
                        }
                        //Thread.Sleep(500);
                        SetSearchingScreenProgressText("Connected: " + oldDevice.OutputPort);

                        lock (oldDevice)
                            DeviceHlprs.WriteSingleDeviceInfoJson(oldDevice);
                    }

                }

                SearchingForDevices = false;
                IsDeviceDiscoveryInit = true;
            }

        }

        public object AudioUpdateLock { get; } = new object();
        public object AvailableDeviceLock { get; } = new object();
        public VisualizerDataModel CurrentVisualizer { get; set; }
        public void AudioVisualizerUpdate(ByteFrame frame, int index)
        {
            if (AudioVisualizers == null)
                return;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                foreach (var column in AudioVisualizers[index].Columns)
                {
                    column.SetValue(frame.Frame[column.Index]);
                }


            });
        }

        private ObservableCollection<AudioDevice> _availableAudioDevices;

        public ObservableCollection<AudioDevice> AvailableAudioDevices {
            get { return _availableAudioDevices; }

            set
            {
                _availableAudioDevices = value;
                RaisePropertyChanged();
            }
        }

        public List<AudioDevice> GetAvailableAudioDevices()
        {
            var availableDevices = new List<AudioDevice>();
            int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();

            for (int i = 0; i < devicecount; i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                if (device.IsEnabled && device.IsLoopback)
                {
                    var audioDevice = new AudioDevice() { Name = device.name, Index = i };

                    availableDevices.Add(audioDevice);
                }
            }
            return availableDevices;
        }

        public void DesktopsPreviewUpdate(ByteFrame frame, int index)
        {
            if (frame == null)
                return;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                lock (AvailableBitmaps)
                {
                    if (index >= AvailableBitmaps.Count())
                    {
                        return;
                    }
                    if (AvailableBitmaps[index] == null)
                    {
                        AvailableBitmaps[index].Bitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                        UpdateRegionView();
                    }
                    else if (AvailableBitmaps[index].Bitmap != null && (AvailableBitmaps[index].Bitmap.Width != frame.FrameWidth || AvailableBitmaps[index].Bitmap.Height != frame.FrameHeight))
                    {
                        AvailableBitmaps[index].Bitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                        UpdateRegionView();
                    }
                    else if (AvailableBitmaps[index].Bitmap != null && AvailableBitmaps[index].Bitmap.Width == frame.FrameWidth && AvailableBitmaps[index].Bitmap.Height == frame.FrameHeight)
                    {
                        if (frame != null)
                        {
                            AvailableBitmaps[index].Bitmap.Lock();
                            IntPtr pixelAddress = AvailableBitmaps[index].Bitmap.BackBuffer;
                            Marshal.Copy(frame.Frame, 0, pixelAddress, frame.Frame.Length);

                            AvailableBitmaps[index].Bitmap.AddDirtyRect(new Int32Rect(0, 0, frame.FrameWidth, frame.FrameHeight));

                            AvailableBitmaps[index].Bitmap.Unlock();

                        }
                        else
                        {
                            //notify the UI show error message
                        }
                    }
                }

            });
        }

        private int _currentLEDEditWizardState = 0;

        public int CurrentLEDEditWizardState {
            get => _currentLEDEditWizardState;

            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentLEDEditWizardState, value);
                RaisePropertyChanged();
            }
        }

        private int _currentAddDeviceWizardState = 0;

        public int CurrentAddDeviceWizardState {
            get => _currentAddDeviceWizardState;

            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentAddDeviceWizardState, value);
                RaisePropertyChanged();
            }
        }

        private int _count = 0;

        public int Count {
            get => _count;

            set
            {
                Set(ref _count, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private int _countMID = 0;

        public int CountMID {
            get => _countMID;

            set
            {
                Set(ref _countMID, value);
            }
        }

        private int _countVID = 0;

        public int CountVID {
            get => _countVID;

            set
            {
                Set(ref _countVID, value);
            }
        }

        private int _countPID = 0;

        public int CountPID {
            get => _countPID;

            set
            {
                Set(ref _countPID, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private int _gapVID = 5;

        public int GapVID {
            get => _gapVID;

            set
            {
                Set(ref _gapVID, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private int _maxLEDCount = 0;

        public int MaxLEDCount {
            get => _maxLEDCount;

            set
            {
                _maxLEDCount = value;
                RaisePropertyChanged(nameof(MaxLEDCount));
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private bool _isRichCanvasWindowOpen = false;

        public bool IsRichCanvasWindowOpen {
            get => _isRichCanvasWindowOpen;

            set
            {
                Set(ref _isRichCanvasWindowOpen, value);
                RaisePropertyChanged();
            }
        }

        private AutomationSettings _currentSelectedAutomation;

        public AutomationSettings CurrentSelectedAutomation {
            get => _currentSelectedAutomation;

            set
            {
                Set(ref _currentSelectedAutomation, value);

            }
        }


        private string _nameToChange;

        public string NameToChange {
            get
            {
                return _nameToChange;
            }

            set
            {
                _nameToChange = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileName = "New Profile";

        public string NewProfileName {
            get
            {
                return _newProfileName;
            }

            set
            {
                _newProfileName = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileOwner;

        public string NewProfileOwner {
            get
            {
                return _newProfileOwner;
            }

            set
            {
                _newProfileOwner = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileDescription;

        public string NewProfileDescription {
            get
            {
                return _newProfileDescription;
            }

            set
            {
                _newProfileDescription = value;

                RaisePropertyChanged();
            }
        }

        private List<IDeviceSpot> _backupSpots;

        public List<IDeviceSpot> BackupSpots {
            get
            {
                return _backupSpots;
            }

            set
            {
                _backupSpots = value;

                RaisePropertyChanged();
            }
        }

        private Color _selectedSolidColor;

        public Color SelectedSolidColor {
            get
            {
                return _selectedSolidColor;
            }

            set
            {
                _selectedSolidColor = value;
                //CurrentOutput.OutputStaticColor = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// new Palette
        /// </summary>
        private string _newPaletteName = "New Palette" + DateTime.Now.ToString("yyyyMMdd");

        public string NewPaletteName {
            get { return _newPaletteName; }

            set
            {
                _newPaletteName = value;
            }
        }

        private string _newEffectName;

        public string NewEffectName {
            get { return _newEffectName; }

            set
            {
                _newEffectName = value;
            }
        }

        private string _newPaletteOwner;

        public string NewPaletteOwner {
            get { return _newPaletteOwner; }

            set
            {
                _newPaletteOwner = value;
            }
        }

        private string _newEffectOwner;

        public string NewEffectOwner {
            get { return _newEffectOwner; }

            set
            {
                _newEffectOwner = value;
            }
        }

        private string _newPaletteDescription;

        public string NewPaletteDescription {
            get { return _newPaletteDescription; }

            set
            {
                _newPaletteDescription = value;
            }
        }

        private string _newEffectDescription;

        public string NewEffectDescription {
            get { return _newEffectDescription; }

            set
            {
                _newEffectDescription = value;
            }
        }

        /// <summary>
        ///
        /// new Automation
        /// </summary>
        private string _newAutomationName = "Shortcut";

        public string NewAutomationName {
            get { return _newAutomationName; }

            set
            {
                _newAutomationName = value;
            }
        }

        private int _rangeMinValue = 0;

        public int RangeMinValue {
            get { return _rangeMinValue; }

            set
            {
                _rangeMinValue = value;
            }
        }

        private int _iDMinValue = 0;

        public int IDMinValue {
            get { return _iDMinValue; }

            set
            {
                _iDMinValue = value;
            }
        }

        private int _rangeMaxValue = 100;

        public int RangeMaxValue {
            get { return _rangeMaxValue; }

            set
            {
                _rangeMaxValue = value;
            }
        }

        private bool _outputModeChangeEnable;

        public bool OutputModeChangeEnable {
            get { return _outputModeChangeEnable; }

            set
            {
                _outputModeChangeEnable = value;
                RaisePropertyChanged();
            }
        }

        private Color _currentStartColor;

        public Color CurrentStartColor {
            get { return _currentStartColor; }

            set
            {
                _currentStartColor = value;
                RaisePropertyChanged();
            }
        }

        private bool[] _selectedSpotData = new bool[4];

        public bool[] SelectedSpotData {
            get { return _selectedSpotData; }

            set
            {
                _selectedSpotData = value;
                RaisePropertyChanged();
            }
        }

        private Color _currentStopColor;

        public Color CurrentStopColor {
            get { return _currentStopColor; }

            set
            {
                _currentStopColor = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNextable {
            get
            {
                if (CurrentLEDEditWizardState == 0)
                    return Count != 0;
                else if (CurrentLEDEditWizardState == 1)
                    return MaxLEDCount == 0;
                else
                    return true;
            }
        }

        public void ReadData()
        {
            LoadContextMenu();
            LoadData();

            #region Command setup

            SelecFirmwareForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //set device hardware version with one selected in the list
                CurrentDevice.HardwareVersion = CurrentSelectedFirmware.TargetHardware;
                // lauch firmware upgrade
                UpgradeIfAvailable(CurrentDevice);
            });
            ApplyDeviceHardwareSettingsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ApplyDeviceHardwareSettings(CurrentDevice);
            });
            UpdateCurrentSelectedDeviceFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UpgradeIfAvailable(CurrentDevice);
            });
            OpenSurfaceEditorWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSurfaceEditorWindow();
            });
            OpenAddNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAddNewAutomationWindowCommand();
            });
            OpenAppSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAppSettingsWindow();
            });
            OpenDebugWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenDebugWindow();
            });

            OpenLogFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var logPath = Path.Combine(JsonPath, "Logs");
                Process.Start("explorer.exe", logPath);
            });
            SaveAllAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveAllAutomation();
            });
            SaveCurrentSelectedAutomationShortkeyCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSelectedAutomationShortkey();
            });
            CloseIDSetupCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (LiveViewItems.Contains(EraserBrush))
                    LiveViewItems.Remove(EraserBrush);
                if (LiveViewItems.Contains(IDEditBrush))
                    LiveViewItems.Remove(IDEditBrush);
                foreach (var item in LiveViewItems)
                {
                    item.IsSelectable = true;
                    item.IsSelected = false;
                }
                IsInIDEditStage = false;
            });
            ResetAllItemsIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                VIDCount = 0;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    foreach (var item in LiveViewItems.Where(i => i is LEDSetup))
                    {
                        (item as LEDSetup).ResetVIDStage();
                    }
                }
            });
            RefreshCurrentCollectionCommand = new RelayCommand<IModeParameter>((p) =>
            {
                return true;
            }, (p) =>
            {
                var listParam = p as ListSelectionParameter;
                listParam.LoadAvailableValues();
            });
            ListSelectionItemMouseEnterCommand = new RelayCommand<IParameterValue>((p) =>
            {
                return true;
            }, async (p) =>
            {
                //play gif here
                if (p is Gif)
                {
                    var gif = p as Gif;
                    await gif.PlayGif(5);
                }
            });
            ListSelectionItemMouseLeaveCommand = new RelayCommand<IParameterValue>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p is Gif)
                {
                    var gif = p as Gif;
                    gif.DisposeGif();
                }

            });
            RefreshAudioDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateAudioDevicesCollection();
            });
            RefreshMonitorCollectionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ScreenBitmapCollectionInit();
                UpdateRegionView();
            });
            RefreshLocalSlaveDeviceCollectionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                RefreshLocalSlaveDeviceCollection();
            });
            DeleteSelectedItemFromCurrentCollectionCommand = new RelayCommand<IModeParameter>((p) =>
            {
                return true;
            }, (p) =>
            {
                var listParam = p as ListSelectionParameter;
                int removeFileCount = 0;
                int totalFileCount = listParam.AvailableValues.Count;
                var itemsToRemove = listParam.AvailableValues.Where(i => i.IsChecked).ToList();
                foreach (var item in itemsToRemove)
                {
                    if (removeFileCount == totalFileCount - 1)
                        return;
                    ItemStore.RemoveItems(new List<IGenericCollectionItem>() { item });
                    if (File.Exists(item.InfoPath))
                        File.Delete(item.InfoPath);
                    listParam.DeletedSelectedItem(item);
                    removeFileCount++;
                }

            });
            SubParamActionCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     switch (p)
                     {
                         case "Add Color":
                             OpenColorPickerWindow(2);
                             break;

                         case "Add VID":
                             IdEditMode = IDMode.VID;
                             IsInIDEditStage = true;
                             break;

                         case "Add FID":
                             IdEditMode = IDMode.FID;
                             IsInIDEditStage = true;
                             break;
                         case "Add Palette":
                             OpenPaletteEditorWindow(16);
                             break;

                         case "Import Palette":
                             ImportPaletteFromFile();
                             break;
                         case "Import Gif":
                             ImportGifFromFile();
                             break;
                         case "Import Pattern":
                             ImportChasingPatternFromFile();
                             break;
                         case "Export Palette":
                             var _paletteControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault();
                             var palette = (_paletteControl as ListSelectionParameter).SelectedValue;
                             ExportItemForOnlineStore(palette);
                             break;
                         case "Export Gif":
                             var _gifControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Gifs).FirstOrDefault();
                             var gif = (_gifControl as ListSelectionParameter).SelectedValue;
                             ExportItemForOnlineStore(gif);
                             break;
                         case "Export Pattern":
                             var _patternControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.ChasingPattern).FirstOrDefault();
                             var pattern = (_patternControl as ListSelectionParameter).SelectedValue;
                             ExportItemForOnlineStore(pattern);
                             break;
                     }
                 });
            OpenExternalWindowsFromModeParameterCommand = new RelayCommand<BaseButtonParameter>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p.CommandParameter)
                {
                    case "screenRegionSelection":
                        ClickedRegionButtonParameter = p as CapturingRegionSelectionButtonParameter;
                        OpenRegionSelectionWindow("screen");
                        break;
                    case "gifRegionSelection":
                        ClickedRegionButtonParameter = p as CapturingRegionSelectionButtonParameter;
                        OpenRegionSelectionWindow("gif");
                        break;

                    case "audioDevice":
                        ClickedAudioButtonParameter = p as AudioDeviceSelectionButtonParameter;
                        OpenAudioSelectorWindow();
                        break;
                }
            });
            AddPickedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (ColorPickerMode)
                {
                    case "color":
                        AddNewColorToCollection();
                        break;

                    case "palette":
                        OpenCreateNewPaletteDialog();
                        break;
                }
            }
            );
            CreateNewPaletteCommand = new RelayCommand<string>((p) =>
                   {
                       return true;
                   }, (p) =>
                   {
                       var newPalette = new ColorPalette(CurrentPaletteNumColor);
                       newPalette.Name = NewPaletteName;
                       newPalette.Description = NewPaletteDescription;
                       newPalette.Owner = NewPaletteOwner;
                       for (int i = 0; i < CurrentPaletteNumColor; i++)
                       {
                           newPalette.Colors[i] = ColorList[i].Color;
                       }
                       AddNewPaletteToCollection(newPalette);
                   });

            CreateNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewAutomation();
            });
            ActivateSelectedProfileCommmand = new RelayCommand<AppProfile>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsLoadingProfile = true;
                await Task.Run(() => { System.Windows.Application.Current.Dispatcher.InvokeAsync(() => ActivateSelectedProfile(p)); });
                IsLoadingProfile = false;
            }
          );

            DeleteAttachedProfileCommand = new RelayCommand<AppProfile>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteAttachedProfile(p);
            }
          );


            SetSelectedMotionCommand = new RelayCommand<ITimeLineDataItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentSelectedMotion = p;
            }
        );
            CompositionNextFrameCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                if (CurrentCompositionFrame < 9600)
                    CurrentCompositionFrame++;
            }
     );

            CutSelectedMotionCommand = new RelayCommand<ITimeLineDataItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CutSelectedMotion(p);
            }
     );
            ToggleCompositionPlayingStateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentCompositionPlayingState = !CurrentCompositionPlayingState;
            }
);
            CompositionPreviousFrameCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                if (CurrentCompositionFrame > 0)
                    CurrentCompositionFrame--;
            }

);
            CompositionFrameStartOverCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentCompositionFrame = 0;
            }

);


            SurfaceEditorWindowClosingCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                IsRichCanvasWindowOpen = false;
            }
    );

            SetSelectedItemScaleFactorCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetSelectedItemScaleFactor(p);
            }
            );
            SetSelectedItemRotateFactorCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetSelectedItemRotateFactor(p);
            }
           );
            ResetToDefaultRectangleScaleCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ResetToDefaultRectangleScale();
            }
            );
            AglignSelectedItemstoLeftCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                AglignSelectedItemstoLeft(p);
            }
            );
            LockSelectedItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                LockSelectedItem(p);
            }
            );
            AddItemsToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddItemsToPIDCanvas();
            }
            );
            AddImageToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddImage();
            }
            );
            AddSpotGeometryCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddSpotGeometry();
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
            DeleteSelectedItemsCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedItems(p);
            }
            );
            OpenImageSelectorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenImageSelector(p);
            });
            ExportCurrentOnlineItemToFilesCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentOnlineItemToFiles();
            });
            ExportItemForOnlineStoreCommand = new RelayCommand<object>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportItemForOnlineStore(p);
            });

            ClearPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ClearPIDCanvas();
            }
            );
            DownloadCurrentItemCommand = new RelayCommand<OnlineItemModel>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await Task.Run(() => DownloadCurrentOnlineItem(p));
            });

            SaveCurretSurfaceLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSurfaceLayout();
            }
            );
            UnlockSelectedItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p)
                {
                    case "PID":
                        UnlockSelectedItem(PIDEditWindowsRichCanvasItems);
                        break;

                    case "SUR":
                        UnlockSelectedItem(SurfaceEditorItems);
                        break;
                }
            }
       );
            SpreadItemRightHorizontalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemHorizontal(p, 0);
            }
            );
            SpreadItemLeftHorizontalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemHorizontal(p, 1);
            }
            );
            SpreadItemUpVerticalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemVertical(p, 1);
            }
           );
            SpreadItemDownVerticalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemVertical(p, 0);
            }
            );
            AglignSelectedItemstoTopCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                AglignSelectedItemstoTop(p);
            }
            );
            ExitCurrentRunningAppCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                System.Windows.Application.Current.Shutdown();
            }
       );

            UpdateAppCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await UpdateApp();
            }
    );

            OpenLEDSteupSelectionWindowsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenLEDSteupSelectionWindows();
            }
          );

            OpenAvailableUpdateListWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsCheckingForUpdate = true;
                UpdateButtonEnable = false;
                AvailableUpdates = null;
                await CheckForUpdate();
                IsCheckingForUpdate = false;
                UpdateButtonEnable = true;
                if (AvailableUpdates == null)
                {
                    return;
                }
                if (AvailableUpdates.ReleasesToApply.Any())
                {
                    OpenAvailableUpdateListWindow();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy bản cập nhật mới nào", "No Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
          );
            ImportDeviceFromFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportDeviceFromLocalFile();
            }
          );
            ImportProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportProfile();
            }
          );
            CreateNewProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewProfile();
            }
            );
            OpenLightingProfileManagerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenLightingProfileManagerWindow();
            }
            );
            CreateNewLightingProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewLightingProfile();
            }
            );

            CreateNewVIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveVID();
            }
            );

            OpenCreateNewLightingProfileWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenCreateNewLightingProfileWindow();
            }
            );
            OpenCreateNewVIDWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenCreateNewVIDWindow();
            }
          );
            OpenProfileCreateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenCreatenewProfileWindow();
            }
           );
            CoppyColorCodeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                System.Windows.Clipboard.SetText(SelectedSolidColor.ToString());
                Growl.Success("Color code copied to clipboard!");
            }
         );

            DeleteSelectedPaletteCommand = new RelayCommand<ColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                //DeleteSelectedPalette(p);
            });

            DeleteSelectedAutomationCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedAutomation(p);
            });

            DeleteSelectedDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedDevice();
            });
            ImportLEDSetupCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportPIDToCanvas();
            });
            AddNewZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddNewZone(PIDEditWindowsRichCanvasItems, PIDEditWindowSelectedItems);
            });
            UnZoneItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UnZone();
            });
            SnapshotCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //SnapShot();
            });

            OpenSpotPIDQuickEDitWindowCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSpotPIDQuickEDitWindow(p);
            });
            RotateSelectedSurfaceEditorItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                RotateSelectedSurfaceEditorItem(p);
            });
            ReflectSelectedSurfaceEditorItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                ReflectSelectedSurfaceEditorItem(p);
            });
            SaveCurrentPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentPID();
            });
            CancelCurrentPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CancelCurrentPID();
            });
            LaunchCompositionEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchCompositionEditWindow();
            });

            UnselectAllLiveiewItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {
                    foreach (var item in LiveViewItems)
                    {
                        item.IsSelected = false;
                    }
                    ShowSelectedItemToolbar = false;
                    SelectedSlaveDevice = null;
                }
            });
            UnselectAllSurfaceEditorItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {
                    foreach (var item in SurfaceEditorItems)
                    {
                        item.IsSelected = false;
                    }
                    SurfaceEditorSelectedDevice = null;
                }
            });
            LiveViewMouseButtonUpCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (LiveViewItems.Where(p => p.IsSelected).Count() > 0)
                    ShowSelectedItemToolbar = true;
                CanUnGroup = false;
                CanGroup = true;

                if (LiveViewItems.Any(p => p.IsSelected && p is Border))
                {
                    CanUnGroup = true;
                }
                //if (LiveViewItems.Any(p => p.IsSelected && p is not Border))
                //{
                //    CanGroup = true;
                //}

            });

            IsolateSelectedItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                IsolateSelectedItems();
                UpdateLiveView();
            });
            ExitIsolateModeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GetItemsForLiveView();
                UpdateLiveView();
            });
            UnGroupZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UngroupZone();
            });

            SelectLiveViewItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                LiveViewSelectedItemChanged(p);
            });
            ChangeSelectedControlZoneActiveControlModeCommand = new RelayCommand<IControlMode>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await ChangeSelectedControlZoneActiveControlMode(p);
            });
            SelectSurfaceEditorItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p.IsSelectable)
                {
                    p.IsSelected = true;
                    SurfaceEditorSelectedDevice = p as ARGBLEDSlaveDevice;
                    SelectedItemScaleValue = SurfaceEditorSelectedDevice.Scale;
                    SelectedItemRotationValue = SurfaceEditorSelectedDevice.Angle;
                }


            });
            SetSelectedSurfaceEditorDevicePreviewColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (SurfaceEditorSelectedDevice != null)
                {
                    var color = new Color();
                    switch (p)
                    {
                        case "red":
                            color = Color.FromRgb(255, 0, 0);
                            break;
                        case "green":
                            color = Color.FromRgb(0, 255, 0);
                            break;
                        case "blue":
                            color = Color.FromRgb(0, 0, 255);
                            break;
                        case "white":
                            color = Color.FromRgb(255, 255, 255);
                            break;
                    }
                    CurrentPreviewColor = color;
                    SurfaceEditorSelectedDevice.SetPreviewColor(CurrentPreviewColor);
                }


            });
            ApplySelectedItemsScaleCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (SurfaceEditorSelectedDevice != null)
                {

                    SurfaceEditorSelectedDevice.ApplyScale(SelectedItemScaleValue);
                }

            });
            ApplySelectedItemsRotationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (SurfaceEditorSelectedDevice != null)
                {
                    SurfaceEditorSelectedDevice.RotateLEDSetup(SelectedItemRotationValue);
                }

            });
            RestoreSelectedSurfaceDeviceSizeAndRotationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (SurfaceEditorSelectedDevice != null)
                {
                    SurfaceEditorSelectedDevice.RotateLEDSetup(0);
                    SurfaceEditorSelectedDevice.ApplyScale(1.0);
                }

            });

            GroupSelectedZoneForMaskedControlCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await MakeNewGroup();
            });
            SelectSlaveDeviceForCurrentOutputCommand = new RelayCommand<ARGBLEDSlaveDevice>((p) =>
            {
                return true;
            }, (p) =>
            {
                //change uid to prevent injection conflict
                foreach (var zone in p.ControlableZones)
                {
                    zone.ZoneUID = Guid.NewGuid().ToString();
                    if (zone.AvailableControlMode == null)
                    {
                        if (CtrlHlprs == null)
                            CtrlHlprs = new ControlModeHelpers();
                        CtrlHlprs.MakeZoneControlable(zone);
                    }
                }
                p.ParrentID = CurrentSelectedOutputMap.OutputID;
                CurrentSelectedOutputMap.SlaveDevice = p;
                SlaveDeviceSelection.Close();
            });
            SelectSlaveDeviceForAllOutputCommand = new RelayCommand<ARGBLEDSlaveDevice>((p) =>
            {
                return true;
            }, (p) =>
            {
                //change uid to prevent injection conflict
                foreach (var output in CurrentDevice.AvailableLightingOutputs)
                {
                    var cloneDevice = ObjectHelpers.Clone<ARGBLEDSlaveDevice>(p);
                    foreach (var zone in cloneDevice.ControlableZones)
                    {
                        zone.ZoneUID = Guid.NewGuid().ToString();
                        if (zone.AvailableControlMode == null)
                        {
                            if (CtrlHlprs == null)
                                CtrlHlprs = new ControlModeHelpers();
                            CtrlHlprs.MakeZoneControlable(zone);
                        }
                    }
                    cloneDevice.ParrentID = output.OutputID;
                    output.SlaveDevice = cloneDevice;
                }
                SlaveDeviceSelection.Close();
            });
            OpenSlaveDeviceSelectorCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                var currentOutput = p as OutputSettings;
                OpenSlaveDeviceSelector(currentOutput);

                //open slave device selection
                //browse online store for items that match this target device
                //add items to collection and display
            });
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

            OpenActionsManagerWindowCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, async (p) =>
            {
                if (!p.IsLocked)
                    await Task.Run(() => OpenActionsManagerWindow(p));
            });
            OpenHotKeySelectionWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenHotKeySelectionWindow();
            });
            OpenTargetDeviceSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetDeviceSelectionWindow(p);
            });
            OpenTargetActionSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetActionSelectionWindow(p);
            });
            OpenAutomationValuePickerWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAutomationValuePickerWindow(p);
            });
            OpenTargetParamSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetParamSelectionWindow(p);
            });

            OpenSpotMapWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSpotMapWindow();
            });
            OpenAutomationManagerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAutomationManagerWindow();
            });
            OpenAmbinoStoreWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAmbinoStoreWindow(p);
            });
            OpenAmbinoStoreWindowForCurrentDeviceCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAmbinoStoreWindow(p);
            });
            SetCurrentActionTypeForSelectedActionCommand = new RelayCommand<ActionType>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionTypeForSelectedAction(p);
            });
            AddSelectedActionTypeToListCommand = new RelayCommand<ActionType>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddSelectedActionTypeToList(p);
            });
            ExecuteAutomationFromManagerCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteAutomationActions(p.Actions);
            });
            UnpinAutomationFromDashboardCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                p.IsPinned = false;
                RaisePropertyChanged(nameof(DashboardPinnedAutomation));
            });
            DeleteSelectedActionFromListCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedActionFromList(p);
            });
            SetCurrentActionTargetDeviceForSelectedActionCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionTargetDeviceForSelectedAction(p);
            });
            SetCurrentActionParamForSelectedActionCommand = new RelayCommand<ActionParameter>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionParamForSelectedAction(p);
            });
            SetCurrentSelectedActionTypeColorValueCommand = new RelayCommand<Color>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentSelectedActionTypeColorValue(p);
            });
            OpenHardwareMonitorWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenHardwareMonitorWindow();
            });
            LaunchWBAdjustWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchWBAdjustWindow();
            });
            SelectOnlineItemCommand = new RelayCommand<OnlineItemModel>((p) =>
            {
                return true;
            }, async (p) =>
            {
                CarouselImageLoading = true;
                await Task.Run(() => gotoItemDetails(p));
                _lastView = CurrentOnlineStoreView;
                CurrentOnlineStoreView = "Details";
            });
            SeAllCarouselItemCommand = new RelayCommand<HomePageCarouselItem>((p) =>
            {
                return true;
            }, async (p) =>
            {
                CarouselImageLoading = true;
                CurrentStoreFilter = new StoreFilterModel() {
                    Name = p.Name,
                    Carousel = p,
                    PageIndex = 1
                };
                await Task.Run(() => SeeAllCarouselItems(p));
                CurrentOnlineStoreView = "Collections";
            });
            FilterStoreItemByTargetDeviceTypeCommand = new RelayCommand<DeviceType>((p) =>
            {
                return true;
            }, async (p) =>
            {
                CarouselImageLoading = true;
                // await Task.Run(() => UpdateStoreView(p));
                CurrentOnlineStoreView = "Collections";
            });
            OpenOOTBCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OOTBStage = 0;
                OpenLEDSteupSelectionWindows();
            });
            SaveDeviceInformationToDiskCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeviceHlprs.WriteSingleDeviceInfoJson(CurrentDevice);
            });
            SelectCardCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return p != null;
            }, async (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    foreach (var device in AvailableDevices) //cancel any selection if it's active
                    {
                        if (device.IsSelected)
                        {
                            device.IsSelected = false;
                            if (SelectedDeviceCount > 0)
                                SelectedDeviceCount--;
                        }
                    }

                    CurrentDevice = p;

                    if (CurrentDevice.IsSizeNeedUserDefine) // this device has default ledsetup so we need to tell user to select the led setup
                    {
                        OOTBStage = 0;
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            OpenLEDSteupSelectionWindows();
                        });
                    }
                    else
                    {
                        GotoChild(p);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl)) // device is selected with ctrl key, start multiple select
                {
                    if (p.IsSelected)
                    {
                        p.IsSelected = false;
                        if (SelectedDeviceCount > 0)
                            SelectedDeviceCount--;
                    }
                    else
                    {
                        p.IsSelected = true;
                        SelectedDeviceCount++;
                    }
                }

                if (SelectedDeviceCount > 0)
                {
                    ToolbarVisible = true;
                }
                else
                {
                    ToolbarVisible = false;
                }
            });

            NextOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (OOTBStage < 2)
                    OOTBStage++;
                if (OOTBStage == 2)
                {
                    IsRichCanvasWindowOpen = true;
                    GetItemsReadyForOOTBQuickSurfaceEditor();
                }
            });

            OpenTutorialCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                System.Diagnostics.Process.Start("https://github.com/Kaitoukid93/Ambinity-5.0/wiki/VID-l%C3%A0-g%C3%AC%3F-C%C3%A1ch-s%E1%BB%AD-d%E1%BB%A5ng");
            });
            FinishOOTBCommand = new RelayCommand<string>((p) =>
                        {
                            return p != null;
                        }, (p) =>
                        {
                            CurrentDevice.IsSizeNeedUserDefine = false;
                            IsRichCanvasWindowOpen = false;
                            GotoChild(CurrentDevice);
                            OOTBWindows.Close();
                        });

            PrevioustOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (OOTBStage > 0)
                    OOTBStage--;
            });
            SkipOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OOTBItems.Clear();
                OOTBSelectedItems.Clear();
                if (OOTBWindows.askAgain.IsChecked == true)
                    CurrentDevice.IsSizeNeedUserDefine = false;
                GotoChild(CurrentDevice);
                OOTBWindows.Close();
            });
            ApplySpotImportDataCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //ApplySpotImportData();
            }
            );
            ApplyOutputImportDataCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //ApplyOutputImportData();
            }
            );

            BackToPreviousAddDeviceWizardStateCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentAddDeviceWizardState--;
            });
            RenameSelectedItemCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (NameChangingSelectedItem != null)
                {
                    if (NameChangingSelectedItem is IDrawable)
                    {
                        (NameChangingSelectedItem as IDrawable).Name = NameToChange;
                    }
                }
            });
            ShowNameEditWindow = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //OpenNameEditWindow();
            });

            OpenDeviceConnectionSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenDeviceConnectionSettingsWindow();
            });
            OpenDeviceFirmwareSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, async (p) =>
            {
                //check for update
                await OpenDeviceFirmwareSettingsWindow();
            });
            OpenAdvanceSettingWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenAdvanceSettingWindow();
            });

            SetSpotPIDCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SetSpotID(p);
            });
            SetSpotVIDCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SetSpotID(p);
            });
            SetZoneMIDCommand = new RelayCommand<int>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SetZoneMID(p);
            });
            ResetSpotIDCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ResetPID();
            });

            SaveCurrentSelectedRegionCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                switch (p)
                {
                    case "screen":
                        SaveCurrentSelectedScreenRegion();
                        break;
                    case "gif":
                        SaveCurrentSelectedGifRegion();
                        break;
                }
                ;
            });
            CloseSearchingScreenCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CloseSearchingScreen();
            });
            RequestingRescanDevicesCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OnRequestingDeviceRescanEvent();
            });
            ManuallyAddSelectedDeviceToDashboard = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p == null)
                    return;
                p.UpdateChildSize();
                lock (p)
                    WriteDeviceInfo(p);
                AvailableDevices.Add(p);
                LoadAvailableDefaultDevices();
            });
            BackCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToDashboard();
            });
            BackToCollectionViewCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToCollectionView();
            });
            BackToHomePageCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToHomePage();
            });
            #endregion Command setup
        }

        private void TurnOnBetaChanel()
        {
            GeneralSettings.IsInBetaChanel = true;
        }
        private ObservableCollection<ITriggerCondition> _availableExecuteCondition;
        public ObservableCollection<ITriggerCondition> AvailableExecuteCondition {
            get { return _availableExecuteCondition; }
            set
            {
                _availableExecuteCondition = value;
                RaisePropertyChanged();
            }
        }
        private ActionManagerWindow actionManagerWindow { get; set; }
        private void OpenActionsManagerWindow(AutomationSettings selectedautomation)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                CurrentSelectedAutomation = selectedautomation;
                AvailableActionsforCurrentDevice = new ObservableCollection<ActionType>();
                //init action
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Kích hoạt", Description = "Kích hoạt một Profile có sẵn", Geometry = "apply", Type = "Activate", LinkText = "Cho thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tăng", Description = "Tăng giá trị của một thuộc tính", Geometry = "apply", Type = "Increase", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Giảm", Description = "Giảm giá trị của một thuộc tính", Geometry = "apply", Type = "Decrease", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Bật", Description = "Bật một tính năng", Geometry = "apply", Type = "On", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tắt", Description = "Tắt một tính năng", Geometry = "apply", Type = "Off", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Bật-Tắt", Description = "Chuyển đổi trạng thái Bật Tắt", Geometry = "apply", Type = "On/Off", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Chuyển", Description = "Chuyển đổi đồng thời kích hoạt một tính năng", Geometry = "apply", Type = "Change", LinkText = "Của thiết bị", ToResultText = "thành", IsValueDisplayed = true, IsTargetDeviceDisplayed = true });
                //init trigger condition
                AvailableExecuteCondition = new ObservableCollection<ITriggerCondition>();
                var hotkeyCondition = new HotkeyTriggerCondition("Hotkey", "Sử dụng tổ hợp phím tắt để kích hoạt chuỗi hành động này", null, null);
                var systemShutdownCondition = new SystemEventTriggerCondition("Khi tắt máy hoặc thoát App ", "Kích hoạt chuỗi hành động khi máy tính Shutdown", SystemEventEnum.Shutdown);
                var systemMonitorSleepCondition = new SystemEventTriggerCondition("Khi màn hình tắt ", "Kích hoạt chuỗi hành động khi màn hình tắt bởi Windows", SystemEventEnum.MonitorSleep);
                var systemMonitorWakeupCondition = new SystemEventTriggerCondition("Khi màn hình bật ", "Kích hoạt chuỗi hành động khi màn hình được bật trở lại", SystemEventEnum.MonitorWakeup);
                //var systemSleepCondition = new SystemEventTriggerCondition("Khi sleep", "Kích hoạt chuỗi hành động khi máy tính Sleep", SystemEventEnum.Sleep);
                //var appExitCondition = new SystemEventTriggerCondition("Khi thoát App", "Kích hoạt chuỗi hành động khi thoát App", SystemEventEnum.AppExit);
                //var timeStampCondition = new SystemEventTriggerCondition("Mốc thời gian", "Thực hiện chuỗi hành động khi đồng hồ điểm giờ nhất định", SystemEventEnum.TimeStamp);
                AvailableExecuteCondition.Add(hotkeyCondition);
                AvailableExecuteCondition.Add(systemShutdownCondition);
                AvailableExecuteCondition.Add(systemMonitorSleepCondition);
                AvailableExecuteCondition.Add(systemMonitorWakeupCondition);
                actionManagerWindow = new ActionManagerWindow();
                actionManagerWindow.Owner = automationManagerWindow;
                actionManagerWindow.ShowDialog();


            });

        }

        private void OpenHotKeySelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"HotKeySelectionWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedShortKeys = new ObservableCollection<KeyModel>();
                CurrentSelectedModifiers = new ObservableCollection<string>();
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenTargetDeviceSelectionWindow(ActionSettings selectedAction)
        {
            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "device";
            AutomationParamList = new ObservableCollection<object>();
            foreach (var device in AvailableDevices)
            {
                if (CurrentSelectedAction.ActionParameter.Type == "speed") // only filter hubfan
                {
                    if (device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub)
                        AutomationParamList.Add(device);
                }
                else
                {
                    AutomationParamList.Add(device);
                }
            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenTargetActionSelectionWindow(ActionSettings selectedAction)
        {
            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "action";
            AutomationParamList = new ObservableCollection<object>();
            foreach (var actionType in AvailableActionsforCurrentDevice)
            {
                AutomationParamList.Add(actionType);
            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ObservableCollection<object> _automationValuePickerList;

        public ObservableCollection<object> AutomationValuePickerList {
            get { return _automationValuePickerList; }

            set
            {
                _automationValuePickerList = value;
                RaisePropertyChanged();
            }
        }

        private void OpenAutomationValuePickerWindow(ActionSettings selectedAction)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AutomationValuePickerWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAction = selectedAction;
                var targetDevice = AvailableDevices.Where(x => x.DeviceUID == CurrentSelectedAction.TargetDeviceUID).FirstOrDefault();
                if (targetDevice == null)
                    return;
                CurrentSelectedAction = selectedAction;
                RaisePropertyChanged(nameof(CurrentSelectedAction));
                AutomationValuePickerList = new ObservableCollection<object>();
                switch (selectedAction.ActionParameter.Type)
                {
                    case "color":
                        (targetDevice.AvailableLightingDevices[0].ControlableZones[0] as LEDSetup).GetStaticColorDataSource().ForEach(c => AutomationValuePickerList.Add(c));
                        break;

                    case "mode":
                        targetDevice.AvailableLightingDevices[0].ControlableZones[0].AvailableControlMode.ForEach(m => AutomationValuePickerList.Add((m as LightingMode).BasedOn));
                        break;
                }
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ActionParameter GetAutoMationParam(string paramType, string value)
        {
            var returnParam = new ActionParameter();
            switch (paramType)
            {
                case "brightness":
                    returnParam.Name = "Độ sáng";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "brightness";
                    returnParam.Value = value;
                    break;

                case "state":
                    returnParam.Name = "Tất cả LED";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "state";
                    returnParam.Value = value;
                    break;

                case "color":
                    returnParam.Name = "Màu";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "color";
                    returnParam.Value = Color.FromRgb(255, 255, 0);
                    break;

                case "mode":
                    returnParam.Name = "Chế độ";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "mode";
                    returnParam.Value = value;
                    break;

                case "speed":
                    returnParam.Name = "Tốc độ fan";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "speed";
                    returnParam.Value = value;
                    break;
            }
            return returnParam;
        }

        private void OpenTargetParamSelectionWindow(ActionSettings selectedAction)
        {
            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "param";
            var targetDevice = AvailableDevices.Where(x => x.DeviceUID == CurrentSelectedAction.TargetDeviceUID).FirstOrDefault();

            switch (CurrentSelectedAction.ActionType.Type)
            {
                case "Activate":
                    AutomationParamList = new ObservableCollection<object>();
                    foreach (var profile in LightingProfileManagerViewModel.AvailableLightingProfiles.Items)
                    {
                        if (profile != null)
                            AutomationParamList.Add(new ActionParameter { Geometry = "brightness", Name = profile.Name, Type = "profile", Value = (profile as LightingProfile).ProfileUID });
                    }
                    break;

                case "Increase":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "up"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "up"));
                    break;

                case "Decrease":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "down"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "down"));
                    break;

                case "Off":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "off"));
                    break;

                case "On":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "on"));
                    break;
                case "On/Off":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "on"));
                    break;
                case "Change":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("color", "#ffff53c9"));
                    if (targetDevice != null)
                        AutomationParamList.Add(GetAutoMationParam("mode", targetDevice.AvailableLightingDevices[0].ControlableZones[0].CurrentActiveControlMode.Name));
                    break;
            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenSpotMapWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SpotMapViewWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.Show();
            }
        }

        private void SetCurrentActionTypeForSelectedAction(ActionType actionType)
        {
            CurrentSelectedAction.ActionType = actionType;
            //reset the param too
            CurrentSelectedAction.ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" };
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }

        private void getTextForActionType(string type, out string linkTxt, out string resultTxt)
        {
            linkTxt = string.Empty;
            resultTxt = string.Empty;
            switch (type)
            {
                case "Activate":
                    linkTxt = "Cho thiết bị";
                    resultTxt = "";
                    break;

                case "Increase":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";//could add more param
                    break;

                case "Decrease":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;

                case "On":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;

                case "Off":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;
                case "On/Off":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;
                case "Change":
                    linkTxt = "Của thiết bị";
                    resultTxt = "thành";
                    break;
            }
        }

        private void AddSelectedActionTypeToList(ActionType actionType)
        {
            ActionSettings newBlankAction = new ActionSettings {
                ActionType = actionType,
                ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" },
            };
            string lnkText = string.Empty;
            string resultText = string.Empty;
            getTextForActionType(actionType.Type, out lnkText, out resultText);
            newBlankAction.ActionType.LinkText = lnkText;
            newBlankAction.ActionType.ToResultText = resultText;
            if (CurrentSelectedAutomation.Actions == null)
            {
                CurrentSelectedAutomation.Actions = new ObservableCollection<ActionSettings>();
            }
            CurrentSelectedAutomation.Actions.Add(newBlankAction);
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Actions));
        }

        private void DeleteSelectedActionFromList(ActionSettings action)
        {
            CurrentSelectedAutomation.Actions.Remove(action);
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Actions));
        }

        private void SetCurrentActionTargetDeviceForSelectedAction(IDeviceSettings targetDevice)
        {
            CurrentSelectedAction.TargetDeviceUID = targetDevice.DeviceUID;
            CurrentSelectedAction.TargetDeviceName = targetDevice.DeviceName;
            //after this step, the parameter has to be reset because the profile UID will return invalid profile for new device
            if (CurrentSelectedAction.ActionType.Type == "Activate")
                CurrentSelectedAction.ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" };
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }

        private void SetCurrentActionParamForSelectedAction(ActionParameter param)
        {
            CurrentSelectedAction.ActionParameter = param;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }

        private void SetCurrentSelectedActionTypeColorValue(Color color)
        {
            CurrentSelectedAction.ActionParameter.Value = color;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionParameter.Value));
        }


        #region Online Item Exporter

        public OnlineItemExporterView onlineExportWindow;
        public OnlineItemModel CurrentItemForExport { get; set; }
        public object CurrentContentForExport { get; set; }
        public ObservableCollection<string> OnlineItemScreenShotCollection { get; set; }
        private string _onlineItemAvatar;

        public string OnlineItemAvatar {
            get { return _onlineItemAvatar; }
            set { _onlineItemAvatar = value; RaisePropertyChanged(); }
        }

        private List<DeviceType> _onlineItemSelectedTargetTypes;

        public List<DeviceType> OnlineItemSelectedTargetTypes {
            get { return _onlineItemSelectedTargetTypes; }
            set { _onlineItemSelectedTargetTypes = value; RaisePropertyChanged(); }
        }

        private string _onlineItemMarkdownDescription;

        public string OnlineItemMarkdownDescription {
            get { return _onlineItemMarkdownDescription; }
            set { _onlineItemMarkdownDescription = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<DeviceType> OnlineItemSelectableTargetType { get; set; }

        private void ExportCurrentOnlineItemToFiles()
        {
            //creat all needed path
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentItemForExport.Name;
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;
            string contentjson = "";
            string version = "1.0.0";
            var content = CurrentContentForExport;

            if (Export.ShowDialog() == DialogResult.OK)
            {
                //create directory with same name
                var newFolder = Directory.CreateDirectory(Export.FileName);
                var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
                //create main content
                if (content is ColorPalette)
                {
                    contentjson = JsonConvert.SerializeObject(content);
                    File.WriteAllText(Path.Combine(Export.FileName, "content", "config.json"), contentjson);
                }
                else if (content is ChasingPattern)
                {
                    var pattern = content as ChasingPattern;
                    File.Copy(pattern.LocalPath, Path.Combine(Path.Combine(Export.FileName, "content", pattern.Name)));
                }
                else if (content is Gif)
                {
                    var gif = content as Gif;
                    File.Copy(gif.LocalPath, Path.Combine(Path.Combine(Export.FileName, "content", gif.Name)));

                }
                if (content is ARGBLEDSlaveDevice)
                {
                    var device = content as ARGBLEDSlaveDevice;
                    version = device.Version;
                    contentjson = JsonConvert.SerializeObject(content);
                    File.WriteAllText(Path.Combine(Export.FileName, "content", "config.json"), contentjson);
                    if (File.Exists(device.Thumbnail))
                        File.Copy(device.Thumbnail, Path.Combine(Export.FileName, "content", "thumbnail.png"));
                    //coppy thumb with color
                    if (OnlineItemAvatar != null && OnlineItemAvatar != string.Empty)
                        File.Copy(OnlineItemAvatar, Path.Combine(Export.FileName, "content", "colored_thumbnail.png"));
                }
                if (content is LightingMode)
                {
                    var lightingProfile = new LightingProfile() {
                        Name = CurrentItemForExport.Name,
                        Description = CurrentItemForExport.Description,
                        Owner = CurrentItemForExport.Owner,
                        ControlMode = content as LightingMode,
                        ProfileUID = Guid.NewGuid().ToString()
                    };

                    contentjson = JsonConvert.SerializeObject(lightingProfile);
                    File.WriteAllText(Path.Combine(Export.FileName, "content", "config.json"), contentjson);

                }
                //create info
                var info = new OnlineItemModel() {
                    Name = CurrentItemForExport.Name,
                    Owner = CurrentItemForExport.Owner,
                    Description = CurrentItemForExport.Description,
                    Type = CurrentItemForExport.Type,
                    AvatarType = CurrentItemForExport.AvatarType,
                    Version = version,
                    TargetDevices = OnlineItemSelectedTargetTypes.ToList()
                };
                var infoJson = JsonConvert.SerializeObject(info);
                File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
                File.WriteAllText(Path.Combine(Export.FileName, "description.md"), OnlineItemMarkdownDescription);
                //coppy image data
                if (OnlineItemAvatar != null && OnlineItemAvatar != string.Empty && CurrentItemForExport.AvatarType == OnlineItemAvatarTypeEnum.Image)
                    File.Copy(OnlineItemAvatar, Path.Combine(Export.FileName, "thumb.png"));
                //copy thumbnail also
                Directory.CreateDirectory(Path.Combine(Export.FileName, "screenshots"));
                int count = 0;
                foreach (var file in OnlineItemScreenShotCollection)
                {
                    File.Copy(file, Path.Combine(Export.FileName, "screenshots", "screenshot" + "_" + count++.ToString() + ".png"));
                }
            }
        }

        private void OpenImageSelector(string type)
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn Screenshot";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Image files (*.png)|*.Png";
            Import.FilterIndex = 2;
            Import.Multiselect = type == "screenshot";
            Import.ShowDialog();
            switch (type)
            {
                case "screenshot":
                    for (int i = 0; i < Import.FileNames.Length; i++)
                    {
                        //add string path to list
                        OnlineItemScreenShotCollection.Add(Import.FileNames[i].ToString());
                    }
                    break;

                case "avatar":
                    OnlineItemAvatar = Import.FileName;
                    break;
            }
        }

        private void ExportItemForOnlineStore(object p)
        {
            if (p == null)
                return;
            CurrentContentForExport = p;
            CurrentItemForExport = new OnlineItemModel() { Name = "Change Name", Description = "Change Description", Type = p.GetType().Name.ToString(), Owner = "Change Owner", Version = "Change version" };
            OnlineItemScreenShotCollection = new ObservableCollection<string>();
            OnlineItemSelectableTargetType = new ObservableCollection<DeviceType>();
            OnlineItemSelectedTargetTypes = new List<DeviceType>();
            foreach (DeviceTypeEnum type in Enum.GetValues(typeof(DeviceTypeEnum)))
            {
                OnlineItemSelectableTargetType.Add(new DeviceType(type));
            }
            CurrentItemForExport.AvatarType = OnlineItemAvatarTypeEnum.Image;
            if (p.GetType() == typeof(ARGBLEDSlaveDevice))
            {
                CurrentItemForExport.Name = (p as ARGBLEDSlaveDevice).Name;
            }
            else if (p.GetType() == typeof(ColorPalette))
            {
                CurrentItemForExport.Name = (p as ColorPalette).Name;

            }
            else if (p.GetType() == typeof(ChasingPattern))
            {
                CurrentItemForExport.Name = (p as ChasingPattern).Name;

            }
            else if (p.GetType() == typeof(Gif))
            {
                CurrentItemForExport.Name = (p as Gif).Name;

            }
            else if (p.GetType() == typeof(Gif))
            {
                CurrentItemForExport.Name = (p as Gif).Name;

            }
            else if (p.GetType() == typeof(LightingMode))
            {
                CurrentItemForExport.Name = (p as LightingMode).Name;
                CurrentItemForExport.AvatarType = OnlineItemAvatarTypeEnum.Gravatar;
                CurrentItemForExport.Type = typeof(LightingProfile).Name.ToString();
            }
            onlineExportWindow = new OnlineItemExporterView();
            onlineExportWindow.Show();
        }

        #endregion Online Item Exporter

        #region Online Item downloader
        private int _currentDownloadProgress;
        public int CurrentDownloadProgress {
            get { return _currentDownloadProgress; }
            set
            {
                _currentDownloadProgress = value;
                RaisePropertyChanged();
            }
        }
        private int _currentItemSize;
        public int CurrentItemSize {
            get { return _currentItemSize; }
            set
            {
                _currentItemSize = value;
                RaisePropertyChanged();
            }
        }
        private void DownloadProgresBar(ulong uploaded)
        {
            // Update progress bar on foreground thread
            CurrentDownloadProgress = (int)uploaded;
        }

        private async Task SaveItemToLocalCollection<T>(string collectionPath, DeserializeMethodEnum mode, OnlineItemModel item, string extension)
        {
            item.IsDownloading = true;
            Log.Information("Downloading Item" + " " + item.Name);
            if (!Directory.Exists(collectionPath))
                Directory.CreateDirectory(collectionPath);
            if (FTPHlprs != null)
            {
                //get list of files
                var listofFiles = await FTPHlprs.GetAllFilesInFolder(item.Path + "/content");
                if (listofFiles == null)
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy file, vui lòng chọn nội dung khác!!", "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                switch (mode)
                {
                    case DeserializeMethodEnum.MultiJson:
                        //save to local folder , we only need json config file
                        var data = await FTPHlprs.GetFiles<T>(item.Path + "/content" + "/" + "config.json");
                        WriteSimpleJson(data, Path.Combine(Path.Combine(collectionPath, "collection"), item.Name + extension));
                        break;

                    case DeserializeMethodEnum.FolderStructure:
                        var localFolderPath = Path.Combine(collectionPath, item.Name);
                        Directory.CreateDirectory(localFolderPath);
                        foreach (var file in listofFiles)
                        {
                            //save to local folder
                            var remotePath = item.Path + "/content" + "/" + file.Name;
                            var itemSize = FTPHlprs.GetFileAttributes(remotePath).Size;
                            CurrentItemSize = (int)itemSize;
                            FTPHlprs.DownloadFile(remotePath, localFolderPath + "/" + file.Name, DownloadProgresBar);
                        }
                        break;
                    case DeserializeMethodEnum.Files:
                        var localFilePath = Path.Combine(collectionPath, "collection");
                        if (!Directory.Exists(localFilePath))
                            Directory.CreateDirectory(localFilePath);
                        foreach (var file in listofFiles.Where(f => f.Name != "thumbnail.png"))
                        {
                            //save to local folder
                            var remotePath = item.Path + "/content" + "/" + file.Name;
                            var itemSize = FTPHlprs.GetFileAttributes(remotePath).Size;
                            CurrentItemSize = (int)itemSize;
                            FTPHlprs.DownloadFile(remotePath, localFilePath + "/" + file.Name, DownloadProgresBar);
                        }
                        break;
                }
                //download online details for upgrade purpose
                await Task.Delay(TimeSpan.FromSeconds(1));
                item.IsDownloading = false;
                Log.Information("Item Downloaded" + " " + item.Name);
                item.IsLocalExisted = true;
                Log.Information("Generating Online Infomation" + " " + item.Name);
                var itemInfo = new OnlineItemModel();
                itemInfo.Name = item.Name;
                itemInfo.Owner = item.Owner;
                itemInfo.Version = item.Version;
                itemInfo.Type = item.Type;
                var infoFolderPath = Path.Combine(collectionPath, "info");
                if (!Directory.Exists(infoFolderPath))
                    Directory.CreateDirectory(infoFolderPath);
                JsonHelpers.WriteSimpleJson(itemInfo, Path.Combine(infoFolderPath, item.Name + ".info"));
                Log.Information("Online Infomation Created" + " " + item.Name);
                item.IsUpgradeAvailable = false;
            }

        }

        private async Task DownloadCurrentOnlineItem(OnlineItemModel o)
        {
            //create needed info
            string localFolder = string.Empty; // the resource folder this item will be saved to
            switch (o.Type)
            {
                case "ColorPalette":
                    await SaveItemToLocalCollection<ColorPalette>(PalettesCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".col");
                    break;

                case "ARGBLEDSlaveDevice":
                    await SaveItemToLocalCollection<ARGBLEDSlaveDevice>(SupportedDeviceCollectionFolderPath, DeserializeMethodEnum.FolderStructure, o, string.Empty);
                    if (SlaveDeviceSelection != null)
                        RefreshLocalSlaveDeviceCollection();
                    break;

                case "AppProfile":
                    await SaveItemToLocalCollection<AppProfile>(ProfileCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".aap");
                    break;
                case "Gif":
                    await SaveItemToLocalCollection<Gif>(GifsCollectionFolderPath, DeserializeMethodEnum.Files, o, "");
                    break;
                case "ChasingPattern":
                    await SaveItemToLocalCollection<ChasingPattern>(ChasingPatternsCollectionFolderPath, DeserializeMethodEnum.Files, o, "AML");
                    break;
                case "LightingProfile":
                    await SaveItemToLocalCollection<LightingProfile>(LightingProfilesCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".ALP");
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        LightingProfileManagerViewModel.SaveData();
                        LightingProfileManagerViewModel.LoadData();
                    });
                    break;
            }
        }

        #endregion Online Item downloader

        private ObservableCollection<StoreCategory> _availableStoreCategories;

        public ObservableCollection<StoreCategory> AvailableStoreCategories {
            get { return _availableStoreCategories; }
            set { _availableStoreCategories = value; RaisePropertyChanged(); }
        }

        private StoreCategory _currentSelectedCategory;

        public StoreCategory CurrentSelectedCategory {
            get { return _currentSelectedCategory; }

            set
            {
                //clear collection
                if (AvailableOnlineItems != null)
                    AvailableOnlineItems.Clear();
                _currentSelectedCategory = value;
                RaisePropertyChanged();
                if (value != null)
                {
                    CurrentOnlineStoreView = "Collections";
                    CarouselImageLoading = true;
                    StoreCatergoryUpdateCmd.Execute(value);
                    //StorePageIndex = 1;
                    //StorePageUpdatedCmd.Execute(new FunctionEventArgs<int>(1));
                }
            }
        }

        private ObservableCollection<BitmapImage> _availableCarouselImage;

        public ObservableCollection<BitmapImage> AvailableCarouselImage {
            get { return _availableCarouselImage; }
            set { _availableCarouselImage = value; RaisePropertyChanged(); }
        }

        private bool _carouselImageLoading;

        public bool CarouselImageLoading {
            get { return _carouselImageLoading; }
            set { _carouselImageLoading = value; RaisePropertyChanged(); }
        }
        private bool _paginationEnable;

        public bool PaginationEnable {
            get { return _paginationEnable; }
            set { _paginationEnable = value; RaisePropertyChanged(); }
        }
        private int _maxPaginationPageCount;
        public int MaxPaginationPageCount {
            get { return _maxPaginationPageCount; }
            set { _maxPaginationPageCount = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<OnlineItemModel> _availableOnlineItems;

        public ObservableCollection<OnlineItemModel> AvailableOnlineItems {
            get { return _availableOnlineItems; }
            set { _availableOnlineItems = value; RaisePropertyChanged(); }
        }
        private string _noItemText;
        public string NoItemText {
            get
            {
                return _noItemText;
            }
            set
            {
                _noItemText = value;
                RaisePropertyChanged();
            }
        }
        private int _storePageIndex;
        public int StorePageIndex {
            get
            {
                return _storePageIndex;
            }
            set
            {
                if (value > 0)
                {
                    _storePageIndex = value;
                    RaisePropertyChanged();

                }

            }
        }

        public RelayCommand<DeviceType> StoreDeviceTypeUpdateCmd => new(StoreDeviceTypeUpdate);
        public RelayCommand<StoreCategory> StoreCatergoryUpdateCmd => new(CatergoryUpdated);
        public RelayCommand<FunctionEventArgs<int>> StorePageUpdatedCmd => new(PageUpdated);
        public RelayCommand<string> StoreSearchUpdateCmd => new(StoreSearchUpdate);
        public RelayCommand<StoreFilterModel> StoreFilterUpdateCmd => new(StoreFilterUpdate);

        /// <summary>
        ///     页码改变
        /// </summary>
        /// 
        private async void StoreFilterUpdate(StoreFilterModel filter)
        {
            CarouselImageLoading = true;
            await Task.Run(() => UpdateStoreView(filter));
        }
        private async void PageUpdated(FunctionEventArgs<int> info)
        {
            CarouselImageLoading = true;
            await Task.Run(() => UpdateStoreView(info.Info));
        }
        private async void StoreSearchUpdate(string searchContent)
        {
            CarouselImageLoading = true;
            await Task.Run(() => UpdateStoreView(searchContent));
        }
        private async void CatergoryUpdated(StoreCategory catergory)
        {
            CarouselImageLoading = true;
            AvailableHomePageCarousel = new ObservableCollection<HomePageCarouselItem>();
            await Task.Run(() => UpdateStoreView(catergory));
            //CarouselImageLoading = false;
        }
        private async void StoreDeviceTypeUpdate(DeviceType type)
        {
            CarouselImageLoading = true;
            await Task.Run(() => UpdateStoreView(type, CurrentSelectedCategory));
            //CarouselImageLoading = false;
        }
        private async void StoreDeviceTypeUpdate(DeviceType type, StoreCategory catergory)
        {
            CarouselImageLoading = true;
            await Task.Run(() => UpdateStoreView(type, catergory));
            //CarouselImageLoading = false;
        }
        private StoreFilterModel _currentStoreFilter;
        public StoreFilterModel CurrentStoreFilter {
            get
            {
                return _currentStoreFilter;
            }
            set
            {
                _currentStoreFilter = value;
                RaisePropertyChanged();
            }
        }
        public AmbinoOnlineStoreView StoreWindow { get; set; }
        private async void UpdateStoreView(StoreCategory catergory)
        {
            if (catergory.Name == "Home")
            {
                CurrentOnlineStoreView = "Home";
                _lastView = "Home";
                GetCarousel();
            }
            else
            {
                CurrentOnlineStoreView = "Collections";
                var filter = new StoreFilterModel() {
                    CatergoryFilter = catergory,
                    Name = catergory.Name.ToString(),
                    PageIndex = 1
                };
                //get this catergory filter
                CurrentSelectedCategory.DefaultFilters = new ObservableCollection<StoreFilterModel>();
                var downloadedFilter = await GetCatergoryFilter(catergory.OnlineFolderPath + "/filters.json");
                if (downloadedFilter != null)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                     {
                         downloadedFilter.ForEach(i => CurrentSelectedCategory.DefaultFilters.Add(i));
                     });
                }
                AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
                NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
                await GetStoreItems(filter);
            }

        }

        private async void UpdateStoreView(StoreFilterModel filter)
        {
            CurrentOnlineStoreView = "Collections";
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            await GetStoreItems(filter);
        }
        private async void UpdateStoreView(string searchContent)
        {
            CurrentOnlineStoreView = "Collections";
            var filter = new StoreFilterModel() {
                CatergoryFilter = CurrentSelectedCategory,
                NameFilter = searchContent,
                Name = searchContent,
                PageIndex = 1
            };
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            await GetStoreItems(filter);
        }
        private async void UpdateStoreView(DeviceType type, StoreCategory catergory)
        {
            CurrentOnlineStoreView = "Collections";
            var filter = new StoreFilterModel() {
                DeviceTypeFilter = type.Type.ToString(),
                CatergoryFilter = catergory,
                Name = catergory.Name + ": " + type.Type.ToString(),
                PageIndex = 1
            };
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            await GetStoreItems(filter);
        }
        private async void UpdateStoreView(int pageIndex)
        {
            CurrentOnlineStoreView = "Collections";
            CurrentStoreFilter.PageIndex = pageIndex;
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
            if (CurrentStoreFilter.Carousel != null)
            {
                SeeAllCarouselItems(CurrentStoreFilter.Carousel);
            }
            else
            {
                await GetStoreItems(CurrentStoreFilter);
            }

        }
        //private async void UpdateStoreView(DeviceType filter)
        //{
        //    AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
        //    PaginationEnable = false;
        //    NoItemText = "Mục này hiện chưa có hiệu ứng nào hoặc server chưa được kết nối";
        //    switch (CurrentSelectedCategory.Type)
        //    {
        //        case "Palette":
        //            await GetStoreItem(paletteFolderpath, Path.Combine(PalettesCollectionFolderPath, "collection"), Path.Combine(PalettesCollectionFolderPath, "info"), filter);
        //            break;
        //        case "Pattern":
        //            await GetStoreItem(chasingPatternsFolderPath, Path.Combine(ChasingPatternsCollectionFolderPath, "collection"), Path.Combine(ChasingPatternsCollectionFolderPath, "info"), filter);
        //            break;
        //        case "Gif":
        //            await GetStoreItem(gifxelationsFolderPath, Path.Combine(GifsCollectionFolderPath, "collection"), Path.Combine(GifsCollectionFolderPath, "info"), filter);
        //            break;
        //        case "SupportedDevice":
        //            await GetStoreItem(SupportedDevicesFolderPath, SupportedDeviceCollectionFolderPath, Path.Combine(SupportedDeviceCollectionFolderPath, "info"), filter);
        //            break;
        //        case "Profiles":
        //            await GetStoreItem(ProfilesFolderPath, Path.Combine(ProfileCollectionFolderPath, "collection"), Path.Combine(ProfileCollectionFolderPath, "info"), filter);
        //            break;
        //    }

        //}
        //private async void UpdateStoreView(int pageIndex)
        //{
        //    AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
        //    NoItemText = "Không có item nào khớp với từ khóa hoặc mục đã chọn";
        //    PaginationEnable = true;
        //    switch (CurrentSelectedCategory.Type)
        //    {
        //        case "Palette":
        //            await GetStoreItem(paletteFolderpath, Path.Combine(PalettesCollectionFolderPath, "collection"), Path.Combine(PalettesCollectionFolderPath, "info"), pageIndex);
        //            break;
        //        case "Pattern":
        //            await GetStoreItem(chasingPatternsFolderPath, Path.Combine(ChasingPatternsCollectionFolderPath, "collection"), Path.Combine(ChasingPatternsCollectionFolderPath, "info"), pageIndex);
        //            break;
        //        case "Gif":
        //            await GetStoreItem(gifxelationsFolderPath, Path.Combine(GifsCollectionFolderPath, "collection"), Path.Combine(GifsCollectionFolderPath, "info"), pageIndex);
        //            break;
        //        case "SupportedDevice":
        //            await GetStoreItem(SupportedDevicesFolderPath, SupportedDeviceCollectionFolderPath, Path.Combine(SupportedDeviceCollectionFolderPath, "info"), pageIndex);
        //            break;
        //        case "Profiles":
        //            await GetStoreItem(ProfilesFolderPath, Path.Combine(ProfileCollectionFolderPath, "collection"), Path.Combine(ProfileCollectionFolderPath, "info"), pageIndex);
        //            break;
        //    }
        //}
        //private async void UpdateStoreView(string filter)
        //{
        //    AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
        //    PaginationEnable = false;
        //    NoItemText = "Không có item nào khớp với từ khóa hoặc mục đã chọn";
        //    AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
        //    await GetStoreItem(paletteFolderpath, Path.Combine(PalettesCollectionFolderPath, "collection"), Path.Combine(PalettesCollectionFolderPath, "info"), filter);
        //    await GetStoreItem(chasingPatternsFolderPath, Path.Combine(ChasingPatternsCollectionFolderPath), Path.Combine(ChasingPatternsCollectionFolderPath, "info"), filter);
        //    await GetStoreItem(gifxelationsFolderPath, Path.Combine(GifsCollectionFolderPath, "collection"), Path.Combine(GifsCollectionFolderPath, "info"), filter);
        //    await GetStoreItem(SupportedDevicesFolderPath, SupportedDeviceCollectionFolderPath, Path.Combine(SupportedDeviceCollectionFolderPath, "info"), filter);
        //    await GetStoreItem(ProfilesFolderPath, Path.Combine(ProfileCollectionFolderPath, "collection"), Path.Combine(ProfileCollectionFolderPath, "info"), filter);

        //}
        private bool CheckEqualityObjects(object object1, object object2)
        {
            string object1String = JsonConvert.SerializeObject(object1);
            string object2String = JsonConvert.SerializeObject(object2);
            return (string.Equals(object2String, object1String));
        }

        #region Store Folder Path

        private string paletteFolderpath = "/home/adrilight_enduser/ftp/files/ColorPalettes";
        private string chasingPatternsFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns";
        private string gifxelationsFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations";
        private string SupportedDevicesFolderPath = "/home/adrilight_enduser/ftp/files/SupportedDevices";
        private string ProfilesFolderPath = "/home/adrilight_enduser/ftp/files/Profiles";
        private string thumbResourceFolderPath = "/home/adrilight_enduser/ftp/files/Resources/Thumbs";
        private string openRGBDevicesFolderPath = "/home/adrilight_enduser/ftp/files/OpenRGBDevices";
        private string ambinoDevicesFolderPath = "/home/adrilight_enduser/ftp/files/AmbinoDevices";
        #endregion Store Folder Path

        #region Resource Folder Path
        #endregion Resource Folder Path
        public class StringIEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
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
                case "LightingProfile":
                    localPath = LightingProfilesCollectionFolderPath;
                    break;
            }
            var itemLocalInfo = Path.Combine(localPath, "info", item.Name + ".info");
            return itemLocalInfo;
        }
        private async Task<List<OnlineItemModel>> GetStoreItems(List<string> listAddress)
        {

            //download items
            var items = new List<OnlineItemModel>();
            if (listAddress == null)
                return await Task.FromResult(items);
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                await Task.Delay(1000);
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    CarouselImageLoading = false;
                    return await Task.FromResult(items);
                }

            }
            foreach (var url in listAddress)
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
                items.Add(info);
            }
            return await Task.FromResult(items);
        }
        private async Task<List<StoreFilterModel>> GetCatergoryFilter(string path)
        {
            var listFilter = new List<StoreFilterModel>();
            if (path == null || path == string.Empty)
            {
                return await Task.FromResult(listFilter);
            }
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                await Task.Delay(1000);
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    CarouselImageLoading = false;
                    return await Task.FromResult(listFilter);
                }

            }
            listFilter = FTPHlprs.GetFiles<List<StoreFilterModel>>(path).Result;
            return await Task.FromResult(listFilter);
        }
        private async Task GetStoreItems(StoreFilterModel filter)
        {
            //init if null
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                await Task.Delay(1000);
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    CarouselImageLoading = false;
                    return;
                }

            }
            CurrentStoreFilter = filter;
            var currentPageListItemAddress = new List<string>();
            var currentDeviceTypeFilter = filter.DeviceTypeFilter;
            var currentnameFilter = filter.NameFilter;
            var currentCatergoryFilter = filter.CatergoryFilter;
            if (currentCatergoryFilter == null)
                currentCatergoryFilter = CurrentSelectedCategory;
            // get itemfolderPath and itemLocalFolderPath based on datatype if datatpye == all search all folder
            var itemFolderPath = currentCatergoryFilter.OnlineFolderPath;
            var listItemAddress = await FTPHlprs.GetAllFilesAddressInFolder(itemFolderPath);
            var pageIndex = filter.PageIndex;
            //filter item by name if exist
            if (listItemAddress != null && listItemAddress.Count > 0)
            {
                var lowerFilter = string.Empty;
                //filter name
                if (currentnameFilter != null)
                    lowerFilter = currentnameFilter.ToLower();
                //filter by supported device

                var filteredItemAddress = new List<string>();
                foreach (var address in listItemAddress)
                {
                    //get name
                    var itemName = FTPHlprs.GetFileOrFoldername(address).Name;
                    if (itemName.Contains("filters"))
                        continue;
                    //get description content
                    var descriptionPath = address + "/description.md";
                    var description = await FTPHlprs.GetStringContent(descriptionPath);
                    if (itemName.ToLower().Contains(lowerFilter) || lowerFilter == string.Empty || description.ToLower().Contains(lowerFilter))
                    {
                        filteredItemAddress.Add(address);
                    }
                }
                //get info
                var finalItemList = new List<string>();
                foreach (var address in filteredItemAddress)
                {
                    var infoPath = address + "/info.json";
                    var info = FTPHlprs.GetFiles<OnlineItemModel>(infoPath).Result;
                    info.Path = address;
                    if (info.TargetDevices != null && currentDeviceTypeFilter != null && info.TargetDevices.Any(t => t.Type.ToString() == currentDeviceTypeFilter))
                    {
                        finalItemList.Add(address);
                    }
                    else if (currentDeviceTypeFilter == null)
                    {
                        finalItemList.Add(address);
                    }
                }
                MaxPaginationPageCount = finalItemList.Count / 12 + 1;
                var itemList = await GetStoreItems(finalItemList.Skip((pageIndex - 1) * 12).Take(12).ToList());
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    lock (AvailableOnlineItems)
                    {
                        itemList.ForEach(i => AvailableOnlineItems.Add(i));
                    }
                });
                lock (AvailableOnlineItems)
                {
                    foreach (var item in AvailableOnlineItems)
                    {
                        if (item.AvatarType == OnlineItemAvatarTypeEnum.Image)
                        {
                            var thumbPath = item.Path + "/thumb.png";
                            item.Thumb = FTPHlprs.GetThumb(thumbPath).Result;
                        }

                    }
                }

                CarouselImageLoading = false;
            }
            else
            {
                //HandyControl.Controls.MessageBox.Show("Không có item nào cho mục này, vui lòng thử lại sau", "Item notfound", MessageBoxButton.OK, MessageBoxImage.Error);
                CarouselImageLoading = false;
                return;
            }
        }

        private string _currentOnlineStoreView;

        public string CurrentOnlineStoreView {
            get { return _currentOnlineStoreView; }
            set { _currentOnlineStoreView = value; RaisePropertyChanged(); }
        }
        private void AmbinoStoreInit()
        {
            if (AvailableStoreCategories == null)
            {
                AvailableStoreCategories = new ObservableCollection<StoreCategory>();
                var home = new StoreCategory() {
                    Name = "Home",
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/HomePage",
                    LocalFolderPath = CacheFolderPath,
                    DataType = typeof(ColorPalette),
                    Description = "Home page",
                    Geometry = "onlineStore"
                };
                var palettes = new StoreCategory() {
                    Name = "Colors",
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/ColorPalettes",
                    LocalFolderPath = Path.Combine(PalettesCollectionFolderPath),
                    DataType = typeof(ColorPalette),
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "palette"
                };
                var chasingPatterns = new StoreCategory() {
                    Name = "Animations",
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns",
                    LocalFolderPath = Path.Combine(ChasingPatternsCollectionFolderPath),
                    DataType = typeof(ChasingPattern),
                    Description = "All  Animations created by Ambino and Contributed by Ambino Community",
                    Geometry = "chasingPattern"
                };
                var gif = new StoreCategory() {
                    Name = "Gifs",
                    DataType = typeof(Gif),
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations",
                    LocalFolderPath = Path.Combine(GifsCollectionFolderPath),
                    Description = "All Gifs created by Ambino and Contributed by Ambino Community",
                    Geometry = "gifxelation"
                };
                var supportedDevices = new StoreCategory() {
                    Name = "Devices",
                    DataType = typeof(ARGBLEDSlaveDevice),
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/SupportedDevices",
                    LocalFolderPath = SupportedDeviceCollectionFolderPath,
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "slaveDevice"
                };
                var lightingProfiles = new StoreCategory() {
                    Name = "Lighting",
                    DataType = typeof(LightingProfile),
                    OnlineFolderPath = "/home/adrilight_enduser/ftp/files/LightingProfiles",
                    LocalFolderPath = SupportedDeviceCollectionFolderPath,
                    Description = "All Lighting Profiles created by Ambino and Contributed by Ambino Community",
                    Geometry = "appProfile"
                };
                AvailableStoreCategories.Add(home);
                AvailableStoreCategories.Add(palettes);
                AvailableStoreCategories.Add(lightingProfiles);
                AvailableStoreCategories.Add(chasingPatterns);
                AvailableStoreCategories.Add(gif);
                AvailableStoreCategories.Add(supportedDevices);

            }
        }
        private ObservableCollection<HomePageCarouselItem> _availableHomePageCarousel;
        public ObservableCollection<HomePageCarouselItem> AvailableHomePageCarousel {
            get
            {
                return _availableHomePageCarousel;
            }
            set
            {
                _availableHomePageCarousel = value;
                RaisePropertyChanged();
            }
        }
        private async void GetCarousel()
        {
            var carouselFolderPath = "/home/adrilight_enduser/ftp/files/HomePage/Carousel";
            if (FTPHlprs == null)
            {
                SFTPInit(GeneralSettings.CurrentAppUser);
                await Task.Delay(1000);
            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    SFTPConnect();
                }
                catch (Exception ex)
                {
                    CarouselImageLoading = false;
                    return;
                }

            }
            var listItemAddress = await FTPHlprs.GetAllFilesAddressInFolder(carouselFolderPath);
            if (listItemAddress == null)
                return;
            foreach (var address in listItemAddress)
            {
                try
                {
                    var infoPath = address + "/config.json";
                    var info = FTPHlprs.GetFiles<HomePageCarouselItem>(infoPath).Result;
                    info.Path = address;
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        lock (AvailableHomePageCarousel)
                            AvailableHomePageCarousel.Add(info);
                    });
                }

                catch (Exception ex)
                {
                    CarouselImageLoading = false;
                }

            }
            //load carousel item
            foreach (var carousel in AvailableHomePageCarousel)
            {
                carousel.CarouselItem = new ObservableCollection<OnlineItemModel>();
                var listItem = await GetStoreItems(carousel.EmbeddedURL.Take(5).ToList());
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    listItem.ForEach(i => carousel.CarouselItem.Add(i));
                });
                foreach (var item in listItem)
                {
                    var thumbPath = item.Path + "/thumb.png";
                    item.Thumb = FTPHlprs.GetThumb(thumbPath).Result;
                }
            }
            CarouselImageLoading = false;
        }
        private bool _isInCarouselList;
        public bool IsInCarouselList {
            get { return _isInCarouselList; }
            set
            {
                _isInCarouselList = value;
                RaisePropertyChanged();
            }
        }
        private string _lastView;
        private void BackToHomePage()
        {
            IsInCarouselList = false;
            CurrentOnlineStoreView = "Home";
        }
        private async void SeeAllCarouselItems(HomePageCarouselItem carousel)
        {
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            IsInCarouselList = true;
            var pageIndex = CurrentStoreFilter.PageIndex;
            MaxPaginationPageCount = carousel.EmbeddedURL.Count / 12 + 1;
            var itemList = await GetStoreItems(carousel.EmbeddedURL.Skip((pageIndex - 1) * 12).Take(12).ToList());
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                lock (AvailableOnlineItems)
                    itemList.ForEach(i => AvailableOnlineItems.Add(i));
            });
            foreach (var item in itemList)
            {
                var thumbPath = item.Path + "/thumb.png";
                item.Thumb = FTPHlprs.GetThumb(thumbPath).Result;
            }
            CarouselImageLoading = false;
        }
        private void OpenAmbinoStoreWindow(string catergory)
        {
            CurrentOnlineStoreView = "Collections";
            StoreWindow = new AmbinoOnlineStoreView();
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            StoreWindow.Owner = System.Windows.Application.Current.MainWindow;
            StoreWindow.Show();
            AmbinoStoreInit();
            CurrentSelectedCategory = AvailableStoreCategories.Where(c => c.Name == catergory).FirstOrDefault();
            AvailableCarouselImage = new ObservableCollection<BitmapImage>();
        }
        private void OpenAmbinoStoreWindow(IDeviceSettings device)
        {
            CurrentOnlineStoreView = "Collections";
            StoreWindow = new AmbinoOnlineStoreView();
            AvailableOnlineItems = new ObservableCollection<OnlineItemModel>();
            StoreWindow.Owner = System.Windows.Application.Current.MainWindow;
            StoreWindow.Show();
            AmbinoStoreInit();
            AvailableCarouselImage = new ObservableCollection<BitmapImage>();
            CurrentSelectedCategory = null;
            var devCatergory = AvailableStoreCategories.Where(c => c.Name == "Devices").FirstOrDefault();
            StoreDeviceTypeUpdate(device.DeviceType, devCatergory);
        }
        private AutomationManagerWindow automationManagerWindow { get; set; }
        private void OpenAutomationManagerWindow()
        {
            automationManagerWindow = new AutomationManagerWindow();
            automationManagerWindow.Owner = System.Windows.Application.Current.MainWindow;
            automationManagerWindow.ShowDialog();

        }

        private void OpenOutputDataImportSelection()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"OutputDataImportSelection"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenHardwareMonitorWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ComputerHardwareInformationWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAddNewAutomationWindowCommand()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewAutomationWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ListSelectionParameter _accentColorSelection;

        public ListSelectionParameter AccentColorSelection {
            get { return _accentColorSelection; }

            set
            {
                _accentColorSelection = value;
                RaisePropertyChanged();
            }
        }
        private void OpenDebugWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DebugWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.Show();
            }
        }
        private void OpenAppSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AppSettingsWindow"}") is System.Windows.Window window)
            {
                //init app acent color selection
                AccentColorSelection = new ListSelectionParameter(ModeParameterEnum.Color) {
                    Name = "Màu chủ đề",
                    Description = "Chọn màu chủ đạo cho ứng dụng, Sau khi chọn màu, bạn nên khởi động lại ứng dụng",
                    DataSourceLocaFolderNames = new List<string>() { "Colors" }
                };
                AccentColorSelection.LoadAvailableValues();
                AccentColorSelection.SelectedValue = new ColorCard(GeneralSettings.AccentColor, GeneralSettings.AccentColor);
                AccentColorSelection.PropertyChanged += (_, __) =>
                {
                    switch (__.PropertyName)
                    {
                        case nameof(AccentColorSelection.SelectedValue):
                            ThemeManager.Current.AccentColor = new SolidColorBrush((AccentColorSelection.SelectedValue as ColorCard).StartColor);
                            GeneralSettings.AccentColor = (AccentColorSelection.SelectedValue as ColorCard).StartColor;

                            break;
                    }
                };
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void LaunchWBAdjustWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"CustomWBWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void ClearCacheFolder()
        {
            System.IO.DirectoryInfo cache = new DirectoryInfo(CacheFolderPath);
            foreach (FileInfo file in cache.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in cache.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }
        private IDeviceSettings ImportDevice(string path)
        {
            if (!File.Exists(path))
                return null;
            IDeviceSettings device;
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                //extract device
                //Create directory to extract
                Directory.CreateDirectory(CacheFolderPath);
                //then extract
                ZipFile.ExtractToDirectory(path, CacheFolderPath);
                var deviceJson = File.ReadAllText(Path.Combine(CacheFolderPath, fileName, "config.json"));
                device = JsonConvert.DeserializeObject<DeviceSettings>(deviceJson);
                if (device != null)
                {
                    //create device info
                    //device.UpdateChildSize();
                    device.UpdateUID();
                    //copy thumb
                    if (File.Exists(Path.Combine(CacheFolderPath, fileName, "thumbnail.png")) && !File.Exists(Path.Combine(ResourceFolderPath, device.DeviceName + "_thumb.png")))
                    {
                        File.Copy(Path.Combine(CacheFolderPath, fileName, "thumbnail.png"), Path.Combine(ResourceFolderPath, device.DeviceName + "_thumb.png"), true);
                    }
                    if (File.Exists(Path.Combine(CacheFolderPath, fileName, "outputmap.png")) && !File.Exists(Path.Combine(ResourceFolderPath, device.DeviceName + "_outputmap.png")))
                    {
                        File.Copy(Path.Combine(CacheFolderPath, fileName, "outputmap.png"), Path.Combine(ResourceFolderPath, device.DeviceName + "_outputmap.png"), true);
                    }
                    //copy required SlaveDevice
                    var dependenciesFiles = Path.Combine(CacheFolderPath, fileName, "dependencies");
                    if (Directory.Exists(dependenciesFiles))
                    {
                        foreach (var sub in Directory.GetDirectories(dependenciesFiles))
                        {
                            LocalFileHelpers.CopyDirectory(sub, SupportedDeviceCollectionFolderPath, true);
                        }
                    }
                    //remove cache
                    ClearCacheFolder();
                    return device;
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "File Import", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return null;
        }
        private void ImportDeviceFromLocalFile()
        {
            var deviceFile = LocalFileHlprs.OpenImportFileDialog(".zip", "ZIP Folders (.ZIP)|*.zip");

            if (deviceFile != null)
            {

                var device = ImportDevice(deviceFile);
                if (device != null)
                {
                    lock (AvailableDevices)
                        AvailableDevices.Insert(0, device);
                    lock (device)
                        WriteDeviceInfo(device);
                }
            }
        }

        private void ImportProfile()
        {
            var profile = LocalFileHlprs.OpenImportFileDialog<AppProfile>("aap", "aap files (*.aap)|*.Aap|Text files (*.txt)|*.Txt");
            if (profile != null)
            {
                AvailableProfiles.Add(profile);
                WriteSimpleJson(profile, Path.Combine(Path.Combine(ProfileCollectionFolderPath, "collection"), profile.Name + ".aap")); // .aap stands for adrilight application profile ...
            }

        }

        public void SaveCurrentProfile(string profileUID)
        {
            //var currentProfile = AvailableProfiles.Where(p => p.ProfileUID == profileUID).FirstOrDefault();
            //if (currentProfile != null)
            //    // currentProfile.SaveProfile(CurrentDevice.AvailableOutputs);

            //    // WriteDeviceProfileCollection();

            //    //Growl.Success("Profile saved successfully!");
            //    IsSettingsUnsaved = BadgeStatus.Dot;
        }

        public void DeleteAttachedProfile(AppProfile profile)
        {
            //check if profile is in used
            if (profile == null)
                return;
            if (profile.IsActivated)
            {
                HandyControl.Controls.MessageBox.Show(profile.Name + " Không thể xóa, profile này đang được sử dụng!!!", "Profile is in used", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                AvailableProfiles.Remove(profile);
                //delete from disk
            }
        }

        private AppProfile _currentActivatedProfile;

        public AppProfile CurrentActivatedProfile {
            get { return _currentActivatedProfile; }
            set { _currentActivatedProfile = value; RaisePropertyChanged(); }
        }

        private bool _isloadingProfile = false;

        public bool IsLoadingProfile {
            get { return _isloadingProfile; }

            set
            {
                _isloadingProfile = value;
                RaisePropertyChanged();
            }
        }

        public void ActivateSelectedProfile(AppProfile profile)
        {
            if (profile == null)
                return;
            //construct devices from profile data
            //check if any connected device has same type and hardware version
            List<IDeviceSettings> compatibleDevices = new List<IDeviceSettings>();
            lock (AvailableDeviceLock)
            {
                int counter = 0;
                foreach (var deviceProfile in profile.DeviceProfiles)
                {
                    var targetDevice = AvailableDevices.Where(d => d.DeviceType.Type == deviceProfile.DeviceSettings.DeviceType.Type).FirstOrDefault();
                    if (targetDevice != null)
                    {
                        //get uid for local writing
                        var device = ObjectHelpers.Clone<DeviceSettings>(deviceProfile.DeviceSettings as DeviceSettings);
                        device.DeviceUID = targetDevice.DeviceUID;
                        device.DeviceName = targetDevice.DeviceName;
                        device.IsLoadingProfile = false;
                        device.OutputPort = targetDevice.OutputPort;
                        compatibleDevices.Add(device);
                        AvailableDevices.Remove(targetDevice);
                        counter++;
                    }
                }
                //takes what left
                compatibleDevices.AddRange(AvailableDevices.ToList());
                AvailableDevices.Clear();//reset the collection so kernel can be unbind
                compatibleDevices.ForEach(d => AvailableDevices.Add(d));
                SelectedViewPart = SelectableViewParts[0];
                if (counter == 0)
                {
                    HandyControl.Controls.MessageBox.Show("Profile không thích hợp với bất kỳ thiết bị nào hiện có", "No Device Compatible", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Áp dụng thành công Profile" + profile.Name, "Profile Activated", MessageBoxButton.OK, MessageBoxImage.Information);
                    CurrentActivatedProfile = profile;
                }
            }
        }

        private UpdateInfo _availableUpdates;

        public UpdateInfo AvailableUpdates {
            get { return _availableUpdates; }

            set
            {
                _availableUpdates = value;
                RaisePropertyChanged();
            }
        }

        private bool _isCheckingForUpdate = false;

        public bool IsCheckingForUpdate {
            get { return _isCheckingForUpdate; }

            set
            {
                _isCheckingForUpdate = value;
                RaisePropertyChanged();
            }
        }

        private bool _updateButtonEnable = true;

        public bool UpdateButtonEnable {
            get { return _updateButtonEnable; }

            set
            {
                _updateButtonEnable = value;
                RaisePropertyChanged();
            }
        }

        private string _updateErrorMessage = "";

        public string UpdateErrorMessage {
            get { return _updateErrorMessage; }

            set
            {
                _updateErrorMessage = value;
                RaisePropertyChanged();
            }
        }

        private Task<UpdateManager> mgr;
        private SplashScreen _splashScreen;

        private async Task CheckForUpdate()
        {
            UpdateErrorMessage = "";
            try
            {
                mgr = UpdateManager.GitHubUpdateManager(ADRILIGHT_RELEASES);
                using (var result = await mgr)
                {
                    AvailableUpdates = await mgr.Result.CheckForUpdate();
                    RaisePropertyChanged(nameof(AvailableUpdates));
                }
            }
            catch (Exception ex)
            {
                UpdateErrorMessage = ex.Message + "\n" + "Kiểm tra lại mạng hoặc liên hệ Ambino để được hỗ trợ";
            }

            //check once a day for updates
        }

        private async Task UpdateApp()
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen = new View.SplashScreen();
                _splashScreen.Header.Text = "Downloading Update";
                _splashScreen.status.Text = "UPDATING...";
                _splashScreen.Show();
            });

            // this.logger.Info("Downloading updates");
            var releaseEntry = await mgr.Result.UpdateApp();

            if (releaseEntry != null)
            {
                //restart adrilight if an update was installed
                //dispose locked WinRing0 file first
                //if (HWMonitor != null)
                //    HWMonitor.Dispose();
                //if (AmbinityClient != null)
                //    AmbinityClient.Dispose();
                //remember to dispose openrgbstream too!!!
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _splashScreen.status.Text = "RESTARTING...";
                });
                UpdateManager.RestartApp();
            }
            //this.logger.Info($"Download complete. Version {updateResult.Version} will take effect when App is restarted.");
        }

        private void SaveAllAutomation()
        {
            WriteAutomationCollectionJson();
            RaisePropertyChanged(nameof(DashboardPinnedAutomation));
            if (GeneralSettings.HotkeyEnable)
            {
                Unregister();
                Register();
            }
        }

        private NonInvasiveKeyboardHookLibrary.ModifierKeys ConvertStringtoModifier(string key)
        {
            NonInvasiveKeyboardHookLibrary.ModifierKeys returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.WindowsKey;
            switch (key)
            {
                case "Shift":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift;
                    break;

                case "Ctrl":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control;
                    break;

                case "Alt":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt;
                    break;
            }
            return returnKey;
        }

        private void SaveCurrentSelectedAutomationShortkey()
        {
            var modifiers = CurrentSelectedModifiers;
            var key = CurrentSelectedShortKeys.ToArray();
            var modifiersKey = new ObservableCollection<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
            foreach (var modifier in modifiers)
            {
                modifiersKey.Add(ConvertStringtoModifier(modifier));
            }
            var condition = new HotkeyTriggerCondition("HotKey", "Hotkey", modifiersKey, key[0]);
            CurrentSelectedAutomation.Condition = condition;
            WriteAutomationCollectionJson();
            if (GeneralSettings.HotkeyEnable)
            {
                Unregister();
                Register();
            }
        }

        public ObservableCollection<IDeviceSettings> SelectedDevicesForCurrentProfile { get; set; }

        private void CreateNewProfile()
        {
            if (SelectedDevicesForCurrentProfile.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải chọn ít nhất một thiết bị tham gia vào profile này", "No device selected!!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var newAppProfile = new AppProfile();
            newAppProfile.Name = NewProfileName;
            newAppProfile.Owner = NewProfileOwner;
            newAppProfile.Description = NewProfileDescription;
            foreach (var device in SelectedDevicesForCurrentProfile)
            {
                var newprofile = new DeviceProfile {
                    Name = device.DeviceName + "Profile",
                    Description = "UserCreated profile for" + device.DeviceName,
                    Geometry = "profile",
                    ProfileUID = device.DeviceUID
                };
                newprofile.SaveProfile(device);
                newAppProfile.DeviceProfiles.Add(newprofile);
            }

            //AvailableProfiles.Add(newprofile);
            AvailableProfiles.Add(newAppProfile);
            WriteSimpleJson(newAppProfile, Path.Combine(Path.Combine(ProfileCollectionFolderPath, "collection"), newAppProfile.Name + ".aap")); // .aap stands for adrilight application profile ...
            addNewProfileWindow.Close();
            //WriteDeviceProfileCollection();
        }

        private AddNewProfileWindow addNewProfileWindow { get; set; }

        private void OpenCreatenewProfileWindow()
        {
            addNewProfileWindow = new AddNewProfileWindow();
            addNewProfileWindow.Owner = System.Windows.Application.Current.MainWindow;
            SelectedDevicesForCurrentProfile = new ObservableCollection<IDeviceSettings>();
            addNewProfileWindow.ShowDialog();
        }

        public void Register()
        {
            //_identifiers = new List<Guid?>();
            foreach (var automation in AvailableAutomations.Where(x => x.IsEnabled == true))
            {
                if (automation.Condition is not HotkeyTriggerCondition)
                    continue;
                var condition = automation.Condition as HotkeyTriggerCondition;
                if (condition == null)
                    continue;
                var modifierkeys = new List<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
                if (condition.Modifiers != null)
                {
                    foreach (var modifier in condition.Modifiers)
                    {
                        modifierkeys.Add(modifier);
                    }
                }

                try
                {
                    switch (modifierkeys.Count)
                    {
                        case 0:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(condition.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                   {
                                       ExecuteAutomationActions(automation.Actions);
                                   });

                                Debug.WriteLine(automation.Name + " excuted");
                                if (GeneralSettings.NotificationEnabled)
                                    SendNotification(automation.Name);
                            });
                            //_identifiers.Add(_identifier);
                            break;

                        case 1:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(modifierkeys.First(), condition.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                {
                                    ExecuteAutomationActions(automation.Actions);
                                });
                                Debug.WriteLine(automation.Name + " excuted");
                                if (GeneralSettings.NotificationEnabled)
                                    SendNotification(automation.Name);
                            });
                            //_identifiers.Add(_identifier);
                            break;

                        default:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(modifierkeys.ToArray(), condition.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                {
                                    ExecuteAutomationActions(automation.Actions);
                                });
                                Debug.WriteLine(automation.Name + " excuted");
                            });
                            //_identifiers.Add(_identifier);
                            break;
                    }
                }
                catch (NonInvasiveKeyboardHookLibrary.HotkeyAlreadyRegisteredException ex)
                {
                    HandyControl.Controls.MessageBox.Show(automation.Name + " Hotkey is being used by another automation!!!", "HotKey Already Registered", MessageBoxButton.OK, MessageBoxImage.Error);
                    //disable automation
                    automation.Condition = null;
                    // automation.IsEnabled = false;
                    WriteAutomationCollectionJson();
                }
                catch (Exception ex)
                {
                }
            }
        }
        public void ExecuteShudownAutomationActions()
        {
            foreach (var automation in ShutdownAutomations)
            {
                ExecuteAutomationActions(automation.Actions);
            }
        }
        public void ExecuteAutomationActions(ObservableCollection<ActionSettings> actions)
        {
            if (actions == null)
                return;
            foreach (var action in actions)
            {

                var targetDevice = AvailableDevices.Where(x => x.DeviceUID == action.TargetDeviceUID).FirstOrDefault();
                if (targetDevice == null)
                {
                    WriteAutomationCollectionJson();
                    continue;
                }
                if (action.ActionType.Type == "Activate") // this type of action require no target 
                    goto execute;

                if (!targetDevice.IsEnabled)
                    return;
                execute:
                switch (action.ActionType.Type)
                {

                    case "Activate":
                        var destinationProfile = LightingProfileManagerViewModel.AvailableLightingProfiles.Items.Where(x => (x as LightingProfile).ProfileUID == (string)action.ActionParameter.Value).FirstOrDefault() as LightingProfile;
                        if (destinationProfile != null)
                        {
                            targetDevice.TurnOnLED();
                            LightingPlayer.Play(destinationProfile, targetDevice);
                            // ActivateCurrentLightingProfileForSpecificDevice(destinationProfile, targetDevice);
                        }

                        break;

                    case "Increase":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessUp(10);
                                break;

                            case "speed":
                                targetDevice.SpeedUp(10);
                                break;
                        }
                        break;

                    case "Decrease":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessDown(10);
                                break;

                            case "speed":
                                targetDevice.SpeedDown(10);
                                break;
                        }
                        break;

                    case "Off":
                        // just turn off all leds for now
                        targetDevice.TurnOffLED();
                        break;

                    case "On":
                        //targetDevice.IsEnabled = true;
                        targetDevice.TurnOnLED();
                        break;
                    case "On/Off":
                        targetDevice.ToggleOnOffLED();
                        break;
                    case "Change":
                        //just change solid color and activate static mode
                        targetDevice.TurnOnLED();
                        switch (action.ActionParameter.Type)
                        {
                            case "color":
                                targetDevice.SetStaticColor(action.ActionParameter.Value as ColorCard);
                                break;
                            case "mode":
                                LightingModeEnum value = (LightingModeEnum)Enum.Parse(typeof(LightingModeEnum), action.ActionParameter.Value.ToString());
                                targetDevice.SetModeByEnumValue(value);
                                break;
                        }

                        break;
                }
            }
        }

        private MainView _mainForm;

        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            if (_mainForm == null) return;

            //deregister to avoid memory leak
            _mainForm.Closed -= MainForm_FormClosed;
            _mainForm = null;
        }

        private void SendNotification(string content)
        {
            NotifyIcon.ShowBalloonTip("Ambinity | Automation excuted", content, NotifyIconInfoType.Info, "Ambinity");
        }

        private void Unregister()
        {
            KeyboardHookManagerSingleton.Instance.UnregisterAll();
        }

        private int _fwUploadPercent;

        public int FwUploadPercent {
            get { return _fwUploadPercent; }

            set
            {
                _fwUploadPercent = value;
                RaisePropertyChanged();
            }
        }

        private bool _fwUploadPercentVissible = false;

        public bool FwUploadPercentVissible {
            get { return _fwUploadPercentVissible; }

            set
            {
                _fwUploadPercentVissible = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ITimeLineDataItem> _availableMotions;

        public ObservableCollection<ITimeLineDataItem> AvailableMotions {
            get { return _availableMotions; }

            set
            {
                _availableMotions = value;
                RaisePropertyChanged();
            }
        }

        private bool _reloadDeviceLoadingVissible = false;

        public bool ReloadDeviceLoadingVissible {
            get { return _reloadDeviceLoadingVissible; }

            set
            {
                _reloadDeviceLoadingVissible = value;
                RaisePropertyChanged();
            }
        }

        private bool _effectLoadingVissible = false;

        public bool EffectLoadingVissible {
            get { return _effectLoadingVissible; }

            set
            {
                _effectLoadingVissible = value;
                RaisePropertyChanged();
            }
        }

        private string _fwUploadOutputLog;

        public string FwUploadOutputLog {
            get { return _fwUploadOutputLog; }

            set
            {
                _fwUploadOutputLog = value;
                RaisePropertyChanged();
            }
        }
        private bool _isDownloadingFirmware;
        public bool IsDownloadingFirmware {
            get
            {
                return _isDownloadingFirmware;
            }
            set
            {
                _isDownloadingFirmware = value;
                RaisePropertyChanged();
            }
        }
        private bool isApplyingDeviceHardwareSettings;
        public bool IsApplyingDeviceHardwareSettings {
            get
            {
                return isApplyingDeviceHardwareSettings;
            }
            set
            {
                isApplyingDeviceHardwareSettings = value;
                RaisePropertyChanged();
            }
        }
        private async Task ApplyDeviceHardwareSettings(IDeviceSettings device)
        {
            IsApplyingDeviceHardwareSettings = true;
            var result = await Task.Run(() => device.SendHardwareSettings());
            IsApplyingDeviceHardwareSettings = false;
        }

        public bool FrimwareUpgradeIsInProgress { get; set; }
        private async Task UpgradeIfAvailable(IDeviceSettings device)
        {
            FrimwareUpgradeIsInProgress = true;
            if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("HUBV2 cần sử dụng FlyMCU để nạp firmware, nhấn [OK] để vào chế độ DFU", "External Software Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    device.DeviceState = DeviceStateEnum.DFU;

                    Thread.Sleep(5000);
                    device.DeviceState = DeviceStateEnum.Normal;
                    HandyControl.Controls.MessageBox.Show("Đã gửi thông tin đến Device, mở FlyMCU để tiếp tục nạp firmware sau đó bật lại kết nối", "DFU", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (device.HardwareVersion == "unknown") // old firmware or not supported
                {
                    // show message box : unknown hardware version, please update firmware manually by chosing one of these firmware file in the list below
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show("Thiết bị đang ở firmware cũ hoặc phần cứng không hỗ trợ! bạn có muốn chọn firmware để cập nhật không?", "Unknown hardware version", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //grab available firmware for current device type
                        var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                        var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);
                        AvailableFirmwareForCurrentDevice = new ObservableCollection<DeviceFirmware>();
                        foreach (var firmware in availableFirmware)
                        {
                            if (firmware.TargetDeviceType == device.DeviceType.Type)
                                AvailableFirmwareForCurrentDevice.Add(firmware);
                        }

                        // show list selected firmware
                        OpenFirmwareSelectionWindow();
                    }
                }
                else
                {
                    // regconize this device, find the compatible firmware
                    var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                    var requiredFwVersion = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json);

                    var currentDeviceFirmwareInfo = requiredFwVersion.Where(p => p.TargetHardware == device.HardwareVersion).FirstOrDefault();
                    if (currentDeviceFirmwareInfo == null)
                    {
                        //not supported hardware

                        var result = HandyControl.Controls.MessageBox.Show("Phần cứng không còn được hỗ trợ hoặc không nhận ra: " + device.HardwareVersion + " Bạn có muốn chọn phần cứng được hỗ trợ?", "Firmware uploading", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            var fwjson = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                            var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(fwjson);
                            AvailableFirmwareForCurrentDevice = new ObservableCollection<DeviceFirmware>();
                            foreach (var firmware in availableFirmware)
                            {
                                if (firmware.TargetDeviceType == device.DeviceType.Type)
                                    AvailableFirmwareForCurrentDevice.Add(firmware);
                            }

                            // show list selected firmware
                            OpenFirmwareSelectionWindow();
                        }
                    }
                    else
                    {
                        var fwOutputLocation = Path.Combine(JsonFWToolsFileNameAndPath, currentDeviceFirmwareInfo.Name);
                        try
                        {
                            ResourceHlprs.CopyResource(currentDeviceFirmwareInfo.ResourceName, fwOutputLocation);
                        }
                        catch (ArgumentException)
                        {
                            //show messagebox no firmware found for this device
                            return;
                        }
                        if (device.FirmwareVersion != currentDeviceFirmwareInfo.Version)
                        {
                            //coppy hex file to FWTools folder
                            IsDownloadingFirmware = true;
                            await Task.Run(() => UpgradeSelectedDeviceFirmware(device, fwOutputLocation));
                            IsDownloadingFirmware = false;
                        }
                        else
                        {
                            var result = HandyControl.Controls.MessageBox.Show("Không có phiên bản mới cho thiết bị này, Bạn có muốn nạp lại phiên bản mới nhất không?", "Firmware uploading", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes)
                            {
                                IsDownloadingFirmware = true;
                                await Task.Run(() => UpgradeSelectedDeviceFirmware(device, fwOutputLocation));
                                IsDownloadingFirmware = false;
                            }
                            FrimwareUpgradeIsInProgress = false;
                        }
                    }
                }
            }
        }
        private void PromptDriverInstaller()
        {
            //coppy resource
            ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.CH372DRV.EXE", Path.Combine(JsonFWToolsFileNameAndPath, "CH372DRV.EXE"));
            //launch driver installer
            var drvInstlr = System.Diagnostics.Process.Start(Path.Combine(JsonFWToolsFileNameAndPath, "CH372DRV.EXE"));
            drvInstlr.WaitForExit();

            GeneralSettings.DriverRequested = false;
            // return;
        }
        private async Task UpgradeSelectedDeviceFirmware(IDeviceSettings device, string fwPath)
        {
            if (GeneralSettings.DriverRequested)
            {
                await Task.Run(() => PromptDriverInstaller());
                return;
            }
            //put device in dfu state
            device.DeviceState = DeviceStateEnum.DFU;
            // wait for device to enter dfu
            Thread.Sleep(1000);
            FwUploadPercentVissible = true;
            var startInfo = new System.Diagnostics.ProcessStartInfo {
                WorkingDirectory = JsonFWToolsFileNameAndPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = "/C vnproch55x " + fwPath
            };
            var proc = new Process() {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            // see below for output handler
            proc.ErrorDataReceived += proc_DataReceived;
            proc.OutputDataReceived += proc_DataReceived;

            proc.Start();

            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            proc.Exited += proc_FinishUploading;
        }
        private void proc_FinishUploading(object sender, System.EventArgs e)
        {
            //FwUploadPercent = 0;
            ////clear loading bar
            //FwUploadOutputLog = String.Empty;
            ////clear text box
            //percentCount = 0;
            ReloadDeviceLoadingVissible = true;

            Thread.Sleep(5000);
            CurrentDevice.DeviceState = DeviceStateEnum.Normal;

            if (FwUploadOutputLog.Split('\n').Last() == "Found no CH55x USB")
            {
                //there is a chance of missing driver so first we install CH375 driver first
                //execute CH375 driver

                //try to restart uploading by resetting the state

                HandyControl.Controls.MessageBox.Show("Update firmware không thành công, Không tìm thấy thiết bị ở trạng thái DFU", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // check for current device actual firmware version
                CurrentDevice.RefreshFirmwareVersion();
                // reset loading bar
                percentCount = 0;
                FwUploadPercent = 0;
                FwUploadPercentVissible = false;
                FwUploadOutputLog = string.Empty;

                HandyControl.Controls.MessageBox.Show("Update firmware thành công - Phiên bản : " + CurrentDevice.FirmwareVersion, "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Information);
                ReloadDeviceLoadingVissible = false;
                FrimwareUpgradeIsInProgress = false;
            }
        }

        private int percentCount = 0;

        private void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("[2K"))//clear current line
                {
                    percentCount++;
                    FwUploadPercent = percentCount * 100 / 308;

                    //Dispatcher.BeginInvoke(new Action(() => Prog.Value = percent));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (Environment.NewLine + percentCount)));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (e.Data)));
                }
                else
                {
                    FwUploadOutputLog += Environment.NewLine + e.Data;
                }
            }
        }

        private void UnZone()
        {
            var zoneList = PIDEditWindowsRichCanvasItems.Where(z => z is LEDSetup && z.IsSelected).ToList();
            //zoneList.ForEach(zone => zone.IsSelected = false);
            foreach (var zone in zoneList)
            {
                var currentZone = zone as LEDSetup;
                //(SurfaceEditorSelectedItem as ARGBLEDSlaveDevice).ControlableZones.Remove(currentZone);
                //add spot to temp bag
                var spotTempBag = new List<DeviceSpot>();
                foreach (var spot in currentZone.Spots)
                {
                    var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                    spotTempBag.Add(clonedSpot);
                }
                //remove zone from canvas

                foreach (var spot in spotTempBag)
                {
                    (spot as DeviceSpot).Left += currentZone.Left;
                    (spot as DeviceSpot).Top += currentZone.Top;
                    PIDEditWindowsRichCanvasItems.Add(spot as IDrawable);
                }
            }
            PIDEditWindowSelectedItems.Clear();
            zoneList.ForEach(zone => PIDEditWindowsRichCanvasItems.Remove(zone));

            //now add item from temp bag
        }

        private object NameChangingSelectedItem { get; set; }
        private void AddNewZone(ObservableCollection<IDrawable> itemSource, ObservableCollection<IDrawable> selectedItemsSource) // create new zone from selected spot fomr wellknown itemsource
        {
            var newZone = new LEDSetup();
            var spotList = itemSource.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            //spotList.ForEach(spot => spot.IsSelected = false);
            if (spotList.Count < 1)
                return;
            foreach (var spot in spotList)
            {
                var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                newZone.Spots.Add(clonedSpot);
            }
            newZone.UpdateSizeByChild(true);
            spotList.ForEach(spot => itemSource.Remove(spot));
            foreach (var spot in newZone.Spots)
            {
                (spot as DeviceSpot).Left -= newZone.Left;
                (spot as DeviceSpot).Top -= newZone.Top;
                (spot as DeviceSpot).IsSelected = false;
            }
            selectedItemsSource.Clear();
            itemSource.Add(newZone);
        }

        private void ImportPIDToCanvas()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn LED Setup file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "json";
            Import.Filter = "Text files (*.json)|*.json";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                try
                {
                    var ledSetup = JsonConvert.DeserializeObject<LEDSetup>(json);
                    foreach (var spot in ledSetup.Spots)
                    {
                        (spot as IDrawable).IsSelected = false;
                        PIDEditWindowsRichCanvasItems.Add(spot as IDrawable);
                    }

                    //prompt selection
                    //OpenSpotDataSelectionWindow();
                }
                catch (Exception)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private void ExportCurrentOutputPID()
        //{
        //    SaveFileDialog Export = new SaveFileDialog();
        //    Export.CreatePrompt = true;
        //    Export.OverwritePrompt = true;

        //    Export.Title = "Xuất dữ liệu";
        //    Export.FileName = CurrentOutput.OutputName + " LEDSetup";
        //    Export.CheckFileExists = false;
        //    Export.CheckPathExists = true;
        //    Export.InitialDirectory =
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    Export.RestoreDirectory = true;

        //    var ledsetupJson = JsonConvert.SerializeObject(CurrentOutput.OutputLEDSetup, new JsonSerializerSettings() {
        //        TypeNameHandling = TypeNameHandling.Auto
        //    });

        //    if (Export.ShowDialog() == DialogResult.OK)
        //    {
        //        //create directory with same name
        //        var newFolder = Directory.CreateDirectory(Export.FileName);
        //        var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
        //        //create main content
        //        File.WriteAllText(Path.Combine(Export.FileName, "content", CurrentOutput.OutputLEDSetup.Name + ".json"), ledsetupJson);
        //        //create info
        //        var info = new OnlineItemModel() {
        //            Name = CurrentOutput.OutputLEDSetup.Name,
        //            Owner = CurrentOutput.OutputLEDSetup.Owner,
        //            Description = CurrentOutput.OutputLEDSetup.Description,
        //            Type = "LEDSetup",
        //            SubType = CurrentOutput.OutputLEDSetup.TargetType
        //        };
        //        var infoJson = JsonConvert.SerializeObject(info, new JsonSerializerSettings() {
        //            TypeNameHandling = TypeNameHandling.Auto
        //        });
        //        File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
        //        //create image, require user input later thumb.png???

        //    }
        //}

        //private void ProcessSelectedSpotsWithRange(int rangeMinValue, int rangeMaxValue)
        //{
        //    int counter = 0;

        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.BorderThickness != 0)//spots is selecteds
        //        {
        //            counter++;
        //        }
        //    }
        //    var spacing = (rangeMaxValue - rangeMinValue) / counter;
        //    if (spacing < 1)
        //        spacing = 1;
        //    int offset = rangeMinValue;
        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.BorderThickness != 0)//spots is selected
        //        {
        //            switch (SetIDMode)
        //            {
        //                case "VID":
        //                    spot.SetVID(offset);
        //                    offset += spacing;
        //                    spot.IsEnabled = true;
        //                    break;

        //                case "MID":
        //                    spot.SetMID(offset);
        //                    offset += spacing;
        //                    break;
        //            }
        //        }
        //    }
        //}

        //private void ProcessSelectedSpotsID(string mode, string dirrection)
        //{
        //    switch (mode)
        //    {
        //        case "VID":
        //            switch (dirrection)
        //            {
        //                case "lefttoright":
        //                    int min = RangeMinValue;
        //                    int max = RangeMaxValue;
        //                    var selectedSpots = new List<IDeviceSpot>();
        //                    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //                    {
        //                        if (spot.BorderThickness != 0)//spots is selected
        //                        {
        //                            selectedSpots.Add(spot);
        //                        }
        //                    }
        //                    //re-arrange selectedSpots as X and Y to a list of increment order to set ID
        //                    //could implement drag move?
        //                    break;
        //            }

        //            break;

        //        case "MID":
        //            break;

        //        case "CID":
        //            break;
        //    }
        //}

        //private void ProcessSelectedSpots(string userInput)
        //{
        //    switch (SetIDMode)
        //    {
        //        case "VID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    int mIDToSet = Int32.Parse(userInput);
        //                    if (mIDToSet > 1023)
        //                        mIDToSet = 0;
        //                    spot.SetVID(mIDToSet);
        //                }
        //            }
        //            break;

        //        case "MID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    int mIDToSet = Int32.Parse(userInput);
        //                    if (mIDToSet > 1023)
        //                        mIDToSet = 0;
        //                    spot.SetMID(mIDToSet);
        //                }
        //            }
        //            break;

        //        case "CID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                int mIDToSet = Int32.Parse(userInput);
        //                if (mIDToSet > 32)
        //                    mIDToSet = 0;
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    spot.SetCID(mIDToSet);
        //                }
        //            }
        //            break;
        //    }
        //    RaisePropertyChanged(nameof(CurrentOutput.OutputLEDSetup));
        //}

        private void OpenFirmwareSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"FirmwareSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenSpotDataSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SpotDataSelection"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenDeviceConnectionSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceConnectionSettingsWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private async Task OpenDeviceFirmwareSettingsWindow()
        {
            //get device settings info
            if (CurrentDevice.DeviceType.Type == DeviceTypeEnum.AmbinoBasic || CurrentDevice.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE)
            {
                string fwversion = CurrentDevice.FirmwareVersion;
                if (fwversion == "unknown" || fwversion == string.Empty || fwversion == null)
                    fwversion = "1.0.0";
                var deviceFWVersion = new Version(fwversion);
                var requiredVersion = new Version();
                if (CurrentDevice.DeviceType.Type == DeviceTypeEnum.AmbinoBasic)
                {
                    requiredVersion = new Version("1.0.8");
                }
                else if (CurrentDevice.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE)
                {
                    requiredVersion = new Version("1.0.5");
                }
                if (deviceFWVersion >= requiredVersion)
                {
                    IsApplyingDeviceHardwareSettings = true;
                    var result = await Task.Run(() => CurrentDevice.GetHardwareSettings());
                    IsApplyingDeviceHardwareSettings = false;
                }
            }


            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceFirmwareSettingsWindow"}") is System.Windows.Window window)
            {
                //reset progress and log display
                FwUploadPercentVissible = false;
                percentCount = 0;
                FwUploadPercent = 0;
                FwUploadOutputLog = String.Empty;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }

        }

        private void OpenNameEditWindow(System.Windows.Window owner)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"RenameItemWindow"}") is System.Windows.Window window)
            {
                window.Owner = owner;
                window.ShowDialog();
            }
        }

        private void OpenAdvanceSettingWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"OutputAdvanceSettingsWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void launchSizeSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SizeSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
                window.ShowDialog();
            }
        }

        //private void ApplySpotImportData()
        //{
        //    int count = 0;
        //    if (SelectedSpotData[0]) // overwrite current spot data
        //    {
        //        //ignore all other spotdata because this will erase all
        //        CurrentOutput.OutputLEDSetup = _selectedLEDSetup;
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputLEDSetup));
        //    }
        //    if (SelectedSpotData[1])
        //    {
        //        //set ID by spot's matrix position

        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetVID(spot.VID);
        //            }
        //        }
        //    }
        //    if (SelectedSpotData[2])
        //    {
        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetMID(spot.MID);
        //            }
        //        }
        //    }
        //    if (SelectedSpotData[3])
        //    {
        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetCID(spot.CID);
        //            }
        //        }
        //    }
        //    if (count > 0)
        //    {
        //        HandyControl.Controls.MessageBox.Show("Một số LED không thể lấy vị trí do hình dạng LED khác nhau giữa file và thiết bị", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //    else
        //    {
        //        HandyControl.Controls.MessageBox.Show("Import thành công", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        //private void ReorderActivatedSpot()
        //{
        //    BackupSpots.Clear();
        //    BackupSpots = ActivatedSpots.OrderBy(o => o.id).ToList();
        //    // CurrentOutput.OutputLEDSetup.Spots = BackupSpots;
        //}

        //private void GrabActivatedSpot()
        //{
        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.IsActivated)
        //        {
        //            MaxLEDCount++;
        //            spot.SetID(1000);
        //            ActivatedSpots.Add(spot);
        //            spot.SetStroke(0);
        //        }
        //    }
        //}

        private void LaunchDeleteSelectedDeviceWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeleteSelectedDeviceWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ObservableCollection<IDrawable> _pIDEditWindowSelectedItems;

        public ObservableCollection<IDrawable> PIDEditWindowSelectedItems {
            get { return _pIDEditWindowSelectedItems; }
            set { _pIDEditWindowSelectedItems = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<IDrawable> _pIDEditWindowsRichCanvasItems;

        public ObservableCollection<IDrawable> PIDEditWindowsRichCanvasItems {
            get { return _pIDEditWindowsRichCanvasItems; }
            set { _pIDEditWindowsRichCanvasItems = value; RaisePropertyChanged(); }
        }

        private IDrawable _pIDEditWindowsRichCanvasSelectedItem;

        public IDrawable PIDEditWindowsRichCanvasSelectedItem {
            get { return _pIDEditWindowsRichCanvasSelectedItem; }
            set { _pIDEditWindowsRichCanvasSelectedItem = value; RaisePropertyChanged(); }
        }

        private string _currentIDType;

        public string CurrentIDType {
            get { return _currentIDType; }
            set { _currentIDType = value; }
        }

        private void RemoveGroup(ControlZoneGroup group)
        {
            //turn on Isregistering group
            //remove border from canvas
            if (group == null)
                return;
            LiveViewItems.Remove(LiveViewItems.Where(b => b.Name == group.Border.Name).FirstOrDefault());
            //set each zone selectable
            foreach (var zone in CurrentDevice.CurrentLiveViewZones)
            {
                if (group.GroupUID == zone.GroupID)
                {
                    //set each zone IsIngroup to false
                    //set maskedControl to null
                    zone.GroupID = null;
                    zone.IsInControlGroup = false;
                    (zone as IDrawable).IsSelectable = true;
                    //(zone as IControlZone).MaskedControlMode = null;
                }
            }
            //SelectedSlaveDevice = null;
            CurrentDevice.ControlZoneGroups.Remove(group);
            //
            //remove group
            //turn off IsRegisteringGroup
        }

        private void UngroupZone()
        {
            //turn on Isregistering group
            //remove border from canvas
            var borderToRemove = LiveViewItems.Where(p => p.IsSelected && p is Border).ToList();
            if (borderToRemove == null)
                return;
            borderToRemove.ForEach(b => LiveViewItems.Remove(b));
            foreach (var border in borderToRemove)
            {
                var currentGroup = CurrentDevice.ControlZoneGroups.Where(g => g.Border != null && g.Border.Name == border.Name && g.Type == CurrentDevice.CurrentActiveController.Type).FirstOrDefault();
                //set each zone selectable
                foreach (var zone in CurrentDevice.CurrentLiveViewZones)
                {
                    if (currentGroup.GroupUID == zone.GroupID)
                    {
                        //set each zone IsIngroup to false
                        //set maskedControl to null
                        zone.GroupID = null;
                        zone.IsInControlGroup = false;
                        (zone as IDrawable).IsSelectable = true;
                        (zone as IControlZone).CurrentActiveControlMode = (zone as IControlZone).AvailableControlMode.First();
                    }
                }
                SelectedSlaveDevice = null;
                CurrentDevice.ControlZoneGroups.Remove(currentGroup);
            }
            //turn off IsRegisteringGroup
        }
        private List<IDrawable> UngroupZone(List<IDrawable> groups)
        {
            //turn on Isregistering group
            //remove border from canvas
            var returnItems = new List<IDrawable>();
            if (groups == null)
                return null;
            groups.ForEach(b => LiveViewItems.Remove(b));
            foreach (var border in groups)
            {
                var currentGroup = CurrentDevice.ControlZoneGroups.Where(g => g.Border != null && g.Border.Name == border.Name && g.Type == CurrentDevice.CurrentActiveController.Type).FirstOrDefault();
                //set each zone selectable
                foreach (var zone in CurrentDevice.CurrentLiveViewZones)
                {
                    if (currentGroup.GroupUID == zone.GroupID)
                    {
                        //set each zone IsIngroup to false
                        //set maskedControl to null
                        zone.GroupID = null;
                        zone.IsInControlGroup = false;
                        (zone as IDrawable).IsSelectable = true;
                        (zone as IControlZone).CurrentActiveControlMode = (zone as IControlZone).AvailableControlMode.First();
                        returnItems.Add(zone as IDrawable);
                    }
                }
                SelectedSlaveDevice = null;
                CurrentDevice.ControlZoneGroups.Remove(currentGroup);
            }
            //turn off IsRegisteringGroup
            return returnItems;
        }


        private async Task MakeNewGroup()
        {
            if (CurrentDevice.ControlZoneGroups == null)
            {
                CurrentDevice.ControlZoneGroups = new ObservableCollection<ControlZoneGroup>();
            }
            var selectedItems = LiveViewItems.Where(z => z.IsSelected && z is not ARGBLEDSlaveDevice).ToList();
            //ungroup existed group
            var existedGroup = selectedItems.Where(i => i is Border).ToList();
            existedGroup.ForEach(i => selectedItems.Remove(i));
            var ungroupedItems = UngroupZone(existedGroup);
            if (ungroupedItems != null)
            {
                ungroupedItems.ForEach(i => selectedItems.Add(i));
            }
            if (selectedItems != null && selectedItems.Count > 1)
            {
                var newGroupName = "Group" + " " + (CurrentDevice.ControlZoneGroups.Count + 1).ToString();
                var newGroup = new ControlZoneGroup(newGroupName);
                await newGroup.AddZonesToGroup(selectedItems);
                LiveViewItems.Add(newGroup.Border);
                LiveViewSelectedItem = newGroup;
                SelectedControlZone = newGroup.MaskedControlZone;
                SelectedControlZone.CurrentActiveControlMode = newGroup.MaskedControlZone.AvailableControlMode.First();
                //set display slave device
                SelectedSlaveDevice = newGroup.MaskedSlaveDevice;
                CanUnGroup = true;
                CanGroup = false;
                CurrentDevice.ControlZoneGroups.Add(newGroup);
            }
        }

        private IControlZone _selectedControlZone;

        public IControlZone SelectedControlZone {
            get { return _selectedControlZone; }
            set { _selectedControlZone = value; RaisePropertyChanged(); }
        }

        private ISlaveDevice _selectedSlaveDevice;

        public ISlaveDevice SelectedSlaveDevice {
            get { return _selectedSlaveDevice; }
            set { _selectedSlaveDevice = value; RaisePropertyChanged(); }
        }

        private double _currentLiveViewWidth;

        public double CurrentLiveViewWidth {
            get { return _currentLiveViewWidth; }
            set { _currentLiveViewWidth = value; RaisePropertyChanged(); if (value > 0) UpdateLiveView(); }
        }

        private double _currentLiveViewHeight;

        public double CurrentLiveViewHeight {
            get { return _currentLiveViewHeight; }
            set { _currentLiveViewHeight = value; RaisePropertyChanged(); if (value > 0) UpdateLiveView(); }
        }

        private Point _currentLiveViewOffset;

        public Point CurrentLiveViewOffset {
            get { return _currentLiveViewOffset; }
            set { _currentLiveViewOffset = value; RaisePropertyChanged(); }
        }

        private double _selectionRectangleStrokeThickness = 2.0;

        public double SelectionRectangleStrokeThickness {
            get { return _selectionRectangleStrokeThickness; }

            set
            {
                _selectionRectangleStrokeThickness = value;
                RaisePropertyChanged();
            }
        }

        private Thickness _canvasItemBorder = new Thickness(2.0);

        public Thickness CanvasItemBorder {
            get { return _canvasItemBorder; }

            set
            {
                _canvasItemBorder = value;
                RaisePropertyChanged();
            }
        }

        private double _canvasScale = 1.0;

        public double CanvasScale {
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

        private void UpdateLiveView()
        {
            if (!IsLiveViewOpen)
                return;
            if (LiveViewItems.Count <= 0)
                return;
            //simple just set the scale
            //CurrentDevice.UpdateChildSize();
            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var liveViewItemsBound = DrawableHlprs.GetRealBound(LiveViewItems.Where(i => i is not PathGuide).ToArray());
            var widthScale = (CurrentLiveViewWidth - 50) / liveViewItemsBound.Width;
            var scaleHeight = (CurrentLiveViewHeight - 50) / liveViewItemsBound.Height;
            CanvasScale = Math.Min(widthScale, scaleHeight);
            var currentWidth = liveViewItemsBound.Width * CanvasScale;
            var currentHeight = liveViewItemsBound.Height * CanvasScale;
            //set current device offset
            CurrentLiveViewOffset = new Point(0 - liveViewItemsBound.Left * CanvasScale + (CurrentLiveViewWidth - currentWidth) / 2, 0 - liveViewItemsBound.Top * CanvasScale + (CurrentLiveViewHeight - currentHeight) / 2);
            Log.Information("Live View Updated");
        }

        private bool _showSelectedItemToolbar;

        public bool ShowSelectedItemToolbar {
            get { return _showSelectedItemToolbar; }
            set { _showSelectedItemToolbar = value; RaisePropertyChanged(); }
        }

        private bool _canGroup = false;

        public bool CanGroup {
            get { return _canGroup; }
            set { _canGroup = value; RaisePropertyChanged(); }
        }

        private bool _canUnGroup = false;

        public bool CanUnGroup {
            get { return _canUnGroup; }
            set { _canUnGroup = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<IDrawable> _liveViewSelectedItems;

        public ObservableCollection<IDrawable> LiveViewSelectedItems {
            get { return _liveViewSelectedItems; }

            set
            {
                _liveViewSelectedItems = value;
                RaisePropertyChanged();
            }
        }
        private bool _isLoadingControlMode;
        public bool IsLoadingControlMode {
            get { return _isLoadingControlMode; }
            set
            {
                _isLoadingControlMode = value;
                RaisePropertyChanged();
            }
        }
        private async Task ChangeSelectedControlZoneActiveControlMode(IControlMode controlMode)
        {
            IsLoadingControlMode = true;
            if (LiveViewSelectedItem is ControlZoneGroup)
            {
                var group = LiveViewSelectedItem as ControlZoneGroup;
                foreach (var zone in group.ControlZones)
                {
                    await Task.Run(() => { (zone as IControlZone).CurrentActiveControlMode = controlMode; });
                }
                await Task.Run(() => { group.MaskedControlZone.CurrentActiveControlMode = controlMode; });
            }

            else if (LiveViewSelectedItem is IControlZone)
            {
                var zone = LiveViewSelectedItem as IControlZone;
                await Task.Run(() => { zone.CurrentActiveControlMode = controlMode; });
            }

            IsLoadingControlMode = false;

        }

        private object LiveViewSelectedItem { get; set; }

        private void LiveViewSelectedItemChanged(IDrawable item)
        {

            var shouldBeSelected = IsInIDEditStage ? true : item.IsSelectable;
            if (shouldBeSelected)
            {
                item.IsSelected = true;
                ShowSelectedItemToolbar = true;
                SelectedControlZone = null;
                SelectedSlaveDevice = null;
                if (item is LEDSetup)
                {
                    CanUnGroup = false;
                    SelectedControlZone = item as IControlZone;
                    LiveViewSelectedItem = SelectedControlZone;
                    SelectedSlaveDevice = CurrentDevice.AvailableLightingDevices.Where(d => d.ControlableZones.Contains(item as IControlZone)).FirstOrDefault();
                }
                else if (item is FanMotor)
                {
                    CanUnGroup = false;
                    SelectedControlZone = item as IControlZone;
                    LiveViewSelectedItem = SelectedControlZone;
                    SelectedSlaveDevice = CurrentDevice.AvailablePWMDevices.Where(d => d.ControlableZones.Contains(item as IControlZone)).FirstOrDefault();
                }
                else if (item is Border)
                {
                    CanUnGroup = true;
                    foreach (var group in CurrentDevice.ControlZoneGroups)
                    {
                        if (group.Name == item.Name && group.Type == CurrentDevice.CurrentActiveController.Type)
                        {
                            LiveViewSelectedItem = group;
                            SelectedControlZone = group.MaskedControlZone;
                            //set display slave device
                            SelectedSlaveDevice = group.MaskedSlaveDevice;
                        }
                    }
                }
            }
        }

        private ObservableCollection<IDrawable> _liveViewItems;

        public ObservableCollection<IDrawable> LiveViewItems {
            get { return _liveViewItems; }

            set
            {
                _liveViewItems = value;
                RaisePropertyChanged();
            }
        }

        private int _vidCount;

        public int VIDCount {
            get { return _vidCount; }

            set
            {
                _vidCount = value;
                RaisePropertyChanged();
            }
        }

        private double _lastBrushX;
        private double _lastBrushY;

        private Double CalculateDelta(double lastX, double lastY, double currentX, double currentY)
        {
            var deltaX = currentX - lastX;
            var deltaY = currentY - lastY;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private void LiveViewSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //PIDEditWindowsRichCanvasSelectedItem = PIDEditWindowSelectedItems.Count == 1 ? PIDEditWindowSelectedItems[0] : null;
                ShowSelectedItemToolbar = true;
                //if (LiveViewItems.Where(p => p.IsSelected).Any(p => p is Border))
                //{
                //    CanGroup = false;
                //}
                //else
                //{
                //    CanGroup = true;
                //}
            }
        }
        public ObservableCollection<IDrawable> OOTBQUickSurfaceEditorItems { get; set; }
        public ObservableCollection<IDrawable> OOTBQUickSurfaceEditorSelectedItems { get; set; }
        //this called when ootb stage==2
        private void GetItemsReadyForOOTBQuickSurfaceEditor()
        {
            OOTBQUickSurfaceEditorItems = new ObservableCollection<IDrawable>();
            OOTBQUickSurfaceEditorSelectedItems = new ObservableCollection<IDrawable>();
            //add child
            foreach (var output in CurrentDevice.AvailableLightingOutputs.Where(o => o.IsEnabled))
            {
                var device = output.SlaveDevice as ARGBLEDSlaveDevice;
                device.IsSelected = true;
                OOTBQUickSurfaceEditorItems.Add(device);
            }
            //spread items
            if (OOTBQUickSurfaceEditorItems.Count <= 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải Enable ít nhất một output!!!", "No output", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SpreadItemVertical(OOTBQUickSurfaceEditorItems, 0);
            foreach (var item in OOTBQUickSurfaceEditorItems)
            {
                item.IsSelected = false;
            }
            //add surface border
            var border = new ScreenBound() {
                Width = 2000,
                Height = 2000,
                ShouldBringIntoView = true,
                IsDraggable = false,
                IsSelectable = false
            };
            OOTBQUickSurfaceEditorItems.Insert(0, border);
        }

        private void GetItemsForLiveView()
        {
            IsInIsolateMode = false;
            if (LiveViewItems == null)
            {
                LiveViewItems = new ObservableCollection<IDrawable>();
            }
            else
            {
                LiveViewItems.Clear();
            }
            LiveViewSelectedItems = new ObservableCollection<IDrawable>();

            IDrawable lastSelectedItem = null;
            foreach (var item in CurrentDevice.CurrentLiveViewZones)
            {
                if (!item.IsInControlGroup)
                    (item as IDrawable).IsSelectable = true;
                LiveViewItems.Add(item as IDrawable);
                if ((item as IDrawable).IsSelected)
                    lastSelectedItem = item as IDrawable;
            }
            if (CurrentDevice.ControlZoneGroups != null)
            {
                var groupList = new List<ControlZoneGroup>();
                var obsoleteGroupList = new List<ControlZoneGroup>();
                foreach (var group in CurrentDevice.CurrentLiveViewGroup)
                {
                    group.Init(CurrentDevice);
                    group.GetGroupBorder();
                    if (group.Border != null)
                    {
                        if ((group.Border.IsSelected))
                            lastSelectedItem = group.Border;
                        groupList.Add(group);
                    }
                    else
                    {
                        obsoleteGroupList.Add(group);
                    }
                }
                obsoleteGroupList.ForEach(g => CurrentDevice.ControlZoneGroups.Remove(g));
                var orderedGroups = groupList.OrderBy(o => o.Border.Width * o.Border.Height).ToList();
                foreach (var group in orderedGroups)
                {
                    LiveViewItems.Insert(0, group.Border);
                }
            }

            foreach (var output in CurrentDevice.AvailableLightingOutputs.Where(o => o.IsEnabled))
            {
                var lightingDevice = output.SlaveDevice as ARGBLEDSlaveDevice;

                LiveViewItems.Insert(0, lightingDevice);
            }
            //reset toolbard width
            ToolBarWidth = 450;
            if (IsInIDEditStage)
            {
                GetThingsReadyForIDEdit();
            }
            if (lastSelectedItem != null)
            {
                SelectLiveViewItemCommand.Execute(lastSelectedItem);
            }
            else
            {
                if (LiveViewItems.Count >= 1)
                {
                    SelectLiveViewItemCommand.Execute(LiveViewItems.Where(i => i.IsSelectable).First());
                }
            }


            ShowSelectedItemToolbar = true;



        }

        private void GetThingsReadyForIDEdit()
        {
            switch (IdEditMode)
            {
                case IDMode.VID:
                    GetToolsForIDEdit();
                    GetBrushForIDEdit();
                    ToolBarWidth = 52;
                    break;

                case IDMode.FID:
                    var _audioDeviceSelectionControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p is AudioDeviceSelectionButtonParameter).FirstOrDefault() as AudioDeviceSelectionButtonParameter;
                    CurrentVisualizer = AudioVisualizers[_audioDeviceSelectionControl.CapturingSourceIndex];
                    ToolBarWidth = 450;
                    break;
            }
        }

        private ObservableCollection<IDrawable> _vIDEditWindowSelectedItems;

        public ObservableCollection<IDrawable> VIDEditWindowSelectedItems {
            get { return _vIDEditWindowSelectedItems; }
            set { _vIDEditWindowSelectedItems = value; RaisePropertyChanged(); }
        }

        private CompositionEditWindow compositionEditWindow { get; set; }
        private Composition _currentSelectedComposition;

        public Composition CurrentSelectedComposition {
            get { return _currentSelectedComposition; }

            set
            {
                _currentSelectedComposition = value;
                RaisePropertyChanged();
            }
        }

        private ITimeLineDataItem _currentSelectedMotion;

        public ITimeLineDataItem CurrentSelectedMotion {
            get { return _currentSelectedMotion; }

            set
            {
                _currentSelectedMotion = value;
                RaisePropertyChanged();
            }
        }

        private bool _currentCompositionPlayingState;

        public bool CurrentCompositionPlayingState {
            get { return _currentCompositionPlayingState; }
            set { _currentCompositionPlayingState = value; RaisePropertyChanged(); }
        }

        private double _currentCompositionFrame;

        public double CurrentCompositionFrame {
            get { return _currentCompositionFrame; }
            set { _currentCompositionFrame = value; RaisePropertyChanged(); }
        }

        private double _timeLineHeight = 30;

        public double TimeLineHeight {
            get { return _timeLineHeight; }
            set { _timeLineHeight = value; RaisePropertyChanged(); }
        }

        private double _unitSize = 1;

        public double UnitSize {
            get { return _unitSize; }
            set { _unitSize = value; RaisePropertyChanged(); }
        }

        private void LaunchCompositionEditWindow()
        {
            ////////// testing binding logic/////////
            CurrentSelectedComposition = new Composition();
            CurrentSelectedComposition.Layers = new ObservableCollection<MotionLayer>();
            var layer1 = new MotionLayer();
            var layer2 = new MotionLayer();
            var emptyLayer = new MotionLayer();
            var emptyLayer1 = new MotionLayer();
            var emptyLayer2 = new MotionLayer();
            var emptyLayer3 = new MotionLayer();
            var emptyLayer4 = new MotionLayer();
            var bouncing = new TempDataType() {
                Name = "Bouncing",
                StartFrame = 5,
                EndFrame = 125,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 120,
                Color = Color.FromRgb(255, 255, 0)
            };
            var bouncing2 = new TempDataType() {
                Name = "Chasing",
                StartFrame = 20,
                EndFrame = 300,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 280,
                Color = Color.FromRgb(255, 0, 0)
            };
            layer1.Motions.Add(bouncing);
            layer2.Motions.Add(bouncing2);
            CurrentSelectedComposition.Layers.Add(layer1);
            CurrentSelectedComposition.Layers.Add(layer2);
            CurrentSelectedComposition.Layers.Add(emptyLayer);
            CurrentSelectedComposition.Layers.Add(emptyLayer1);
            CurrentSelectedComposition.Layers.Add(emptyLayer2);
            CurrentSelectedComposition.Layers.Add(emptyLayer3);
            CurrentSelectedComposition.Layers.Add(emptyLayer4);

            //catch collection changed to resize motions from source (path)
            foreach (var layer in CurrentSelectedComposition.Layers)
            {
                layer.Motions.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            var newMotions = e.NewItems;
                            foreach (TempDataType temp in newMotions)
                            {
                                //load motion from disk
                                //  var motion = ReadMotionFromResource(testMotionPath); // load test
                                //the resize state is implemeting inside render state
                                //ResizeMotion(motion, CurrentOutput.OutputLEDSetup.Spots.Count());
                            }
                            break;
                    }
                };
            }

            ///////
            compositionEditWindow = new CompositionEditWindow();
            compositionEditWindow.Owner = System.Windows.Application.Current.MainWindow;
            compositionEditWindow.ShowDialog();
        }

        private static Motion ReadMotionFromResource(string resourceName)
        {
            Motion motion = new Motion(256);
            motion.GUID = Guid.NewGuid().ToString();
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (StreamReader reader = new StreamReader(resource))
                {
                    string json = reader.ReadToEnd(); //Make string equal to full file
                    try
                    {
                        motion = JsonConvert.DeserializeObject<Motion>(json);
                    }
                    catch (Exception)
                    {
                        HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            return motion;
        }

        public void CutSelectedMotion(ITimeLineDataItem motion)
        {
            if (motion.StartFrame < CurrentCompositionFrame && CurrentCompositionFrame < motion.EndFrame)
            {
                var totalDuration = motion.OriginalDuration;
                motion.EndFrame = CurrentCompositionFrame;
                motion.OriginalDuration = motion.EndFrame - motion.StartFrame;
                //create new motion with the cutted out part
                var newMotion = new TempDataType() {
                    StartFrame = CurrentCompositionFrame,
                    OriginalDuration = totalDuration - motion.OriginalDuration,
                    Color = motion.Color,
                    Name = motion.Name + "_1",
                    EndFrame = CurrentCompositionFrame + totalDuration - motion.OriginalDuration
                };

                var currentLayer = CurrentSelectedComposition.Layers.Where(l => l.Motions.Contains(motion)).FirstOrDefault();
                currentLayer.Motions.Insert(currentLayer.Motions.IndexOf(motion) + 1, newMotion);
            }
        }

        public Motion ResizeMotion(Motion input, int framesize)
        {
            return null;
        }

        private ARGBLEDSlaveDevice CurrentEditingDevice { get; set; }

        private void PIDEditSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //PIDEditWindowsRichCanvasSelectedItem = PIDEditWindowSelectedItems.Count == 1 ? PIDEditWindowSelectedItems[0] : null;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (PIDEditWindowSelectedItems.Count == 0 || PIDEditWindowSelectedItems.Count > 1)
                {
                    PIDEditWindowsRichCanvasSelectedItem = null;
                }
            }
        }

        public ISlaveDevice CurrentEditingPIDItem { get; set; }
        private PIDQuickEditWindow pidQuickEditWindow { get; set; }

        private void OpenSpotPIDQuickEDitWindow(ObservableCollection<IDrawable> p)
        {
            CurrentIDType = "PID";
            CountPID = 0;
            CurrentEditingPIDItem = p.Where(p => p.IsSelected).FirstOrDefault() as ARGBLEDSlaveDevice;
            if (CurrentEditingPIDItem == null)
                return;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                (zone as LEDSetup).BackupSpots();
                (zone as LEDSetup).FillSpotsColor(Color.FromRgb(0, 0, 0));
            }
            pidQuickEditWindow = new PIDQuickEditWindow();
            pidQuickEditWindow.Owner = surfaceeditorWindow;
            pidQuickEditWindow.ShowDialog();
        }

        private void RotateSelectedSurfaceEditorItem(ObservableCollection<IDrawable> p)
        {
            if (SurfaceEditorSelectedDevice == null)
                return;
            var targetAngle = SurfaceEditorSelectedDevice.Angle + 90;
            if (targetAngle >= 360)
            {
                targetAngle -= 360;
            }
            SurfaceEditorSelectedDevice.RotateLEDSetup(targetAngle);
        }

        private void ReflectSelectedSurfaceEditorItem(ObservableCollection<IDrawable> p)
        {
            CurrentEditingPIDItem = p.Where(p => p.IsSelected).FirstOrDefault() as ARGBLEDSlaveDevice;
            CurrentEditingPIDItem.ReflectLEDSetupVertical();
        }

        private void SaveCurrentPID()
        {
            //check if any error
            var unfinishedZoneCount = 0;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                if ((zone as LEDSetup).Spots.Any(s => !s.IsEnabled))
                {
                    unfinishedZoneCount++;
                }
            }
            if (unfinishedZoneCount > 0)
            {
                HandyControl.Controls.MessageBox.Show("Chưa set hết ID, vui lòng kiểm tra lại", "Spot ID error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                pidQuickEditWindow.Close();
            }
        }

        private void CancelCurrentPID()
        {
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                (zone as LEDSetup).RestoreSpots();
                (zone as LEDSetup).FillSpotsColor(Color.FromRgb(0, 0, 0));
                pidQuickEditWindow.Close();
            }
        }

        public bool IsAudioSelectionOpen { get; set; }

        private void OpenAudioSelectorWindow()
        {
            if (ClickedAudioButtonParameter == null)
                return;
            if (AssemblyHelper.CreateInternalInstance($"View.{"AudioDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                IsAudioSelectionOpen = true;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.Closing += (_, __) => IsAudioSelectionOpen = false;
                window.ShowDialog();
            }
        }
        public bool IsRegionSelectionOpen { get; set; }
        private HandyControl.Controls.Window regionSelectionView { get; set; }
        public CapturingRegionSelectionButtonParameter ClickedRegionButtonParameter { get; set; }
        public AudioDeviceSelectionButtonParameter ClickedAudioButtonParameter { get; set; }
        private double _adjustingRectangleWidth;

        private Border _regionSelectionRect;
        public Border RegionSelectionRect {
            get { return _regionSelectionRect; }
            set
            {
                _regionSelectionRect = value;
                RaisePropertyChanged();
            }
        }
        // public WriteableBitmap SelectedBitmap => ClickedRegionButtonParameter.CapturingSourceIndex > 0 ? AvailableBitmaps[ClickedRegionButtonParameter.CapturingSourceIndex].Bitmap : AvailableBitmaps[0].Bitmap;
        private void OpenRegionSelectionWindow(string mode)
        {
            if (ClickedRegionButtonParameter == null)
                return;
            switch (mode)
            {
                case "screen":
                    ScreenBitmapCollectionInit();
                    UpdateRegionView();
                    regionSelectionView = new ScreenRegionSelectionWindow();
                    IsRegionSelectionOpen = true;
                    break;
                case "gif":
                    GifsCollectionInit();
                    var _gifControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
                    var selectedGif = _gifControl.SelectedValue;
                    ClickedRegionButtonParameter.CapturingSourceIndex = _gifControl.AvailableValues.IndexOf(_gifControl.SelectedValue);
                    Bitmap img = new Bitmap((selectedGif as Gif).LocalPath);
                    CalculateAdjustingRectangle(img, ClickedRegionButtonParameter.CapturingRegion);
                    regionSelectionView = new GifRegionSelectionWindow();
                    break;
            }

            regionSelectionView.Owner = System.Windows.Application.Current.MainWindow;
            regionSelectionView.Closing += (_, __) => IsRegionSelectionOpen = false;
            regionSelectionView.ShowDialog();



        }
        private void UpdateRegionView()
        {
            var index = ClickedRegionButtonParameter.CapturingSourceIndex > 0 && ClickedRegionButtonParameter.CapturingSourceIndex < AvailableBitmaps.Count ? ClickedRegionButtonParameter.CapturingSourceIndex : 0;
            if (index != ClickedRegionButtonParameter.CapturingSourceIndex)
                ClickedRegionButtonParameter.CapturingSourceIndex = index;
            SelectedBitmap = AvailableBitmaps[index];
            CalculateAdjustingRectangle(AvailableBitmaps[index].Bitmap, ClickedRegionButtonParameter.CapturingRegion);
        }
        public void CalculateAdjustingRectangle(WriteableBitmap bitmap, CapturingRegion region)
        {
            if (region.ScaleX > 1 || region.ScaleX < 0)
                region.ScaleX = 0;
            if (region.ScaleY > 1 || region.ScaleY < 0)
                region.ScaleY = 0;
            if (region.ScaleWidth > 1 || region.ScaleWidth < 0)
                region.ScaleWidth = 1;
            if (region.ScaleHeight > 1 || region.ScaleHeight < 0)
                region.ScaleHeight = 1;
            var left = region.ScaleX * bitmap.Width;
            var top = region.ScaleY * bitmap.Height;
            var width = region.ScaleWidth * bitmap.Width;
            var height = region.ScaleHeight * bitmap.Height;
            RegionSelectionRect = new Border() {
                Left = left,
                Top = top,
                Width = width,
                Height = height
            };
        }
        public void CalculateAdjustingRectangle(Bitmap bitmap, CapturingRegion region)
        {
            if (region.ScaleX > 1 || region.ScaleX < 0)
                region.ScaleX = 0;
            if (region.ScaleY > 1 || region.ScaleY < 0)
                region.ScaleY = 0;
            if (region.ScaleWidth > 1 || region.ScaleWidth < 0)
                region.ScaleWidth = 1;
            if (region.ScaleHeight > 1 || region.ScaleHeight < 0)
                region.ScaleHeight = 1;
            var left = region.ScaleX * bitmap.Width;
            var top = region.ScaleY * bitmap.Height;
            var width = region.ScaleWidth * bitmap.Width;
            var height = region.ScaleHeight * bitmap.Height;
            RegionSelectionRect = new Border() {
                Left = left,
                Top = top,
                Width = width,
                Height = height
            };
        }
        private void SaveCurrentSelectedScreenRegion()
        {
            var index = ClickedRegionButtonParameter.CapturingSourceIndex > 0 ? ClickedRegionButtonParameter.CapturingSourceIndex : 0;
            var bitmap = AvailableBitmaps[index].Bitmap;
            var newRegion = new CapturingRegion(
                RegionSelectionRect.Left / bitmap.Width,
                RegionSelectionRect.Top / bitmap.Height,
                RegionSelectionRect.Width / bitmap.Width,
                RegionSelectionRect.Height / bitmap.Height);
            ClickedRegionButtonParameter.CapturingRegion = newRegion;
            ClickedRegionButtonParameter.CapturingSourceRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
            regionSelectionView.Close();
        }
        private void SaveCurrentSelectedGifRegion()
        {

            var _gifControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
            var selectedGif = _gifControl.SelectedValue;
            Bitmap bitmap = new Bitmap((selectedGif as Gif).LocalPath);
            var newRegion = new CapturingRegion(
                RegionSelectionRect.Left / bitmap.Width,
                RegionSelectionRect.Top / bitmap.Height,
                RegionSelectionRect.Width / bitmap.Width,
                RegionSelectionRect.Height / bitmap.Height);
            ClickedRegionButtonParameter.CapturingRegion = newRegion;
            ClickedRegionButtonParameter.CapturingSourceRect = new Rect(0, 0, bitmap.Width / 8, bitmap.Height / 8);
            regionSelectionView.Close();
        }
        public void GifsCollectionInit()
        {
            AvailableGifs = new ObservableCollection<GifCard>();
            var _gifControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
            foreach (var gif in _gifControl.AvailableValues)
            {
                try
                {
                    Bitmap bmp = new Bitmap((gif as Gif).LocalPath);
                    AvailableGifs.Add(new GifCard() {
                        Name = gif.Name,
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Path = (gif as Gif).LocalPath,
                        Gif = gif as Gif
                    });
                    bmp.Dispose();
                }
                catch (Exception ex)
                {
                    //gif problem

                }

            }
        }

        public void ScreenBitmapCollectionInit()
        {
            if (AvailableBitmaps == null)
            {
                AvailableBitmaps = new ObservableCollection<DesktopFrameCard>();
            }
            AvailableBitmaps.Clear();
            lock (AvailableBitmaps)
            {
                if (GeneralSettings.IsMultipleScreenEnable)
                {
                    for (int i = 0; i < Screen.AllScreens.Length; i++)
                    {

                        var source = new WriteableBitmap((int)(Screen.AllScreens[i].Bounds.Width / 8.0), (int)(Screen.AllScreens[i].Bounds.Height / 8.0), 96, 96, PixelFormats.Bgra32, null);
                        var image = new DesktopFrameCard() {
                            Bitmap = source,
                            Name = Screen.AllScreens[i].DeviceName
                        };
                        AvailableBitmaps.Add(image);

                    }
                }
                else
                {
                    var source = new WriteableBitmap((int)(Screen.AllScreens[0].Bounds.Width / 8.0), (int)(Screen.AllScreens[0].Bounds.Height / 8.0), 96, 96, PixelFormats.Bgra32, null);
                    var image = new DesktopFrameCard() {
                        Bitmap = source,
                        Name = Screen.AllScreens[0].DeviceName
                    };
                    AvailableBitmaps.Add(image);
                }
            }
        }
        #region ControlModeSharing, Lighting Profiles
        private string _newLightingProfileName = "New Profile";

        public string NewLightingProfileName {
            get
            {
                return _newLightingProfileName;
            }

            set
            {
                _newLightingProfileName = value;

                RaisePropertyChanged();
            }
        }
        private string _newLightingProfilePlayListName = "New PlayList";

        public string NewLightingProfilePlayListName {
            get
            {
                return _newLightingProfilePlayListName;
            }

            set
            {
                _newLightingProfilePlayListName = value;

                RaisePropertyChanged();
            }
        }
        private string _newLightingProfileOwner;

        public string NewLightingProfileOwner {
            get
            {
                return _newLightingProfileOwner;
            }

            set
            {
                _newLightingProfileOwner = value;

                RaisePropertyChanged();
            }
        }

        private string _newLightingProfileDescription;

        public string NewLightingProfileDescription {
            get
            {
                return _newLightingProfileDescription;
            }

            set
            {
                _newLightingProfileDescription = value;
                RaisePropertyChanged();
            }
        }

        private void OpenCreateNewLightingProfileWindow()
        {
            var window = new AddNewLightingProfileWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        private LightingProfileManagerWindow _lightingProfileManagerWindow { get; set; }
        private void OpenLightingProfileManagerWindow()
        {
            //init available profiles
            _lightingProfileManagerWindow = new LightingProfileManagerWindow();
            _lightingProfileManagerWindow.DataContext = LightingProfileManagerViewModel;
            _lightingProfileManagerWindow.Owner = Application.Current.MainWindow;
            _lightingProfileManagerWindow.Show();
        }
        private void CreateNewLightingProfile()
        {

            var currentLightingMode = SelectedControlZone.CurrentActiveControlMode as LightingMode;
            var lightingProfile = new LightingProfile() {
                Name = NewLightingProfileName,
                Description = NewLightingProfileDescription,
                Owner = NewLightingProfileOwner,
                ControlMode = ObjectHelpers.Clone<LightingMode>(currentLightingMode),
                ProfileUID = Guid.NewGuid().ToString()
            };
            var contentjson = JsonConvert.SerializeObject(lightingProfile);
            File.WriteAllText(Path.Combine(LightingProfilesCollectionFolderPath, "collection", NewLightingProfileName + ".ALP"), contentjson);
            //reload collection
            LightingProfileManagerViewModel.AvailableLightingProfiles.AddItems(lightingProfile);

        }
        #endregion
        #region color and palette edit properties

        /// <summary>
        /// contains list of color that user can edit
        /// </summary>
        private ObservableCollection<ColorEditingObject> _colorList;

        public ObservableCollection<ColorEditingObject> ColorList {
            get { return _colorList; }

            set
            {
                _colorList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// this property store current color picked by hc:colorpicker
        /// </summary>
        private Color _currentPickedColor;

        public Color CurrentPickedColor {
            get => _currentPickedColor;

            set
            {
                Set(ref _currentPickedColor, value);
                foreach (var item in ColorList)
                {
                    if (item.IsSelected)
                    {
                        item.Color = value;
                    }
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// this property store current selected color on the color list
        /// </summary>
        private ColorEditingObject _currentSelectedColor;

        public ColorEditingObject CurrentSelectedColor {
            get { return _currentSelectedColor; }

            set
            {
                _currentSelectedColor = value;
                if (CurrentPickedColor != null)
                    CurrentPickedColor = value != null ? value.Color : Color.FromRgb(255, 255, 255);
                RaisePropertyChanged();
            }
        }

        private SelectionMode _colorListBoxSelectionMode = SelectionMode.Single;

        public SelectionMode ColorListBoxSelectionMode {
            get { return _colorListBoxSelectionMode; }

            set
            {
                _colorListBoxSelectionMode = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSolidMode;

        public bool IsSolidMode {
            get { return _isSolidMode; }

            set
            {
                _isSolidMode = value;
                RaisePropertyChanged();
                if (value)
                {
                    ColorListBoxSelectionMode = SelectionMode.Multiple;
                    foreach (var color in ColorList)
                    {
                        color.IsSelected = true;
                        color.Color = Color.FromRgb(255, 0, 0);
                    }
                }
                else
                {
                    ColorListBoxSelectionMode = SelectionMode.Single;
                    foreach (var color in ColorList)
                    {
                        color.IsSelected = false;
                    }
                    ColorList[0].IsSelected = true;
                }
            }
        }

        #endregion color and palette edit properties

        private void OpenColorPickerWindow(int numColor)
        {
            ColorPickerMode = "color";
            ColorList = new ObservableCollection<ColorEditingObject>();
            var currentColorControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault() as ListSelectionParameter;
            var currentSelectedColorCard = currentColorControl.SelectedValue as ColorCard;
            ColorList.Add(new ColorEditingObject() { Color = currentSelectedColorCard.StartColor });
            ColorList.Add(new ColorEditingObject() { Color = currentSelectedColorCard.StopColor });
            ColorListBoxSelectionMode = SelectionMode.Single;
            foreach (var color in ColorList)
            {
                color.IsSelected = true;
                color.Color = ColorList[0].Color;
            }
            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void ImportPaletteFromFile()
        {
            var palette = LocalFileHlprs.OpenImportFileDialog<ColorPalette>("col", "col files (*.col)|*.Col|Text files (*.txt)|*.Txt");
            if (palette != null)
            {
                ColorList = new ObservableCollection<ColorEditingObject>();
                foreach (var color in palette.Colors)
                {
                    var editingColor = new ColorEditingObject(color);
                    ColorList.Add(editingColor);
                }
            }
        }
        private void ImportChasingPatternFromFile()
        {
            var motion = LocalFileHlprs.OpenImportFileDialog("AML", "AML files (*.AML)|*.Aml|Text files (*.txt)|*.Txt");

            if (motion != null)
            {
                var newPattern = new ChasingPattern() {
                    LocalPath = motion,
                    Name = Path.GetFileName(motion),
                    Owner = "User imported Pattern"

                };
                AddNewChasingPatternToCollection(newPattern);
            }
        }
        private void ImportGifFromFile()
        {
            var gif = LocalFileHlprs.OpenImportFileDialog("gif", "gif files (*.gif)|*.Gif|Image files (*.jpg)|*.Jpeg");
            if (gif != null)
            {
                var newGif = new Gif() {
                    LocalPath = gif,
                    Name = Path.GetFileName(gif),
                    Owner = "User imported Gif"

                };
                AddNewGifToCollection(newGif);
            }
        }
        private int _currentPaletteNumColor = 16;
        public int CurrentPaletteNumColor {
            get
            {
                return _currentPaletteNumColor;

            }
            set
            {
                if (value >= 4 && value <= 128)
                {
                    ColorList = new ObservableCollection<ColorEditingObject>();
                    for (int i = 0; i < value; i++)
                    {
                        ColorList.Add(new ColorEditingObject() { Color = Color.FromRgb(0, 0, 0) });
                    }
                    _currentPaletteNumColor = value;
                    RaisePropertyChanged();
                }

            }
        }
        private string _colorPickerMode;
        public string ColorPickerMode {
            get
            {
                return _colorPickerMode;

            }
            set
            {
                _colorPickerMode = value;
                RaisePropertyChanged();
            }
        }
        private void OpenPaletteEditorWindow(int numColor)
        {

            ColorPickerMode = "palette";
            ColorList = new ObservableCollection<ColorEditingObject>();
            ColorListBoxSelectionMode = SelectionMode.Extended;
            var currentColorControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            var currentSelectedPalette = currentColorControl.SelectedValue as ColorPalette;
            CurrentPaletteNumColor = currentSelectedPalette.Colors.Count();
            foreach (var color in currentSelectedPalette.Colors)
            {
                ColorList.Add(new ColorEditingObject() { Color = color });
            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void AddNewColorToCollection()
        {
            var newColor = new ColorCard();
            newColor.StartColor = ColorList[0].Color;
            newColor.StopColor = ColorList[1].Color;
            //read current collection '
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(newColor);
        }

        private void AddNewPaletteToCollection(ColorPalette source)
        {
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(source);
            AddNewPaletteDialog?.Close();
        }
        private void AddNewChasingPatternToCollection(ChasingPattern source)
        {
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.ChasingPattern).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(source);
        }
        private void AddNewGifToCollection(Gif gif)
        {
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(gif);
        }

        private void WriteSimpleJson(object obj, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        private void OpenAboutWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AboutAppWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ObservableCollection<IDrawable> _surfaceEditorItems;

        public ObservableCollection<IDrawable> SurfaceEditorItems {
            get { return _surfaceEditorItems; }

            set
            {
                _surfaceEditorItems = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<IDrawable> SurfaceEditorSelectedItems { get; set; }
        private bool _canSelectMultipleItems;
        public bool CanSelectMultipleItems {
            get { return _canSelectMultipleItems; }

            set
            {
                _canSelectMultipleItems = value;
                RaisePropertyChanged(nameof(CanSelectMultipleItems));
            }
        }

        private double _itemScaleValue = 1.0;

        public double ItemScaleValue {
            get { return _itemScaleValue; }
            set { _itemScaleValue = value; RaisePropertyChanged(nameof(ItemScaleValue)); }
        }

        private double _itemRotateValue = 15.0;

        public double ItemRotateValue {
            get { return _itemRotateValue; }
            set { _itemRotateValue = value; RaisePropertyChanged(nameof(ItemRotateValue)); }
        }

        private void SetSelectedItemScaleFactor(ObservableCollection<IDrawable> itemSource)
        {
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                //find centerPoint
                Point center = new Point(item.CenterX, item.CenterY);
                //scale
                if (item.SetScale(ItemScaleValue, ItemScaleValue, false))
                {
                    //move back to center
                    item.Left = center.X - item.Width / 2;
                    item.Top = center.Y - item.Height / 2;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("kích thước scale quá nhỏ", "Zone size too small", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void SetSelectedItemRotateFactor(ObservableCollection<IDrawable> itemSource)
        {
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected && d is DeviceSpot);
            ////find center point of selected item(s)
            //var maxX = selectedItems.MaxBy(d => (d.Width + d.Left)).FirstOrDefault();
            //var maxY = selectedItems.MaxBy(d => (d.Height + d.Top)).FirstOrDefault();
            //var minX = selectedItems.MinBy(d => (d.Left)).FirstOrDefault();
            //var minY = selectedItems.MinBy(d => (d.Top)).FirstOrDefault();
            //var x = minX.Left + (maxX.Width + maxX.Left - minX.Left) / 2;
            //var y = minY.Top + (maxY.Height + maxY.Top - minY.Top) / 2;
            //System.Windows.Point centerPoint = new System.Windows.Point(x, y);
            foreach (var item in selectedItems)
            {
                //item.CenterX = x;
                //item.CenterY = y;
                item.Angle = ItemRotateValue;
            }
        }

        private void ResetToDefaultRectangleScale()
        {
            foreach (var item in SurfaceEditorItems.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Width = 100.0;
                item.Height = 100.0;
                item.SetScale(1.0, 1.0, true);
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
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected);
            IDrawable[] orderedItems = new IDrawable[selectedItems.Count()];
            orderedItems = selectedItems.OrderBy(i => i.Left).ToArray();
            switch (dirrection)
            {
                case 0:

                    for (int i = 0; i < orderedItems.Count(); i++)
                    {
                        if (i == 0)
                            continue;
                        var previousLeft = orderedItems[i - 1].Left;
                        orderedItems[i].Left = orderedItems[i - 1].Width + previousLeft + spacing;

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < orderedItems.Count(); i++)
                    {
                        if (i == 0)
                            continue;
                        var previousLeft = orderedItems[i - 1].Left;
                        orderedItems[i].Left = previousLeft - (orderedItems[i].Width + spacing);

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;
            }
        }

        private void SpreadItemVertical(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min Y
            double spacing = 10.0;
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected);
            IDrawable[] orderedItems = new IDrawable[selectedItems.Count()];
            orderedItems = selectedItems.OrderBy(i => i.Top).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            continue;
                        var previousTop = orderedItems[i - 1].Top;
                        orderedItems[i].Top = orderedItems[i - 1].Height + previousTop + spacing;

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;

                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            continue;
                        var previousTop = orderedItems[i - 1].Top;
                        orderedItems[i].Top = previousTop - (orderedItems[i].Height + spacing);

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

        public class DrawableShape
        {
            public Geometry Geometry { get; set; }
            public string Name { get; set; }
        }

        public DrawableShape SelectedShape { get; set; }
        public double NewItemWidth { get; set; }
        public double NewItemHeight { get; set; }
        public int ItemNumber { get; set; }
        private ObservableCollection<DrawableShape> _availableShapeToAdd;

        public ObservableCollection<DrawableShape> AvailableShapeToAdd {
            get { return _availableShapeToAdd; }

            set
            {
                _availableShapeToAdd = value;
                RaisePropertyChanged();
            }
        }
        private Point _mousePosition;

        public Point MousePosition {
            get { return _mousePosition; }

            set
            {
                _mousePosition = value; RaisePropertyChanged();
                if (IsInIDEditStage)
                {
                    if (SelectedToolIndex == 1)
                    {
                        IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                        IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                    }
                    else if (SelectedToolIndex == 2)
                    {
                        EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                        EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                    }
                }
            }
        }

        #region pid canvas viewmodel

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

                var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
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
                    PIDEditWindowsRichCanvasItems.Add(item);
                    //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void AddSpotGeometry()
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

                Geometry geo = (Geometry)XamlReader.Load(reader);

                var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
                var newItem = new DrawableShape() {
                    Geometry = geo,
                };
                AvailableShapeToAdd.Add(newItem);
            }
            catch (Exception ex)
            {
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
                var image = new ImageVisual {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    ImagePath = addImage.FileName,
                    Height = bitmap.Height,
                    Width = bitmap.Width,
                    IsResizeable = true,
                    IsDeleteable = true
                };
                PIDEditWindowsRichCanvasItems.Insert(0, image);
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

        private void AddItemsToPIDCanvas()
        {
            var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
            var newItems = new ObservableCollection<IDrawable>();
            if (ItemNumber == 0)
            {
                ItemNumber = 1;
            }
            for (int i = 0; i < ItemNumber; i++)
            {
                var newItem = new DeviceSpot(
                    MousePosition.Y,
                    MousePosition.X,
                    NewItemWidth > 0 ? NewItemWidth : 20.0,
                    NewItemHeight > 0 ? NewItemHeight : 20.0,
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
                    SelectedShape.Geometry);
                newItem.IsSelected = true;
                newItem.IsDeleteable = true;
                newItems.Add(newItem);
            }
            SpreadItemHorizontal(newItems, 0);
            foreach (var item in newItems)
            {
                PIDEditWindowsRichCanvasItems.Add(item);
                //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
            }
        }

        private void ClearPIDCanvas()
        {
            CountPID = 0;
            var itemToRemove = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot || s is LEDSetup).ToList();
            PIDEditWindowSelectedItems.Clear();
            itemToRemove.ForEach(item => PIDEditWindowsRichCanvasItems.Remove(item));
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

        #endregion pid canvas viewmodel

        private DrawableHelpers DrawableHlprs { get; set; }
        private ControlModeHelpers CtrlHlprs { get; set; }
        private async void SaveCurrentSurfaceLayout()
        {
            var screens = SurfaceEditorItems.OfType<IDrawable>().Where(d => d is ScreenBound);
            foreach (var item in SurfaceEditorItems.Where(i => i is ARGBLEDSlaveDevice))
            {
                item.IsSelected = false;
            }

            if (IsLiveViewOpen)
            {
                GetItemsForLiveView();
                UpdateLiveView();
            }
            else
            {
                foreach (var device in AvailableDevices)
                {
                    await Task.Run(() => UpdateDeviceGroupLayout(device));
                }
            }
            surfaceeditorWindow.Close();
            surfaceeditorWindow = null;
            SurfaceEditorItems = null;
            SurfaceEditorSelectedItems = null;
            IsRichCanvasWindowOpen = false;
        }
        private void UpdateDeviceGroupLayout(IDeviceSettings device)
        {
            if (device.ControlZoneGroups != null)
            {
                foreach (var group in device.ControlZoneGroups)
                {
                    group.GetGroupBorder();
                }
            }
        }
        private void ResetPID()
        {
            CountPID = 0;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                foreach (var spot in ledZone.Spots)
                {
                    spot.SetColor(0, 0, 0, true);
                    spot.IsEnabled = false;
                    spot.SetID(0);
                }
                // ledZone.ReorderSpots();
            }
        }

        private void SetZoneMID(int id)
        {
            var selectedZone = SelectedControlZone as LEDSetup;
            foreach (var spot in selectedZone.Spots)
            {
                spot.SetMID(id);
            }
        }

        private void SetSpotID(IDeviceSpot spot)
        {
            switch (CurrentIDType)
            {
                case "VID":
                    spot.SetVID(CountVID += GapVID);
                    spot.IsEnabled = true;
                    if (CountVID > 1023)
                        CountVID = 0;
                    break;

                case "FID":
                    spot.SetMID(CountVID += GapVID);
                    spot.IsEnabled = true;
                    if (CountVID > 1023)
                        CountVID = 0;
                    break;

                case "CID":
                    spot.SetCID(CountVID += GapVID);
                    spot.IsEnabled = true;
                    if (CountVID > 32)
                        CountVID = 0;
                    break;

                case "PID":
                    if (spot.IsEnabled)
                    {
                        spot.SetColor(0, 0, 0, true);
                        spot.IsEnabled = false;
                        if (CountPID >= 1)
                        {
                            CountPID -= 1;
                        }
                        else
                            CountPID = 0;
                    }
                    else
                    {
                        spot.SetID(CountPID++);
                        spot.IsEnabled = true;
                        spot.SetColor(0, 0, 255, true);
                    }
                    //foreach(var zone in CurrentEditingPIDItem.ControlableZones)
                    //{
                    //    (zone as LEDSetup).ReorderSpots();
                    //}
                    break;
            }
        }
        private ARGBLEDSlaveDevice _surfaceEditorSelectedDevice;
        private Color _currentPreviewColor = Color.FromRgb(255, 255, 255);
        public Color CurrentPreviewColor {
            get { return _currentPreviewColor; }
            set
            {
                _currentPreviewColor = value;
                RaisePropertyChanged();
            }
        }
        public ARGBLEDSlaveDevice SurfaceEditorSelectedDevice { get { return _surfaceEditorSelectedDevice; } set { _surfaceEditorSelectedDevice = value; RaisePropertyChanged(); } }

        private double _selectedItemScaleValue;
        public double SelectedItemScaleValue { get { return _selectedItemScaleValue; } set { _selectedItemScaleValue = value; RaisePropertyChanged(); } }
        private double _selectedItemRotationValue;
        public double SelectedItemRotationValue { get { return _selectedItemRotationValue; } set { _selectedItemRotationValue = value; RaisePropertyChanged(); } }
        public List<RGBLEDOrderEnum> AvailableRGBOrders {
            get
            {
                var list = new List<RGBLEDOrderEnum>();
                foreach (RGBLEDOrderEnum type in Enum.GetValues(typeof(RGBLEDOrderEnum)))
                {
                    list.Add(type);
                }
                return list;
            }
        }
        private void OpenSurfaceEditorWindow()
        {
            SurfaceEditorItems = new ObservableCollection<IDrawable>();
            SurfaceEditorSelectedItems = new ObservableCollection<IDrawable>();
            foreach (var device in AvailableDevices)
            {
                foreach (var output in device.AvailableLightingOutputs.Where(o => o.IsEnabled))
                {
                    SurfaceEditorItems.Add(output.SlaveDevice as ARGBLEDSlaveDevice);
                }

            }
            var border = new ScreenBound() {
                Width = 2000,
                Height = 2000,
                ShouldBringIntoView = true,
                IsDraggable = false,
                IsSelectable = false
            };
            SurfaceEditorItems.Add(border);
            surfaceeditorWindow = new SurfaceEditorWindow();
            IsRichCanvasWindowOpen = true;

            surfaceeditorWindow.Owner = System.Windows.Application.Current.MainWindow;
            surfaceeditorWindow.ShowDialog();
        }

        private SurfaceEditorWindow surfaceeditorWindow { get; set; }



        private OnlineItemExporterView itemExporter { get; set; }
        public object CurrentOnlineItemToExport { get; set; }
        private ObservableCollection<ISlaveDevice> _availableARGBSlaveDevices;

        public ObservableCollection<ISlaveDevice> AvailableARGBSlaveDevices {
            get { return _availableARGBSlaveDevices; }

            set
            {
                _availableARGBSlaveDevices = value;
                RaisePropertyChanged();
            }
        }

        private SlaveDeviceCollectionView SlaveDeviceSelection { get; set; }
        public IOutputSettings CurrentSelectedOutputMap { get; set; }

        private void OpenSlaveDeviceSelector(OutputSettings o)
        {
            //get items ready
            //load local folder
            AvailableARGBSlaveDevices = new ObservableCollection<ISlaveDevice>();
            CurrentSelectedOutputMap = o;
            SlaveDeviceSelection = new SlaveDeviceCollectionView();
            SlaveDeviceSelection.Owner = OOTBWindows;
            SlaveDeviceSelection.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SlaveDeviceSelection.Show(); //open

            Task.Run(() => LoadLocalExistedSlaveDevice());


        }
        private void RefreshLocalSlaveDeviceCollection()
        {
            AvailableARGBSlaveDevices = new ObservableCollection<ISlaveDevice>();
            Task.Run(() => LoadLocalExistedSlaveDevice());
        }
        private bool _isSlaveDeviceCollectionLoading;
        public bool IsSlaveDeviceCollectionLoading {
            get
            {
                return _isSlaveDeviceCollectionLoading;
            }
            set
            {
                _isSlaveDeviceCollectionLoading = value;
                RaisePropertyChanged();
            }
        }

        private async void LoadLocalExistedSlaveDevice()
        {
            IsSlaveDeviceCollectionLoading = true;
            string[] existedSlaveDeviceFolder = Directory.GetDirectories(SupportedDeviceCollectionFolderPath);

            foreach (var deviceFolder in existedSlaveDeviceFolder)
            {
                if (File.Exists(Path.Combine(deviceFolder, "config.json")))
                {
                    var json = File.ReadAllText(Path.Combine(deviceFolder, "config.json"));
                    try
                    {
                        var existedDevice = JsonConvert.DeserializeObject<ARGBLEDSlaveDevice>(json);
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (existedDevice != null)
                                AvailableARGBSlaveDevices.Add(existedDevice);
                        });

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    // existedDevice.Thumbnail = Path.Combine(deviceFolder, "thumbnail.png");

                }
            }
            IsSlaveDeviceCollectionLoading = false;
        }
        private void CreateNewAutomation()
        {
            var name = NewAutomationName;
            AutomationSettings newAutomation = new AutomationSettings { Name = name };
            AvailableAutomations.Add(newAutomation);
            if (newAutomation.Condition != null && newAutomation.Condition is SystemEventTriggerCondition)
            {
                var condition = newAutomation.Condition as SystemEventTriggerCondition;
                if (condition.Event == SystemEventEnum.Shutdown)
                {
                    ShutdownAutomations.Add(newAutomation);
                }
            }
            WriteAutomationCollectionJson();
        }

        private void Reset()
        {
            //disable all device
            if (File.Exists(JsonDeviceFileNameAndPath))
            {
                File.Delete(JsonDeviceFileNameAndPath);
            }
            if (File.Exists(JsonDeviceProfileFileNameAndPath))
            {
                File.Delete(JsonDeviceProfileFileNameAndPath);
            }
            if (File.Exists(JsonGeneralFileNameAndPath))
            {
                File.Delete(JsonGeneralFileNameAndPath);
            }
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }

        private void DeleteSelectedAutomation(AutomationSettings automation)
        {
            if (automation.IsLocked)
                return;
            AvailableAutomations.Remove(automation);
            WriteAutomationCollectionJson();
        }

        public FTPServerHelpers FTPHlprs { get; set; }

        private AddNewPaletteWindow AddNewPaletteDialog;

        public void OpenCreateNewPaletteDialog()
        {
            AddNewPaletteDialog = new AddNewPaletteWindow();
            AddNewPaletteDialog.Owner = System.Windows.Application.Current.MainWindow;
            AddNewPaletteDialog.ShowDialog();
        }

        public void OpenAvailableUpdateListWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"UpdateSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private int _oOTBStage;

        public int OOTBStage {
            get { return _oOTBStage; }
            set { _oOTBStage = value; RaisePropertyChanged(); }
        }

        private OOTBWindow OOTBWindows { get; set; }
        private ObservableCollection<IDrawable> _ootbItems;

        public ObservableCollection<IDrawable> OOTBItems {
            get { return _ootbItems; }
            set { _ootbItems = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<IDrawable> OOTBSelectedItems { get; set; }
        public IDrawable OOTBSelectedItem { get; set; }

        public void OpenLEDSteupSelectionWindows()
        {
            GetItemReadyForOOTB();
            OOTBWindows = new OOTBWindow();
            OOTBWindows.Owner = System.Windows.Application.Current.MainWindow;
            OOTBWindows.ShowDialog();
        }

        private void GetItemReadyForOOTB()
        {
            OOTBItems = new ObservableCollection<IDrawable>();
            OOTBSelectedItems = new ObservableCollection<IDrawable>();
            //get image ready
            //output map
            foreach (var output in CurrentDevice.AvailableLightingOutputs)
            {
                OOTBItems.Add(output as OutputSettings);
            }
        }
        private void CreateFWToolsFolderAndFiles()
        {
            if (!Directory.Exists(JsonFWToolsFileNameAndPath))
            {
                Directory.CreateDirectory(JsonFWToolsFileNameAndPath);
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.busybox.exe", Path.Combine(JsonFWToolsFileNameAndPath, "busybox.exe"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.CH375DLL.dll", Path.Combine(JsonFWToolsFileNameAndPath, "CH375DLL.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libgcc_s_sjlj-1.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libgcc_s_sjlj-1.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libusb-1.0.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusb-1.0.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.libusbK.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusbK.dll"));
                ResourceHlprs.CopyResource("adrilight_shared.Tools.FWTools.vnproch55x.exe", Path.Combine(JsonFWToolsFileNameAndPath, "vnproch55x.exe"));
                //required fw version
            }
            CreateRequiredFwVersionJson();
        }

        private void CreateColorCollectionFolder()
        {
            if (!Directory.Exists(ColorsCollectionFolderPath))
            {
                Directory.CreateDirectory(ColorsCollectionFolderPath);
                //get data from resource file and copy to local folder
                var colorCollectionResourcePath = "adrilight_shared.Resources.Colors.ColorCollection.json";
                ResourceHlprs.CopyResource(colorCollectionResourcePath, Path.Combine(ColorsCollectionFolderPath, "collection.json"));
                //Create deserialize config
                var config = new ResourceLoaderConfig(nameof(ColorCard), DeserializeMethodEnum.SingleJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(ColorsCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }
        private void CreateGifCollectionFolder()
        {
            if (!Directory.Exists(GifsCollectionFolderPath))
            {
                Directory.CreateDirectory(GifsCollectionFolderPath);
                var collectionFolder = Path.Combine(GifsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".gif")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(Gif), DeserializeMethodEnum.Files);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(GifsCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }
        private void CreateVIDCollectionFolder()
        {
            if (!Directory.Exists(VIDCollectionFolderPath))
            {
                Directory.CreateDirectory(VIDCollectionFolderPath);
                var collectionFolder = Path.Combine(VIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var vidCollection = new List<VIDDataModel>();
                var lef2Right = new VIDDataModel() {
                    Name = "Trái sang phải",
                    IsDeleteable = false,
                    Geometry = "left2right",
                    Description = "Màu chạy từ trái sang phải",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.left2right
                };
                var right2Left = new VIDDataModel() {
                    Name = "Phải sang trái",
                    IsDeleteable = false,
                    Geometry = "right2left",
                    Description = "Màu chạy từ phải sang trái",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.right2left
                };
                var up2Down = new VIDDataModel() {
                    Name = "Trên xuống dưới",
                    IsDeleteable = false,
                    Geometry = "topdown",
                    Description = "Màu chạy từ trên xuống dưới",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.top2bot
                };
                var down2Up = new VIDDataModel() {
                    Name = "Dưới lên trên",
                    IsDeleteable = false,
                    Geometry = "bottomup",
                    Description = "Màu chạy từ dưới lên trên",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.bot2top
                };
                var linear = new VIDDataModel() {
                    Name = "Tuyến tính",
                    Geometry = "back",
                    Description = "Màu chạy theo thứ tự LED",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.linear
                };
                vidCollection.Add(lef2Right);
                vidCollection.Add(right2Left);
                vidCollection.Add(up2Down);
                vidCollection.Add(down2Up);
                vidCollection.Add(linear);
                foreach (var vid in vidCollection)
                {
                    var json = JsonConvert.SerializeObject(vid);
                    File.WriteAllText(Path.Combine(collectionFolder, vid.Name + ".json"), json);
                }
                //coppy all internal palettes to local
                var config = new ResourceLoaderConfig(nameof(VIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(VIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }

        private void CreateMIDCollectionFolder()
        {
            if (!Directory.Exists(MIDCollectionFolderPath))
            {
                Directory.CreateDirectory(MIDCollectionFolderPath);
                var collectionFolder = Path.Combine(MIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var midCollection = new List<MIDDataModel>();
                var low = new MIDDataModel() {
                    Name = "Low",
                    Description = "Trung bình của các tần số thấp (Bass)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Low
                };
                var middle = new MIDDataModel() {
                    Name = "Mid",
                    Description = "Trung bình của các tần số trung (Mid)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Middle
                };
                var high = new MIDDataModel() {
                    Name = "High",
                    Description = "Trung bình của các tần số cao (Treble)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.High
                };

                var custom = new MIDDataModel() {
                    Name = "Custom",
                    Description = "Chọn Tần số bằng tay",
                    ExecutionType = VIDType.PredefinedID
                };
                midCollection.Add(low);
                midCollection.Add(middle);
                midCollection.Add(high);
                midCollection.Add(custom);
                foreach (var mid in midCollection)
                {
                    var json = JsonConvert.SerializeObject(mid);
                    File.WriteAllText(Path.Combine(collectionFolder, mid.Name + ".json"), json);
                }
                //coppy all internal palettes to local
                var config = new ResourceLoaderConfig(nameof(MIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(MIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection
        }

        private void CreateRequiredFwVersionJson()
        {
            var ABR1p = new DeviceFirmware() {
                Name = "ABR1p.hex",
                Version = "1.0.6",
                TargetHardware = "ABR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ABR1p.hex"
            };
            var ABR2e = new DeviceFirmware() {
                Name = "ABR2e.hex",
                Version = "1.0.9",
                TargetHardware = "ABR2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ABR2e.hex"
            };
            var AER1e = new DeviceFirmware() {
                Name = "AER1e.hex",
                Version = "1.0.4",
                TargetHardware = "AER1e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AER1e.hex"
            };
            var AER2e = new DeviceFirmware() {
                Name = "AER2e.hex",
                Version = "1.0.6",
                TargetHardware = "AER2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AER2e.hex"
            };
            var AFR1g = new DeviceFirmware() {
                Name = "AFR1g.hex",
                Version = "1.0.3",
                TargetHardware = "AFR1g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR1g.hex"
            };
            var AFR2g = new DeviceFirmware() {
                Name = "AFR2g.hex",
                Version = "1.0.5",
                TargetHardware = "AFR2g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR2g.hex"
            };
            var AFR3g = new DeviceFirmware() {
                Name = "AFR3g.hex",
                Version = "1.0.6",
                TargetHardware = "AFR3g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AFR3g.hex"
            };
            var AHR2g = new DeviceFirmware() {
                Name = "AHR2g.hex",
                Version = "1.0.1",
                TargetHardware = "AHR2g",
                TargetDeviceType = DeviceTypeEnum.AmbinoHUBV3,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.AHR2g.hex"
            };
            var ARR1p = new DeviceFirmware() {
                Name = "ARR1p.hex",
                Version = "1.0.2",
                TargetHardware = "ARR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoRainPowPro,
                Geometry = "binary",
                ResourceName = "adrilight_shared.DeviceFirmware.ARR1p.hex"
            };
            var firmwareList = new List<DeviceFirmware>();
            firmwareList.Add(ABR1p);
            firmwareList.Add(ABR2e);
            firmwareList.Add(AER1e);
            firmwareList.Add(AER2e);
            firmwareList.Add(AFR1g);
            firmwareList.Add(AHR2g);
            firmwareList.Add(ARR1p);
            firmwareList.Add(AFR2g);
            firmwareList.Add(AFR3g);
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P without PowerLED Support    | CH552P | 32 | 14 | ABR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E Without PowerLED Support    | CH552E | 15 | 17 | ABR1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E(rev2) With PowerLED Support | CH552e | 14 | 15 | ARR2e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552E Without PowerLED Support     | CH552E | 15 | 17 | AER1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P(rev2) With PowerLED Support | CH552P |    |    | ABR2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552P (rev2) With PowerLED Support | CH552P |    |    | AER2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev1                       | CH552G |    |    | AFR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev2                       | CH552G |    |    | AFR2g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev1                        | CH552G |    |    | AHR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino RainPow CH552P rev1                      | CH552P |    |    | ARR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev2                        | CH552G |    |    | AHR2g |
            //+-------------------------------------------------+--------+----+----+-------+
            var requiredFwVersionjson = JsonConvert.SerializeObject(firmwareList, Formatting.Indented);
            File.WriteAllText(JsonFWToolsFWListFileNameAndPath, requiredFwVersionjson);
        }
        private void LoadAvailableBaudRate()
        {
            AvailableBaudrates = new ObservableCollection<int> {
                9600,
                19200,
                38400,
                57600,
                115200,
                230400,
                460800,
                576000,
                921600,
                1000000,
                1500000,
                2000000,
                2500000,
            };
        }
        private void LoadAvailableProfiles()
        {
            AvailableProfiles = new ObservableCollection<AppProfile>();
            foreach (var profile in LoadAppProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }
        }
        private PasswordDialog pwDialog { get; set; }
        private void SFTPInit(AppUser user)
        {
            if (user == null)
                return;

            if (FTPHlprs != null && FTPHlprs.sFTP.IsConnected)
            {
                FTPHlprs.sFTP.Disconnect();
            }
            string host = @"103.148.57.184";
            string userName = user.LoginName;
            string password = user.LoginPassword;
            FTPHlprs = new FTPServerHelpers();
            FTPHlprs.sFTP = new SftpClient(host, 1512, userName, password);
            paletteFolderpath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/ColorPalettes";
            chasingPatternsFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/ChasingPatterns";
            gifxelationsFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/Gifxelations";
            SupportedDevicesFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/SupportedDevices";
            ProfilesFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/Profiles";
            thumbResourceFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/Resources/Thumbs";
            openRGBDevicesFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/OpenRGBDevices";
            ambinoDevicesFolderPath = "/home/" + GeneralSettings.CurrentAppUser.DataBaseUserName + "/ftp/files/AmbinoDevices";

        }
        private string _developerPassword;
        public string DeveloperPassword {
            get
            {
                return _developerPassword;
            }
            set
            {
                _developerPassword = value;
                RaisePropertyChanged();
            }
        }
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
                catch (SshAuthenticationException ex)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        pwDialog = new PasswordDialog();
                        pwDialog.Owner = StoreWindow;
                        var result = pwDialog.ShowDialog();
                        if (result != null && result.Value)
                        {
                            GeneralSettings.CurrentAppUser.LoginPassword = pwDialog.UnsafePassword;
                            SFTPInit(GeneralSettings.CurrentAppUser);
                        }
                    });

                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show("Adrilight Server không khả dụng ở thời điểm hiện tại", "Server notfound", MessageBoxButton.OK, MessageBoxImage.Error);
                    //return;
                }
            }
        }

        private DeviceSearchingScreen searchingDeviceScreen { get; set; }
        public void LoadData()
        {

            if (ResourceHlprs == null)
                ResourceHlprs = new ResourceHelpers();
            if (LocalFileHlprs == null)
                LocalFileHlprs = new LocalFileHelpers();
            if (DeviceHlprs == null)
                DeviceHlprs = new DeviceHelpers();
            //create Public user
            LoadAvailableAppUser();
            //Task.Run(() => SFTPInit(GeneralSettings.CurrentAppUser));
            #region checking and creating resource folder path if not exist
            CreateColorCollectionFolder();
            CreatePaletteCollectionFolder();
            CreateResourceCollectionFolder();
            CreateChasingPatternCollectionFolder();
            CreateGifCollectionFolder();
            CreateVIDCollectionFolder();
            CreateMIDCollectionFolder();
            ScreenBitmapCollectionInit();
            CreateAudioDevicesCollection();
            CreateProfileCollectionFolder();
            CreateSupportedDevicesCollectionFolder();
            LoadAvailableDefaultDevices();
            LoadSelectableIcons();
            #endregion
            LoadAvailableBaudRate();
            LoadAvailableProfiles();
            //LoadAvailableLightingProfiles();
            LoadAvailableAutomations();
        }
        private void LoadAvailableAppUser()
        {
            AvailableAppUser = new ObservableCollection<AppUser>();
            string public_User_DisplayName = "Public User";
            string public_User_LoginName = "adrilight_publicuser";
            string public_User_Name = "adrilight_enduser";
            string public_User_PassWord = "@drilightPublic";
            var public_User_Geometry = Geometry.Parse("M340.769 341.93C340.339 377.59 315.859 404.13 280.389 408.04C279.507 408.217 278.64 408.468 277.799 408.79H62.9991C58.3591 407.79 53.6491 407.12 49.0891 405.79C19.9991 397.63 1.54915 375.17 0.269149 344.59C-1.06085 312.99 2.40915 281.86 14.4091 252.27C22.1591 233.18 33.4092 216.65 52.2892 206.57C63.6947 200.41 76.51 197.332 89.4692 197.64C93.5892 197.73 97.9192 199.81 101.679 201.85C107.739 205.15 113.359 209.24 119.239 212.85C153.432 233.956 187.619 233.93 221.799 212.77C226.539 209.83 231.389 207 235.799 203.66C243.979 197.53 252.979 196.76 262.689 198.19C289.209 202.09 307.929 216.61 320.559 239.78C331.869 260.49 336.999 283 339.199 306.1C340.372 318.006 340.896 329.967 340.769 341.93ZM168.139 197C222.309 196.85 266.769 152.16 266.399 98.2998C266.069 43.4698 221.899 -0.400216 167.399 -0.000215969C141.305 0.126814 116.327 10.6011 97.9461 29.1239C79.5657 47.6468 69.2847 72.7051 69.3592 98.7998C69.3892 152.52 113.999 197.16 168.139 197Z");
            var public_User = new AppUser(public_User_DisplayName, public_User_LoginName, public_User_Name, public_User_PassWord, public_User_Geometry);
            string developer_User_DisplayName = "Developer User";
            string developer_User_LoginName = "adrilight_developeruser";
            string developer_User_UserName = "adrilight_developeruser";
            string developer_User_PassWord = "@12345";
            var developer_User_Geometry = Geometry.Parse("M124.8 0C100.115 7.92343e-08 75.9836 7.32023 55.4585 21.035C34.9334 34.7497 18.9362 54.243 9.48993 77.0496C0.0436497 99.8562 -2.42742 124.952 2.38921 149.163C7.20584 173.374 19.0938 195.613 36.5498 213.068C54.0058 230.522 76.2458 242.408 100.457 247.223C124.669 252.038 149.764 249.565 172.57 240.117C195.376 230.668 214.868 214.67 228.581 194.143C242.294 173.617 249.612 149.486 249.61 124.8C249.608 91.7001 236.457 59.9567 213.051 36.5525C189.645 13.1482 157.9 -1.06243e-07 124.8 0ZM82.5003 162C82.283 163.745 81.5704 165.391 80.4468 166.743C79.3232 168.095 77.8357 169.097 76.1603 169.63C74.5014 170.308 72.679 170.48 70.9226 170.124C69.1663 169.768 67.5545 168.9 66.2903 167.63C54.197 155.63 42.167 143.58 30.2003 131.48C28.9703 130.24 28.3903 128.36 27.5103 126.78V122.98C28.3703 120.1 30.3203 117.98 32.4103 115.98C43.5903 104.813 54.7403 93.6433 65.8603 82.47C67.0717 81.1537 68.6333 80.21 70.3619 79.7495C72.0905 79.289 73.9146 79.3308 75.6203 79.87C77.3558 80.3104 78.9249 81.2482 80.1347 82.5682C81.3445 83.8881 82.1424 85.5328 82.4303 87.3C82.7777 88.9512 82.6832 90.6649 82.1565 92.2679C81.6297 93.8709 80.6894 95.3066 79.4303 96.43C70.477 105.357 61.517 114.307 52.5503 123.28C52.0703 123.76 51.6103 124.28 51.0703 124.84C51.6003 125.41 52.0703 125.91 52.5503 126.4C61.4503 135.307 70.3603 144.21 79.2803 153.11C81.7903 155.59 83.0903 158.5 82.5003 162ZM162.69 64.34C158.33 74.1 153.91 83.83 149.53 93.57C134.684 126.563 119.85 159.563 105.03 192.57C103.14 196.77 100.23 199.3 96.5903 199.4C88.3903 199.4 83.9003 192.17 86.8803 185.4C90.9803 176.15 95.1703 166.95 99.3303 157.72C114.404 124.22 129.48 90.7167 144.56 57.21C146.98 51.84 151.22 49.36 156.09 50.46C162.26 51.88 165.37 58.34 162.69 64.35V64.34ZM183.54 170.62C182.644 171.582 181.562 172.352 180.36 172.883C179.158 173.414 177.86 173.696 176.546 173.71C175.231 173.725 173.927 173.473 172.713 172.969C171.499 172.465 170.4 171.72 169.483 170.779C168.565 169.838 167.848 168.72 167.376 167.493C166.903 166.267 166.684 164.957 166.732 163.644C166.781 162.33 167.095 161.04 167.657 159.852C168.219 158.663 169.016 157.601 170 156.73C179.007 147.65 188.054 138.603 197.14 129.59C197.58 129.14 198.14 128.79 198.81 128.22C192.88 122.31 187.23 116.68 181.59 111.04C177.65 107.1 173.67 103.2 169.78 99.2C168.375 97.8044 167.44 96.0063 167.103 94.0549C166.766 92.1036 167.045 90.0958 167.9 88.31C168.707 86.59 169.997 85.1423 171.613 84.1442C173.229 83.146 175.101 82.6407 177 82.69C179.435 82.7522 181.75 83.7559 183.46 85.49C195.39 97.39 207.33 109.26 219.18 121.24C220.121 122.128 220.869 123.2 221.377 124.389C221.886 125.578 222.145 126.859 222.137 128.152C222.13 129.446 221.856 130.724 221.334 131.907C220.811 133.09 220.051 134.153 219.1 135.03C207.307 146.937 195.454 158.803 183.54 170.63V170.62Z");
            var developer_User = new AppUser(developer_User_DisplayName, developer_User_LoginName, developer_User_UserName, developer_User_PassWord, developer_User_Geometry);
            AvailableAppUser.Add(public_User);
            AvailableAppUser.Add(developer_User);
            if (GeneralSettings.CurrentAppUser == null)
            {
                GeneralSettings.CurrentAppUser = AvailableAppUser.First();
            }
        }
        private void LoadSelectableIcons()
        {
            SelectableIcons = new ObservableCollection<string>() {
                 "Shortcut",
                 "Youtube",
                 "Gaming",
                 "Reading",
                 "Still Image",
                 "Boost",
                 "Study",
                 "Fire",
                 "Spotlight",
                 "Eye Open",
                 "Eye Close",
                 "Meeting",
                 "Coding",
                 "Lightbulb",
                 "Switch On",
                 "Switch Off"
            };
        }
        private void LoadAvailableAutomations()
        {
            AvailableAutomations = new ObservableCollection<AutomationSettings>();
            ShutdownAutomations = new ObservableCollection<AutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
                if (automation.Condition != null && automation.Condition is SystemEventTriggerCondition)
                {
                    var condition = automation.Condition as SystemEventTriggerCondition;
                    if (condition.Event == SystemEventEnum.Shutdown)
                    {
                        ShutdownAutomations.Add(automation);
                    }
                }
            }
            WriteAutomationCollectionJson();
        }
        private void CreateResourceCollectionFolder()
        {
            if (!Directory.Exists(ResourceFolderPath))
            {
                Directory.CreateDirectory(ResourceFolderPath);
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".png")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(ResourceFolderPath, name));
                }
            }
        }
        private void CreateProfileCollectionFolder()
        {
            if (!Directory.Exists(ProfileCollectionFolderPath))
                Directory.CreateDirectory(ProfileCollectionFolderPath);
            var collectionFolder = Path.Combine(ProfileCollectionFolderPath, "collection");
            Directory.CreateDirectory(collectionFolder);
            var config = new ResourceLoaderConfig(nameof(AppProfile), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(ProfileCollectionFolderPath, "config.json"), configJson);
        }

        private void CreateAudioDevicesCollection()
        {
            if (AudioVisualizers == null)
            {
                AudioVisualizers = new ObservableCollection<VisualizerDataModel>();
            }
            if (AvailableAudioDevices == null)
            {
                AvailableAudioDevices = new ObservableCollection<AudioDevice>();
            }

            AvailableAudioDevices.Clear();
            AudioVisualizers.Clear();
            foreach (var device in GetAvailableAudioDevices())
            {
                AvailableAudioDevices.Add(device);
                AudioVisualizers.Add(new VisualizerDataModel(32, device.Name));
            }
        }

        public void CreateChasingPatternCollectionFolder()
        {
            if (!Directory.Exists(ChasingPatternsCollectionFolderPath))
            {
                Directory.CreateDirectory(ChasingPatternsCollectionFolderPath);
                var collectionFolder = Path.Combine(ChasingPatternsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".AML")))
                {

                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(ChasingPattern), DeserializeMethodEnum.Files);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(ChasingPatternsCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }

        public void CreateSupportedDevicesCollectionFolder()
        {
            if (!Directory.Exists(SupportedDeviceCollectionFolderPath))
            {
                Directory.CreateDirectory(SupportedDeviceCollectionFolderPath);
                //create generic device
                //var undefinedThumbnailResourcePath = "adrilight.Resources.Thumbnails.Undefined_thumb.png";
                //var genericDeviceDirectory = Path.Combine(SupportedDeviceCollectionFolderPath, "GenericDevice");
                //Directory.CreateDirectory(genericDeviceDirectory);
                //ResourceHlprs.CopyResource(undefinedThumbnailResourcePath, Path.Combine(genericDeviceDirectory, "thumbnail.png"));
            }
        }

        public void CreatePaletteCollectionFolder()
        {
            if (!Directory.Exists(PalettesCollectionFolderPath))
            {
                Directory.CreateDirectory(PalettesCollectionFolderPath);
                var collectionFolder = Path.Combine(PalettesCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var allResourceNames = ResourceHlprs.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".col")))
                {
                    var name = ResourceHlprs.GetResourceFileName(resourceName);
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(ColorPalette), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }


        public List<AppProfile> LoadAppProfileIfExist()
        {
            var existedProfile = new List<AppProfile>();
            string[] files = Directory.GetFiles(Path.Combine(ProfileCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<AppProfile>(jsonData);
                existedProfile.Add(profile);
            }
            return existedProfile;
        }

        public List<AutomationSettings> LoadAutomationIfExist()
        {
            var loadedAutomations = new List<AutomationSettings>();
            if (!File.Exists(JsonAutomationFileNameAndPath))
            {
                ResourceHlprs.CopyResource("adrilight_shared.Resources.Automations.DefaultAutomations.json", JsonAutomationFileNameAndPath);
            }
            if (File.Exists(JsonAutomationFileNameAndPath))
            {
                var json = File.ReadAllText(JsonAutomationFileNameAndPath);

                var existedAutomation = JsonConvert.DeserializeObject<List<AutomationSettings>>(json);
                foreach (var automation in existedAutomation)
                {
                    loadedAutomations.Add(automation);
                }
            }

            return loadedAutomations;
        }

        private void DeleteSelectedDevice()
        {
            AvailableDevices.Remove(CurrentDevice);
            //WriteDeviceInfoJson();
            // AmbinityClient.Dispose();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
        public void WriteAutomationCollectionJson()
        {
            var automations = new List<AutomationSettings>();
            foreach (var automation in AvailableAutomations)
            {
                automations.Add(automation);
            }
            var json = JsonConvert.SerializeObject(automations);
            File.WriteAllText(JsonAutomationFileNameAndPath, json);
        }
        private string _currentSelectedOnlineItemType;

        public string CurrentSelectedOnlineItemType {
            get { return _currentSelectedOnlineItemType; }
            set { _currentSelectedOnlineItemType = value; RaisePropertyChanged(); }
        }

        private OnlineItemModel _currentSelectedOnlineItem;

        public OnlineItemModel CurrentSelectedOnlineItem {
            get { return _currentSelectedOnlineItem; }
            set { _currentSelectedOnlineItem = value; RaisePropertyChanged(); }
        }

        public async Task gotoItemDetails(OnlineItemModel item)
        {
            //CurrentSelectedOnlineItemType = item.GetType().ToString();
            CurrentSelectedOnlineItem = item;
            var screenshotsPath = item.Path + "/screenshots";
            var descriptionPath = item.Path + "/description.md";
            var screenshotsListAddress = await FTPHlprs.GetAllFilesAddressInFolder(screenshotsPath);
            item.Screenshots = new List<BitmapImage>();
            if (screenshotsListAddress == null)
                return;
            foreach (var screenshotAddress in screenshotsListAddress)
            {
                var screenshot = FTPHlprs.GetScreenShot(screenshotAddress).Result;
                item.Screenshots.Add(screenshot);
            }
            var description = await FTPHlprs.GetStringContent(descriptionPath);
            item.MarkDownDescription = description;
            CarouselImageLoading = false;
        }

        public bool IsLiveViewOpen => SelectedViewPart == SelectableViewParts[1];
        private bool _isAppActivated;

        public bool IsAppActivated {
            get { return _isAppActivated; }
            set { _isAppActivated = value; RaisePropertyChanged(); }
        }

        private int _toolBarWidth = 450;

        public int ToolBarWidth {
            get { return _toolBarWidth; }
            set { _toolBarWidth = value; RaisePropertyChanged(); }
        }

        private bool _isInIDEditStage;

        public bool IsInIDEditStage {
            get { return _isInIDEditStage; }

            set
            {
                _isInIDEditStage = value;
                GetItemsForLiveView();
                UpdateLiveView();
                RaisePropertyChanged();
            }
        }

        public enum IDMode
        {
            VID = 0,
            FID = 1
        }

        private IDMode _idEditMode;

        public IDMode IdEditMode {
            get { return _idEditMode; }

            set
            {
                _idEditMode = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<VIDToolBarDataModel> AvailableTools { get; set; }
        private int _selectedToolIndex = 0;

        public int SelectedToolIndex {
            get { return _selectedToolIndex; }

            set
            {
                _selectedToolIndex = value;
                RaisePropertyChanged();
                if (SelectedToolIndex == 0)
                {
                    //selection mode
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Remove(IDEditBrush);
                        if (LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Remove(EraserBrush);
                    }
                }
                else if (SelectedToolIndex == 1)
                {
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Remove(EraserBrush);
                        if (!LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Add(IDEditBrush);
                    }
                }
                else if (SelectedToolIndex == 2)
                {
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Remove(IDEditBrush);
                        if (!LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Add(EraserBrush);
                    }
                }
            }
        }

        private int _brushIntensity = 5;

        public int BrushIntensity {
            get { return _brushIntensity; }

            set
            {
                _brushIntensity = value;
                RaisePropertyChanged();
            }
        }

        private PathGuide _iDEditBrush;

        public PathGuide IDEditBrush {
            get { return _iDEditBrush; }

            set
            {
                _iDEditBrush = value;
                RaisePropertyChanged();
            }
        }

        private PathGuide _eraserBrush;

        public PathGuide EraserBrush {
            get { return _eraserBrush; }

            set
            {
                _eraserBrush = value;
                RaisePropertyChanged();
            }
        }

        private int _brushSize;

        public int BrushSize {
            get { return _brushSize; }

            set
            {
                _brushSize = value;
                if (SelectedToolIndex == 1)
                {
                    IDEditBrush.Width = value;
                    IDEditBrush.Height = value;
                    IDEditBrush.Left = MousePosition.X - value / 2;
                    IDEditBrush.Top = MousePosition.Y - value / 2;
                }
                else if (SelectedToolIndex == 2)
                {
                    EraserBrush.Width = value;
                    EraserBrush.Height = value;
                    EraserBrush.Left = MousePosition.X - value / 2;
                    EraserBrush.Top = MousePosition.Y - value / 2;
                }

                RaisePropertyChanged();
            }
        }

        public void IncreaseBrushSize()
        {
            if (SelectedToolIndex == 1)
            {
                if (IDEditBrush.Width < 1000)
                {
                    IDEditBrush.Width += 5;
                    IDEditBrush.Height += 5;
                    IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                    IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                }
            }
            else if (SelectedToolIndex == 2)
            {
                if (EraserBrush.Width < 1000)
                {
                    EraserBrush.Width += 5;
                    EraserBrush.Height += 5;
                    EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                    EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                }
            }
        }

        public void DecreaseBrushSize()
        {
            if (SelectedToolIndex == 1)
            {
                if (IDEditBrush.Width > 10)
                {
                    IDEditBrush.Width -= 5;
                    IDEditBrush.Height -= 5;
                    IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                    IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                }
            }
            else if (SelectedToolIndex == 2)
            {
                if (EraserBrush.Width < 1000)
                {
                    EraserBrush.Width -= 5;
                    EraserBrush.Height -= 5;
                    EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                    EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                }
            }
        }

        private void GetToolsForIDEdit()
        {
            if (AvailableTools == null)
            {
                AvailableTools = new ObservableCollection<VIDToolBarDataModel>() {
                                 new VIDToolBarDataModel(){Name = "Selection Tool",Geometry= "selectionTool"},
                                 new VIDToolBarDataModel(){Name = "Brush Tool",Geometry= "brushTool"},
                                 new VIDToolBarDataModel(){Name = "Eraser Tool",Geometry= "eraserTool"},
                     };
            }
            //add brush
            SelectedToolIndex = 0;
        }

        private void GetBrushForIDEdit()
        {
            IDEditBrush = new PathGuide() {
                Width = 200,
                Height = 200,
                IsSelectable = false,
                VisualProperties = new VisualProperties() { FillColor = Color.FromRgb(255, 255, 255) },
                Geometry = "genericCircle"
            };
            EraserBrush = new PathGuide() {
                Width = 200,
                Height = 200,
                IsSelectable = false,
                VisualProperties = new VisualProperties() { FillColor = Color.FromRgb(255, 0, 0) },
                Geometry = "genericCircle"
            };
            BrushSize = 200;
            VIDCount = 0;
            _lastBrushX = 0;
            _lastBrushY = 0;
            RegisterBrush(IDEditBrush);
            RegisterBrush(EraserBrush);
        }
        private void DrawVID(IDrawable brush)
        {
            //check if brush has moved more than 10 unit, consider as update interval
            if (CalculateDelta(_lastBrushX, _lastBrushY, brush.Left, brush.Top) > 10)
            {
                _lastBrushX = brush.Left;
                _lastBrushY = brush.Top;
                //check if this position intersect any zone
                int settedVIDCount = 0;

                foreach (var zone in LiveViewItems.Where(z => z is LEDSetup))
                {
                    if (!Rect.Intersect(zone.GetRect, brush.GetRect).IsEmpty)
                    {
                        //this zone is being touch by the brush
                        //check if this brush touch any spot inside this zone
                        var ledSetup = zone as LEDSetup;
                        var intersectRect = Rect.Intersect(ledSetup.GetRect, brush.GetRect);
                        intersectRect.Offset(0 - ledSetup.GetRect.Left, 0 - ledSetup.GetRect.Top);

                        foreach (var spot in ledSetup.Spots)
                        {
                            switch (SelectedToolIndex)
                            {
                                case 1:
                                    if (spot.GetVIDIfNeeded(VIDCount, intersectRect, 0))
                                        settedVIDCount++;
                                    break;

                                case 2:
                                    if (spot.GetVIDIfNeeded(VIDCount, intersectRect, 1))
                                        settedVIDCount++;
                                    break;
                            }
                        }

                    }
                }
                if (settedVIDCount > 0)
                {
                    if (SelectedToolIndex == 1)
                    {
                        VIDCount += BrushIntensity;
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    else if (SelectedToolIndex == 2)
                    {
                        VIDCount -= BrushIntensity;
                        if (VIDCount < 0)
                            VIDCount = 1023;
                    }
                }
            }
        }
        public VIDDataModel CurrentExportingVID { get; set; }
        private string _vidName = "New VID";

        public string VIDName {
            get
            {
                return _vidName;
            }

            set
            {
                _vidName = value;

                RaisePropertyChanged();
            }
        }
        private string _vidDescription;

        public string VIDDescription {
            get
            {
                return _vidDescription;
            }

            set
            {
                _vidDescription = value;

                RaisePropertyChanged();
            }
        }
        private void OpenCreateNewVIDWindow()
        {
            var createNewVIDWindow = new AddNewVIDWindow();
            createNewVIDWindow.Owner = System.Windows.Application.Current.MainWindow;
            createNewVIDWindow.ShowDialog();
        }
        private void SaveVID()
        {
            var isValid = LiveViewItems != null && LiveViewItems.Any(i => i is LEDSetup);
            if (!isValid)
                return;
            CurrentExportingVID = new VIDDataModel();
            CurrentExportingVID.Name = VIDName;
            CurrentExportingVID.Description = VIDDescription;
            CurrentExportingVID.ExecutionType = VIDType.PredefinedID;
            CurrentExportingVID.Geometry = "record";
            //get drawing path
            //sort current drawing board spot by ID
            //foreach spot, save ID and rect
            var liveViewSpots = new List<IDeviceSpot>();
            foreach (var zone in LiveViewItems.Where(z => z is LEDSetup))
            {
                (zone as LEDSetup).Spots.ToList().ForEach(s =>
                {
                    (s as DeviceSpot).OffsetX = zone.GetRect.Left;
                    (s as DeviceSpot).OffsetY = zone.GetRect.Top;
                    liveViewSpots.Add(s);

                });
            }
            var reorderdLiveViewSpots = liveViewSpots.OrderBy(s => s.VID).ToList();
            CurrentExportingVID.DrawingPath = new BrushData[reorderdLiveViewSpots.Count];
            var spotCount = 0;
            foreach (DeviceSpot spot in reorderdLiveViewSpots)
            {
                var spotAbsoluteLocation = new Rect(spot.GetRect.Left, spot.GetRect.Top, spot.GetRect.Width, spot.GetRect.Height);
                spotAbsoluteLocation.Offset(spot.OffsetX, spot.OffsetY);
                CurrentExportingVID.DrawingPath[spotCount++] = new BrushData(spot.VID, spotAbsoluteLocation);
            }
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.VID).FirstOrDefault() as ListSelectionParameter;
            if (currentParam == null)
                return;
            currentParam.AddItemToCollection(CurrentExportingVID);
        }
        private void RegisterBrush(IDrawable brush)
        {
            brush.PropertyChanged += async (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(brush.Left):
                    case nameof(brush.Top):
                        if (Keyboard.IsKeyDown(Key.B))
                        {
                            await Task.Run(() => DrawVID(brush));
                        }

                        break;
                }
            };
        }

        private bool _isInIsolateMode;

        public bool IsInIsolateMode {
            get { return _isInIsolateMode; }

            set
            {
                _isInIsolateMode = value;
                RaisePropertyChanged();
            }
        }

        private void IsolateSelectedItems()
        {
            var selectedItems = LiveViewItems.Where(i => i.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải chọn một vùng LED hoặc Group trước", "No item selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IsInIsolateMode = true;
            LiveViewItems.Clear();
            foreach (var item in selectedItems)
            {
                if (item is LEDSetup)
                {
                    LiveViewItems.Add(item);
                }
                else if (item is Border)
                {
                    var group = CurrentDevice.ControlZoneGroups.Where(g => g.Border.Name == item.Name).FirstOrDefault();
                    foreach (var zone in group.ControlZones)
                    {
                        if (zone is LEDSetup)
                            LiveViewItems.Add(zone as LEDSetup);
                        else if (zone is FanMotor)
                            LiveViewItems.Add(zone as FanMotor);
                    }
                    LiveViewItems.Add(item);
                }
                else if (item is FanMotor)
                {
                    LiveViewItems.Add(item);
                }
            }
            if (IsInIDEditStage)
            {
                GetBrushForIDEdit();
            }
            SelectLiveViewItemCommand.Execute(selectedItems.First());
        }

        public void GotoChild(IDeviceSettings selectedDevice)
        {
            SelectedViewPart = SelectableViewParts[1];
            Log.Information("Navigating to Device Control");
            CurrentDevice = selectedDevice;
            GetItemsForLiveView();
            UpdateLiveView();
        }

        public IList<ISelectableViewPart> SelectableViewParts { get; }
        public ISelectableViewPart _selectedViewPart;

        public ISelectableViewPart SelectedViewPart {
            get => _selectedViewPart;

            set
            {
                Set(ref _selectedViewPart, value);
            }
        }

        public void BackToDashboard()
        {
            SelectedViewPart = SelectableViewParts[0];
            Log.Information("Navigating to Dashboard");
        }

        public void BackToCollectionView()
        {
            if (_lastView == "Home")
            {
                CurrentOnlineStoreView = "Home";
                IsInCarouselList = false;
            }
            else
            {
                CurrentOnlineStoreView = "Collections";
                Log.Information("Navigating to Store Collection View");
            }

        }

        /// <summary>
        /// Load vertical menu
        /// </summary>

        public void LoadContextMenu()
        {
            AvailableContextMenus = new ObservableCollection<SystemTrayContextMenu>();
            //exit button
            var exit = new SystemTrayContextMenu() {
                Header = "Exit",
                Geometry = "exit",
                Action = ExitCurrentRunningAppCommand
            };
            var dashboard = new SystemTrayContextMenu() {
                Header = "Dashboard",
                Geometry = "dashboard",
                Action = ExitCurrentRunningAppCommand,
                SubMenus = new ObservableCollection<SystemTrayContextMenu>() {
                    exit,exit
                }
            };
            AvailableContextMenus.Add(exit);
            AvailableContextMenus.Add(dashboard);
        }
    }
}