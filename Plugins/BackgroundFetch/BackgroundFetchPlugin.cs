using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitUIPluginInterfaces;
using ResourceManager;

namespace BackgroundFetch
{
    public class BackgroundFetchPlugin : GitPluginBase, IGitPluginForRepository
    {
        public BackgroundFetchPlugin()
        {
            SetNameAndDescription("Periodic background fetch");
            Translate();
        }

        private IDisposable cancellationToken;
        private IGitUICommands currentGitUiCommands;

        private StringSetting GitCommand = new StringSetting("Arguments of git command to run", "fetch --all");
        private NumberSetting<int> FetchInterval = new NumberSetting<int>("Fetch every (seconds) - set to 0 to disable", 0);
        private BoolSetting AutoRefresh = new BoolSetting("Refresh view after fetch", false);
        private BoolSetting FetchAllSubmodules = new BoolSetting("Fetch all submodules", false);

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return GitCommand;
            yield return FetchInterval;
            yield return AutoRefresh;
            yield return FetchAllSubmodules;
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

            int fetchInterval = FetchInterval.ValueOrDefault(Settings);

            var gitModule = currentGitUiCommands.GitModule;
            if (fetchInterval > 0 && gitModule.IsValidGitWorkingDir())
            {
                cancellationToken =
                    Observable.Timer(TimeSpan.FromSeconds(Math.Max(5, fetchInterval)))
                              .SelectMany(i => {
                                // if git not runing - start fetch immediately
                                if (!gitModule.IsRunningGitProcess())
                                    return Observable.Return(i);

                                // in other case - every 5 seconds check if git still runnnig
                                return Observable
                                    .Interval(TimeSpan.FromSeconds(5))
                                    .SkipWhile(ii => gitModule.IsRunningGitProcess())
                                    .FirstAsync()
                                ;
                              })
                              .Repeat()
                              .ObserveOn(ThreadPoolScheduler.Instance)
                              .Subscribe(i =>
                                  {
                                      if (FetchAllSubmodules.ValueOrDefault(Settings))
                                          currentGitUiCommands.GitCommand("submodule foreach --recursive git fetch --all");

                                      var gitCmd = GitCommand.ValueOrDefault(Settings).Trim();
                                      var msg = currentGitUiCommands.GitCommand(gitCmd);
                                      if (AutoRefresh.ValueOrDefault(Settings))
                                      {
                                          if (gitCmd.StartsWith("fetch", StringComparison.InvariantCultureIgnoreCase))
                                          {
                                              if (msg.Contains("From"))
                                                  currentGitUiCommands.RepoChangedNotifier.Notify();
                                          }
                                          else
                                              currentGitUiCommands.RepoChangedNotifier.Notify();
                                      }
                                  });
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
