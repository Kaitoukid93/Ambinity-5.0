using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.Stores
{
    public interface ISelectableViewPart
    {
        int Order { get; }
        string ViewPartName { get; }
        string Geometry { get; }
        object Content { get; }
    }
}
