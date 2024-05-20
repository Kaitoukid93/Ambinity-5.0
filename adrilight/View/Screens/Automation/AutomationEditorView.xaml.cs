using adrilight.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for ActionmanagerWindow.xaml
    /// </summary>
    public partial class AutomationEditorView
    {
        public AutomationEditorView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }
        public class AutomationEditorViewPage : ISelectablePage
        {
            private readonly Lazy<AutomationEditorView> lazyContent;

            public AutomationEditorViewPage(Lazy<AutomationEditorView> lazyContent)
            {
                this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
            }

            public int Order => 10;

            public string PageName => "Automation Editor";
            public string Geometry => "";

            public object Content { get => lazyContent.Value; }
        }

    }
}
