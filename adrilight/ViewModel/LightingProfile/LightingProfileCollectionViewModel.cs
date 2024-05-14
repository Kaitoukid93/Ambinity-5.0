using adrilight_shared.Models.ItemsCollection;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using adrilight_shared.Models.RelayCommand;

namespace adrilight.ViewModel.LightingProfile
{
    public class LightingProfileCollectionViewModel : ViewModelBase
    {
        /// <summary>
        /// this viewmodel contains lighting profile collection and lighting profile playlist collection
        /// on profile card click-> overide play this profile
        /// on playlist play click-> overide play this playlist
        /// on playlist card click-> go to playlist editor
        /// </summary>
        #region Construct
        public LightingProfileCollectionViewModel()
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
            AvailableLightingProfiles = new ItemsCollection();
            CommandSetup();
        }
        #endregion
        #region Properties
        private string _warningMessage = adrilight_shared.Properties.Resources.DeviceManager_DisConnect_Warning_Message;
        public ItemsCollection AvailableLightingProfiles { get; set; }
        public ItemsCollection AvailableLightingProfilesPlaylists { get; set; }
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }

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
        private void CommandSetup()
        {
            CollectionItemToolCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                switch (p)
                {
                    case "delete":
                        AvailableLightingProfiles.RemoveSelectedItems(true);
                        break;
                    case "addto":

                        break;
                }
                UpdateTools();
            });
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
        #endregion
    }
}
