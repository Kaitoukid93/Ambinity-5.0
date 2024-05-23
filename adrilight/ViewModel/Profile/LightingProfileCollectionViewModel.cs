using adrilight_shared.Models.ItemsCollection;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using System.Windows.Documents;
using System.Collections.Generic;
using System;
using adrilight.Manager;

namespace adrilight.ViewModel.Profile
{

    public class LightingProfileCollectionViewModel : ViewModelBase
    {
        /// <summary>
        /// this viewmodel contains lighting profile collection and lighting profile playlist collection
        /// on profile card click-> overide play this profile
        /// on playlist play click-> overide play this playlist
        /// on playlist card click-> go to playlist editor
        /// </summary>
        /// 
        public event Action<IGenericCollectionItem> LightingProfileClicked;
        public event Action<IGenericCollectionItem> PlaylistClicked;
        public event Action<IGenericCollectionItem> PlaylistCardButtonClicked;
        #region Construct
        public LightingProfileCollectionViewModel(DialogService dialogService,LightingProfileManager manager)
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            _manager = manager;
            _dialogService = dialogService;
            CommandSetup();
        }
        #endregion
        #region Properties
        private DialogService _dialogService;
        private LightingProfileManager _manager;
        private string _warningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message;
        public ItemsCollection AvailableLightingProfiles { get; set; }
        public ItemsCollection AvailableLightingProfilesPlaylists { get; set; }
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
        public bool ShowToolBar => AvailableTools.Count > 0;

        public string WarningMessage {
            get
            {
                return _warningMessage;
            }
            set
            {
                _warningMessage = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region Methods
        public void Init()
        {
            AvailableLightingProfiles = new ItemsCollection("Lighting Profiles", _dialogService);
            AvailableLightingProfilesPlaylists = new ItemsCollection("Playlists", _dialogService);
            foreach (var profile in _manager.AvailableProfiles)
            {
                AvailableLightingProfiles.AddItem(profile);
            }
            foreach (var playlist in _manager.AvailablePlaylists)
            {
                AvailableLightingProfilesPlaylists.AddItem(playlist);
            }

        }
        private void CommandSetup()
        {
            CollectionItemToolCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p)
                {
                    case "delete":
                        AvailableLightingProfiles.RemoveSelectedItems(true);
                        break;
                    case "addto":
                        //open popup view
                        //addto popup init

                        break;
                }
                UpdateTools();
            });
            OpenCreateNewPlaylistCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var vm = new AddNewDialogViewModel(adrilight_shared.Properties.Resources.AddNew, "New Playlist", null);
                _dialogService.ShowDialog<AddNewDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        CreateNewPlaylist(vm.Content);
                    }

                }, vm);
            });
            AddSelectedProfileToPlaylistCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddSelectedProfileToPlaylist(p);
            });
            LightingProfileClickCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            {
                return true;
            }, (p) =>
            {
                LightingProfileClicked?.Invoke(p);
            });
            PlaylistClickCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            {
                return true;
            }, (p) =>
            {
                PlaylistClicked?.Invoke(p);
            });
            PlaylistPlayButtonClickCommand = new RelayCommand<LightingProfilePlaylist>((p) =>
            {
                return true;
            }, (p) =>
            {
                PlaylistCardButtonClicked?.Invoke(p);
            });
        }
        private void CreateNewPlaylist(string name)
        {
            var newPlaylist = new LightingProfilePlaylist(name);
            foreach (var item in AvailableLightingProfiles.Items)
            {
                if (item.IsChecked)
                    newPlaylist.LightingProfiles.Add(item as adrilight_shared.Models.Lighting.LightingProfile);
            }
            AvailableLightingProfilesPlaylists.AddItem(newPlaylist);
        }
        private void AddSelectedProfileToPlaylist(LightingProfilePlaylist targetPlaylist)
        {
            foreach (var item in AvailableLightingProfiles.Items)
            {
                if (item.IsChecked)
                    targetPlaylist.LightingProfiles.Add(item as adrilight_shared.Models.Lighting.LightingProfile);
            }

        }
        private void UpdateTools()
        {
            //clear Tool
            AvailableTools?.Clear();
            var selectedItems = AvailableLightingProfiles.Items.Where(d => d.IsSelected).ToList();
            if (selectedItems == null)
                return;
            if (selectedItems.Count == 0)
                return;
            AvailableTools.Add(DeleteTool());

            AvailableTools.Add(AddtoTool());
            RaisePropertyChanged(nameof(ShowToolBar));
        }
        private CollectionItemTool DeleteTool()
        {
            return new CollectionItemTool() {
                Name = "Delete",
                ToolTip = "Delete Selected Items",
                Geometry = "remove",
                CommandParameter = "delete"

            };
        }
        private CollectionItemTool AddtoTool()
        {
            return new CollectionItemTool() {
                Name = "Add to...",
                ToolTip = "Add SelectedItem to Playlist",
                Geometry = "add",
                CommandParameter = "addto"

            };
        }
        #endregion
        #region Command
        public ICommand AdditemToSelectionCommand { get; set; }
        public ICommand CollectionItemToolCommand { get; set; }
        public ICommand OpenCreateNewPlaylistCommand { get; set; }
        public ICommand AddSelectedProfileToPlaylistCommand { get; set; }
        public ICommand LightingProfileClickCommand { get; set; }
        public ICommand PlaylistClickCommand { get; set; }
        public ICommand PlaylistPlayButtonClickCommand { get; set; }
        #endregion
    }
}
