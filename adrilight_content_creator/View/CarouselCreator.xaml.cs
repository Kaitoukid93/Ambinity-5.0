using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class CarouselCreator : UserControl
    {
        public CarouselCreator()
        {
            InitializeComponent();
        }
        public class CarouselCreatorSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<CarouselCreator> lazyContent;

            public CarouselCreatorSelectableViewPart(Lazy<CarouselCreator> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 300;

            public string ViewPartName => "Carousel+";
            public string Geometry => "addImage";

            public object Content { get => lazyContent.Value; }
        }

    }
}

