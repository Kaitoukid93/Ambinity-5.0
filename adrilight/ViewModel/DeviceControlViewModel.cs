using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.Stores;
using adrilight_shared.View.Canvas;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class DeviceControlViewModel : ViewModelBase
    {
        #region Construct
        public DeviceControlViewModel(DeviceCanvasViewModel canvasViewModel, EffectControlViewModel effectControlViewModel, VerticalMenuControlViewModel verticalMenu)
        {
            DrawableHlprs = new DrawableHelpers();
            EffectControl = effectControlViewModel;
            CanvasViewModel = canvasViewModel;
            VerticalMenu = verticalMenu;
            _deviceControlEvent = new DeviceControlEvent();
            EffectControl.DeviceControlEvent = _deviceControlEvent;
            CanvasViewModel.DeviceControlEvent = _deviceControlEvent;
            VerticalMenu.DeviceControlEvent = _deviceControlEvent;
            _deviceControlEvent.SelectedItemChanged += OnSelectedCanvasItemChanged;
            _deviceControlEvent.SelectedVerticalMenuIndexChanged += OnSelectedMenuIndexChanged;
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
        private void OnSelectedCanvasItemChanged(IDrawable item)
        {
            item.IsSelected = true;
            object controlItem;
            if (item is Border)
            {
                var group = Device.ControlZoneGroups.FirstOrDefault(g => g.Name == item.Name && g.Type == Device.CurrentActiveController.Type);
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
            if (CanvasViewModel.Items == null)
            {
                CanvasViewModel.Items = new System.Collections.ObjectModel.ObservableCollection<IDrawable>();
            }
            CanvasViewModel.Items.Clear();
            foreach (var item in Device.CurrentLiveViewZones)
            {
                if (!item.IsInControlGroup)
                    (item as IDrawable).IsSelectable = true;
                CanvasViewModel.Items.Add(item as IDrawable);
                if ((item as IDrawable).IsSelected)
                    CanvasViewModel.SelectedItem = item as IDrawable;
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
                foreach (var group in orderedGroups)
                {
                    CanvasViewModel.Items.Insert(0, group.Border);
                }
                foreach (var output in Device.AvailableLightingOutputs.Where(o => o.IsEnabled))
                {
                    var lightingDevice = output.SlaveDevice as ARGBLEDSlaveDevice;
                    lightingDevice.IsSelectable = false;

                    CanvasViewModel.Items.Insert(0, lightingDevice);
                }
            }
            OnSelectedCanvasItemChanged(CanvasViewModel.Items.Where(i => i.IsSelectable).First());
            UpdateView();
        }
        public void UpdateView()
        {
            // this called when items change or group change to resize the canvas accordingly
            if (CanvasViewModel == null)
                return;
            CanvasViewModel.UpdateView();

        }
        #endregion


        #region Commands

        #endregion
    }
}
