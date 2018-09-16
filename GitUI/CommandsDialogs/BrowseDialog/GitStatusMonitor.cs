using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using CancellationToken = System.Threading.CancellationToken;

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

        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _globalIgnoreWatcher = new FileSystemWatcher();

        private readonly Timer _timerRefresh;

        private string _globalIgnoreFilePath;
        private string _gitPath;
        private string _submodulesPath;

        // Timestamps to schedule status updates, limit the update interval dynamically
        private int _nextUpdateTime;
        private int _previousUpdateTime;
        private int _currentUpdateInterval = MinUpdateInterval;
        private GitStatusMonitorState _currentStatus;
        private IgnoredFilesCache _ignoredFilesCache;
        private readonly CancellationTokenSequence _statusCancellation = new CancellationTokenSequence();
        private CancellationToken _statusCancellationToken;

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
            _statusCancellationToken = _statusCancellation.Next();
            _timerRefresh = new Timer
            {
                Enabled = true,
                Interval = 500
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

            Init(commandsSource);

            _ignoredFilesCache = new IgnoredFilesCache(new GitModule(null));

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
                if (_nextUpdateTime < Environment.TickCount + UpdateDelay)
                {
                    return;
                }

                var fileName = e.FullPath.Substring(_workTreeWatcher.Path.Length).ToPosixPath();
                if (_ignoredFilesCache.IsIgnored(fileName))
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

                _ignoredFilesCache.AddCandidate(fileName);
                CalculateNextUpdateTime(UpdateDelay);
            }

            void GlobalIgnoreChanged(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == _globalIgnoreFilePath)
                {
                    _ignoredFilesCache.ClearCachedData();
                    CalculateNextUpdateTime(UpdateDelay);
                }
            }
        }

        private bool IsAboutToUpdate()
        {
            return _nextUpdateTime < Environment.TickCount + UpdateDelay && _nextUpdateTime >= Environment.TickCount;
        }

        public void Dispose()
        {
            _statusCancellation.Dispose();
            _workTreeWatcher.Dispose();
            _gitDirWatcher.Dispose();
            _globalIgnoreWatcher.Dispose();
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
                    oldCommands.PostEditGitIgnore -= GitUICommands_PostEditGitIgnore;
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
                newCommands.PostEditGitIgnore += GitUICommands_PostEditGitIgnore;

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

            void GitUICommands_PostEditGitIgnore(object sender, GitUIEventArgs e)
            {
                _ignoredFilesCache.ClearCachedData();
            }
        }

        private void StartWatchingChanges(string workTreePath, string gitDirPath)
        {
            _statusCancellationToken = _statusCancellation.Next();

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
                    _ignoredFilesCache = new IgnoredFilesCache(Module);
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
            ThreadHelper.AssertOnUIThread();

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
                (GitVersion.Current.RaceConditionWhenGitStatusIsUpdatingIndex && Module.IsRunningGitProcess()))
            {
                _statusIsUpToDate = false; // tell that computed status isn't up to date
                return;
            }

            _commandIsRunning = true;
            _statusIsUpToDate = true;
            _previousUpdateTime = Environment.TickCount;

            // capture a consistent state in the main thread
            var statusCancellationToken = _statusCancellationToken;
            var ignoredFilesCache = _ignoredFilesCache;
            IGitModule module = Module;

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        var changedFiles = await GetChangedFilesAsync().ConfigureAwait(false);
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(statusCancellationToken);

                        UpdatedStatusReceived(changedFiles);
                        UpdateIgnoredCacheAsync(changedFiles).FileAndForget();
                    }
                    finally
                    {
                        _commandIsRunning = false;

                        // Schedule update every 5 min, even if we don't know that anything changed
                        CalculateNextUpdateTime(MaxUpdatePeriod);
                    }
                })
                .FileAndForget();

            return;

            async Task<IEnumerable<GitItemStatus>> GetChangedFilesAsync()
            {
                try
                {
                    await TaskScheduler.Default;
                    if (statusCancellationToken.IsCancellationRequested)
                    {
                        return Enumerable.Empty<GitItemStatus>();
                    }

                    var cmd = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default, noLocks: true);
                    var output = module.RunGitCmd(cmd);
                    return GitCommandHelpers.GetStatusChangedFilesFromString(module, output);
                }
                catch
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(statusCancellationToken);
                    CurrentStatus = GitStatusMonitorState.Stopped;

                    throw;
                }
            }

            async Task UpdateIgnoredCacheAsync(IEnumerable<GitItemStatus> changedFiles)
            {
                await TaskScheduler.Default;
                if (statusCancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ignoredFilesCache.Update(changedFiles);

                if (IsAboutToUpdate())
                {
                    return;
                }

                ignoredFilesCache.VerifyIgnoredCandidates();
            }

            void UpdatedStatusReceived(IEnumerable<GitItemStatus> changedFiles)
            {
                // Adjust the interval between updates. (This does not affect an update already scheduled).
                _currentUpdateInterval = Math.Max(MinUpdateInterval, 3 * (Environment.TickCount - _previousUpdateTime));

                if (CurrentStatus != GitStatusMonitorState.Running)
                {
                    return;
                }

                GitWorkingDirectoryStatusChanged?.Invoke(this, new GitWorkingDirectoryStatusEventArgs(changedFiles));

                if (!_statusIsUpToDate)
                {
                    // Still not up-to-date, but present what received, GetAllChangedFilesCmd() is the heavy command
                    CalculateNextUpdateTime(UpdateDelay);
                }
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

    // TODO: move to GitCommands project and add tests
    public sealed class IgnoredFilesCache
    {
        private readonly IGitModule _gitModule;
        private readonly ConcurrentDictionary<string, object> _ignoredCandidates = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _ignoredFiles = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _trackedSeen = new ConcurrentDictionary<string, object>();

        public IgnoredFilesCache(IGitModule gitModule)
        {
            _gitModule = gitModule;
        }

        public void AddCandidate(string fileName)
        {
            // TODO: validate input parameters

            if (!_trackedSeen.ContainsKey(fileName))
            {
                _ignoredCandidates.TryAdd(fileName, null);
            }
        }

        public void ClearCachedData()
        {
            _ignoredFiles.Clear();
            _trackedSeen.Clear();
        }

        public bool IsIgnored(string fileName)
        {
            // TODO: validate input parameters

            return _ignoredFiles.ContainsKey(fileName);
        }

        public void Update(IEnumerable<GitItemStatus> changedFiles)
        {
            // TODO: validate input parameters
            foreach (var file in changedFiles)
            {
                if (file.IsTracked)
                {
                    _trackedSeen.TryAdd(file.Name, null);
                }

                _ignoredCandidates.TryRemove(file.Name, out var _);
            }
        }

        public void VerifyIgnoredCandidates()
        {
            var candidates = _ignoredCandidates.Keys.ToArray();

            if (candidates.Length == 0)
            {
                return;
            }

            var ignoredFiles = GetIgnoredFiles();
            UpdateIgnoredFiles();
            RemoveIgnoredCandidates();

            return;

            IEnumerable<GitItemStatus> GetIgnoredFiles()
            {
                var candidatesDirs = candidates.Select(Path.GetDirectoryName).ToHashSet();
                var dirsArg = string.Join(" ", candidatesDirs.Select(dir => dir.Quote()));
                var cmd = GitCommandHelpers.GetAllChangedFilesCmd(false, UntrackedFilesMode.Default, noLocks: true);
                cmd += " -- " + dirsArg;
                var output = _gitModule.RunGitCmd(cmd);
                return GitCommandHelpers.GetStatusChangedFilesFromString(_gitModule, output).Where(file => file.IsIgnored);
            }

            void RemoveIgnoredCandidates()
            {
                foreach (var file in candidates)
                {
                    _ignoredCandidates.TryRemove(file, out var _);
                }
            }

            void UpdateIgnoredFiles()
            {
                foreach (var file in ignoredFiles)
                {
                    _ignoredFiles.TryAdd(file.Name, null);
                }
            }
        }
    }
}