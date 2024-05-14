using adrilight.Ticker;
using adrilight.View;
using adrilight.ViewModel.LightingProfile;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class LightingProfileManagerViewModel : ViewModelBase
    {
        #region Construct

        public LightingProfileManagerViewModel(PlaylistDecoder decoder, DialogService service, IList<ISelectablePage> availablePages,
            LightingProfileCollectionViewModel profileCollectionViewModel,
            LightingProfilePlayerViewModel playerViewModel)
        {
            //load profile and playlist
            SelectablePages = availablePages;
            _decoder = decoder;
            _profileCollectionViewModel = profileCollectionViewModel;
            _playerViewModel = playerViewModel;
            _dialogService = service;
            LoadNonClientAreaData("Adrilight  |  Lighting Profile Manager", "profileManager",false,null);
            LoadData();
            CommandSetup();
        }
        #endregion


        #region Events

        #endregion


        #region Properties
        public NonClientArea NonClientAreaContent { get; set; }
        private PlaylistDecoder _decoder;
        private LightingProfileCollectionViewModel _profileCollectionViewModel;
        private LightingProfilePlayerViewModel _playerViewModel;
        private ProfilesManager _lightingProfileManager;
        private bool _isManagerWindowOpen;
        private DialogService _dialogService;
        private ISelectablePage _selectedPage;

        private readonly CollectionItemStore _collectionItemStore;
        public IList<ISelectablePage> SelectablePages { get; set; }
        public ISelectablePage SelectedPage {
            get => _selectedPage;

            set
            {
                Set(ref _selectedPage, value);
            }
        }
        public bool IsManagerWindowOpen {
            get
            {
                return _isManagerWindowOpen;
            }
            set
            {
                _isManagerWindowOpen = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Methods
        private void CommandSetup()
        {
            ////ChangePlaylistProfileDurationCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            ////{
            ////    return p != null;
            ////}, (p) =>
            ////{
            ////    var vm = new NumberInputDialogViewModel("Set time", 60, "Thời gian cho mỗi profile tính bằng giây", "stop_clock");
            ////    _dialogService?.ShowDialog<NumberInputDialogViewModel>(result =>
            ////    {
            ////        if (result == "True")
            ////            p.SetProfileDuration(vm.Value);
            ////    }, vm);

            ////});
  
            WindowClosing = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                IsManagerWindowOpen = false;

            });
            WindowOpen = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                IsManagerWindowOpen = true;

            });
        }
        private void LoadNonClientAreaData(string content, string geometry, bool showBackButton, ICommand buttonCommand)
        {
            var vm = new NonClientAreaContentViewModel(content, geometry, showBackButton, buttonCommand);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }
        public void LoadData()
        {

            _lightingProfileManager = new ProfilesManager();
            var profiles = _lightingProfileManager.LoadLightingProfileIfExist();
            if (profiles == null)
                return;
            foreach (var profile in profiles)
            {
                _profileCollectionViewModel.AvailableLightingProfiles.AddItem(profile);
            }
            var playlists = _lightingProfileManager.LoadLightingProfilePlaylistIfExist();
            if (playlists == null)
                return;
            foreach (var playlist in playlists)
            {
                _profileCollectionViewModel.AvailableLightingProfilesPlaylists.AddItem(playlist);
               // if (playlist.IsPlaying)
                 //   PlaySelectedItem(playlist);
            }
        }
        
        #endregion


        #region Commands
        public ICommand ChangePlaylistProfileDurationCommand { get; set; }
        public ICommand PlaySelectedItemCommand { get; set; }
        public ICommand StopSelectedItemCommand { get; set; }
        public ICommand WindowClosing { get; private set; }
        public ICommand WindowOpen { get; private set; }
        #endregion





    }
}
