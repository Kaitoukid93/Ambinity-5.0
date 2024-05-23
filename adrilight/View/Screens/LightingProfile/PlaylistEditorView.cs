using adrilight.ViewModel;
using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class PlaylistEditorView
    {
        public PlaylistEditorView()
        {
            InitializeComponent();

        }

        

        public class PlaylistEditorViewPage : ISelectablePage
        {
            private readonly Lazy<PlaylistEditorView> lazyContent;

            public PlaylistEditorViewPage(Lazy<PlaylistEditorView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Playlist Editor View";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }
    }
}
