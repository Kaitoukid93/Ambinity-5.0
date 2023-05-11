using System.ComponentModel;

namespace adrilight.Util
{
    public interface IParameterValue : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
    }
}