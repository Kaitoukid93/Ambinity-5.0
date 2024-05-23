using adrilight_shared.Helpers;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Windows.Controls;
using System.Xml.Linq;
using HandyControl.Tools.Extension;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.ItemsCollection;

namespace adrilight_shared.Models.Drawable
{
    public class DeviceCanvas : ViewModelBase
    {
        #region Construct

        public DeviceCanvas(DeviceControlEvent controlEvent)
        {
            Items = new ObservableCollection<IDrawable>();
            Layers = new ObservableCollection<IDrawable>();
            _deviceControlEvent = controlEvent;
            _drawableHelpers = new DrawableHelpers();
            SelectionRectangle = new CanvasSelectionRectangle() { FillColor = Color.FromArgb(64, 255, 255, 255) };
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            EnableMultipleItemSelection = true;
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
        private ObservableCollection<CollectionItemTool> _availableTools;
        private bool _isIsolated;
        private bool _isHiglighted;

        /// <summary>
        /// helpers
        /// </summary>
        private DrawableHelpers _drawableHelpers;


        //public//
        public bool IsIsolated
        {
            get { return _isIsolated; }

            set
            {
                _isIsolated = value;
                RaisePropertyChanged();
            }
        }
        public bool IsHighlighted
        {
            get { return _isHiglighted; }

            set
            {
                _isHiglighted = value;
                RaisePropertyChanged();
            }
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
            get
            {
                return _canvasScale;
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
            get
            {
                return _canvasOffset;
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
        public ObservableCollection<IDrawable> Layers { get; set; }

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

        public ObservableCollection<CollectionItemTool> AvailableTools
        {
            get
            {
                return _availableTools;
            }
            set
            {
                _availableTools = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Methods

        /// <summary>
        /// setup all commands
        /// </summary>

        private void CalculateItemsBorderThickness(double scaleValue) // this keeps items border remain the same 2px anytime
        {
            if (SelectionRectangle == null) return;
            SelectionRectangle.StrokeThickness = 3 / scaleValue;
            CanvasItemBorder = new Thickness(SelectionRectangle.StrokeThickness);
        }
        public void UpdateView()
        {
            if (Items == null || Items.Count() == 0)
                return;
            foreach (var item in Items)
            {
                item.IsVisible = true;
            }
            IsIsolated = false;
            IsHighlighted = false;
            ToolInit();
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
        public void BringIntoView(Rect area)
        {
            if (area == null) return;
            var widthScale = (CanvasWidth - 50) / area.Width;
            var scaleHeight = (CanvasHeight - 50) / area.Height;
            CanvasScale = Math.Min(widthScale, scaleHeight);
            var currentWidth = area.Width * CanvasScale;
            var currentHeight = area.Height * CanvasScale;
            CanvasOffset = new Point(0 - area.Left * CanvasScale + (CanvasWidth - currentWidth) / 2,
                0 - area.Top * CanvasScale + (CanvasHeight - currentHeight) / 2);
            Log.Information("Live View Updated");
        }
        public void HighlightSelectedItems()
        {
            //remove all unnecessary item but selected
            var selectedItems = Items.Where(i => i.IsSelected).ToList();
            if (selectedItems.Count == 0) return;
            // selectedItems Boundingbox
            foreach (var item in Items)
            {
                if (!item.IsSelected)
                {
                    item.IsVisible = false;
                }
                if (item is Border)
                {
                    var border = item as Border;
                    if (item.IsSelected)
                        foreach (var child in GetGroupItems(border))
                            child.IsVisible = true;

                }

            }
            var visibleItems = Items.Where(i => i.IsVisible).ToList();
            var focusArea = _drawableHelpers.GetRealBound(visibleItems.ToArray());
            BringIntoView(focusArea);
            IsHighlighted = true;
            ToolInit();

        }
        public List<IDrawable> GetGroupItems(Border border)
        {
            var items = new List<IDrawable>();
            foreach (var item in Items)
            {
                if (item is not LEDSetup)
                    continue;
                var zone = item as LEDSetup;
                if (border.GetRect.Contains(zone.GetRect) && zone.GroupID == border.GroupID)
                    items.Add(item);
            }
            return items;
        }

        public void IsolateSelectedItems()
        {
            var selectedItems = Items.Where(i => i.IsSelected).ToList();
            if (selectedItems.Count == 0)
                return;
            // selectedItems Boundingbox
            //var selectedArea = _drawableHelpers.GetRealBound(selectedItems.ToArray());
            //foreach (var item in Items)
            //{
            //    if (Rect.Intersect(item.GetRect, selectedArea) == Rect.Empty)
            //    {
            //        item.IsVisible = false;
            //    }
            //    else
            //    {
            //        if(!item.IsSelected)
            //            item.IsVisible = false;
            //    }
            //}
            //hide all item first
            foreach (var item in Items)
            {
                item.IsVisible = false;
            }
            foreach (var sitem in selectedItems)
            {
                //set everything related to selected item area to visible
                foreach (var item in Items)
                {
                    if (Rect.Intersect(item.GetRect, sitem.GetRect) != Rect.Empty)
                        item.IsVisible = true;
                }
            }
            var visibleItems = Items.Where(i => i.IsVisible && i.IsSelectable).ToList();
            var focusArea = _drawableHelpers.GetRealBound(visibleItems.ToArray());
            BringIntoView(focusArea);
            IsIsolated = true;
            ToolInit();

        }
        public void SelectFirstSelectableItem()
        {
            var firstItem = Items.Where(i => i.IsSelectable).First();
            firstItem.IsSelected = true;
            _deviceControlEvent.ChangeSelectedItem(firstItem);
            ToolInit();
        }
        public bool ChangeSelectedItem(IDrawable item)
        {
            if (item.IsSelectable)
            {
                item.IsSelected = true;
                return true;
            }
            return false;
        }
        public bool UnselectAllCanvasItem()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
            if (!Items.Any(i => i.GetRect.Contains(MousePosition)))
            {
                //show nullable view
                return true;
            }
            return false;
        }

        //return list of items after remove their group, they become free
        public List<IDrawable> UngroupSelectedItems(bool notify)
        {
            //turn on Isregistering group
            //remove border from canvas
            var freeItems = new List<IDrawable>();
            var borderToRemove = Items.Where(p => p.IsSelected && p is Border).ToList();
            if (borderToRemove == null)
                return null;
            borderToRemove.ForEach(b => Items.Remove(b));
            //free items in group
            foreach (var item in borderToRemove)
            {
                var border = item as Border;
                if (border == null)
                    continue;
                freeItems.AddRange(GetGroupItems(border));
            }
            if (notify)
                _deviceControlEvent.UngroupSelectedItem(borderToRemove);
            return freeItems;
        }
        //group selected items on canvas include existed group
        public async Task GroupSelectedItems()
        {
            //free grouped items
            var ungroupedItems = UngroupSelectedItems(false);
            var selectedItems = Items.Where(z => z.IsSelected && z.IsVisible && z is not ARGBLEDSlaveDevice).ToList();
            if (ungroupedItems != null)
            {
                ungroupedItems.ForEach(i => selectedItems.Add(i));
            }
            if (selectedItems.Count > 1)
            {
                var groupCount = Items.Where(p => p is Border).ToList().Count;
                var newGroupName = "Group" + " " + (groupCount + 1).ToString();
                var newGroup = new ControlZoneGroup(newGroupName);
                await newGroup.AddZonesToGroup(selectedItems);
                Items.Add(newGroup.Border);
                //rearrange group based on size
                _deviceControlEvent.GroupSelectedItem(newGroup);
            }

        }
        private CollectionItemTool UngroupTool()
        {
            return new CollectionItemTool()
            {
                Name = "Ungroup",
                ToolTip = "Ungroup Selected Item",
                Geometry = "canvasUngrop",
                CommandParameter = "ungroup"

            };
        }
        private CollectionItemTool GroupTool()
        {
            return new CollectionItemTool()
            {
                Name = "Group",
                ToolTip = "Group Selected Items",
                Geometry = "canvasGroup",
                CommandParameter = "group"

            };
        }
        private CollectionItemTool IsolateTool()
        {
            return new CollectionItemTool()
            {
                Name = "Isolate",
                ToolTip = "Isolate Selected Items",
                Geometry = "canvasIsolate",
                CommandParameter = "isolate"

            };
        }
        private CollectionItemTool HighlightTool()
        {
            return new CollectionItemTool()
            {
                Name = "Highlight",
                ToolTip = "Highlight Selected Items",
                Geometry = "canvasIsolate",
                CommandParameter = "highlight"

            };
        }
        private CollectionItemTool UnIsolateTool()
        {
            return new CollectionItemTool()
            {
                Name = "Show All",
                ToolTip = "Show All Item",
                Geometry = "canvasIsolate",
                CommandParameter = "showall"

            };
        }
        public void ToolInit()
        {
            AvailableTools.Clear();
            var selectedItems = Items.Where(p => p.IsSelected && p.IsVisible).ToList();
            var selectedItemsCount = selectedItems.Count;
            if (selectedItemsCount > 1)
            {
                //available tools = isolate + group
                AvailableTools = new ObservableCollection<CollectionItemTool> { GroupTool(), IsolateTool() };

            }
            if (selectedItemsCount == 1 && selectedItems.Where(p => p.IsSelected).First() is Border)
            {
                // available tools = isolate + ungroup
                AvailableTools = new ObservableCollection<CollectionItemTool> { UngroupTool(), IsolateTool() };
            }
            if (selectedItemsCount == 1 && selectedItems.Where(p => p.IsSelected).First() is IControlZone)
            {
                AvailableTools = new ObservableCollection<CollectionItemTool> { IsolateTool() };
            }
            if (IsIsolated || IsHighlighted)
            {
                AvailableTools.Add(UnIsolateTool());
            }
            if (selectedItemsCount >= 1 && !IsHighlighted)
            {
                AvailableTools.Add(HighlightTool());
            }
        }
        public void UpdateLayers()
        {
            Layers?.Clear();
            foreach (var item in Items)
            {
                if (item.IsSelectable && item.IsVisible)
                {
                    Layers.Add(item);
                }
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
