using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.ControlMode.ModeParameters;

namespace adrilight_shared.Models.DataSource
{
    public abstract class DataSourceBase : ViewModelBase, IDataSource
    {
        public string appData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        #region database local folder paths
        public string PalettesCollectionFolderPath => Path.Combine(appData, "ColorPalettes");
        public string ChasingPatternsCollectionFolderPath => Path.Combine(appData, "ChasingPatterns");
        public string ColorsCollectionFolderPath => Path.Combine(appData, "Colors");
        public string GifsCollectionFolderPath => Path.Combine(appData, "Gifs");
        public string VIDCollectionFolderPath => Path.Combine(appData, "VID");
        public string MIDCollectionFolderPath => Path.Combine(appData, "MID");
        #endregion
        public DataSourceBase()
        {
            Items = new ObservableCollection<IGenericCollectionItem>();
            
        }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string CollectionPath => Path.Combine(FolderPath, "collection");
        public string InfoPath => Path.Combine(FolderPath, "info");
        public ObservableCollection<IGenericCollectionItem> Items { get; set; }
        public virtual void RemoveItem(IGenericCollectionItem item)
        {

        }
        public virtual void RemoveSelectedItems()
        {
            foreach (var item in Items.Where(i => i.IsChecked).ToList())
            {
                Items.Remove(item);
                try
                {
                    if (item.LocalPath != null && File.Exists(item.LocalPath))
                    {
                        File.Delete(item.LocalPath);
                        if (File.Exists(item.InfoPath))
                            File.Delete(item.InfoPath);
                    }
                }
                catch (Exception ex)
                {
                    //item is unable to remove
                }
            }
        }
        public virtual void AddItem(IGenericCollectionItem item)
        {
            Items.Add(item);
        }
        public virtual bool InsertItem(IGenericCollectionItem item)
        {
            Items.Insert(0,item);
            return true;
        }
        public virtual void SaveData()
        {
            //var result = SaveAvailableValues();
        }
        public virtual void LoadData()
        {

        }
        public virtual void CreateDefault()
        {

        }
        public static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
      
    }
}
