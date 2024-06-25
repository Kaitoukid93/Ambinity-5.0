﻿using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace adrilight_shared.Models.DataSource
{
    public class GIFDataSource : DataSourceBase
    {
        public GIFDataSource()
        {
            FolderPath = GifsCollectionFolderPath;
            Name = "Gifs";
            if (!Directory.Exists(FolderPath) || !Directory.Exists(CollectionPath))
            {
                CreateDefault();
            }
            LoadData();
        }
        public override bool InsertItem(IGenericCollectionItem item)
        {
            var path = Path.Combine(CollectionPath, item.Name);
            if(File.Exists(path))
            {
                return false;
            }
            else
            {
                base.InsertItem(item);
                var gif = item as Gif;
                File.Copy(gif.LocalPath, path);
                return true;
            }
            
        }
        public override void LoadData()
        {
            Items?.Clear();
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
        public override void CreateDefault()
        {
            Directory.CreateDirectory(FolderPath);
            Directory.CreateDirectory(CollectionPath);
            //get data from resource file and copy to local folder
            var resourceHlprs = new ResourceHelpers();
            var allResourceNames = resourceHlprs.GetResourceFileNames();
            foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".gif")))
            {
                var name = resourceHlprs.GetResourceFileName(resourceName);
                resourceHlprs.CopyResource(resourceName, Path.Combine(CollectionPath, name));
            }
            var config = new ResourceLoaderConfig(nameof(Gif), DeserializeMethodEnum.Files);
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText(Path.Combine(FolderPath, "config.json"), configJson);

        }
    }
}