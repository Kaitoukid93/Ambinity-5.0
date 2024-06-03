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

namespace adrilight.View.Screens.Store
{
    /// <summary>
    /// Interaction logic for StoreItemsCollectionView.xaml
    /// </summary>
    public partial class StoreItemsCollectionView : UserControl
    {
        public StoreItemsCollectionView()
        {
            InitializeComponent();
        }
        public class StoreItemsCollectionViewPage : ISelectablePage
        {
            private readonly Lazy<StoreItemsCollectionView> lazyContent;

            public StoreItemsCollectionViewPage(Lazy<StoreItemsCollectionView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Store Items Collection View";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }


    }
}
