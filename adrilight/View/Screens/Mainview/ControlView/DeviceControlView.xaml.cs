using adrilight_shared.Models.Stores;
using System;
using System.Windows.Controls;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for DeviceLigtingControl.xaml
    /// </summary>
    public partial class DeviceControlView : UserControl
    {


        public DeviceControlView()
        {
            InitializeComponent();
        }


        public class DeviceControlViewSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<DeviceControlView> lazyContent;

            public DeviceControlViewSelectableViewPart(Lazy<DeviceControlView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 900;
            public string Geometry => "animation";
            public string ViewPartName => "Device Control View";

            public object Content { get => lazyContent.Value; }
        }
    }
}
