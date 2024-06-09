using adrilight_shared.Enums;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.ControlMode.ModeParameters;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.RelayCommand;
using System.Windows.Input;
using adrilight_shared.Models.ItemsCollection;
using System.Linq;
using System.Collections.ObjectModel;
using adrilight_shared.Services;
using System.Collections.Generic;
using System;

namespace adrilight.ViewModel.DeviceControl
{
    /// <summary>
    /// Handle command and view logic for parameter, resolve stupid complex from legacy mainviewviewmodel
    /// </summary>
    public abstract class ControlParameterViewModelBase : ViewModelBase
    {
        public ControlParameterViewModelBase()
        {
        }
        public ModeParameterTemplateEnum TemplateSelector { get; set; }
        public IModeParameter Parameter { get; set; }
        public ModeParameterEnum Type { get; set; }
        public DialogService DialogService { get; set; }
        public IList<IDataSource> DataSources { get; set; }
        public virtual void Init(IModeParameter param)
        {
            CommandSetup();
            Parameter = param;
            TemplateSelector = param.Template;
            Type = param.ParamType;
        }
        public virtual void CommandSetup()
        {
            ParameterClickCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteparameterClick(p);
            });

        }
        private void OpenRegionSelectionWindow(string type)
        {

        }
        private void OpenAudioDeviceSelectionWindow()
        {

        }
        private void ExecuteparameterClick(string parameter)
        {
            switch (parameter)
            {
                case "screenRegionSelection":
                    OpenRegionSelectionWindow("screen");
                    break;
                case "gifRegionSelection":
                    OpenRegionSelectionWindow("gif");
                    break;

                case "audioDevice":
                    OpenAudioDeviceSelectionWindow();
                    break;
            }
        }
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }
        public ICommand ParameterClickCommand { get; set; }
    }
}
