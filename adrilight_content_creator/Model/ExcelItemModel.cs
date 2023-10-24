using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_content_creator.Model
{
    internal class ExcelItemModel : ViewModelBase, IExcelItemModel
    {
        private string _name;
        private string _description;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Description { get => _description; set { Set(() => Description, ref _description, value); } }
        public ItemTypeEnum ItemType { get; set; }
        public ItemStatusEnum Status { get; set; }
    }
}
