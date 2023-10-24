using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_content_creator.Model
{
    public interface IExcelItemModel
    {
        string Name { get; set; }
        string Description { get; set; }
        ItemTypeEnum ItemType { get; set; }
        ItemStatusEnum Status { get; set; }

    }
}
