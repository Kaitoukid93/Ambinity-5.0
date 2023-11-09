using adrilight.Ticker;
using adrilight_shared.Models;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class LightingProfileManagerViewModel : ViewModelBase
    {
        public LightingProfileManagerViewModel(DataCollection profiles, DataCollection playlists, CollectionItemStore store, PlaylistDecoder player, DialogService service)
        {
            AvailableLightingProfiles = profiles;
            AvailableLightingProfilePlaylists = playlists;
            _collectionItemStore = store;
            _player = player;
            CommandSetup();
            _collectionItemStore.CollectionCreated += OnCollectionCreated;
            _collectionItemStore.SelectedItemChanged += OnSelectedItemChanged;
            _collectionItemStore.SelectedItemsChanged += OnSelectedItemsChanged;
            _collectionItemStore.Navigated += OnCollectionViewNavigated;
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
                    _collectionItemStore.BackToCollectionView();
                }
                );
            }
            else
            {
                nonClientVm.ShowBackButton = false;
            }

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
        }
        public NonClientArea NonClientAreaContent { get; set; }
        private PlaylistDecoder _player;
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
        }

        private readonly CollectionItemStore _collectionItemStore;
        private void CommandSetup()
        {

        }

        public ICommand CreateNewLightingProfileFromSelectedProfilesCommand { get; set; }
        public DataCollection AvailableLightingProfiles { get; set; }
        public DataCollection AvailableLightingProfilePlaylists { get; set; }
        private DialogService _dialogService;
        private LightingProfile _currentSelectedLightingProfile;
        public LightingProfile CurrentSelectedLightingProfile {
            get
            {
                return _currentSelectedLightingProfile;
            }
            set
            {
                _currentSelectedLightingProfile = value;
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
    }
}
