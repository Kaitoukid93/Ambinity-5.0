using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace adrilight_shared.Models.DataSource
{
    public class ChasingPatternDataSource : DataSourceBase
    {
        public ChasingPatternDataSource()
        {
            FolderPath = ChasingPatternsCollectionFolderPath;
            Name = "ChasingPatterns";
            if (!Directory.Exists(FolderPath) || !Directory.Exists(CollectionPath))
            {
                CreateDefault();
            }
            LoadData();
        }
        public override bool InsertItem(IGenericCollectionItem item)
        {

            var pattern = item as ChasingPattern;
            var path = Path.Combine(CollectionPath, item.Name);
            if (System.IO.File.Exists(path))
            {
                //show error dialog
                return false;
            }
            else
            {
                System.IO.File.Copy(pattern.LocalPath,path);
                item.LocalPath = path;  
                base.InsertItem(item);
                return true;
            }
            
        }
        public override void LoadData()
        {
            Items?.Clear();
            var files = Directory.GetFiles(CollectionPath);
            foreach (var file in files)
            {
                var data = new ChasingPattern()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = "xxx",
                    Type = ChasingPatternTypeEnum.BlacknWhite,
                    LocalPath = file,
                    ThumbPath = Path.Combine(CollectionPath,"thumb", Path.GetFileNameWithoutExtension(file) + ".png"),
                    InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info")

                };
                Items.Add(data);
            }
        }
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            Directory.CreateDirectory(CollectionPath);
            var resourceHlprs = new ResourceHelpers();
            var allResourceNames = resourceHlprs.GetResourceFileNames();
            foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".AML")))
            {

                var name = resourceHlprs.GetResourceFileName(resourceName);
                resourceHlprs.CopyResource(resourceName, Path.Combine(CollectionPath, name));
            }
            var config = new ResourceLoaderConfig(nameof(ChasingPattern), DeserializeMethodEnum.Files);
            var configJson = JsonConvert.SerializeObject(config);
            System.IO.File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);
            //copy default chasing pattern from resource
        }
    }
}
