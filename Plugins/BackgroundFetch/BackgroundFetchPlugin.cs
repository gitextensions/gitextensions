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

        private IDisposable _cancellationToken;
        private IGitUICommands _currentGitUiCommands;

        private StringSetting _GitCommand = new StringSetting("Arguments of git command to run", "fetch --all");
        private NumberSetting<int> _FetchInterval = new NumberSetting<int>("Fetch every (seconds) - set to 0 to disable", 0);
        private BoolSetting _AutoRefresh = new BoolSetting("Refresh view after fetch", false);
        private BoolSetting _FetchAllSubmodules = new BoolSetting("Fetch all submodules", false);

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return _GitCommand;
            yield return _FetchInterval;
            yield return _AutoRefresh;
            yield return _FetchAllSubmodules;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);

            _currentGitUiCommands = gitUiCommands;
            _currentGitUiCommands.PostSettings += OnPostSettings;

            RecreateObservable();
        }

        private void OnPostSettings(object sender, GitUIPostActionEventArgs e)
        {
            RecreateObservable();
        }

        private void RecreateObservable()
        {
            CancelBackgroundOperation();

            int fetchInterval = _FetchInterval.ValueOrDefault(Settings);

            var gitModule = _currentGitUiCommands.GitModule;
            if (fetchInterval > 0 && gitModule.IsValidGitWorkingDir())
            {
                _cancellationToken =
                    Observable.Timer(TimeSpan.FromSeconds(Math.Max(5, fetchInterval)))
                              .SelectMany(i =>
                              {
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
                                      if (_FetchAllSubmodules.ValueOrDefault(Settings))
                                          _currentGitUiCommands.GitCommand("submodule foreach --recursive git fetch --all");

                                      var gitCmd = _GitCommand.ValueOrDefault(Settings).Trim();
                                      var msg = _currentGitUiCommands.GitCommand(gitCmd);
                                      if (_AutoRefresh.ValueOrDefault(Settings))
                                      {
                                          if (gitCmd.StartsWith("fetch", StringComparison.InvariantCultureIgnoreCase))
                                          {
                                              if (msg.Contains("From"))
                                                  _currentGitUiCommands.RepoChangedNotifier.Notify();
                                          }
                                          else
                                              _currentGitUiCommands.RepoChangedNotifier.Notify();
                                      }
                                  });
            }
        }

        private void CancelBackgroundOperation()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Dispose();
                _cancellationToken = null;
            }
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            CancelBackgroundOperation();

            if (_currentGitUiCommands != null)
            {
                _currentGitUiCommands.PostSettings -= OnPostSettings;
                _currentGitUiCommands = null;
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
