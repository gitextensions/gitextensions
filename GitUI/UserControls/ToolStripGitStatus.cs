using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public sealed partial class ToolStripGitStatus : ToolStripMenuItem
    {
        private static readonly Bitmap IconClean = Properties.Resources.IconClean;
        private static readonly Bitmap IconDirty = Properties.Resources.IconDirty;
        private static readonly Bitmap IconDirtySubmodules = Properties.Resources.IconDirtySubmodules;
        private static readonly Bitmap IconStaged = Properties.Resources.IconStaged;
        private static readonly Bitmap IconMixed = Properties.Resources.IconMixed;

        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 500;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private bool _statusIsUpToDate = true;
        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private string _gitPath;
        private string _submodulesPath;
        private int _nextUpdateTime;
        private WorkingStatus _currentStatus;
        private HashSet<string> _ignoredFiles = new HashSet<string>(); 

        public string CommitTranslatedString { get; set; }

        private IGitUICommandsSource _UICommandsSource;
        public IGitUICommandsSource UICommandsSource
        {
            get
            {
                return _UICommandsSource;
            }

            set
            {
                _UICommandsSource = value;
                _UICommandsSource.GitUICommandsChanged += GitUICommandsChanged;
                GitUICommandsChanged(UICommandsSource, null);
            }
        }
        
        public GitUICommands UICommands 
        {
            get
            {
                return UICommandsSource.UICommands;
            }
        }

        public GitModule Module 
        {
            get
            {
                return UICommands.Module;
            }
        }

        public ToolStripGitStatus()
        {
            InitializeComponent();
            components.Add(_workTreeWatcher);
            components.Add(_gitDirWatcher);
            CommitTranslatedString = "Commit";
            ignoredFilesTimer.Interval = MaxUpdatePeriod;
            CurrentStatus = WorkingStatus.Stopped;

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
        }

        private void GitUICommandsChanged(IGitUICommandsSource source, GitUICommands oldCommands)
        {
            if (oldCommands != null)
            {
                oldCommands.PreCheckoutBranch -= GitUICommands_PreCheckout;
                oldCommands.PreCheckoutRevision -= GitUICommands_PreCheckout;
                oldCommands.PostCheckoutBranch -= GitUICommands_PostCheckout;
                oldCommands.PostCheckoutRevision -= GitUICommands_PostCheckout;
                oldCommands.PostEditGitIgnore -= GitUICommands_PostEditGitIgnore;
            }

            if (UICommands != null)
            {
                UICommands.PreCheckoutBranch += GitUICommands_PreCheckout;
                UICommands.PreCheckoutRevision += GitUICommands_PreCheckout;
                UICommands.PostCheckoutBranch += GitUICommands_PostCheckout;
                UICommands.PostCheckoutRevision += GitUICommands_PostCheckout;
                UICommands.PostEditGitIgnore += GitUICommands_PostEditGitIgnore;
                
                TryStartWatchingChanges(Module.WorkingDir, Module.GetGitDirectory());
            }
        }

        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIPostActionEventArgs e)
        {
            CurrentStatus = WorkingStatus.Started;
        }

        private void GitUICommands_PostEditGitIgnore(object sender, GitUIBaseEventArgs e)
        {
            UpdateIgnoredFiles(true);
        }
        
        private void TryStartWatchingChanges(string workTreePath, string gitDirPath)
        {
            // reset status info, it was outdated
            Text = CommitTranslatedString;
            Image = IconClean;

            try
            {
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath))
                {
                    _workTreeWatcher.Path = workTreePath;
                    _gitDirWatcher.Path = gitDirPath;
                    _gitPath = Path.GetDirectoryName(gitDirPath);
                    _submodulesPath = Path.Combine(_gitPath, "modules");
                    UpdateIgnoredFiles(true);
                    ignoredFilesTimer.Stop();
                    ignoredFilesTimer.Start();
                    CurrentStatus = WorkingStatus.Started;
                }
                else
                {
                    CurrentStatus = WorkingStatus.Stopped;
                }
            }
            catch { }
        }

        // destructor shouldn't be used because it's not predictable when
        // it's going to be called by the GC!
        private void WorkTreeWatcherError(object sender, ErrorEventArgs e)
        {
            ScheduleDeferredUpdate();
        }

        private void WorkTreeChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = GitCommandHelpers.FixPath(e.FullPath.Substring(_workTreeWatcher.Path.Length));
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

        private HashSet<string> LoadIgnoredFiles()
        { 
            string lsOutput = Module.RunGitCmd("ls-files -o -i --exclude-standard");
            string[] tab = lsOutput.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(tab);
        }

        private void UpdateIgnoredFiles(bool clearImmediately)
        {
            if (clearImmediately)
                _ignoredFiles = new HashSet<string>();

            AsyncLoader.DoAsync(
                LoadIgnoredFiles, 
                (ignoredSet) => { _ignoredFiles = ignoredSet; },
                (e) => { _ignoredFiles = new HashSet<string>(); }
                );   
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            if (Environment.TickCount >= _nextUpdateTime || 
                (Environment.TickCount < 0 && _nextUpdateTime > 0))
            {
                // If the previous status call hasn't exited yet, we'll wait until it is
                // so we don't queue up a bunch of commands
                if (UICommands.RepoChangedNotifier.IsLocked || Module.IsRunningGitProcess())
                {
                    _statusIsUpToDate = false;//tell that computed status isn't up to date
                    return;
                }

                _statusIsUpToDate = true;
                AsyncLoader.DoAsync(RunStatusCommand, UpdatedStatusReceived, (e) => { CurrentStatus = WorkingStatus.Stopped; });
                // Always update every 5 min, even if we don't know anything changed
                ScheduleNextJustInCaseUpdate();
            }
        }

        private String RunStatusCommand()
        {
            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, true);
            return Module.RunGitCmd(command);
        }

        private void UpdatedStatusReceived(string updatedStatus)
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            if (_statusIsUpToDate)
            {
                var allChangedFiles = GitCommandHelpers.GetAllChangedFilesFromString(Module, updatedStatus);
                var stagedCount = allChangedFiles.Count(status => status.IsStaged);
                var unstagedCount = allChangedFiles.Count - stagedCount;
                var unstagedSubmodulesCount = allChangedFiles.Count(status => status.IsSubmodule && !status.IsStaged);

                Image = GetStatusIcon(stagedCount, unstagedCount, unstagedSubmodulesCount);

                if (allChangedFiles.Count == 0)
                    Text = CommitTranslatedString;
                else
                    Text = string.Format(CommitTranslatedString + " ({0})", allChangedFiles.Count.ToString());
            }
            else
                UpdateImmediately();
        }

        private static Image GetStatusIcon(int stagedCount, int unstagedCount, int unstagedSubmodulesCount)
        {
            if (stagedCount == 0 && unstagedCount == 0)
                return IconClean;

            if (stagedCount == 0)
                return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;

            return unstagedCount == 0 ? IconStaged : IconMixed;
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
        
        private WorkingStatus CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                switch (_currentStatus)
                {
                    case WorkingStatus.Stopped:
                        timerRefresh.Stop();
                        _workTreeWatcher.EnableRaisingEvents = false;
                        _gitDirWatcher.EnableRaisingEvents = false;
                        Visible = false;
                        return;
                    case WorkingStatus.Paused:
                        timerRefresh.Stop();
                        _workTreeWatcher.EnableRaisingEvents = false;
                        _gitDirWatcher.EnableRaisingEvents = false;
                        return;
                    case WorkingStatus.Started:
                        timerRefresh.Start();
                        _workTreeWatcher.EnableRaisingEvents = true;
                        _gitDirWatcher.EnableRaisingEvents = !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
                        ScheduleDeferredUpdate();
                        Visible = true;
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private enum WorkingStatus
        {
            Stopped,
            Paused,
            Started
        }

        private void ignoredFilesTimer_Tick(object sender, EventArgs e)
        {
            UpdateIgnoredFiles(false);
        }
    }
}
