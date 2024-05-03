using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Helpers;
using Serilog;
using adrilight_shared.Models.Stores;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.View.Canvas;

namespace adrilight_shared.ViewModel
{
    /// <summary>
    /// this is representing a rich canvas
    /// contains interaction logic with the UI
    /// </summary>
    public class DeviceCanvasViewModel : ViewModelBase, IDisposable
    {
        #region Construct
        public DeviceCanvasViewModel()
        {
            Items = new ObservableCollection<IDrawable>();
            _drawableHelpers = new DrawableHelpers();
            Init();
        }
        ~DeviceCanvasViewModel()
        {

        }
        #endregion


        #region Properties
        private ObservableCollection<IDrawable> _items;
        private IDrawable _selectedItem;
        private DialogService _dialogService;
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
        private double _canvasScale = 1.0;
        private Point _canvasOffset = new Point(0, 0);
        private Thickness _canvasItemBorder = new Thickness(2.0);
        private DeviceControlEvent _deviceControlEvent;
        private bool _enableMultipleItemSelection;
        private CanvasSelectionRectangle _selectionRectangle;
        private double _canvasWidth;
        private double _canvasHeight;

        /// <summary>
        /// helpers
        /// </summary>
        private DrawableHelpers _drawableHelpers;


        //public//
        public DeviceControlEvent DeviceControlEvent
        {
            get { return _deviceControlEvent; }
            set { _deviceControlEvent = value; }
        }
        public bool EnableMultipleItemSelection
        {
            get { return _enableMultipleItemSelection; }

            set
            {
                _enableMultipleItemSelection = value;
                RaisePropertyChanged();
            }
        }
        public double CanvasScale
        {
            get { return _canvasScale;
            }

            set
            {
                _canvasScale = value;
                CalculateItemsBorderThickness(value);
                RaisePropertyChanged();
            }
        }
        public Point CanvasOffset
        {
            get { return _canvasOffset;
            }

            set
            {
                _canvasOffset = value;
                RaisePropertyChanged();
            }
        }
        public double CanvasWidth
        {
            get { return _canvasWidth; }
            set { _canvasWidth = value; RaisePropertyChanged(); if (value > 0) UpdateView(); }
        }
        public double CanvasHeight
        {
            get { return _canvasHeight; }
            set { _canvasHeight = value; RaisePropertyChanged(); if (value > 0) UpdateView(); }
        }
        public Thickness CanvasItemBorder
        {
            get { return _canvasItemBorder; }

            set
            {
                _canvasItemBorder = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<IDrawable> Items
        {
            get { return _items; }

            set
            {
                _items = value;
                RaisePropertyChanged();
            }
        }
        public IDrawable SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                _selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public Point MousePosition
        {
            get { return _mousePosition; }

            set
            {
                _mousePosition = value; RaisePropertyChanged();
            }
        }
        public CanvasSelectionRectangle SelectionRectangle
        {
            get { return _selectionRectangle; }

            set
            {
                _selectionRectangle = value; RaisePropertyChanged();
            }
        }


        #endregion


        #region Methods

        /// <summary>
        /// setup all commands
        /// </summary>
        private void CommandSetup()
        {
            ExitIsolateModeButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                // Get all available devices and add to canvas

            });
            SelectItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                SelectedItemChanged(p);
            });
            RichCanvasMouseButtonUpCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (Items.Where(p => p.IsSelected).Count() > 0)
                    //ShowSelectedItemToolbar = true;
                    //CanUnGroup = false;
                    // CanGroup = true;

                    if (Items.Any(p => p.IsSelected && p is Border))
                    {
                        // CanUnGroup = true;
                    }

            });
            RichCanvasMouseButtonDownCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                        if (item is ARGBLEDSlaveDevice)
                        {
                            var dev = item as ARGBLEDSlaveDevice;
                            dev.OnIsSelectedChanged(true, false);
                        }
                    }
                }
            });
            UnGroupSelectedItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UngroupSelectedItems();
            });
            GroupSelectedItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GroupSelectedItems();
            });
        }

        private void Init()
        {
            CommandSetup();
        }
        private void CalculateItemsBorderThickness(double scaleValue) // this keeps items border remain the same 2px anytime
        {
            if (SelectionRectangle == null) return;
            SelectionRectangle.StrokeThickness = 2 / scaleValue;
            CanvasItemBorder = new Thickness(SelectionRectangle.StrokeThickness);
        }
        public void UpdateView()
        {
            var liveViewItemsBound = _drawableHelpers.GetRealBound(Items.Where(i => i is not PathGuide).ToArray());
            var widthScale = (CanvasWidth - 50) / liveViewItemsBound.Width;
            var scaleHeight = (CanvasHeight - 50) / liveViewItemsBound.Height;
            CanvasScale = Math.Min(widthScale, scaleHeight);
            var currentWidth = liveViewItemsBound.Width * CanvasScale;
            var currentHeight = liveViewItemsBound.Height * CanvasScale;
            //set current device offset
            CanvasOffset = new Point(0 - liveViewItemsBound.Left * CanvasScale + (CanvasWidth - currentWidth) / 2,
                0 - liveViewItemsBound.Top * CanvasScale + (CanvasHeight - currentHeight) / 2);
            Log.Information("Live View Updated");
        }
        public void IsolateSelectedItems()
        {
            var selectedItems = Items.Where(i => i.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải chọn một vùng LED hoặc Group trước", "No item selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Items.Clear();
            foreach (var item in selectedItems)
            {
                if (item is LEDSetup)
                {
                    Items.Add(item);
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
                    Items.Add(item);
                }
            }
            //SelectLiveViewItemCommand.Execute(selectedItems.First());
        }
        private void SelectedItemChanged(IDrawable item)
        {
            if (item.IsSelectable)
            {
                _deviceControlEvent.ChangeSelectedItem(item);
            }
        }
        private void UngroupSelectedItems()
        {
            //turn on Isregistering group
            //remove border from canvas
            var borderToRemove = Items.Where(p => p.IsSelected && p is Border).ToList();
            if (borderToRemove == null)
                return;
            borderToRemove.ForEach(b => Items.Remove(b));
            //free items in group
            _deviceControlEvent.UngroupSelectedItem(borderToRemove);
        }
        private void GroupSelectedItems()
        {
            //get selected free items
            var selectedItems = Items.Where(z => z.IsSelected && z is not ARGBLEDSlaveDevice).ToList();
            if (selectedItems != null && selectedItems.Count > 1)
            {
                //notify LiveViewViewModel to make new group
                _deviceControlEvent.GroupSelectedItem(selectedItems);
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Commands
        public ICommand ExitIsolateModeButtonCommand { get; set; }
        public ICommand IsolateSelectedItemCommand { get; set; }
        public ICommand GroupSelectedItemsCommand { get; set; }
        public ICommand UnGroupSelectedItemsCommand { get; set; }
        public ICommand SelectItemCommand { get; set; }
        public ICommand RichCanvasMouseButtonUpCommand { get; set; }
        public ICommand RichCanvasMouseButtonDownCommand { get; set; }
        #endregion
    }
}
