﻿using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.ItemsCollection
{
    public class ItemsCollection : ViewModelBase
    {
        #region Construct
        public ItemsCollection()
        {
            AvailableTools = new ObservableCollection<CollectionItemTool>();
        }
        public ItemsCollection(string name, IDialogService dialogService, CollectionItemStore store)
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
        //private
        private readonly CollectionItemStore _collectionItemStore;
        //public
        public ObservableCollection<CollectionItemTool> AvailableTools { get; set; }
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        public IDialogService DialogService { get; set; }
        public string Geometry { get; set; } = "binary";
        public ItemsCollectionSource ItemsCollectionSource { get; set; }
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

        public void AddItem(IGenericCollectionItem item)
        {
            item.PropertyChanged += ItemPropertyChanged;
            Items.Add(item);
        }
        public void RemoveSelectedItems(bool alsoRemoveLocalFiles)
        {
            var selectedItems = Items.Where(i => i.IsSelected).ToList();
            foreach (var item in selectedItems)
            {
                Items.Remove(item);
                //remove file local path
                if (alsoRemoveLocalFiles)
                {
                    if (Directory.Exists(item.LocalPath))
                    {
                        try
                        {
                            Directory.Delete(item.LocalPath);
                        }
                        catch (Exception ex)
                        {
                            //
                        }
                    }

                }
            }

        }
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IGenericCollectionItem.Name):
                    _collectionItemStore.ChangeItemName(sender as IGenericCollectionItem);
                    break;
                case nameof(IGenericCollectionItem.IsPinned):
                    _collectionItemStore.ChangeItemName(sender as IGenericCollectionItem);
                    break;
                case nameof(IGenericCollectionItem.IsChecked):
                    _collectionItemStore.ChangeItemName(sender as IGenericCollectionItem);
                    break;
            }
        }

        public void ResetSelectionStage()
        {
            foreach (var item in Items)
                if (item.IsChecked)
                {
                    item.IsChecked = false;
                }
        }
        #endregion





    }
}
