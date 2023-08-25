using System;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AmbinoOnlineStoreView
    {
        public AmbinoOnlineStoreView()
        {
            InitializeComponent();

        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            NonClientAreaContent = new StoreNonClientAreaContent();
        }
    }
}
