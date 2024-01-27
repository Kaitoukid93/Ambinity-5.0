using adrilight.ViewModel;
using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceLoadingView
    {
        public DeviceLoadingView()
        {
            InitializeComponent();

        }
    }
    public class DeviceLoadingViewPage : ISelectablePage
    {
        private readonly Lazy<DeviceLoadingView> lazyContent;

        public DeviceLoadingViewPage(Lazy<DeviceLoadingView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 5;

        public string PageName => "Loading";
        public string Geometry => "";

        public object Content { get => lazyContent.Value; }
    }
}
