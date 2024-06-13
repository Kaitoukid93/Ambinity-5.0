using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
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
    public class VIDDataSource:DataSourceBase
    {
        public VIDDataSource()
        {
            FolderPath = VIDCollectionFolderPath;
            Name = "VID";
            if (!Directory.Exists(FolderPath) || Directory.Exists(CollectionPath))
            {
                CreateDefault();
            }
            LoadData();
        }
        public override bool InsertItem(IGenericCollectionItem item)
        {
            var path = Path.Combine(CollectionPath, item.Name + ".json");
            if(File.Exists(path))
            {
                return false;
            }
            else
            {
                item.LocalPath = path;
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
                    var vidData = DeserializeFromStream<VIDDataModel>(stream);
                    vidData.LocalPath = file;
                    vidData.IsDeleteable = vidData.ExecutionType == VIDType.PositonGeneratedID ? false : true;
                    vidData.InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info");
                    Items.Add(vidData);
                }
            }
        }
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            Directory.CreateDirectory(CollectionPath);
            var vidCollection = new List<VIDDataModel>();
            var lef2Right = new VIDDataModel()
            {
                Name = "Trái sang phải",
                IsDeleteable = false,
                Geometry = "left2right",
                Description = "Màu chạy từ trái sang phải",
                ExecutionType = VIDType.PositonGeneratedID,
                Dirrection = VIDDirrection.left2right
            };
            var right2Left = new VIDDataModel()
            {
                Name = "Phải sang trái",
                IsDeleteable = false,
                Geometry = "right2left",
                Description = "Màu chạy từ phải sang trái",
                ExecutionType = VIDType.PositonGeneratedID,
                Dirrection = VIDDirrection.right2left
            };
            var up2Down = new VIDDataModel()
            {
                Name = "Trên xuống dưới",
                IsDeleteable = false,
                Geometry = "topdown",
                Description = "Màu chạy từ trên xuống dưới",
                ExecutionType = VIDType.PositonGeneratedID,
                Dirrection = VIDDirrection.top2bot
            };
            var down2Up = new VIDDataModel()
            {
                Name = "Dưới lên trên",
                IsDeleteable = false,
                Geometry = "bottomup",
                Description = "Màu chạy từ dưới lên trên",
                ExecutionType = VIDType.PositonGeneratedID,
                Dirrection = VIDDirrection.bot2top
            };
            var linear = new VIDDataModel()
            {
                Name = "Tuyến tính",
                Geometry = "back",
                Description = "Màu chạy theo thứ tự LED",
                ExecutionType = VIDType.PositonGeneratedID,
                Dirrection = VIDDirrection.linear
            };
            vidCollection.Add(lef2Right);
            vidCollection.Add(right2Left);
            vidCollection.Add(up2Down);
            vidCollection.Add(down2Up);
            vidCollection.Add(linear);
            foreach (var vid in vidCollection)
            {
                var json = JsonConvert.SerializeObject(vid);
                File.WriteAllText(Path.Combine(CollectionPath, vid.Name + ".json"), json);
            }
            //coppy all internal palettes to local
            var config = new ResourceLoaderConfig(nameof(VIDDataModel), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);
        }
    }
}
