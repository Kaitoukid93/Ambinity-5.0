using adrilight.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DeviceAdvanceSettingsView
    {
        public DeviceAdvanceSettingsView()
        {
            InitializeComponent();

        }
        private void GroupBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grBx = (Border)sender;
            var dataCntx = grBx.DataContext;
            var dataSource = (adrilight_shared.Models.ControlMode.ModeParameters.ListSelectionParameter)dataCntx;
            if (dataSource != null)
            {
                //if (dataSource.ShowMore)
                //    dataSource.ShowMore = false;
                //else
                //    dataSource.ShowMore = true;
            }
        }
    }

    public class DeviceAdvanceSettingsViewPage : ISelectablePage
    {
        private readonly Lazy<DeviceAdvanceSettingsView> lazyContent;

        public DeviceAdvanceSettingsViewPage(Lazy<DeviceAdvanceSettingsView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 10;

        public string PageName => "Devices Advance Settings";
        public string Geometry => "";

        public object Content { get => lazyContent.Value; }
    }

}
