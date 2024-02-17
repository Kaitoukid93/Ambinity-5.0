﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight_shared.ViewModel
{
    public class ProgressDialogViewModel : ViewModelBase
    {
        public ProgressDialogViewModel(string header, string content, string geometry)
        {
            Header = header;
            Geometry = geometry;
            Content = content;
        }
        private Visibility _progressbarVisibility = Visibility.Collapsed;
        private Visibility _successMesageVisibility = Visibility.Collapsed;
        private int _value;
        private string _successMessage;
        private string _primaryActionButtonContent;
        private string _secondaryActionButtonContent;
        public int Value { get => _value; set { Set(() => Value, ref _value, value); } }
        public string Header { get; set; }
        public string Content { get; set; }
        public string PrimaryActionButtonContent { get => _primaryActionButtonContent; set { Set(() => PrimaryActionButtonContent, ref _primaryActionButtonContent, value); } }
        public string SecondaryActionButtonContent { get => _secondaryActionButtonContent; set { Set(() => SecondaryActionButtonContent, ref _secondaryActionButtonContent, value); } }
        public string SuccessMessage { get => _successMessage; set { Set(() => SuccessMessage, ref _successMessage, value); } }
        public Visibility SuccessMesageVisibility { get => _successMesageVisibility; set { Set(() => SuccessMesageVisibility, ref _successMesageVisibility, value); } }
        public Visibility ProgressBarVisibility { get => _progressbarVisibility; set { Set(() => ProgressBarVisibility, ref _progressbarVisibility, value); } }
        public string Geometry { get; set; } = "rename";
    }
}
