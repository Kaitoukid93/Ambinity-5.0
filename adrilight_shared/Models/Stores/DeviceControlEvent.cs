using adrilight_shared.Model.VerticalMenu;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Drawable;
using System;
using System.Collections.Generic;

namespace adrilight_shared.Models.Stores
{
    public class DeviceControlEvent
    {
        public event Action<IDrawable> SelectedItemChanged;
        public event Action<List<IDrawable>> SelectedItemsChanged;
        public event Action<List<IDrawable>> SelectedItemUngrouped;
        public event Action<ControlZoneGroup> SelectedItemGrouped;
        public event Action<int> SelectedVerticalMenuIndexChanged;
        public event Action UnselectAllItemEvent;
        public event Action<bool> LoadingControlParamStatusChanged;
        
        public void ChangeSelectedItem(IDrawable item)
        {
            SelectedItemChanged?.Invoke(item);
        }
        public void ChangeSelectedItems(List<IDrawable> items)
        {
            SelectedItemsChanged?.Invoke(items);
        }
        public void UngroupSelectedItem(List<IDrawable> item)
        {
            SelectedItemUngrouped?.Invoke(item);
        }
        public void GroupSelectedItem(ControlZoneGroup newGroup)
        {
            SelectedItemGrouped?.Invoke(newGroup);
        }
        public void ChangeSelectedVerticalMenuIndex(int item)
        {
            SelectedVerticalMenuIndexChanged?.Invoke(item);
        }
        public void UnSelectAllItem()
        {
            UnselectAllItemEvent?.Invoke();
        }
        public void ChangeLoadingParamStatus(bool status)
        {
            LoadingControlParamStatusChanged?.Invoke(status);
        }
    }
}
