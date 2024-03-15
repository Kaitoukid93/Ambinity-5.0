using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class DeviceUtil : UserControl
    {
        public DeviceUtil()
        {
            InitializeComponent();
            this.DataContext = new DeviceUtilViewModel();
        }
        public class DeviceUtilSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<DeviceUtil> lazyContent;

            public DeviceUtilSelectableViewPart(Lazy<DeviceUtil> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 600;

            public string ViewPartName => "DeviceUtil";
            public string Geometry => "auto";

            public object Content { get => lazyContent.Value; }
        }

    }
}

