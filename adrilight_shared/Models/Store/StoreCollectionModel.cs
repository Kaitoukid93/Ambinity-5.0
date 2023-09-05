using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace adrilight_shared.Models.Store
{
    public class StoreCollectionModel : ViewModelBase
    {

        public StoreCollectionModel()
        {

        }
        public List<string> ListItemAddress { get; set; }
        public string Name { get; set; }

    }
}
