using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class FilterCreator : UserControl
    {
        public FilterCreator()
        {
            InitializeComponent();
        }
        public class FilterCreatorSelectableViewPart : ISelectableViewPart
        {
            private readonly Lazy<FilterCreator> lazyContent;

            public FilterCreatorSelectableViewPart(Lazy<FilterCreator> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 300;

            public string ViewPartName => "Filter+";
            public string Geometry => "auto";

            public object Content { get => lazyContent.Value; }
        }

    }
}

