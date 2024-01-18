using adrilight.ViewModel;
using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceCollectionView
    {
        public DeviceCollectionView()
        {
            InitializeComponent();

        }
    }
    public class DeviceCollectionViewPage : ISelectablePage
    {
        private readonly Lazy<DeviceCollectionView> lazyContent;

        public DeviceCollectionViewPage(Lazy<DeviceCollectionView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 0;

        public string PageName => "Devices";
        public string Geometry => "";

        public object Content { get => lazyContent.Value; }
    }
}
