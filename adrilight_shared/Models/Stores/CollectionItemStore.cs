using adrilight_shared.Models.ItemsCollection;
using System;
using System.Collections.Generic;

namespace adrilight_shared.Models.Stores
{
    public class CollectionItemStore
    {
        //raise when check box on item is checked or unchecked
        public event Action<IGenericCollectionItem> ItemSelectionChanged;
        // raise when item pin button is clicked
        public event Action<IGenericCollectionItem> ItemPinStatusChanged;
        // raise when item name changed
        public event Action<IGenericCollectionItem> ItemNameChanged;
        //raise when item get clicked by user
        public event Action<IGenericCollectionItem> ItemClicked;

        public void ChangeItemSelectedStatus(IGenericCollectionItem item)
        {
            ItemSelectionChanged?.Invoke(item);
        }
        public void ChangeItemPinStatus(IGenericCollectionItem item)
        {
            ItemSelectionChanged?.Invoke(item);
        }
        public void ChangeItemName(IGenericCollectionItem item)
        {
            ItemNameChanged?.Invoke(item);
        }
        public void ClickItem (IGenericCollectionItem item)
        {
            ItemClicked?.Invoke(item);
        }
    }
}
