using adrilight.Ticker;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Lighting;
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
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string LightingProfilesCollectionFolderPath => Path.Combine(JsonPath, "LightingProfiles");
        private string LightingProfilePlaylistsCollectionFolderPath => Path.Combine(JsonPath, "Playlists");
        public LightingProfileManagerViewModel(CollectionItemStore store, PlaylistDecoder player, DialogService service)
        {
            //load profile and playlist
            _player = player;
            _player.CurrentPlayingProfileChanged += OnCurrentPlayingProfileChanged;

            _collectionItemStore = store;
            _collectionItemStore.CollectionCreated += OnCollectionCreated;
            _collectionItemStore.SelectedItemChanged += OnSelectedItemChanged;
            _collectionItemStore.SelectedItemsChanged += OnSelectedItemsChanged;
            _collectionItemStore.Navigated += OnCollectionViewNavigated;
            _collectionItemStore.ItemPinStatusChanged += OnCollectionPinStatusChanged;
            _collectionItemStore.ItemsRemoved += OnItemsRemove;


            _dialogService = service;
            LoadNonClientAreaData();
            LoadData();
            CommandSetup();
        }
        #endregion


        #region Events
        private void OnItemsRemove(List<IGenericCollectionItem> list)
        {
            foreach (var item in list)
            {
                DashboardPinnedItems.RemoveItems(item, false);
            }
            AvailableLightingProfiles.ResetSelectionStage();
            AvailableLightingProfilePlaylists.ResetSelectionStage();
        }
        private void OnCollectionPinStatusChanged(IGenericCollectionItem item)
        {
            RefreshDashboardItems();
        }
        private void OnSelectedItemsChanged(List<IGenericCollectionItem> list, string target)
        {
            var targetPlaylist = AvailableLightingProfilePlaylists.Items.Where(p => p.Name == target).FirstOrDefault();
            if (targetPlaylist != null)
            {
                foreach (var item in list)
                {
                    var playlist = targetPlaylist as LightingProfilePlaylist;
                    if (playlist.LightingProfiles == null)
                    {
                        playlist.LightingProfiles = new DataCollection(target, _dialogService, "profile", _collectionItemStore);
                    }
                    playlist.LightingProfiles.AddItems(item);
                }

            }
            else
            {
                targetPlaylist = AvailableLightingProfilePlaylists.Items.Where(p => (p as LightingProfilePlaylist).IsPlaying == true).FirstOrDefault();
                foreach (var item in list)
                    (targetPlaylist as LightingProfilePlaylist).LightingProfiles.AddItems(item);
            }
            AvailableLightingProfiles.ResetSelectionStage();
            AvailableLightingProfilePlaylists.ResetSelectionStage();
        }
        private void OnSelectedItemChanged(IGenericCollectionItem item)
        {
            if (item is LightingProfilePlaylist)
            {
                _player.Play(item as LightingProfilePlaylist);
            }
            else if (item is LightingProfile)
            {
                _player.Play(item as LightingProfile);
            }
        }
        private void OnCollectionCreated(DataCollection collection)
        {
            var newPlaylist = new LightingProfilePlaylist(collection.Name);
            newPlaylist.LightingProfiles = collection;
            AvailableLightingProfilePlaylists.AddItems(newPlaylist);
            AvailableLightingProfiles.ResetSelectionStage();
            AvailableLightingProfilePlaylists.ResetSelectionStage();
        }
        private void OnCurrentPlayingProfileChanged(LightingProfile profile)
        {
            CurrentPlayingProfile = profile;
        }
        private void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            //show button if needed
            var nonClientVm = NonClientAreaContent.DataContext as NonClientAreaContentViewModel;
            if (mode == DataViewMode.Detail)
            {
                //show button
                nonClientVm.ShowBackButton = true;
                nonClientVm.BackButtonCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, (p) =>
                {
                    _collectionItemStore.BackToCollectionView(item);
                }
                );
            }
            else
            {
                nonClientVm.ShowBackButton = false;
            }

        }
        #endregion


        #region Properties
        public NonClientArea NonClientAreaContent { get; set; }
        private PlaylistDecoder _player;
        private readonly CollectionItemStore _collectionItemStore;
        public List<LightingProfile> LoadLightingProfileIfExist()
        {
            var existedProfile = new List<LightingProfile>();
            string[] files = Directory.GetFiles(Path.Combine(LightingProfilesCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<LightingProfile>(jsonData);
                profile.LocalPath = file;
                existedProfile.Add(profile);
            }
            return existedProfile;
        }
        public List<LightingProfilePlaylist> LoadLightingProfilePlaylistIfExist()
        {
            var existedPlaylist = new List<LightingProfilePlaylist>();
            string[] files = Directory.GetFiles(Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var playlist = JsonConvert.DeserializeObject<LightingProfilePlaylist>(jsonData);
                playlist.LocalPath = file;
                if (playlist != null)
                {
                    playlist.LoadLightingProfiles(AvailableLightingProfiles.Items);
                    existedPlaylist.Add(playlist);
                }

            }
            return existedPlaylist;
        }
        private void RefreshDashboardItems()
        {
            if (DashboardPinnedItems == null)
                DashboardPinnedItems = new DataCollection("Dashboard Items", _dialogService, "profile", _collectionItemStore);
            DashboardPinnedItems.Items.Clear();
            foreach (var item in AvailableLightingProfilePlaylists.Items)
            {
                if (item.IsPinned)
                    DashboardPinnedItems.AddItems(item);
            }
            foreach (var item in AvailableLightingProfiles.Items)
            {
                if (item.IsPinned)
                    DashboardPinnedItems.AddItems(item);
            }
        }
        public DataCollection DashboardPinnedItems { get; set; }
        public DataCollection AvailableLightingProfiles { get; set; }
        public DataCollection AvailableLightingProfilePlaylists { get; set; }
        private DialogService _dialogService;
        private LightingProfile _currentPlayingProfile;
        public LightingProfile CurrentPlayingProfile {
            get
            {
                return _currentPlayingProfile;
            }
            set
            {
                _currentPlayingProfile = value;
                RaisePropertyChanged();
            }
        }
        private int _currentProfileTime;
        public int CurrentProfileTime {
            get
            {
                return _currentProfileTime;
            }
            set
            {
                _currentProfileTime = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Methods
        private void CommandSetup()
        {
            ChangePlaylistProfileDurationCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new NumberInputDialogViewModel("Set time", 60, "Thời gian cho mỗi profile tính bằng giây", "stop_clock");
                _dialogService?.ShowDialog<NumberInputDialogViewModel>(result =>
                {
                    if (result == "True")
                        p.SetProfileDuration(vm.Value);
                }, vm);

            });
        }
        public void SaveData()
        {
            lock (AvailableLightingProfiles.Items)
            {
                foreach (var item in AvailableLightingProfiles.Items)
                {
                    var profile = item as LightingProfile;
                    var localPath = Path.Combine(LightingProfilesCollectionFolderPath, "collection", item.Name + ".ALP");
                    JsonHelpers.WriteSimpleJson(profile, localPath);
                }
            }
            lock (AvailableLightingProfilePlaylists.Items)
            {
                foreach (var item in AvailableLightingProfilePlaylists.Items)
                {
                    var playlist = item as LightingProfilePlaylist;
                    var localPath = Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection", item.Name + ".LPP");
                    JsonHelpers.WriteSimpleJson(playlist, localPath);
                }
            }

        }
        private void LoadNonClientAreaData()
        {
            var vm = new NonClientAreaContentViewModel("Adrilight  |  Lighting Profile Manager", "profileManager");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                NonClientAreaContent = new NonClientArea();
                (NonClientAreaContent as FrameworkElement).DataContext = vm;
            });

        }
        public void LoadData()
        {
            CreateLightingProfilesCollection();
            AvailableLightingProfiles = new DataCollection("Lighting Profiles", _dialogService, "profile", _collectionItemStore);
            foreach (var profile in LoadLightingProfileIfExist())
            {
                AvailableLightingProfiles.AddItems(profile);
            }

            CreateLightingProfilePlaylistsCollection();
            AvailableLightingProfilePlaylists = new DataCollection("Lighting Profile Playlists", _dialogService, "profilePlaylist", _collectionItemStore);
            foreach (var playlist in LoadLightingProfilePlaylistIfExist())
            {
                AvailableLightingProfilePlaylists.AddItems(playlist);
                if (playlist.IsPlaying)
                    OnSelectedItemChanged(playlist);
            }

            RefreshDashboardItems();
        }
        private void CreateLightingProfilePlaylistsCollection()
        {
            if (!Directory.Exists(LightingProfilePlaylistsCollectionFolderPath))
            {
                Directory.CreateDirectory(LightingProfilePlaylistsCollectionFolderPath);
                var collectionFolder = Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var demoPlaylist = new LightingProfilePlaylist("All profiles");
                demoPlaylist.LightingProfiles = new DataCollection("profiles", _dialogService, "profile", _collectionItemStore);
                foreach (var profile in AvailableLightingProfiles.Items)
                {
                    demoPlaylist.LightingProfiles.AddItems(profile);
                    demoPlaylist.LightingProfilesUID.Add((profile as LightingProfile).ProfileUID);
                    demoPlaylist.IsDeleteable = false;
                }
                JsonHelpers.WriteSimpleJson(demoPlaylist, Path.Combine(collectionFolder, demoPlaylist.Name + ".LPP"));
                ///
                var config = new ResourceLoaderConfig(nameof(LightingProfilePlaylist), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(LightingProfilePlaylistsCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }


        }
        private void CreateLightingProfilesCollection()
        {
            if (!Directory.Exists(LightingProfilesCollectionFolderPath))
            {
                Directory.CreateDirectory(LightingProfilesCollectionFolderPath);
                var collectionFolder = Path.Combine(LightingProfilesCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var _resourceHelpers = new ResourceHelpers();
                var allResourceNames = _resourceHelpers.GetResourceFileNames();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".ALP")))
                {
                    var name = _resourceHelpers.GetResourceFileName(resourceName);
                    _resourceHelpers.CopyResource(resourceName, Path.Combine(collectionFolder, name));
                }
                var config = new ResourceLoaderConfig(nameof(LightingProfile), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(Path.Combine(LightingProfilesCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource
            }
        }
        #endregion


        #region Commands
        public ICommand ChangePlaylistProfileDurationCommand { get; set; }
        #endregion





    }
}
