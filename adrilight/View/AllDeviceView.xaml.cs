using adrilight.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for AllDeviceView.xaml
    /// </summary>
    public partial class AllDeviceView : UserControl
    {
        public AllDeviceView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        public class AllDeviceViewSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<AllDeviceView> lazyContent;

            public AllDeviceViewSelectableViewPart(Lazy<AllDeviceView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 100;

            public string ViewPartName => "All Device View";

            public object Content { get => lazyContent.Value; }
        }

    }
}
