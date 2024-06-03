using adrilight.ViewModel;
using System;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class StoreHomePageView
    {
        public StoreHomePageView()
        {
            InitializeComponent();

        }


        public class StoreHomePageViewPage : ISelectablePage
        {
            private readonly Lazy<StoreHomePageView> lazyContent;

            public StoreHomePageViewPage(Lazy<StoreHomePageView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Store Home Page";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }



    }
}
