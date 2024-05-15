﻿using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.Stores;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace adrilight.ViewModel
{
    public class DeviceControlViewModel : ViewModelBase
    {
        #region Construct
        public DeviceControlViewModel(DeviceCanvasViewModel canvasViewModel,
            EffectControlViewModel effectControlViewModel,
            VerticalMenuControlViewModel verticalMenu,
            DeviceControlEvent controlEvent)
        {
            DrawableHlprs = new DrawableHelpers();
            EffectControl = effectControlViewModel;
            CanvasViewModel = canvasViewModel;
            VerticalMenu = verticalMenu;
            _deviceControlEvent = controlEvent;
            EffectControl.DeviceControlEvent = _deviceControlEvent;
            VerticalMenu.DeviceControlEvent = _deviceControlEvent;
            _deviceControlEvent.SelectedItemChanged += OnSelectedCanvasItemChanged;
            _deviceControlEvent.SelectedVerticalMenuIndexChanged += OnSelectedMenuIndexChanged;
            _deviceControlEvent.UnselectAllItemEvent += OnCanvasUnSelectionChanged;
            _deviceControlEvent.SelectedItemUngrouped += OnCanvasUngrouped;
            _deviceControlEvent.SelectedItemGrouped += OnCanvasGrouped;
        }
        #endregion


        #region Properties
        private EffectControlViewModel _effectControl;
        public IDeviceSettings Device { get; set; }
        public DeviceCanvasViewModel CanvasViewModel { get; set; }
        public VerticalMenuControlViewModel VerticalMenu { get; set; }
        public EffectControlViewModel EffectControl {
            get
            {
                return _effectControl;
            }
            set
            {
                _effectControl = value;
                RaisePropertyChanged();
            }
        }
        private DrawableHelpers DrawableHlprs { get; set; }

        private DeviceControlEvent _deviceControlEvent;
        #endregion


        #region Events
        // selected Live View Item Changed
        // selected Live View Items Changed
        private void OnSelectedMenuIndexChanged(int index)
        {
            //update view
            Device.CurrentActiveControlerIndex = index;
            Init();
        }
        private void OnCanvasUnSelectionChanged()
        {
            //show nullable view
            EffectControl.ControlItem = null;
        }
        private void OnCanvasUngrouped(List<IDrawable> items)
        {
            foreach (var item in items)
            {
                //find which group
                var currentGroup = Device.ControlZoneGroups.Where(g => g.Border != null && Rect.Equals(g.Border.GetRect,item.GetRect) && g.Type == Device.CurrentActiveController.Type).FirstOrDefault();
                //set each zone selectable
                foreach (var zone in Device.CurrentLiveViewZones)
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
                Device.ControlZoneGroups.Remove(currentGroup);
            }
        }
        private void OnCanvasGrouped(ControlZoneGroup newGroup)
        {
            Device.ControlZoneGroups.Add(newGroup);
            OnSelectedCanvasItemChanged(newGroup.Border);
        }
        private void OnSelectedCanvasItemChanged(IDrawable item)
        {
            //load item effect control if available
            object controlItem;
            if(!item.IsSelected)
            {
                item.IsSelected = true;
            }
            if (item is Border)
            {
                var border = (Border)item;
                var group = Device.ControlZoneGroups.Where(g => g.Border != null && g.GroupUID == border.GroupID && g.Type == Device.CurrentActiveController.Type).FirstOrDefault();
                controlItem = group;
            }
            else
            {
                controlItem = item;
            }
            EffectControl.ControlItem = controlItem;


        }

        #endregion

        #region Methods 
        private void CommandSetup()
        {

        }
        public void LoadVerticalMenuItem()
        {
            VerticalMenu.Items.Clear();
            foreach (var item in Device.AvailableControllers)
            {
                VerticalMenu.Items.Add(item as DeviceController);
            }
            if (VerticalMenu.Items.Count > 0)
                VerticalMenu.SelectedIndex = 0;
        }
        public void Init()
        {
            if (Device == null)
                return;
            if (CanvasViewModel.Canvas.Items == null)
            {
                CanvasViewModel.Canvas.Items = new System.Collections.ObjectModel.ObservableCollection<IDrawable>();
            }
            CanvasViewModel.Canvas.Items.Clear();
            foreach (var item in Device.CurrentLiveViewZones)
            {
                if (!item.IsInControlGroup)
                    (item as IDrawable).IsSelectable = true;
                CanvasViewModel.Canvas.Items.Add(item as IDrawable);
            }
            //add all zone in group
            if (Device.ControlZoneGroups != null)
            {
                var groupList = new List<ControlZoneGroup>();
                var obsoleteGroupList = new List<ControlZoneGroup>();
                foreach (var group in Device.CurrentLiveViewGroup)
                {
                    group.Init(Device);
                    group.GetGroupBorder();
                    if (group.Border != null)
                    {
                        groupList.Add(group);
                    }
                    else
                    {
                        obsoleteGroupList.Add(group);
                    }
                }
                obsoleteGroupList.ForEach(g => Device.ControlZoneGroups.Remove(g));
                var orderedGroups = groupList.OrderBy(o => o.Border.Width * o.Border.Height).ToList();
                orderedGroups.Reverse();
                foreach (var group in orderedGroups)
                {
                    CanvasViewModel.Canvas.Items.Add(group.Border);
                }
                foreach (var output in Device.AvailableLightingOutputs.Where(o => o.IsEnabled))
                {
                    var lightingDevice = output.SlaveDevice as ARGBLEDSlaveDevice;
                    lightingDevice.IsSelectable = false;

                    CanvasViewModel.Canvas.Items.Insert(0, lightingDevice);
                }
            }
            CanvasViewModel.Canvas.UpdateLayers();
            CanvasViewModel.Canvas.SelectFirstSelectableItem();
            UpdateView();
        }
        public void UpdateView()
        {
            // this called when items change or group change to resize the canvas accordingly
            if (CanvasViewModel == null)
                return;
            CanvasViewModel.Canvas.UpdateView();

        }
        public void Reset() // reset all items behavior
        {
            CanvasViewModel.Canvas.UnselectAllCanvasItem();
        }
        #endregion


        #region Commands

        #endregion
    }
}