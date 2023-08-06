namespace adrilight.Util.ModeParameters
{
    public class AudioDeviceSelectionButtonParameter : BaseButtonParameter // parameter specific for lighting control
    {
        private int _capturingSourceIndex;
        private string _capturingSourceName;
        public AudioDeviceSelectionButtonParameter()
        {

        }
        public AudioDeviceSelectionButtonParameter(string commandParameter)
        {

            Template = ModeParameterTemplateEnum.PushButtonAction;
            CommandParameter = commandParameter;

        }
        public int CapturingSourceIndex { get => _capturingSourceIndex; set { Set(() => CapturingSourceIndex, ref _capturingSourceIndex, value > 0 ? value : 0); } }
        public string CapturingSourceName { get => _capturingSourceName; set { Set(() => CapturingSourceName, ref _capturingSourceName, value); RaisePropertyChanged(nameof(PreviewContent)); } }
    }
}

