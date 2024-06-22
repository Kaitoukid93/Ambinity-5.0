using adrilight.Manager;
using adrilight.Ticker;
using adrilight.View;
using adrilight.View.Screens.LightingProfile;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static adrilight.View.PlaylistEditorView;
using static adrilight.View.Screens.LightingProfile.ManagerCollectionView;

namespace adrilight.ViewModel.Profile
{
    public class LightingProfileManagerViewModel : ItemsManagerViewModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// handle profile and playlist, raise event when profile changed to show on player view (player viewmodel)
        /// <param name="decoder"></param>
        /// event store for ItemsCollection
        /// <param name="itemStore"></param>
        /// 
        /// <param name="service"></param>
        /// <param name="availablePages"></param>
        /// <param name="editorViewModel"></param>
        /// <param name="profileCollectionViewModel"></param>
        /// <param name="playerViewModel"></param>
        #region Construct

        public LightingProfileManagerViewModel(
            DialogService service,
            LightingProfileManager profileManager,
            IList<ISelectablePage> availablePages,
            LightingProfilePlaylistEditorViewModel editorViewModel,
            LightingProfileCollectionViewModel profileCollectionViewModel,
            LightingProfilePlayerViewModel playerViewModel)
        {
            //load profile and playlist
            SelectablePages = availablePages;
            _profileCollectionViewModel = profileCollectionViewModel;
            _playerViewModel = playerViewModel;
            _profileManager = profileManager;
            _playlistEditorViewModel = editorViewModel;
            CommandSetup();
        }


        #endregion


        #region Events
        //when profile or playlist card get clicked
        private async void OnProfileClicked(IGenericCollectionItem item)
        {
            //play this profile
           await _profileManager.ActivateProfile(item as LightingProfile);
        }
        private void OnPlaylistClicked(IGenericCollectionItem item)
        {
            //go to editor
            GotoPlaylistEditor(item);
        }
        private async void OnPlaylistPlayButtonClicked(IGenericCollectionItem item)
        {
            //play this playlist
            await _profileManager.ActivatePlaylist(item as LightingProfilePlaylist);
        }
        #endregion


        #region Properties
        private NonClientArea _nonClientAreaContent;
        public NonClientArea NonClientAreaContent {
            get
            {
                return _nonClientAreaContent;
            }
            set
            {
                _nonClientAreaContent = value;
                RaisePropertyChanged();
            }
        }
        private LightingProfileManager _profileManager;
        private LightingProfileCollectionViewModel _profileCollectionViewModel;
        private LightingProfilePlayerViewModel _playerViewModel;
        private bool _isManagerWindowOpen;
        private ISelectablePage _selectedPage;
        private LightingProfilePlaylistEditorViewModel _playlistEditorViewModel;
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
        public override void LoadData()
        {
            _profileCollectionViewModel.Init();
            _profileCollectionViewModel.LightingProfileClicked += OnProfileClicked;
            _profileCollectionViewModel.PlaylistClicked += OnPlaylistClicked;
            _profileCollectionViewModel.PlaylistCardButtonClicked += OnPlaylistPlayButtonClicked;
            _profileManager.WindowsStatusChanged(true);
            BacktoCollectionView();
        }
        public override void Dispose()
        {

            _profileCollectionViewModel.LightingProfileClicked -= OnProfileClicked;
            _profileCollectionViewModel.PlaylistClicked -= OnPlaylistClicked;
            _profileCollectionViewModel.PlaylistCardButtonClicked -= OnPlaylistPlayButtonClicked;
            _profileManager.WindowsStatusChanged(false);
            _playerViewModel.Dispose();
            _profileCollectionViewModel.Dispose();
            base.Dispose();
        }

        private void GotoPlaylistEditor(IGenericCollectionItem item)
        {
            if (item == null)
            {
                return;
            }
            var playlist = item as LightingProfilePlaylist;
            //show editor view
            var editorView = SelectablePages.Where(p => p is PlaylistEditorViewPage).First();
            _playlistEditorViewModel.Init(playlist);
            SelectedPage = editorView;
            ICommand backButtonCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BacktoCollectionView();
            }
            );
            LoadNonClientAreaData("Adrilight  |  Profile Manager | " + playlist.Name, "profileManager", true, backButtonCommand);

        }
        private void BacktoCollectionView()
        {
            LoadNonClientAreaData("Adrilight  |  Lighting Profile Manager", "profileManager", false, null);
            var collectionView = SelectablePages.Where(p => p is ManagerCollectionViewPage).First();
            SelectedPage = collectionView;

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
