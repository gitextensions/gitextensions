using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitCommands.Utils;
using GitUIPluginInterfaces;
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

        private readonly FileSystemWatcher _workTreeWatcher = new();
        private readonly FileSystemWatcher _gitDirWatcher = new();
        private readonly System.Windows.Forms.Timer _timerRefresh;
        private bool _commandIsRunning;
        private bool _isFirstPostRepoChanged;
        private string? _gitPath;
        private string? _submodulesPath;
        private readonly CancellationTokenSequence _statusSequence = new();
        private readonly GetAllChangedFilesOutputParser _getAllChangedFilesOutputParser;

        // Timestamps to schedule status updates, limit the update interval dynamically
        // Note that TickCount wraps after 25 days uptime, always compare diff
        private int _nextUpdateTime;
        private int _nextEarliestTime;

        // Track changes detected (used while command is running)
        private bool _pendingUpdate;
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

        public GitStatusMonitor(IGitUICommandsSource commandsSource)
        {
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

            _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(() => commandsSource.UICommands.GitModule);

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
            _workTreeWatcher.Dispose();
            _gitDirWatcher.Dispose();
            _timerRefresh.Dispose();
            _statusSequence.Dispose();
        }

        private bool GitDirWatcherEnableRaisingEvents()
        {
            return Directory.Exists(_gitDirWatcher.Path)
                    && !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
        }

        private GitStatusMonitorState CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                var prevStatus = _currentStatus;
                _currentStatus = value;
                switch (_currentStatus)
                {
                    case GitStatusMonitorState.Stopped:
                        {
                            _timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
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
                            if (prevStatus != GitStatusMonitorState.Inactive)
                            {
                                InvalidateGitWorkingDirectoryStatus();
                            }
                        }

                        break;

                    case GitStatusMonitorState.Running:
                        {
                            if (prevStatus == GitStatusMonitorState.Inactive)
                            {
                                // Timer is already running, schedule new update
                                ScheduleNextInteractiveTime();
                                break;
                            }

                            _workTreeWatcher.EnableRaisingEvents = Directory.Exists(_workTreeWatcher.Path);
                            _gitDirWatcher.EnableRaisingEvents = GitDirWatcherEnableRaisingEvents();

                            lock (_statusSequence)
                            {
                                // An interactive update will be requested separately
                                _nextUpdateTime
                                    = _nextEarliestTime
                                    = Environment.TickCount + FileChangedUpdateDelay;
                                _pendingUpdate = true;
                                _commandIsRunning = false;
                                _statusSequence.CancelCurrent();
                            }

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
                var oldCommands = e.OldCommands;
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
                var newCommands = sender.UICommands;
                newCommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                newCommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                newCommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                newCommands.PostCheckoutRevision += GitUICommands_PostCheckout;
                newCommands.PostRepositoryChanged += GitUICommands_PostRepositoryChanged;

                var module = newCommands.Module;
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
                CurrentStatus = GitStatusMonitorState.Inactive;
                CurrentStatus = GitStatusMonitorState.Running;
                _isFirstPostRepoChanged = true;
            }
        }

        private void StartWatchingChanges(string workTreePath, string gitDirPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath))
                {
                    _workTreeWatcher.Path = workTreePath;
                    _gitDirWatcher.Path = gitDirPath;
                    _gitPath = Path.GetDirectoryName(gitDirPath);
                    _submodulesPath = Path.Combine(_gitPath, "modules");

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

        private static bool IsMinimized()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return false;
            }

            var currentProcess = Process.GetCurrentProcess();
            if (currentProcess is null)
            {
                return false;
            }

            return NativeMethods.IsIconic(currentProcess.MainWindowHandle).IsTrue();
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

            if (IsMinimized()
                || UICommandsSource.UICommands.RepoChangedNotifier.IsLocked ||
                (GitVersion.Current.RaceConditionWhenGitStatusIsUpdatingIndex && Module.IsRunningGitProcess()))
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
                CurrentStatus = GitStatusMonitorState.Running;
            }

            if (_nextUpdateTime - Environment.TickCount > 0)
            {
                return;
            }

            IGitModule module;
            CancellationToken cancelToken;
            int commandStartTime;

            lock (_statusSequence)
            {
                if (_commandIsRunning)
                {
                    return;
                }

                if (!Directory.Exists(_workTreeWatcher.Path))
                {
                    // The directory no longer exists, watcher cannot be enabled
                    return;
                }

                _workTreeWatcher.EnableRaisingEvents = true;
                _gitDirWatcher.EnableRaisingEvents = GitDirWatcherEnableRaisingEvents();

                // capture a consistent state in the main thread
                module = Module;
                cancelToken = _statusSequence.Next();

                // Schedule update every 5 min, even if we don't know that anything changed
                _nextUpdateTime = Environment.TickCount + PeriodicUpdateInterval;
                commandStartTime = Environment.TickCount;
                _pendingUpdate = false;
                _commandIsRunning = true;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        bool isSuccess = false;
                        try
                        {
                            var cmd = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default,
                                noLocks: !_isFirstPostRepoChanged);
                            _isFirstPostRepoChanged = false;
                            var output = await module.GitExecutable.GetOutputAsync(cmd).ConfigureAwait(false);
                            IReadOnlyList<GitItemStatus> changedFiles = _getAllChangedFilesOutputParser.Parse(output);

                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                            if (!ModuleHasChanged())
                            {
                                isSuccess = !cancelToken.IsCancellationRequested;

                                // Update callers also if cancelled, this is for the correct module
                                GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs(changedFiles));
                            }
                        }
                        catch
                        {
                            try
                            {
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
                            if (!ModuleHasChanged() && !cancelToken.IsCancellationRequested)
                            {
                                lock (_statusSequence)
                                {
                                    if (isSuccess)
                                    {
                                        // Adjust the min time to next update
                                        var commandTime = Environment.TickCount - commandStartTime;
                                        _nextEarliestTime = commandStartTime + Math.Max(MinUpdateInterval, 3 * commandTime);
                                        if (_pendingUpdate)
                                        {
                                            // Schedule the update, a request is pending
                                            _nextUpdateTime = _nextEarliestTime;
                                        }
                                    }

                                    _commandIsRunning = false;
                                }
                            }
                        }
                    })
                .FileAndForget();

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
        /// <param name="delay">delay in milli seconds.</param>
        private void ScheduleNextUpdateTime(int delay)
        {
            lock (_statusSequence)
            {
                // If the previous status call hasn't exited yet,
                // schedule new update when command is finished
                _pendingUpdate = true;

                var ticks = Environment.TickCount;
                var currDelay = _nextUpdateTime - ticks;
                if (delay < currDelay)
                {
                    // Enforce a minimal time between updates, to not update too frequently
                    var minDelay = _nextEarliestTime - ticks;
                    _nextUpdateTime = ticks + Math.Max(delay, minDelay);
                }
            }
        }

        /// <summary>
        /// Schedule a status update from interactive changes (repo changed or refreshed)
        /// A short delay is added.
        /// </summary>
        private void ScheduleNextInteractiveTime()
        {
            // Start commands, also if running already
            lock (_statusSequence)
            {
                _commandIsRunning = false;
                _statusSequence.CancelCurrent();
                _nextUpdateTime
                    = _nextEarliestTime
                    = Environment.TickCount + InteractiveUpdateDelay;
            }
        }
    }
}
