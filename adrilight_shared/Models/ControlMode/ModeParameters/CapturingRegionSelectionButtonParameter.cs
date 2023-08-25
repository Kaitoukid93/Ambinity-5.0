using Newtonsoft.Json;
using System.Windows;

namespace adrilight.Models.ControlMode.ModeParameters
{
    public class CapturingRegionSelectionButtonParameter : BaseButtonParameter // parameter specific for lighting control
    {
        private CapturingRegion _capturingRegion;
        private int _capturingSourceIndex;
        private Rect _capturingSourceRect = new Rect(0, 0, 240, 135);
        private string _capturingSourceName;
        public CapturingRegionSelectionButtonParameter()
        {

        }
        public CapturingRegionSelectionButtonParameter(CapturingRegion defaultRegion, string commandParameter)
        {
            CommandParameter = commandParameter;
            CapturingRegion = defaultRegion;
            Template = ModeParameterTemplateEnum.PushButtonAction;
            ParamType = ModeParameterEnum.CapturingRegion;

        }
        [JsonIgnore]
        public override PreviewableContent PreviewContent => GetPreviewContent();
        public CapturingRegion CapturingRegion { get => _capturingRegion; set { Set(() => CapturingRegion, ref _capturingRegion, value); RaisePropertyChanged(nameof(PreviewContent)); } }
        public int CapturingSourceIndex { get => _capturingSourceIndex; set { Set(() => CapturingSourceIndex, ref _capturingSourceIndex, value > 0 ? value : 0); } }
        public Rect CapturingSourceRect { get => _capturingSourceRect; set { Set(() => CapturingSourceRect, ref _capturingSourceRect, value); RaisePropertyChanged(nameof(PreviewContent)); } }
        public string CapturingSourceName { get => _capturingSourceName; set { Set(() => CapturingSourceName, ref _capturingSourceName, value); RaisePropertyChanged(nameof(PreviewContent)); } }
        private PreviewableContent GetPreviewContent()
        {
            var previewContent = new CapturingRegionPreview();
            previewContent.Canvas = CapturingSourceRect;
            previewContent.Region = new Rect(
                CapturingRegion.ScaleX * CapturingSourceRect.Width,
                CapturingRegion.ScaleY * CapturingSourceRect.Height,
                CapturingRegion.ScaleWidth * CapturingSourceRect.Width,
                CapturingRegion.ScaleHeight * CapturingSourceRect.Height);
            previewContent.SourceName = CapturingSourceName;
            return previewContent;
        }
    }
}

