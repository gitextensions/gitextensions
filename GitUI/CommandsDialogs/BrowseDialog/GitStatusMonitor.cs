using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed class GitStatusMonitor : IDisposable
    {
        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 1000;

        /// <summary>
        /// Minimum interval between subsequent updates
        /// </summary>
        private const int MinUpdateInterval = 3000;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private bool _commandIsRunning;
        private bool _statusIsUpToDate = true;
        private bool _ignoredFilesPending;

        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _globalIgnoreWatcher = new FileSystemWatcher();

        private readonly Timer _timerRefresh;
        private readonly Timer _ignoredFilesTimer;

        private string _globalIgnoreFilePath;
        private bool _ignoredFilesAreStale;
        private string _gitPath;
        private string _submodulesPath;

        // Timestamps to schedule status updates, limit the update interval dynamically
        private int _nextUpdateTime;
        private int _previousUpdateTime;
        private int _currentUpdateInterval = MinUpdateInterval;
        private GitStatusMonitorState _currentStatus;
        private HashSet<string> _ignoredFiles = new HashSet<string>();

        /// <summary>
        /// Occurs whenever git status monitor state changes.
        /// </summary>
        public event EventHandler<GitStatusMonitorStateEventArgs> GitStatusMonitorStateChanged;

        /// <summary>
        /// Occurs whenever current working directory status changes.
        /// </summary>
        public event EventHandler<GitWorkingDirectoryStatusEventArgs> GitWorkingDirectoryStatusChanged;

        public GitStatusMonitor()
        {
            _timerRefresh = new Timer
            {
                Enabled = true,
                Interval = 500
            };
            _timerRefresh.Tick += delegate { Update(); };

            _ignoredFilesTimer = new Timer { Interval = MaxUpdatePeriod };
            _ignoredFilesTimer.Tick += delegate { _ignoredFilesAreStale = true; };

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

            // Setup a file watcher to detect changes to the global ignore file. When it
            // changes, we'll update our status.
            _globalIgnoreWatcher.EnableRaisingEvents = false;
            _globalIgnoreWatcher.Changed += GlobalIgnoreChanged;
            _globalIgnoreWatcher.Created += GlobalIgnoreChanged;
            _globalIgnoreWatcher.Deleted += GlobalIgnoreChanged;
            _globalIgnoreWatcher.Renamed += GlobalIgnoreChanged;
            _globalIgnoreWatcher.Error += WorkTreeWatcherError;
            _globalIgnoreWatcher.IncludeSubdirectories = false;
            _globalIgnoreWatcher.NotifyFilter = NotifyFilters.LastWrite;

            return;

            void WorkTreeWatcherError(object sender, ErrorEventArgs e)
            {
                // Called for instance at buffer overflow
                CalculateNextUpdateTime(UpdateDelay);
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

                CalculateNextUpdateTime(UpdateDelay);
            }

            void WorkTreeChanged(object sender, FileSystemEventArgs e)
            {
                // Update already scheduled?
                if (_nextUpdateTime < Environment.TickCount + UpdateDelay)
                {
                    return;
                }

                var fileName = e.FullPath.Substring(_workTreeWatcher.Path.Length).ToPosixPath();
                if (_ignoredFiles.Contains(fileName))
                {
                    return;
                }

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

                CalculateNextUpdateTime(UpdateDelay);
            }

            void GlobalIgnoreChanged(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == _globalIgnoreFilePath)
                {
                    _ignoredFilesAreStale = true;
                    CalculateNextUpdateTime(UpdateDelay);
                }
            }
        }

        public void Dispose()
        {
            _workTreeWatcher.Dispose();
            _gitDirWatcher.Dispose();
            _globalIgnoreWatcher.Dispose();
            _ignoredFilesTimer.Dispose();
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
                            _globalIgnoreWatcher.EnableRaisingEvents = false;
                        }

                        break;

                    case GitStatusMonitorState.Paused:
                        {
                            _timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
                            _globalIgnoreWatcher.EnableRaisingEvents = false;
                        }

                        break;

                    case GitStatusMonitorState.Running:
                        {
                            _timerRefresh.Start();
                            _workTreeWatcher.EnableRaisingEvents = true;
                            _gitDirWatcher.EnableRaisingEvents = !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
                            _globalIgnoreWatcher.EnableRaisingEvents = !string.IsNullOrWhiteSpace(_globalIgnoreWatcher.Path);
                            CalculateNextUpdateTime(UpdateDelay);
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

        public void Init(IGitUICommandsSource commandsSource)
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
                    oldCommands.PostEditGitIgnore -= GitUICommands_PostEditGitIgnore;
                }

                commandsSource_activate(sender as IGitUICommandsSource);
            }

            void commandsSource_activate(IGitUICommandsSource sender)
            {
                var newCommands = sender.UICommands;
                if (newCommands != null)
                {
                    newCommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                    newCommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                    newCommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                    newCommands.PostCheckoutRevision += GitUICommands_PostCheckout;
                    newCommands.PostEditGitIgnore += GitUICommands_PostEditGitIgnore;

                    var module = newCommands.Module;
                    StartWatchingChanges(module.WorkingDir, module.WorkingDirGitDir);
                }
            }

            void GitUICommands_PreCheckout(object sender, GitUIEventArgs e)
            {
                CurrentStatus = GitStatusMonitorState.Paused;
            }

            void GitUICommands_PostCheckout(object sender, GitUIPostActionEventArgs e)
            {
                CurrentStatus = GitStatusMonitorState.Running;
            }

            void GitUICommands_PostEditGitIgnore(object sender, GitUIEventArgs e)
            {
                _ignoredFiles = new HashSet<string>();
                _ignoredFilesAreStale = true;
            }
        }

        private void StartWatchingChanges(string workTreePath, string gitDirPath)
        {
            // reset status info, it was outdated
            GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs());

            try
            {
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath))
                {
                    _workTreeWatcher.Path = workTreePath;
                    _gitDirWatcher.Path = gitDirPath;
                    _globalIgnoreFilePath = DetermineGlobalIgnoreFilePath();
                    string globalIgnoreDirectory = Path.GetDirectoryName(_globalIgnoreFilePath);
                    if (Directory.Exists(globalIgnoreDirectory))
                    {
                        _globalIgnoreWatcher.Path = globalIgnoreDirectory;
                    }

                    _gitPath = Path.GetDirectoryName(gitDirPath);
                    _submodulesPath = Path.Combine(_gitPath, "modules");
                    _currentUpdateInterval = MinUpdateInterval;
                    _previousUpdateTime = 0;
                    _ignoredFilesAreStale = true;
                    _ignoredFiles = new HashSet<string>();
                    _ignoredFilesTimer.Stop();
                    _ignoredFilesTimer.Start();
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

            return;

            string DetermineGlobalIgnoreFilePath()
            {
                // According to https://git-scm.com/docs/git-config, the following are checked in order:
                //  - core.excludesFile configuration,
                //  - $XDG_CONFIG_HOME/git/ignore, if XDG_CONFIG_HOME is set and not empty,
                //  - $HOME/.config/git/ignore.

                string globalExcludeFile = Module.GetEffectiveSetting("core.excludesFile");
                if (!string.IsNullOrWhiteSpace(globalExcludeFile))
                {
                    return Path.GetFullPath(globalExcludeFile);
                }

                string xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (!string.IsNullOrWhiteSpace(xdgConfigHome))
                {
                    return Path.GetFullPath(Path.Combine(xdgConfigHome, "git/ignore"));
                }

                return Path.GetFullPath(Path.Combine(EnvironmentConfiguration.GetHomeDir(), ".config/git/ignore"));
            }
        }

        private void Update()
        {
            if (CurrentStatus != GitStatusMonitorState.Running)
            {
                return;
            }

            if (Environment.TickCount < _nextUpdateTime && (Environment.TickCount >= 0 || _nextUpdateTime <= 0))
            {
                return;
            }

            // If the previous status call hasn't exited yet, we'll wait until it is
            // so we don't queue up a bunch of commands
            if (_commandIsRunning ||

                // don't update status while repository is being modified by GitExt
                // or while any git process is running, mostly repository status will change
                // after these actions. Moreover, calling git status while other git command is performed
                // can cause repository crash
                UICommandsSource.UICommands.RepoChangedNotifier.IsLocked ||
                (GitCommandHelpers.VersionInUse.RaceConditionWhenGitStatusIsUpdatingIndex && Module.IsRunningGitProcess()))
            {
                _statusIsUpToDate = false; // tell that computed status isn't up to date
                return;
            }

            _commandIsRunning = true;
            _statusIsUpToDate = true;
            _previousUpdateTime = Environment.TickCount;
            AsyncLoader.DoAsync(RunStatusCommand, UpdatedStatusReceived, OnUpdateStatusError);

            // Schedule update every 5 min, even if we don't know that anything changed
            CalculateNextUpdateTime(MaxUpdatePeriod);

            return;

            string RunStatusCommand()
            {
                _ignoredFilesPending = _ignoredFilesAreStale;

                // git-status with ignored files when needed only
                string command = GitCommandHelpers.GetAllChangedFilesCmd(!_ignoredFilesPending, UntrackedFilesMode.Default, noLocks: true);
                return Module.RunGitCmd(command);
            }

            void UpdatedStatusReceived(string updatedStatus)
            {
                // Adjust the interval between updates. (This does not affect an update already scheduled).
                _currentUpdateInterval = Math.Max(MinUpdateInterval, 3 * (Environment.TickCount - _previousUpdateTime));
                _commandIsRunning = false;

                if (CurrentStatus != GitStatusMonitorState.Running)
                {
                    return;
                }

                var allChangedFiles = GitCommandHelpers.GetStatusChangedFilesFromString(Module, updatedStatus);
                GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs(allChangedFiles.Where(item => !item.IsIgnored)));
                if (_ignoredFilesPending)
                {
                    _ignoredFilesPending = false;
                    _ignoredFiles = new HashSet<string>(allChangedFiles.Where(item => item.IsIgnored).Select(item => item.Name));
                    if (_statusIsUpToDate)
                    {
                        _ignoredFilesAreStale = false;
                    }
                }

                if (!_statusIsUpToDate)
                {
                    // Still not up-to-date, but present what received, GetAllChangedFilesCmd() is the heavy command
                    CalculateNextUpdateTime(UpdateDelay);
                }
            }

            void OnUpdateStatusError(AsyncErrorEventArgs e)
            {
                _commandIsRunning = false;
                CurrentStatus = GitStatusMonitorState.Stopped;
            }
        }

        private void CalculateNextUpdateTime(int delay)
        {
            var next = Environment.TickCount + delay;
            if (_nextUpdateTime > Environment.TickCount)
            {
                // A time is already set, use closest
                next = Math.Min(_nextUpdateTime, next);
            }

            // Enforce a minimal time between updates, to not update too frequently
            _nextUpdateTime = Math.Max(next, _previousUpdateTime + _currentUpdateInterval);
        }
    }
}