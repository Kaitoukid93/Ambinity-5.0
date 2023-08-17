using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        public class HomeViewSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<HomeView> lazyContent;

            public HomeViewSelectableViewPart(Lazy<HomeView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 0;

            public string ViewPartName => "Home";
            public string Geometry => "contentCreatorIcon";

            public object Content { get => lazyContent.Value; }
        }


    }
}

