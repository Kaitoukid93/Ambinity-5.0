namespace adrilight.Util.ModeParameters
{
    public class AudioDeviceSelectionButtonParameter : BaseButtonParameter // parameter specific for lighting control
    {

        public AudioDeviceSelectionButtonParameter()
        {

        }
        public AudioDeviceSelectionButtonParameter(string commandParameter)
        {

            Template = ModeParameterTemplateEnum.PushButtonAction;
            CommandParameter = commandParameter;

        }

    }
}

