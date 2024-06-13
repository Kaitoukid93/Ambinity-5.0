using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
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
            if (!Directory.Exists(FolderPath))
            {
                CreateDefault();
            }
            LoadData();
        }
        public override bool InsertItem(IGenericCollectionItem item)
        {
            base.InsertItem(item);
            JsonHelpers.WriteSimpleJson(Items, Path.Combine(FolderPath, "collection.json"));
            return true;
        }
        public override void LoadData()
        {
            Items?.Clear();
            var valuesJson = File.ReadAllText(Path.Combine(FolderPath, "collection.json"));
            JsonConvert.DeserializeObject<List<ColorCard>>(valuesJson).ForEach(c =>
            {
                Items.Add(c);
            });
        }
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            //get data from resource file and copy to local folder
            var colorCollectionResourcePath = "adrilight_shared.Resources.Colors.ColorCollection.json";
            var resourceHlprs = new ResourceHelpers();
            resourceHlprs.CopyResource(colorCollectionResourcePath, Path.Combine(FolderPath, "collection.json"));
            //Create deserialize config
            var config = new ResourceLoaderConfig(nameof(ColorCard), DeserializeMethodEnum.SingleJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);
        }
    }
}
