using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_content_creator.ViewModel
{
    public interface ISelectableViewPart
    {
        int Order { get; }
        string ViewPartName { get; }
        string Geometry { get; }
        object Content { get; }
    }
}
