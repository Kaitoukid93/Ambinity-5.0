using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel
{
    public interface ISelectablePage
    {
        int Order { get; }
        string PageName { get; }
        string Geometry { get; }
        object Content { get; }
        
    }
}
