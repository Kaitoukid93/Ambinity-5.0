using adrilight.Resources;
using adrilight.View;
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
        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Stable_Release";

        public AdrilightUpdater(IGeneralSettings settings, IContext context, IHWMonitor hWmonitor)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            HWMonitor = hWmonitor ?? throw new ArgumentNullException(nameof(hWmonitor));

        }

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
                            var message = new StringBuilder().AppendLine("Phát hiện một bản cập nhật ").
                                                              AppendLine("Chọn [Yes] nếu bạn muốn cập nhật ngay bây giờ").
                                                              ToString();

                            var userAction = HandyControl.Controls.MessageBox.Show(message, "New Update detected", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (userAction != MessageBoxResult.Yes)
                            {
                                // this.logger.Info("update declined by user.");
                                return;
                            }
                            //show loading

                            // this.logger.Info("Downloading updates");
                            var updatingview = new UpdatingView();
                            updatingview.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            updatingview.Show();
                            updatingview.WindowState = WindowState.Normal;
                            var releaseEntry = await mgr.Result.UpdateApp();

                            if (releaseEntry != null)
                            {

                                //restart adrilight if an update was installed
                                //dispose licked file first
                                if (HWMonitor != null)
                                    HWMonitor.Dispose();
                                //remember to dispose openrgbstream too!!!
                                updatingview.Status.Text = "Restarting...";
                                updatingview.Close();
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