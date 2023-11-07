namespace adrilight_shared.Models
{
    public interface IGenericCollectionItem
    {
        string Name { get; set; }
        bool IsSelected { get; set; }
        bool IsEditing { get; set; }
        bool IsChecked { get; set; }
    }
}
