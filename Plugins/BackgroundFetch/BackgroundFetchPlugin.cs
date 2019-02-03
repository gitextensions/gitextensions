using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BackgroundFetch.Properties;
using GitCommands;
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
            Icon = Resources.IconBackgroundFetch;
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
                                  // if git not running - start fetch immediately
                                  if (!gitModule.IsRunningGitProcess())
                                  {
                                      return Observable.Return(i);
                                  }

                                  // in other case - every 5 seconds check if git still running
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
                                      GitArgumentBuilder args;
                                      if (_fetchAllSubmodules.ValueOrDefault(Settings))
                                      {
                                        args = new GitArgumentBuilder("submodule")
                                        {
                                            "foreach",
                                            "--recursive",
                                            "git",
                                            "fetch",
                                            "--all"
                                        };

                                        _currentGitUiCommands.GitModule.GitExecutable.GetOutput(args);
                                      }

                                      var gitCmd = _gitCommand.ValueOrDefault(Settings).Trim().SplitBySpace();
                                      args = new GitArgumentBuilder(gitCmd[0]) { gitCmd.Skip(1) };
                                      var msg = _currentGitUiCommands.GitModule.GitExecutable.GetOutput(args);
                                      if (_autoRefresh.ValueOrDefault(Settings))
                                      {
                                          if (gitCmd[0].Equals("fetch", StringComparison.InvariantCultureIgnoreCase))
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
