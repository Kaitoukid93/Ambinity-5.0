using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace adrilight_shared.Models.DataSource
{
    public class StaticColorDataSource : DataSourceBase
    {
        public StaticColorDataSource()
        {
            FolderPath = ColorsCollectionFolderPath;
            Name = "Colors";
            LoadData();
        }
        public override void AddItem(IGenericCollectionItem item)
        {
            base.AddItem(item);
        }
        public override void LoadData()
        {
            var valuesJson = File.ReadAllText(Path.Combine(FolderPath, "collection.json"));
            JsonConvert.DeserializeObject<List<ColorCard>>(valuesJson).ForEach(c =>
            {
                Items.Add(c);
            });
        }
    }
}
