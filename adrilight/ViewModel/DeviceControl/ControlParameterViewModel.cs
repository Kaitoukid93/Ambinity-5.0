using adrilight_shared.Enums;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.ControlMode.ModeParameters;
using GalaSoft.MvvmLight;
using adrilight_shared.Models.RelayCommand;
using System.Windows.Input;

namespace adrilight.ViewModel.DeviceControl
{
    /// <summary>
    /// Handle command and view logic for parameter, resolve stupid complex from legacy mainviewviewmodel
    /// </summary>
    public class ControlParameterViewModel : ViewModelBase
    {
        public ControlParameterViewModel()
        {

        }
        public ModeParameterTemplateEnum TemplateSelector { get; set; }
        public IModeParameter Parameter { get; set; }
        public ModeParameterEnum Type { get; set; }

        #region Methods
        public void Init(IModeParameter param)
        {
            Parameter = param;
            TemplateSelector = param.Template;
            Type = param.ParamType;
        }
        public void CommandSetup()
        {
            ParameterClickCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteparameterClick(p);
            });
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

        private void OpenRegionSelectionWindow(string type)
        {

        }
        private void OpenAudioDeviceSelectionWindow()
        {

        }

        #endregion

        #region Icommand
        public ICommand ParameterClickCommand { get; set; }
        public ICommand SubParameterButtonClickCommand { get; set; }

        public ICommand DeleteButtonClickCommand { get; set; }

        public ICommand GetMoreValueButtonClickCommand { get; set; }
        public ICommand ShowMoreButtonClickCommand { get; set; }

        #endregion
    }
}
