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
        public DataCollection(string name, IDialogService dialogService, string geometry, CollectionItemStore store)
        {
            DialogService = dialogService;
            Name = name;
            Items = new ObservableCollection<IGenericCollectionItem>();
            Geometry = geometry;
            _collectionItemStore = store;
            _collectionItemStore.Navigated += OnCollectionViewNavigated;
            CommandSetup();
        }
        #endregion


        #region Event
        private void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            CurrentView = mode;
            if (mode == DataViewMode.Collection)
            {
                if (item != null)
                    item.IsEditing = false;
            }

        }
        #endregion


        #region Properties
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        public IDialogService DialogService { get; set; }
        public string Geometry { get; set; } = "binary";
        private bool _showManagerToolBar;
        private string _warningMessage;
        public string WarningMessage
        {
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
                //send signal back to
                GotoDetailView(p);
            }

        );
            ShowLoadingScreenCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                //send signal back to
                ShowLoadingView(p);
            }
        );
         

            UnpinItem = new RelayCommand<IGenericCollectionItem>((p) =>
               {
                   return true;
               }, (p) =>
               {
                   p.IsPinned = false;
                   _collectionItemStore.ChangeItemPinStatus(p);
               }

        );
            SelectItem = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                _collectionItemStore.ChangeSelectedItem(p);
            }

     );
            CreateNewCollectionFromSelectedItemsCommand = new RelayCommand<string>((p) =>
        {
            return true;
        }, (p) =>
        {
            var vm = new AddNewDialogViewModel(adrilight_shared.Properties.Resources.AddNew, Name, Geometry);
            DialogService.ShowDialog<AddNewDialogViewModel>(result =>
            {
                if (result == "True")
                {
                    var newCollection = CreateDataCollectionFromSelectedItem(vm.Content, p);
                    _collectionItemStore.CreateCollection(newCollection);
                }

            }, vm);
        }

        );
            SelectItems = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                _collectionItemStore.ChangeSelectedItems(SelectedItems, p);
            }

        );

            ItemCheckUncheckCommand = new RelayCommand<bool>((p) =>
            {
                return true;
            }, (p) =>
            {
                RefreshToolBarState();
            }

       );
            ItemPinUnPinCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                _collectionItemStore.ChangeItemPinStatus(p);
            }

);
            OpenRenameDialogCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new RenameDialogViewModel(adrilight_shared.Properties.Resources.RenameDialog_Rename_titleelement, p.Name, "rename");
                DialogService.ShowDialog<RenameDialogViewModel>(result =>
                {
                    if (result == "True")
                        p.Name = vm.Content;
                }, vm);

            });
            OpenAddNewDialogCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new AddNewDialogViewModel(adrilight_shared.Properties.Resources.AddNew, Name, Geometry);
                DialogService.ShowDialog<AddNewDialogViewModel>(result =>
                {
                    if (result == "True")
                        AddNewItemToCollection(vm.Content);
                }, vm);

            });
            RemoveItemCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                RemoveItems(p, true);

            });
            OpenDeleteDialogCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new DeleteDialogViewModel(adrilight_shared.Properties.Resources.DeleteDialog_Name, adrilight_shared.Properties.Resources.DeleteDialog_Confirm_Header);
                DialogService.ShowDialog<DeleteDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        RemoveItems(true);
                        RefreshToolBarState();
                        _collectionItemStore.BackToCollectionView(null);
                    }


                }, vm);


                ;
            });
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
        private DataCollection CreateDataCollectionFromSelectedItem(string name, string geometry)
        {
            var newCollection = new DataCollection(name, DialogService, geometry, _collectionItemStore);
            foreach (var item in Items)
            {
                if (item.IsChecked)
                    newCollection.AddItems(item);
            }
            return newCollection;
        }

        private void GotoDetailView(IGenericCollectionItem item)
        {
            item.IsEditing = true;
            CurrentSelectedItem = item;
            CurrentView = DataViewMode.Detail;
            _collectionItemStore.GotoSelectedItemDetail(item, CurrentView);
        }
        private void ShowLoadingView(IGenericCollectionItem item)
        {
            item.IsEditing = true;
            CurrentSelectedItem = item;
            CurrentView = DataViewMode.Loading;
            _collectionItemStore.ShowLoadingScreen(item, CurrentView);
        }
        public void RemoveItems(bool notify)
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

            if (notify)
            {
                _collectionItemStore.RemoveItems(itemsToRemove);
            }

        }
        public void RemoveItems(IGenericCollectionItem item, bool notify)
        {
            Items.Remove(item);
            if (notify)
            {
                _collectionItemStore.RemoveItems(new List<IGenericCollectionItem>() { item });
            }
        }
        public void AddItems(IGenericCollectionItem item)
        {
            if (Items == null)
                Items = new ObservableCollection<IGenericCollectionItem>();
            if (Items.Contains(item))
                return;
            Items.Add(item);
        }
        public void AddNewItemToCollection(string name)
        {
            var type = Items.First().GetType();
            var newItem = Activator.CreateInstance(type);
            (newItem as IGenericCollectionItem).Name = name;
            Items.Add(newItem as IGenericCollectionItem);
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


        #region Commands
        public ICommand RemoveItemCommand { get; set; }
        public ICommand GotoCurrentItemDetailViewCommand { get; set; }
        public ICommand ShowLoadingScreenCommand { get; set; }
        public ICommand CreateNewCollectionFromSelectedItemsCommand { get; set; }
        public ICommand SelectItems { get; set; }
        public ICommand OpenAddNewDialogCommand { get; set; }
        public ICommand OpenRenameDialogCommand { get; set; }
        public ICommand OpenDeleteDialogCommand { get; set; }
        public ICommand ItemCheckUncheckCommand { get; set; }
        public ICommand SelectItem { get; set; }
        public ICommand UnpinItem { get; set; }
        public ICommand ItemPinUnPinCommand { get; set; }
        #endregion




    }
}
