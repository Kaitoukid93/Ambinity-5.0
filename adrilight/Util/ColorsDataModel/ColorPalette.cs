using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Windows.Media;

namespace adrilight.Util
{
    internal class ColorPalette : ViewModelBase, IParameterValue
    {
        public ColorPalette(string name, string owner, string type, string description, System.Windows.Media.Color[] colors)
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            Colors = colors;


        }
        public ColorPalette(int colorNum)
        {
            Colors = new Color[colorNum];
            GUID = Guid.NewGuid().ToString();
        }
        public ColorPalette()
        {

        }
        public string Name { get; set; }
        public string Owner { get; set; }
        public System.Windows.Media.Color[] Colors { get; set; }
        public string Type { get; set; }
        public string GUID { get; set; }
        public string Description { get; set; }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        public void SetColor(int index, Color color)
        {
            Colors[index] = color;
            RaisePropertyChanged(nameof(Colors));
        }
    }
}
