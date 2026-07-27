using System.Diagnostics;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs.BrowseDialog;

/// <summary>
///  Watches the active repository and publishes its parsed working-directory status.
/// </summary>
public sealed class GitStatusMonitor : IDisposable
{
    private const int InteractiveUpdateDelay = 200;
    private const int FileChangedUpdateDelay = 1000;
    private const int MinUpdateInterval = 30000;
    private const int PeriodicUpdateInterval = 5 * 60 * 1000;
    private const int PeriodicUpdateIntervalWSL = 60 * 1000;
    private const int MaxConsecutiveErrors = 3;

    private readonly FileSystemWatcher _workTreeWatcher = new();
    private readonly FileSystemWatcher _gitDirWatcher = new();
    private readonly DispatcherTimer _timerRefresh;
    private readonly Lock _statusSequenceLock = new();
    private readonly CancellationTokenSequence _statusSequence = new();
    private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
    private readonly GetAllChangedFilesOutputParser _getAllChangedFilesOutputParser;
    private readonly Func<bool> _isMinimized;
    private bool _commandIsRunningAndNotCancelled;
    private int _consecutiveErrorCount;
    private GitStatusMonitorState _currentStatus;
    private bool _disposed;
    private bool _isFirstPostRepoChanged;
    private string? _gitPath;
    private int _nextEarliestTime;
    private int _nextUpdateTime;
    private string? _submodulesPath;

    public GitStatusMonitor(IGitUICommandsSource commandsSource, Func<bool> isMinimized)
    {
        _isMinimized = isMinimized;
        _timerRefresh = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(InteractiveUpdateDelay / 2),
        };
        _timerRefresh.Tick += TimerRefreshTick;

        CurrentStatus = GitStatusMonitorState.Stopped;

        _workTreeWatcher.EnableRaisingEvents = false;
        _workTreeWatcher.Changed += WorkTreeChanged;
        _workTreeWatcher.Created += WorkTreeChanged;
        _workTreeWatcher.Deleted += WorkTreeChanged;
        _workTreeWatcher.Renamed += WorkTreeChanged;
        _workTreeWatcher.Error += WorkTreeWatcherError;
        _workTreeWatcher.IncludeSubdirectories = true;
        _workTreeWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        _gitDirWatcher.EnableRaisingEvents = false;
        _gitDirWatcher.Changed += GitDirChanged;
        _gitDirWatcher.Created += GitDirChanged;
        _gitDirWatcher.Deleted += GitDirChanged;
        _gitDirWatcher.Error += WorkTreeWatcherError;
        _gitDirWatcher.IncludeSubdirectories = true;
        _gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

