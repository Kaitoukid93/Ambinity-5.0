using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models
{
    public class DataCollection : ViewModelBase
    {
        public DataCollection()
        {

        }
        public DataCollection(string name)
        {
            Name = name;
            Items = new ObservableCollection<IGenericCollectionItem>();
        }
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        private bool _showManagerToolBar;
        public bool ShowManagerToolBar
        {
            get
            {
                return _showManagerToolBar;
            }

            set
            {
                _showManagerToolBar = value;
                RaisePropertyChanged();
            }
        }
        private string _currentView;
        public string CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                _currentView = value;
                RaisePropertyChanged();
            }
        }
        public string Name { get; set; }
        public void RemoveItems()
        {
            if (Items == null)
                return;
            var itemsToRemove = new List<IGenericCollectionItem>();

            foreach (var i in Items)
            {
                if (i.IsEditing || i.IsChecked)
                    itemsToRemove.Add(i);
            }
            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }

        }
        public void AddItems(IGenericCollectionItem item)
        {
            if (Items == null)
                Items = new ObservableCollection<IGenericCollectionItem>();
            Items.Add(item);
        }
    }
}
