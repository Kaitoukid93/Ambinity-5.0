
using System.Collections.ObjectModel;
using System.Windows;


namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ScaleSelectionWindow
    {
        public ScaleSelectionWindow(ObservableCollection<IDrawable> itemSource)
        {
            ItemSource = new ObservableCollection<IDrawable>();
            ItemSource = itemSource;
            InitializeComponent();
        }
        public ScaleSelectionWindow()
        {

            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public ObservableCollection<IDrawable> ItemSource { get; private set; }
    }
}
