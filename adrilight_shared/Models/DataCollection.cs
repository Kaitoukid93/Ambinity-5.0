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

        private void OnCollectionViewNavigated(IGenericCollectionItem item, DataViewMode mode)
        {
            CurrentView = mode;
        }

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
            var vm = new AddNewDialogViewModel("Add New" + p, Name, Geometry);
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
            OpenRenameDialogCommand = new RelayCommand<IGenericCollectionItem>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new RenameDialogViewModel("Rename", p.Name, "rename");
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
                var vm = new AddNewDialogViewModel("Add New" + Geometry, Name, Geometry);
                DialogService.ShowDialog<AddNewDialogViewModel>(result =>
                {
                    if (result == "True")
                        AddNewItemToCollection(vm.Content);
                }, vm);

            });
            OpenDeleteDialogCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                var vm = new DeleteDialogViewModel("Delete", "Bạn chắc chắn muốn xoá nội dung này?");
                DialogService.ShowDialog<DeleteDialogViewModel>(result =>
                {
                    if (result == "True")
                    {
                        RemoveItems();
                        CurrentView = DataViewMode.Collection;
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
        #endregion


        #region Commands
        public ICommand GotoCurrentItemDetailViewCommand { get; set; }
        public ICommand CreateNewCollectionFromSelectedItemsCommand { get; set; }
        public ICommand SelectItems { get; set; }
        public ICommand OpenAddNewDialogCommand { get; set; }
        public ICommand OpenRenameDialogCommand { get; set; }
        public ICommand OpenDeleteDialogCommand { get; set; }
        public ICommand ItemCheckUncheckCommand { get; set; }
        public ICommand SelectItem { get; set; }
        #endregion




    }
}
