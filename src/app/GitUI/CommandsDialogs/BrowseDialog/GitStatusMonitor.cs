using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using Microsoft;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed class GitStatusMonitor : IDisposable
    {
        /// <summary>
        /// We often change several files at once.
        /// Short delay before we try to get the status.
        /// </summary>
        private const int InteractiveUpdateDelay = 200;

        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int FileChangedUpdateDelay = 1000;

        /// <summary>
        /// Minimum interval between subsequent updates.
        /// </summary>
        private const int MinUpdateInterval = 30000;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int PeriodicUpdateInterval = 5 * 60 * 1000;

        /// <summary>
        /// Periodic update in WSL, where FileSystemWatcher may not report changes
        /// https://github.com/microsoft/WSL/issues/4581
        /// </summary>
        private const int PeriodicUpdateIntervalWSL = 60 * 1000;

        /// <summary>
        /// The number how often an update must fail in a row until the monitoring is stopped.
        /// </summary>
        private const int _maxConsecutiveErrors = 3;

        /// <summary>
        /// git-status command is running and no cancellation has been requested
        /// </summary>
        private bool _commandIsRunningAndNotCancelled;

        /// <summary>
        /// The number of consecutive update failures.
        /// </summary>
        private int _consecutiveErrorCount;

        private readonly FileSystemWatcher _workTreeWatcher = new();
        private readonly FileSystemWatcher _gitDirWatcher = new();
        private readonly System.Windows.Forms.Timer _timerRefresh;
        private bool _isFirstPostRepoChanged;
        private string? _gitPath;
        private string? _submodulesPath;
        private readonly CancellationTokenSequence _statusSequence = new();
        private readonly GetAllChangedFilesOutputParser _getAllChangedFilesOutputParser;
        private readonly Func<bool> _isMinimized;
        private bool _disposed;

        // Timestamps to schedule status updates, limit the update interval dynamically
        // Note that TickCount wraps after 25 days uptime, always compare diff

        /// <summary>
        /// Next scheduled update time
        /// </summary>
        private int _nextUpdateTime;

        /// <summary>
        /// Earliest time for an scheduled update (interactive requests bypasses this)
        /// </summary>
        private int _nextEarliestTime;

        private GitStatusMonitorState _currentStatus;

        public bool Active
        {
            get => CurrentStatus != GitStatusMonitorState.Stopped;
            set => CurrentStatus = value ? GitStatusMonitorState.Running : GitStatusMonitorState.Stopped;
        }

        /// <summary>
        /// Occurs whenever git status monitor state changes.
        /// </summary>
        public event EventHandler<GitStatusMonitorStateEventArgs>? GitStatusMonitorStateChanged;

        /// <summary>
        /// Occurs whenever current working directory status changes.
        /// </summary>
        public event EventHandler<GitWorkingDirectoryStatusEventArgs?>? GitWorkingDirectoryStatusChanged;

        public GitStatusMonitor(IGitUICommandsSource commandsSource, Func<bool> isMinimized)
        {
            _isMinimized = isMinimized;
            _timerRefresh = new System.Windows.Forms.Timer
            {
                Enabled = true,
                Interval = InteractiveUpdateDelay / 2
            };
            _timerRefresh.Tick += delegate { Update(); };

            CurrentStatus = GitStatusMonitorState.Stopped;

            // Setup a file watcher to detect changes to our files. When they
            // change, we'll update our status.
            _workTreeWatcher.EnableRaisingEvents = false;
            _workTreeWatcher.Changed += WorkTreeChanged;
            _workTreeWatcher.Created += WorkTreeChanged;
            _workTreeWatcher.Deleted += WorkTreeChanged;
            _workTreeWatcher.Renamed += WorkTreeChanged;
            _workTreeWatcher.Error += WorkTreeWatcherError;
            _workTreeWatcher.IncludeSubdirectories = true;
            _workTreeWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            // Setup a file watcher to detect changes to the .git repo files. When they
            // change, we'll update our status.
            _gitDirWatcher.EnableRaisingEvents = false;
            _gitDirWatcher.Changed += GitDirChanged;
            _gitDirWatcher.Created += GitDirChanged;
            _gitDirWatcher.Deleted += GitDirChanged;
            _gitDirWatcher.Error += WorkTreeWatcherError;
            _gitDirWatcher.IncludeSubdirectories = true;
            _gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            Init(commandsSource);

            _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(() => commandsSource.UICommands.Module);

            return;

            void WorkTreeWatcherError(object sender, ErrorEventArgs e)
            {
                // Called for instance at buffer overflow
                ScheduleNextUpdateTime(FileChangedUpdateDelay);
            }

            void GitDirChanged(object sender, FileSystemEventArgs e)
            {
                Validates.NotNull(_gitPath);

                // git directory changed
                if (e.FullPath.Length == _gitPath.Length)
                {
                    return;
                }

                if (e.FullPath.EndsWith("\\index.lock"))
                {
                    return;
                }

                // submodules directory's subdir changed
                // cut/paste/rename/delete operations are not expected on directories inside nested .git dirs
                if (e.FullPath.StartsWith(_submodulesPath) && Directory.Exists(e.FullPath))
                {
                    return;
                }

                _gitDirWatcher.EnableRaisingEvents = false;
                ScheduleNextUpdateTime(FileChangedUpdateDelay);
            }

            void WorkTreeChanged(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath.StartsWith(_gitPath))
                {
                    GitDirChanged(sender, e);
                    return;
                }

                // new submodule .git file
                if (e.FullPath.EndsWith("\\.git"))
                {
                    return;
                }

                // old submodule .git\index.lock file
                if (e.FullPath.EndsWith("\\.git\\index.lock"))
                {
                    return;
                }

                _workTreeWatcher.EnableRaisingEvents = false;
                ScheduleNextUpdateTime(FileChangedUpdateDelay);
            }
        }

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
            _workTreeWatcher.Dispose();
            _gitDirWatcher.Dispose();
            _timerRefresh.Dispose();
            _statusSequence.Dispose();
        }

        private void EnableRaisingEvents()
        {
            if (_disposed)
            {
                return;
            }

            _workTreeWatcher.EnableRaisingEvents = Directory.Exists(_workTreeWatcher.Path);
            _gitDirWatcher.EnableRaisingEvents = Directory.Exists(_gitDirWatcher.Path)
                    && !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
        }

        private GitStatusMonitorState CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                ThreadHelper.AssertOnUIThread();

                GitStatusMonitorState prevStatus = _currentStatus;
                _currentStatus = value;
                switch (_currentStatus)
                {
                    case GitStatusMonitorState.Stopped:
                        {
                            _timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
                            _consecutiveErrorCount = 0;

                            if (_currentStatus != prevStatus)
                            {
                                lock (_statusSequence)
                                {
                                    if (_commandIsRunningAndNotCancelled)
                                    {
                                        _statusSequence.CancelCurrent();
                                        _commandIsRunningAndNotCancelled = false;
                                    }
                                }

                                InvalidateGitWorkingDirectoryStatus();
                            }
                        }

                        break;

                    case GitStatusMonitorState.Paused:
                        {
                            _timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
                        }

                        break;

                    case GitStatusMonitorState.Inactive:
                        {
                            if (_currentStatus != prevStatus)
                            {
                                InvalidateGitWorkingDirectoryStatus();
                            }
                        }

                        break;

                    case GitStatusMonitorState.Running:
                        {
                            if (prevStatus == GitStatusMonitorState.Inactive || prevStatus == GitStatusMonitorState.Running)
                            {
                                // Timer is already running, schedule a new command only if a command is not running,
                                // to avoid that many commands are started (and cancelled) if quickly switching Inactive/Running
                                // If data has changed when Inactive it should be updated by normal means
                                if (!_commandIsRunningAndNotCancelled)
                                {
                                    ScheduleNextInteractiveTime();
                                }

                                break;
                            }

                            EnableRaisingEvents();

                            // An interactive update may be requested separately
                            ScheduleNextInteractiveTime(FileChangedUpdateDelay);
                            _timerRefresh.Start();
                        }

                        break;

                    default:
                        throw new NotSupportedException();
                }

                GitStatusMonitorStateChanged?.Invoke(this, new GitStatusMonitorStateEventArgs(_currentStatus));
            }
        }

        private IGitModule? Module => UICommandsSource?.UICommands.Module;

        private IGitUICommandsSource? UICommandsSource { get; set; }

        private void Init(IGitUICommandsSource commandsSource)
        {
            UICommandsSource = commandsSource ?? throw new ArgumentNullException(nameof(commandsSource));
            UICommandsSource.UICommandsChanged += commandsSource_GitUICommandsChanged;

            commandsSource_activate(commandsSource);

            return;

            void commandsSource_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                IGitUICommands oldCommands = e.OldCommands;
                if (oldCommands is not null)
                {
                    oldCommands.PreCheckoutBranch -= GitUICommands_PreCheckout;
                    oldCommands.PreCheckoutRevision -= GitUICommands_PreCheckout;
                    oldCommands.PostCheckoutBranch -= GitUICommands_PostCheckout;
                    oldCommands.PostCheckoutRevision -= GitUICommands_PostCheckout;
                    oldCommands.PostRepositoryChanged -= GitUICommands_PostRepositoryChanged;
                }

                commandsSource_activate((IGitUICommandsSource)sender);
            }

            void commandsSource_activate(IGitUICommandsSource sender)
            {
                IGitUICommands newCommands = sender.UICommands;
                newCommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                newCommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                newCommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                newCommands.PostCheckoutRevision += GitUICommands_PostCheckout;
                newCommands.PostRepositoryChanged += GitUICommands_PostRepositoryChanged;

                IGitModule module = newCommands.Module;
                StartWatchingChanges(module.WorkingDir, module.WorkingDirGitDir);
            }

            void GitUICommands_PreCheckout(object sender, GitUIEventArgs e)
            {
                CurrentStatus = GitStatusMonitorState.Paused;
            }

            void GitUICommands_PostCheckout(object sender, GitUIPostActionEventArgs e)
            {
                CurrentStatus = GitStatusMonitorState.Running;
            }

            void GitUICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
            {
                lock (_statusSequence)
                {
                    // First time after open a repo, trigger an update with locked buffers (to speed up subsequent updates)
                    _isFirstPostRepoChanged = true;
                }
            }
        }

        private void StartWatchingChanges(string workTreePath, string gitDirPath)
        {
            try
            {
                bool isValidGitDir = !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath);
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    (isValidGitDir || PathUtil.IsWslLink(gitDirPath)))
                {
                    _workTreeWatcher.Path = workTreePath;
                    _gitPath = Path.GetDirectoryName(gitDirPath);
                    if (isValidGitDir)
                    {
                        _gitDirWatcher.Path = PathUtil.RemoveTrailingPathSeparator(gitDirPath);
                        _submodulesPath = Path.Combine(_gitPath, "modules");
                    }
                    else
                    {
                        // WSL link, FileSystemWatcher will throw on directories that are symbolic links
                        _gitDirWatcher.Path = workTreePath;
                        _submodulesPath = Path.Combine(gitDirPath, "modules");
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
                // no-op
            }
        }

        private void Update()
        {
            ThreadHelper.AssertOnUIThread();

            if (CurrentStatus != GitStatusMonitorState.Running
                && CurrentStatus != GitStatusMonitorState.Inactive)
            {
                return;
            }

            Validates.NotNull(UICommandsSource);
            Validates.NotNull(Module);

            if (_isMinimized() || UICommandsSource.UICommands.RepoChangedNotifier.IsLocked)
            {
                // No run for minimized,
                // don't update status while repository is being modified by GitExt,
                // repository status may change after these actions.
                if (CurrentStatus == GitStatusMonitorState.Running)
                {
                    CurrentStatus = GitStatusMonitorState.Inactive;
                }

                return;
            }
            else if (CurrentStatus == GitStatusMonitorState.Inactive)
            {
                // Schedule a new update
                CurrentStatus = GitStatusMonitorState.Running;
            }

            int commandStartTime = Environment.TickCount;
            if (_nextUpdateTime - commandStartTime > 0)
            {
                return;
            }

            IGitModule module;
            CancellationToken cancelToken;
            bool noLocks;

            lock (_statusSequence)
            {
                if (_disposed)
                {
                    return;
                }

                if (_commandIsRunningAndNotCancelled)
                {
                    return;
                }

                if (!Directory.Exists(_workTreeWatcher.Path))
                {
                    // The directory no longer exists, watcher cannot be enabled
                    return;
                }

                EnableRaisingEvents();

                // capture a consistent state in the main thread
                module = Module;
                noLocks = !_isFirstPostRepoChanged;
                cancelToken = _statusSequence.Next();
                _commandIsRunningAndNotCancelled = true;

                // Schedule periodic update, even if we don't know that anything changed
                _nextUpdateTime = commandStartTime + (PathUtil.IsWslPath(_workTreeWatcher.Path) ? PeriodicUpdateIntervalWSL : PeriodicUpdateInterval);
                _nextEarliestTime = commandStartTime + MinUpdateInterval;
                _isFirstPostRepoChanged = false;
            }

            ThreadHelper.FileAndForget(async () =>
                    {
                        try
                        {
                            ArgumentString cmd = Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.Default, noLocks: noLocks);
                            ExecutionResult result = await module.GitExecutable.ExecuteAsync(cmd, cancellationToken: cancelToken);

                            if (result.ExitedSuccessfully && !ModuleHasChanged())
                            {
                                // Update callers also if cancelled, this is for the correct module
                                string output = result.StandardOutput;
                                IReadOnlyList<GitItemStatus> changedFiles = _getAllChangedFilesOutputParser.Parse(output);

                                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancelToken);
                                GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs(changedFiles));
                            }

                            _consecutiveErrorCount = 0;
                        }
                        catch (OperationCanceledException)
                        {
                            // No action
                        }
                        catch
                        {
                            try
                            {
                                if (++_consecutiveErrorCount < _maxConsecutiveErrors)
                                {
                                    // Try again
                                    ScheduleNextInteractiveTime();
                                    return;
                                }

                                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                                // Avoid possible popups on every file changes
                                CurrentStatus = GitStatusMonitorState.Stopped;
                            }
                            catch
                            {
                                // No action
                            }
                        }
                        finally
                        {
                            lock (_statusSequence)
                            {
                                if (!cancelToken.IsCancellationRequested)
                                {
                                    _commandIsRunningAndNotCancelled = false;
                                    if (!ModuleHasChanged())
                                    {
                                        // Adjust the min time to next update
                                        int endTime = Environment.TickCount;
                                        int commandTime = endTime - commandStartTime;
                                        int minDelay = Math.Max(MinUpdateInterval, 2 * commandTime);
                                        _nextEarliestTime = endTime + minDelay;
                                        if (_nextUpdateTime - commandStartTime < _nextEarliestTime - commandStartTime)
                                        {
                                            // Postpone the requested update
                                            _nextUpdateTime = _nextEarliestTime;
                                        }
                                    }
                                }
                            }
                        }
                    });

            return;

            bool ModuleHasChanged()
            {
                return module != Module;
            }
        }

        /// <summary>
        /// Schedule a status update after the specified delay
        /// Do not change if a value is already set at a earlier time,
        /// but respect the minimal (dynamic) update times between updates.
        /// </summary>
        /// <param name="delay">delay in milliseconds.</param>
        private void ScheduleNextUpdateTime(int delay)
        {
            lock (_statusSequence)
            {
                // Enforce a minimal time between updates, to not update too frequently
                int ticks = Environment.TickCount;
                int currDelay = _nextUpdateTime - ticks;
                int minDelay = Math.Max(delay, _nextEarliestTime - ticks);
                if (minDelay < currDelay)
                {
                    _nextUpdateTime = ticks + minDelay;
                }
            }
        }

        /// <summary>
        /// Schedule a status update from interactive changes (repo changed or refreshed)
        /// Cancel any ongoing requests.
        /// A short delay is added.
        /// </summary>
        /// <param name="delay">delay in milliseconds.</param>
        private void ScheduleNextInteractiveTime(int delay = InteractiveUpdateDelay)
        {
            // Start commands, also if running already
            lock (_statusSequence)
            {
                _statusSequence.CancelCurrent();
                _commandIsRunningAndNotCancelled = false;

                if (_disposed)
                {
                    return;
                }

                int ticks = Environment.TickCount;
                _nextEarliestTime = ticks + MinUpdateInterval;
                int currDelay = _nextUpdateTime - ticks;
                if (delay < currDelay)
                {
                    _nextUpdateTime = ticks + delay;
                }
            }
        }
    }
}
