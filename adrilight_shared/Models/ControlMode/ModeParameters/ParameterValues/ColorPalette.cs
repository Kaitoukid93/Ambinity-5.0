﻿using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class ColorPalette : ViewModelBase, IParameterValue , IGenericCollectionItem
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
            Colors = new System.Windows.Media.Color[colorNum];
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
        private bool _isDeleteable = true;
        private bool _isVisible = true;
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }
        [JsonIgnore]
        public bool IsEditing { get; set; }
        [JsonIgnore]
        public bool IsPinned { get; set; }

        public void SetColor(int index, System.Windows.Media.Color color)
        {
            Colors[index] = color;
            RaisePropertyChanged(nameof(Colors));
        }
    }
}
