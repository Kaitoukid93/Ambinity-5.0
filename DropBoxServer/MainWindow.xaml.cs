using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Paper;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;


namespace DropBoxServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DropBoxHelpers drbx = new DropBoxHelpers();
            drbx.Client = new DropboxClient("sl.BZ6BfMYlYs0CzGAzC7Wdi8sTMbe5iwbzFdlTCScCGceQa6bIkCrEkGNZOQnFKVAOPhzueISPmhqlXQynC04kWc9sKUz9Y5MIjtL83QiBG-pGBZVFg7oezYEFmxj4d7S1IN5tWYR3Dht-");
            var destPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filetoUpload = Path.Combine(destPath, "StripeMoving.AML");
           // Task.Run(async () => await drbx.GetAllFilesInAppFolder());
            Task.Run(async () => await drbx.UploadFile("/chasingpatterns" + "/"+ "StripeMoving.AML", filetoUpload));
        }

      

  
    
    }
}
