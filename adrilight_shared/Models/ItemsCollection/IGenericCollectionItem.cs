using System.ComponentModel;

namespace adrilight_shared.Models.ItemsCollection
{
    public interface IGenericCollectionItem : INotifyPropertyChanged
    {
        string Name { get; set; }
        bool IsSelected { get; set; }
        bool IsEditing { get; set; }
        bool IsChecked { get; set; }
        bool IsPinned { get; set; }
        string LocalPath { get; set; }
        string InfoPath { get; set; }
    }
}
