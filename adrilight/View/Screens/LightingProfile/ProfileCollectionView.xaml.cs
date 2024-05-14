using adrilight.ViewModel;
using System;
using System.Windows;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ProfileCollectionView
    {
        public ProfileCollectionView()
        {
            InitializeComponent();

        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            PopUpAddTo.IsOpen = true;
        }
        private void Close_Popup_Button_Click(object sender, RoutedEventArgs e)
        {
            PopUpAddTo.IsOpen = false;
        }
        public class ProfileCollectionViewPage : ISelectablePage
        {
            private readonly Lazy<ProfileCollectionView> lazyContent;

            public ProfileCollectionViewPage(Lazy<ProfileCollectionView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Profiles Collection View";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }

    }
}
