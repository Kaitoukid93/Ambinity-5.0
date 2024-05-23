using adrilight_shared.Models.Stores;
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
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }
        public class DashboardViewSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<DashboardView> lazyContent;

            public DashboardViewSelectableViewPart(Lazy<DashboardView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 900;
            public string Geometry => "dashboard";
            public string ViewPartName => "Dashboard View";

            public object Content { get => lazyContent.Value; }
        }
    }
}
