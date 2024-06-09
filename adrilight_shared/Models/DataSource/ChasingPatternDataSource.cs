using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using static System.Net.WebRequestMethods;

namespace adrilight_shared.Models.DataSource
{
    public class ChasingPatternDataSource : DataSourceBase
    {
        public ChasingPatternDataSource()
        {
            FolderPath = ChasingPatternsCollectionFolderPath;
            Name = "ChasingPatterns";
            LoadData();
        }
        public override void AddItem(IGenericCollectionItem item)
        {
            base.AddItem(item);
        }
        public override void LoadData()
        {
            var files = Directory.GetFiles(CollectionPath);
            foreach (var file in files)
            {
                var data = new ChasingPattern()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = "xxx",
                    Type = ChasingPatternTypeEnum.BlacknWhite,
                    LocalPath = file,
                    InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info")

                };
                Items.Add(data);
            }
        }
    }
}
