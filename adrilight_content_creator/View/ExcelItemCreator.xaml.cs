using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class ExcelItemCreator : UserControl
    {
        public ExcelItemCreator()
        {
            InitializeComponent();
        }
        public class ExcelItemCreatorSelectableViewPart
        {
            private readonly Lazy<ExcelItemCreator> lazyContent;

            public ExcelItemCreatorSelectableViewPart(Lazy<ExcelItemCreator> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 600;

            public string ViewPartName => "Excel+";
            public string Geometry => "auto";

            public object Content { get => lazyContent.Value; }
        }

    }
}

