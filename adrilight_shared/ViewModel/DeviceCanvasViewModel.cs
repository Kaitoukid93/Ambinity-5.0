using adrilight_shared.Models.Drawable;
using adrilight_shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.Stores;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Device;

namespace adrilight_shared.ViewModel
{
    /// <summary>
    /// this is representing a rich canvas
    /// contains interaction logic with the UI
    /// </summary>
    public class DeviceCanvasViewModel : ViewModelBase, IDisposable
    {
        #region Construct

        public DeviceCanvasViewModel(DeviceCanvas canvas, DeviceControlEvent controlEvent)
        {
            _deviceControlEvent = controlEvent;
            Canvas = canvas;
            Init();
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
        private bool _enableMultipleItemSelection;
        private CanvasSelectionRectangle _selectionRectangle;
        private double _canvasWidth;
        private double _canvasHeight;

        public DeviceCanvas Canvas { get; set; }
        //public//
        public DeviceControlEvent _deviceControlEvent;
        #endregion

        #region Methods

        /// <summary>
        /// setup all commands
        /// </summary>
        private void CommandSetup()
        {
            SelectItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                //if (Canvas.ChangeSelectedItem(p))
                //    _deviceControlEvent.ChangeSelectedItem(p);
                //if (Keyboard.IsKeyDown(Key.LeftShift))
                //{
                //    p.IsSelected = false;
                //}
                _selectedItem = p;

            });
            RichCanvasMouseButtonUpCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (_selectedItem != null && _selectedItem.GetRect.Contains(Canvas.MousePosition))
                {
                    if (Canvas.ChangeSelectedItem(_selectedItem))
                        _deviceControlEvent.ChangeSelectedItem(_selectedItem);
                }
                Canvas.ToolInit();

            });
            RichCanvasMouseButtonDownCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift)) // user is draging or holding ctrl
                {

                    if (Canvas.UnselectAllCanvasItem())
                        _deviceControlEvent.UnSelectAllItem();
                }
               
            });
            LayerViewMouseButtonDownCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p.IsSelected)
                    return;
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {

                    if (Canvas.UnselectAllCanvasItem())
                        _deviceControlEvent.UnSelectAllItem();
                    if (Canvas.ChangeSelectedItem(p))
                        _deviceControlEvent.ChangeSelectedItem(p);
                }
                else
                {
                    p.IsSelected = true;
                }
                Canvas.ToolInit();

            });
            CanvasItemToolCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                switch (p)
                {
                    case "ungroup":
                        Canvas.UngroupSelectedItems(true);
                        break;
                    case "group":
                        await Canvas.GroupSelectedItems();
                        break;
                    case "isolate":
                        Canvas.IsolateSelectedItems();
                        break;
                    case "showall":
                        Canvas.UpdateView();
                        break;
                    case "highlight":
                        Canvas.HighlightSelectedItems();
                        break;
                }
                Canvas.UpdateLayers();
                Canvas.ToolInit();
            });
        }

        private void Init()
        {
            CommandSetup();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public void AddItem(IDrawable item)
        {
            Canvas.Items.Add(item);
        }
        public void ClearAll()
        {
            Canvas.Items.Clear();
        }
        #endregion


        #region Commands
        public ICommand SelectItemCommand { get; set; }
        public ICommand RichCanvasMouseButtonUpCommand { get; set; }
        public ICommand RichCanvasMouseButtonDownCommand { get; set; }
        public ICommand CanvasItemToolCommand { get; set; }
        public ICommand LayerViewMouseButtonDownCommand { get; set; }
        #endregion
    }
}
