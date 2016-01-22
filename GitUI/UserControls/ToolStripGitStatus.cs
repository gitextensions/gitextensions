using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI
{
    public sealed partial class ToolStripGitStatus : ToolStripMenuItem
    {
        private static readonly Bitmap IconClean = Properties.Resources.IconClean;
        private static readonly Bitmap IconDirty = Properties.Resources.IconDirty;
        private static readonly Bitmap IconDirtySubmodules = Properties.Resources.IconDirtySubmodules;
        private static readonly Bitmap IconStaged = Properties.Resources.IconStaged;
        private static readonly Bitmap IconMixed = Properties.Resources.IconMixed;
        private static readonly object SyncFetchingRepoStatus = new object();

        /// <summary>
        /// We often change several files at once.
        /// Wait a second so they're all changed before we try to get the status.
        /// </summary>
        private const int UpdateDelay = 500;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private readonly FileSystemWatcher _workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher _gitDirWatcher = new FileSystemWatcher();
        private string _gitPath;
        private string _submodulesPath;
        private WorkingStatus _currentStatus;
        private HashSet<string> _ignoredFiles = new HashSet<string>();
        private long skipInterval;

        public string CommitTranslatedString { get; set; }

        private IGitUICommandsSource _UICommandsSource;
        private readonly IObservable<List<GitItemStatus>> _repoStatusObservable;

        [CanBeNull]
        private IDisposable _repoStatusSubscription;

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
                GitUICommandsChanged(UICommandsSource, new GitUICommandsChangedEventArgs(oldCommands: null));
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
            _workTreeWatcher.IncludeSubdirectories = true;
            _workTreeWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            // Setup a file watcher to detect changes to the .git repo files. When they
            // change, we'll update our status.
            _gitDirWatcher.EnableRaisingEvents = false;
            _gitDirWatcher.Changed += GitDirChanged;
            _gitDirWatcher.Created += GitDirChanged;
            _gitDirWatcher.Deleted += GitDirChanged;
            _gitDirWatcher.IncludeSubdirectories = true;
            _gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            _repoStatusObservable = Observable.Interval(TimeSpan.FromMilliseconds(UpdateDelay))
                .Where(i => CurrentStatus == WorkingStatus.Started)
                .SkipWhile(i =>
                {
                    lock (SyncFetchingRepoStatus)
                    {
                        if (skipInterval > 0)
                        {
                            skipInterval -= 1;
                        }

                        return skipInterval > 0;
                    }
                })
                .Where(i => !UICommands.RepoChangedNotifier.IsLocked &&
                            !GitCommandHelpers.VersionInUse.RaceConditionWhenGitStatusIsUpdatingIndex ||
                            !Module.IsRunningGitProcess())
                .Select(i =>
                {
                    var command = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default);
                    var result = Module.RunGitCmd(command);

                    return GitCommandHelpers.GetAllChangedFilesFromString(Module, result);
                })
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(SynchronizationContext.Current);
        }

        private void GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
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

            Interlocked.Increment(ref skipInterval);
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

            Interlocked.Increment(ref skipInterval);
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

        private void UpdateUI(IEnumerable<GitItemStatus> allChangedFiles)
        {
            var itemsStatusArray = allChangedFiles.ToArray();

            var stagedCount = itemsStatusArray.Count(status => status.IsStaged);
            var unstagedCount = itemsStatusArray.Length - stagedCount;
            var unstagedSubmodulesCount = itemsStatusArray.Count(status => status.IsSubmodule && !status.IsStaged);

            Image = GetStatusIcon(stagedCount, unstagedCount, unstagedSubmodulesCount);
            Text = itemsStatusArray.Length == 0 ? CommitTranslatedString : string.Format(CommitTranslatedString + " ({0})", itemsStatusArray.Length);
        }

        private static Image GetStatusIcon(int stagedCount, int unstagedCount, int unstagedSubmodulesCount)
        {
            if (stagedCount == 0 && unstagedCount == 0)
                return IconClean;

            if (stagedCount == 0)
                return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;

            return unstagedCount == 0 ? IconStaged : IconMixed;
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
                        _workTreeWatcher.EnableRaisingEvents = false;
                        _gitDirWatcher.EnableRaisingEvents = false;
                        Visible = false;
                        break;

                    case WorkingStatus.Paused:
                        _workTreeWatcher.EnableRaisingEvents = false;
                        _gitDirWatcher.EnableRaisingEvents = false;
                        break;

                    case WorkingStatus.Started:
                        _workTreeWatcher.EnableRaisingEvents = true;
                        _gitDirWatcher.EnableRaisingEvents = !_gitDirWatcher.Path.StartsWith(_workTreeWatcher.Path);
                        Visible = true;

                        if (_repoStatusSubscription == null)
                        {
                            _repoStatusSubscription = _repoStatusObservable
                                .Subscribe(UpdateUI, e =>
                                {
                                    CurrentStatus = WorkingStatus.Stopped;
                                },
                                    () =>
                                    {
                                        CurrentStatus = WorkingStatus.Stopped;
                                    });
                        }
                        break;

                    default: throw new NotImplementedException("Others status aren't implememted yet.");
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
