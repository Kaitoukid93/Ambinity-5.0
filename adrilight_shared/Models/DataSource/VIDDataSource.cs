using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.DataSource
{
    public class VIDDataSource:DataSourceBase
    {
        public VIDDataSource()
        {
            FolderPath = VIDCollectionFolderPath;
            Name = "VID";
            LoadData();
        }
        public override void LoadData()
        {
            var files = Directory.GetFiles(CollectionPath);
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var vidData = DeserializeFromStream<VIDDataModel>(stream);
                    vidData.LocalPath = file;
                    vidData.IsDeleteable = vidData.ExecutionType == VIDType.PositonGeneratedID ? false : true;
                    vidData.InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                    Items.Add(vidData);
                }
            }
        }
    }
}
