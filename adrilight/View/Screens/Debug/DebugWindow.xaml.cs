using Serilog;
using System;
using System.IO;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class DebugWindow
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        public DebugWindow()
        {
            InitializeComponent();

            //Log.CloseAndFlush();
            //var logFilePath = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()).ToString(), "log.txt");
            //TextRange range;
            //FileStream fStream;
            //if (File.Exists(logFilePath))
            //{
            //    range = new TextRange(MyRichTextBox.Document.ContentStart, MyRichTextBox.Document.ContentEnd);
            //    fStream = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    range.Load(fStream, DataFormats.Text);
            //    fStream.Close();
            //}
            SetupDebugLogging();
        }
        private void SetupDebugLogging()
        {
            var logPath = Path.Combine(JsonPath, "Logs");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console().WriteTo.RichTextBox(MyRichTextBox)
                .WriteTo.RollingFile(Path.Combine(logPath, "adrilight-{Date}.txt"), retainedFileCountLimit: 10, shared: true, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information($"Debug Window Opened!");
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);


            NonClientAreaContent = new DebugNonClientAreaContent();

        }
    }
}
