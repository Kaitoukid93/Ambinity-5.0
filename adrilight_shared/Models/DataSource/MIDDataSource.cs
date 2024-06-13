using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_shared.Models.DataSource
{
    public class MIDDataSource: DataSourceBase
    {
        public MIDDataSource()
        {
            FolderPath = MIDCollectionFolderPath;
            Name = "MID";
            if (!Directory.Exists(FolderPath) || !Directory.Exists(CollectionPath))
            {
                CreateDefault();
            }
            LoadData();
        }

        public override void LoadData()
        {
            Items?.Clear();
            var files = Directory.GetFiles(CollectionPath);
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var midData = DeserializeFromStream<MIDDataModel>(stream);
                    midData.LocalPath = file;
                    midData.InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                    Items.Add(midData);
                }
            }
        }
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            Directory.CreateDirectory(CollectionPath);
            Directory.CreateDirectory(MIDCollectionFolderPath);
            var midCollection = new List<MIDDataModel>();
            var low = new MIDDataModel()
            {
                Name = "Low",
                Description = "Trung bình của các tần số thấp (Bass)",
                ExecutionType = VIDType.PositonGeneratedID,
                Frequency = MIDFrequency.Low
            };
            var middle = new MIDDataModel()
            {
                Name = "Mid",
                Description = "Trung bình của các tần số trung (Mid)",
                ExecutionType = VIDType.PositonGeneratedID,
                Frequency = MIDFrequency.Middle
            };
            var high = new MIDDataModel()
            {
                Name = "High",
                Description = "Trung bình của các tần số cao (Treble)",
                ExecutionType = VIDType.PositonGeneratedID,
                Frequency = MIDFrequency.High
            };

            var custom = new MIDDataModel()
            {
                Name = "Custom",
                Description = "Chọn Tần số bằng tay",
                ExecutionType = VIDType.PredefinedID
            };
            midCollection.Add(low);
            midCollection.Add(middle);
            midCollection.Add(high);
            midCollection.Add(custom);
            foreach (var mid in midCollection)
            {
                var json = JsonConvert.SerializeObject(mid);
                File.WriteAllText(Path.Combine(CollectionPath, mid.Name + ".json"), json);
            }
            //coppy all internal palettes to local
            var config = new ResourceLoaderConfig(nameof(MIDDataModel), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);

        }
    }
}
