using adrilight_content_creator.ViewModel;
using HandyControl.Themes;
using Ninject;
using Ninject.Extensions.Conventions;
using System;
using System.Windows;
using System.Windows.Media;

namespace adrilight_content_creator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs startupEvent)
        {

            base.OnStartup(startupEvent);
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            ThemeManager.Current.AccentColor = new SolidColorBrush(Color.FromArgb(255, 255, 69, 0));
            kernel = SetupDependencyInjection();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            this.Resources["Locator"] = new ViewModelLocator(kernel);
            OpenMainWindow();

        }





        internal static IKernel SetupDependencyInjection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<MainViewModel>().ToSelf().InSingletonScope();
            kernel.Bind(x => x.FromThisAssembly()
            .SelectAllClasses()
            .InheritedFrom<ISelectableViewPart>()
            .BindAllInterfaces());
            return kernel;
        }

        MainWindow _mainForm;
        private IKernel kernel;


        private void OpenMainWindow()
        {
            if (_mainForm == null)
            {
                _mainForm = new MainWindow();
                _mainForm.Closed += MainForm_FormClosed;
                _mainForm.Show();
            }
            else
            {
                //bring to front?
                _mainForm.Focus();
            }
        }

        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            if (_mainForm == null) return;

            //deregister to avoid memory leak
            _mainForm.Closed -= MainForm_FormClosed;
            _mainForm = null;
        }

    }
}