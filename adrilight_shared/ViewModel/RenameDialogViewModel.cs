﻿using GalaSoft.MvvmLight;

namespace adrilight_shared.ViewModel
{
    public class RenameDialogViewModel : ViewModelBase
    {
        public RenameDialogViewModel(string header, string content, string geometry)
        {
            Header = header;
            Content = content;
            Geometry = geometry;
        }
        public string Content { get; set; }
        public string Header { get; set; }
        public string Geometry { get; set; } = "rename";
    }
}
