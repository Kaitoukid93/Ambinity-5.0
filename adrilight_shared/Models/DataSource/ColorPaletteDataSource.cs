using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace adrilight_shared.Models.DataSource
{
    public class ColorPaletteDataSource : DataSourceBase
    {
        public ColorPaletteDataSource()
        {
            FolderPath = PalettesCollectionFolderPath;
            Name = "ColorPalettes";
            if (!Directory.Exists(FolderPath) || !Directory.Exists(CollectionPath))
            {
                CreateDefault();
            }
            LoadData();
        }
        public override bool InsertItem(IGenericCollectionItem item)
        {

            item.LocalPath = Path.Combine(CollectionPath, item.Name + ".col");
            if(File.Exists(item.LocalPath))
            {
                return false;
            }
            else
            {
                base.InsertItem(item);
                JsonHelpers.WriteSimpleJson(item, item.LocalPath);
                return true;
            }
            
        }
        public override void LoadData()
        {
            Items?.Clear();
            var files = Directory.GetFiles(CollectionPath);
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var colorPalette = DeserializeFromStream<ColorPalette>(stream);
                    colorPalette.LocalPath = file;
                    colorPalette.InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info");

                    Items.Add(colorPalette);
                }

            }
        }
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            Directory.CreateDirectory(CollectionPath);
            var resourceHlprs = new ResourceHelpers();
            var allResourceNames = resourceHlprs.GetResourceFileNames();
            foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".col")))
            {
                var name = resourceHlprs.GetResourceFileName(resourceName);
                resourceHlprs.CopyResource(resourceName, Path.Combine(CollectionPath, name));
            }
            var config = new ResourceLoaderConfig(nameof(ColorPalette), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);
        }
    }
}
