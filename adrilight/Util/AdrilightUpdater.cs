using adrilight.Resources;
using Newtonsoft.Json;
using NLog;
using Semver;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Util
{
    class AdrilightUpdater
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Developer_Release";

        public AdrilightUpdater(IGeneralSettings settings,IOpenRGBStream openRGBStream, IContext context, IHWMonitor hWmonitor)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            HWMonitor = hWmonitor ?? throw new ArgumentNullException(nameof(hWmonitor));
            OpenRGBStream = openRGBStream ?? throw new ArgumentNullException(nameof(openRGBStream));

        }
        private static View.SplashScreen _splashScreen;
        public void StartThread()
        {
            //if (App.IsPrivateBuild) return;

            var t = new Thread(async () => await StartSquirrel()) {
                Name = "adrilight Update Checker",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            t.Start();

        }

        public IGeneralSettings Settings { get; }
        public IOpenRGBStream OpenRGBStream { get; }
        public IHWMonitor HWMonitor { get; }
        public IContext Context { get; }

        private async Task StartSquirrel()
        {
            while (true)
            {
                try
                {
                    var mgr = UpdateManager.GitHubUpdateManager(ADRILIGHT_RELEASES);
                    using (var result = await mgr)
                    {
                        var updateInfo = await mgr.Result.CheckForUpdate();
                        // display update info and ask user update or not
                        if (updateInfo.ReleasesToApply.Any())
                        {

                            var versionCount = updateInfo.ReleasesToApply.Count;
                            // this.logger.Info($"{versionCount} update(s) found.");

                            var versionWord = versionCount > 1 ? "versions" : "version";
                            var message = new StringBuilder().AppendLine($"App is {versionCount} {versionWord} behind.").
                                                              AppendLine("If you choose to update, changes wont take affect until App is restarted.").
                                                              AppendLine("Would you like to download and install them?").
                                                              ToString();

                            var userAction = HandyControl.Controls.MessageBox.Show(message, "New Update detected", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (userAction != MessageBoxResult.Yes)
                            {
                               // this.logger.Info("update declined by user.");
                                return;
                            }
                            //show loading
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                _splashScreen = new View.SplashScreen();
                                _splashScreen.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                _splashScreen.WindowState = WindowState.Minimized;

                                _splashScreen.Show();
                                _splashScreen.WindowState = WindowState.Normal;
                                _splashScreen.status.Text = "UPDATING...";
                            });

                                // this.logger.Info("Downloading updates");
                                var releaseEntry = await mgr.Result.UpdateApp();

                            if (releaseEntry != null)
                            {

                                //restart adrilight if an update was installed
                                //dispose locked WinRing0 file first
                                if (HWMonitor != null)
                                    HWMonitor.Dispose();
                                if (OpenRGBStream != null)
                                    OpenRGBStream.Dispose();
                                //remember to dispose openrgbstream too!!!
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    _splashScreen.status.Text = "RESTARTING...";
                                });
                                UpdateManager.RestartApp();
                            }
                            //this.logger.Info($"Download complete. Version {updateResult.Version} will take effect when App is restarted.");
                        }
                        else
                        {
                            //this.logger.Info("No updates detected.");
                        }



                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"error when update checking: {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a day for updates
                await Task.Delay(TimeSpan.FromDays(1));
            }
        }
    }
}