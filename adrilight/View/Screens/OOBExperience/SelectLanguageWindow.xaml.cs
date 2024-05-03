using adrilight_shared.Models.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace adrilight.View.Screens.OOBExperience
{
    /// <summary>
    /// Interaction logic for SelectLanguageWindow.xaml
    /// </summary>
    public partial class SelectLanguageWindow
    {
        public SelectLanguageWindow()
        {
            InitializeComponent();
        }
        public LangModel SelectedLanguage { get; private set; }

        private void Vietnamese_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = new LangModel(new CultureInfo("vi-VN"), "Tiếng Việt", "");
            this.DialogResult = true;
            this.Close();
        }

        private void English_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = new LangModel(new CultureInfo("en-US"), "English", "");
            this.DialogResult = true;
            this.Close();
        }

    }
}
