using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class DeviceExporterView : UserControl
    {
        public DeviceExporterView()
        {
            InitializeComponent();
        }
        public class DeviceExporterViewSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<DeviceExporterView> lazyContent;

            public DeviceExporterViewSelectableViewPart(Lazy<DeviceExporterView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 150;

            public string ViewPartName => "Device+";
            public string Geometry => "mouse";

            public object Content { get => lazyContent.Value; }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = sender as Button;
        }



        private void ListBox1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            userAddDeviceList.SelectedIndex = -1;
        }

        private void ListBox2_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            existedDeviceList.SelectedIndex = -1;
        }
    }
}

