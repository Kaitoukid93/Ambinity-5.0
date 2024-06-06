﻿using adrilight_shared.Models.Device;
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
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device.Group;
using System.Threading.Tasks;
using System.ComponentModel;
using adrilight.Ticker;
using adrilight_shared.Enums;
using adrilight.Manager;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight.ViewModel.DeviceControl;

namespace adrilight.ViewModel
{
    /// <summary>
    /// this is representing a rich canvas
    /// contains interaction logic with the UI
    /// </summary>
    public class EffectControlViewModel : ViewModelBase
    {
        #region Construct
        public EffectControlViewModel(PlaylistDecoder lightingPlayer, LightingProfileManager lightingProfileManager)
        {
            LightingPlayer = lightingPlayer;
            LightingPlayer.IsRunningPropertyChanged += LightingPlayer_IsRunningPropertyChanged;
            LightingProfileManager = lightingProfileManager;
            AvailableParameters = new ObservableCollection<ControlParameterViewModel>();
            AvailableControlMode = new ObservableCollection<IControlMode>();
            CommandSetup();
        }

        private void LightingPlayer_IsRunningPropertyChanged(bool obj)
        {
            RaisePropertyChanged(nameof(ShowPlayerWarning));
        }

        #endregion


        #region Properties
        private DialogService _dialogService;
        private DeviceControlEvent _deviceControlEvent;
        private Object _controlItem;
        private bool _isLoadingControlMode;
        private IControlMode _selectedControlMode;
        private ObservableCollection<IControlMode> _availableControlMode;
        private bool _isLoadingParam;


        //public//
        public bool ShowPlayerWarning {
            get
            {
                return LightingPlayer.IsRunning && SelectedControlMode is LightingMode;
            }
        }
        public DeviceControlEvent DeviceControlEvent {
            get { return _deviceControlEvent; }
            set { _deviceControlEvent = value; }
        }
        public bool IsLoadingControlMode {
            get { return _isLoadingControlMode; }
            set
            {
                _isLoadingControlMode = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<IControlMode> AvailableControlMode {
            get { return _availableControlMode; }
            set
            {
                _availableControlMode = value;
                RaisePropertyChanged();
            }
        }
        public IControlMode SelectedControlMode {
            get { return _selectedControlMode; }
            set
            {
                _selectedControlMode = value;
                RaisePropertyChanged();
                AvailableParameters?.Clear();
                ControlParametersInit(_selectedControlMode);

            }
        }
        private async Task ControlParametersInit(IControlMode controlMode)
        {
            _deviceControlEvent.ChangeLoadingParamStatus(true);
            await Task.Run(async () =>
           {
               foreach (var param in controlMode.Parameters)
               {
                   await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                   {
                       var vm = new ControlParameterViewModel();
                       vm.Init(param);
                       AvailableParameters.Add(vm);
                   });
                   await Task.Delay(100);
               }

           });
            _deviceControlEvent.ChangeLoadingParamStatus(false);

        }
        private ObservableCollection<ControlParameterViewModel> _availableParameters;
        public ObservableCollection<ControlParameterViewModel> AvailableParameters {
            get
            {
                return _availableParameters;
            }
            set
            {
                _availableParameters = value;
                RaisePropertyChanged();

            }
        }
        public Object ControlItem {
            get
            {
                return _controlItem;
            }
            set
            {
                if (_controlItem != value)
                {
                    _controlItem = value;
                    Init();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShowPlayerWarning));
                }

            }
        }
        public PlaylistDecoder LightingPlayer { get; set; }
        public LightingProfileManager LightingProfileManager { get; set; }
        #endregion


        #region Methods

        /// <summary>
        /// setup all commands
        /// </summary>
        private void CommandSetup()
        {

            ChangeSelectedControlModeCommand = new RelayCommand<IControlMode>((p) =>
            {
                return true;
            }, async (p) =>
            {
                SelectedControlMode = p;
                await ChageControlMode(p);
            });
            StopLightingPlaylistCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LightingPlayer.Stop();
                //stop playlist
                //select first item
                //update effect view to first item
            });
        }

        public void Init()
        {
            //load available control mode
            //load selected control mode parameters
            if (ControlItem == null)
            {
                //show null view
                return;
            }
            AvailableControlMode?.Clear();
            if (ControlItem is IControlZone)
            {
                foreach (var item in (ControlItem as IControlZone).AvailableControlMode)
                {
                    AvailableControlMode.Add(item);
                };
                SelectedControlMode = (ControlItem as IControlZone).CurrentActiveControlMode;
            }
            else if (ControlItem is ControlZoneGroup)
            {
                foreach (var item in (ControlItem as ControlZoneGroup).MaskedControlZone.AvailableControlMode)
                {
                    AvailableControlMode.Add(item);
                };
                SelectedControlMode = (ControlItem as ControlZoneGroup).MaskedControlZone.CurrentActiveControlMode;
            }
        }
        private async Task ChageControlMode(IControlMode controlMode)
        {
            IsLoadingControlMode = true;

            if (ControlItem is ControlZoneGroup)
            {
                var group = ControlItem as ControlZoneGroup;
                foreach (var zone in group.ControlZones)
                {
                    (zone as IControlZone).ColapseAllController();
                    await Task.Run(() => { (zone as IControlZone).CurrentActiveControlMode = controlMode; });
                }
                await Task.Run(() => { group.MaskedControlZone.CurrentActiveControlMode = controlMode; });
            }

            else if (ControlItem is IControlZone)
            {
                var zone = ControlItem as IControlZone;
                zone.ColapseAllController();
                await Task.Run(() => { zone.CurrentActiveControlMode = controlMode; });
            }

            IsLoadingControlMode = false;

        }

        public void Dispose()
        {
            LightingPlayer.IsRunningPropertyChanged -= LightingPlayer_IsRunningPropertyChanged;
        }
        #endregion


        #region Commands
        public ICommand ChangeSelectedControlModeCommand { get; set; }
        public ICommand StopLightingPlaylistCommand { get; set; }
        #endregion
    }
}
