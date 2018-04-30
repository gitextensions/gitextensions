using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitUIPluginInterfaces;
using ResourceManager;

namespace BackgroundFetch
{
    [Export(typeof(IGitPlugin))]
    public class BackgroundFetchPlugin : GitPluginBase, IGitPluginForRepository
    {
        public BackgroundFetchPlugin()
        {
            SetNameAndDescription("Periodic background fetch");
            Translate();
        }

        private IDisposable _cancellationToken;
        private IGitUICommands _currentGitUiCommands;

        private readonly StringSetting _gitCommand = new StringSetting("Arguments of git command to run", "fetch --all");
        private readonly NumberSetting<int> _fetchInterval = new NumberSetting<int>("Fetch every (seconds) - set to 0 to disable", 0);
        private readonly BoolSetting _autoRefresh = new BoolSetting("Refresh view after fetch", false);
        private readonly BoolSetting _fetchAllSubmodules = new BoolSetting("Fetch all submodules", false);

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return _gitCommand;
            yield return _fetchInterval;
            yield return _autoRefresh;
            yield return _fetchAllSubmodules;
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

            int fetchInterval = _fetchInterval.ValueOrDefault(Settings);

            var gitModule = _currentGitUiCommands.GitModule;
            if (fetchInterval > 0 && gitModule.IsValidGitWorkingDir())
            {
                _cancellationToken =
                    Observable.Timer(TimeSpan.FromSeconds(Math.Max(5, fetchInterval)))
                              .SelectMany(i =>
                              {
                                  // if git not runing - start fetch immediately
                                  if (!gitModule.IsRunningGitProcess())
                                  {
                                      return Observable.Return(i);
                                  }

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
                                      if (_fetchAllSubmodules.ValueOrDefault(Settings))
                                      {
                                          _currentGitUiCommands.GitCommand("submodule foreach --recursive git fetch --all");
                                      }

                                      var gitCmd = _gitCommand.ValueOrDefault(Settings).Trim();
                                      var msg = _currentGitUiCommands.GitCommand(gitCmd);
                                      if (_autoRefresh.ValueOrDefault(Settings))
                                      {
                                          if (gitCmd.StartsWith("fetch", StringComparison.InvariantCultureIgnoreCase))
                                          {
                                              if (msg.Contains("From"))
                                              {
                                                  _currentGitUiCommands.RepoChangedNotifier.Notify();
                                              }
                                          }
                                          else
                                          {
                                              _currentGitUiCommands.RepoChangedNotifier.Notify();
                                          }
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

        public override bool Execute(GitUIEventArgs args)
        {
            args.GitUICommands.StartSettingsDialog(this);
            return false;
        }
    }
}
