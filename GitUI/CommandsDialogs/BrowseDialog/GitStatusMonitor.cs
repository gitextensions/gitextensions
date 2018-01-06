using System;
using System.Collections.Generic;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class GitStatusMonitor : IDisposable
    {
        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 500;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private bool _commandIsRunning;
        private bool _statusIsUpToDate = true;
        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _globalIgnoreWatcher = new FileSystemWatcher();
        private string _globalIgnoreFilePath;
        private bool _ignoredFilesAreStale;
        private string _gitPath;
        private string _submodulesPath;
        private int _nextUpdateTime;
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
            InitializeComponent();

            ignoredFilesTimer.Interval = MaxUpdatePeriod;
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
                            timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
                            _globalIgnoreWatcher.EnableRaisingEvents = false;
                        }
                        break;

                    case GitStatusMonitorState.Paused:
                        {
                            timerRefresh.Stop();
                            _workTreeWatcher.EnableRaisingEvents = false;
                            _gitDirWatcher.EnableRaisingEvents = false;
                            _globalIgnoreWatcher.EnableRaisingEvents = false;
                        }
                        break;

                    case GitStatusMonitorState.Running:
                        {
                            timerRefresh.Start();
                            _workTreeWatcher.EnableRaisingEvents = true;
                            _gitDirWatcher.EnableRaisingEvents = !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
                            _globalIgnoreWatcher.EnableRaisingEvents = !string.IsNullOrWhiteSpace(_globalIgnoreWatcher.Path);
                            ScheduleDeferredUpdate();
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }
                OnGitStatusMonitorStateChanged(new GitStatusMonitorStateEventArgs(_currentStatus));
            }
        }

        private IGitModule Module => UICommandsSource.UICommands.Module;

        private IGitUICommandsSource UICommandsSource { get; set; }


        public void Init(IGitUICommandsSource commandsSource)
        {
            if (commandsSource == null)
            {
                throw new ArgumentNullException(nameof(commandsSource));
            }
            UICommandsSource = commandsSource;
            UICommandsSource.GitUICommandsChanged += commandsSource_GitUICommandsChanged;
            commandsSource_activate(commandsSource);
        }


        protected void OnGitStatusMonitorStateChanged(GitStatusMonitorStateEventArgs e)
        {
            GitStatusMonitorStateChanged?.Invoke(this, e);
        }

        protected void OnGitWorkingDirectoryStatusChanged(GitWorkingDirectoryStatusEventArgs e)
        {
            GitWorkingDirectoryStatusChanged?.Invoke(this, e);
        }


        private void GlobalIgnoreChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _globalIgnoreFilePath)
            {
                _ignoredFilesAreStale = true;
                ScheduleDeferredUpdate();
            }
        }

        /// <summary>
        /// Determine what file contains the global ignores.
        /// </summary>
        /// <remarks>
        /// According to https://git-scm.com/docs/git-config, the following are checked in order:
        ///  - core.excludesFile configuration,
        ///  - $XDG_CONFIG_HOME/git/ignore, if XDG_CONFIG_HOME is set and not empty,
        ///  - $HOME/.config/git/ignore.
        ///  </remarks>
        private string DetermineGlobalIgnoreFilePath()
        {
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

            return Path.GetFullPath(Path.Combine(GitCommandHelpers.GetHomeDir(), ".config/git/ignore"));
        }

        private void StartWatchingChanges(string workTreePath, string gitDirPath)
        {
            // reset status info, it was outdated
            OnGitWorkingDirectoryStatusChanged(new GitWorkingDirectoryStatusEventArgs());

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
                    UpdateIgnoredFiles(true);
                    ignoredFilesTimer.Stop();
                    ignoredFilesTimer.Start();
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

        private HashSet<string> LoadIgnoredFiles()
        {
            string lsOutput = Module.RunGitCmd("ls-files -o -i --exclude-standard");
            string[] tab = lsOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(tab);
        }

        private void UpdateIgnoredFiles(bool clearImmediately)
        {
            if (clearImmediately)
                _ignoredFiles = new HashSet<string>();

            AsyncLoader.DoAsync(
                LoadIgnoredFiles,
                (ignoredSet) =>
                {
                    _ignoredFiles = ignoredSet;
                    _ignoredFilesAreStale = false;
                },
                (e) => { _ignoredFiles = new HashSet<string>(); }
            );
        }

        private void Update()
        {
            if (CurrentStatus != GitStatusMonitorState.Running)
                return;

            if (Environment.TickCount >= _nextUpdateTime ||
                (Environment.TickCount < 0 && _nextUpdateTime > 0))
            {
                // If the previous status call hasn't exited yet, we'll wait until it is
                // so we don't queue up a bunch of commands
                if (_commandIsRunning ||
                    //don't update status while repository is being modyfied by GitExt
                    //or while any git process is running, mostly repository status will change
                    //after these actions. Moreover, calling git status while other git command is performed
                    //can cause repository crash
                    UICommandsSource.UICommands.RepoChangedNotifier.IsLocked ||
                    GitCommandHelpers.VersionInUse.RaceConditionWhenGitStatusIsUpdatingIndex && Module.IsRunningGitProcess())
                {
                    _statusIsUpToDate = false; //tell that computed status isn't up to date
                    return;
                }

                _commandIsRunning = true;
                _statusIsUpToDate = true;
                if (_ignoredFilesAreStale)
                {
                    UpdateIgnoredFiles(false);
                }
                AsyncLoader.DoAsync(RunStatusCommand, UpdatedStatusReceived, OnUpdateStatusError);
                // Always update every 5 min, even if we don't know anything changed
                ScheduleNextJustInCaseUpdate();
            }
        }

        private string RunStatusCommand()
        {
            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default);
            return Module.RunGitCmd(command);
        }

        private void OnUpdateStatusError(AsyncErrorEventArgs e)
        {
            _commandIsRunning = false;
            CurrentStatus = GitStatusMonitorState.Stopped;
        }

        private void UpdatedStatusReceived(string updatedStatus)
        {
            _commandIsRunning = false;

            if (CurrentStatus != GitStatusMonitorState.Running)
                return;

            if (_statusIsUpToDate)
            {
                var allChangedFiles = GitCommandHelpers.GetAllChangedFilesFromString(Module, updatedStatus);
                OnGitWorkingDirectoryStatusChanged(new GitWorkingDirectoryStatusEventArgs(allChangedFiles));
            }
            else
            {
                UpdateImmediately();
            }
        }

        private void ScheduleNextJustInCaseUpdate()
        {
            _nextUpdateTime = Environment.TickCount + MaxUpdatePeriod;
        }

        private void ScheduleDeferredUpdate()
        {
            _nextUpdateTime = Environment.TickCount + UpdateDelay;
        }

        private void ScheduleImmediateUpdate()
        {
            _nextUpdateTime = Environment.TickCount;
        }

        private void UpdateImmediately()
        {
            ScheduleImmediateUpdate();
            Update();
        }

        private void commandsSource_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
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

        private void commandsSource_activate(IGitUICommandsSource sender)
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

        private void GitDirChanged(object sender, FileSystemEventArgs e)
        {
            // git directory changed
            if (e.FullPath.Length == _gitPath.Length)
                return;

            if (e.FullPath.EndsWith("\\index.lock"))
                return;

            // submodules directory's subdir changed
            // cut/paste/rename/delete operations are not expected on directories inside nested .git dirs
            if (e.FullPath.StartsWith(_submodulesPath) && (Directory.Exists(e.FullPath)))
                return;

            ScheduleDeferredUpdate();
        }

        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = GitStatusMonitorState.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIPostActionEventArgs e)
        {
            CurrentStatus = GitStatusMonitorState.Running;
        }

        private void GitUICommands_PostEditGitIgnore(object sender, GitUIBaseEventArgs e)
        {
            UpdateIgnoredFiles(true);
        }

        private void ignoredFilesTimer_Tick(object sender, EventArgs e)
        {
            UpdateIgnoredFiles(false);
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            Update();
        }

        // Called for instance at buffer overflow
        private void WorkTreeWatcherError(object sender, ErrorEventArgs e)
        {
            ScheduleDeferredUpdate();
        }

        private void WorkTreeChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = e.FullPath.Substring(_workTreeWatcher.Path.Length).ToPosixPath();
            if (_ignoredFiles.Contains(fileName))
                return;

            if (e.FullPath.StartsWith(_gitPath))
            {
                GitDirChanged(sender, e);
                return;
            }

            // new submodule .git file
            if (e.FullPath.EndsWith("\\.git"))
                return;

            // old submodule .git\index.lock file
            if (e.FullPath.EndsWith("\\.git\\index.lock"))
                return;

            ScheduleDeferredUpdate();
        }
    }
}