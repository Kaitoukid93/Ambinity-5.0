using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace adrilight_shared.Models
{
    public class DataCollection : ViewModelBase
    {
        #region Construct
        public DataCollection()
        {

        }
        public DataCollection(string name, IDialogService dialogService, CollectionItemStore store)
        {
            DialogService = dialogService;
            Name = name;
            Items = new ObservableCollection<IGenericCollectionItem>();
            _collectionItemStore = store;
        }
        #endregion


        #region Event
        #endregion


        #region Properties
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        public IDialogService DialogService { get; set; }
        public string Geometry { get; set; } = "binary";
        private bool _showManagerToolBar;

        private List<IGenericCollectionItem> SelectedItems => Items.Where(i => i.IsChecked == true).ToList();
        private readonly CollectionItemStore _collectionItemStore;
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
        public void UnPinItem(IGenericCollectionItem item)
        {
            item.IsPinned = false;
        }
        public void PinItem (IGenericCollectionItem item)
        {
            item.IsPinned = true;
         
        }
        private void RefreshToolBarState()
        {
            if (Items.Where(i => i.IsChecked).Count() > 0)
            {
                ShowManagerToolBar = true;
            }
            else
            {
                ShowManagerToolBar = false;
            }
        } 
        public void ResetSelectionStage()
        {
            foreach (var item in Items)
                if (item.IsChecked)
                {
                    item.IsChecked = false;
                }
            RefreshToolBarState();
        }
        #endregion





    }
}
