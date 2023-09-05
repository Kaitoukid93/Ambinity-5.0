using System;
using System.Collections.Generic;
using System.IO;

namespace adrilight_shared.Models.Store
{
    public class StoreCategory
    {
        public string Name { get; set; }
        public List<StoreFilterModel> DefaultFilters { get; set; }
        public Type DataType { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string LocalFolderPath { get; set; }
        public string OnlineFolderPath { get; set; }
        public string LocalInfoPath => Path.Combine(LocalFolderPath, "info");
        public string LocalCollectionPath => Path.Combine(LocalFolderPath, "collection");
    }
}
