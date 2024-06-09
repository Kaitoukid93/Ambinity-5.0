using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.ItemsCollection;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace adrilight_shared.Models.DataSource
{
    public class ColorPaletteDataSource:DataSourceBase
    {
        public ColorPaletteDataSource()
        {
            FolderPath = PalettesCollectionFolderPath;
            Name = "ColorPalettes";
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
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var colorPalette = DeserializeFromStream<ColorPalette>(stream);
                    colorPalette.LocalPath = file;
                    colorPalette.InfoPath = Path.Combine(InfoPath, Path.GetFileNameWithoutExtension(file) + ".info");

                    Items.Add(colorPalette);
                }

            }
        }
    }
}
