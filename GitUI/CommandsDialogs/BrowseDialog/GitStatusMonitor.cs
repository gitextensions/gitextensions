using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

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
        /// Minimum interval between subsequent updates
        /// </summary>
        private const int MinUpdateInterval = 3000;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int PeriodicUpdateInterval = 5 * 60 * 1000;

        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private readonly Timer _timerRefresh;
        private bool _commandIsRunning;
        private string _gitPath;
        private string _submodulesPath;

        // Timestamps to schedule status updates, limit the update interval dynamically
        private int _nextUpdateTime;
        private int _nextEarliestTime;
        private bool _nextIsInteractive;
        private GitStatusMonitorState _currentStatus;

        public bool Active
        {
            get => CurrentStatus != GitStatusMonitorState.Stopped;
            set => CurrentStatus = value ? GitStatusMonitorState.Running : GitStatusMonitorState.Stopped;
        }

        /// <summary>
        /// Occurs whenever git status monitor state changes.
        /// </summary>
        public event EventHandler<GitStatusMonitorStateEventArgs> GitStatusMonitorStateChanged;

        /// <summary>
        /// Occurs whenever current working directory status changes.
        /// </summary>
        public event EventHandler<GitWorkingDirectoryStatusEventArgs> GitWorkingDirectoryStatusChanged;

        public GitStatusMonitor(IGitUICommandsSource commandsSource)
        {
            _timerRefresh = new Timer
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

            return;

            void WorkTreeWatcherError(object sender, ErrorEventArgs e)
            {
                // Called for instance at buffer overflow
                ScheduleNextUpdateTime(FileChangedUpdateDelay);
            }

            void GitDirChanged(object sender, FileSystemEventArgs e)
            {
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

                ScheduleNextInteractiveTime();
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

                ScheduleNextUpdateTime(FileChangedUpdateDelay);
                _workTreeWatcher.EnableRaisingEvents = false;
            }
        }

        public void InvalidateGitWorkingDirectoryStatus()
        {
            GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs());
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
        }

        private GitStatusMonitorState CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
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

                    case GitStatusMonitorState.Running:
                        {
                            _timerRefresh.Start();
                            _workTreeWatcher.EnableRaisingEvents = true;
                            _gitDirWatcher.EnableRaisingEvents = !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
                            ScheduleNextInteractiveTime();
                        }

                        break;

                    default:
                        throw new NotSupportedException();
                }

                GitStatusMonitorStateChanged?.Invoke(this, new GitStatusMonitorStateEventArgs(_currentStatus));
            }
        }

        private IGitModule Module => UICommandsSource.UICommands.Module;

        private IGitUICommandsSource UICommandsSource { get; set; }

        private void Init(IGitUICommandsSource commandsSource)
        {
            UICommandsSource = commandsSource ?? throw new ArgumentNullException(nameof(commandsSource));
            UICommandsSource.UICommandsChanged += commandsSource_GitUICommandsChanged;

            commandsSource_activate(commandsSource);

            return;

            void commandsSource_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                var oldCommands = e.OldCommands;
                if (oldCommands != null)
                {
                    oldCommands.PreCheckoutBranch -= GitUICommands_PreCheckout;
                    oldCommands.PreCheckoutRevision -= GitUICommands_PreCheckout;
                    oldCommands.PostCheckoutBranch -= GitUICommands_PostCheckout;
                    oldCommands.PostCheckoutRevision -= GitUICommands_PostCheckout;
                }

                commandsSource_activate(sender as IGitUICommandsSource);
            }

            void commandsSource_activate(IGitUICommandsSource sender)
            {
                var newCommands = sender.UICommands;
                newCommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                newCommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                newCommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                newCommands.PostCheckoutRevision += GitUICommands_PostCheckout;

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

        private void Update()
        {
            ThreadHelper.AssertOnUIThread();

            if (CurrentStatus != GitStatusMonitorState.Running)
            {
                return;
            }

            if (Environment.TickCount < _nextUpdateTime)
            {
                return;
            }

            // If the previous status call hasn't exited yet,
            // schedule new update when command is finished
            if (_commandIsRunning)
            {
                ScheduleNextUpdateTime(0);
                return;
            }

            // don't update status while repository is being modified by GitExt,
            // repository status will change after these actions.
            if (UICommandsSource.UICommands.RepoChangedNotifier.IsLocked ||
                (GitVersion.Current.RaceConditionWhenGitStatusIsUpdatingIndex && Module.IsRunningGitProcess()))
            {
                ScheduleNextUpdateTime(0);
                return;
            }

            _commandIsRunning = true;
            _nextIsInteractive = false;
            var commandStartTime = Environment.TickCount;
            _workTreeWatcher.EnableRaisingEvents = true;

            // Schedule update every 5 min, even if we don't know that anything changed
            ScheduleNextUpdateTime(PeriodicUpdateInterval);

            // capture a consistent state in the main thread
            IGitModule module = Module;

            ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await TaskScheduler.Default;

                            var cmd = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default,
                                noLocks: true);
                            var output = module.GitExecutable.GetOutput(cmd);
                            var changedFiles = GitCommandHelpers.GetStatusChangedFilesFromString(module, output);

                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                            if (!ModuleHasChanged())
                            {
                                UpdatedStatusReceived(changedFiles);
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

                            throw;
                        }
                        finally
                        {
                            _commandIsRunning = false;
                        }
                    })
                .FileAndForget();

            return;

            bool ModuleHasChanged()
            {
                return module != Module;
            }

            void UpdatedStatusReceived(IEnumerable<GitItemStatus> changedFiles)
            {
                // Adjust the interval between updates, schedule new to recalculate
                _nextEarliestTime = commandStartTime +
                                    Math.Max(MinUpdateInterval, 3 * (Environment.TickCount - commandStartTime));
                ScheduleNextUpdateTime(PeriodicUpdateInterval);

                GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs(changedFiles));
            }
        }

        /// <summary>
        /// Schedule a status update after the specified delay
        /// Do not change if a value is already set at a earlier time,
        /// but respect the minimal (dynamic) update times between updates
        /// </summary>
        /// <param name="delay">delay in milli seconds</param>
        private void ScheduleNextUpdateTime(int delay)
        {
            var next = Environment.TickCount + delay;
            if (_nextUpdateTime > Environment.TickCount)
            {
                // A time is already set, use closest
                next = Math.Min(_nextUpdateTime, next);
            }

            // timer wraps after 25 days uptime
            if (_nextUpdateTime < 0 && _nextEarliestTime > 0)
            {
                _nextEarliestTime = next;
            }

            if (!_nextIsInteractive)
            {
                // Enforce a minimal time between updates, to not update too frequently
                _nextUpdateTime = Math.Max(next, _nextEarliestTime);
            }
        }

        /// <summary>
        /// Schedule a status update from interactive changes (repo changed or refreshed)
        /// A short delay is added
        /// </summary>
        private void ScheduleNextInteractiveTime()
        {
            // Start commands, also if running already
            _commandIsRunning = false;
            _nextIsInteractive = true;
            _nextUpdateTime = Environment.TickCount + InteractiveUpdateDelay;
        }
    }
}