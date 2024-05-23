using adrilight.Manager;
using adrilight.Ticker;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ItemsCollection;
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

namespace adrilight.ViewModel.Profile
{
    public class LightingProfilePlaylistEditorViewModel : ViewModelBase
    {
        #region Construct
        public LightingProfilePlaylistEditorViewModel(DialogService service,LightingProfileManager manager)
        {
            _dialogService = service;
            _profileManager = manager;
        }
        #endregion


        #region Events

        #endregion


        #region Properties
        private DialogService _dialogService;
        private LightingProfileManager _profileManager;
        private bool _isPlaying;
        public LightingProfilePlaylist Playlist { get; set; }
        #endregion


        #region Methods
        public void Init(LightingProfilePlaylist playlist)
        {
            Playlist = playlist;
        }
        private void CommandSetup()
        {
            OpenPlaylistDurationDialog = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new NumberInputDialogViewModel("Set time", 60, "Thời gian cho mỗi profile tính bằng giây", "stop_clock");
                _dialogService?.ShowDialog<NumberInputDialogViewModel>(result =>
                {
                    if (result == "True")
                        Playlist.SetProfileDuration(vm.Value);
                }, vm);

            });
            OpenRenamePlaylistDialog = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new RenameDialogViewModel(adrilight_shared.Properties.Resources.RenameDialog_Rename_titleelement, Playlist.Name, "rename");
                _dialogService?.ShowDialog<RenameDialogViewModel>(result =>
                {
                    if (result == "True")
                        Playlist.Name = vm.Content;
                }, vm);

            });
            Play = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                _profileManager.ActivatePlaylist(Playlist);

            });
        }
        #endregion


        #region Commands
        public ICommand Play { get; set; }
        public ICommand Stop { get; set; }
        public ICommand DeleteProfile { get; set; }
        public ICommand OpenPlaylistDurationDialog { get; set; }
        public ICommand OpenRenamePlaylistDialog { get; set; }
        #endregion





    }
}
