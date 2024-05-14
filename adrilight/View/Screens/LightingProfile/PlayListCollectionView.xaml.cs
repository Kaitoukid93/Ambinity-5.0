using adrilight.ViewModel;
using System;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PlayListCollectionView
    {
        public PlayListCollectionView()
        {
            InitializeComponent();

        }
        public class PlayListCollectionViewPage : ISelectablePage
        {
            private readonly Lazy<PlayListCollectionView> lazyContent;

            public PlayListCollectionViewPage(Lazy<PlayListCollectionView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Playlist Collection View";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }
    }
}
