using adrilight.ViewModel;
using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceAdvanceSettingsView
    {
        public DeviceAdvanceSettingsView()
        {
            InitializeComponent();

        }
    }
    public class DeviceAdvanceSettingsViewPage : ISelectablePage
    {
        private readonly Lazy<DeviceAdvanceSettingsView> lazyContent;

        public DeviceAdvanceSettingsViewPage(Lazy<DeviceAdvanceSettingsView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 10;

        public string PageName => "Devices Advance Settings";
        public string Geometry => "";

        public object Content { get => lazyContent.Value; }
    }
}
