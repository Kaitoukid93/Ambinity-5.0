using adrilight.Resources;
using adrilight.Services.OpenRGBService;
using adrilight.View;
using Serilog;
using Squirrel;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Util
{
    class AdrilightUpdater
    {
        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Developer_Release";

        public AdrilightUpdater(IGeneralSettings settings, IAmbinityClient ambinityClient, IContext context, HWMonitor hWmonitor)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            HWMonitor = hWmonitor ?? throw new ArgumentNullException(nameof(hWmonitor));
            AmbinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));

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
        public IAmbinityClient AmbinityClient { get; }
        public HWMonitor HWMonitor { get; }
        public IContext Context { get; }

        private async Task StartSquirrel()
        {
            while (Settings.UpdaterAskAgain)
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
                            var asked = await Application.Current.Dispatcher.Invoke<Task<bool>>(AskUserForUpdating);
                            if (!asked)
                                return;
                            //var userAction = HandyControl.Controls.MessageBox.Show(message, "New Update detected", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            //if (userAction != MessageBoxResult.Yes)
                            //{
                            //   // this.logger.Info("update declined by user.");
                            //    return;
                            //}
                            //show loading

                            // Enable OpenRGB
                            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                _splashScreen = new View.SplashScreen();
                                _splashScreen.Header.Text = "Downloading Update";
                                _splashScreen.status.Text = "UPDATING...";
                                _splashScreen.Show();

                            });


                            // this.logger.Info("Downloading updates");
                            var releaseEntry = await mgr.Result.UpdateApp();

                            if (releaseEntry != null)
                            {

                                //restart adrilight if an update was installed
                                //dispose locked WinRing0 file first
                                if (HWMonitor != null)
                                    HWMonitor.Dispose();
                                if (AmbinityClient != null)
                                    AmbinityClient.Dispose();
                                //remember to dispose openrgbstream too!!!
                                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
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
                    Log.Error(ex, $"error when update checking: {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a day for updates
                await Task.Delay(TimeSpan.FromDays(1));
            }
        }

        public async Task<bool> AskUserForUpdating()
        {
            var dialog = new CommonAskingDialog();
            //dialog.header.Text = "OpenRGB is disabled"
            dialog.question.Text = "Phát hiện một bản cập nhật mới, bạn có muốn cập nhật không?";
            var result = await Task.FromResult(dialog.ShowDialog());
            if (result == false)
            {
                if (dialog.askagaincheckbox.IsChecked == true)
                {
                    Settings.UpdaterAskAgain = false;
                }
                else
                {
                    Settings.UpdaterAskAgain = true;
                }
                return false;

            }
            else
            {
                return true;
            }
        }
    }
}