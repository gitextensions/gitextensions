using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
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
        private const int UpdateDelay = 1000;

        /// <summary>
        /// Update every 5min, just to make sure something didn't slip through the cracks.
        /// </summary>
        private const int MaxUpdatePeriod = 5 * 60 * 1000;

        private GitCommandsInstance gitGetUnstagedCommand = new GitCommandsInstance();
        private readonly SynchronizationContext syncContext;
        private readonly FileSystemWatcher workTreeWatcher = new FileSystemWatcher();
        private readonly FileSystemWatcher gitDirWatcher = new FileSystemWatcher();
        private string gitPath, submodulesPath;
        private int nextUpdateTime;
        private WorkingStatus currentStatus;
        private bool hasDeferredUpdateRequests;

        public string CommitTranslatedString { get; set; }

        public ToolStripGitStatus()
        {
            syncContext = SynchronizationContext.Current;
            gitGetUnstagedCommand.Exited += (o, ea) => syncContext.Post(_ => UpdatedStatusReceived(gitGetUnstagedCommand.Output.ToString()), null);

            InitializeComponent();
            components.Add(workTreeWatcher);
            components.Add(gitDirWatcher);
            CommitTranslatedString = "Commit";

            Settings.WorkingDirChanged += (_, newDir, newGitDir) => TryStartWatchingChanges(newDir, newGitDir);

            GitUICommands.Instance.PreCheckoutBranch += GitUICommands_PreCheckout;
            GitUICommands.Instance.PreCheckoutRevision += GitUICommands_PreCheckout;
            GitUICommands.Instance.PostCheckoutBranch += GitUICommands_PostCheckout;
            GitUICommands.Instance.PostCheckoutRevision += GitUICommands_PostCheckout;

            // Setup a file watcher to detect changes to our files. When they
            // change, we'll update our status.
            workTreeWatcher.Changed += WorkTreeChanged;
            workTreeWatcher.Created += WorkTreeChanged;
            workTreeWatcher.Deleted += WorkTreeChanged;
            workTreeWatcher.Renamed += WorkTreeChanged;
            workTreeWatcher.Error += WorkTreeWatcherError;
            workTreeWatcher.IncludeSubdirectories = true;
            workTreeWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

            // Setup a file watcher to detect changes to the .git repo files. When they
            // change, we'll update our status.
            gitDirWatcher.Changed += GitDirChanged;
            gitDirWatcher.Created += GitDirChanged;
            gitDirWatcher.Deleted += GitDirChanged;
            gitDirWatcher.Error += WorkTreeWatcherError;
            gitDirWatcher.IncludeSubdirectories = true;
            gitDirWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            TryStartWatchingChanges(Settings.WorkingDir, Settings.Module.GetGitDirectory());
        }


        private void GitUICommands_PreCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Paused;
        }

        private void GitUICommands_PostCheckout(object sender, GitUIBaseEventArgs e)
        {
            CurrentStatus = WorkingStatus.Started;
        }

        private void TryStartWatchingChanges(string workTreePath, string gitDirPath)
        {
            // reset status info, it was outdated
            Text = string.Empty;
            Image = null;

            try
            {
                if (!string.IsNullOrEmpty(workTreePath) && Directory.Exists(workTreePath) &&
                    !string.IsNullOrEmpty(gitDirPath) && Directory.Exists(gitDirPath))
                {
                    workTreeWatcher.Path = workTreePath;
                    gitDirWatcher.Path = gitDirPath;
                    gitPath = Path.GetDirectoryName(gitDirPath);
                    submodulesPath = Path.Combine(gitPath, "modules");
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
            ScheduleNextRegularUpdate();
        }

        private void WorkTreeChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.StartsWith(gitPath))
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

            ScheduleNextRegularUpdate();
        }

        private void GitDirChanged(object sender, FileSystemEventArgs e)
        {
            // git directory changed
            if (e.FullPath.Length == gitPath.Length)
                return;

            if (e.FullPath.EndsWith("\\index.lock"))
                return;

            // submodules directory's subdir changed
            // cut/paste/rename/delete operations are not expected on directories inside nested .git dirs
            if (e.FullPath.StartsWith(submodulesPath) && (Directory.Exists(e.FullPath)))
                return;

            ScheduleNextRegularUpdate();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            // If the previous status call hasn't exited yet, we'll wait until it is
            // so we don't queue up a bunch of commands
            if (gitGetUnstagedCommand.IsRunning)
            {
                hasDeferredUpdateRequests = true; // defer this update request
                return;
            }

            hasDeferredUpdateRequests = false;

            if (Environment.TickCount > nextUpdateTime)
            {
                try
                {
                    string command = GitCommandHelpers.GetAllChangedFilesCmd(true, true);
                    gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, command);

                    if (hasDeferredUpdateRequests)
                        // New changes were detected while processing previous changes, schedule deferred update
                        ScheduleDeferredUpdate();
                    else
                        // Always update every 5 min, even if we don't know anything changed
                        ScheduleNextJustInCaseUpdate();
                }
                catch (Exception)
                {
                    CurrentStatus = WorkingStatus.Stopped;
                }
            }
        }

        private void UpdatedStatusReceived(string updatedStatus)
        {
            if (CurrentStatus != WorkingStatus.Started)
                return;

            var allChangedFiles = GitCommandHelpers.GetAllChangedFilesFromString(updatedStatus);
            var stagedCount = allChangedFiles.Count(status => status.IsStaged);
            var unstagedCount = allChangedFiles.Count - stagedCount;
            var unstagedSubmodulesCount = allChangedFiles.Count(status => status.IsSubmodule && !status.IsStaged);

            Image = GetStatusIcon(stagedCount, unstagedCount, unstagedSubmodulesCount);

            if (allChangedFiles.Count == 0)
                Text = CommitTranslatedString;
            else
                Text = string.Format(CommitTranslatedString + " ({0})", allChangedFiles.Count.ToString());
        }

        private static Image GetStatusIcon(int stagedCount, int unstagedCount, int unstagedSubmodulesCount)
        {
            if (stagedCount == 0 && unstagedCount == 0)
                return IconClean;

            if (stagedCount == 0)
                return unstagedCount != unstagedSubmodulesCount ? IconDirty : IconDirtySubmodules;

            return unstagedCount == 0 ? IconStaged : IconMixed;
        }

        private void ScheduleNextRegularUpdate()
        {
            nextUpdateTime = Math.Min(nextUpdateTime, Environment.TickCount + UpdateDelay);
            hasDeferredUpdateRequests = true;
        }

        private void ScheduleNextJustInCaseUpdate()
        {
            nextUpdateTime = Environment.TickCount + MaxUpdatePeriod;
        }

        private void ScheduleDeferredUpdate()
        {
            nextUpdateTime = Environment.TickCount + UpdateDelay;
        }

        private void ScheduleImmediateUpdate()
        {
            nextUpdateTime = Environment.TickCount;
            Update();
        }

        private WorkingStatus CurrentStatus
        {
            get { return currentStatus; }
            set
            {
                currentStatus = value;
                switch (currentStatus)
                {
                    case WorkingStatus.Stopped:
                        timerRefresh.Stop();
                        workTreeWatcher.EnableRaisingEvents = false;
                        gitDirWatcher.EnableRaisingEvents = false;
                        Visible = false;
                        return;
                    case WorkingStatus.Paused:
                        timerRefresh.Stop();
                        workTreeWatcher.EnableRaisingEvents = false;
                        gitDirWatcher.EnableRaisingEvents = false;
                        return;
                    case WorkingStatus.Started:
                        timerRefresh.Start();
                        workTreeWatcher.EnableRaisingEvents = true;
                        gitDirWatcher.EnableRaisingEvents = !gitDirWatcher.Path.StartsWith(workTreeWatcher.Path);
                        ScheduleImmediateUpdate();
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
    }
}
