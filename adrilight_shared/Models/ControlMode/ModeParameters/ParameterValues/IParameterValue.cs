using System.ComponentModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public interface IParameterValue : INotifyPropertyChanged, IGenericCollectionItem
    {
        new string Name { get; }
        string Description { get; }
        new bool IsChecked { get; set; }
        new string LocalPath { get; set; }
        new string InfoPath { get; set; }
        bool IsDeleteable { get; set; }
        bool IsVisible { get; set; }
    }
}