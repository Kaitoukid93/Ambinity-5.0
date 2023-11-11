using System;
using System.Collections.Generic;

namespace adrilight_shared.Models.Stores
{
    public class CollectionItemStore
    {
        public event Action<IGenericCollectionItem> ItemCreated;
        public event Action<DataCollection> CollectionCreated;
        public event Action<IGenericCollectionItem> SelectedItemChanged;
        public event Action<List<IGenericCollectionItem>, string> SelectedItemsChanged;
        public event Action<IGenericCollectionItem, DataViewMode> Navigated;
        public event Action<IGenericCollectionItem> ItemPinStatusChanged;
        public event Action<List<IGenericCollectionItem>> ItemsRemoved;
        public void CreateItem(IGenericCollectionItem item)
        {
            ItemCreated?.Invoke(item);
        }
        public void CreateCollection(DataCollection collection)
        {
            CollectionCreated?.Invoke(collection);
        }
        public void ChangeSelectedItem(IGenericCollectionItem item)
        {
            SelectedItemChanged?.Invoke(item);
        }
        public void ChangeSelectedItems(List<IGenericCollectionItem> items, string target)
        {
            SelectedItemsChanged?.Invoke(items, target);
        }
        public void GotoSelectedItemDetail(IGenericCollectionItem item, DataViewMode lastViewMode)
        {
            Navigated?.Invoke(item, lastViewMode);
        }
        public void BackToCollectionView(IGenericCollectionItem item)
        {
            Navigated?.Invoke(item, DataViewMode.Collection);
        }
        public void ChangeItemPinStatus(IGenericCollectionItem item)
        {
            ItemPinStatusChanged?.Invoke(item);
        }
        public void RemoveItems(List<IGenericCollectionItem> items)
        {
            ItemsRemoved?.Invoke(items);
        }
    }
}
