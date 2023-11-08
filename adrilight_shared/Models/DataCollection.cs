using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace adrilight_shared.Models
{
    public class DataCollection : ViewModelBase
    {
        public DataCollection()
        {

        }
        public DataCollection(string name, IDialogService dialogService, string type)
        {
            DialogService = dialogService;
            Name = name;
            Type = type;
            Items = new ObservableCollection<IGenericCollectionItem>();
            CommandSetup();
        }

        #region Properties
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        public string Type { get; set; }
        public IDialogService DialogService { get; set; }
        private bool _showManagerToolBar;
        public bool ShowManagerToolBar
        {
            get
            {
                return _showManagerToolBar;
            }

            set
            {
                _showManagerToolBar = value;
                RaisePropertyChanged();
            }
        }
        private DataViewMode _currentView = DataViewMode.Collection;
        public DataViewMode CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                _currentView = value;
                RaisePropertyChanged();
            }
        }
        private IGenericCollectionItem _currentSelectedItem;
        public IGenericCollectionItem CurrentSelectedItem
        {
            get
            {
                return _currentSelectedItem;
            }
            set
            {
                _currentSelectedItem = value;
                RaisePropertyChanged();
            }
        }
        public string Name { get; set; }
        #endregion


        #region Methods
        private void CommandSetup()
        {
            GotoCurrentItemDetailViewCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                GotoDetailView(p);
            }
        );
            OpenRenameDialogCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                DialogService.ShowDialog<RenameDialogViewModel>(result => { var test = result; });

            });
            OpenDeleteDialogCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                DialogService.ShowDialog<DeleteDialogViewModel>(result => { var test = result; });
                //if result
                RemoveItems();
                CurrentView = DataViewMode.Collection;
            });
        }
        private void GotoDetailView(IGenericCollectionItem item)
        {
            item.IsEditing = true;
            CurrentSelectedItem = item;
            CurrentView = DataViewMode.Detail;
        }
        public void RemoveItems()
        {
            if (Items == null)
                return;
            var itemsToRemove = new List<IGenericCollectionItem>();

            foreach (var i in Items)
            {
                if (i.IsEditing || i.IsChecked)
                    itemsToRemove.Add(i);
            }
            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }

        }
        public void AddItems(IGenericCollectionItem item)
        {
            if (Items == null)
                Items = new ObservableCollection<IGenericCollectionItem>();
            Items.Add(item);
        }
        #endregion


        #region Commands
        public ICommand GotoCurrentItemDetailViewCommand { get; set; }
        public ICommand OpenRenameDialogCommand { get; set; }
        public ICommand OpenDeleteDialogCommand { get; set; }
        #endregion




    }
}
