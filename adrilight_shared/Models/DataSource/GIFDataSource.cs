using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace adrilight_shared.Models.DataSource
{
    public class GIFDataSource : DataSourceBase
    {
        public GIFDataSource()
        {
            FolderPath = GifsCollectionFolderPath;
            Name = "Gifs";
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
                var data = new Gif()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = "Ambino Default Gif Collection",
                    LocalPath = file,
                    InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info")
                };
                Items.Add(data);
            }
        }
    }
}
