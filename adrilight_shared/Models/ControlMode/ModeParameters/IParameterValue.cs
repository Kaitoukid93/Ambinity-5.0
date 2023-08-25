﻿using System.ComponentModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public interface IParameterValue : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
        bool IsChecked { get; set; }
        string LocalPath { get; set; }
    }
}