        Init(commandsSource);
        _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(() => commandsSource.UICommands.Module);
    }

    public bool Active
    {
        get => CurrentStatus != GitStatusMonitorState.Stopped;
        set => CurrentStatus = value ? GitStatusMonitorState.Running : GitStatusMonitorState.Stopped;
    }

    public event EventHandler<GitStatusMonitorStateEventArgs>? GitStatusMonitorStateChanged;

    public event EventHandler<GitWorkingDirectoryStatusEventArgs?>? GitWorkingDirectoryStatusChanged;

    public void InvalidateGitWorkingDirectoryStatus()
    {
        GitWorkingDirectoryStatusChanged?.Invoke(this, null);
    }

    public void RequestRefresh()
    {
        ScheduleNextInteractiveTime();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _currentStatus = GitStatusMonitorState.Stopped;
        _timerRefresh.Stop();
        _timerRefresh.Tick -= TimerRefreshTick;
        lock (_statusSequenceLock)
        {
            _statusSequence.CancelCurrent();
            _commandIsRunningAndNotCancelled = false;
        }

        _taskManager.JoinPendingOperations();
        _workTreeWatcher.Dispose();
        _gitDirWatcher.Dispose();
        _statusSequence.Dispose();
    }

    private GitStatusMonitorState CurrentStatus
    {
        get => _currentStatus;
        set
        {
            ThreadHelper.AssertOnUIThread();

            GitStatusMonitorState previousStatus = _currentStatus;
            _currentStatus = value;
            switch (_currentStatus)
            {
                case GitStatusMonitorState.Stopped:
                    _timerRefresh.Stop();
                    _workTreeWatcher.EnableRaisingEvents = false;
                    _gitDirWatcher.EnableRaisingEvents = false;
                    _consecutiveErrorCount = 0;

                    if (_currentStatus != previousStatus)
                    {
                        lock (_statusSequenceLock)
                        {
                            if (_commandIsRunningAndNotCancelled)
                            {
                                _statusSequence.CancelCurrent();
                                _commandIsRunningAndNotCancelled = false;
                            }
                        }

                        InvalidateGitWorkingDirectoryStatus();
                    }

                    break;

                case GitStatusMonitorState.Paused:
                    _timerRefresh.Stop();
                    _workTreeWatcher.EnableRaisingEvents = false;
                    _gitDirWatcher.EnableRaisingEvents = false;
                    break;

                case GitStatusMonitorState.Inactive:
                    if (_currentStatus != previousStatus)
                    {
                        InvalidateGitWorkingDirectoryStatus();
                    }

                    break;

                case GitStatusMonitorState.Running:
                    if (previousStatus == GitStatusMonitorState.Inactive
                        || previousStatus == GitStatusMonitorState.Running)
                    {
                        if (!_commandIsRunningAndNotCancelled)
                        {
                            ScheduleNextInteractiveTime();
                        }

                        break;
                    }

                    EnableRaisingEvents();
                    ScheduleNextInteractiveTime(FileChangedUpdateDelay);
                    _timerRefresh.Start();
                    break;

                default:
                    throw new NotSupportedException();
            }

            GitStatusMonitorStateChanged?.Invoke(this, new GitStatusMonitorStateEventArgs(_currentStatus));
        }
    }

    private IGitModule? Module => UICommandsSource?.UICommands.Module;

    private IGitUICommandsSource? UICommandsSource { get; set; }

    private void EnableRaisingEvents()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _workTreeWatcher.EnableRaisingEvents = Directory.Exists(_workTreeWatcher.Path);
            _gitDirWatcher.EnableRaisingEvents = Directory.Exists(_gitDirWatcher.Path)
                && !IsSameOrDescendantPath(_gitDirWatcher.Path, _workTreeWatcher.Path);
        }
        catch
        {
            _workTreeWatcher.EnableRaisingEvents = false;
            _gitDirWatcher.EnableRaisingEvents = false;
        }
    }

    private void GitDirChanged(object? sender, FileSystemEventArgs e)
    {
        if (_gitPath is null)
        {
            return;
        }

        if (string.Equals(Path.GetFileName(e.FullPath), "index.lock", PathComparison))
        {
            return;
        }

        if (_submodulesPath is not null
            && IsSameOrDescendantPath(e.FullPath, _submodulesPath)
            && Directory.Exists(e.FullPath))
        {
            return;
        }

        _gitDirWatcher.EnableRaisingEvents = false;
        ScheduleNextUpdateTime(FileChangedUpdateDelay);
    }

    private void Init(IGitUICommandsSource commandsSource)
    {
        UICommandsSource = commandsSource ?? throw new ArgumentNullException(nameof(commandsSource));
        UICommandsSource.UICommandsChanged += CommandsSourceGitUICommandsChanged;
        ActivateCommands(commandsSource.UICommands);
    }

    private void CommandsSourceGitUICommandsChanged(object? sender, GitUICommandsChangedEventArgs e)
    {
        IGitUICommands? oldCommands = e.OldCommands;
        if (oldCommands is not null)
        {
            oldCommands.PreCheckoutBranch -= GitUICommandsPreCheckout;
            oldCommands.PreCheckoutRevision -= GitUICommandsPreCheckout;
            oldCommands.PostCheckoutBranch -= GitUICommandsPostCheckout;
            oldCommands.PostCheckoutRevision -= GitUICommandsPostCheckout;
            oldCommands.PostRepositoryChanged -= GitUICommandsPostRepositoryChanged;
        }

        if (sender is IGitUICommandsSource source)
        {
            ActivateCommands(source.UICommands);
        }
    }

    private void ActivateCommands(IGitUICommands commands)
    {
        commands.PreCheckoutBranch += GitUICommandsPreCheckout;
        commands.PreCheckoutRevision += GitUICommandsPreCheckout;
        commands.PostCheckoutBranch += GitUICommandsPostCheckout;
        commands.PostCheckoutRevision += GitUICommandsPostCheckout;
        commands.PostRepositoryChanged += GitUICommandsPostRepositoryChanged;

        IGitModule module = commands.Module;
        StartWatchingChanges(module.WorkingDir, module.WorkingDirGitDir);
    }

    private void GitUICommandsPostCheckout(object? sender, GitUIPostActionEventArgs e)
    {
        CurrentStatus = GitStatusMonitorState.Running;
    }

    private void GitUICommandsPostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        lock (_statusSequenceLock)
        {
            _isFirstPostRepoChanged = true;
        }
    }

    private void GitUICommandsPreCheckout(object? sender, GitUIEventArgs e)
    {
        CurrentStatus = GitStatusMonitorState.Paused;
    }

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static bool IsSameOrDescendantPath(string path, string directory)
    {
        string relativePath = Path.GetRelativePath(directory, path);
        return relativePath == "."
            || (!Path.IsPathRooted(relativePath)
                && relativePath != ".."
                && !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private void ScheduleNextInteractiveTime(int delay = InteractiveUpdateDelay)
    {
        lock (_statusSequenceLock)
        {
            _statusSequence.CancelCurrent();
            _commandIsRunningAndNotCancelled = false;

            if (_disposed)
            {
                return;
            }

            int ticks = Environment.TickCount;
            _nextEarliestTime = ticks + MinUpdateInterval;
            int currentDelay = _nextUpdateTime - ticks;
            if (delay < currentDelay)
            {
                _nextUpdateTime = ticks + delay;
            }
        }
    }

    private void ScheduleNextUpdateTime(int delay)
    {
        lock (_statusSequenceLock)
        {
            int ticks = Environment.TickCount;
            int currentDelay = _nextUpdateTime - ticks;
            int minimumDelay = Math.Max(delay, _nextEarliestTime - ticks);
            if (minimumDelay < currentDelay)
            {
                _nextUpdateTime = ticks + minimumDelay;
            }
        }
    }

    private void StartWatchingChanges(string workTreePath, string gitDirPath)
    {
        try
        {
            bool isValidGitDir = !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath);
            if (!string.IsNullOrEmpty(workTreePath)
                && Directory.Exists(workTreePath)
                && (isValidGitDir || PathUtil.IsWslLink(gitDirPath)))
            {
                _workTreeWatcher.Path = workTreePath;
                _gitPath = PathUtil.RemoveTrailingPathSeparator(gitDirPath);
                if (isValidGitDir)
                {
                    _gitDirWatcher.Path = PathUtil.RemoveTrailingPathSeparator(gitDirPath);
                    _submodulesPath = Path.Join(_gitPath, "modules");
                }
                else
                {
                    _gitDirWatcher.Path = workTreePath;
                    _submodulesPath = Path.Join(gitDirPath, "modules");
                }

                CurrentStatus = GitStatusMonitorState.Running;
            }
            else
            {
                CurrentStatus = GitStatusMonitorState.Stopped;
            }
        }
        catch
        {
            CurrentStatus = GitStatusMonitorState.Stopped;
        }
    }

    private void TimerRefreshTick(object? sender, EventArgs e)
    {
        Update();
    }

    private void Update()
    {
        ThreadHelper.AssertOnUIThread();

        if (CurrentStatus is not (GitStatusMonitorState.Running or GitStatusMonitorState.Inactive))
        {
            return;
        }

        IGitUICommandsSource? commandsSource = UICommandsSource;
        IGitModule? activeModule = Module;
        if (commandsSource is null || activeModule is null)
        {
            return;
        }

        if (_isMinimized() || commandsSource.UICommands.RepoChangedNotifier.IsLocked)
        {
            if (CurrentStatus == GitStatusMonitorState.Running)
            {
                CurrentStatus = GitStatusMonitorState.Inactive;
            }

            return;
        }

        if (CurrentStatus == GitStatusMonitorState.Inactive)
        {
            CurrentStatus = GitStatusMonitorState.Running;
        }

        int commandStartTime = Environment.TickCount;
        if (_nextUpdateTime - commandStartTime > 0)
        {
            return;
        }

        IGitModule module;
        CancellationToken cancellationToken;
        bool noLocks;

        lock (_statusSequenceLock)
        {
            if (_disposed || _commandIsRunningAndNotCancelled)
            {
                return;
            }

            if (!Directory.Exists(_workTreeWatcher.Path) || activeModule.IsBareRepository())
            {
                return;
            }

            EnableRaisingEvents();
            module = activeModule;
            noLocks = !_isFirstPostRepoChanged;
            cancellationToken = _statusSequence.Next();
            _commandIsRunningAndNotCancelled = true;
            _nextUpdateTime = commandStartTime
                + (PathUtil.IsWslPath(_workTreeWatcher.Path) ? PeriodicUpdateIntervalWSL : PeriodicUpdateInterval);
            _nextEarliestTime = commandStartTime + MinUpdateInterval;
            _isFirstPostRepoChanged = false;
        }

        _taskManager.FileAndForget(async () =>
        {
            try
            {
                ArgumentString command = Commands.GetAllChangedFiles(
                    excludeIgnoredFiles: true,
                    UntrackedFilesMode.Default,
                    noLocks: noLocks);
                ExecutionResult result = await module.GitExecutable.ExecuteAsync(
                    command,
                    cancellationToken: cancellationToken);

                if (result.ExitedSuccessfully && ReferenceEquals(module, Module))
                {
                    IReadOnlyList<GitItemStatus> changedFiles =
                        _getAllChangedFilesOutputParser.Parse(result.StandardOutput);
                    await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                    GitWorkingDirectoryStatusChanged?.Invoke(
                        this,
                        new GitWorkingDirectoryStatusEventArgs(changedFiles));
                }

                _consecutiveErrorCount = 0;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception.Message);
                try
                {
                    if (++_consecutiveErrorCount < MaxConsecutiveErrors)
                    {
                        ScheduleNextInteractiveTime();
                        return;
                    }

                    await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync();
                    CurrentStatus = GitStatusMonitorState.Stopped;
                }
                catch
                {
                }
            }
            finally
            {
                lock (_statusSequenceLock)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _commandIsRunningAndNotCancelled = false;
                        if (ReferenceEquals(module, Module))
                        {
                            int endTime = Environment.TickCount;
                            int commandTime = endTime - commandStartTime;
                            int minimumDelay = Math.Max(MinUpdateInterval, 2 * commandTime);
                            _nextEarliestTime = endTime + minimumDelay;
                            if (_nextUpdateTime - commandStartTime < _nextEarliestTime - commandStartTime)
                            {
                                _nextUpdateTime = _nextEarliestTime;
                            }
                        }
                    }
                }
            }
        });
    }

    private void WorkTreeChanged(object? sender, FileSystemEventArgs e)
    {
        if (_gitPath is not null && IsSameOrDescendantPath(e.FullPath, _gitPath))
        {
            GitDirChanged(sender, e);
            return;
        }

        if (string.Equals(Path.GetFileName(e.FullPath), ".git", PathComparison))
        {
            return;
        }

        if (string.Equals(Path.GetFileName(e.FullPath), "index.lock", PathComparison)
            && string.Equals(Path.GetFileName(Path.GetDirectoryName(e.FullPath)), ".git", PathComparison))
        {
            return;
        }

        _workTreeWatcher.EnableRaisingEvents = false;
        ScheduleNextUpdateTime(FileChangedUpdateDelay);
    }

    private void WorkTreeWatcherError(object? sender, ErrorEventArgs e)
    {
        ScheduleNextUpdateTime(FileChangedUpdateDelay);
    }
}
