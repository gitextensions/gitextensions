using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.BackgroundFetch.Properties;
using GitExtUtils;
using Microsoft;

namespace GitExtensions.Plugins.BackgroundFetch;

[Export(typeof(IGitPlugin))]
public class BackgroundFetchPlugin : GitPluginBase, IGitPluginForRepository
{
    public BackgroundFetchPlugin() : base(true)
    {
        Id = new Guid("D19A7905-8AAD-4271-ACA9-817669B94A1D");
        Name = "Periodic background fetch";
        Translate(AppSettings.CurrentTranslation);
        Icon = Resources.IconBackgroundFetch;
    }

    private IDisposable? _cancellationToken;
    private IGitUICommands? _currentGitUiCommands;

    private readonly PseudoSetting _warningForceWithLease = new("WARNING: be careful when force push with lease having the periodic background fetch enabled but chose not to auto-refresh after each fetch.\r\n\r\nYou could lose new commits pushed by others to the remote branch.\r\n\r\nBe sure to refresh the revision grid before doing a force push with lease.", textboxSettings: tb =>
    {
        tb.Multiline = true;
        tb.Height = 500;
    });
    private const string _defaultGitCommand = "fetch --all";
    private readonly StringSetting _gitCommand = new("Arguments of git command to run", _defaultGitCommand);
    private readonly NumberSetting<int> _fetchInterval = new("Fetch every (seconds) - set to 0 to disable", 0);
    private readonly BoolSetting _autoRefresh = new("Refresh view after fetch", false);
    private readonly BoolSetting _fetchAllSubmodules = new("Fetch all submodules", false);
    private readonly BoolSetting _fetchImmediatelyOnRepoOpening = new("Fetch immediately on repository opening", false);

    public override IEnumerable<ISetting> GetSettings()
    {
        // return all settings or introduce implementation based on reflection on GitPluginBase level
        yield return _gitCommand;
        yield return _fetchInterval;
        yield return _autoRefresh;
        yield return _fetchAllSubmodules;
        yield return _fetchImmediatelyOnRepoOpening;
        yield return _warningForceWithLease;
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

        Validates.NotNull(_currentGitUiCommands);

        IGitModule gitModule = _currentGitUiCommands.Module;
        bool fetchOnOpening = _fetchImmediatelyOnRepoOpening.ValueOrDefault(Settings);
        bool isRefreshDisabled = fetchInterval <= 0;
        if ((isRefreshDisabled && !fetchOnOpening) || !gitModule.IsValidGitWorkingDir())
        {
            return;
        }

        IObservable<long> fetchTriggers;
        if (isRefreshDisabled)
        {
            // just wait running git exits
            fetchTriggers = WaitForRunningGitExitsObservable(gitModule);
        }
        else
        {
            // generate triggers periodically
            TimeSpan triggersInterval = TimeSpan.FromSeconds(Math.Max(5, fetchInterval));
            TimeSpan firstTrigger = fetchOnOpening ? TimeSpan.FromMilliseconds(10) : triggersInterval;
            fetchTriggers = Observable
                .Timer(firstTrigger, triggersInterval)
                .SelectMany(_ => WaitForRunningGitExitsObservable(gitModule))
                .Repeat();
        }

        _cancellationToken = fetchTriggers
            .ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(_ => RunBackgroundGitFetch());
    }

    private static IObservable<long> WaitForRunningGitExitsObservable(IGitModule gitModule)
    {
        // if git not running - start fetch immediately
        if (!gitModule.IsRunningGitProcess())
        {
            return Observable.Return(1L);
        }

        // in other case - every seconds check if git still running
        return Observable
            .Interval(TimeSpan.FromSeconds(1))
            .SkipWhile(_ => gitModule.IsRunningGitProcess())
            .FirstAsync();
    }

    private void RunBackgroundGitFetch()
    {
        GitArgumentBuilder args;
        if (_fetchAllSubmodules.ValueOrDefault(Settings))
        {
            // The Git command is hardcoded compared, not using _gitCommand
            args = new GitArgumentBuilder("submodule")
                        {
                            "foreach",
                            "--recursive",
                            "git",
                            "fetch",
                            "--all"
                        };

            try
            {
                _currentGitUiCommands.Module.GitExecutable.GetOutput(args);
            }
            catch
            {
                // Ignore background errors
            }
        }

        string gitCmdString = _gitCommand.ValueOrDefault(Settings);
        if (string.IsNullOrWhiteSpace(gitCmdString))
        {
            gitCmdString = _defaultGitCommand;
        }

        string[] gitCmd = gitCmdString.Trim().Split(Delimiters.Space, StringSplitOptions.RemoveEmptyEntries);
        args = new GitArgumentBuilder(gitCmd[0]) { gitCmd.Skip(1) };
        string msg;
        try
        {
            // git fetch is writing result details into standard error and not standard output, see:
            // https://github.com/gitextensions/gitextensions/pull/10793
            // https://lore.kernel.org/git/xmqq7cvqrdu6.fsf@gitster.g/
            msg = _currentGitUiCommands.Module.GitExecutable.Execute(args).StandardError;
        }
        catch
        {
            // Ignore background errors
            return;
        }

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
    }

    private void CancelBackgroundOperation()
    {
        if (_cancellationToken is not null)
        {
            _cancellationToken.Dispose();
            _cancellationToken = null;
        }
    }

    public override void Unregister(IGitUICommands gitUiCommands)
    {
        CancelBackgroundOperation();

        if (_currentGitUiCommands is not null)
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
