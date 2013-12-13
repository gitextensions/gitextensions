using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace BackgroundFetch
{
    public class BackgroundFetchPlugin : GitPluginBase, IGitPluginForRepository
    {
        private IDisposable cancellationToken;
        private IGitUICommands currentGitUiCommands;
        private const string GitCommandSetting = "Arguments of git command to run";
        private const string FetchIntervalSetting = "Fetch every (seconds) - set to 0 to disable";
        private const string AutoRefreshSetting = "Refresh view after fetch (true / false)";
        private const string FetchSubmodules = "Fetch all submodules (true/false)";

        public override string Description
        {
            get { return "Periodic background fetch"; }
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();

            Settings.AddSetting(FetchIntervalSetting, "0");
            Settings.AddSetting(AutoRefreshSetting, "false");
            Settings.AddSetting(GitCommandSetting, "fetch --all");
            Settings.AddSetting(FetchSubmodules, "false");
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);

            currentGitUiCommands = gitUiCommands;
            currentGitUiCommands.PostSettings += OnPostSettings;

            RecreateObservable();
        }

        private void OnPostSettings(object sender, GitUIPostActionEventArgs e)
        {
            RecreateObservable();
        }

        private void RecreateObservable()
        {
            CancelBackgroundOperation();

            int fetchInterval;
            if (!int.TryParse(Settings.GetSetting(FetchIntervalSetting), out fetchInterval))
                fetchInterval = 0;

            bool autoRefresh;
            if (!bool.TryParse(Settings.GetSetting(AutoRefreshSetting), out autoRefresh))
                autoRefresh = false;

            bool fetchSubMods;
            if (!bool.TryParse(Settings.GetSetting(FetchSubmodules), out fetchSubMods))
                fetchSubMods = false;


            var gitModule = currentGitUiCommands.GitModule;
            if (fetchInterval > 0 && GitModule.IsValidGitWorkingDir(gitModule.GitWorkingDir))
            {
                cancellationToken =
                    Observable.Timer(TimeSpan.FromSeconds(Math.Max(5, fetchInterval)))
                              .SkipWhile(i => gitModule.IsRunningGitProcess())
                              .Repeat()
                              .ObserveOn(ThreadPoolScheduler.Instance)
                              .Subscribe(i =>
                                  {
                                      if (fetchSubMods)
                                          currentGitUiCommands.GitCommand("submodule foreach --recursive git fetch --all");

                                      var gitCmd = Settings.GetSetting(GitCommandSetting).Trim();
                                      var msg = currentGitUiCommands.GitCommand(gitCmd);
                                      if (autoRefresh)
                                      {
                                          if (gitCmd.StartsWith("fetch", StringComparison.InvariantCultureIgnoreCase))
                                          {
                                              if (msg.Contains("From"))
                                                  currentGitUiCommands.RepoChangedNotifier.Notify();
                                          }
                                          else
                                              currentGitUiCommands.RepoChangedNotifier.Notify();
                                      }
                                  }
                                  );
            }
        }

        private void CancelBackgroundOperation()
        {
            if (cancellationToken != null)
            {
                cancellationToken.Dispose();
                cancellationToken = null;
            }
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            CancelBackgroundOperation();

            if (currentGitUiCommands != null)
            {
                currentGitUiCommands.PostSettings -= OnPostSettings;
                currentGitUiCommands = null;
            }

            base.Unregister(gitUiCommands);
        }

        public override bool Execute(GitUIBaseEventArgs gitUiArgs)
        {
            gitUiArgs.GitUICommands.StartSettingsDialog(this);
            return false;
        }
    }
}
