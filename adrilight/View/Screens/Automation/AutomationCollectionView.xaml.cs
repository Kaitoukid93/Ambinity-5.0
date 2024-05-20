using adrilight.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AutomationCollectionView
    {
        public AutomationCollectionView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
          //  this.Close();
        }
        public ModifierKeys[] AllModifiers => Enum.GetValues(typeof(ModifierKeys)).Cast<ModifierKeys>().ToArray();

        public class AutomationCollectionViewPage : ISelectablePage
        {
            private readonly Lazy<AutomationCollectionView> lazyContent;

            public AutomationCollectionViewPage(Lazy<AutomationCollectionView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Automations Collection";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }

    }
}
