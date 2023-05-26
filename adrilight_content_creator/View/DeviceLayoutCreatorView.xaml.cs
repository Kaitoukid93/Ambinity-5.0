using adrilight.Helpers;
using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class DeviceLayoutCreatorView : UserControl
    {
        public DeviceLayoutCreatorView()
        {
            InitializeComponent();
        }
        public class DeviceLayoutCreatorSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<DeviceLayoutCreatorView> lazyContent;

            public DeviceLayoutCreatorSelectableViewPart(Lazy<DeviceLayoutCreatorView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 100;

            public string ViewPartName => "Device";
            public string Geometry => "slaveDevice";

            public object Content { get => lazyContent.Value; }
        }

        private void OnScrolling(object sender, System.Windows.RoutedEventArgs e)
        {
            //AttachedAdorner.OnScrolling();
        }

        private void Canvas_Keydown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key==System.Windows.Input.Key.LeftCtrl)
            {
                layoutCanvas.CanSelectMultipleItems = true;
                e.Handled = true;
            }
        }

        private void Canvas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftCtrl)
            {
                layoutCanvas.CanSelectMultipleItems = false;
                e.Handled = true;
            }
        }
    }
}

