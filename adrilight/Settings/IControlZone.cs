﻿using adrilight.Util;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace adrilight
{
    public interface IControlZone : INotifyPropertyChanged
    {
        /// <summary>
        /// this control zone contains control modes and params
        /// </summary>
        string Name { get; set; }
        string Description { get; set; }
        BitmapImage Thumb { get; set; } // this level of property using bitmap image
        List<IControlMode> AvailableControlMode { get; set; }
        IControlMode CurrentActiveControlMode { get; }
        IControlMode MaskedControlMode { get; set; }
        string ZoneUID { get; set; }
        bool IsEnabled { get; set; }
        int CurrentActiveControlModeIndex { get; set; }
        bool IsInControlGroup { get; set; }
        void UpdateSizeByChild(bool withPoint);
    }
}