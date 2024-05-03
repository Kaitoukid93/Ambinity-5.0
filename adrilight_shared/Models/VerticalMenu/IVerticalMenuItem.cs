
using System.ComponentModel;


namespace adrilight_shared.Model.VerticalMenu
{
    public interface IVerticalMenuItem
    {
        string Name { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
    }
}
