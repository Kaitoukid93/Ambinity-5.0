using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace adrilight_shared.Models.Store
{
    public class StoreCategory : ViewModelBase
    {
        private ObservableCollection<StoreFilterModel> _defaultFilters;
        public string Name { get; set; }
        public ObservableCollection<StoreFilterModel> DefaultFilters { get => _defaultFilters; set { Set(() => DefaultFilters, ref _defaultFilters, value); } }
        public Type DataType { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string LocalFolderPath { get; set; }
        public string OnlineFolderPath { get; set; }
        public string LocalInfoPath => Path.Combine(LocalFolderPath, "info");
        public string LocalCollectionPath => Path.Combine(LocalFolderPath, "collection");
    }
}
