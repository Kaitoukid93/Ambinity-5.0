using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Services;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static adrilight.ViewModel.MainViewViewModel;
using adrilight_shared.Models.RelayCommand;

namespace adrilight.ViewModel
{
    public class DeviceIDEditViewModel : ViewModelBase
    {
        #region Construct
        public DeviceIDEditViewModel(DialogService dialogService,
            IDeviceSettings currentDevice,
            ObservableCollection<IDeviceSettings> availableDevices,
            int idMode)
        {
            _dialogService = dialogService;
            _currentDevice = currentDevice;
            _availableDevices = availableDevices;
            CommandSetup();


        }
        #endregion



        #region Properties
        private bool _isInIsolateMode;
        private ObservableCollection<IDrawable> _liveViewItems;
        private DialogService _dialogService;
        private IDeviceSettings _currentDevice;
        private ObservableCollection<IDeviceSettings> _availableDevices;
        private int _vidCount;
        private double _lastBrushX;
        private double _lastBrushY;
        private int _brushIntensity = 5;
        private PathGuide _iDEditBrush;
        private int _brushSize;
        private PathGuide _eraserBrush;
        private int _selectedToolIndex = 0;
        private Point _mousePosition;
        private int _idMode; // indicate that current mode is either music or color palette
        
        public ObservableCollection<IDrawable> LiveViewItems {
            get { return _liveViewItems; }

            set
            {
                _liveViewItems = value;
                RaisePropertyChanged();
            }
        }
        public int VIDCount {
            get { return _vidCount; }

            set
            {
                _vidCount = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<VIDToolBarDataModel> AvailableTools { get; set; }
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
        public int BrushIntensity {
            get { return _brushIntensity; }

            set
            {
                _brushIntensity = value;
                RaisePropertyChanged();
            }
        }
        public PathGuide IDEditBrush {
            get { return _iDEditBrush; }

            set
            {
                _iDEditBrush = value;
                RaisePropertyChanged();
            }
        }
        public PathGuide EraserBrush {
            get { return _eraserBrush; }

            set
            {
                _eraserBrush = value;
                RaisePropertyChanged();
            }
        }
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
        public Point MousePosition {
            get { return _mousePosition; }

            set
            {
                _mousePosition = value; RaisePropertyChanged();

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
        public VIDDataModel CurrentExportingVID { get; set; }
        public string VIDName { get; set; }
        public string VIDDescription { get; set; }
      
        public bool IsInIsolateMode {
            get { return _isInIsolateMode; }

            set
            {
                _isInIsolateMode = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region Methods

        /// <summary>
        /// setup all commands
        /// </summary>
        private void CommandSetup()
        {
            ShowAllDeviceButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                // Get all available devices and add to canvas

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
            CreateNewVIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveVID();
            }
 );
        }

        private void Init()
        {
            // add spots to live view to handle drag drop
            // how to extract spots from live view items?
            var spots = new ObservableCollection<DeviceSpot>();
            foreach (var item in LiveViewItems.Where(i => i is LEDSetup))
            {
                var ledSetup = item as LEDSetup;
                foreach (var spot in ledSetup.Spots)
                {
                    spots.Add(spot as DeviceSpot);
                }
            }
            switch (_idMode)
            {
                case 0:
                    GetToolsForIDEdit();
                    GetBrushForIDEdit();
                    break;

                case 1:
                    //get audio device graph or show all device graph
                    //var _audioDeviceSelectionControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p is AudioDeviceSelectionButtonParameter).FirstOrDefault() as AudioDeviceSelectionButtonParameter;
                    //CurrentVisualizer = AudioVisualizers[_audioDeviceSelectionControl.CapturingSourceIndex];
                    break;
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

        private Double CalculateDelta(double lastX, double lastY, double currentX, double currentY)
        {
            var deltaX = currentX - lastX;
            var deltaY = currentY - lastY;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
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
            //set current parram active vid as this
           // var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.VID).FirstOrDefault() as ListSelectionParameter;
          //  if (currentParam == null)
          //      return;
          //  currentParam.AddItemToCollection(CurrentExportingVID);
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
                //else if (item is Border)
                //{
                //    var group = CurrentDevice.ControlZoneGroups.Where(g => g.Border.Name == item.Name).FirstOrDefault();
                //    foreach (var zone in group.ControlZones)
                //    {
                //        if (zone is LEDSetup)
                //            LiveViewItems.Add(zone as LEDSetup);
                //        else if (zone is FanMotor)
                //            LiveViewItems.Add(zone as FanMotor);
                //    }
                //    LiveViewItems.Add(item);
                //}
                else if (item is FanMotor)
                {
                    LiveViewItems.Add(item);
                }
            }
            //SelectLiveViewItemCommand.Execute(selectedItems.First());
        }
        #endregion






        #region Commands
        public ICommand ShowAllDeviceButtonCommand { get; set; }
        public ICommand ResetAllItemsIDCommand { get; set; }
        public ICommand CreateNewVIDCommand { get; set; }
        #endregion

    }
}
