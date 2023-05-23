
using adrilight_content_creator.ViewModel;
using System;
using System.Windows.Controls;


namespace adrilight_content_creator.View
{
    /// <summary>
    /// Interaction logic for EffectAnalyzerView.xaml
    /// </summary>
    public partial class EffectAnalyzerView : UserControl
    {
        public EffectAnalyzerView()
        {
            InitializeComponent();
        }
    }
    public class EffectAnalyzerSelectableViewPart : ISelectableViewPart
    {
        private readonly Lazy<EffectAnalyzerView> lazyContent;

        public EffectAnalyzerSelectableViewPart(Lazy<EffectAnalyzerView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 200;

        public string ViewPartName => "Efect Creator";
        public string Geometry => "chasingPattern";

        public object Content { get => lazyContent.Value; }
    }
}
